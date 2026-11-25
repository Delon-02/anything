using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace MatchaBakery.Backend
{
    /// Antrean aksi yang disimpan ke disk, satu file per akun (pending_<uid>.json).
    public class ActionQueue
    {
        public const string DeviceOwner = "device";   // dipakai saat belum punya uid
        const int MaxItems = 1000;

        readonly string _path;
        PendingActionFile _file = new PendingActionFile();

        public string OwnerUid { get { return _file.ownerUid; } }
        public int Count { get { return _file.items.Count; } }

        public ActionQueue(string ownerUid)
        {
            _path = PathFor(ownerUid);
            Load();
            _file.ownerUid = ownerUid;
        }

        public static string PathFor(string uid)
        {
            foreach (char c in Path.GetInvalidFileNameChars()) uid = uid.Replace(c, '_');
            return Path.Combine(Application.persistentDataPath, "pending_" + uid + ".json");
        }

        void Load()
        {
            try
            {
                if (!File.Exists(_path)) return;
                var f = JsonUtility.FromJson<PendingActionFile>(File.ReadAllText(_path));
                if (f != null && f.items != null) _file = f;
            }
            catch (Exception e)
            {
                Debug.LogWarning("[ActionQueue] File rusak, dipindah: " + e.Message);
                try { File.Move(_path, _path + ".corrupt"); } catch { }
            }
        }

        void Save()
        {
            try
            {
                string tmp = _path + ".tmp";
                File.WriteAllText(tmp, JsonUtility.ToJson(_file));
                File.Copy(tmp, _path, true);
                File.Delete(tmp);
            }
            catch (Exception e) { Debug.LogWarning("[ActionQueue] Gagal menyimpan: " + e.Message); }
        }

        /// Mengembalikan actionId, atau null bila ditolak. Aman dipanggil dua kali dengan id yang sama.
        public string Enqueue(string type, string payloadJson, string actionId = null)
        {
            if (string.IsNullOrEmpty(actionId)) actionId = Guid.NewGuid().ToString("N");
            if (actionId.Length < 8 || actionId.Length > 64 || payloadJson.Length > 2000) return null;
            if (_file.items.Exists(i => i.actionId == actionId)) return actionId;
            if (_file.items.Count >= MaxItems) { Debug.LogWarning("[ActionQueue] Antrean penuh"); return null; }

            _file.items.Add(new PendingAction
            {
                actionId = actionId, type = type, payloadJson = payloadJson,
                createdAtUtcMs = ServerClock.NowUtcMs, attempts = 0, nextTryUtcMs = 0
            });
            Save();
            return actionId;
        }

        public List<PendingAction> Due(long nowMs, int max)
        {
            var list = new List<PendingAction>();
            foreach (var a in _file.items)
            {
                if (a.nextTryUtcMs <= nowMs) list.Add(a);
                if (list.Count >= max) break;
            }
            return list;
        }

        public void Remove(ICollection<string> ids)
        {
            if (ids.Count == 0) return;
            _file.items.RemoveAll(i => ids.Contains(i.actionId));
            Save();
        }

        public void RecordFailure(IEnumerable<PendingAction> batch, long nowMs)
        {
            foreach (var a in batch)
            {
                a.attempts++;
                double delay = Math.Min(Math.Pow(2, Math.Min(a.attempts, 8)) * 2000.0, 300000.0);
                a.nextTryUtcMs = nowMs + (long)delay + UnityEngine.Random.Range(0, 1000);
            }
            Save();
        }

        public void MoveAllTo(ActionQueue target)
        {
            foreach (var a in _file.items) target.Enqueue(a.type, a.payloadJson, a.actionId);
            _file.items.Clear();
            Save();
        }
    }
}