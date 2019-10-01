using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationHelper : MonoBehaviour {
	[SerializeField] List<AudioClip> randomClips;
	[SerializeField] float minVolume = 1, maxVolume = 1;

	public void PlaySound(AudioClip clip) {
		SoundManager.PlaySFX(clip, minVolume, maxVolume);
	}

	public void PlayRandomSound() {
		SoundManager.PlaySFX(randomClips, minVolume, maxVolume);
	}
}