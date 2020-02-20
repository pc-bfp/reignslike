using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using DG.Tweening;

public class RLUtilities {
	public delegate void MouseClickEvent();
	public static MouseClickEvent OnMouseClick;

	class RLUtilitiesMB : MonoBehaviour {
		bool isMouseHeld = false;
		private void Update() {
			if (Input.GetMouseButtonDown(0)) isMouseHeld = true;
			else if (isMouseHeld && Input.GetMouseButtonUp(0)) {
				isMouseHeld = false;
				OnMouseClick?.Invoke();
			}
		}
	}

	static RLUtilitiesMB Instance { get; set; }

	public static void Initialize() {
		if (!Instance) UnityEngine.Object.DontDestroyOnLoad(Instance = new GameObject("Utilities", typeof(RLUtilitiesMB)).GetComponent<RLUtilitiesMB>());
	}

	public static T RandomFromList<T>(List<T> list) {
		return list[UnityEngine.Random.Range(0, list.Count)];
	}

	public static void LoadNextScene() {
		SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
	}

	public static void ResetScene(bool withSetup = false) {
		SetupDisplay.DoShowSetup = withSetup;
		SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
	}

	static AudioSource scribbleSFX;

	public static Tween TweenText(TextMeshProUGUI textMesh, string textToSet, float charsPerSecond, Action doOnComplete) {
		int charIndex = 0, textLength = textToSet.Length;
		if (!scribbleSFX) {
			scribbleSFX = AudioManager.PlaySFX(SFXType.SCRIBBLE, false);
			scribbleSFX.loop = true;
		}
		scribbleSFX.time = UnityEngine.Random.Range(0, scribbleSFX.clip.length);
		scribbleSFX.PlayDelayed(0.33f);

		//textMesh.text = textToSet;
		//for (int c = 0; c < textLength; c++) textMesh.textInfo.characterInfo[c].isVisible = false;
		//textMesh.ForceMeshUpdate();
		//return DOTween.To(() => charIndex, c => textMesh.textInfo.characterInfo[c].isVisible = true, textLength, textLength / charsPerSecond);

		textMesh.text = string.Empty;
		bool doUpdateText = true;
		Tween retval = DOTween.To(() => charIndex, c => {
			//if (textToSet[c] == '<') doUpdateText = false;
			//if (textToSet[c] == '>') doUpdateText = true;
			if (doUpdateText) textMesh.text = textToSet.Substring(0, c);
		}, textLength, textLength / charsPerSecond)
		.SetEase(Ease.Linear)
		.OnComplete(() => doOnComplete.Invoke());

		doOnComplete += () => {
			scribbleSFX.Stop();
			OnMouseClick -= retval.Complete;
			textMesh.text = textToSet; // Inn-sewer-ants
		};

		OnMouseClick += retval.Complete;
		return retval;
	}

	const char TEMP_QUOTE = '¶', ACTUAL_COLUMN_SEPARATOR = '§';

	public static string[][] ReadSheet(string sheetString) {
		if (string.IsNullOrEmpty(sheetString)) return null;

		// Deal with "enclosed" substrings
		sheetString = sheetString.Replace("\"\"", TEMP_QUOTE.ToString());
		bool isWithinQuotes = false;
		List<int> removeCharsAtIndices = new List<int>();

		char[] sheetChars = sheetString.ToCharArray();
		for (int c = 0; c < sheetChars.Length; c++) {
			if (sheetChars[c] == '\"') isWithinQuotes = !isWithinQuotes;
			else if (sheetChars[c] == RLConstants.COLUMN_SEPARATOR && !isWithinQuotes) sheetChars[c] = ACTUAL_COLUMN_SEPARATOR;
			else if (sheetChars[c] == RLConstants.ROW_SEPARATOR && isWithinQuotes) sheetChars[c] = '\"';
		}
		sheetString = new string(sheetChars).Replace("\"", "").Replace(TEMP_QUOTE, '\"');

		char[] rowSplit = { RLConstants.ROW_SEPARATOR }, columnSplit = { ACTUAL_COLUMN_SEPARATOR };
		string[] rowStrings = sheetString.Split(rowSplit, StringSplitOptions.RemoveEmptyEntries);
		string[][] retval = new string[rowStrings.Length][];

		for (int r = 0; r < rowStrings.Length; r++) {
			retval[r] = rowStrings[r].Split(columnSplit, StringSplitOptions.None);
			for (int f = 0; f < retval[r].Length; f++) retval[r][f] = retval[r][f].Trim(RLConstants.TRIM_CHARS);
		}

		return retval;
	}


	public static string ApplyBoldItalic(string toFormat) {

		void ProcessTag(char tagChar, string tagIn, string tagOut) {
			List<int> tagIndices = new List<int>();
			bool justTagged = false, runningTag = false;
			for (int c = 0; c < toFormat.Length; c++) {
				if (toFormat[c] == tagChar) {
					if (justTagged) runningTag = true;
					else justTagged = true;
				}
				else if (justTagged) {
					if (!runningTag) tagIndices.Add(c - 1);
					runningTag = justTagged = false;
				}
			}

			tagIndices.Reverse();	// Reversing so that index number is unaffected by string length
			bool taggingIn = false;
			foreach (int t in tagIndices) {
				toFormat = toFormat.Remove(t, 1).Insert(t, taggingIn ? tagIn : tagOut);
				taggingIn = !taggingIn;
			}
		}

		ProcessTag(RLConstants.TAG_BOLD, "<b>", "</b>");
		ProcessTag(RLConstants.TAG_ITALIC, "<i>", "</i>");
		return toFormat;
	}
}


public class RLPool<T> {
	List<T> pool, inUse = new List<T>();

	public delegate void ReturnToPoolEvent(T member);
	public ReturnToPoolEvent OnReturned;

	public RLPool(int size, T defaultMember) {
		pool = new List<T>();
		for (; size > 0; size--) pool.Add(defaultMember);
	}

	public RLPool(List<T> members) {
		pool = members;
	}

	public T Get() {
		if (pool.Count == 0) ReturnToPool(inUse[0]);
		T retval = pool[0];
		pool.RemoveAt(0);
		inUse.Add(retval);
		return retval;
	}

	public void ReturnToPool(T member) {
		if (!inUse.Contains(member)) return;
		inUse.Remove(member);
		pool.Add(member);
		OnReturned?.Invoke(member);
	}
}