using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EndgameDisplay : MonoBehaviour {
	[SerializeField] TextMeshProUGUI resultsText, hintsText;
	[SerializeField] LearningDisplay learningDisplay;
	[SerializeField] Button showLearningButton, showHintsButton, hideHintsButton;
	[SerializeField] StoryView summaryDisplay;

	Animator animator;

	const string ANIM_TRIGGER_RESULT = "Result", ANIM_BOOL_HINTS = "Hints", ANIM_TRIGGER_HIDE = "Hide";

	private void Awake() {
		animator = GetComponent<Animator>();
		if (showHintsButton) showHintsButton.onClick.AddListener(() => SetAnimBoolParam(ANIM_BOOL_HINTS, true));
		if (hideHintsButton) hideHintsButton.onClick.AddListener(() => SetAnimBoolParam(ANIM_BOOL_HINTS, false));
		if (showLearningButton) showLearningButton.onClick.AddListener(() => {
			SetAnimBoolParam(ANIM_TRIGGER_HIDE, true);
			learningDisplay.ShowNext();
		});
		if (summaryDisplay) summaryDisplay.OnCompleted += () => learningDisplay.ShowNext();
		gameObject.SetActive(false);
	}

	public void ShowResults(EndgameResults results) {
		gameObject.SetActive(true);
		foreach (TextMeshProUGUI text in new TextMeshProUGUI[] { resultsText, hintsText }) if (text) text.text = string.Empty;

		//void AddStringToText(string str, TextMeshProUGUI text) {
		//	if (text && !string.IsNullOrEmpty(str)) text.text += (string.IsNullOrEmpty(text.text) ? string.Empty : "\n") + str;
		//}

		List<TextImage> summaries = new List<TextImage>();
		foreach (var curSummary in results.summaries) {
			summaries.Add(new TextImage(curSummary.SummaryText, curSummary.Image));
			//AddStringToText(summaryHint.Key, resultsText);
			//AddStringToText(summaryHint.Value, hintsText);
		}
		summaryDisplay.Reset(summaries);
		summaryDisplay.Activate();

		if (learningDisplay) learningDisplay.Learnings = results.learning;
		//if (animator) animator.SetTrigger(ANIM_TRIGGER_RESULT);
	}

	void SetAnimBoolParam(string boolParam, bool setTo) {
		if (animator) animator.SetBool(boolParam, setTo);
	}
}