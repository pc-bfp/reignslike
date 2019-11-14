using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RLSaveData {
	static RLSaveData Instance {
		get {
			if (_instance == null) {
				if (PlayerPrefs.HasKey(SAVE_PREF_NAME))
					_instance = JsonUtility.FromJson<RLSaveData>(PlayerPrefs.GetString(SAVE_PREF_NAME));
				else
					_instance = new RLSaveData();
			}
			return _instance;
		}
	}
	static RLSaveData _instance;

	const string SAVE_PREF_NAME = "Savedata";

	[SerializeField] StatDisplayType statDisplay;
	[SerializeField] int numTurns, startingStatValue;
	
	private RLSaveData() {
		statDisplay = StatDisplayType.NUMBER;
		numTurns = 10;
		startingStatValue = 5;
	}

	public static StatDisplayType StatDisplay {
		get { return Instance.statDisplay; }
		set {
			Instance.statDisplay = value;
			UpdateSave();
		}
	}
	public static int NumTurns {
		get { return Instance.numTurns; }
		set {
			Instance.numTurns = value;
			UpdateSave();
		}
	}
	public static int StartingStat {
		get { return Instance.startingStatValue; }
		set {
			Instance.startingStatValue = value;
			UpdateSave();
		}
	}

	private static void UpdateSave() {
		PlayerPrefs.SetString(SAVE_PREF_NAME, JsonUtility.ToJson(Instance));
	}
}