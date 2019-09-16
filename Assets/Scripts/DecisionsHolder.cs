using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Decision {
	public string decisionText, resultYes, resultNo;
	public int[] statEffectsYes, statEffectsNo;
}

public class DecisionsHolder : MonoBehaviour {
	[SerializeField] TextAsset decisionsFile;
	const string ROW_SPLIT = "\n", COLUMN_SPLIT = "\t";

	List<Decision> decisions;

	void Start() {
		string allDecisionsStr = decisionsFile.text.Replace("\r", "");
		string[] decisionStrings = allDecisionsStr.Split(new string[] { ROW_SPLIT }, System.StringSplitOptions.RemoveEmptyEntries);
		decisions = new List<Decision>();
		int numStatEffects = 0;

		for (int d = 0; d < decisionStrings.Length; d++) {
			string[] decFields = decisionStrings[d].Split(new string[] { COLUMN_SPLIT }, System.StringSplitOptions.None);
			if (d == 0) {    // Column names
				bool countingValues = false;
				for (int i = 0; i < decFields.Length; i++) {
					if (decFields[i].StartsWith("Yes:")) countingValues = true;
					else if (decFields[i].StartsWith("No:")) countingValues = false;
					else if (countingValues) numStatEffects++;
				}
				continue;
			}

			Decision curDec = new Decision() {
				decisionText = decFields[0],
				resultYes = decFields[1],
				resultNo = decFields[numStatEffects + 2],
				statEffectsYes = new int[numStatEffects],
				statEffectsNo = new int[numStatEffects]
			};
			for (int i = 0; i < numStatEffects; i++) {
				int.TryParse(decFields[2 + i], out curDec.statEffectsYes[i]);
				int.TryParse(decFields[3 + i + numStatEffects], out curDec.statEffectsNo[i]);
			}
			decisions.Add(curDec);
		}
	}
}