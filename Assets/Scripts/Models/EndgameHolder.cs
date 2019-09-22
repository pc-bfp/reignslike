using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Endgame {
	
	[System.Serializable]
	public class MinMaxStat : System.IEquatable<MinMaxStat> {
		public int statIndex, minValue, maxValue;
		public bool minInclusive = false, maxInclusive = false;

		public MinMaxStat(int statIndex) {
			this.statIndex = statIndex;
			minValue = GameManager.Instance.MinStatValue - 1;
			maxValue = GameManager.Instance.MaxStatValue + 1;
		}

		public bool IsWithinBounds() {
			return GameManager.Instance.CurGameState.stats[statIndex] > minValue - (minInclusive ? 1 : 0) && GameManager.Instance.CurGameState.stats[statIndex] < maxValue + (maxInclusive ? 1 : 0);
		}

		#region IEquatable_IMPLEMENTATION

		public bool Equals(MinMaxStat other) {
			return other != null &&
				   statIndex == other.statIndex &&
				   minValue == other.minValue &&
				   maxValue == other.maxValue;
		}

		public override int GetHashCode() {
			var hashCode = -603274273;
			hashCode = hashCode * -1521134295 + statIndex.GetHashCode();
			hashCode = hashCode * -1521134295 + minValue.GetHashCode();
			hashCode = hashCode * -1521134295 + maxValue.GetHashCode();
			return hashCode;
		}

		#endregion
	}

	public List<string> summaries = new List<string>(), hints = new List<string>(), learning = new List<string>();
	public List<MinMaxStat> statConditions = new List<MinMaxStat>(), unlockPath = new List<MinMaxStat>();

	public bool UnlockPathComplete {
		get { return unlockPathProgress >= unlockPath.Count; }
	}
	public bool StatConditionsMet {
		get { return statConditions.TrueForAll(sc => sc.IsWithinBounds()); }
	}

	int unlockPathProgress = 0;

	// Returns true if unlocked
	public void UpdateUnlockPath() {
		if (unlockPath.Count == 0 || UnlockPathComplete) return;
		for (int i = unlockPathProgress; i < unlockPath.Count; i++) {
			if (unlockPath[i].IsWithinBounds()) unlockPathProgress = i + 1;
			else break;
		}
	}
	
	//public enum CompareResult { UNRELATED, OVERRIDE, OVERRIDDEN }

	//public CompareResult DoOverride(Endgame other) {
	//	CompareResult retval = CompareResult.UNRELATED;
	//	if (statConditions.Count != other.statConditions.Count) {
	//		Dictionary<int, KeyValuePair<MinMaxStat, MinMaxStat>> bothSCs = new Dictionary<int, KeyValuePair<MinMaxStat, MinMaxStat>>();
	//		foreach (MinMaxStat sc in statConditions) {
	//			if (!bothSCs.ContainsKey(sc.statIndex)) bothSCs[sc.statIndex] = new KeyValuePair<MinMaxStat, MinMaxStat>(sc, null);
	//			else bothSCs[sc.statIndex] = new KeyValuePair<MinMaxStat, MinMaxStat>(sc, bothSCs[sc.statIndex].Value);
	//		}
	//	}
	//	return retval;
	//}
}


public class EndgameResults {
	public Dictionary<string, string> summaryHintMap = new Dictionary<string, string>();
	public List<string> learning = new List<string>();
}

[System.Serializable]
public class EndgameHolder {
	List<Endgame> endgames;

	public EndgameHolder(TextAsset endgameFile) {
		string[][] allEndgameRows = SheetReader.ReadSheet(endgameFile.text);
		endgames = new List<Endgame>();
		int numStats = DecisionHolder.StatNames.Count;
		
		Endgame.MinMaxStat ParseRange(int statIndex, string range) {
			if (string.IsNullOrEmpty(range)) return null;
			bool isValid = false;
			Endgame.MinMaxStat retval = new Endgame.MinMaxStat(statIndex);
			range = range.Replace(" ", "");
			if (range.Contains("<")) isValid = int.TryParse(range.Substring(range.IndexOf("<") + 1), out retval.maxValue);
			else if (range.Contains(">")) isValid = int.TryParse(range.Substring(range.IndexOf(">") + 1), out retval.minValue);
			else if (range.Contains("-")) {
				isValid = int.TryParse(range.Substring(0, range.IndexOf("-")), out retval.minValue)
					&& int.TryParse(range.Substring(range.IndexOf("-") + 1), out retval.maxValue);
				retval.minInclusive = retval.maxInclusive = true;
			}
			return isValid ? retval : null;
		}

		char[] upSplitChars = new char[] { ' ', '<', '>' };

		for (int r = 1; r < allEndgameRows.Length; r++) {
			Endgame curEG = new Endgame();

			for (int eg = 0; eg < allEndgameRows[r].Length; eg++) {
				string curField = allEndgameRows[r][eg].Trim();
				if (string.IsNullOrEmpty(curField)) continue;

				if (eg < numStats) {
					// Stat requirements
					Endgame.MinMaxStat curMinMax = ParseRange(eg, curField);
					if (curMinMax == null) continue;
					curEG.statConditions.Add(curMinMax);
				}
				else if (eg == numStats) {
					// Unlock path
					string[] splitUP = curField.Split(RLConstants.STRING_SPLIT_AND, System.StringSplitOptions.RemoveEmptyEntries);
					foreach (string untrimmedUP in splitUP) {
						string up = untrimmedUP.Trim();
						int rangeStartIndex = up.IndexOfAny(upSplitChars);
						string statName = up.Substring(0, rangeStartIndex);
						Endgame.MinMaxStat curUP = ParseRange(DecisionHolder.StatNames.IndexOf(statName), up.Substring(rangeStartIndex));
						if (curUP != null) curEG.unlockPath.Add(curUP);
					}
				}
				else {
					List<string> stringList = new List<string>(curField.Split(RLConstants.STRING_SPLIT_OR, System.StringSplitOptions.RemoveEmptyEntries));
					switch (eg - numStats) {
					case 1: // Summary
						curEG.summaries = stringList;
						break;
					case 2: // Hint
						curEG.hints = stringList;
						break;
					case 3: // Learning outcomes
						curEG.learning = stringList;
						break;
					default: // ???
						break;
					}
				}
			}

			if (curEG.statConditions.Count > 0 || curEG.unlockPath.Count > 0) endgames.Add(curEG);
		}

		GameManager.Instance.OnDecisionTaken += () => endgames.ForEach(endgame => endgame.UpdateUnlockPath());
	}


	public EndgameResults GetResults() {
		EndgameResults retval = new EndgameResults();
		List<Endgame> validEndgames = endgames.FindAll(eg => eg.StatConditionsMet && eg.UnlockPathComplete);

		int maxNumStats = 0;

		// Unlock Paths and Learning Outcomes are easy, just get all that apply
		validEndgames.ForEach(eg => {
			maxNumStats = Mathf.Max(maxNumStats, eg.statConditions.Count);
			if (eg.learning.Count > 0) retval.learning.Add(RLUtilities.RandomFromList(eg.learning));
		});

		// Stat-based endgames are a bit trickier though
		List<Endgame> finalStats = new List<Endgame>();
		List<Endgame>[] statResults = new List<Endgame>[maxNumStats + 1];
		for (int i = 0; i < statResults.Length; i++) statResults[i] = new List<Endgame>();

		// Sort endgames in descending order of statConditions.Count, for an ordered comparison
		// Note: HORRIBLY INEFFICIENT! There's gotta be a better way...
		validEndgames.ForEach(eg => {
			statResults[eg.statConditions.Count].Add(eg);
		});
		HashSet<Endgame> removeEGs = new HashSet<Endgame>();
		for (int r = 2; r < statResults.Length; r++) {
			if (statResults[r].Count == 0 || statResults[r - 1].Count == 0) continue;
			List<Endgame> curStatResults = statResults[r], lesserStatResults = statResults[r - 1];
			foreach (Endgame curEG in curStatResults) {
				removeEGs.Clear();
				for (int lessR = r - 1; lessR >= 1; lessR--) statResults[lessR].ForEach(lessEG => {
					if (lessEG.statConditions.TrueForAll(sc => curEG.statConditions.Contains(sc)))
						removeEGs.Add(lessEG);
				});
				foreach (Endgame eg in removeEGs) statResults[r - 1].Remove(eg);
			}
			statResults[r].Reverse(); // Put back in spreadsheet order (1)
		}

		foreach (var sr in statResults) finalStats.AddRange(sr);
		finalStats.Reverse(); // Put back in spreadsheet order (2)

		foreach (Endgame eg in finalStats) {
			if (eg.summaries.Count > 0) {
				string curSummary = RLUtilities.RandomFromList(eg.summaries),
					   curHint = eg.hints.Count > 0 ? RLUtilities.RandomFromList(eg.hints) : null;
				retval.summaryHintMap[curSummary] = curHint;
			}
		}

		return retval;
	}
}
