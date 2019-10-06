using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class GameBoard : MonoBehaviour
{
    public Road[] Roads;
    public FollowerController followerPrefab;
    [SerializeField] GameObject[] placableObjects;
    public Dragon CenterDragon;
    public Image MineButtonIcon;
    public int MineCost = 10000;
    
    [Header("Treasures")]
    public TreasureInfo[] Treasures;

    private void Start()
    {
        //Reset brush
        brush.content = 2;
        setBrushContentState(0);
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Mouse1))
        {
            UpdateSelf();
        }
    }

    public void UpdateSelf()
    {
        var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        switch (brush.content)
        {
            case 0: break;
            //case 1: spawnFollower(mousePos); break;
            case 2: spawnMine(mousePos); break;

            default: break;
        }
    }
    public void spawnFollower(Vector2 mousePos, TreasureInfo tresureHeld)
    {
        var newFollower = Instantiate(followerPrefab, mousePos, Quaternion.identity, null);
        var searchResult = GetClosestPoint(newFollower.transform.position);
        newFollower.currentRoad = searchResult.ParentRoad;
        searchResult.ParentRoad.RegisterWalker(newFollower, searchResult.PointIndex);
        newFollower.TreasureHeld = tresureHeld;
        newFollower.TreasureRenderer.sprite = tresureHeld.Sprite;
        newFollower.Master = CenterDragon;
    }
    public void spawnMine(Vector2 mousePos)
    {
        bool ableToPlace = true;
        foreach (var G in FindObjectsOfType<MineController>())
            if (((Vector2)G.transform.position - mousePos).magnitude < 0.6f)
                ableToPlace = false;
        //foreach (var G in GameBoard.Roads.SelectMany(x => x.points))
        //if (((Vector2)G.position - mousePos).magnitude < 0.6f)
        //    ableToPlace = false;

        //Check the player has enough gold to pay
        if (CenterDragon.GoldCoins < MineCost)
        {
            ableToPlace = false;
            CenterDragon.PlayNegativeTreasure();
        }

        if (ableToPlace)
        {
            var newMine = Instantiate(placableObjects[0], mousePos, Quaternion.identity, null).GetComponent<MineController>();
            newMine.ore = brush.treasureState;

            CenterDragon.GoldCoins -= MineCost;
        }
    }

    public Road.PointSearchResult GetClosestPoint(Vector3 position)
    {
        var closestPointOnEachRoad = Roads.Select(x => x.GetClosestPoint(position));
        var closestRoadPoint = closestPointOnEachRoad.OrderBy(x => x.Distance).FirstOrDefault();

        return closestRoadPoint;
    }

    public void setBrushContent(int value)
    {
        brush.content = value;
    }

    public void setBrushContentState(int value)
    {
        var goodTreasure = Treasures.Where(x => !x.IsTrash).ToArray();
        
        brush.contentState = Mathf.Clamp(value, 0, goodTreasure.Length-1);
        brush.treasureState = goodTreasure[value];

        MineButtonIcon.sprite = brush.treasureState.UISprite;
    }

    public void ToggleBrushState()
    {
        var goodTreasure = Treasures.Where(x => !x.IsTrash).ToArray();

        brush.contentState += 1;
        if (brush.contentState >= goodTreasure.Length)
            brush.contentState = 0;

        brush.treasureState = goodTreasure[brush.contentState];
        MineButtonIcon.sprite = brush.treasureState.UISprite;
    }

    private static class brush
    {
        public static int content;
        public static int contentState;
        public static TreasureInfo treasureState;
        static Vector2 position;
    }

    public TreasureInfo GetRandomTreasure()
    {
        var goodTreasure = Treasures.Where(x => !x.IsTrash).ToArray();

        return goodTreasure[Random.Range(0, goodTreasure.Length)];
    }

    public TreasureInfo GetRandomTrash()
    {
        var trashTreasure = Treasures.Where(x => x.IsTrash).ToArray();

        return trashTreasure[Random.Range(0, trashTreasure.Length)];
    }
}