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

	Vector3 origScale = Vector3.zero;

	private void Awake() {
		origScale = transform.localScale;
	}

	public void SetButtonText(string text) {
		gameObject.SetActive(!string.IsNullOrEmpty(text));
		if (gameObject.activeSelf) {
			buttonText.text = text;
			if (origScale != Vector3.zero) transform.localScale = origScale;
		}
	}

	public void ButtonPressed() {
		OnButtonPressed?.Invoke(ButtonIndex);
		AudioManager.PlayOneShot(sfxPress);
	}

	const float SHRINK_TIME = 0.33f;

	public void ButtonShrink() {
		if (origScale == Vector3.zero) origScale = transform.localScale;
		transform.DOScale(0, SHRINK_TIME).SetEase(Ease.Linear);
	}
}