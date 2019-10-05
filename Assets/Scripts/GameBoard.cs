﻿using System;
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
        Debug.Log("gameboard::UpdateSelf");
    }
    public void spawnFollower(Vector2 mousePos, Tresure tresureHeld = Tresure.None)
    {
        var newFollower = Instantiate(followerPrefab, mousePos, Quaternion.identity, null);
        var searchResult = GetClosestPoint(newFollower.transform.position);
        newFollower.currentRoad = searchResult.ParentRoad;
        searchResult.ParentRoad.RegisterWalker(newFollower, searchResult.PointIndex);
        followerPrefab.TresureHeld = tresureHeld;
        newFollower.TresureHeld = GetRandomTreasure();
        newFollower.Master = CenterDragon;
    }
    public void spawnMine(Vector2 mousePos)
    {
        bool ableToPlace = true;
        if (ableToPlace)
        {
            var newMine = Instantiate(placableObjects[0], mousePos, Quaternion.identity, null).GetComponent<MineController>().ore = (Tresure)brush.contentState;
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
