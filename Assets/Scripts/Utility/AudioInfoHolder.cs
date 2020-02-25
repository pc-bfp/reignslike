using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;


public class AudioManager {
	static AudioSource sfxSource, sfxPrioritySource, bgmSource;
	static RLPool<AudioSource> sfxPool;
	static AudioInfoHolder audioInfo;

	const int SFX_POOL_SIZE = 5;

	class AudioHolder : MonoBehaviour {
		void Awake() {
			sfxSource = gameObject.AddComponent<AudioSource>();
			sfxPrioritySource = gameObject.AddComponent<AudioSource>();
			bgmSource = gameObject.AddComponent<AudioSource>();

			sfxSource.loop = sfxPrioritySource.loop = false;
			bgmSource.loop = true;
			sfxSource.playOnAwake = sfxPrioritySource.playOnAwake = bgmSource.playOnAwake = false;

			List<AudioSource> allSFX = new List<AudioSource>();
			for (int i = 0; i < SFX_POOL_SIZE; i++) {
				AudioSource curSFX = gameObject.AddComponent<AudioSource>();
				curSFX.loop = curSFX.playOnAwake = false;
				allSFX.Add(curSFX);
			}
			sfxPool = new RLPool<AudioSource>(allSFX);
			sfxPool.OnReturned += sfx => sfx.Stop();

			if (audioInfo != null) {
				sfxSource.outputAudioMixerGroup = audioInfo.sfxGroup;
				sfxPrioritySource.outputAudioMixerGroup = audioInfo.sfxPriorityGroup;
				bgmSource.outputAudioMixerGroup = audioInfo.bgmGroup;
				allSFX.ForEach(sfx => sfx.outputAudioMixerGroup = audioInfo.sfxGroup);
			}
		}
	}

	static AudioHolder Instance;

	public static void Initialize(AudioInfoHolder info) {
		if (!Instance) {
			audioInfo = info;
			Object.DontDestroyOnLoad(Instance = new GameObject("Audio holder").AddComponent<AudioHolder>());
		}
	}

	public static void PlayBGM(AudioClip bgmClip) {
		if (!bgmClip || (bgmSource.isPlaying && bgmSource.clip == bgmClip)) return;
		bgmSource.Stop();
		bgmSource.clip = bgmClip;
		bgmSource.Play();
		bgmSource.UnPause();
	}

	public static void PauseBGM(bool doPause) {
		if (!bgmSource.clip) return;
		if (doPause) bgmSource.Pause();
		else bgmSource.UnPause();
	}

	public static void PlayOneShot(AudioClip sfxClip, bool isPriority = false, float minVolume = 1f, float maxVolume = 1f) {
		if (!sfxClip) return;
		(isPriority ? sfxPrioritySource : sfxSource).PlayOneShot(sfxClip, Random.Range(minVolume, maxVolume));
	}

	public static void PlayOneShot(List<AudioClip> sfxClips, bool isPriority = false, float minVolume = 1f, float maxVolume = 1f) {
		if (sfxClips == null || sfxClips.Count == 0) return;
		PlayOneShot(sfxClips[Random.Range(0, sfxClips.Count)], isPriority, minVolume, maxVolume);
	}

	public static AudioSource PlaySFX(SFXType sfx, bool startPlaying = true) {
		AudioSource retval = sfxPool.Get();
		retval.clip = audioInfo.GetClip(sfx);
		if (startPlaying) retval.Play();
		return retval;
	}
}


public enum SFXType { SCRIBBLE };

[System.Serializable]
public class AudioInfoHolder {
	public AudioMixerGroup sfxGroup, sfxPriorityGroup, bgmGroup;
	[SerializeField] List<SFXMapper> sfxList;

	[System.Serializable]
	class SFXMapper {
		public SFXType type;
		public List<AudioClip> clips;
	}

	Dictionary<SFXType, List<AudioClip>> sfxMap;

	public AudioClip GetClip(SFXType type) {
		if (sfxMap == null) {
			sfxMap = new Dictionary<SFXType, List<AudioClip>>();
			foreach (SFXMapper map in sfxList) {
				if (!sfxMap.ContainsKey(map.type)) sfxMap[map.type] = new List<AudioClip>();
				sfxMap[map.type].AddRange(map.clips);
			}
		}
		if (!sfxMap.ContainsKey(type)) return null;
		var clips = sfxMap[type];
		return clips.Count > 0 ? clips[Random.Range(0, clips.Count)] : null;
	}
}