using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class DecisionsHolder {
	List<Decision> alwaysAvailable, unavailable, decisionQueue;

	const int BUTTON_INFO_FIELDS = 3, BUTTON_COUNT = 2;

	public DecisionsHolder(TextAsset decisionsFile) {
		alwaysAvailable = new List<Decision>();
		unavailable = new List<Decision>();
		decisionQueue = new List<Decision>();

		List<string> statNames = new List<string>();
		string[] allDecs = decisionsFile.text.Split(new string[] { Constants.ROW_SPLIT }, System.StringSplitOptions.RemoveEmptyEntries);
		int fieldsPerButton = 0, fieldsBeforeButtons = 0;

		for (int d = 0; d < allDecs.Length; d++) {
			allDecs[d] = allDecs[d].Trim('\n', ' ');
			string[] curDecFields = allDecs[d].Split(new string[] { Constants.COLUMN_SPLIT }, System.StringSplitOptions.None);
			if (curDecFields.Length < 1 || string.IsNullOrEmpty(curDecFields[0])) continue; // Invalid row. REJECTED

			if (d == 0) {    // Column names
				string curButtonName = string.Empty;
				int firstStatEffectIndex = 0;

				for (int i = 0; i < curDecFields.Length; i++) {
					string curField = curDecFields[i];
					if (curField.StartsWith("Button:")) {
						if (!string.IsNullOrEmpty(curButtonName)) break;
						curButtonName = curField.Replace("Button:", "").Trim();
						fieldsBeforeButtons = i;
					}
					else if (i >= fieldsBeforeButtons + BUTTON_INFO_FIELDS) {
						statNames.Add(curField);
					}
				}
				fieldsPerButton = BUTTON_INFO_FIELDS + statNames.Count;
				continue;   // That's all we want from the first row
			}

			Decision curDec = new Decision() {
				decisionText = curDecFields[0]
			};

			for (int b = 0; b < BUTTON_COUNT; b++) {
				int i = fieldsBeforeButtons + (b * fieldsPerButton);    // Button info start index
				int[] curStatEffects = new int[statNames.Count];

				for (int s = 0; s < statNames.Count; s++) {
					// Stat effects
					string curEffect = curDecFields[i + BUTTON_INFO_FIELDS + s];
					if (string.IsNullOrEmpty(curEffect)) continue;
					int.TryParse(curEffect, out curStatEffects[s]);
				}
				curDec.buttonResults.Add(new Decision.ButtonResult(curDecFields[i], curDecFields[i + 1], curDecFields[i + 2], curStatEffects));
			}

			int reqIndex = fieldsBeforeButtons + (BUTTON_COUNT * fieldsPerButton),
				freeTurnIndex = reqIndex + 1;
			curDec.SetRequirements(curDecFields[reqIndex], statNames);
			curDec.turnCost = curDecFields[freeTurnIndex] == "TRUE" ? 0 : 1;

			(string.IsNullOrEmpty(curDecFields[reqIndex]) ? alwaysAvailable : unavailable).Add(curDec);
		}

		// Set up decision queue
		decisionQueue.Clear();
		decisionQueue.InsertRange(0, alwaysAvailable);
		decisionQueue.Sort((d1, d2) => { // Shuffle it
			return Random.value > 0.5f ? 1 : -1;
		});
		UpdateAvailableDecisions();

		GameManager.Instance.OnDecisionTaken += UpdateAvailableDecisions;
	}


	#region DECISION_DETERMINATION

	//void UnlockChanged(string unlock, bool wasAdded) {
	//	List<Decision> toBeAdded = new List<Decision>(), toBeRemoved = new List<Decision>();

	//	decisionQueue.ForEach(dec => {
	//		if (dec.unlockedRequirements.Contains(unlock)) (wasAdded ? toBeAdded : toBeRemoved).Add(dec);
	//	});
	//	unavailable.ForEach(dec => {
	//		if (dec.IsAvailable()) toBeAdded.Add(dec);
	//	});

	//	toBeAdded.ForEach(dec => AddToDecisionQueue(dec));
	//	toBeRemoved.ForEach(dec => RemoveFromDecisionQueue(dec));
	//}

	List<Decision> makeAvailable = new List<Decision>(), makeUnavailable = new List<Decision>();

	void UpdateAvailableDecisions() {
		makeAvailable.Clear();
		makeUnavailable.Clear();

		decisionQueue.ForEach(dec => {
			if (!dec.IsAvailable()) makeUnavailable.Add(dec);
		});
		unavailable.ForEach(dec => {
			if (dec.IsAvailable()) makeAvailable.Add(dec);
		});

		makeAvailable.ForEach(dec => AddToDecisionQueue(dec));
		makeUnavailable.ForEach(dec => RemoveFromDecisionQueue(dec));
	}

	void AddToDecisionQueue(Decision dec, bool ignoreTurns = false) {
		if (decisionQueue.Contains(dec)) decisionQueue.Remove(dec);
		int insertIndex = (ignoreTurns || dec.doWithinTurns == null) ? Random.Range(0, decisionQueue.Count) : Mathf.Clamp(dec.doWithinTurns.GetWithinRange(), 0, decisionQueue.Count);
		decisionQueue.Insert(insertIndex, dec);
		if (unavailable.Contains(dec)) unavailable.Remove(dec);
	}

	void RemoveFromDecisionQueue(Decision dec) {
		if (!decisionQueue.Contains(dec)) return;
		decisionQueue.Remove(dec);
		if (!unavailable.Contains(dec)) unavailable.Add(dec);
	}

	public Decision GetDecision() {
		Decision dec = decisionQueue[0];
		if (!dec.IsRecurring) dec.doWithinTurns = null;	// "Turns:" countdown only happens once
		AddToDecisionQueue(dec);
		return dec;
	}

	#endregion
}