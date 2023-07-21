using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudController : MonoBehaviour
{
    public float speed = 5f;
    public Transform pointTarget;
    private Vector3 initialPosition; 

    private void Start()
    {
        initialPosition = transform.position;
        
    }

    private void Update()
    {
        transform.Translate(Vector3.right * speed * Time.deltaTime);

        if (transform.position.x >pointTarget.position.x )
        {
            ResetCloudPosition();
        }
    }

    private void ResetCloudPosition()
    {
        Debug.Log("reset");
        transform.position = initialPosition;
    }
}
