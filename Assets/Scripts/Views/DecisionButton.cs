using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DecisionButton : MonoBehaviour {
	[SerializeField] TextMeshProUGUI buttonText;

	public delegate void ButtonPressEvent(int buttonIndex);
	public ButtonPressEvent OnButtonPressed;

	public int ButtonIndex { get; set; }

	public string ButtonText {
		get { return buttonText ? buttonText.text : string.Empty; }
	}

	public void SetButtonText(string text) {
		gameObject.SetActive(!string.IsNullOrEmpty(text));
		if (gameObject.activeSelf) buttonText.text = text;
	}

	public void ButtonPressed() {
		OnButtonPressed?.Invoke(ButtonIndex);
	}
}