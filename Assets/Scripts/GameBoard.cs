using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameBoard : MonoBehaviour
{
    public Road[] Roads;

    public Transform TestSpawnPosition;
    public RoadWalker TestWalkerPrefab;
    
    public void Update()
    {
        if(Input.GetKeyDown(KeyCode.Mouse0))
        {
            var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            var newFollower = Instantiate(TestWalkerPrefab, mousePos, Quaternion.identity, null);

            var searchResult = GetClosestPoint(newFollower.transform.position);

            newFollower.currentRoad = searchResult.ParentRoad;
            searchResult.ParentRoad.RegisterWalker(newFollower, searchResult.PointIndex);
        }
    }

    public Road.PointSearchResult GetClosestPoint(Vector3 position)
    {
        var closestPointOnEachRoad = Roads.Select(x => x.GetClosestPoint(position));
        var closestRoadPoint = closestPointOnEachRoad.OrderBy(x => x.Distance).FirstOrDefault();

        return closestRoadPoint;
    }
}
