using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelSelectScreen : MonoBehaviour
{
    public void LoadMainMenu()
    {
        FMODManager.Play(Sounds.ButtonClick);
        SceneManager.LoadScene("MainMenu");
    }
}
