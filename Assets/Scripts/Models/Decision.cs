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
	public class StatEffect {
		[System.Serializable]
		public class StatChange {
			public int statIndex, statChange;

			public StatChange(string statChangeText) {
				string statChangeStr = string.Empty;
				for (int sn = 0; sn < RLConstants.STAT_NAMES.Count; sn++) {
					string statName = RLConstants.STAT_NAMES[sn];
					if (statChangeText.StartsWith(statName)) {
						statIndex = sn;
						statChangeStr = statChangeText.Replace(statName, "").Trim();
						break;
					}
				}
				statChange = int.Parse(statChangeStr, System.Globalization.NumberStyles.AllowLeadingSign);
			}
		}

		public List<StatChange> statChanges = new List<StatChange>();
		public string EffectText {
			get { return effectTexts.Count == 0 ? string.Empty : effectTexts[Random.Range(0, effectTexts.Count)]; }
		}
		public Sprite EffectImage { get; private set; }

		List<string> effectTexts;

		private StatEffect() { }

		public static StatEffect Create(string fieldText, Sprite image) {
			if (string.IsNullOrEmpty(fieldText)) return null;
			StatEffect retval = new StatEffect();
			int splitIndex = fieldText.IndexOf('\n');
			
			foreach (string scText in fieldText.Substring(0, splitIndex).Split(RLConstants.STRING_SPLIT_AND))
				retval.statChanges.Add(new StatChange(scText.Trim()));

			retval.effectTexts = new List<string>(fieldText.Substring(splitIndex + 1).Split(RLConstants.STRING_SPLIT_OR, System.StringSplitOptions.RemoveEmptyEntries));
			for (int e = 0; e < retval.effectTexts.Count; e++) retval.effectTexts[e] = RLUtilities.ApplyBoldItalic(retval.effectTexts[e]);

			retval.EffectImage = image;
			return retval;
		}
	}

	[System.Serializable]
	public class ButtonResult {
		public string buttonText;
		public List<string> unlockAdd, unlockRemove;
		public List<StatEffect> statEffects;

		public ButtonResult(string buttonText, string unlocks, List<string> statEffectFields, List<Sprite> statImages) {
			this.buttonText = buttonText;
			unlockAdd = new List<string>();
			unlockRemove = new List<string>();
			foreach (string unlock in unlocks.Split(RLConstants.STRING_SPLIT_AND, System.StringSplitOptions.RemoveEmptyEntries)) {
				if (unlock.StartsWith("-")) unlockRemove.Add(unlock.Remove(0, 1));
				else unlockAdd.Add(unlock);
			}
			statEffects = new List<StatEffect>();
			for (int sf = 0; sf < statEffectFields.Count; sf++) {
				StatEffect curSE = StatEffect.Create(statEffectFields[sf], statImages != null && sf < statImages.Count ? statImages[sf] : null);
				if (curSE != null) statEffects.Add(curSE);
			}
		}
	}

	public string decisionID, decisionText;
	public Sprite decisionImage;
	public List<ButtonResult> buttonResults = new List<ButtonResult>();
	public IntRange[] statRequirements;
	public List<string> unlockedRequirements = new List<string>(), lockedRequirements = new List<string>();
	public IntRange doWithinTurns = null;
	public int turnCost = 1;

	public bool IsRecurring { get; private set; }

	const string TURNS_REQ = "Turns:";
	static string[] LT_GT = { "<", ">" };

	public void SetRequirements(string requirements) {
		if (string.IsNullOrEmpty(requirements)) return;
		statRequirements = new IntRange[RLConstants.STAT_NAMES.Count];
		for (int s = 0; s < RLConstants.STAT_NAMES.Count; s++) statRequirements[s] = new IntRange(GameManager.Instance.MinStatValue - 1, GameManager.Instance.MaxStatValue + 1);

		foreach (string req in requirements.Split(RLConstants.STRING_SPLIT_AND, System.StringSplitOptions.RemoveEmptyEntries)) {
			string curReq = req.Trim();
			if (curReq.Contains("<") || curReq.Contains(">")) { // Stat requirement
				string[] statReqSplit = curReq.Split(LT_GT, System.StringSplitOptions.None);
				for (int i = 0; i < 2; i++) statReqSplit[i] = statReqSplit[i].Trim();

				int statVal = 0;
				if (!int.TryParse(statReqSplit[1], out statVal)) continue;
				int statIndex = RLConstants.STAT_NAMES.IndexOf(statReqSplit[0].Trim());
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