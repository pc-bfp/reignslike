using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class ButtonRepeater : Button {
	public UnityEvent OnHold = new UnityEvent(), OnRelease = new UnityEvent();

	bool isPressed = false;
	float timeToNextRepeat;

	const float REPEAT_TIME_MAX = 1f, REPEAT_TIME_MIN = 0.1f, REPEAT_TIME_REDUCTION = 0.6f;

	protected override void Awake() {
		OnHold.AddListener(() => StartCoroutine(WhilePressedCR()));
		OnRelease.AddListener(() => {
			if (isPressed) {
				StopCoroutine(WhilePressedCR());
				Initialize();
			}
		});
	}

	public override void OnPointerDown(PointerEventData eventData) {
		base.OnPointerDown(eventData);
		//OnHold?.Invoke();
	}

	public override void OnPointerUp(PointerEventData eventData) {
		base.OnPointerUp(eventData);
		//OnRelease?.Invoke();
	}

	public override void OnPointerExit(PointerEventData eventData) {
		base.OnPointerExit(eventData);
		//OnRelease?.Invoke();
	}

	public void Initialize() {
		isPressed = false;
		timeToNextRepeat = REPEAT_TIME_MAX;
	}

	IEnumerator WhilePressedCR() {
		isPressed = true;
		while (isPressed) {
			yield return new WaitForSeconds(timeToNextRepeat);
			onClick?.Invoke();
			timeToNextRepeat = Mathf.Max(REPEAT_TIME_MIN, timeToNextRepeat * REPEAT_TIME_REDUCTION);
			yield return null;
		}
	}
}