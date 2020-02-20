using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SetupDisplay : MonoBehaviour {
	[SerializeField] Button buttonComplete, buttonReset;
	[SerializeField] SetupNumberDisplay turnsDisplay, startStatDisplay, minStatDisplay, maxStatDisplay;

	public static bool DoShowSetup { get; set; } = false;

	public delegate void SetupCompleteEvent();
	public SetupCompleteEvent OnCompleted;

	public int NumTurns { get { return turnsDisplay ? turnsDisplay.Value : 10; } }

	List<SetupNumberDisplay> numDisplays;

	private void Awake() {
		gameObject.SetActive(false);
		numDisplays = new List<SetupNumberDisplay>(new SetupNumberDisplay[] { turnsDisplay, startStatDisplay, minStatDisplay, maxStatDisplay });
	}

	public void Initialize(List<GameManager.StatHolder> statHolders) {
		if (buttonComplete) buttonComplete.onClick.AddListener(() => Complete());
		if (minStatDisplay) minStatDisplay.OnValueChanged += newValue => GameManager.Instance.MinStatValue = newValue;
		if (maxStatDisplay) maxStatDisplay.OnValueChanged += newValue => GameManager.Instance.MaxStatValue = newValue;

		numDisplays.ForEach(numDisplay => {
			numDisplay.Initialize();
			if (buttonReset) buttonReset.onClick.AddListener(() => numDisplay.ResetToDefault());
		});
		if (minStatDisplay) GameManager.Instance.MinStatValue = minStatDisplay.Value;
		if (maxStatDisplay) GameManager.Instance.MaxStatValue = maxStatDisplay.Value;

		statHolders.ForEach(sh => {
			sh.display.SetButtonVisibility(true);
			if (startStatDisplay) startStatDisplay.OnValueChanged += newValue => sh.display.SetValue(newValue);
			sh.Initialize(startStatDisplay.Value);
		});
		OnCompleted += () => statHolders.ForEach(sh => sh.display.SetButtonVisibility(false));

		if (DoShowSetup) gameObject.SetActive(true);
		else Complete();
	}

	void Complete() {
		gameObject.SetActive(false);
		OnCompleted?.Invoke();
	}
}