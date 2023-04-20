using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;

public class CatController : MonoBehaviour
{

    public Transform pointA;
    public Transform pointB;
    public float speed = 1.0f;
    public float waitTime = 3.0f;
    private bool isMovingToPointA = true;
    private SkeletonAnimation skeletonAnimation;
    private bool isMoving = false;

    void Start()
    {
        skeletonAnimation = GetComponent<SkeletonAnimation>();
        transform.position = pointA.position;
        StartCoroutine(WaitAtPoint(waitTime, false));
    }

    void Update()
    {
        if (!isMoving)
            return;
        if (isMovingToPointA)
        {
            float distanceToA = Vector2.Distance(transform.position, pointA.position);
            transform.position = Vector3.MoveTowards(transform.position, pointA.position, speed * Time.deltaTime);
            //skeletonAnimation.AnimationState.SetAnimation(0, "run2", true);
            transform.localScale = new Vector3(-1, 1, 1);
            if (distanceToA <= 0.1f)
            {

                //skeletonAnimation.AnimationState.SetAnimation(0, "happy", false);
                StartCoroutine(WaitAtPoint(waitTime,isMovingToPointA));
            }
        }
        else
        {
            transform.position = Vector3.MoveTowards(transform.position, pointB.position, speed * Time.deltaTime);
            float distanceToB = Vector2.Distance(transform.position, pointB.position);
            //skeletonAnimation.AnimationState.SetAnimation(0, "run2", true);
            transform.localScale = new Vector3(1, 1, 1);
            if (distanceToB <= 0.1f)
            {

                //skeletonAnimation.AnimationState.SetAnimation(0, "idle2", false);
                StartCoroutine(WaitAtPoint(waitTime,isMovingToPointA));
                
            }
        }
    }

    IEnumerator WaitAtPoint(float seconds,bool isMove)
    {
        isMoving = false;
        skeletonAnimation.AnimationState.SetAnimation(0, "idle2", false);
        yield return new WaitForSeconds(seconds);
        Debug.Log("chuyen state");
        isMovingToPointA = !isMove;
        skeletonAnimation.AnimationState.SetAnimation(0, "run2", true);
        isMoving = true;
    }
}
