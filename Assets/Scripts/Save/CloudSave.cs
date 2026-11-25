using System;
using System.Threading.Tasks;
using UnityEngine;
#if FIREBASE_ENABLED
using Firebase.Database;
#endif

namespace MatchaBakery.Backend
{
    public class PullResult { public bool ok; public SaveData data; }   // ok=false: gagal; ok=true & data=null: cloud kosong

    public static class CloudSave
    {
        public static bool IsAvailable
        {
            get
            {
                return CloudFunctionsClient.CurrentUid != null && !CloudFunctionsClient.ForceOffline
                       && Application.internetReachability != NetworkReachability.NotReachable;
            }
        }

        public static async Task<bool> PushJsonAsync(string json, int timeoutMs = 10000)
        {
#if FIREBASE_ENABLED
            string uid = CloudFunctionsClient.CurrentUid;
            if (uid == null || !IsAvailable) return false;
            try
            {
                var task = FirebaseDatabase.DefaultInstance.GetReference("saves/" + uid).SetRawJsonValueAsync(json);
                if (await Task.WhenAny(task, Task.Delay(timeoutMs)) != task) return false;
                await task;   // melempar exception bila ditolak (mis. rules)
                return true;
            }
            catch (Exception e) { Debug.LogWarning("[CloudSave] Push gagal: " + e.Message); return false; }
#else
            await Task.Yield();
            return false;
#endif
        }

        public static async Task<PullResult> PullAsync(int timeoutMs = 10000)
        {
#if FIREBASE_ENABLED
            string uid = CloudFunctionsClient.CurrentUid;
            if (uid == null || !IsAvailable) return new PullResult { ok = false };
            try
            {
                var task = FirebaseDatabase.DefaultInstance.GetReference("saves/" + uid).GetValueAsync();
                if (await Task.WhenAny(task, Task.Delay(timeoutMs)) != task) return new PullResult { ok = false };
                var snap = await task;
                if (snap == null || !snap.Exists) return new PullResult { ok = true, data = null };
                var data = SaveMigrator.Migrate(JsonUtility.FromJson<SaveData>(snap.GetRawJsonValue()));
                return new PullResult { ok = data != null, data = data };
            }
            catch (Exception e) { Debug.LogWarning("[CloudSave] Pull gagal: " + e.Message); return new PullResult { ok = false }; }
#else
            await Task.Yield();
            return new PullResult { ok = false };
#endif
        }

        /// Aturan konflik: waktu terbaru menang, seri dipecah revision, data revision 0 kalah dari data yang punya progres.
        public static bool CloudWins(SaveData local, SaveData cloud)
        {
            if (cloud == null) return false;
            if (local == null) return true;
            bool localHas = local.revision > 0, cloudHas = cloud.revision > 0;
            if (!localHas && cloudHas) return true;
            if (localHas && !cloudHas) return false;
            if (cloud.updatedAtUtcMs != local.updatedAtUtcMs) return cloud.updatedAtUtcMs > local.updatedAtUtcMs;
            return cloud.revision > local.revision;
        }

        public static bool SameStamp(SaveData a, SaveData b)
        { return a.updatedAtUtcMs == b.updatedAtUtcMs && a.revision == b.revision; }
    }
}