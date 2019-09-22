using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Decision {
	
	[System.Serializable]
	public class IntRange {
		public int min, max;

		public IntRange(int min = -1, int max = -1) {
			this.min = min;
			this.max = max;
		}

		public bool IsWithinRange(int val) { return val > min && val < max; }
		public int GetWithinRange() { return Random.Range(min, max + 1); }
	}

	[System.Serializable]
	public class ButtonResult {
		public string buttonText;
		public List<string> resultTexts, unlockAdd, unlockRemove;
		public int[] statEffects;
		public string ResultText {
			get { return resultTexts.Count == 0 ? string.Empty : resultTexts[Random.Range(0, resultTexts.Count)]; }
		}

		public ButtonResult(string button, string result, string unlocks, int[] statEffects) {
			buttonText = button;
			resultTexts = new List<string>(result.Split(RLConstants.STRING_SPLIT_OR, System.StringSplitOptions.RemoveEmptyEntries));
			unlockAdd = new List<string>();
			unlockRemove = new List<string>();
			foreach (string unlock in unlocks.Split(RLConstants.STRING_SPLIT_AND, System.StringSplitOptions.RemoveEmptyEntries)) {
				if (unlock.StartsWith("-")) unlockRemove.Add(unlock.Remove(0, 1));
				else unlockAdd.Add(unlock);
			}
			this.statEffects = statEffects;
		}
	}

	public string decisionText;
	public List<ButtonResult> buttonResults = new List<ButtonResult>();
	public IntRange[] statRequirements;
	public List<string> unlockedRequirements = new List<string>(), lockedRequirements = new List<string>();
	public IntRange doWithinTurns = null;
	public int turnCost = 1;

	public bool IsRecurring { get; private set; }

	const string TURNS_REQ = "Turns:";
	static string[] ltgt = { "<", ">" };

	public void SetRequirements(string requirements, List<string> stats) {
		if (string.IsNullOrEmpty(requirements)) return;
		statRequirements = new IntRange[stats.Count];
		for (int s = 0; s < stats.Count; s++) statRequirements[s] = new IntRange(GameManager.Instance.MinStatValue - 1, GameManager.Instance.MaxStatValue + 1);

		foreach (string req in requirements.Split(RLConstants.STRING_SPLIT_AND, System.StringSplitOptions.RemoveEmptyEntries)) {
			string curReq = req.Trim();
			if (curReq.Contains("<") || curReq.Contains(">")) { // Stat requirement
				string[] statReqSplit = curReq.Split(ltgt, System.StringSplitOptions.None);
				for (int i = 0; i < 2; i++) statReqSplit[i] = statReqSplit[i].Trim();

				int statVal = 0;
				if (!int.TryParse(statReqSplit[1], out statVal)) continue;
				int statIndex = stats.IndexOf(statReqSplit[0].Trim());
				if (statIndex < 0) continue;
				if (curReq.Contains(">")) statRequirements[statIndex].min = statVal;
				else statRequirements[statIndex].max = statVal;
			}

			else if (curReq.StartsWith(TURNS_REQ)) {    // Do within turns
				curReq = curReq.Remove(0, TURNS_REQ.Length).Trim();
				string[] turnsMinMax = curReq.Split(RLConstants.STRING_SPLIT_RANGE);
				try {
					int minTurns = int.Parse(turnsMinMax[0]),
						maxTurns = turnsMinMax.Length > 1 ? int.Parse(turnsMinMax[1]) : minTurns;
					doWithinTurns = new IntRange(minTurns, maxTurns);
				} catch (System.Exception) { }
			}

			else {  // Unlocked or locked requirement
				if (curReq.StartsWith("-")) {
					curReq = curReq.Substring(1).TrimStart();
					lockedRequirements.Add(curReq);
					IsRecurring |= !buttonResults.TrueForAll(br => !br.unlockRemove.Contains(curReq));
				}
				else {
					unlockedRequirements.Add(curReq);
					IsRecurring |= !buttonResults.TrueForAll(br => !br.unlockAdd.Contains(curReq));
				}
				// It's recurring if an unlock requirement matches a button unlock result
			}
		}
	}

	public bool IsAvailable() {
		bool retval = true;
		if (statRequirements != null) {
			for (int i = 0; retval && i < GameManager.Instance.CurGameState.stats.Length; i++) {
				retval &= statRequirements[i].IsWithinRange(GameManager.Instance.CurGameState.stats[i]);
			}
		}
		retval &= unlockedRequirements.TrueForAll(req => GameManager.Instance.CurGameState.unlocks.Contains(req));
		retval &= lockedRequirements.TrueForAll(req => !GameManager.Instance.CurGameState.unlocks.Contains(req));
		return retval;
	}
}