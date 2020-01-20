using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetupDisplay : MonoBehaviour {
	[SerializeField] List<int> turnsList, startValueList;
	[SerializeField] List<string> statTypeList;

	public delegate void ListValueChanged<T>(ref List<T> list, T newValue);

	void Start() {

	}

	void AdjustListValue<T>(ref List<T> list, bool increment) {
		
	}
}