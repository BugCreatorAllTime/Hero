using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

public class SoundManager
{
	public const string dungeonBgmPath = "Sound/MusicDungeon";

	[Inject]
	public ConfigManager config { get; set;}

	[Inject]
	public AssetMgr mgr{get; set;}

	private PlaySound player;
	public static SoundManager intance;
		
	[PostConstruct]
	public void PostConstruct()
	{
		player = GameObject.Find("PlaySound").GetComponent<PlaySound>();
		intance = this;
		SetVolumeMusic ((float)config.UserData.musicValue);
	}

	public void PlaySound(int idSound)
	{
		if(config.UserData.soundValue > 0)
		{
			if(config.soundCfg.sound.ContainsKey(idSound.ToString()))
			{
				SoundData soundData = config.soundCfg.sound[idSound.ToString()];
				mgr.GetAsset<AudioClip>("Sound/"+soundData.SoundName, PlaySoundLoad);
			}
		}
	}

	private void PlaySoundLoad(AudioClip audioClip)
	{
		player.RunSound(audioClip, (float)config.UserData.soundValue);
	}

	public void SetVolumeMusic(float volume)
	{
		player.SetVolume (volume);
	}

	public void PlayMusic(int idMusic)
	{
		if(config.soundCfg.sound.ContainsKey(idMusic.ToString()))
		{
			SoundData soundData = config.soundCfg.sound[idMusic.ToString()];
			mgr.GetAsset<AudioClip>("Sound/"+soundData.SoundName, delegate(AudioClip audioClip) {
				bool loop;
				if(idMusic == SoundName.MUSIC_DUNGEON || idMusic == SoundName.MUSIC_HOMETOWN)
					loop = true;
				else loop = false;
				player.PlayMusic(audioClip, loop);
			});
		}
	}

	public void PauseMusic()
	{
		player.PauseMusic ();
	}

	public void PauseSound()
	{
		player.PauseSound();
	}
}
