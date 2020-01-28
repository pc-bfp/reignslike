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

	[SerializeField] TextAsset decisionsFile, endgameFile, imageSubmissionsFile, imageMapFile;
	[SerializeField] AudioInfoHolder audioInfo;
	[SerializeField] int numTurns = 10, editorNumTurns = 10, initialStatValue = 5, minStatValue = 0, maxStatValue = 10;
	[SerializeField] DecisionDisplay decisionDisplay;
	[SerializeField] TurnsDisplay turnsDisplay;
	[SerializeField] GameObject statsObject;
	[SerializeField] List<StatHolder> statHolders;
	[SerializeField] StoryView introDisplay, outroDisplay;
	[SerializeField] EndgameDisplay endgameDisplay;
	[SerializeField] float dramaticPause = 0.5f, timeBetweenStatUpdates = 0.25f;
	[SerializeField] AudioClip bgmClip;

	DecisionHolder decisionsHolder;
	EndgameHolder endgameHolder;
	Decision curDecision;
	bool isDecisionMade = false, canShowNextOutcome = false;
	Decision.ButtonResult curResult;
	int curStatEffectIndex = -1;

	void Awake() {
		RLUtilities.Initialize();
		AudioManager.Initialize(audioInfo);
		Instance = this;
		if (introDisplay) {
			decisionDisplay.gameObject.SetActive(false);
			statsObject.SetActive(false);
		}
	}

	void Start() {
		if (introDisplay) {
			introDisplay.Begin();
			introDisplay.OnCompleted += Activate;
		}
		else Activate();
	}

	void Activate() {
		decisionDisplay.gameObject.SetActive(true);
		statsObject.SetActive(true);
		statHolders.ForEach(sh => sh.Initialize(initialStatValue));
		CurGameState = new GameState() { stats = new int[statHolders.Count] };
		for (int s = 0; s < CurGameState.stats.Length; s++) CurGameState.stats[s] = initialStatValue;
		ImagesHolder.Initialize(imageSubmissionsFile, imageMapFile);
		decisionsHolder = new DecisionHolder(decisionsFile);
		endgameHolder = new EndgameHolder(endgameFile);
		DecisionDisplay.OnDecisionMade += OnDecisionMade;
		DecisionDisplay.OnNextOutcome += () => {
			if (canShowNextOutcome) StartCoroutine(NextOutcomeCR());
		};
		StartCoroutine(StartCR());
	}

	IEnumerator StartCR() {
		yield return new WaitForSeconds(dramaticPause);
		turnsDisplay.Initialize(Application.isEditor ? editorNumTurns : numTurns);
		yield return new WaitForSeconds(turnsDisplay.AnimTime);
		NextDecision();
	}

	void NextDecision() {
		isDecisionMade = false;
		curDecision = decisionsHolder.GetDecision();
		decisionDisplay.ShowDecision(curDecision);
		AudioManager.PlayBGM(bgmClip);
	}

	void OnDecisionMade(int answerIndex) {
		if (isDecisionMade) return; // Safeguard against accidental double presses
		isDecisionMade = true;
		OnDecisionTaken?.Invoke();
		curResult = curDecision.buttonResults[answerIndex];
		curStatEffectIndex = -1;

		bool unlocksChanged = false;    // Unused
		unlocksChanged |= !curResult.unlockAdd.TrueForAll(unlock => !CurGameState.ChangeUnlock(unlock, true));
		unlocksChanged |= !curResult.unlockRemove.TrueForAll(unlock => !CurGameState.ChangeUnlock(unlock, false));

		StartCoroutine(NextOutcomeCR());
	}


	IEnumerator NextOutcomeCR() {
		canShowNextOutcome = false;
		if (curStatEffectIndex >= 0) statHolders.ForEach(sh => sh.display.EndSetValue());
		curStatEffectIndex++;

		if (curStatEffectIndex < curResult.statEffects.Count) {
			// Next stat effect
			decisionDisplay.HideOutcome();
			yield return new WaitForSeconds(timeBetweenStatUpdates);

			Decision.StatEffect statEffect = curResult.statEffects[curStatEffectIndex];
			foreach (var sc in statEffect.statChanges) {
				statHolders[sc.statIndex].display.SetValue(Mathf.Clamp(statHolders[sc.statIndex].Value + sc.statChange, MinStatValue, MaxStatValue), sc.statChange);
				yield return new WaitForSeconds(timeBetweenStatUpdates);
				CurGameState.stats[sc.statIndex] = statHolders[sc.statIndex].Value;
			}
			decisionDisplay.ShowOutcome(statEffect.EffectText, statEffect.EffectImage);
			canShowNextOutcome = true;
		}
		else {
			// Decision end
			yield return new WaitForSeconds(timeBetweenStatUpdates);
			decisionDisplay.End(() => StartCoroutine(EndDecisionCR()));
		}
	}

	IEnumerator EndDecisionCR() {
		yield return new WaitForSeconds(dramaticPause);

		if (curDecision.turnCost > 0) {
			turnsDisplay.ReduceTurns(curDecision.turnCost);
			yield return new WaitForSeconds(turnsDisplay.AnimTime);
		}

		if (turnsDisplay.NumTurns <= 0) {
			if (outroDisplay) {
				outroDisplay.Begin();
				outroDisplay.OnCompleted += ShowEndgame;
			}
			else ShowEndgame();
		}
		else NextDecision();
	}

	void ShowEndgame() {
		if (endgameDisplay) endgameDisplay.ShowResults(endgameHolder.GetResults());
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