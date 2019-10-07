using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public AudioSource MainAudioSource;
    public AudioClip ClickSound;

    public Button PlayButton;
    public Button HowToPlayButton;
    public Button CreditsButton;
    public Button ExitButton;

    public void ClickedPlay()
    {
        MainAudioSource.PlayOneShot(ClickSound, 2f);
        PlayButton.interactable = false;

        Invoke("LoadGame", ClickSound.length);
    }

    public void ClickedExit()
    {
        MainAudioSource.PlayOneShot(ClickSound, 2f);
        ExitButton.interactable = false;

        Invoke("Exit", ClickSound.length);
    }

    public void LoadGame()
    {
        SceneManager.LoadScene(1);
    }

    public void Exit()
    {
        Application.Quit();
    }
}
