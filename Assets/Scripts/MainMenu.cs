using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField] GameObject ayuda;
    [SerializeField] GameObject puntuaciones;
    [SerializeField] Text textoPuntuaciones;
	[SerializeField] GameObject opciones;
	[SerializeField] Slider sfxSlider;
	[SerializeField] Slider musicSlider;
	private void Start()
	{
		SoundManager.Instance.StartTrack(SoundManager.Tracks.MENU_THEME);		
		sfxSlider.value = SoundManager.Instance.SFXVolume;
		musicSlider.value = SoundManager.Instance.TrackVolume;
		textoPuntuaciones.text = File.ReadAllText(Application.dataPath + "/Resources/ScoreData.txt");
		StartCoroutine(laPeorCorrutinaDelMundo());
	}
	public void Jugar()
    {
        SoundManager.Instance.PlaySound(SoundManager.Sounds.SELECT_PLAY);
        SceneManager.LoadScene("GameScene");
    }
	public void Ayuda()
    {
        SoundManager.Instance.PlaySound(SoundManager.Sounds.MENU_NAVIGATION);
        ayuda.SetActive(true);
    }
	public void CerrarAyuda()
    {
		SoundManager.Instance.PlaySound(SoundManager.Sounds.MENU_NAVIGATION);
		ayuda.SetActive(false);
    }
    public void Puntuaciones()
    {
		SoundManager.Instance.PlaySound(SoundManager.Sounds.MENU_NAVIGATION);
		puntuaciones.SetActive(true);
    }
    public void CerrarPuntuaciones()
    {
		SoundManager.Instance.PlaySound(SoundManager.Sounds.MENU_NAVIGATION);
		puntuaciones.SetActive(false);
	}
    public void Opciones()
    {
		SoundManager.Instance.PlaySound(SoundManager.Sounds.MENU_NAVIGATION);
		opciones.SetActive(true);
	}
	public void CerrarOpciones()
    {
		SoundManager.Instance.PlaySound(SoundManager.Sounds.MENU_NAVIGATION);
		opciones.SetActive(false);
	}
	public void CambiarVolumenMusica()
	{
		SoundManager.Instance.TrackVolume = musicSlider.value;
	}
	public void CambiarVolumenSFX()
	{
		SoundManager.Instance.SFXVolume = sfxSlider.value;
	}
	private void OnDisable()
	{
		StopAllCoroutines();
	}
	IEnumerator laPeorCorrutinaDelMundo()	//Si no hago esto, unity no se da cuenta de que el archivo txt ha cambiado :)
	{
		while (true)
		{
			yield return new WaitForSeconds(0.5f);
			textoPuntuaciones.text = File.ReadAllText(Application.dataPath + "/Resources/ScoreData.txt");
		}
	}
	public void Salir()
    {
		Application.Quit();
	}
}
