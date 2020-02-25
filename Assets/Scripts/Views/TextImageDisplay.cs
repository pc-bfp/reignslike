using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class TextImage {
	[Multiline] public string text;
	[Multiline] public List<string> extraText;
	public Sprite image;
	public List<Sprite> extraImages;

	public TextImage(string text, Sprite image) {
		this.text = text;
		this.image = image;
		extraText = new List<string>();
		extraImages = new List<Sprite>();
	}
}

public class TextImageDisplay : MonoBehaviour {
	public TextMeshProUGUI displayText;
	public Image displayImage;
	[SerializeField] Button nextButton;
	[SerializeField] float nextTimeDelay = 1f;
	[SerializeField] bool autoHideOnNext = true;

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
		if (autoHideOnNext) {
			HideText();
			HideImage();
		}
		OnNext?.Invoke();
	}
}
