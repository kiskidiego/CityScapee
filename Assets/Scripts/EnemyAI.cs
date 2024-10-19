using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [SerializeField] Transform target;
    [SerializeField] float maxSpeed = 10f;
    [SerializeField] float acceleration = 5f;
    [SerializeField] float rotationSpeed = 5f;
    public int randomSeed;
    Vector3 spawnPoint;
    Rigidbody rigidBody;
    bool waiting = false;

	private void Start()
	{
        spawnPoint = transform.position;
		rigidBody = GetComponent<Rigidbody>();
        SetTarget(GameObject.FindGameObjectWithTag("Player").transform);
	}
	// Update is called once per frame
	void Update()
    {
        if(waiting)
        {
            return;
        }
		Vector3 targetPosition = target.position + Vector3.up;
		Vector3 direction = targetPosition - transform.position;
        direction.Normalize();
        Ray ray = new Ray(rigidBody.position, transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Min(5, (targetPosition - transform.position).magnitude), ~LayerMask.GetMask("Player", "Enemies")))
        {
            //Debug.DrawRay(ray.origin, ray.direction * 5, Color.red);
            //Debug.Log(Vector3.SignedAngle(transform.forward, hit.normal, transform.up));
            if(Vector3.SignedAngle(transform.forward, hit.normal, transform.up) > 0)
            {
                transform.RotateAround(transform.position, transform.up, 10);
			}
			else
            {
				transform.RotateAround(transform.position, transform.up, -10);
			}

        }
        else
        {
            rigidBody.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), rotationSpeed * Time.deltaTime);
        }

        Random.InitState(randomSeed++);
        if(Random.Range(0f, 1f) < 0.0005f)
        {
			switch(Random.Range(0, 3))
            {
                case 0:
                    SoundManager.Instance.PlaySound(SoundManager.Sounds.ENEMY_SOUND_1, transform);
                    break;
                case 1:
                    SoundManager.Instance.PlaySound(SoundManager.Sounds.ENEMY_SOUND_2, transform);
                    break;
                case 2:
                    SoundManager.Instance.PlaySound(SoundManager.Sounds.ENEMY_SOUND_3, transform);
                    break;
            }
        }
    }
	private void FixedUpdate()
	{
        if(waiting)
        {
			return;
		}
		rigidBody.AddForce((transform.forward * acceleration + Utilities.FlattenVectorY(rigidBody.velocity) * -acceleration / maxSpeed) * Time.fixedDeltaTime, ForceMode.VelocityChange);
	}
    public void Wait()
    {
        waiting = true;
        rigidBody.velocity = Vector3.zero;
        rigidBody.angularVelocity = Vector3.zero;
        Invoke(nameof(Resume), 1.5f);
    }
    void Resume()
    {
        waiting = false;
    }
	public void SetTarget(Transform target)
    {
        //Debug.Log("Setting target to " + target.name);
		this.target = target;
        GetComponentInChildren<EnemyGun>().SetPlayer(target.gameObject);
	}
	private void LateUpdate()
	{
		if (!PlayerManager.Instance.canTakeDamage)
		{
			transform.position = spawnPoint;
		}
	}
}
