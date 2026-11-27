using UnityEngine;
using UnityEngine.SceneManagement;

public class StartButton : MonoBehaviour
{
    [Header("Nama Scene Tujuan")]
    [SerializeField] private string sceneName = "Outside";

    // Method ini dipanggil oleh OnClick() pada Button
    public void OnStartButtonClicked()
    {
        Debug.Log("Memuat scene: " + sceneName);
        SceneManager.LoadScene(sceneName);
    }
}