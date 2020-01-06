using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class MainMenu : MonoBehaviour
{
    public Button PlayButton;
    public Button HowToPlayButton;
    public Button CreditsButton;
    public Button ExitButton;

    [Header("Panels")]
    public GameObject[] AllPanels;
    public GameObject MenuPanel;
    public GameObject ItemsPanel;

    [Header("ItemsPanel")]
    public TextMeshProUGUI Item_ClearScreen_Amount;
    public TextMeshProUGUI Item_DoubleTreasure_Amount;
    public TextMeshProUGUI Item_FastScore_Amount;
    public Button AdButton_ClearScreen;
    public Button AdButton_DoubleTreasure;
    public Button AdButton_FastScore;

    private void Start()
    {
        Time.timeScale = 1f;

        if(!FMODManager.IsMusicPlaying)
            FMODManager.Play(Music.MenuMusic);

        UpdateUI();
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.G))
        {
            GameSaveManager.Instance.CurrentPlayerData.Item_ClearScreen_Amount++;
            GameSaveManager.Instance.CurrentPlayerData.Item_DoubleTreasure_Amount++;
            GameSaveManager.Instance.CurrentPlayerData.Item_FastScore_Amount++;

            GameSaveManager.SaveToDisk();
        }

        AdButton_ClearScreen.interactable = AdButton_DoubleTreasure.interactable = AdButton_FastScore.interactable = AdsManager.RewardedAdReady;

        UpdateUI();
    }

    public void UpdateUI()
    {
        Item_ClearScreen_Amount.text = "x" + GameSaveManager.Instance.CurrentPlayerData.Item_ClearScreen_Amount;
        Item_DoubleTreasure_Amount.text = "x" + GameSaveManager.Instance.CurrentPlayerData.Item_DoubleTreasure_Amount;
        Item_FastScore_Amount.text = "x" + GameSaveManager.Instance.CurrentPlayerData.Item_FastScore_Amount;
    }

    //UI triggers
    public void ShowItemsPanel()
    {
        foreach (var panel in AllPanels)
            panel.SetActive(false);

        ItemsPanel.SetActive(true);
    }

    public void CloseItemsPanel()
    {
        foreach (var panel in AllPanels)
            panel.SetActive(false);

        MenuPanel.SetActive(true);
        ItemsPanel.SetActive(false);
    }

    public void ClickedPlay()
    {
        PlayButton.interactable = false;
        FMODManager.Play(Sounds.ButtonClick, LoadGame);
    }

    public void ClickedCredits()
    {
        CreditsButton.interactable = false;
        FMODManager.Play(Sounds.ButtonClick, LoadCredits);
    }

    public void ClickedHowTo()
    {
        HowToPlayButton.interactable = false;
        FMODManager.Play(Sounds.ButtonClick, LoadHowTo);
    }

    public void ClickedExit()
    {
        ExitButton.interactable = false;
        FMODManager.Play(Sounds.ButtonClick, Exit);
    }

    public void GetItem_ClearScreen()
    {
        AdsManager.RewardAd_ClearScreen();
    }
    public void GetItem_DoubleTreasure()
    {
        AdsManager.RewardAd_DoubleTreasure();
    }
    public void GetItem_FastScore()
    {
        AdsManager.RewardAd_FastScore();
    }

    public void LoadGame()
    {
        SceneManager.LoadScene("LevelSelect");
    }

    public void LoadCredits()
    {
        SceneManager.LoadScene("Credits");
    }

    public void LoadHowTo()
    {
        SceneManager.LoadScene("HowTo");
    }

    public void Exit()
    {
        Application.Quit();
    }
}
