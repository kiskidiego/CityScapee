using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class MapEditor : MonoBehaviour
{
	class JsonContainer
	{
		public float[] coords;
		public JsonContainer(float[] coords)
		{
			this.coords = coords;
		}
	}
	[SerializeField] uint levelNumber;
    [SerializeField] Camera topCamera;
    [SerializeField] Camera sideCamera;
	[SerializeField] int platformCount = 10;
	[SerializeField] GameObject platformPrefab;
	[SerializeField] Text platformText;
	[SerializeField] GameManager gameManager;
	int maxPlatforms;
	List<GameObject> platforms = new List<GameObject>();
	Stack<float[]> gameStates = new Stack<float[]>();
	Stack<float[]> reDoStack = new Stack<float[]>();
	GameObject heldPlatform;
    bool topCameraActive = true;
	float zoom = 1;
	string path;
	private void Start()
	{
		maxPlatforms = platformCount;
		topCamera.gameObject.SetActive(true);
		sideCamera.gameObject.SetActive(false);
		path = Application.dataPath + "/Resources/LevelSaveFiles/Level" + levelNumber + "Save.json";
	}
	private void Update()
	{
		MoveCamera();
		PlatformOperations();
	}
	private void PlatformOperations()
	{
		if (heldPlatform != null)
		{
			if (Input.GetButtonUp("Fire1"))
			{
				heldPlatform = null;
			}
			else if (topCameraActive)
			{
				heldPlatform.transform.position = Camera.main.ScreenToWorldPoint(Input.mousePosition + Vector3.forward * (Camera.main.transform.position.y - heldPlatform.transform.position.y));
			}
			else
			{
				heldPlatform.transform.position = Camera.main.ScreenToWorldPoint(Input.mousePosition + Vector3.forward * (Camera.main.transform.position.x - heldPlatform.transform.position.x));
			}
		}
		else if (Input.GetButtonDown("Save"))
		{
			SaveToFile();
		}
		else if (Input.GetButtonDown("Load"))
		{
			LoadFromFile();
		}
		else if (Input.GetButtonDown("Delete"))
		{
			DeleteSaveFile();
		}
		else if (Input.GetButtonDown("Undo"))
		{
			Undo();
		}
		else if (Input.GetButtonDown("Redo"))
		{
			ReDo();
		}
		else if (Input.GetButtonDown("Cancel"))
		{
			SaveGameState(gameStates);
			reDoStack.Clear();
			ClearPlatforms();
			SoundManager.Instance.PlaySound(SoundManager.Sounds.REMOVE_PLATFORM);
		}
		else if (Input.GetButtonDown("Fire1"))
		{
			RaycastHit hit;
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			//Debug.DrawRay(ray.origin, ray.direction * 100, Color.red, 10);
			if (Physics.Raycast(ray, out hit, float.MaxValue, 1 << LayerMask.NameToLayer("Platforms")))
			{
				//Debug.Log(hit.collider.gameObject.name);
				if (Input.GetButton("Crouch"))
				{
					SaveGameState(gameStates);
					reDoStack.Clear();
					platforms.Remove(hit.collider.gameObject);
					Destroy(hit.collider.gameObject);
					platformCount++;
					platformText.text = "Platforms: " + platformCount;
					SoundManager.Instance.PlaySound(SoundManager.Sounds.REMOVE_PLATFORM);
				}
				else
				{
					SaveGameState(gameStates);
					reDoStack.Clear();
					heldPlatform = hit.collider.gameObject;
				}
			}
			else if (platformCount > 0)
			{
				SaveGameState(gameStates);
				reDoStack.Clear();
				if (topCameraActive)
				{
					heldPlatform = Instantiate(platformPrefab, Camera.main.ScreenToWorldPoint(Input.mousePosition + Vector3.forward * zoom * 50), Quaternion.identity);
				}
				else
				{
					heldPlatform = Instantiate(platformPrefab, Camera.main.ScreenToWorldPoint(Input.mousePosition + Vector3.forward * zoom * 50), Quaternion.identity);
				}

				platforms.Add(heldPlatform);
				//Debug.Log(platforms.Count);
				platformCount--;
				platformText.text = "Platforms: " + platformCount;
				SoundManager.Instance.PlaySound(SoundManager.Sounds.PLACE_PLATFORM);
			}
		}
		else if (Input.GetButtonDown("Submit"))
		{
			gameManager.StartGame();
			SoundManager.Instance.PlaySound(SoundManager.Sounds.SELECT_PLAY);
		}
    }
	void SaveToFile()
	{
		JsonContainer gameState = new JsonContainer(GetGameState());
		string jsonData = JsonUtility.ToJson(gameState);
		File.WriteAllText(path, jsonData);
	}
	void LoadFromFile()
	{
		if (File.Exists(path))
		{
			string jsonData = File.ReadAllText(path);
			JsonContainer gameState = JsonUtility.FromJson<JsonContainer>(jsonData);
			if (gameState.coords.Length / 3 <= platformCount)
			{
				SaveGameState(gameStates);
				LoadGameState(gameState.coords);
			}
		}
	}
	private void MoveCamera()
	{
		if (Input.GetButtonDown("Switch"))
		{
			SwitchCamera();
		}
		zoom -= Input.GetAxis("Mouse ScrollWheel") * Time.deltaTime * 60;
		if (topCameraActive)
		{
			Camera.main.transform.position = new Vector3(Camera.main.transform.position.x, zoom * 50, Camera.main.transform.position.z);
		}
		else
		{
			Camera.main.transform.position = new Vector3(zoom * 50, Camera.main.transform.position.y, Camera.main.transform.position.z);
		}
		if (Input.GetButton("Fire2"))
		{
			if (topCameraActive)
			{
				Camera.main.transform.position += new Vector3(Input.GetAxis("Mouse Y"), 0, -Input.GetAxis("Mouse X")) *  2;
			}
			else 
			{
				Vector3 targetPos = Camera.main.transform.position + new Vector3(0, -Input.GetAxis("Mouse Y"), -Input.GetAxis("Mouse X"))  * 2;
				if (targetPos.y >= -19)
					Camera.main.transform.position = targetPos;
				else
					Camera.main.transform.position = Utilities.FlattenVectorY(Camera.main.transform.position) + Vector3.down * 19 + Vector3.back * Input.GetAxis("Mouse X") * 2;
			}
		}
		if (Input.GetButton("Jump") || Camera.main.transform.position.magnitude > 300)
		{
			zoom = 1;
			if (topCameraActive)
			{
				Camera.main.transform.position = new Vector3(0, 50, 0);
			}
			else
			{
				Camera.main.transform.position = new Vector3(50, 20, 0);
			}
		}
	}
	void SaveGameState(Stack<float[]> stack)
	{
		stack.Push(GetGameState());
	}
	void LoadGameState(float[] coords)
	{

		ClearPlatforms();
		for (int i = 0; i < coords.Length / 3; i++)
		{
			platforms.Add(Instantiate(platformPrefab, new Vector3(coords[i * 3], coords[i * 3 + 1], coords[i * 3 + 2]), Quaternion.identity));
			platformCount--;
		}
		platformText.text = "Platforms: " + platformCount;
	}
	void DeleteSaveFile()
	{
		if (File.Exists(path))
		{
			File.Delete(path);
		}
	}
	void ClearPlatforms()
	{
		//Debug.Log("Borrando");
		while (platforms.Count > 0)
		{
			GameObject plataforma = platforms[platforms.Count - 1];
			platforms.RemoveAt(platforms.Count - 1);
			Destroy(plataforma);
			platformCount++;
		}
		platformText.text = "Platforms: " + platformCount;
	}
	void Undo()
	{
		if (gameStates.Count > 0)
		{
			SaveGameState(reDoStack);
			LoadGameState(gameStates.Pop());
		}
	}
	void ReDo()
	{
		if (reDoStack.Count > 0)
		{
			SaveGameState(gameStates);
			LoadGameState(reDoStack.Pop());
		}
	}
	void SwitchCamera()
	{
		heldPlatform = null;
		topCamera.gameObject.SetActive(!topCamera.gameObject.activeInHierarchy);
		sideCamera.gameObject.SetActive(!sideCamera.gameObject.activeInHierarchy);
		topCameraActive = !topCameraActive;
	}
	float[] GetGameState()
	{
		float[] coords = new float[platforms.Count * 3];
		for (int i = 0; i < platforms.Count; i++)
		{
			//Debug.Log(coords[i * 3] + ", " + coords[i * 3 + 1] + ", " + coords[i * 3 + 2]);
			coords[i * 3] = platforms[i].transform.position.x;
			coords[i * 3 + 1] = platforms[i].transform.position.y;
			coords[i * 3 + 2] = platforms[i].transform.position.z;
		}
		return coords;
	}
	private void OnDisable()
	{
		if(platformText != null)
			platformText.gameObject.SetActive(false);
		if(topCamera != null)
			topCamera.gameObject.SetActive(false);
		if(sideCamera != null)
			sideCamera.gameObject.SetActive(false);
	}
	private void OnEnable()
	{
		topCamera.gameObject.SetActive(true);
		sideCamera.gameObject.SetActive(false);
		topCameraActive = true;
		platformText.gameObject.SetActive(true);
		platformText.text = "Platforms: " + platformCount;
	}
	public int GetUsedPlatforms()
	{
		return maxPlatforms - platformCount;
	}
}
