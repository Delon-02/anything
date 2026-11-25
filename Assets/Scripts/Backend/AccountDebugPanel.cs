using System;
using System.Threading.Tasks;
using UnityEngine;

namespace MatchaBakery.Backend
{
    /// Panel uji akun (IMGUI) di sisi kanan layar. Hapus komponen ini sebelum rilis.
    public class AccountDebugPanel : MonoBehaviour
    {
        string _email = "", _password = "", _name = "", _msg = "";
        bool _show = true, _busy;

        void OnGUI()
        {
            float w = 320f;
            float x = Screen.width - w - 10f;
            if (GUI.Button(new Rect(x, 10, 120, 28), _show ? "Tutup Akun" : "Buka Akun")) _show = !_show;
            if (!_show) return;

            GUILayout.BeginArea(new Rect(x, 44, w, 360), GUI.skin.box);
            GUILayout.Label("Akun: " + AccountService.Label);
            GUILayout.Label("Email");
            _email = GUILayout.TextField(_email);
            GUILayout.Label("Password (min 6 karakter)");
            _password = GUILayout.PasswordField(_password, '*');

            GUI.enabled = !_busy;
            if (GUILayout.Button("Buat akun (tautkan tamu)")) Run(() => AccountService.RegisterAndLinkAsync(_email, _password));
            if (GUILayout.Button("Masuk ke akun yang sudah ada")) Run(() => AccountService.SignInAsync(_email, _password));
            if (GUILayout.Button("Reset password (kirim email)")) Run(() => AccountService.SendPasswordResetAsync(_email));
            if (GUILayout.Button("Keluar")) Run(() => AccountService.SignOutAsync());
            GUILayout.Label("Nama tampilan (leaderboard)");
            _name = GUILayout.TextField(_name, 24);
            if (GUILayout.Button("Simpan nama")) Run(() => AccountService.SetDisplayNameAsync(_name));
            GUI.enabled = true;

            GUILayout.Label(_msg);
            GUILayout.EndArea();
        }

        async void Run(Func<Task<AccountOutcome>> action)
        {
            if (_busy) return;
            _busy = true;
            _msg = "Memproses...";
            try
            {
                var r = await action();
                _msg = (r.Ok ? "OK: " : "Gagal: ") + r.Message;
            }
            catch (Exception e) { _msg = "Error: " + e.Message; }
            finally { _busy = false; }
        }
    }
}