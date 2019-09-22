using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EndgameDisplay : MonoBehaviour {
	[SerializeField] TextMeshProUGUI resultsText, hintsText, learningText;
	[SerializeField] Button showLearningButton, hideLearningButton, showHintsButton, hideHintsButton, resetButton;

	Animator animator;

	const string ANIM_TRIGGER_RESULT = "Result", ANIM_BOOL_HINTS = "Hints", ANIM_BOOL_LEARNING = "Learning";

	private void Awake() {
		animator = GetComponent<Animator>();
		if (showLearningButton) showLearningButton.onClick.AddListener(() => SetAnimBoolParam(ANIM_BOOL_LEARNING, true));
		if (hideLearningButton) hideLearningButton.onClick.AddListener(() => SetAnimBoolParam(ANIM_BOOL_LEARNING, false));
		if (showHintsButton) showHintsButton.onClick.AddListener(() => SetAnimBoolParam(ANIM_BOOL_HINTS, true));
		if (hideHintsButton) hideHintsButton.onClick.AddListener(() => SetAnimBoolParam(ANIM_BOOL_HINTS, false));
		if (resetButton) resetButton.onClick.AddListener(() => RLUtilities.ResetGame());
		gameObject.SetActive(false);
	}

	public void ShowResults(EndgameResults results) {
		gameObject.SetActive(true);

		foreach (TextMeshProUGUI text in new TextMeshProUGUI[] { resultsText, hintsText, learningText }) if (text) text.text = string.Empty;

		void AddStringToText(string str, TextMeshProUGUI text) {
			if (text && !string.IsNullOrEmpty(str)) text.text += (string.IsNullOrEmpty(text.text) ? string.Empty : "\n") + str;
		}

		foreach (var summaryHint in results.summaryHintMap) {
			AddStringToText(summaryHint.Key, resultsText);
			AddStringToText(summaryHint.Value, hintsText);
		}
		results.learning.ForEach(learning => AddStringToText(learning, learningText));

		if (animator) animator.SetTrigger(ANIM_TRIGGER_RESULT);
	}

	void SetAnimBoolParam(string boolParam, bool setTo) {
		if (animator) animator.SetBool(boolParam, setTo);
	}
}