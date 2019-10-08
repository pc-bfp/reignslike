using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class DecisionDisplay : MonoBehaviour {
	[SerializeField] TextMeshProUGUI decisionText, outcomeText;
	[SerializeField] Image mainImage;
	[SerializeField] List<DecisionButton> decisionButtons;
	[SerializeField] Button nextButton;

	public delegate void DecisionMadeEvent(int answerIndex);
	public static DecisionMadeEvent OnDecisionMade;

	public delegate void NextOutcomeEvent();
	public static NextOutcomeEvent OnNextOutcome;

	//enum DecisionState { HIDDEN, SHOW, DECIDED, OUTCOME }
	//DecisionState state = DecisionState.HIDDEN;

	Animator animator;

	const string ANIM_TRIGGER_SHOW = "Show", ANIM_TRIGGER_DECIDED = "Decided", ANIM_BOOL_OUTCOME = "Outcome", ANIM_TRIGGER_END = "End";

	void Awake() {
		animator = GetComponent<Animator>();
		for (int d = 0; d < decisionButtons.Count; d++) {
			decisionButtons[d].ButtonIndex = d;
			decisionButtons[d].OnButtonPressed += DecisionMade;
		}
		if (nextButton) nextButton.onClick.AddListener(NextOutcome);
	}

	public void ShowDecision(Decision decision) {
		if (decisionText) decisionText.text = decision.decisionText;
		for (int b = 0; b < decisionButtons.Count; b++) {
			decisionButtons[b].SetButtonText(b >= decision.buttonResults.Count ? string.Empty : decision.buttonResults[b].buttonText);
		}
		if (animator) animator.SetTrigger(ANIM_TRIGGER_SHOW);
	}

	void DecisionMade(int buttonIndex) {
		decisionButtons.ForEach(db => {
			if (db.ButtonIndex != buttonIndex) db.ButtonShrink();
		});
		StartCoroutine(AnimActionSandwich(() => animator.SetTrigger(ANIM_TRIGGER_DECIDED), () => OnDecisionMade?.Invoke(buttonIndex)));
	}

	public void ShowOutcome(string text, Sprite image = null) {
		outcomeText.text = text;
		mainImage.sprite = image;
		if (animator) animator.SetBool(ANIM_BOOL_OUTCOME, true);
	}

	void NextOutcome() {
		StartCoroutine(AnimActionSandwich(() => animator.SetBool(ANIM_BOOL_OUTCOME, false), () => OnNextOutcome?.Invoke()));
	}

	public void End(System.Action doOnEnd) {
		StartCoroutine(AnimActionSandwich(() => animator.SetTrigger(ANIM_TRIGGER_END), doOnEnd));
	}

	IEnumerator AnimActionSandwich(System.Action before, System.Action after) {
		if (animator) {
			before?.Invoke();
			yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
		}
		after?.Invoke();
	}
}
