using System;
using System.Collections.Generic;
using System.Threading.Tasks;
#if FIREBASE_ENABLED
using Firebase.Database;
#endif

namespace MatchaBakery.Backend
{
    public struct LeaderboardRow { public string name; public long score; }

    public static class LeaderboardClient
    {
        public static async Task<List<LeaderboardRow>> GetTopAsync(int count = 20)
        {
            var rows = new List<LeaderboardRow>();
#if FIREBASE_ENABLED
            if (CloudFunctionsClient.CurrentUid == null) return rows;
            try
            {
                var q = FirebaseDatabase.DefaultInstance.GetReference("leaderboard/alltime").OrderByChild("score").LimitToLast(count);
                var task = q.GetValueAsync();
                if (await Task.WhenAny(task, Task.Delay(8000)) != task) return rows;
                foreach (var c in (await task).Children)
                    rows.Add(new LeaderboardRow { name = Convert.ToString(c.Child("name").Value), score = Convert.ToInt64(c.Child("score").Value) });
                rows.Sort((a, b) => b.score.CompareTo(a.score));   // Firebase mengurutkan naik
            }
            catch (Exception) { }
#else
            await Task.Yield();
#endif
            return rows;
        }
    }
}