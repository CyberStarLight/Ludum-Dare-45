using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameBoard : MonoBehaviour
{
    public Road[] Roads;

    public Transform TestSpawnPosition;
    public FollowerController followerPrefab;
    [SerializeField] GameObject[] placableObjects;

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
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
    }
    public void spawnFollower(Vector2 mousePos)
    {
        var newFollower = Instantiate(followerPrefab, mousePos, Quaternion.identity, null);
        var searchResult = GetClosestPoint(newFollower.transform.position);
        newFollower.currentRoad = searchResult.ParentRoad;
        searchResult.ParentRoad.RegisterWalker(newFollower, searchResult.PointIndex);
        followerPrefab.TresureHeld = (Tresure)Random.Range(1,3);
    }
    public void spawnMine(Vector2 mousePos)
    {
        bool ableToPlace = true;
        if (ableToPlace)
        {
            var newMine = Instantiate(placableObjects[0], mousePos, Quaternion.identity, null);
        }
    }

    public Road.PointSearchResult GetClosestPoint(Vector3 position)
    {
        var closestPointOnEachRoad = Roads.Select(x => x.GetClosestPoint(position));
        var closestRoadPoint = closestPointOnEachRoad.OrderBy(x => x.Distance).FirstOrDefault();

        return closestRoadPoint;
    }
    public void setBrushContent(int value) { brush.content = value; }
    private static class brush
    {
        public static int content;
        static Vector2 position;
    }
}
