using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class Menu : MonoBehaviour
{
    public static Menu instance;
    private void Awake()
    {
        instance = this;
    }
    public void ChangeScene(string sceneName)
    {

        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }

    public void ChangeScene(int sceneIndex)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneIndex);
    }

    
    public void QuitGame()
    {
        Application.Quit();
    }
}
