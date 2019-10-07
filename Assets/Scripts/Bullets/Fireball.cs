using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fireball : MonoBehaviour
{
    public const float IMPACT_DISTANCE = 0.01F;

    public float Speed;

    public Animator MainAnimator;
    [HideInInspector]
    public Transform Target;
    [HideInInspector]
    public Vector2 LastKnownPosition;

    private void FixedUpdate()
    {
        //Move towards target, explode when close enough
        Vector2 targetPos;
        if (Target != null)
        {
            LastKnownPosition = targetPos = Target.position;
        }
        else
        {
            targetPos = LastKnownPosition;
        }

        transform.position = Vector2.MoveTowards(transform.position, targetPos, Time.fixedDeltaTime * Speed);

        if (Vector2.Distance(transform.position, targetPos) <= IMPACT_DISTANCE)
            Expload();
    }

    public void Expload()
    {
        MainAnimator.SetTrigger("Explode");

        if (Target != null)
        {
            if(Target.gameObject.tag  == "Follower")
                Target.GetComponent<FollowerController>().Panic();

            Destroy(Target.gameObject);
        }
    }

    //Animator events
    private void FinishedExploding()
    {
        Destroy(gameObject);
    }
}
