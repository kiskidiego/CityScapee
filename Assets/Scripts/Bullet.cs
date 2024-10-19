using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] float lifeTime = 2;
    void Start()
    {
		Invoke(nameof(Die), lifeTime);
	}
	private void OnTriggerEnter(Collider other)
	{
		Die();
	}
	void Die()
	{
		Destroy(gameObject);
	}
	private void LateUpdate()
	{
		if(!PlayerManager.Instance.canTakeDamage)
		{
			Die();
		}
	}
}
