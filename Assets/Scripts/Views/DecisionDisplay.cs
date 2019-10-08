using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class DecisionDisplay : MonoBehaviour {
	[SerializeField] TextMeshProUGUI decisionText, statusText, outcomeText;
	[SerializeField] Image mainImage;
	[SerializeField] Sprite placeholder;
	[SerializeField] List<DecisionButton> decisionButtons;
	[SerializeField] Button nextButton;
	[SerializeField] float textSpeed = 20f, textDelay = 0.5f;

	public delegate void DecisionMadeEvent(int answerIndex);
	public static DecisionMadeEvent OnDecisionMade;

	public delegate void NextOutcomeEvent();
	public static NextOutcomeEvent OnNextOutcome;

	Animator animator;

	const string ANIM_TRIGGER_SHOW = "Show", ANIM_TRIGGER_BUTTONS = "Buttons", ANIM_TRIGGER_DECIDED = "Decided", ANIM_BOOL_OUTCOME = "Outcome", ANIM_TRIGGER_END = "End";

	void Awake() {
		animator = GetComponent<Animator>();
		for (int d = 0; d < decisionButtons.Count; d++) {
			decisionButtons[d].ButtonIndex = d;
			decisionButtons[d].OnButtonPressed += DecisionMade;
		}
		if (nextButton) {
			nextButton.onClick.AddListener(() => OnNextOutcome?.Invoke());
			SetNextButtonState(false);
		}
		if (mainImage) {
			mainImage.preserveAspect = true;
			mainImage.type = Image.Type.Simple;
		}
	}

	public void ShowDecision(Decision decision) {
		if (animator) {
			animator.SetTrigger(ANIM_TRIGGER_SHOW);
			animator.SetBool(ANIM_BOOL_OUTCOME, false);
		}
		SetImage(decision.decisionImage);
		RLUtilities.TweenText(decisionText, decision.decisionText, textSpeed, () => {
			for (int b = 0; b < decisionButtons.Count; b++) {
				decisionButtons[b].SetButtonText(b >= decision.buttonResults.Count ? string.Empty : decision.buttonResults[b].buttonText);
			}
			if (animator) animator.SetTrigger(ANIM_TRIGGER_BUTTONS);
		}).SetDelay(textDelay);
	}

	void DecisionMade(int buttonIndex) {
		if (statusText) statusText.text = decisionButtons[buttonIndex].ButtonText;
		StartCoroutine(ActionSandwichCR(() => animator.SetTrigger(ANIM_TRIGGER_DECIDED), () => OnDecisionMade?.Invoke(buttonIndex)));
	}

	public void SetStatus(string status) {
		if (statusText) statusText.text = status;
	}

	Tween outcomeTween = null;

	public void ShowOutcome(string text, Sprite image = null) {
		if (animator) animator.SetBool(ANIM_BOOL_OUTCOME, true);
		RLUtilities.TweenText(outcomeText, text, textSpeed, () => SetNextButtonState(true)).SetDelay(textDelay);
		SetImage(image);
	}

	public void HideOutcome() {
		if (outcomeTween != null && outcomeTween.IsPlaying()) outcomeTween.Complete();
		if (animator) animator.SetBool(ANIM_BOOL_OUTCOME, false);
		SetNextButtonState(false);
	}

	public void End(System.Action doOnEnd) {
		SetNextButtonState(false);
		StartCoroutine(ActionSandwichCR(() => animator.SetTrigger(ANIM_TRIGGER_END), doOnEnd));
	}

	IEnumerator ActionSandwichCR(System.Action before, System.Action after) {
		if (animator) {
			before?.Invoke();
			yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
		}
		after?.Invoke();
	}

	void SetNextButtonState(bool isOn) {
		if (nextButton) nextButton.gameObject.SetActive(isOn);
	}

	void SetImage(Sprite image) {
		if (!mainImage) return;
		mainImage.sprite = image ? image : placeholder;
	}
}