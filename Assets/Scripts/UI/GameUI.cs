using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameUI : MonoBehaviour
{
    public const string GOLD_COUNTER_PREFIX = "x ";
    public const string CAP_PREFIX = "MAX:\n";

    [Header("UI References")]
    public GameBoard MainGameBoard;
    public Sprite TranparentSprite;
    public Image PanicBar;
    public Image MinPanicBar;
    public Image RageBar;
    public TextMeshProUGUI GoldText;
    public Image Desire01;
    public Image Desire02;
    public Image Desire03;
    public Image PauseButton;
    public Sprite PauseSprite;
    public Sprite PlaySprite;
    public GameObject PauseOverlay;
    public AudioClip PauseMusic;
    public Button BuildMineButton;
    public Image BuildMineButton_Icon;
    public Image BuildMineButton_Icon2;
    public TextMeshProUGUI BuildMineButton_Text;
    public TextMeshProUGUI BuildMineButton_Text2;
    public Animator MineButtonAnimator;
    public TextMeshProUGUI CapMaxText;
    public Image CapStart;
    public Image CapEnd;
    public Image CapLine;

    public Dragon Dragon;

    private bool isGamePaused;
    private AudioClip PreviousMusic;
    private float PreviousMusicPos;

    private void Update()
    {
        UpdateUI();
    }

    public void UpdateUI()
    {
        PanicBar.fillAmount = Dragon.PanicRatio;
        MinPanicBar.fillAmount = Dragon.RageRatio * 0.5f;
        RageBar.fillAmount = Dragon.RageRatio;
        GoldText.text = GOLD_COUNTER_PREFIX + Dragon.GoldCoins.ToString("N0");

        //Update desires
        Desire01.sprite = Dragon.DesiredTreasure1 == null || Dragon.DesiredTreasure1.Value == Treasure.None ? TranparentSprite : Dragon.DesiredTreasure1.UISprite;
        Desire02.sprite = Dragon.DesiredTreasure2 == null || Dragon.DesiredTreasure2.Value == Treasure.None ? TranparentSprite : Dragon.DesiredTreasure2.UISprite;
        Desire03.sprite = Dragon.DesiredTreasure3 == null || Dragon.DesiredTreasure3.Value == Treasure.None ? TranparentSprite : Dragon.DesiredTreasure3.UISprite;

        if(Dragon.GoldCoins >= MainGameBoard.MineCost)
        {
            BuildMineButton.interactable = true;
            MineButtonAnimator.SetBool("Available", true);
            BuildMineButton_Icon.color = new Color(BuildMineButton_Icon.color.r, BuildMineButton_Icon.color.g, BuildMineButton_Icon.color.b, 1f);
            BuildMineButton_Icon2.color = new Color(BuildMineButton_Icon2.color.r, BuildMineButton_Icon2.color.g, BuildMineButton_Icon2.color.b, 1f);
            BuildMineButton_Text.color = new Color(BuildMineButton_Text.color.r, BuildMineButton_Text.color.g, BuildMineButton_Text.color.b, 1f);
            BuildMineButton_Text2.color = new Color(BuildMineButton_Text2.color.r, BuildMineButton_Text2.color.g, BuildMineButton_Text2.color.b, 1f);
        }
        else
        {
            BuildMineButton.interactable = false;
            MineButtonAnimator.SetBool("Available", false);
            BuildMineButton_Icon.color = new Color(BuildMineButton_Icon.color.r, BuildMineButton_Icon.color.g, BuildMineButton_Icon.color.b, 0.5f);
            BuildMineButton_Icon2.color = new Color(BuildMineButton_Icon2.color.r, BuildMineButton_Icon2.color.g, BuildMineButton_Icon2.color.b, 0.5f);
            BuildMineButton_Text.color = new Color(BuildMineButton_Text.color.r, BuildMineButton_Text.color.g, BuildMineButton_Text.color.b, 0.5f);
            BuildMineButton_Text2.color = new Color(BuildMineButton_Text2.color.r, BuildMineButton_Text2.color.g, BuildMineButton_Text2.color.b, 0.5f);
        }

        //Update gold cap
        int capAmount = MainGameBoard.CenterDragon.GoldCoinCap;
        CapMaxText.text = CAP_PREFIX + capAmount.ToString("N0");
        float capPoint = Mathf.Clamp01((float)(capAmount - MainGameBoard.MineCapBonus) / (float)(MainGameBoard.CenterDragon.MaxGoldCoins - MainGameBoard.MineCapBonus));

        float yOffset = CapEnd.rectTransform.anchoredPosition.y - CapStart.rectTransform.anchoredPosition.y;
        float yPos = CapStart.rectTransform.anchoredPosition.y + (yOffset * capPoint);
        CapLine.rectTransform.anchoredPosition = new Vector2(CapLine.rectTransform.anchoredPosition.x, yPos);

        //CapLine.transform.position = Vector3.Lerp(CapStart.transform.position, CapEnd.transform.position, capPoint);
    }

    public void ClickedDesire1()
    {
        if (Dragon.DesiredTreasure1 == null || Dragon.DesiredTreasure1.Value == Treasure.None)
            return;

        MainGameBoard.SetBrushToTreasure(Dragon.DesiredTreasure1);
    }

    public void ClickedDesire2()
    {
        if (Dragon.DesiredTreasure2 == null || Dragon.DesiredTreasure2.Value == Treasure.None)
            return;

        MainGameBoard.SetBrushToTreasure(Dragon.DesiredTreasure2);
    }

    public void ClickedDesire3()
    {
        if (Dragon.DesiredTreasure3 == null || Dragon.DesiredTreasure3.Value == Treasure.None)
            return;

        MainGameBoard.SetBrushToTreasure(Dragon.DesiredTreasure3);
    }

    public void TogglePauseGame()
    {
        if(isGamePaused)
        {
            Time.timeScale = 1f;
            PauseButton.sprite = PauseSprite;
            PauseOverlay.SetActive(false);
            isGamePaused = false;

            MainGameBoard.MusicAudioSource.clip = PreviousMusic;
            MainGameBoard.MusicAudioSource.time = PreviousMusicPos;
            MainGameBoard.MusicAudioSource.Play();
        }
        else
        {
            Time.timeScale = 0f;
            PauseButton.sprite = PlaySprite;
            PauseOverlay.SetActive(true);

            PreviousMusic = MainGameBoard.MusicAudioSource.clip;
            PreviousMusicPos = MainGameBoard.MusicAudioSource.time;

            MainGameBoard.MusicAudioSource.clip = PauseMusic;
            MainGameBoard.MusicAudioSource.Play();

            isGamePaused = true;
        }
    }
}
