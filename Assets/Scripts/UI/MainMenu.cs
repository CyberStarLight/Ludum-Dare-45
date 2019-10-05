using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void ClickedPlay()
    {
        SceneManager.LoadScene(1);
    }

    public void ClickedExit()
    {
        Application.Quit();
    }

}
