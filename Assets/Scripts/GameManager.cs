using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {
	[System.Serializable]
	class StatHolder {
		[SerializeField] string statName;
		[SerializeField] StatDisplay display;

		public int Value {
			get { return _value; }
			set {
				if (_value == value) return;
				_value = value;
				display.SetValue(_value);
			}
		}
		int _value;

		public void Initialize(int startingValue) {
			if (!display) return;
			display.StatName = statName;
			display.SetValue(startingValue);
		}
	}

	[SerializeField] TextAsset decisionsFile;
	[SerializeField] int initialStatValue = 5;
	[SerializeField] List<StatHolder> statHolders;

	DecisionsHolder decHolder;

	private void Awake() {
		decHolder = new DecisionsHolder(decisionsFile);
	}

	void Start() {
		statHolders.ForEach(sh => sh.Initialize(initialStatValue));
	}
	
	void Update() {

	}
}
