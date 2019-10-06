using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameUI : MonoBehaviour
{
    public const string GOLD_COUNTER_PREFIX = "Gold: ";

    [Header("UI References")]
    public GameBoard MainGameBoard;
    public Sprite TranparentSprite;
    public Image PanicBar;
    public Image MinPanicBar;
    public Image RageBar;
    public TextMeshPro GoldText;
    public Image Desire01;
    public Image Desire02;
    public Image Desire03;

    public Dragon Dragon;

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
}
