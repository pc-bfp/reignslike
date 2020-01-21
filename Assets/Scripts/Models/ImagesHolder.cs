using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class ImagesHolder {
	[System.Serializable]
	public class DecisionImages {
		public string decisionID;
		public Sprite decisionImage;
		public List<Sprite>[] resultImages;
	}

	static Dictionary<string, Sprite> spriteIDLookup;
	static Dictionary<string, DecisionImages> decisionLookup;

	const string REMOVE_URL_UPTO = "id=", IMAGE_LOCATION = "Doodles/";

	public static void Initialize(TextAsset submissions, TextAsset mapping) {
		string[][] submissionStrings = RLUtilities.ReadSheet(submissions.text),
				   mapStrings = RLUtilities.ReadSheet(mapping.text);
		
		spriteIDLookup = new Dictionary<string, Sprite>();
		decisionLookup = new Dictionary<string, DecisionImages>();

		for (int r = 1; r < mapStrings.Length; r++) {
			string[] mapRow = mapStrings[r];
			string spriteName = mapRow[0];
			int fileExtIndex = spriteName.LastIndexOf('.');
			if (fileExtIndex >= 0) spriteName = spriteName.Remove(fileExtIndex);
			Sprite mapSprite = Resources.Load<Sprite>(IMAGE_LOCATION + spriteName);
			if (mapSprite) spriteIDLookup[mapRow[1]] = mapSprite;
		}

		// Process the submissions

		for (int r = 1; r < submissionStrings.Length; r++) {
			string[] subRow = submissionStrings[r];
			if (string.IsNullOrEmpty(subRow[0])) continue;

			DecisionImages curDI = new DecisionImages() {
				decisionID = subRow[1],
				resultImages = new List<Sprite>[Mathf.CeilToInt((subRow.Length - 3) / (float)RLConstants.STAT_NAMES.Count)]
			};
			for (int ri = 0; ri < curDI.resultImages.Length; ri++) curDI.resultImages[ri] = new List<Sprite>();

			for (int i = 2; i < subRow.Length; i++) {
				if (string.IsNullOrEmpty(subRow[i])) continue;
				try {
					subRow[i] = subRow[i].Remove(0, subRow[i].IndexOf(REMOVE_URL_UPTO) + REMOVE_URL_UPTO.Length); // URL into sprite ID
				} catch (System.Exception) {
					Debug.LogError(subRow[i]);
				}
				Sprite curSprite = GetSprite(subRow[i]);
				if (!curSprite) continue;
				if (i == 2) curDI.decisionImage = curSprite;
				else curDI.resultImages[(i - 3) / RLConstants.STAT_NAMES.Count].Add(curSprite);
			}

			decisionLookup[curDI.decisionID] = curDI;
		}

		Debug.Log("Processed submissions");
	}


	public static Sprite GetSprite(string spriteID) {
		return spriteIDLookup.ContainsKey(spriteID) ? spriteIDLookup[spriteID] : null;
	}

	public static DecisionImages GetImages(string decisionID) {
		return decisionLookup.ContainsKey(decisionID) ? decisionLookup[decisionID] : null;
	}
}
