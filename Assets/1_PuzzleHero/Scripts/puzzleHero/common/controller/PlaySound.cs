using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class PlaySound : MonoBehaviour {

	private AudioSource[] audio;
	private int index = 0;
	private const int NUMB_AUDIO = 5;

	void Awake() 
	{
		audio = gameObject.GetComponents<AudioSource>();
		DontDestroyOnLoad (gameObject);
	}

	public void RunSound(AudioClip audioClip, float volume)
	{
		if(audioClip != null)
		{
			audio[index].clip = audioClip;
			audio[index].PlayOneShot(audioClip, volume);
			index++;
			if(index == NUMB_AUDIO) index = 0;
		}
	}

	public void PauseSound()
	{
		for(int i = 0; i < NUMB_AUDIO; i++)
		{
			audio[i].clip = null;
			audio[i].Pause ();
		}
	}

	public void SetVolume (float volume)
	{
		audio [NUMB_AUDIO].volume = volume;
	}

	public void PlayMusic(AudioClip audioClip, bool loop)
	{
		audio[NUMB_AUDIO].clip = audioClip;
		audio[NUMB_AUDIO].Play();
		audio [NUMB_AUDIO].loop = loop;
	}

	public void PauseMusic()
	{
		audio [NUMB_AUDIO].Pause ();
	}
}
