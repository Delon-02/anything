using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace MatchaBakery.Backend
{
    /// Panel uji sederhana (IMGUI). Hapus komponen ini sebelum rilis.
    public class BackendDebugPanel : MonoBehaviour
    {
        bool _show = true;
        Vector2 _scroll;
        string _log = "";
        string _board = "";
        bool _confirmDelete;

        void Start()
        {
            var sas = ServerActionService.Instance;
            if (sas == null) return;
            sas.Notice += OnNotice;
            sas.ActionRejected += OnRejected;
            sas.StateChanged += OnState;
        }

        void OnDestroy()
        {
            var sas = ServerActionService.Instance;
            if (sas == null) return;
            sas.Notice -= OnNotice;
            sas.ActionRejected -= OnRejected;
            sas.StateChanged -= OnState;
        }

        void OnNotice(string m) { Log("INFO: " + m); }
        void OnRejected(string id, string code, string msg) { Log("DITOLAK server [" + code + "] " + msg); }
        void OnState(ServerActionService.SyncState s) { Log("Status kirim: " + s); }

        void Log(string m)
        {
            _log = DateTime.Now.ToString("HH:mm:ss") + " " + m + "\n" + _log;
            if (_log.Length > 1500) _log = _log.Substring(0, 1500);
        }

        void OnGUI()
        {
            if (GUI.Button(new Rect(10, 10, 120, 28), _show ? "Tutup Debug" : "Buka Debug")) _show = !_show;
            if (!_show) return;

            GUILayout.BeginArea(new Rect(10, 44, 430, Screen.height - 54), GUI.skin.box);
            _scroll = GUILayout.BeginScrollView(_scroll);
            GUILayout.Label(StatusText());

            var sm = SaveManager.Instance;
            var sas = ServerActionService.Instance;
            if (sm == null || sas == null)
            {
                GUILayout.Label("SaveManager / ServerActionService belum ada di scene.");
            }
            else
            {
                if (GUILayout.Button("Paksa OFFLINE: " + (CloudFunctionsClient.ForceOffline ? "ON" : "OFF")))
                    CloudFunctionsClient.ForceOffline = !CloudFunctionsClient.ForceOffline;

                if (GUILayout.Button("Tambah 3 matcha_powder + 50 uang (lokal)"))
                { sm.AddItem("matcha_powder", 3); sm.AddMoney(50); sm.SaveNow(); Log("Item & uang ditambah (lokal)"); }

                if (GUILayout.Button("Selesaikan pesanan: 2x matcha_latte"))
                {
                    var lines = new List<OrderLine> { new OrderLine { id = "matcha_latte", qty = 2 } };
                    bool ok = sas.EnqueueCompleteOrder(Guid.NewGuid().ToString("N"), lines);
                    Log(ok ? "Pesanan masuk antrean" : "Pesanan gagal diantrekan");
                }

                if (GUILayout.Button("Kirim antrean sekarang")) { _ = sas.FlushAsync(); }
                if (GUILayout.Button("Beli 1 pak matcha_powder (online)")) Buy();
                if (GUILayout.Button("Klaim hadiah harian (online)")) Daily();
                if (GUILayout.Button("Sinkronkan save dengan cloud")) { _ = sm.SyncWithCloudAsync(); }
                if (GUILayout.Button("Lihat leaderboard (top 10)")) Board();
                if (!string.IsNullOrEmpty(_board)) GUILayout.Label(_board);

                if (GUILayout.Button("New Game (reset progres)")) { sm.NewGame(); Log("Progres di-reset"); }

                if (!_confirmDelete)
                {
                    if (GUILayout.Button("Hapus akun & semua data...")) _confirmDelete = true;
                }
                else
                {
                    GUILayout.Label("YAKIN? Tidak bisa dibatalkan.");
                    if (GUILayout.Button("YA, hapus sekarang")) { _confirmDelete = false; DeleteAll(); }
                    if (GUILayout.Button("Batal")) _confirmDelete = false;
                }
            }
            GUILayout.Label("Log:\n" + _log);
            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        string StatusText()
        {
            var sm = SaveManager.Instance;
            var sas = ServerActionService.Instance;
            var boot = FirebaseBootstrap.Instance;
            var sb = new StringBuilder();
            sb.AppendLine("Firebase siap : " + (boot != null && boot.Ready));
            sb.AppendLine("UID           : " + (CloudFunctionsClient.CurrentUid ?? "(belum login)"));
            sb.AppendLine("Paksa offline : " + CloudFunctionsClient.ForceOffline);
            if (boot != null && !string.IsNullOrEmpty(boot.LastError)) sb.AppendLine("Error terakhir: " + boot.LastError);
            if (sm != null && sm.Current != null)
            {
                sb.AppendLine("Save lokal    : rev " + sm.Current.revision + ", uang " + sm.Current.money + ", scene " + sm.Current.sceneName);
                sb.AppendLine("Save cloud    : " + sm.CloudStatus);
            }
            if (sas != null)
            {
                sb.AppendLine("Antrean aksi  : " + sas.PendingCount + " (" + sas.State + ")");
                if (sas.LastSnapshot != null)
                    sb.AppendLine("Dompet server : " + sas.LastSnapshot.money + " | total " + sas.LastSnapshot.totalEarned + " | xp " + sas.LastSnapshot.xp);
            }
            return sb.ToString();
        }

        string Describe(string what, ActionResultDto r)
        {
            if (r == null) return what + ": belum berhasil (offline / dicoba ulang otomatis)";
            return what + ": " + (r.ok ? "BERHASIL" : "DITOLAK [" + r.code + "] " + r.message);
        }

        async void Buy()
        {
            try { Log(Describe("Beli", await ServerActionService.Instance.BuyIngredientAsync("matcha_powder", 1))); }
            catch (Exception e) { Log("Error: " + e.Message); }
        }

        async void Daily()
        {
            try { Log(Describe("Hadiah harian", await ServerActionService.Instance.ClaimDailyAsync())); }
            catch (Exception e) { Log("Error: " + e.Message); }
        }

        async void Board()
        {
            try
            {
                var rows = await LeaderboardClient.GetTopAsync(10);
                var sb = new StringBuilder();
                int n = 1;
                foreach (var r in rows) sb.AppendLine((n++) + ". " + r.name + " - " + r.score);
                _board = rows.Count == 0 ? "(kosong / tidak tersedia)" : sb.ToString();
            }
            catch (Exception e) { Log("Error: " + e.Message); }
        }

        async void DeleteAll()
        {
            try
            {
                string err = await AccountDataService.DeleteAccountAsync(SaveManager.Instance.Current.playerId);
                Log(err ?? "Akun dihapus. Tutup game, lalu jalankan lagi.");
            }
            catch (Exception e) { Log("Error: " + e.Message); }
        }
    }
}