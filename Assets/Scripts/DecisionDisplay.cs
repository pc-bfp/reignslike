using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DecisionDisplay : MonoBehaviour {
	[SerializeField] TextMeshProUGUI decisionText, outcomeText;
	[SerializeField] Button yesButton, noButton;
	[SerializeField] GameObject yesStatus, noStatus;

	public delegate void DecisionMadeEvent(bool yes);
	public static DecisionMadeEvent OnDecisionMade;

	Animator animator;

	const string PARAM_TRIGGER_SHOW = "Show", PARAM_TRIGGER_DECIDED = "Decided", PARAM_TRIGGER_OUTCOME = "Outcome", PARAM_TRIGGER_END = "End";

	void Awake() {
		if (yesButton) yesButton.onClick.AddListener(() => DecisionMade(true));
		if (noButton) noButton.onClick.AddListener(() => DecisionMade(false));
		animator = GetComponent<Animator>();
	}

	public void ShowDecision(string decision) {
		if (decisionText) decisionText.text = decision;
		if (animator) animator.SetTrigger(PARAM_TRIGGER_SHOW);
	}

	void DecisionMade(bool yes) {
		OnDecisionMade?.Invoke(yes);
		if (yesStatus) yesStatus.SetActive(yes);
		if (noStatus) noStatus.SetActive(!yes);
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
