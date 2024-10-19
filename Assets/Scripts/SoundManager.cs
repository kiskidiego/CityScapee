using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
	public enum Sounds
	{
		ENEMY_SHOT,
		ENEMY_SOUND_1,
		ENEMY_SOUND_2,
		ENEMY_SOUND_3,
		GOAL_GET,
		MENU_NAVIGATION,
		PLACE_PLATFORM,
		REMOVE_PLATFORM,
		SELECT_PLAY,
		SWING_1,
		SWING_2,
		LOSE_LIFE,
		STEP_1,
		STEP_2,
		STEP_3,
		STEP_4
	}
	public enum Tracks
	{
		GAME_THEME,
		MENU_THEME,
		VICTORY_THEME,
		MAP_EDITOR_THEME
	}
    public static SoundManager Instance{get; private set;}
	public float TrackVolume
	{
		get { return trackVolume; }
		set
		{
			trackVolume = value;
			if(trackSource != null)
				trackSource.volume = trackVolume;
		}
	}
	float trackVolume = 1;
	public float SFXVolume
	{
		get { return sfxVolume; }
		set { sfxVolume = value;	}
	}
	float sfxVolume = 1;
    [SerializeField] AudioClip[] tracks;
	[SerializeField] AudioClip[] sounds;
    AudioSource trackSource;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        if(Instance == null)
        {
			Instance = this;
		}
		else
        {
			Destroy(gameObject);
		}
        trackSource = gameObject.AddComponent<AudioSource>();
		trackSource.loop = true;
		trackSource.volume = trackVolume;
    }
	private void Update()
	{
		transform.position = Camera.main.transform.position;
	}
	public void StartTrack(Tracks track)
	{
		if(trackSource.clip == tracks[(int)track] && trackSource.isPlaying)
			return;
		trackSource.clip = tracks[(int)track];
		trackSource.Play();
	}
	public void StopTrack()
	{
		trackSource.Stop();
	}
	public void PlaySound(Sounds sound, Vector3 soundPosition)
	{
		AudioSource source = new GameObject().AddComponent<AudioSource>();
		source.transform.position = soundPosition;
		PlaySound(sound, source);
    }
	public void PlaySound(Sounds sound, Transform soundTransform)
	{
		AudioSource source = new GameObject().AddComponent<AudioSource>();
		source.transform.parent = soundTransform;
		source.transform.localPosition = Vector3.zero;
		PlaySound(sound, source);
	}
	public void PlaySound(Sounds sound)
	{
		AudioSource source = new GameObject().AddComponent<AudioSource>();
		source.transform.parent = transform;
		PlaySound(sound, source);
	}
	void PlaySound(Sounds sound, AudioSource source)
	{
		source.clip = sounds[(int)sound];
		source.volume = sfxVolume;
		source.loop = false;
		source.rolloffMode = AudioRolloffMode.Linear;
		source.maxDistance = 30;
		source.spatialBlend = 1;
		source.Play();
		StartCoroutine(DeleteAudioSource(source));
	}
	IEnumerator DeleteAudioSource(AudioSource source)
	{
		yield return new WaitForSeconds(source.clip.length);
		if(source != null)
			Destroy(source.gameObject);
	}
}
