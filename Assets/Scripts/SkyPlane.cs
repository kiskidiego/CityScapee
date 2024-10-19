using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkyPlane : MonoBehaviour
{
    void Update()
    {
        //Debug.Log(Camera.main.name);
        //Debug.Log(Camera.main.transform.position);
        //Debug.Log(transform.position);
        transform.position = Camera.main.transform.position + Camera.main.transform.forward * 400;
        transform.LookAt(Camera.main.transform);
        transform.Rotate(Vector3.right, 90);
    }
}
