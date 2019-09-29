using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;


public class SoundManager {
	class SoundHolder : MonoBehaviour {
		public AudioSource sfxSource, bgmSource;

		void Awake() {
			sfxSource = gameObject.AddComponent<AudioSource>();
			bgmSource = gameObject.AddComponent<AudioSource>();

			sfxSource.loop = false;
			bgmSource.loop = true;
			sfxSource.playOnAwake = bgmSource.playOnAwake = false;

			AudioInfoHolder info = Resources.FindObjectsOfTypeAll<AudioInfoHolder>()[0];
			sfxSource.outputAudioMixerGroup = info.sfxGroup;
			bgmSource.outputAudioMixerGroup = info.bgmGroup;
		}
	}

	static SoundHolder Instance {
		get {
			if (!_instance) Object.DontDestroyOnLoad(_instance = new GameObject("Sound holder").AddComponent<SoundHolder>());
			return _instance;
		}
	}
	private static SoundHolder _instance = null;

	public static void PlayBGM(AudioClip bgmClip) {
		if (!bgmClip || Instance.bgmSource.clip == bgmClip) return;
		Instance.bgmSource.Stop();
		Instance.bgmSource.clip = bgmClip;
		Instance.bgmSource.Play();
	}

	public static void PauseBGM(bool doPause) {
		if (doPause) Instance.bgmSource.Pause();
		else Instance.bgmSource.UnPause();
	}

	public static void PlaySFX(AudioClip sfxClip, float minVolume = 1f, float maxVolume = 1f) {
		if (!sfxClip) return;
		Instance.sfxSource.PlayOneShot(sfxClip, Random.Range(minVolume, maxVolume));
	}

	public static void PlaySFX(List<AudioClip> sfxClips, float minVolume = 1f, float maxVolume = 1f) {
		if (sfxClips == null || sfxClips.Count == 0) return;
		PlaySFX(sfxClips[Random.Range(0, sfxClips.Count)], minVolume, maxVolume);
	}
}


[CreateAssetMenu(fileName = "AudioInfo", menuName = "Custom/Audio info holder")]
public class AudioInfoHolder : ScriptableObject {
	public AudioMixerGroup sfxGroup, bgmGroup;
}