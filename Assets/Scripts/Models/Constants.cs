using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RLConstants {
	public static char[] STRING_SPLIT_OR = { '/' };
	public static char[] STRING_SPLIT_AND = { ',' };
	public static char[] STRING_SPLIT_RANGE = { '-' };
	public static char[] TRIM_CHARS = { '\n', ' ' };
	public const char ROW_SEPARATOR = '\r', COLUMN_SEPARATOR = ',', TAG_BOLD = '*', TAG_ITALIC = '_';

	static List<string> _statNames;
	public static List<string> STAT_NAMES {
		get {
			if (_statNames == null) _statNames = new List<string>(new string[] { "Wealth", "Health", "Fun", "Learning" });
			return _statNames;
		}
	}
}