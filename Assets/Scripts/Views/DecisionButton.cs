using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class DecisionButton : MonoBehaviour {
	[SerializeField] TextMeshProUGUI buttonText;
	[SerializeField] Image buttonImage, fillImage;
	[SerializeField] bool reverseFill;
	[SerializeField] AudioClip sfxPress;

	public delegate void ButtonPressEvent(int buttonIndex);
	public ButtonPressEvent OnButtonPressed;

	public int ButtonIndex { get; set; }
	public float HoldTime { get; set; }

	public string ButtonText {
		get { return buttonText ? buttonText.text : string.Empty; }
	}

	Vector3 origScale = Vector3.zero;
	bool isButtonHeld = false;
	Tween holdTween = null;

	private void Awake() {
		origScale = transform.localScale;
		OnButtonPressed += index => {
			AudioManager.PlayOneShot(sfxPress);
			holdTween = null;
		};
	}

	public void SetButtonText(string text) {
		gameObject.SetActive(!string.IsNullOrEmpty(text));
		if (gameObject.activeSelf) {
			buttonText.text = text;
			if (origScale != Vector3.zero) transform.localScale = origScale;
		}
	}

	public void ButtonPressed() {
		if (HoldTime > 0) return;
		OnButtonPressed?.Invoke(ButtonIndex);
	}

	public void SetButtonHeld(bool isHeld) {
		if (HoldTime <= 0 || isHeld == isButtonHeld) return;
		isButtonHeld = isHeld;
		if (holdTween != null) holdTween.Kill(false);
		if (isButtonHeld) {
			if (fillImage) {
				if (reverseFill) fillImage.fillAmount = 1f;
				holdTween = fillImage.DOFillAmount(reverseFill ? 0 : 1, HoldTime).SetEase(Ease.Linear);
			}
			else holdTween = DOTween.To(() => 0, x => { }, 1f, HoldTime);
			holdTween.onComplete += () => {
				OnButtonPressed?.Invoke(ButtonIndex);
				if (fillImage) fillImage.fillAmount = 0;
			};
			holdTween.onKill += () => {
				if (fillImage) fillImage.fillAmount = 0;
			};
		}
	}

	const float SHRINK_TIME = 0.33f;

	public void ButtonShrink() {
		if (origScale == Vector3.zero) origScale = transform.localScale;
		transform.DOScale(0, SHRINK_TIME).SetEase(Ease.Linear);
	}
}