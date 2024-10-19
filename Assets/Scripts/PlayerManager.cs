using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerManager : MonoBehaviour
{
    [SerializeField] int lives = 3;
	Text lifeText;
	Text countDownText;
	MovementController movementController;
	GameManager gameManager;
	Transform playerSpawnPoint;
	int maxLives;
	public bool canTakeDamage { get; private set;}
	Rigidbody rb;
	public static PlayerManager Instance {get; private set;}
	bool isMakingWalkingSound;
	private void Awake()
	{
		maxLives = lives;
		Instance = this;
		movementController = GetComponent<MovementController>();
		rb = GetComponent<Rigidbody>();
	}
	void Start()
	{
		StartCoroutine(CountDown());
	}
	private void OnTriggerEnter(Collider other)
	{
		switch (other.tag)
		{
			case "Hazard":
				LoseLife();
				break;
			case "GoalPoint":
				other.gameObject.SetActive(false);
				gameManager.GoalGet();
				break;
		}
	}
	private void OnCollisionEnter(Collision collision)
	{
		switch (collision.collider.tag)
		{
			case "Hazard":
				LoseLife();
				break;
			case "GoalPoint":
				collision.collider.gameObject.SetActive(false);
				gameManager.GoalGet();
				break;
		}
	}
	private void Update()
	{
		if (transform.position.y < -35 || transform.position.y > 125)
		{
			LoseLife();
		}
		if(!isMakingWalkingSound && movementController.walking)
		{
			StartCoroutine(MakeWalkingSound());
		}
	}
	public void LoseLife()
	{
		if (!gameManager.inGame || !canTakeDamage)
			return;
		lives--;
		if(lives > 0)
			SoundManager.Instance.PlaySound(SoundManager.Sounds.LOSE_LIFE, transform);
		lifeText.text = "Lives: " + lives;
		if (lives <= 0)
		{
			gameManager.EndGame(false);
		}
		else
		{
			Respawn();
		}
	}
	private void Respawn()
	{
		rb.velocity = Vector3.zero;
		transform.position = playerSpawnPoint.position;
		StartCoroutine(CountDown());
	}
	public void SetGameManager(GameManager gm, Transform sp)
	{
		gameManager = gm;
		playerSpawnPoint = sp;
	}
	public void SetLifeText(Text t)
	{
		lifeText = t;
		lifeText.text = "Lives: " + lives;
	}
	public void SetCountDownText(Text t)
	{
		countDownText = t;
	}
	IEnumerator CountDown()
	{
		countDownText.gameObject.SetActive(true);
		rb.isKinematic = true;
		canTakeDamage = false;
		Time.timeScale = 0;
		countDownText.text = "3";
		yield return new WaitForSecondsRealtime(1);
		countDownText.text = "2";
		yield return new WaitForSecondsRealtime(1);
		countDownText.text = "1";
		yield return new WaitForSecondsRealtime(1);
		countDownText.text = "GO!";
		yield return new WaitForSecondsRealtime(0.5f);
		Time.timeScale = 1;
		canTakeDamage = true;
		countDownText.gameObject.SetActive(false);
		yield return new WaitForEndOfFrame();
		rb.isKinematic = false;
		rb.velocity = Vector3.zero;
	}
	public int GetUsedLives()
	{
		return maxLives - lives;
	}
	IEnumerator MakeWalkingSound()
	{
		isMakingWalkingSound = true;
		while(movementController.walking)
		{
			//Debug.Log("Walking sound");
			switch(Random.Range(0, 4))
			{
				case 0:
					SoundManager.Instance.PlaySound(SoundManager.Sounds.STEP_1, transform);
					break;
				case 1:
					SoundManager.Instance.PlaySound(SoundManager.Sounds.STEP_2, transform);
					break;
				case 2:
					SoundManager.Instance.PlaySound(SoundManager.Sounds.STEP_3, transform);
					break;
				case 3:
					SoundManager.Instance.PlaySound(SoundManager.Sounds.STEP_4, transform);
					break;
			}
			yield return new WaitForSeconds(movementController.sprinting ? 0.2f : 0.3f);
		}
		isMakingWalkingSound = false;
	}
}
