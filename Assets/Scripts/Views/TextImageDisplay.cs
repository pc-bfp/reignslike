using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class TextImage {
	public string Text { get; private set; }
	public Sprite Image { get; private set; }

	public TextImage(string text, Sprite image) {
		Text = text;
		Image = image;
	}
}


public class TextImageDisplay : MonoBehaviour {
	[SerializeField] TextMeshProUGUI displayText;
	[SerializeField] Image displayImage;
	[SerializeField] GameObject textHolder, imageHolder;
	[SerializeField] Button nextButton;
	[SerializeField] float nextTimeDelay = 1f;

	public delegate void NextEvent();
	public NextEvent OnNext;

	public float NextTimeDelay {
		get { return nextTimeDelay; }
	}
	public Animator Animator {
		get { return animator; }
	}

	Animator animator;
	bool isShowingText = false, isShowingImage = false;

	const string ANIM_BOOL_TEXT = "Text", ANIM_BOOL_IMAGE = "Image";

	private void Awake() {
		animator = GetComponent<Animator>();
		if (nextButton) nextButton.onClick.AddListener(NextPressed);
		if (displayText && !textHolder) textHolder = displayText.gameObject;
		if (displayImage && !imageHolder) imageHolder = displayImage.gameObject;
	}

	public void ShowText(string text) {
		if (isShowingText || !displayText) return;
		isShowingText = true;
		displayText.text = text;
		if (animator) animator.SetBool(ANIM_BOOL_TEXT, true);
	}

	public void HideText() {
		if (!isShowingText) return;
		isShowingText = false;
		if (animator) animator.SetBool(ANIM_BOOL_TEXT, false);
	}

	public void ShowImage(Sprite image = null) {
		if (isShowingImage || !displayImage) return;
		isShowingImage = true;
		displayImage.sprite = image;
		if (animator) animator.SetBool(ANIM_BOOL_IMAGE, true);
	}

	public void HideImage() {
		if (!isShowingImage) return;
		isShowingImage = false;
		if (animator) animator.SetBool(ANIM_BOOL_IMAGE, false);
	}

	void NextPressed() {
		if (!isShowingText && !isShowingImage) return;
		HideText();
		HideImage();
		OnNext?.Invoke();
	}
}
