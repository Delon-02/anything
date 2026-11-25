using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using MatchaBakery.Backend;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }
    public SaveData Current { get; private set; }

    /// Diisi oleh script pemain: SaveManager.Instance.PlayerTransform = transform;
    public Transform PlayerTransform { get; set; }
    public bool SavingEnabled { get; set; } = true;
    public string CloudStatus { get; private set; } = "Offline";   // Offline/Pending/Syncing/Synced/Error
    public bool CloudChecked { get; private set; }                  // true setelah cek cloud pertama berhasil

    public event Action Saved;
    public event Action CloudDataApplied;                           // save cloud menggantikan save lokal

    [SerializeField] string playerId = "player1";
    [SerializeField] string playerName = "Pemain";
    [SerializeField] string firstSceneName = "Outside";
    [SerializeField] float autosaveSeconds = 30f;
    [SerializeField] float cloudPushIntervalSeconds = 120f;
    [SerializeField] string[] ignoreScenes = { "MainMenu" };        // scene yang tidak dicatat sebagai "scene terakhir"

    float _timer, _sinceCloudPush;
    bool _touched, _cloudDirty, _pushing, _syncing, _restoreAvailable;

    string MainPath { get { return Path.Combine(Application.persistentDataPath, "save_" + playerId + ".json"); } }

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        ContinueGame();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy() { if (Instance == this) SceneManager.sceneLoaded -= OnSceneLoaded; }

    void Update()
    {
        if (Current == null || !SavingEnabled) return;
        float dt = Time.unscaledDeltaTime;
        Current.playTimeSeconds += dt;
        _timer += dt;
        _sinceCloudPush += dt;
        if (_timer >= autosaveSeconds) { _timer = 0f; SaveNow(); }
        if (CloudChecked && _cloudDirty && !_pushing && _sinceCloudPush >= cloudPushIntervalSeconds)
        {
            _sinceCloudPush = 0f;
            _ = PushNowAsync();
        }
    }

    void OnApplicationQuit() { SaveNow(); }
    void OnApplicationFocus(bool focus)
    {
        if (focus) return;
        SaveNow();
        if (CloudChecked && _cloudDirty && !_pushing) _ = PushNowAsync();
    }
    void OnSceneLoaded(Scene s, LoadSceneMode m) { SaveNow(); }

    // ---------- memulai game ----------
    public void ContinueGame()
    {
        Current = TryRead(MainPath) ?? TryRead(MainPath + ".bak") ?? CreateNew();   // utama -> cadangan -> baru
        _touched = false;
        _restoreAvailable = true;
    }

    public void NewGame()   // juga dipakai sebagai "reset progres"
    {
        int oldRevision = Current != null ? Current.revision : 0;
        Current = CreateNew();
        Current.revision = oldRevision;   // revision naik di SaveNow, supaya menang atas save lama di cloud
        _touched = true;
        _restoreAvailable = false;
        SaveNow();
    }

    SaveData CreateNew()
    {
        return new SaveData
        {
            version = SaveMigrator.CurrentVersion,
            playerId = playerId, playerName = playerName,
            sceneName = firstSceneName,
            updatedAtUtcMs = ServerClock.NowUtcMs
        };
    }

    SaveData TryRead(string path)
    {
        try
        {
            if (!File.Exists(path)) return null;
            return SaveMigrator.Migrate(JsonUtility.FromJson<SaveData>(File.ReadAllText(path)));
        }
        catch (Exception e) { Debug.LogWarning("[SaveManager] Gagal membaca " + path + ": " + e.Message); return null; }
    }

    // ---------- menyimpan lokal ----------
    public Task SaveNowAsync() { SaveNow(); return Task.CompletedTask; }

    public void SaveNow()
    {
        if (Current == null || !SavingEnabled) return;
        if (Current.version > SaveMigrator.CurrentVersion) return;   // save dari game versi lebih baru: jangan ditimpa

        string scene = SceneManager.GetActiveScene().name;
        if (Array.IndexOf(ignoreScenes, scene) < 0 && Current.sceneName != scene)
        {
            Current.sceneName = scene;
            Current.hasPosition = false;    // posisi lama milik scene lain
            _touched = true;
        }
        if (PlayerTransform != null)
        {
            Current.hasPosition = true;
            Current.posX = PlayerTransform.position.x;
            Current.posY = PlayerTransform.position.y;
        }
        if (_touched)   // revision & cap waktu hanya naik bila ada perubahan nyata
        {
            Current.updatedAtUtcMs = ServerClock.NowUtcMs;
            Current.revision++;
            _touched = false;
            _cloudDirty = true;
        }
        if (WriteLocal() && Saved != null) Saved();
    }

    bool WriteLocal()
    {
        try
        {
            string tmp = MainPath + ".tmp";
            File.WriteAllText(tmp, JsonUtility.ToJson(Current));
            if (File.Exists(MainPath)) File.Copy(MainPath, MainPath + ".bak", true);
            File.Copy(tmp, MainPath, true);
            File.Delete(tmp);
            return true;
        }
        catch (Exception e) { Debug.LogError("[SaveManager] Gagal menyimpan: " + e.Message); return false; }
    }

    // ---------- sinkron cloud ----------
    /// Dipanggil FirebaseBootstrap setelah login. Tarik save cloud, bandingkan, lalu pilih yang menang.
    public async Task SyncWithCloudAsync()
    {
        if (_syncing || Current == null) return;
        if (!CloudSave.IsAvailable) { CloudStatus = "Offline"; return; }
        _syncing = true;
        try
        {
            CloudStatus = "Syncing";
            var pull = await CloudSave.PullAsync();
            if (!pull.ok) { CloudStatus = "Error"; return; }
            CloudChecked = true;

            if (CloudSave.CloudWins(Current, pull.data))
            {
                Current = pull.data;
                _touched = false; _cloudDirty = false;
                WriteLocal();
                CloudStatus = "Synced";
                if (CloudDataApplied != null) CloudDataApplied();
            }
            else if (pull.data != null && CloudSave.SameStamp(Current, pull.data)) CloudStatus = "Synced";
            else if (Current.revision > 0) { _syncing = false; await PushNowAsync(); }
            else CloudStatus = "Synced";
        }
        finally { _syncing = false; }
    }

    public async Task PushNowAsync()
    {
        if (Current == null || _pushing || !CloudChecked) return;
        if (!CloudSave.IsAvailable) { CloudStatus = _cloudDirty ? "Pending" : "Offline"; return; }
        _pushing = true;
        try
        {
            CloudStatus = "Syncing";
            int revAtStart = Current.revision;
            bool ok = await CloudSave.PushJsonAsync(JsonUtility.ToJson(Current));
            CloudStatus = ok ? "Synced" : "Error";
            if (ok && Current.revision == revAtStart) _cloudDirty = false;
        }
        finally { _pushing = false; }
    }

    // ---------- ganti akun ----------
    /// Cadangan save lokal bernama save_<id>.<tag>-<waktu>.json
    public void BackupLocal(string tag)
    {
        try
        {
            if (!File.Exists(MainPath)) return;
            string name = "save_" + playerId + "." + tag + "-" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss") + ".json";
            File.Copy(MainPath, Path.Combine(Application.persistentDataPath, name), true);
        }
        catch (Exception e) { Debug.LogWarning("[SaveManager] Gagal membuat cadangan: " + e.Message); }
    }

    /// Dipanggil setelah masuk ke akun yang SUDAH ADA: data cloud akun menang, data tamu hanya dicadangkan.
    public async Task<bool> SwitchToExistingAccountAsync()
    {
        if (Current == null || _syncing) return false;
        _syncing = true;   // mencegah sinkron otomatis berjalan bersamaan (bisa menimpa data akun)
        try
        {
            SaveNow();
            BackupLocal("tamu");
            CloudChecked = false;
            _cloudDirty = false;
            CloudStatus = "Syncing";

            var pull = await CloudSave.PullAsync();
            if (!pull.ok) { CloudStatus = "Error"; return false; }

            CloudChecked = true;
            if (pull.data != null)
            {
                Current = pull.data;
                _touched = false;
                WriteLocal();
                CloudStatus = "Synced";
                if (CloudDataApplied != null) CloudDataApplied();
            }
            else if (Current.revision > 0) await PushNowAsync();   // akun masih kosong: bawa progres lokal ke akun
            else CloudStatus = "Synced";
            return true;
        }
        finally { _syncing = false; }
    }

    /// Dipanggil setelah keluar dari akun: save lokal dicadangkan lalu direset (revision 0, akun berikutnya akan menang).
    public void ResetForSignOut()
    {
        BackupLocal("keluar");
        Current = CreateNew();
        _touched = false;
        _cloudDirty = false;
        CloudChecked = false;
        _restoreAvailable = false;
        CloudStatus = "Offline";
        WriteLocal();
        if (CloudDataApplied != null) CloudDataApplied();   // beri tahu game bahwa data berganti
    }
    
    // ---------- bantuan untuk gameplay ----------
    public void MarkProgress() { _touched = true; }   // panggil bila Anda mengubah Current.* langsung

    public bool GetFlag(string key)
    {
        foreach (var f in Current.flags) if (f.key == key) return f.value;
        return false;
    }

    public void SetFlag(string key, bool value)
    {
        foreach (var f in Current.flags)
            if (f.key == key) { if (f.value != value) { f.value = value; MarkProgress(); } return; }
        Current.flags.Add(new FlagEntry { key = key, value = value });
        MarkProgress();
    }

    public int GetItemCount(string id)
    {
        foreach (var i in Current.items) if (i.id == id) return i.count;
        return 0;
    }
public void AddItem(string id, int amount)   // amount boleh negatif
    {
        foreach (var i in Current.items)
            if (i.id == id) { i.count = Mathf.Max(0, i.count + amount); MarkProgress(); return; }
        if (amount > 0) { Current.items.Add(new ItemEntry { id = id, count = amount }); MarkProgress(); }
    }

    public void AddMoney(long amount)
    {
        Current.money = Math.Max(0, Current.money + amount);
        MarkProgress();
    }

    /// Dipakai script pemain saat start game: true hanya sekali, dan hanya bila scene-nya cocok.
    public bool TryGetRestorePosition(string currentSceneName, out Vector2 position)
    {
        position = Vector2.zero;
        if (!_restoreAvailable || Current == null || !Current.hasPosition || Current.sceneName != currentSceneName) return false;
        _restoreAvailable = false;
        position = new Vector2(Current.posX, Current.posY);
        return true;
    }
}