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
    private SkeletonAnimation animator;

    void Start()
    {
        animator = GetComponent<SkeletonAnimation>();
        transform.position = pointA.position;
    }

    void Update()
    {
        if (isMovingToPointA)
        {
            transform.position = Vector3.MoveTowards(transform.position, pointA.position, speed * Time.deltaTime);
            transform.localScale = new Vector3(-1f, 1f,1);
            animator.AnimationName = "run2";
            
            if (transform.position == pointA.position)
            {
                animator.AnimationName = "idle2";
                
                StartCoroutine(WaitAtPoint(waitTime,isMovingToPointA));
            }
        }
        else
        {
            transform.position = Vector3.MoveTowards(transform.position, pointB.position, speed * Time.deltaTime);
            transform.localScale = new Vector3(1f, 1f, 1);
            animator.AnimationName = "run2";
            
            if (transform.position == pointB.position)
            {
                animator.AnimationName = "happy";
                
                StartCoroutine(WaitAtPoint(waitTime,isMovingToPointA));
            }
        }
    }

    IEnumerator WaitAtPoint(float seconds,bool ismoving)
    {
        
        yield return new WaitForSeconds(seconds);
        isMovingToPointA = !ismoving;
    }
}
