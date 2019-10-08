using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class TurnsDisplay : MonoBehaviour {
	[SerializeField] TextMeshProUGUI turnsText;
	[SerializeField] Image turnMeter;
	[SerializeField] float turnsDisplayTime;
	[SerializeField] AudioClip sfxInitialize, sfxTurn;

	public int NumTurns { get; private set; }
	public float AnimTime {
		get { return turnsDisplayTime; }
	}

	Animator animator;
	bool doChangeText = false;
	int totalTurns;

	const string PARAM_TRIGGER_SHOW = "Show", PARAM_TRIGGER_REDUCE = "Turn";

	private void Awake() {
		animator = GetComponent<Animator>();
		enabled = false;
	}

	public void Initialize(int numTurns) {
		NumTurns = totalTurns = numTurns;
		UpdateText();
		if (turnMeter) turnMeter.fillAmount = 0;
		ShowAnim(false);
		UpdateTurnMeter(turnsDisplayTime / 2f);
		AudioManager.PlayOneShot(sfxInitialize);
	}

	public void ReduceTurns(int reduceBy = 1) {
		NumTurns -= reduceBy;
		if (ShowAnim()) doChangeText = true;
		else UpdateText();
	}

	public void AnimChangeText() {
		if (!doChangeText) return;
		UpdateText();
		UpdateTurnMeter(0.25f);
		doChangeText = false;
		AudioManager.PlayOneShot(sfxTurn);
	}

	void UpdateText() {
		if (turnsText) turnsText.text = NumTurns.ToString();
	}

	void UpdateTurnMeter(float updateTime) {
		if (!turnMeter) return;
		turnMeter.DOFillAmount((float)NumTurns / totalTurns, updateTime).SetEase(Ease.InOutQuad);
	}

	bool ShowAnim(bool doReduce = true) {
		if (animator) animator.SetTrigger(doReduce ? PARAM_TRIGGER_REDUCE : PARAM_TRIGGER_SHOW);
		return animator;
	}
}