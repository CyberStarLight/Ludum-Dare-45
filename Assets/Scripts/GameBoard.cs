using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameBoard : MonoBehaviour
{
    public Road[] Roads;

    public FollowerController followerPrefab;
    [SerializeField] GameObject[] placableObjects;
    public Dragon CenterDragon;

    public void UpdateSelf()
    {
        var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        switch (brush.content)
        {
            case 0: break;
            case 1: spawnFollower(mousePos); break;
            case 2: spawnMine(mousePos); break;

            default: break;
        }
    }
    public void spawnFollower(Vector2 mousePos, Treasure tresureHeld = Treasure.None)
    {
        var newFollower = Instantiate(followerPrefab, mousePos, Quaternion.identity, null);
        var searchResult = GetClosestPoint(newFollower.transform.position);
        newFollower.currentRoad = searchResult.ParentRoad;
        searchResult.ParentRoad.RegisterWalker(newFollower, searchResult.PointIndex);
        newFollower.TresureHeld = tresureHeld;
        //newFollower.TresureHeld = GetRandomTreasure();
        //newFollower.Master = CenterDragon;
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

        if (ableToPlace)
        {
            var newMine = Instantiate(placableObjects[0], mousePos, Quaternion.identity, null).GetComponent<MineController>().ore = (Treasure)brush.contentState;
        }
    }

    public Road.PointSearchResult GetClosestPoint(Vector3 position)
    {
        var closestPointOnEachRoad = Roads.Select(x => x.GetClosestPoint(position));
        var closestRoadPoint = closestPointOnEachRoad.OrderBy(x => x.Distance).FirstOrDefault();

        return closestRoadPoint;
    }
    public void setBrushContent(int value) { brush.content = value; }
    public void setBrushContentState(int value) { brush.contentState = value; }
    private static class brush
    {
        public static int content;
        public static int contentState;
        static Vector2 position;
    }

    public static Treasure GetRandomTreasure()
    {
        var v = Enum.GetValues(typeof(Treasure));
        return (Treasure)v.GetValue(Random.Range(1, v.Length));
    }
}
