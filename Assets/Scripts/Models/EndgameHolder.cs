using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Endgame {
	public class MinMaxStat {
		public int statIndex, minValue, maxValue;
	}
	public List<MinMaxStat> minMaxStats;
	public List<string> summaries, hints, learning;

	public Endgame() {
		minMaxStats = new List<MinMaxStat>();
		summaries = new List<string>();
		hints = new List<string>();
		learning = new List<string>();
	}
}


public class EndgameHolder {
	List<Endgame> endgames;

	public EndgameHolder(TextAsset endgameFile, int minStat, int maxStat) {
		string allEndgamesStr = endgameFile.text.Replace("\r", "");
		string[] endgameStrings = allEndgamesStr.Split(new string[] { Constants.ROW_SPLIT }, System.StringSplitOptions.RemoveEmptyEntries);

		endgames = new List<Endgame>();
		int numStats = 0;
		for (int e = 0; e < endgameStrings.Length; e++) {
			string[] egFields = endgameStrings[e].Split(new string[] { Constants.COLUMN_SPLIT }, System.StringSplitOptions.None);

			if (e == 0) {
				for (int eg = 0; egFields[eg] != "Summary"; eg++) numStats++;
				continue;
			}

			Endgame curEG = new Endgame();
			
			for (int eg = 0; eg < egFields.Length; eg++) {
				string curField = egFields[0].Trim();
				if (string.IsNullOrEmpty(curField)) continue;

				if (eg < numStats) {
					// Get stats
					curField = curField.Replace(" ", "");
					Endgame.MinMaxStat curMinMax = new Endgame.MinMaxStat() {
						minValue = minStat,
						maxValue = maxStat,
						statIndex = eg
					};
					bool isValid = false;
					if (curField.Contains("<")) isValid = int.TryParse(curField.Substring(curField.IndexOf("<") + 1), out curMinMax.maxValue);
					else if (curField.Contains(">")) isValid = int.TryParse(curField.Substring(curField.IndexOf(">") + 1), out curMinMax.minValue);
					else if (curField.Contains("-")) {
						isValid = int.TryParse(curField.Substring(0, curField.IndexOf("-")), out curMinMax.minValue)
							&& int.TryParse(curField.Substring(curField.IndexOf("-") + 1), out curMinMax.maxValue);
					}
					else continue;
				}
				else {
					// Get summaries, hints, learning
				}
			}
		}
	}
}
