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
				display.SetValue(value, value - _value);
				_value = value;
			}
		}
		int _value;

		public void Initialize(int startingValue) {
			if (!display) return;
			display.StatName = statName;
			display.SetValue(_value = startingValue);
		}

		public void EndSetValue() {
			if (display) display.EndSetValue();
		}
	}

	[SerializeField] TextAsset decisionsFile;
	[SerializeField] int initialStatValue = 5, minStatValue = 0, maxStatValue = 10;
	[SerializeField] DecisionDisplay decisionDisplay;
	[SerializeField] List<StatHolder> statHolders;
	[SerializeField] float dramaticPause = 0.5f, timeBetweenStatUpdates = 0.25f;

	DecisionsHolder decHolder;
	Decision curDecision;
	bool isDecisionMade = false;

	private void Awake() {
		decHolder = new DecisionsHolder(decisionsFile);
		DecisionDisplay.OnDecisionMade += OnDecisionMade;
	}

	IEnumerator Start() {
		statHolders.ForEach(sh => sh.Initialize(initialStatValue));
		yield return new WaitForSeconds(dramaticPause);
		NextDecision();
	}

	void NextDecision() {
		isDecisionMade = false;
		curDecision = decHolder.GetDecision();
		decisionDisplay.ShowDecision(curDecision.decisionText);
	}

	void OnDecisionMade(bool yes) {
		if (isDecisionMade) return;
		isDecisionMade = true;
		StartCoroutine(OnDecisionMadeCR(yes));
	}

	IEnumerator OnDecisionMadeCR(bool yes) {
		yield return new WaitForSeconds(dramaticPause);

		int[] statEffects = yes ? curDecision.statEffectsYes : curDecision.statEffectsNo;
		for (int s = 0; s < statEffects.Length && s < statHolders.Count; s++) {
			if (statEffects[s] == 0) continue;
			if (s > 0) yield return new WaitForSeconds(timeBetweenStatUpdates);
			statHolders[s].Value = Mathf.Clamp(statHolders[s].Value + statEffects[s], minStatValue, maxStatValue);
		}
		yield return new WaitForSeconds(dramaticPause);

		decisionDisplay.ShowOutcome(yes ? curDecision.resultYes : curDecision.resultNo);
		yield return new WaitForSeconds(dramaticPause);

		while (!Input.GetMouseButton(0) && !Input.anyKey) yield return null;

		statHolders.ForEach(sh => sh.EndSetValue());
		bool hasEnded = false;
		decisionDisplay.End(() => hasEnded = true);
		while (!hasEnded) yield return null;

		yield return new WaitForSeconds(dramaticPause);
		NextDecision();
	}
}
