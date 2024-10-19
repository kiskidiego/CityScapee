using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotatingObject : MonoBehaviour
{
	float speed = 20f;
	int direction = 1;
	private void Start()
	{
		speed = Random.Range(30f, 50f);
		direction = Random.Range(0, 2) == 0 ? 1 : -1;
	}
	void Update()
    {
        transform.Rotate(Vector3.up * Time.deltaTime * speed * direction);
    }
}
