using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StatDisplay : MonoBehaviour {
	[SerializeField] TextMeshProUGUI statLabel, statValue, updateText;
	[SerializeField] string statUpColor = "\"blue\"", statDownColor = "\"red\"";
	[SerializeField] Gradient valueGradient;
	[SerializeField] int gradientRange = 10;
	[SerializeField] Image applyValueColor;

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
	}

	const string UPDATE_PARAM_BOOL = "Updating";

	public void SetValue(int newValue, int updateValue = 0) {
		if (statValue) statValue.text = newValue.ToString();
		if (applyValueColor) applyValueColor.color = valueGradient.Evaluate(newValue / (float)gradientRange);
		if (updateValue != 0) {
			if (updateText) updateText.text = string.Format("<color={0}>{1}</color>", updateValue > 0 ? statUpColor : statDownColor, updateValue.ToString("+0;-#"));
			if (animator) animator.SetBool(UPDATE_PARAM_BOOL, true);
		}
	}

	public void EndSetValue() {
		if (animator) animator.SetBool(UPDATE_PARAM_BOOL, false);
	}

	//void Update() {
	//	for (int i = 48; i < 58; i++) {
	//		if (Input.GetKeyDown((KeyCode)i)) SetValue(i - 48);
	//	}
	//}
}