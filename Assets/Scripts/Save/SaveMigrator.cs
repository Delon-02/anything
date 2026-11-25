using System.Collections.Generic;
using UnityEngine;

namespace MatchaBakery.Backend
{
    public static class SaveMigrator
    {
        public const int CurrentVersion = 2;

        public static SaveData Migrate(SaveData d)
        {
            if (d == null) return null;
            if (d.version > CurrentVersion)
            {
                Debug.LogWarning("[SaveMigrator] Save versi " + d.version + " lebih baru dari game ini.");
                return d;
            }
            while (d.version < CurrentVersion)
            {
                switch (d.version)
                {
                    case 0: d.version = 1; break;
                    case 1:
                        if (d.flags == null) d.flags = new List<FlagEntry>();
                        if (d.items == null) d.items = new List<ItemEntry>();
                        d.version = 2; break;
                    default: d.version = CurrentVersion; break;
                }
            }
            return d;
        }
    }
}