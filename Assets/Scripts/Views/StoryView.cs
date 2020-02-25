using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class StoryView : MonoBehaviour {
	[SerializeField] TextImageDisplay display;
	[SerializeField] float timeBetweenPanels = 0.5f;
	[SerializeField] int animLayerForDelay = -1;
	public List<TextImage> textImages;

	public delegate void CompletedEvent();
	public CompletedEvent OnCompleted;

	int storyIndex = 0, textImageIndex = -1;
	bool isShowing = false, isChanging = false;

	const string ANIM_BOOL_SHOW = "Show";

	void Awake() {
		display.OnNext += OnNext;
		gameObject.SetActive(false);
	}

	public void Activate() {
		gameObject.SetActive(true);
		OnNext();
	}

	void OnNext() {
		if (isChanging) return;
		StartCoroutine(OnNextCR());
	}

	IEnumerator OnNextCR() {
		isChanging = true;
		if (isShowing) {
			// Hide the old
			textImageIndex++;
			bool switchTextOrImage = false;
			if (textImageIndex < textImages[storyIndex].extraText.Count) {
				display.displayText.text = textImages[storyIndex].extraText[textImageIndex];
				switchTextOrImage = true;
			}
			if (textImageIndex < textImages[storyIndex].extraImages.Count) {
				display.displayImage.sprite = textImages[storyIndex].extraImages[textImageIndex];
				switchTextOrImage = true;
			}
			if (!switchTextOrImage) {
				ShowHide(false);
				yield return new WaitForSeconds((animLayerForDelay < 0 ? 0 : display.Animator.GetCurrentAnimatorStateInfo(animLayerForDelay).length) + timeBetweenPanels);
				storyIndex++;
				textImageIndex = -1;
			}
		}
		if (storyIndex < textImages.Count) {
			ShowHide(true);
		}
		else {
			End();
		}
		isChanging = false;
	}

	public void Reset(List<TextImage> newStory) {
		textImages = newStory;
		storyIndex = 0;
		isShowing = false;
	}

	void ShowHide(bool doShow) {
		if (isShowing == doShow) return;
		isShowing = doShow;
		if (isShowing) {
			display.ShowText(textImages[storyIndex].text);
			display.ShowImage(textImages[storyIndex].image);
		}
		else {
			display.HideText();
			display.HideImage();
		}
	}

	void End() {
		// The end
		gameObject.SetActive(false);
		OnCompleted?.Invoke();
	}

	float holdTime = 0;

	private void Update() {
		if (Input.GetMouseButton(0)) {
			holdTime += Time.deltaTime;
			if (holdTime >= 1f) End();
		}
		else if (Input.GetMouseButtonUp(0)) holdTime = 0;
	}
}