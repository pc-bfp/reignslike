using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class VideoView : MonoBehaviour {
	[SerializeField] VideoPlayer player;
	[SerializeField] bool playOnce = true;
	[SerializeField] string activeBoolParam = "Active", showSkipBoolParam = "Skip";
	[SerializeField] float postCompletionDelay, skipHoldTime = 2f, skipButtonAutohideTime = 1.5f;
	[SerializeField] Image skipProgress;

	public delegate void CompleteEvent();
	public CompleteEvent OnCompleted;

	Animator animator;
	Tween skipTween;

	private void Awake() {
		animator = GetComponent<Animator>();
		if (player) player.loopPointReached += vp => OnPlaybackComplete();
		enabled = false;
	}

	public void Activate(VideoClip clip) {
		if (player) {
			player.clip = clip;
			player.Prepare();
		}
		if (animator) animator.SetBool(activeBoolParam, true);
		else AnimEventPlayVideo();
		enabled = true;
	}

	public void AnimEventPlayVideo() {
		if (!player) return;
		AudioManager.PauseBGM(true);
		player.Play();
	}

	void OnPlaybackComplete() {
		if (playOnce) Deactivate();
	}

	public void Deactivate() {
		if (player) player.Stop();
		AudioManager.PauseBGM(false);
		if (animator) {
			animator.SetBool(activeBoolParam, false);
			animator.SetBool(showSkipBoolParam, false);
		}
		DOTween.Sequence().AppendInterval(postCompletionDelay).AppendCallback(() => OnCompleted?.Invoke());
		enabled = false;
	}

	public void SkipStart() {
		SkipCancel();
		skipProgress.fillAmount = 0;
		(skipTween = skipProgress.DOFillAmount(1f, skipHoldTime))
			.SetEase(Ease.OutQuad)
			.OnComplete(() => Deactivate())
			.OnKill(() => skipProgress.fillAmount = 0);
		skipPressedThisTouch = true;
	}

	public void SkipCancel() {
		if (skipTween != null && skipTween.IsPlaying()) skipTween.Kill(false);
	}

	float timeSkipShown = -1;
	bool skipPressedThisTouch = false;

	private void Update() {
		if (Input.GetMouseButtonUp(0)) {
			if (timeSkipShown <= 0) {
				animator.SetBool(showSkipBoolParam, true);
				timeSkipShown = 0;
			}
			else {
				if (!skipPressedThisTouch) timeSkipShown = skipButtonAutohideTime;
				else skipPressedThisTouch = false;
			}
		}
		if (timeSkipShown >= 0) {
			if (skipTween != null && skipTween.IsPlaying()) timeSkipShown = 0;
			else timeSkipShown += Time.deltaTime;
			if (timeSkipShown >= skipButtonAutohideTime) {
				animator.SetBool(showSkipBoolParam, false);
				timeSkipShown = -1;
			}
		}
	}
}