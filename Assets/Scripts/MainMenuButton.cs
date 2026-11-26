using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuButton : MonoBehaviour
{
    [SerializeField] private string sceneName = "Game";

    private void OnMouseDown()
    {
        NewGame();
    }

    public void NewGame()
    {
        PlayerPrefs.DeleteKey("LastGameplayScene");
        PlayerPrefs.Save();

        SceneManager.LoadScene(sceneName);
    }
}