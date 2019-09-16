using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StatDisplay : MonoBehaviour {
	[SerializeField] TextMeshProUGUI statLabel;
	[SerializeField] string statValueColor = "\"white\"";
	[SerializeField] Image textBG;
	[SerializeField] Gradient textBGGradient;
	[SerializeField] int gradientRange = 10;

	public string StatName { get; set; }

	public void SetValue(int newValue) {
		statLabel.text = string.Format("{0} : <b><size=125%><color={1}>{2}</color></size></b>", StatName, statValueColor, newValue);
		if (textBG) textBG.color = textBGGradient.Evaluate(newValue / (float)gradientRange);
	}

	private void Update() {
		for (int i = 48; i < 58; i++) {
			if (Input.GetKeyDown((KeyCode)i)) SetValue(i - 48);
		}
	}
}