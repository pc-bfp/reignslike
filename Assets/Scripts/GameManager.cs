using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GameManager : MonoBehaviour {
	public static GameManager Instance { get; private set; }

	[System.Serializable]
	class StatHolder {
		[SerializeField] string statName;
		public StatDisplay display;
		public int Value {
			get { return display ? display.StatValue : 0; }
		}

		public void Initialize(int initValue) {
			if (!display) return;
			display.StatName = statName;
			display.SetValue(initValue);
		}
	}

	public GameState CurGameState { get; private set; }
	public int MinStatValue { get { return minStatValue; } }
	public int MaxStatValue { get { return maxStatValue; } }

	public delegate void DecisionTakenEvent();
	public DecisionTakenEvent OnDecisionTaken;

	[SerializeField] TextAsset decisionsFile, endgameFile;
	[SerializeField] int numTurns = 10, initialStatValue = 5, minStatValue = 0, maxStatValue = 10;
	[SerializeField] DecisionDisplay decisionDisplay;
	[SerializeField] TurnsDisplay turnsDisplay;
	[SerializeField] List<StatHolder> statHolders;
	[SerializeField] float dramaticPause = 0.5f, timeBetweenStatUpdates = 0.25f;

	DecisionsHolder decisionsHolder;
	EndgameHolder endgame;
	Decision curDecision;
	bool isDecisionMade = false;


	void Awake() {
		Instance = this;
	}

	void Start() {
		statHolders.ForEach(sh => sh.Initialize(initialStatValue));
		CurGameState = new GameState() { stats = new int[statHolders.Count] };
		for (int s = 0; s < CurGameState.stats.Length; s++) CurGameState.stats[s] = initialStatValue;
		decisionsHolder = new DecisionsHolder(decisionsFile);
		DecisionDisplay.OnDecisionMade += OnDecisionMade;
		StartCoroutine(StartCR());
	}

	IEnumerator StartCR() {
		yield return new WaitForSeconds(dramaticPause);
		turnsDisplay.Initialize(numTurns);
		yield return new WaitForSeconds(turnsDisplay.AnimTime);
		NextDecision();
	}

	void NextDecision() {
		isDecisionMade = false;
		curDecision = decisionsHolder.GetDecision();
		decisionDisplay.ShowDecision(curDecision);
	}

	void OnDecisionMade(int answerIndex) {
		if (isDecisionMade) return; // Safeguard against accidental double presses
		isDecisionMade = true;
		StartCoroutine(OnDecisionMadeCR(answerIndex));
	}

	IEnumerator OnDecisionMadeCR(int answerIndex) {
		yield return new WaitForSeconds(dramaticPause);

		Decision.ButtonResult result = curDecision.buttonResults[answerIndex];

		int[] statEffects = result.statEffects;
		for (int s = 0; s < statEffects.Length && s < statHolders.Count; s++) {
			if (statEffects[s] == 0) continue;
			if (s > 0) yield return new WaitForSeconds(timeBetweenStatUpdates);
			statHolders[s].display.SetValue(Mathf.Clamp(statHolders[s].Value + statEffects[s], MinStatValue, MaxStatValue), statEffects[s]);
			CurGameState.stats[s] = statHolders[s].Value;
		}
		bool unlocksChanged = false;	// Unused
		unlocksChanged |= !result.unlockAdd.TrueForAll(unlock => !CurGameState.ChangeUnlock(unlock, true));
		unlocksChanged |= !result.unlockRemove.TrueForAll(unlock => !CurGameState.ChangeUnlock(unlock, false));

		OnDecisionTaken?.Invoke();

		yield return new WaitForSeconds(dramaticPause);

		decisionDisplay.ShowOutcome(result.ResultText);
		yield return new WaitForSeconds(dramaticPause);

		while (!Input.GetMouseButton(0) && !Input.anyKey) yield return null;

		statHolders.ForEach(sh => sh.display.EndSetValue());
		bool hasEnded = false;
		decisionDisplay.End(() => hasEnded = true);
		while (!hasEnded) yield return null;

		if (curDecision.turnCost > 0) {
			turnsDisplay.ReduceTurns(curDecision.turnCost);
			yield return new WaitForSeconds(turnsDisplay.AnimTime);
		}

		if (turnsDisplay.NumTurns <= 0) {
			// We're in the endgame now
		}
		else NextDecision();
	}
}

[System.Serializable]
public class GameState {
	public int[] stats;
	public List<string> unlocks = new List<string>();

	// Returns true if unlocks were changed
	public bool ChangeUnlock(string unlock, bool add) {
		if (add && !unlocks.Contains(unlock)) unlocks.Add(unlock);
		else if (!add && unlocks.Contains(unlock)) unlocks.Remove(unlock);
		else return false;
		return true;
	}
}