using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class TurnsDisplay : MonoBehaviour {
	[SerializeField] TextMeshProUGUI turnsText;
	[SerializeField] Image turnMeter;
	[SerializeField] float turnsDisplayTime, endDisplayTime;
	[SerializeField] AudioClip sfxInitialize, sfxTurn;
	[SerializeField] List<AudioClip> completeSFX;
	[SerializeField] ParticleSystem completePFX;

	public int NumTurns { get; private set; }
	public float AnimDelayTime {
		get { return NumTurns > 0 ? turnsDisplayTime : endDisplayTime; }
	}

	Animator animator;
	bool doChangeText = false;
	int totalTurns;

	const string PARAM_TRIGGER_SHOW = "Show", PARAM_TRIGGER_REDUCE = "Turn", PARAM_TRIGGER_END = "End";

	private void Awake() {
		animator = GetComponent<Animator>();
		enabled = false;
	}

	public void Initialize(int numTurns) {
		NumTurns = totalTurns = numTurns;
		UpdateText();
		if (turnMeter) turnMeter.fillAmount = 0;
		ShowAnim(PARAM_TRIGGER_SHOW);
		UpdateTurnMeter(turnsDisplayTime / 2f);
		AudioManager.PlayOneShot(sfxInitialize);
	}

	public void ReduceTurns(int reduceBy = 1) {
		NumTurns -= reduceBy;
		if (ShowAnim(NumTurns > 0 ? PARAM_TRIGGER_REDUCE : PARAM_TRIGGER_END)) doChangeText = true;
		else UpdateText();
	}

	public void AnimChangeText() {
		if (!doChangeText) return;
		UpdateText();
		UpdateTurnMeter(0.25f);
		doChangeText = false;
		AudioManager.PlayOneShot(sfxTurn);
	}

	public void AnimEnd() {
		AudioManager.PlayOneShot(completeSFX, true);
		if (completePFX) completePFX.Play();
	}

	void UpdateText() {
		if (turnsText) turnsText.text = NumTurns.ToString();
	}

	void UpdateTurnMeter(float updateTime) {
		if (!turnMeter) return;
		turnMeter.DOFillAmount((float)NumTurns / totalTurns, updateTime).SetEase(Ease.InOutQuad);
	}

	bool ShowAnim(string trigger) {
		if (animator) animator.SetTrigger(trigger);
		return animator;
	}
}