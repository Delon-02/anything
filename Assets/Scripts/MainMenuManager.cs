using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public GameObject continueButton;
    public GameObject settingsPanel;

    void Start()
    {
    bool hasPlayed = PlayerPrefs.GetInt("HasPlayed", 0) == 1;

    // Hanya Continue yang mengikuti status pemain
    continueButton.SetActive(hasPlayed);

    // Panel Settings ditutup saat awal
    settingsPanel.SetActive(false);
    }

    public void Play()
    {
        PlayerPrefs.SetInt("HasPlayed", 1);
        PlayerPrefs.Save();

        SceneManager.LoadScene("House");
    }

    public void Continue()
    {
        SceneManager.LoadScene("House");
    }

    public void OpenSettings()
    {
        settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        settingsPanel.SetActive(false);
    }

    public void Quit()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}