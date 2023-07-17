using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;
using MyBox;


public class OrderMode : MonoBehaviour
{
    public Transform pointA;
    public Transform pointB;
    public float movementTime = 5f;

    private float timer = 0f;
    private bool isMoving = false;
   [SerializeField] private SkeletonGraphic cat;

    private void Start()
    {
        cat.transform.position = pointA.position;
    }
    [ButtonMethod]
    public void MoveCatToDestination()
    {
        isMoving = true;
        timer = 0f;
    }

    private void Update()
    {
        if (isMoving)
        {
            timer += Time.deltaTime;

            if (timer <= movementTime)
            {
                float t = timer / movementTime;
                cat.transform.position = Vector3.Lerp(pointA.position, pointB.position, t);
            }
            else
            {
                isMoving = false;
            }
        }
    }
}
