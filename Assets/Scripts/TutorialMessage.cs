using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialMessage : MonoBehaviour
{
    public Transform TargetObject;

    public void SetTarget(Vector3 position)
    {
        TargetObject.position = position;
    }
}
