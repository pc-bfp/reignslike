using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ResetDisplay : MonoBehaviour {
	[SerializeField] Button resetButton, quitButton, showButton, hideButton;

	Animator animator;

	const string PARAM_BOOL_VISIBLE = "Visible";

	private void Awake() {
		animator = GetComponent<Animator>();
		resetButton.onClick.AddListener(() => SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex));
		quitButton.onClick.AddListener(() => Application.Quit());
		showButton.onClick.AddListener(() => SetVisible(true));
		hideButton.onClick.AddListener(() => SetVisible(false));
	}

	private void Start() {
		SetVisible(false);
	}

	public void SetVisible(bool isVisible) {
		showButton.gameObject.SetActive(!isVisible);
		hideButton.gameObject.SetActive(isVisible);
		if (animator) animator.SetBool(PARAM_BOOL_VISIBLE, isVisible);
	}
}
