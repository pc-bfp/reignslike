using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DecisionDisplay : MonoBehaviour {
	[SerializeField] TextMeshProUGUI decisionText, outcomeText;
	[SerializeField] List<DecisionButton> decisionButtons;
	[SerializeField] TextMeshProUGUI answerText;

	public delegate void DecisionMadeEvent(int answerIndex);
	public static DecisionMadeEvent OnDecisionMade;

	Animator animator;

	const string PARAM_TRIGGER_SHOW = "Show", PARAM_TRIGGER_DECIDED = "Decided", PARAM_TRIGGER_OUTCOME = "Outcome", PARAM_TRIGGER_END = "End";

	void Awake() {
		for (int d = 0; d < decisionButtons.Count; d++) {
			decisionButtons[d].ButtonIndex = d;
			decisionButtons[d].OnButtonPressed += DecisionMade;
		}
		animator = GetComponent<Animator>();
	}

	public void ShowDecision(Decision decision) {
		if (decisionText) decisionText.text = decision.decisionText;
		for (int b = 0; b < decisionButtons.Count; b++) {
			decisionButtons[b].SetButtonText(b >= decision.buttonResults.Count ? string.Empty : decision.buttonResults[b].buttonText);
		}
		if (animator) animator.SetTrigger(PARAM_TRIGGER_SHOW);
	}

	void DecisionMade(int buttonIndex) {
		OnDecisionMade?.Invoke(buttonIndex);
		if (answerText) answerText.text = decisionButtons[buttonIndex].ButtonText;
		if (animator) animator.SetTrigger(PARAM_TRIGGER_DECIDED);
	}

	public void ShowOutcome(string outcome) {
		if (outcomeText) outcomeText.text = outcome;
		if (animator) animator.SetTrigger(PARAM_TRIGGER_OUTCOME);
	}

	public void End(System.Action doOnEnd) {
		StartCoroutine(EndCR(doOnEnd));
	}

	IEnumerator EndCR(System.Action doOnEnd) {
		if (!animator) {
			doOnEnd?.Invoke();
			yield break;
		}
		animator.SetTrigger(PARAM_TRIGGER_END);
		yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
		doOnEnd?.Invoke();
	}
}
