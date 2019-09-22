using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RLConstants {
	public static char[] STRING_SPLIT_OR = { '/' };
	public static char[] STRING_SPLIT_AND = { ',' };
	public static char[] STRING_SPLIT_RANGE = { '-' };
}

public class SheetReader {
	static char[] ROW_SPLIT = { '\r' }, COLUMN_SPLIT = { '\t' };
	static char[] TRIM_CHARS = { '\n', ' ' };

	public static string[][] ReadSheet(string sheetString) {
		if (string.IsNullOrEmpty(sheetString)) return null;

		string[] rowStrings = sheetString.Split(ROW_SPLIT, StringSplitOptions.RemoveEmptyEntries);
		string[][] retval = new string[rowStrings.Length][];

		for (int r = 0; r < rowStrings.Length; r++) {
			retval[r] = rowStrings[r].Split(COLUMN_SPLIT, StringSplitOptions.None);
			for (int f = 0; f < retval[r].Length; f++)
				retval[r][f] = retval[r][f].Trim(TRIM_CHARS);
		}

		return retval;
	}
}

public class RLUtilities {
	public static T RandomFromList<T>(List<T> list) {
		return list[UnityEngine.Random.Range(0, list.Count)];
	}

	public static void ResetGame() {
		SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
	}
}