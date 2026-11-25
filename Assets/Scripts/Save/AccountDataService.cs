using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
#if FIREBASE_ENABLED
using Firebase;
using Firebase.Auth;
using Firebase.Functions;
#endif

namespace MatchaBakery.Backend
{
    public static class AccountDataService
    {
        /// Hapus akun + data cloud (lewat server), lalu data lokal. Butuh internet.
        /// Mengembalikan pesan kegagalan, atau null bila sukses.
        public static async Task<string> DeleteAccountAsync(string playerId)
        {
            if (CloudFunctionsClient.ForceOffline || Application.internetReachability == NetworkReachability.NotReachable)
                return "Hapus akun butuh internet.";
#if FIREBASE_ENABLED
            try
            {
                string uid = CloudFunctionsClient.CurrentUid;
                if (uid == null) return "Belum login ke Firebase.";
                var fn = FirebaseFunctions.GetInstance(FirebaseApp.DefaultInstance, CloudFunctionsClient.Region).GetHttpsCallable("deleteAccount");
                var call = fn.CallAsync();
                if (await Task.WhenAny(call, Task.Delay(20000)) != call) return "Server lambat merespons. Coba lagi.";
                await call;

                if (SaveManager.Instance != null) SaveManager.Instance.SavingEnabled = false;       // cegah file lokal dibuat ulang
                if (FirebaseBootstrap.Instance != null) FirebaseBootstrap.Instance.Paused = true;   // cegah login anonim baru otomatis
                FirebaseAuth.DefaultInstance.SignOut();
                DeleteLocalData(playerId, uid);
                return null;
            }
            catch (Exception e) { return "Gagal menghapus akun: " + e.Message; }
#else
            await Task.Yield();
            return "Firebase SDK belum aktif.";
#endif
        }

        /// Hapus file lokal saja (boleh dipanggil offline).
        public static void DeleteLocalData(string playerId, string uid)
        {
            string dir = Application.persistentDataPath;
            foreach (var f in Directory.GetFiles(dir, "save_" + playerId + "*")) { try { File.Delete(f); } catch { } }
            if (!string.IsNullOrEmpty(uid))
                foreach (var f in Directory.GetFiles(dir, Path.GetFileName(ActionQueue.PathFor(uid)) + "*")) { try { File.Delete(f); } catch { } }
            PlayerPrefs.DeleteKey("ServerClockOffsetMs");
        }
    }
}