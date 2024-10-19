using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
	[SerializeField] GameObject playerPrefab;
	[SerializeField] GameObject enemyPrefab;
	[SerializeField] Transform playerSpawnPoint;
	[SerializeField] Transform[] enemySpawnPoint;
	[SerializeField] MapEditor mapEditor;
	[SerializeField] GameObject goalPrefab;
	[SerializeField] int goalCount = 4;
	[SerializeField] Text lifeText;
	[SerializeField] Text goalText;
	[SerializeField] Text resultText;
	[SerializeField] Text countDownText;
	[SerializeField] Text timerText;
	PlayerManager playerManager;
	float score = 0;
	int timer = 0;
	int gottenGoals = 0;
	GameObject player;
	GameObject enemy;
	bool inMapEditor = true;
	public bool inGame = false;
	bool winner = false;
	private void Start()
	{
		StartCoroutine(GameLoop());
	}
	IEnumerator GameLoop()
	{
		lifeText.text = "";
		goalText.text = "";
		for (int i = 0; i < goalCount; i++)
		{
			Vector3 pos = new Vector3(Random.Range(-100, 100), Random.Range(-10, 90), Random.Range(-100, 100));

			while (Physics.OverlapSphere(pos, 1, LayerMask.GetMask("Buildings")).Length > 0)
			{
				//Debug.Log("Goal Point is in a building, repositioning...");
				pos = new Vector3(Random.Range(-100, 100), Random.Range(20, 90), Random.Range(-100, 100));
			}
			GameObject gPoint = Instantiate(goalPrefab, pos, Quaternion.identity);
			gPoint.layer = 6;
		}
		mapEditor.enabled = true;
		SoundManager.Instance.StartTrack(SoundManager.Tracks.MAP_EDITOR_THEME);
		yield return new WaitWhile(() => inMapEditor);
		mapEditor.enabled = false;


		lifeText.text = "Lives: 3";
		goalText.text = "Goals: 0/" + goalCount;


		player = Instantiate(playerPrefab, playerSpawnPoint.position, playerSpawnPoint.rotation);
		playerManager = player.GetComponent<PlayerManager>();
		playerManager.SetGameManager(this, playerSpawnPoint);
		playerManager.SetLifeText(lifeText);
		playerManager.SetCountDownText(countDownText);
		goalText.text = "Goals: " + gottenGoals + "/" + goalCount;

		int seed = (int)System.DateTime.Now.Ticks;

		for (int i = 0; i < enemySpawnPoint.Length; i++)
		{
			Instantiate(enemyPrefab, enemySpawnPoint[i].position, Quaternion.identity).GetComponent<EnemyAI>().randomSeed = seed;
			seed /= 2;
		}

		StartCoroutine(Timer());
		SoundManager.Instance.StartTrack(SoundManager.Tracks.GAME_THEME);
		yield return new WaitWhile(() => inGame);


		player.GetComponent<MovementController>().DeactivateInput();
		if (winner)
		{
			ProcessScore();
			SoundManager.Instance.StartTrack(SoundManager.Tracks.VICTORY_THEME);
			resultText.text = "Victoria!\nPuntuacion: " + score;
		}
		else
		{
			resultText.text = "Derrota!";
		}
		resultText.transform.parent.gameObject.SetActive(true);
		Time.timeScale = 0;
		yield return new WaitUntil(() => Input.GetButtonDown("Submit"));
		Time.timeScale = 1;
		Cursor.lockState = CursorLockMode.None;
		SceneManager.LoadScene("MainMenu");
	}
	IEnumerator Timer()
	{
		while(inGame)
		{
			//Debug.Log("Timer");
			yield return new WaitForSeconds(0.01f);
			timer++;
			string minutes = (timer / 100 / 60).ToString();
			string seconds = "0" + ((timer / 100) % 60).ToString();
			string centiseconds = "0" + (timer % 100).ToString();
			timerText.text = minutes + ":" + seconds.Substring(seconds.Length - 2) + ":" + centiseconds.Substring(centiseconds.Length - 2);
		}
	}
	public void StartGame()
	{
		inMapEditor = false;
		inGame = true;
	}
	public void EndGame(bool w)
	{
		winner = w;
		inGame = false;
		
	}
	public void GoalGet()
	{
		gottenGoals++;
		if(gottenGoals < goalCount)
		{
			SoundManager.Instance.PlaySound(SoundManager.Sounds.GOAL_GET, player.transform.position);
		}
		goalText.text = "Goals: " + gottenGoals + "/" + goalCount;
		if (gottenGoals >= goalCount)
		{
			EndGame(true);
		}
	}
	void ProcessScore()
	{
		string rawScoreData = File.ReadAllText(Application.dataPath + "/Resources/ScoreData.txt");
		string[] scoreData = rawScoreData.Split('\n');
		int[] scores = new int[scoreData.Length];
		for (int i = 0; i < scoreData.Length; i++)
		{
			if (scoreData[i] == "" || scoreData[i][0] < '0' || scoreData[i][0] > '9')
				continue;
			//Debug.Log(scoreData[i]);
			scores[i] = int.Parse(scoreData[i]);
		}
		score = int.MaxValue / timer / (1.0f + mapEditor.GetUsedPlatforms()) / (1.0f + playerManager.GetUsedLives());
		for (int i = 0; i < scores.Length; i++)
		{
			if (score > scores[i])
			{
				for (int j = scores.Length - 1; j > i; j--)
				{
					scores[j] = scores[j - 1];
				}
				scores[i] = (int)score;
				break;
			}
		}
		string newScoreData = "";
		for (int i = 0; i < 10; i++)
		{
			newScoreData += scores[i] + "\n";
		}
		File.WriteAllText(Application.dataPath + "/Resources/ScoreData.txt", newScoreData);
	}
}
