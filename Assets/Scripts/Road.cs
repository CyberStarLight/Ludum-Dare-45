using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Road : MonoBehaviour
{
    public Transform[] points;
    public GameBoard MainGameBoard;

    private HashSet<FollowerController> registeredFollowers;

    private void Awake()
    {
        registeredFollowers = new HashSet<FollowerController>();

        points = new Transform[transform.childCount];
        for (int i = 0; i < points.Length; i++)
        {
            points[i] = transform.GetChild(i);
        }
    }

    private void FixedUpdate()
    {
        if (MainGameBoard.HasGameEnded)
            return;

        //Move all walkers registered to this path forward along this path.
        foreach (var walker in registeredFollowers)
        {
            if (walker.HasReachedRoadEnd)
                continue;
            
            //Get distance the walker needs to walk this frame
            float currentSpeed = walker.Speed + (walker.Master.PanicRatio * walker.MaxPanicSpeedBonus);

            currentSpeed *= GameSettings.LevelConfig.FollowerSpeedMultiplier;
            currentSpeed *= MainGameBoard.SpeedItemMultiplier;

            walker.MainAnimator.speed = currentSpeed * 2;
            
            float distanceToTravel = currentSpeed * Time.fixedDeltaTime;
            
            while (distanceToTravel > 0f)
            {
                float distanceToNextPoint = Vector2.Distance(walker.transform.position, walker.currentTargetPosition);

                //flip the sprite if needed
                walker.MainRenderer.flipX = walker.currentTargetPosition.x < walker.transform.position.x;
                //walker.Dust.transform.localScale = walker.MainRenderer.flipX ? new Vector3(-1f, 1f, 1f) : new Vector3(1f, 1f, 1f);

                if (distanceToNextPoint > distanceToTravel)
                {
                    //Move walker forward
                    walker.transform.position = Vector2.MoveTowards(walker.transform.position, walker.currentTargetPosition, distanceToTravel);
                    break;
                }
                else if (distanceToNextPoint <= distanceToTravel)
                {
                    //Move walker directly to the point, then continue moving towards the next point 
                    walker.transform.position = walker.currentTargetPosition;
                    distanceToTravel -= distanceToNextPoint;

                    //set next target point, or stop walking if you reached the last point on the road
                    if (walker.currentTargetPointIndex + 1 < points.Length)
                    {
                        walker.currentTargetPointIndex += 1;
                        walker.currentTargetPosition = GetPositionFromPoint(walker.currentTargetPointIndex);
                    }
                    else
                    {
                        walker.currentTargetPointIndex = -1;
                        walker.HasReachedRoadEnd = true;
                        break;
                    }
                }
            }
        }
    }

    public void RegisterWalker(FollowerController walker, int targetPointIndex)
    {
        walker.currentTargetPointIndex = targetPointIndex;
        walker.currentTargetPosition = GetPositionFromPoint(targetPointIndex);

        registeredFollowers.Add(walker);
    }

    public void UnregisterWalker(FollowerController walker)
    {
        registeredFollowers.Remove(walker);
    }

    public Vector3 GetPositionFromPoint(int pointIndex)
    {
        if (pointIndex >= 0 && pointIndex < points.Length)
            return points[pointIndex].position;
        else
            return Vector3.zero;
    }

    public PointSearchResult GetClosestPoint(Vector3 position)
    {
        var closestPoint = 
            points
            .Select(x => new { Point = x, Distance = Vector3.Distance(position, x.position) })
            .OrderBy(x => x.Distance)
            .First();
        
        return new PointSearchResult() {
            ParentRoad = this,
            PointObject = closestPoint.Point,
            PointIndex = Array.IndexOf(points, closestPoint.Point),

            OriginPosition = position,
            PointPosition = closestPoint.Point.position,
            Distance = closestPoint.Distance,
        };
    }
    
    public class Point
    {
        public Road ParentRoad;
        public Transform PointObject;
    }

    public class PointSearchResult
    {
        public Road ParentRoad;
        public Transform PointObject;
        public int PointIndex;

        public Vector3 OriginPosition;
        public Vector3 PointPosition;
        public float Distance;
    }
}
