using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LearningDisplay : MonoBehaviour {
	[SerializeField] TextMeshProUGUI boldText, followText;
	[SerializeField] Image doodleImage, medalImage;
	[SerializeField] Button nextButton, resetButton;

	public List<EndgameResults.Learning> Learnings = new List<EndgameResults.Learning>();

	Animator animator;
	int curLearningIndex = -1;
	bool isUpdating = false;

	const string ANIM_TRIGGER_NEXT = "Next", IMAGE_LOCATION = "Learnings/";

	private void Awake() {
		animator = GetComponent<Animator>();
		if (nextButton) nextButton.onClick.AddListener(() => ShowNext());
		if (resetButton) resetButton.onClick.AddListener(() => RLUtilities.ResetScene());
		gameObject.SetActive(false);
	}

	public void ShowNext() {
		if (isUpdating) return;
		curLearningIndex++;
		if (curLearningIndex >= Learnings.Count) return;
		if (curLearningIndex == 0) {
			gameObject.SetActive(true);
			UpdateLearning();
		}
		else {
			animator.SetTrigger(ANIM_TRIGGER_NEXT);
			isUpdating = true;
		}
	}

	// Needs to be triggered from animation
	public void UpdateLearning() {
		EndgameResults.Learning nextLearning = Learnings[curLearningIndex];
		boldText.text = nextLearning.BoldText;
		followText.text = nextLearning.FollowText;
		if (doodleImage) doodleImage.sprite = Resources.Load<Sprite>(IMAGE_LOCATION + nextLearning.LearningID + "_doodle");
		if (medalImage) medalImage.sprite = Resources.Load<Sprite>(IMAGE_LOCATION + nextLearning.LearningID + "_medal");
		bool isThereMore = curLearningIndex < Learnings.Count - 1;
		if (nextButton) nextButton.gameObject.SetActive(isThereMore);
		if (resetButton) resetButton.gameObject.SetActive(!isThereMore);
		isUpdating = false;
	}
}