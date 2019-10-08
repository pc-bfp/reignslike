using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class StoryView : MonoBehaviour {
	[SerializeField] TextImageDisplay display;
	[SerializeField] List<TextImage> textImages;
	[SerializeField] float timeBetweenPanels = 0.5f;

	public delegate void CompletedEvent();
	public CompletedEvent OnCompleted;
	
	int storyIndex = -1;
	bool isShowing = false;

	const string ANIM_BOOL_SHOW = "Show";

	void Awake() {
		display.OnNext += OnNext;
		gameObject.SetActive(false);
	}

	public void Begin() {
		gameObject.SetActive(true);
		OnNext();
	}

	void OnNext() {
		StartCoroutine(OnNextCR());
	}

	IEnumerator OnNextCR() {
		storyIndex++;
		if (isShowing) {
			// Hide the old
			ShowHide(false);
			yield return new WaitForSeconds(display.Animator.GetCurrentAnimatorStateInfo(0).length + timeBetweenPanels);
		}
		if (storyIndex < textImages.Count) {
			ShowHide(true);
		}
		else {
			// The end
			gameObject.SetActive(false);
			OnCompleted?.Invoke();
		}
	}

	public void Reset(List<TextImage> newStory) {
		textImages = newStory;
		storyIndex = -1;
		isShowing = false;
	}

	void ShowHide(bool doShow) {
		if (isShowing == doShow) return;
		isShowing = doShow;
		if (isShowing) {
			display.ShowText(textImages[storyIndex].Text);
			display.ShowImage(textImages[storyIndex].Image);
		}
		else {
			display.HideText();
			display.HideImage();
		}
	}
}