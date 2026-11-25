using System;
using UnityEngine;
#if FIREBASE_ENABLED
using Firebase.Database;
#endif

namespace MatchaBakery.Backend
{
    /// Jam yang dikoreksi dengan jam server Firebase, supaya aturan "waktu terbaru menang" tidak tertipu jam perangkat.
    public static class ServerClock
    {
        const string PrefKey = "ServerClockOffsetMs";
        static bool _loaded;
        static bool _attached;
        static long _offsetMs;

        public static long OffsetMs { get { EnsureLoaded(); return _offsetMs; } }
        public static long NowUtcMs { get { return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + OffsetMs; } }

        static void EnsureLoaded()
        {
            if (_loaded) return;
            _loaded = true;
            long saved;
            if (long.TryParse(PlayerPrefs.GetString(PrefKey, "0"), out saved)) _offsetMs = saved;
        }

        /// Dipanggil FirebaseBootstrap setelah Firebase siap.
        public static void AttachToFirebase()
        {
            EnsureLoaded();
#if FIREBASE_ENABLED
            if (_attached) return;
            _attached = true;
            FirebaseDatabase.DefaultInstance.GetReference(".info/serverTimeOffset").ValueChanged += OnOffsetChanged;
#endif
        }

#if FIREBASE_ENABLED
        static void OnOffsetChanged(object sender, ValueChangedEventArgs e)
        {
            if (e.DatabaseError != null || e.Snapshot == null || !e.Snapshot.Exists) return;
            _offsetMs = Convert.ToInt64(e.Snapshot.Value);
            PlayerPrefs.SetString(PrefKey, _offsetMs.ToString());
        }
#endif
    }
}