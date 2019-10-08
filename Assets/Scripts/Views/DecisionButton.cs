using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class DecisionButton : MonoBehaviour {
	[SerializeField] TextMeshProUGUI buttonText;
	[SerializeField] AudioClip sfxPress;

	public delegate void ButtonPressEvent(int buttonIndex);
	public ButtonPressEvent OnButtonPressed;

	public int ButtonIndex { get; set; }

	public string ButtonText {
		get { return buttonText ? buttonText.text : string.Empty; }
	}

	public void SetButtonText(string text) {
		gameObject.SetActive(!string.IsNullOrEmpty(text));
		if (gameObject.activeSelf) {
			buttonText.text = text;
			transform.localScale = Vector3.one;
		}
	}

	public void ButtonPressed() {
		OnButtonPressed?.Invoke(ButtonIndex);
		SoundManager.PlaySFX(sfxPress);
	}

	const float SHRINK_TIME = 0.33f;

	public void ButtonShrink() {
		transform.DOScale(0, SHRINK_TIME).SetEase(Ease.Linear);
	}
}