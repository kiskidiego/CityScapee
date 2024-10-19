using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyGun : MonoBehaviour
{
    [SerializeField] GameObject bulletPrefab;
    [SerializeField] Transform bulletSpawnPoint;
    [SerializeField] float bulletSpeed = 10;
    [SerializeField] float fireCooldown = 1.5f;
    bool canFire = true;
    GameObject player;
    EnemyAI enemyAI;
    void Start()
    {
		enemyAI = GetComponentInParent<EnemyAI>();
    }
    void Update()
    {
        transform.LookAt(player.transform.position + Vector3.up);
		if (!canFire)
		{
			return;
		}
		Ray ray = new Ray(bulletSpawnPoint.position, bulletSpawnPoint.forward);
        RaycastHit hit;
        //Debug.DrawRay(ray.origin, ray.direction * 10, Color.red);
        if (Physics.SphereCast(ray, 0.125f, out hit, 20))
        {
			if (hit.collider.gameObject == player)
            {
                SoundManager.Instance.PlaySound(SoundManager.Sounds.ENEMY_SHOT, transform.position);
                Instantiate(bulletPrefab, bulletSpawnPoint.position, bulletSpawnPoint.rotation).GetComponent<Rigidbody>().velocity = bulletSpawnPoint.forward * bulletSpeed;
				Invoke(nameof(EnableFire), fireCooldown);
                canFire = false;
                enemyAI.Wait();
			}
		}
    }
    void EnableFire()
    {
		canFire = true;
	}

	internal void SetPlayer(GameObject gameObject)
	{
		player = gameObject;
	}
}
