using System;
using System.Collections.Generic;

[Serializable] public class FlagEntry { public string key; public bool value; }   // satu catatan ya/tidak
[Serializable] public class ItemEntry { public string id; public int count; }     // satu baris item di tas

[Serializable]
public class SaveData
{
    public int version = 2;
    public string playerId;
    public string playerName;
    public long updatedAtUtcMs;
    public int revision;
    public string sceneName;
    public bool hasPosition;
    public float posX;
    public float posY;
    public long money;
    public float playTimeSeconds;
    public List<FlagEntry> flags = new List<FlagEntry>();
    public List<ItemEntry> items = new List<ItemEntry>();
}