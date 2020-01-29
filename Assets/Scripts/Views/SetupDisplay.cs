using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SetupDisplay : MonoBehaviour {
	[SerializeField] Button buttonComplete, buttonReset;
	[SerializeField] SetupNumberDisplay turnsDisplay, startStatDisplay, minStatDisplay, maxStatDisplay;

	public delegate void SetupCompleteEvent();
	public SetupCompleteEvent OnCompleted;

	public int NumTurns { get { return turnsDisplay ? turnsDisplay.Value : 10; } }

	private void Awake() {
		gameObject.SetActive(false);
		if (buttonComplete) buttonComplete.onClick.AddListener(() => Complete());
		if (minStatDisplay) minStatDisplay.OnValueChanged += newValue => GameManager.Instance.MinStatValue = newValue;
		if (maxStatDisplay) maxStatDisplay.OnValueChanged += newValue => GameManager.Instance.MaxStatValue = newValue;
		if (buttonReset) buttonReset.onClick.AddListener(() => {
			foreach (var numDisplay in new SetupNumberDisplay[] { turnsDisplay, startStatDisplay, minStatDisplay, maxStatDisplay }) {
				if (numDisplay) numDisplay.ResetToDefault();
			}
		});
	}

	public void Activate(List<GameManager.StatHolder> statHolders) {
		gameObject.SetActive(true);
		if (minStatDisplay) GameManager.Instance.MinStatValue = minStatDisplay.Value;
		if (maxStatDisplay) GameManager.Instance.MaxStatValue = maxStatDisplay.Value;
		statHolders.ForEach(sh => {
			sh.Initialize(startStatDisplay.Value);
			sh.display.SetButtonVisibility(true);
			if (startStatDisplay) startStatDisplay.OnValueChanged += newValue => sh.display.SetValue(newValue);
		});
		OnCompleted += () => statHolders.ForEach(sh => sh.display.SetButtonVisibility(false));
	}

	void Complete() {
		gameObject.SetActive(false);
		OnCompleted?.Invoke();
	}
}