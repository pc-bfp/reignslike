using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class DecisionHolder {
	public static List<string> StatNames;

	List<Decision> alwaysAvailable, unavailable, decisionQueue;

	const int BUTTON_INFO_FIELDS = 3, BUTTON_COUNT = 2;

	public DecisionHolder(TextAsset decisionsFile) {
		alwaysAvailable = new List<Decision>();
		unavailable = new List<Decision>();
		decisionQueue = new List<Decision>();
		StatNames = new List<string>();
		string[][] allDecisions = SheetReader.ReadSheet(decisionsFile.text);
		int fieldsPerButton = 0, fieldsBeforeButtons = 0;

		for (int d = 0; d < allDecisions.Length; d++) {

			if (d == 0) {    // Column names
				string curButtonName = string.Empty;

				for (int i = 0; i < allDecisions[d].Length; i++) {
					string curField = allDecisions[d][i];
					if (curField.StartsWith("Button:")) {
						if (!string.IsNullOrEmpty(curButtonName)) break;
						curButtonName = curField.Replace("Button:", "").Trim();
						fieldsBeforeButtons = i;
					}
					else if (i >= fieldsBeforeButtons + BUTTON_INFO_FIELDS) {
						StatNames.Add(curField);
					}
				}
				fieldsPerButton = BUTTON_INFO_FIELDS + StatNames.Count;
				continue;   // That's all we want from the first row
			}

			if (allDecisions[d].Length < 1 || string.IsNullOrEmpty(allDecisions[d][0])) continue; // Invalid row. REJECTED

			Decision curDec = new Decision() {
				decisionText = allDecisions[d][0]
			};

			for (int b = 0; b < BUTTON_COUNT; b++) {
				int i = fieldsBeforeButtons + (b * fieldsPerButton);    // Button info start index
				int[] curStatEffects = new int[StatNames.Count];

				for (int s = 0; s < StatNames.Count; s++) {
					// Stat effects
					string curEffect = allDecisions[d][i + BUTTON_INFO_FIELDS + s];
					if (string.IsNullOrEmpty(curEffect)) continue;
					int.TryParse(curEffect, out curStatEffects[s]);
				}
				curDec.buttonResults.Add(new Decision.ButtonResult(allDecisions[d][i], allDecisions[d][i + 1], allDecisions[d][i + 2], curStatEffects));
			}

			int reqIndex = fieldsBeforeButtons + (BUTTON_COUNT * fieldsPerButton),
				freeTurnIndex = reqIndex + 1;
			curDec.SetRequirements(allDecisions[d][reqIndex], StatNames);
			curDec.turnCost = allDecisions[d][freeTurnIndex] == "TRUE" ? 0 : 1;

			(string.IsNullOrEmpty(allDecisions[d][reqIndex]) ? alwaysAvailable : unavailable).Add(curDec);
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

	void AddToDecisionQueue(Decision dec, bool awayFromFront = false) {
		int GetRandomIndex() {
			if (awayFromFront) return Mathf.RoundToInt(Mathf.Lerp(1, decisionQueue.Count - 1, Mathf.Sqrt(Random.value)));
			else return Random.Range(0, decisionQueue.Count);
		}

		if (decisionQueue.Contains(dec)) decisionQueue.Remove(dec);
		int insertIndex = dec.doWithinTurns == null ? GetRandomIndex() : Mathf.Clamp(dec.doWithinTurns.GetWithinRange(), 0, decisionQueue.Count);
		//Debug.Log(string.Format("Inserting at {0}/{1}", insertIndex, decisionQueue.Count));
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
		AddToDecisionQueue(dec, true);
		return dec;
	}

	#endregion
}