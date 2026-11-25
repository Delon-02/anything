using System;
using System.Threading.Tasks;
using UnityEngine;
#if FIREBASE_ENABLED
using Firebase;
using Firebase.Auth;
using Firebase.Database;
#endif

namespace MatchaBakery.Backend
{
    public enum AccountResult { Ok, Offline, Unavailable, InvalidInput, EmailInUse, WrongCredentials, NotAllowed, Timeout, Failed }

    public class AccountOutcome
    {
        public AccountResult Result;
        public string Message;
        public bool Ok { get { return Result == AccountResult.Ok; } }
        public static AccountOutcome Make(AccountResult r, string message) { return new AccountOutcome { Result = r, Message = message }; }
    }

    /// Akun email: buat akun (tautkan tamu), masuk, keluar, reset password, nama tampilan leaderboard.
    public static class AccountService
    {
        const int TimeoutMs = 15000;

        static AccountOutcome Fail(AccountResult r, string m) { return AccountOutcome.Make(r, m); }
        static AccountOutcome Success(string m) { return AccountOutcome.Make(AccountResult.Ok, m); }

        public static string Label
        {
            get
            {
#if FIREBASE_ENABLED
                var u = CurrentUser();
                if (u == null) return "(belum login)";
                return u.IsAnonymous ? "Tamu (belum punya akun)" : (string.IsNullOrEmpty(u.Email) ? "akun" : u.Email);
#else
                return "(Firebase belum aktif)";
#endif
            }
        }

#if FIREBASE_ENABLED
        static FirebaseUser CurrentUser()
        {
            if (FirebaseBootstrap.Instance == null || !FirebaseBootstrap.Instance.Ready) return null;
            return FirebaseAuth.DefaultInstance.CurrentUser;
        }

        static async Task<bool> InTime(Task t) { return await Task.WhenAny(t, Task.Delay(TimeoutMs)) == t; }

        static AccountOutcome Map(Exception e)
        {
            var fe = e.GetBaseException() as FirebaseException;
            string code = fe != null ? ((AuthError)fe.ErrorCode).ToString() : e.GetType().Name;
            switch (code)
            {
                case "InvalidEmail": return Fail(AccountResult.InvalidInput, "Format email tidak valid.");
                case "WeakPassword": return Fail(AccountResult.InvalidInput, "Password terlalu lemah (minimal 6 karakter).");
                case "EmailAlreadyInUse":
                case "CredentialAlreadyInUse": return Fail(AccountResult.EmailInUse, "Email ini sudah terdaftar. Gunakan tombol Masuk.");
                case "WrongPassword":
                case "UserNotFound":
                case "InvalidCredential":
                case "InvalidLoginCredentials": return Fail(AccountResult.WrongCredentials, "Email atau password salah.");
                case "NetworkRequestFailed": return Fail(AccountResult.Offline, "Butuh internet.");
                case "TooManyRequests": return Fail(AccountResult.Failed, "Terlalu banyak percobaan. Tunggu beberapa menit.");
                default: return Fail(AccountResult.Failed, code + ": " + e.Message);
            }
        }
#endif

        static AccountOutcome Precheck()
        {
#if FIREBASE_ENABLED
            if (FirebaseBootstrap.Instance == null || !FirebaseBootstrap.Instance.Ready)
                return Fail(AccountResult.Unavailable, "Firebase belum siap. Tunggu sebentar.");
            if (CloudFunctionsClient.ForceOffline || Application.internetReachability == NetworkReachability.NotReachable)
                return Fail(AccountResult.Offline, "Butuh internet.");
            return null;
#else
            return Fail(AccountResult.Unavailable, "Firebase SDK belum aktif.");
#endif
        }

        static AccountOutcome ValidateEmail(string email)
        {
            email = (email ?? "").Trim();
            if (email.Length < 5 || email.IndexOf('@') < 1 || email.LastIndexOf('.') < email.IndexOf('@'))
                return Fail(AccountResult.InvalidInput, "Format email tidak valid.");
            return null;
        }

        static AccountOutcome ValidateCredentials(string email, string password)
        {
            var bad = ValidateEmail(email);
            if (bad != null) return bad;
            if (string.IsNullOrEmpty(password) || password.Length < 6)
                return Fail(AccountResult.InvalidInput, "Password minimal 6 karakter.");
            return null;
        }

        /// Tamu -> akun email. UID tetap, data ikut terbawa.
        public static async Task<AccountOutcome> RegisterAndLinkAsync(string email, string password)
        {
            var pre = Precheck(); if (pre != null) return pre;
            var bad = ValidateCredentials(email, password); if (bad != null) return bad;
#if FIREBASE_ENABLED
            var user = CurrentUser();
            if (user == null) return Fail(AccountResult.NotAllowed, "Belum login sebagai tamu.");
            if (!user.IsAnonymous) return Fail(AccountResult.NotAllowed, "Kamu sudah punya akun. Keluar dulu untuk membuat akun lain.");
            try
            {
                var cred = EmailAuthProvider.GetCredential(email.Trim(), password);
                var task = user.LinkWithCredentialAsync(cred);
                if (!await InTime(task)) return Fail(AccountResult.Timeout, "Server lambat merespons. Coba lagi.");
                await task;
                if (SaveManager.Instance != null) await SaveManager.Instance.PushNowAsync();   // pastikan progres tamu tersimpan di akun
                return Success("Akun dibuat. Progresmu aman di akun ini.");
            }
            catch (Exception e) { return Map(e); }
#else
            await Task.Yield();
            return Fail(AccountResult.Unavailable, "Firebase SDK belum aktif.");
#endif
        }

        /// Masuk ke akun yang SUDAH ADA. Data cloud akun menang; data tamu hanya dicadangkan.
        public static async Task<AccountOutcome> SignInAsync(string email, string password)
        {
            var pre = Precheck(); if (pre != null) return pre;
            var bad = ValidateCredentials(email, password); if (bad != null) return bad;
#if FIREBASE_ENABLED
            var before = CurrentUser();
            if (before != null && !before.IsAnonymous) return Fail(AccountResult.NotAllowed, "Kamu sudah masuk. Keluar dulu.");
            try
            {
                var task = FirebaseAuth.DefaultInstance.SignInWithEmailAndPasswordAsync(email.Trim(), password);
                if (!await InTime(task)) return Fail(AccountResult.Timeout, "Server lambat merespons. Coba lagi.");
                await task;
            }
            catch (Exception e) { return Map(e); }

            string uid = CloudFunctionsClient.CurrentUid;
            if (ServerActionService.Instance != null) ServerActionService.Instance.OnUserChanged(uid, false);   // false: jangan bawa aksi tamu
            bool synced = SaveManager.Instance != null && await SaveManager.Instance.SwitchToExistingAccountAsync();
            if (ServerActionService.Instance != null) await ServerActionService.Instance.RefreshStateAsync();
            return synced
                ? Success("Berhasil masuk. Progres akun dimuat.")
                : Fail(AccountResult.Failed, "Berhasil masuk, tetapi gagal memuat save cloud. Coba tombol Sinkronkan.");
#else
            await Task.Yield();
            return Fail(AccountResult.Unavailable, "Firebase SDK belum aktif.");
#endif
        }

        /// Keluar dari akun email (tamu tidak bisa keluar). Progres disimpan ke cloud dulu.
        public static async Task<AccountOutcome> SignOutAsync()
        {
            var pre = Precheck(); if (pre != null) return pre;
#if FIREBASE_ENABLED
            var user = CurrentUser();
            if (user == null) return Fail(AccountResult.NotAllowed, "Belum login.");
            if (user.IsAnonymous)
                return Fail(AccountResult.NotAllowed, "Kamu masih tamu. Buat akun dulu supaya progres bisa dipulihkan.");
            try
            {
                if (SaveManager.Instance != null)
                {
                    SaveManager.Instance.SaveNow();
                    await SaveManager.Instance.PushNowAsync();
                    if (SaveManager.Instance.CloudStatus != "Synced")
                        return Fail(AccountResult.Failed, "Gagal menyimpan progres ke cloud. Coba lagi.");
                }
                if (ServerActionService.Instance != null) await ServerActionService.Instance.FlushAsync();
            }
            catch (Exception e) { return Fail(AccountResult.Failed, "Gagal menyimpan sebelum keluar: " + e.Message); }

            FirebaseAuth.DefaultInstance.SignOut();
            if (SaveManager.Instance != null) SaveManager.Instance.ResetForSignOut();
            if (ServerActionService.Instance != null) ServerActionService.Instance.OnUserChanged(null, false);
            if (FirebaseBootstrap.Instance != null) FirebaseBootstrap.Instance.Kick();   // segera login sebagai tamu baru
            return Success("Berhasil keluar. Kamu bermain sebagai tamu baru.");
#else
            await Task.Yield();
            return Fail(AccountResult.Unavailable, "Firebase SDK belum aktif.");
#endif
        }

        public static async Task<AccountOutcome> SendPasswordResetAsync(string email)
        {
            var pre = Precheck(); if (pre != null) return pre;
            var bad = ValidateEmail(email); if (bad != null) return bad;
#if FIREBASE_ENABLED
            try
            {
                var task = FirebaseAuth.DefaultInstance.SendPasswordResetEmailAsync(email.Trim());
                if (!await InTime(task)) return Fail(AccountResult.Timeout, "Server lambat merespons. Coba lagi.");
                await task;
                return Success("Email reset password dikirim (cek juga folder spam).");
            }
            catch (Exception e) { return Map(e); }
#else
            await Task.Yield();
            return Fail(AccountResult.Unavailable, "Firebase SDK belum aktif.");
#endif
        }

        /// Nama yang tampil di leaderboard (1-24 karakter).
        public static async Task<AccountOutcome> SetDisplayNameAsync(string name)
        {
            var pre = Precheck(); if (pre != null) return pre;
            name = (name ?? "").Trim();
            if (name.Length < 1 || name.Length > 24) return Fail(AccountResult.InvalidInput, "Nama harus 1-24 karakter.");
#if FIREBASE_ENABLED
            string uid = CloudFunctionsClient.CurrentUid;
            if (uid == null) return Fail(AccountResult.NotAllowed, "Belum login.");
            try
            {
                var task = FirebaseDatabase.DefaultInstance.GetReference("players/" + uid + "/profile/displayName").SetValueAsync(name);
                if (!await InTime(task)) return Fail(AccountResult.Timeout, "Server lambat merespons. Coba lagi.");
                await task;
                return Success("Nama tersimpan. Leaderboard memakai nama ini setelah pesanan berikutnya selesai.");
            }
            catch (Exception e) { return Fail(AccountResult.Failed, "Gagal menyimpan nama: " + e.Message); }
#else
            await Task.Yield();
            return Fail(AccountResult.Unavailable, "Firebase SDK belum aktif.");
#endif
        }
    }
}