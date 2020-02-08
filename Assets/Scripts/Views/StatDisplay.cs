using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class StatDisplay : MonoBehaviour {
	[SerializeField] TextMeshProUGUI statLabel, statValueText, updateText;
	[SerializeField] string statUpColor = "\"blue\"", statDownColor = "\"red\"";
	[SerializeField] Image statUpIcon, statDownIcon;
	[SerializeField] Gradient valueGradient;
	[SerializeField] Image applyValueColor, applyValueFill;
	[SerializeField] Vector2 fillLimits = new Vector2(0.1f, 0.9f);
	[SerializeField] Transform statIcon;
	[SerializeField] AnimationCurve iconScale;
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
	}

	const string UPDATE_PARAM_BOOL = "Updating";

	public void SetValue(int newValue, int updateValue = 0) {
		StatValue = Mathf.Clamp(newValue, GameManager.Instance.MinStatValue, GameManager.Instance.MaxStatValue);
		if (statValueText) statValueText.text = newValue.ToString();
		float valueFraction = newValue / (float)GameManager.Instance.MaxStatValue;
		if (applyValueColor) applyValueColor.color = valueGradient.Evaluate(valueFraction);
		if (applyValueFill) applyValueFill.DOFillAmount(Mathf.Lerp(fillLimits.x, fillLimits.y, valueFraction), 0.25f).SetEase(Ease.OutQuad);
		if (statIcon) statIcon.DOScale(iconScale.Evaluate(valueFraction), 0.33f).SetEase(Ease.OutBack);
		if (updateValue != 0) {
			bool goingUp = updateValue > 0;
			if (updateText) updateText.text = string.Format("<color={0}>{1}</color>", goingUp ? statUpColor : statDownColor, updateValue.ToString("+0;-#"));
			if (statUpIcon) statUpIcon.gameObject.SetActive(goingUp);
			if (statDownIcon) statDownIcon.gameObject.SetActive(!goingUp);
			if (animator) animator.SetBool(UPDATE_PARAM_BOOL, true);
			AudioManager.PlayOneShot(goingUp ? sfxUp : sfxDown);
		}
		else {
			if (statUpIcon) statUpIcon.gameObject.SetActive(false);
			if (statDownIcon) statDownIcon.gameObject.SetActive(false);
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