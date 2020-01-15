using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GameUI : MonoBehaviour
{
    public const string GOLD_COUNTER_PREFIX = "x ";
    public const string CAP_PREFIX = "MAX:\n";

    [Header("UI References")]
    public GameBoard MainGameBoard;
    public Sprite TranparentSprite;
    public Image RageBar;
    public TextMeshProUGUI GoldText;
    public Image Desire01;
    public Image Desire02;
    public Image Desire03;
    public Image PauseButton;
    public Sprite PauseSprite;
    public Sprite PlaySprite;
    public GameObject MenuPanel;
    public GameObject PauseOverlay;
    public GameObject PauseText;
    public AdvancedButton BuildMineButton;
    public Image BuildMineButton_Icon;
    public TextMeshProUGUI BuildMineButton_Text;
    public TextMeshProUGUI BuildMineButton_Text2;
    public Animator MineButtonAnimator;
    public Animator CapLineAnimator;
    public TextMeshProUGUI CapMaxText;
    public Image CapStart;
    public Image CapEnd;
    public Image CapLine;
    public TextMeshProUGUI ScoreText;
    public StageClearScreen ClearScreen;

    public Dragon Dragon;

    [Header("Inventory")]
    public AdvancedButton ClearScreenButton;
    public AdvancedButton SpeedUpButton;
    public AdvancedButton DoubleTreasureButton;
    public TextMeshProUGUI ClearScreenItemAmount;
    public TextMeshProUGUI SpeedItemAmount;
    public TextMeshProUGUI DoubleTreasureItemAmount;


    private bool isGamePaused;
    private bool isMenuOpened;
    private Music PreviousMusic;
    private float PreviousMusicPos;

    private void Start()
    {
        BuildMineButton_Text.text = string.Format("-{0:n0} Gold", GameSettings.LevelConfig.MineCost);
        BuildMineButton_Text2.text = string.Format("+{0:n0} Cap", GameSettings.LevelConfig.Mine_CapBonus);
    }

    private void Update()
    {
        UpdateUI();
    }

    public void UpdateUI()
    {
        //PanicBar.fillAmount = Dragon.PanicRatio;
        //MinPanicBar.fillAmount = Dragon.RageRatio * 0.5f;
        RageBar.fillAmount = Dragon.RageRatio;
        GoldText.text = GOLD_COUNTER_PREFIX + MainGameBoard.GoldCoins.ToString("N0");

        //Update desires
        Desire01.sprite = Dragon.DesiredTreasure1 == null || Dragon.DesiredTreasure1.Value == Treasure.None ? TranparentSprite : Dragon.DesiredTreasure1.UISprite;
        Desire02.sprite = Dragon.DesiredTreasure2 == null || Dragon.DesiredTreasure2.Value == Treasure.None ? TranparentSprite : Dragon.DesiredTreasure2.UISprite;
        Desire03.sprite = Dragon.DesiredTreasure3 == null || Dragon.DesiredTreasure3.Value == Treasure.None ? TranparentSprite : Dragon.DesiredTreasure3.UISprite;

        //update mine button
        if(MainGameBoard.CanAffordMine && !MainGameBoard.BuildingDisabeld && MainGameBoard.MineCount < GameSettings.LevelConfig.Gold_RequiredMineCount)
        {
            BuildMineButton.SetInteractable(true);
            if (MineButtonAnimator.gameObject.activeSelf)
                MineButtonAnimator.SetBool("Available", true);
            //BuildMineButton_Icon.color = new Color(BuildMineButton_Icon.color.r, BuildMineButton_Icon.color.g, BuildMineButton_Icon.color.b, 1f);
            //BuildMineButton_Icon2.color = new Color(BuildMineButton_Icon2.color.r, BuildMineButton_Icon2.color.g, BuildMineButton_Icon2.color.b, 1f);
            //BuildMineButton_Text.color = new Color(BuildMineButton_Text.color.r, BuildMineButton_Text.color.g, BuildMineButton_Text.color.b, 1f);
            //BuildMineButton_Text2.color = new Color(BuildMineButton_Text2.color.r, BuildMineButton_Text2.color.g, BuildMineButton_Text2.color.b, 1f);
        }
        else
        {
            BuildMineButton.SetInteractable(false);
            if(MineButtonAnimator.gameObject.activeSelf)
                MineButtonAnimator.SetBool("Available", false);
            //BuildMineButton_Icon.color = new Color(BuildMineButton_Icon.color.r, BuildMineButton_Icon.color.g, BuildMineButton_Icon.color.b, 0.5f);
            //BuildMineButton_Icon2.color = new Color(BuildMineButton_Icon2.color.r, BuildMineButton_Icon2.color.g, BuildMineButton_Icon2.color.b, 0.5f);
            //BuildMineButton_Text.color = new Color(BuildMineButton_Text.color.r, BuildMineButton_Text.color.g, BuildMineButton_Text.color.b, 0.5f);
            //BuildMineButton_Text2.color = new Color(BuildMineButton_Text2.color.r, BuildMineButton_Text2.color.g, BuildMineButton_Text2.color.b, 0.5f);
        }

        //Update gold cap
        int capAmount = MainGameBoard.GoldCoinCap;

        if(capAmount <= 600000)
        {
            CapMaxText.rectTransform.pivot = new Vector2(0.5f, 0f);
            CapMaxText.rectTransform.anchoredPosition = new Vector2(0f, 0f);
            CapMaxText.margin = new Vector4(0f, 0f, 0f, 0f);
        }
        else if(capAmount < 1000000)
        {
            CapMaxText.rectTransform.pivot = new Vector2(0.5f, 1f);
            CapMaxText.rectTransform.anchoredPosition = new Vector2(0f, 0f);
            CapMaxText.margin = new Vector4(0f, 15f, 0f, 0f);
        }
        else
        {
            CapMaxText.gameObject.SetActive(false);
        }

        CapMaxText.text = CAP_PREFIX + capAmount.ToString("N0");
        float capPoint = Mathf.Clamp01((float)(capAmount - GameSettings.LevelConfig.Mine_CapBonus) / (float)(GameSettings.LevelConfig.Gold_Max - GameSettings.LevelConfig.Mine_CapBonus));

        float yOffset = CapEnd.rectTransform.anchoredPosition.y - CapStart.rectTransform.anchoredPosition.y;
        float yPos = CapStart.rectTransform.anchoredPosition.y + (yOffset * capPoint);
        CapLine.rectTransform.anchoredPosition = new Vector2(CapLine.rectTransform.anchoredPosition.x, yPos);

        CapLineAnimator.SetBool("Full", MainGameBoard.GoldCoins == MainGameBoard.GoldCoinCap);

        //Update score
        ScoreText.text = string.Format("Score: {0}", Mathf.FloorToInt(MainGameBoard.CurrentScore));

        //Update inventory
        ClearScreenItemAmount.text = "x" + MainGameBoard.ClearScreenItemAmount.ToString("00");
        SpeedItemAmount.text = "x" + MainGameBoard.SpeedItemAmount.ToString("00");
        DoubleTreasureItemAmount.text = "x" + MainGameBoard.DoubleTreasureItemAmount.ToString("00");

        ClearScreenButton.SetInteractable(!MainGameBoard.ItemsDisabeld && Time.time > MainGameBoard.NoClearBeforeTime && MainGameBoard.ExistingFollowers.Count > 0 && MainGameBoard.ClearScreenItemAmount > 0);
        SpeedUpButton.SetInteractable(!MainGameBoard.ItemsDisabeld && MainGameBoard.SpeedItemAmount > 0);
        DoubleTreasureButton.SetInteractable(!MainGameBoard.ItemsDisabeld && MainGameBoard.DoubleTreasureItemAmount > 0);
    }
    
    //Cap line
    public void ShakeCapLine()
    {
        CapLineAnimator.SetTrigger("Shake");
    }

    //Menu
    public void ToggleMenu()
    {
        if(isMenuOpened)
        {
            Time.timeScale = 1f;
            PauseButton.sprite = PauseSprite;
            PauseOverlay.SetActive(false);
            PauseText.SetActive(false);
            MenuPanel.SetActive(false);

            FMODManager.Play(PreviousMusic, PreviousMusicPos);
            
            isMenuOpened = false;
            isGamePaused = false;
        }
        else
        {
            Time.timeScale = 0f;
            PauseButton.sprite = PlaySprite;
            PauseOverlay.SetActive(true);
            PauseText.SetActive(false);
            MenuPanel.SetActive(true);

            if (!isGamePaused)
            {
                PreviousMusic = FMODManager.CurrentMusic;
                PreviousMusicPos = FMODManager.CurrentMusicTime;

                FMODManager.Play(Music.MenuMusic);
            }

            isGamePaused = false;
            isMenuOpened = true;
        }
    }

    public void TogglePauseGame()
    {
        if(isMenuOpened)
        {
            ToggleMenu();
            return;
        }

        if(isGamePaused)
        {
            Time.timeScale = 1f;
            PauseButton.sprite = PauseSprite;
            PauseOverlay.SetActive(false);
            PauseText.SetActive(false);

            FMODManager.Play(PreviousMusic, PreviousMusicPos);
            
            isGamePaused = false;
        }
        else
        {
            Time.timeScale = 0f;
            PauseButton.sprite = PlaySprite;
            PauseOverlay.SetActive(true);
            PauseText.SetActive(true);

            PreviousMusic = FMODManager.CurrentMusic;
            PreviousMusicPos = FMODManager.CurrentMusicTime;

            FMODManager.Play(Music.MenuMusic);

            isGamePaused = true;
        }
    }
    
    public void Exit()
    {
        SceneManager.LoadScene("MainMenu");
        Time.timeScale = 1f;
    }
}
