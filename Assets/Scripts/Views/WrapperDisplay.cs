using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WrapperDisplay : MonoBehaviour {
	public void LoadNextScene() {
		RLUtilities.LoadNextScene();
	}

	private void Update() {
		if (Input.GetKeyUp(KeyCode.Escape)) Application.Quit();
	}
}
