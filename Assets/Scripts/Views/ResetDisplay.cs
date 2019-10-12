using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ResetDisplay : MonoBehaviour {
	[SerializeField] Button resetButton, quitButton, showButton, hideButton;
	[SerializeField] DecisionButton quickplayButton;

	Animator animator;

	static bool isQuickplayOn = false;
	const float SPEED_NORMAL = 1f, SPEED_QUICK = 3f;
	const string PARAM_BOOL_VISIBLE = "Visible";

	private void Awake() {
		animator = GetComponent<Animator>();
		resetButton.onClick.AddListener(() => RLUtilities.ResetGame());
		quitButton.onClick.AddListener(() => Application.Quit());
		showButton.onClick.AddListener(() => SetVisible(true));
		hideButton.onClick.AddListener(() => SetVisible(false));
		quickplayButton.OnButtonPressed += _ => SetQuickplay(!isQuickplayOn);
		SetQuickplay(isQuickplayOn);
	}

	private void Start() {
		SetVisible(false);
	}

	public void SetVisible(bool isVisible) {
		showButton.gameObject.SetActive(!isVisible);
		hideButton.gameObject.SetActive(isVisible);
		if (animator) animator.SetBool(PARAM_BOOL_VISIBLE, isVisible);
	}

	void SetQuickplay(bool toOn) {
		isQuickplayOn = toOn;
		Time.timeScale = isQuickplayOn ? SPEED_QUICK : SPEED_NORMAL;
		quickplayButton.SetButtonText(string.Format("Turbo {0}", isQuickplayOn ? "ON" : "off"));
	}

	private void Update() {
		if (Input.GetKeyUp(KeyCode.Escape)) SetVisible(!animator.GetBool(PARAM_BOOL_VISIBLE));
	}
}
