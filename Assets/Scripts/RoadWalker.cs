using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadWalker : MonoBehaviour
{
    public float Speed;
    public SpriteRenderer MainRenderer;

    [HideInInspector]
    public Road currentRoad;
    [HideInInspector]
    public int currentTargetPointIndex;
    [HideInInspector]
    public Vector3 currentTargetPosition;
    [HideInInspector]
    public bool HasReachedRoadEnd;
    
    public void SetCurrentRoad(Road road, int pointIndex)
    {
        currentRoad = road;
        currentTargetPointIndex = pointIndex;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
    }

    public virtual void OnDestroy()
    {
        //If we ever add walker that are not followers, unregister them from the road here.
    }
}
