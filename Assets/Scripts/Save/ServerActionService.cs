using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;

namespace MatchaBakery.Backend
{
    public class ServerActionService : MonoBehaviour
    {
        public enum SyncState { Idle, Offline, Sending, Backoff, NeedsLogin, Unavailable }

        public static ServerActionService Instance { get; private set; }

        public SyncState State { get; private set; } = SyncState.Idle;
        public ServerSnapshot LastSnapshot { get; private set; }
        public int PendingCount { get { return _queue != null ? _queue.Count : 0; } }

        public event Action<SyncState> StateChanged;
        public event Action<ServerSnapshot> SnapshotUpdated;
        public event Action<string, string, string> ActionRejected;   // actionId, kode, pesan
        public event Action<string> Notice;                           // pesan ramah untuk UI

        [SerializeField] float pollIntervalSeconds = 30f;

        ActionQueue _queue;
        bool _busy;
        float _timer;

        void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            OnUserChanged(null, false);   // mulai dengan antrean "device"; FirebaseBootstrap memberi tahu saat login
        }

        void Update()
        {
            _timer += Time.unscaledDeltaTime;
            if (_timer >= pollIntervalSeconds) { _timer = 0f; _ = FlushAsync(); }
        }

        void OnApplicationFocus(bool focus) { if (focus) ScheduleSoon(); }

        /// Dipanggil FirebaseBootstrap setelah login. adoptDeviceQueue = true: aksi yang terkumpul sebelum login jadi milik akun ini.
        public void OnUserChanged(string uid, bool adoptDeviceQueue)
        {
            string owner = string.IsNullOrEmpty(uid) ? ActionQueue.DeviceOwner : uid;
            if (_queue != null && _queue.OwnerUid == owner) return;

            var next = new ActionQueue(owner);
            if (adoptDeviceQueue && owner != ActionQueue.DeviceOwner)
                new ActionQueue(ActionQueue.DeviceOwner).MoveAllTo(next);
            _queue = next;
            LastSnapshot = null;
            ScheduleSoon();
        }

        // ---------- API untuk gameplay ----------

        /// Klaim pesanan selesai. Aman offline: masuk antrean, dikirim saat online.
        public bool EnqueueCompleteOrder(string orderId, List<OrderLine> lines)
        {
            if (string.IsNullOrEmpty(orderId) || !Regex.IsMatch(orderId, "^[A-Za-z0-9_-]{5,61}$"))
            { if (Notice != null) Notice("orderId tidak valid (huruf/angka/_/- saja, 5-61 karakter)."); return false; }

            var p = new CompleteOrderPayload { orderId = orderId, completedAtMs = ServerClock.NowUtcMs, lines = lines };
            string id = _queue != null ? _queue.Enqueue(ActionTypes.CompleteOrder, JsonUtility.ToJson(p), "co_" + orderId) : null;
            if (id == null) { if (Notice != null) Notice("Klaim pesanan gagal disimpan."); return false; }
            ScheduleSoon();
            return true;
        }

        /// Aksi yang butuh server segar (belanja, hadiah harian). Null bila offline/gagal (pesan ada di Notice).
        public async Task<ActionResultDto> ExecuteOnlineAsync(string type, string payloadJson)
        {
            var action = new PendingAction { actionId = Guid.NewGuid().ToString("N"), type = type, payloadJson = payloadJson };
            var outcome = await CloudFunctionsClient.SubmitBatchAsync(new List<PendingAction> { action });

            switch (outcome.Status)
            {
                case CallStatus.Success:
                    ApplySnapshot(outcome.Response.state);
                    return outcome.Response.results.Count > 0 ? outcome.Response.results[0] : null;
                case CallStatus.Offline:
                    if (Notice != null) Notice("Fitur ini butuh internet. Coba lagi saat online ya.");
                    return null;
                case CallStatus.Unavailable:
                    if (Notice != null) Notice("Layanan online belum aktif.");
                    return null;
                case CallStatus.AuthRequired:
                    SetState(SyncState.NeedsLogin);
                    if (Notice != null) Notice("Belum login / sesi berakhir.");
                    return null;
                default:
                    // Hasil tidak pasti: simpan ke antrean dengan id yang SAMA supaya kirim ulang tidak terhitung dua kali.
                    if (_queue != null) _queue.Enqueue(action.type, action.payloadJson, action.actionId);
                    if (Notice != null) Notice("Server sedang sibuk. Akan dicoba lagi otomatis.");
                    ScheduleSoon();
                    return null;
            }
        }

        public Task<ActionResultDto> BuyIngredientAsync(string ingredientId, int packs)
        { return ExecuteOnlineAsync(ActionTypes.BuyIngredient, JsonUtility.ToJson(new BuyIngredientPayload { ingredientId = ingredientId, packs = packs })); }

        public Task<ActionResultDto> ClaimDailyAsync()
        { return ExecuteOnlineAsync(ActionTypes.ClaimDaily, JsonUtility.ToJson(new EmptyPayload())); }

        // ---------- pengiriman antrean ----------
        public void ScheduleSoon() { _timer = Mathf.Max(_timer, pollIntervalSeconds - 3f); }   // kirim dalam ~3 detik

        public async Task FlushAsync()
        {
            if (_busy || _queue == null) return;
            var q = _queue;
            if (q.Count == 0) { if (State != SyncState.Idle && State != SyncState.NeedsLogin) SetState(SyncState.Idle); return; }
            if (q.OwnerUid != CloudFunctionsClient.CurrentUid) return;   // antrean akun lain / belum login: jangan kirim

            long now = ServerClock.NowUtcMs;
            var batch = q.Due(now, 25);
            if (batch.Count == 0) return;

            _busy = true;
            try
            {
                SetState(SyncState.Sending);
                var outcome = await CloudFunctionsClient.SubmitBatchAsync(batch);

                switch (outcome.Status)
                {
                    case CallStatus.Success:
                        var done = new HashSet<string>();
                        foreach (var r in outcome.Response.results)
                        {
                            done.Add(r.actionId);   // hasil server final (receipt sudah tersimpan)
                            if (!r.ok && ActionRejected != null) ActionRejected(r.actionId, r.code, r.message);
                        }
                        q.Remove(done);
                        ApplySnapshot(outcome.Response.state);
                        SetState(SyncState.Idle);
                        if (q.Count > 0) ScheduleSoon();
                        break;

                    case CallStatus.Offline:
                        SetState(SyncState.Offline);
                        break;

                    case CallStatus.AuthRequired:
                        SetState(SyncState.NeedsLogin);
                        if (Notice != null) Notice("Sesi login berakhir. Aksi tertunda aman dan terkirim setelah login lagi.");
                        break;

                    case CallStatus.Rejected:
                        var ids = new HashSet<string>();
                        foreach (var a in batch) { ids.Add(a.actionId); if (ActionRejected != null) ActionRejected(a.actionId, "CLIENT_REJECTED", "Aksi ditolak server."); }
                        q.Remove(ids);
                        SetState(SyncState.Idle);
                        break;

                    case CallStatus.Unavailable:
                        SetState(SyncState.Unavailable);
                        break;

                    default:   // Timeout / RetryLater
                        q.RecordFailure(batch, now);
                        SetState(SyncState.Backoff);
                        break;
                }
            }
            catch (Exception e) { Debug.LogWarning("[ServerActionService] " + e.Message); SetState(SyncState.Backoff); }
            finally { _busy = false; }
        }

        /// Ambil dompet server terbaru (tanpa mengirim aksi).
        public async Task RefreshStateAsync()
        {
            var outcome = await CloudFunctionsClient.SubmitBatchAsync(new List<PendingAction>());
            if (outcome.Status == CallStatus.Success) ApplySnapshot(outcome.Response.state);
        }

        void ApplySnapshot(ServerSnapshot s) { LastSnapshot = s; if (SnapshotUpdated != null) SnapshotUpdated(s); }
        void SetState(SyncState s) { if (State == s) return; State = s; if (StateChanged != null) StateChanged(s); }
    }
}