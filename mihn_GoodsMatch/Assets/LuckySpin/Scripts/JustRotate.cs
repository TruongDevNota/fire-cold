using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JustRotate : MonoBehaviour
{
    private float speed = 35f;
    private void LateUpdate()
    {
        float angle = transform.eulerAngles.z - speed * Time.deltaTime;
        transform.eulerAngles = new Vector3(0, 0, angle);
    }
}
