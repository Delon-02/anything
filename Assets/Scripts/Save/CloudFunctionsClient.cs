using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
#if FIREBASE_ENABLED
using Firebase;
using Firebase.Auth;
using Firebase.Functions;
#endif

namespace MatchaBakery.Backend
{
    public static class CloudFunctionsClient
    {
        public const string Region = "asia-southeast1";
        public static bool ForceOffline;   // tombol uji: pura-pura offline

        public static string CurrentUid
        {
            get
            {
#if FIREBASE_ENABLED
                if (FirebaseBootstrap.Instance == null || !FirebaseBootstrap.Instance.Ready) return null;
                var u = FirebaseAuth.DefaultInstance.CurrentUser;
                return u != null ? u.UserId : null;
#else
                return null;
#endif
            }
        }

        /// Kirim batch aksi. Batch kosong = hanya minta state server terbaru.
        public static async Task<CallOutcome> SubmitBatchAsync(List<PendingAction> batch, int timeoutMs = 12000)
        {
            if (ForceOffline || Application.internetReachability == NetworkReachability.NotReachable)
                return new CallOutcome { Status = CallStatus.Offline };
#if FIREBASE_ENABLED
            if (CurrentUid == null) return new CallOutcome { Status = CallStatus.AuthRequired, Detail = "Belum login" };

            var outcome = await TryOnce(batch, timeoutMs);
            if (outcome.Status == CallStatus.AuthRequired)
            {
                try   // token kedaluwarsa/dicabut: paksa refresh sekali lalu coba lagi
                {
                    await FirebaseAuth.DefaultInstance.CurrentUser.TokenAsync(true);
                    outcome = await TryOnce(batch, timeoutMs);
                }
                catch (Exception e) { outcome.Detail = e.Message; }
            }
            return outcome;
#else
            await Task.Yield();
            return new CallOutcome { Status = CallStatus.Unavailable, Detail = "Firebase SDK belum aktif" };
#endif
        }

#if FIREBASE_ENABLED
        static async Task<CallOutcome> TryOnce(List<PendingAction> batch, int timeoutMs)
        {
            try
            {
                var list = new List<object>();
                foreach (var a in batch)
                    list.Add(new Dictionary<string, object> {
                        { "actionId", a.actionId }, { "type", a.type }, { "payloadJson", a.payloadJson } });
                var data = new Dictionary<string, object> { { "actions", list } };

                var fn = FirebaseFunctions.GetInstance(FirebaseApp.DefaultInstance, Region).GetHttpsCallable("submitActions");
                var call = fn.CallAsync(data);
                var winner = await Task.WhenAny(call, Task.Delay(timeoutMs));
                if (winner != call)
                {
                    var ignore = call.ContinueWith(t => { var ex = t.Exception; });
                    return new CallOutcome { Status = CallStatus.Timeout, Detail = "Server lambat merespons" };   // aman: aksi idempoten
                }

                var res = await call;
                var dict = res.Data as IDictionary<object, object>;
                string json = (dict != null && dict.ContainsKey("json")) ? dict["json"] as string : null;
                if (string.IsNullOrEmpty(json))
                    return new CallOutcome { Status = CallStatus.RetryLater, Detail = "Respons kosong" };
                return new CallOutcome { Status = CallStatus.Success, Response = JsonUtility.FromJson<BatchResponse>(json) };
            }
            catch (Exception e) { return Classify(e); }
        }

        static CallOutcome Classify(Exception e)
        {
            while (e is AggregateException ag && ag.InnerException != null) e = ag.InnerException;
            string code = (e is FunctionsException fe) ? fe.ErrorCode.ToString() : e.GetType().Name;
            switch (code)
            {
                case "Unauthenticated": return new CallOutcome { Status = CallStatus.AuthRequired, Detail = e.Message };
                case "InvalidArgument": return new CallOutcome { Status = CallStatus.Rejected, Detail = e.Message };
                default:                return new CallOutcome { Status = CallStatus.RetryLater, Detail = code + ": " + e.Message };
            }
        }
#endif
    }
}