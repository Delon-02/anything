using System;
using System.Threading.Tasks;
using UnityEngine;
#if FIREBASE_ENABLED
using Firebase;
using Firebase.Auth;
#endif

namespace MatchaBakery.Backend
{
    /// Menyalakan Firebase, login anonim, lalu memberi tahu SaveManager & ServerActionService.
    public class FirebaseBootstrap : MonoBehaviour
    {
        public static FirebaseBootstrap Instance { get; private set; }
        public bool Ready { get; private set; }
        public bool Paused { get; set; }
        public string LastError { get; private set; }

        float _retryTimer;
        bool _busy;

        void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        /// Minta bootstrap mencoba login/sinkron sekarang juga (tidak menunggu 30 detik).
        public void Kick() { _retryTimer = 30f; }
        void Start() { _ = StartupAsync(); }

        void Update()   // bila belum beres (offline saat start dll), coba lagi tiap 30 detik
        {
            if (Paused) return;
            _retryTimer += Time.unscaledDeltaTime;
            if (_retryTimer < 30f) return;
            _retryTimer = 0f;
            bool cloudNotChecked = SaveManager.Instance != null && !SaveManager.Instance.CloudChecked;
            if (!_busy && (!Ready || CloudFunctionsClient.CurrentUid == null || cloudNotChecked)) _ = StartupAsync();
        }

        async Task StartupAsync()
        {
            if (_busy || Paused) return;
            _busy = true;
            await Task.Yield();
            try
            {
#if FIREBASE_ENABLED
                if (!Ready)
                {
                    var status = await FirebaseApp.CheckAndFixDependenciesAsync();
                    if (status != DependencyStatus.Available) { LastError = "Firebase belum siap: " + status; return; }
                    ServerClock.AttachToFirebase();
                    Ready = true;
                }
                var auth = FirebaseAuth.DefaultInstance;
                if (auth.CurrentUser == null)
                {
                    if (CloudFunctionsClient.ForceOffline || Application.internetReachability == NetworkReachability.NotReachable) return;
                    await auth.SignInAnonymouslyAsync();
                }
                string uid = auth.CurrentUser.UserId;
                if (ServerActionService.Instance != null) ServerActionService.Instance.OnUserChanged(uid, true);
                if (SaveManager.Instance != null && !SaveManager.Instance.CloudChecked)
                    await SaveManager.Instance.SyncWithCloudAsync();
                LastError = null;
#endif
            }
            catch (Exception e) { LastError = e.Message; Debug.LogWarning("[FirebaseBootstrap] " + e.Message); }
            finally { _busy = false; }
        }
    }
}