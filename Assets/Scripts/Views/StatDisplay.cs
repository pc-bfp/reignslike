using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class StatDisplay : MonoBehaviour {
	[SerializeField] TextMeshProUGUI statLabel, statValueText, updateText;
	[SerializeField] string statUpColor = "\"blue\"", statDownColor = "\"red\"";
	[SerializeField] Gradient valueGradient;
	[SerializeField] Image applyValueColor, progressBar;
	[SerializeField] float progressMin = 0.25f, progressMax = 0.9f;
	[SerializeField] AudioClip sfxUp, sfxDown;
	[SerializeField] GameObject setupButtonHolder;
	[SerializeField] Button upButton, downButton;

	public int StatValue { get; private set; }

	Animator animator;

	public string StatName {
		get { return _statName; }
		set {
			if (value == _statName) return;
			_statName = value;
			if (statLabel) statLabel.text = value;
		}
	}
	string _statName;

	void Awake() {
		animator = GetComponent<Animator>();
		if (upButton) upButton.onClick.AddListener(() => IncrementValue(true));
		if (downButton) downButton.onClick.AddListener(() => IncrementValue(false));
		if (progressBar) progressBar.fillAmount = 0;
	}

	const string UPDATE_PARAM_BOOL = "Updating";

	public void SetValue(int newValue, int updateValue = 0) {
		StatValue = Mathf.Clamp(newValue, GameManager.Instance.MinStatValue, GameManager.Instance.MaxStatValue);
		float statPercent = Mathf.InverseLerp(GameManager.Instance.MinStatValue, GameManager.Instance.MaxStatValue, newValue);
		if (statValueText) statValueText.text = newValue.ToString();
		if (applyValueColor) applyValueColor.color = valueGradient.Evaluate(statPercent);
		if (progressBar) {
			float progress = Mathf.Lerp(progressMin, progressMax, Mathf.InverseLerp(GameManager.Instance.MinStatValue + 1, GameManager.Instance.MaxStatValue - 1, newValue));
			if (newValue == GameManager.Instance.MaxStatValue) progress = 1;
			else if (newValue == GameManager.Instance.MinStatValue) progress = 0;
			progressBar.DOFillAmount(progress, Mathf.Lerp(0.33f, 0.66f, Mathf.Abs(progressBar.fillAmount - progress))).SetEase(Ease.OutBack);
		}
		if (updateValue != 0) {
			bool goingUp = updateValue > 0;
			if (updateText) updateText.text = string.Format("<color={0}>{1}</color>", goingUp ? statUpColor : statDownColor, updateValue.ToString("+0;-#"));
			if (animator) animator.SetBool(UPDATE_PARAM_BOOL, true);
			AudioManager.PlayOneShot(goingUp ? sfxUp : sfxDown);
		}
	}

	public void EndSetValue() {
		if (animator) animator.SetBool(UPDATE_PARAM_BOOL, false);
	}

	public void SetButtonVisibility(bool isVisible) {
		if (setupButtonHolder) setupButtonHolder.SetActive(isVisible);
	}

	void IncrementValue(bool up) {
		int newValue = Mathf.Clamp(StatValue + (up ? 1 : -1), GameManager.Instance.MinStatValue, GameManager.Instance.MaxStatValue);
		if (newValue != StatValue) SetValue(newValue);
	}

	//void Update() {
	//	for (int i = 48; i < 58; i++) {
	//		if (Input.GetKeyDown((KeyCode)i)) SetValue(i - 48);
	//	}
	//}
}