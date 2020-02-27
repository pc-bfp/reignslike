using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.Video;

public class GameManager : MonoBehaviour {
	public static GameManager Instance { get; private set; }

	[System.Serializable]
	public class StatHolder {
		[SerializeField] string statName;
		public StatDisplay display;
		public int Value {
			get { return display ? display.StatValue : 0; }
		}
		public float NormalizedValue {
			get { return display ? Mathf.InverseLerp(Instance.MinStatValue, Instance.MaxStatValue, display.StatValue) : 0; }
		}

		public void Initialize(int initValue) {
			if (!display) return;
			display.StatName = statName;
			display.SetValue(initValue);
		}
	}

	public GameState CurGameState { get; private set; }
	public int MinStatValue { get; set; }
	public int MaxStatValue { get; set; }

	public delegate void DecisionTakenEvent();
	public DecisionTakenEvent OnDecisionTaken;

	[SerializeField] TextAsset decisionsFile, endgameFile, imageSubmissionsFile, imageMapFile;
	[SerializeField] AudioInfoHolder audioInfo;
	[SerializeField] SetupDisplay setupDisplay;
	[SerializeField] DecisionDisplay decisionDisplay;
	[SerializeField] TurnsDisplay turnsDisplay;
	[SerializeField] Animator statsAnim;
	[SerializeField] List<StatHolder> statHolders;
	[SerializeField] VideoView videoPlayer;
	[SerializeField] VideoClip introClip, outroClip;
	[SerializeField] string prefTurns, prefStatMin, prefStatMax, prefStatStart;
	[SerializeField] bool playStoryOnlyOnce = true;
	[SerializeField] EndgameDisplay endgameDisplay;
	[SerializeField] float dramaticPause = 0.5f, timeBetweenStatUpdates = 0.25f;
	[SerializeField] AudioClip bgmClip;

	static bool introPlayed = false, outroPlayed = false;
	const string STATS_SHOW_BOOL_PARAM = "Show";

	DecisionHolder decisionsHolder;
	EndgameHolder endgameHolder;
	Decision curDecision;
	bool isDecisionMade = false, canShowNextOutcome = false, doPlayIntro = false, doPlayOutro = false;
	Decision.ButtonResult curResult;
	int curStatEffectIndex = -1;
	float curDecisionShownAt = 0, sessionStartTime = 0;

	Dictionary<string, object> analDecisionMade = new Dictionary<string, object>();
	Dictionary<string, object> analSessionEnd = new Dictionary<string, object>();

	bool DoShowStats {
		get { return statsAnim ? statsAnim.GetBool(STATS_SHOW_BOOL_PARAM) : false; }
		set { if (statsAnim) statsAnim.SetBool(STATS_SHOW_BOOL_PARAM, value); }
	}

	void Awake() {
		if (!Instance) {
			RLUtilities.Initialize();
			AudioManager.Initialize(audioInfo);
		}
		Instance = this;
		doPlayIntro = videoPlayer && introClip && !(playStoryOnlyOnce && introPlayed);
		doPlayOutro = videoPlayer && outroClip && !(playStoryOnlyOnce && outroPlayed);
		if (doPlayIntro) DoShowStats = false;
	}

	void Start() {
		if (doPlayIntro) {
			introPlayed = true;
			videoPlayer.Activate(introClip);
			videoPlayer.OnCompleted += Setup;
		}
		else {
			Setup();
		}
	}

	void Setup() {
		if (videoPlayer) videoPlayer.OnCompleted -= Setup;
		DoShowStats = true;
		if (setupDisplay) {
			setupDisplay.OnCompleted += Activate;
			setupDisplay.Initialize(statHolders);
		}
		else Activate();
	}

	void Activate() {
		CurGameState = new GameState() { stats = new int[statHolders.Count] };
		for (int s = 0; s < CurGameState.stats.Length; s++) CurGameState.stats[s] = statHolders[s].Value;
		ImagesHolder.Initialize(imageSubmissionsFile, imageMapFile);
		decisionsHolder = new DecisionHolder(decisionsFile);
		endgameHolder = new EndgameHolder(endgameFile);
		DecisionDisplay.OnDecisionMade += OnDecisionMade;
		DecisionDisplay.OnNextOutcome += NextOutcome;
		sessionStartTime = Time.timeSinceLevelLoad;
		StartCoroutine(ActivateCR());
	}

	IEnumerator ActivateCR() {
		yield return new WaitForSeconds(dramaticPause);
		turnsDisplay.Initialize(setupDisplay ? setupDisplay.NumTurns : 10);
		yield return new WaitForSeconds(turnsDisplay.AnimDelayTime);
		AudioManager.PlayBGM(bgmClip);
		NextDecision();
	}

	void NextDecision() {
		isDecisionMade = false;
		curDecision = decisionsHolder.GetDecision();
		decisionDisplay.ShowDecision(curDecision);
		curDecisionShownAt = Time.timeSinceLevelLoad;
	}

	void OnDecisionMade(int answerIndex) {
		if (isDecisionMade) return; // Safeguard against accidental double presses
		isDecisionMade = true;

		OnDecisionTaken?.Invoke();
		curResult = curDecision.buttonResults[answerIndex];
		curStatEffectIndex = -1;

		// Analytics event
		analDecisionMade["id"] = curDecision.decisionID;
		analDecisionMade["answer"] = answerIndex;
		analDecisionMade["turns_left"] = turnsDisplay.NumTurns;
		analDecisionMade["time_taken"] = Time.timeSinceLevelLoad - curDecisionShownAt;
		AddStatsToAnal(ref analDecisionMade);
		print(AnalyticsEvent.Custom("decision_made", analDecisionMade));

		bool unlocksChanged = false;    // Unused
		unlocksChanged |= !curResult.unlockAdd.TrueForAll(unlock => !CurGameState.ChangeUnlock(unlock, true));
		unlocksChanged |= !curResult.unlockRemove.TrueForAll(unlock => !CurGameState.ChangeUnlock(unlock, false));

		StartCoroutine(NextOutcomeCR());
	}

	void NextOutcome() {
		if (canShowNextOutcome) StartCoroutine(NextOutcomeCR());
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
			decisionDisplay.End(() => EndDecision());
		}
	}

	void EndDecision() {
		StartCoroutine(EndDecisionCR());
	}

	IEnumerator EndDecisionCR() {
		yield return new WaitForSeconds(dramaticPause);

		if (curDecision.turnCost > 0) {
			turnsDisplay.ReduceTurns(curDecision.turnCost);
			yield return new WaitForSeconds(turnsDisplay.AnimDelayTime);
		}

		if (turnsDisplay.NumTurns <= 0) {
			// Reign is ended
			analSessionEnd["turns_left"] = 0;
			analSessionEnd["last_decision"] = null;
			analSessionEnd["session_length"] = Time.timeSinceLevelLoad - sessionStartTime;
			AddStatsToAnal(ref analSessionEnd);
			AnalyticsEvent.LevelComplete(0, analSessionEnd);
			Analytics.FlushEvents();

			if (doPlayOutro) {
				outroPlayed = true;
				videoPlayer.Activate(outroClip);
				DoShowStats = false;
				videoPlayer.OnCompleted += ShowEndgame;
			}
			else ShowEndgame();
		}
		else NextDecision();
	}

	void ShowEndgame() {
		if (endgameDisplay) endgameDisplay.ShowResults(endgameHolder.GetResults());
		DoShowStats = true;
	}

	private void OnDestroy() {
		DecisionDisplay.OnDecisionMade -= OnDecisionMade;
		DecisionDisplay.OnNextOutcome -= NextOutcome;
		if (turnsDisplay.NumTurns > 0) {
			analSessionEnd["turns_left"] = turnsDisplay.NumTurns;
			analSessionEnd["last_decision"] = curDecision.decisionID;
			analSessionEnd["session_length"] = Time.timeSinceLevelLoad - sessionStartTime;
			AddStatsToAnal(ref analSessionEnd);
			AnalyticsEvent.LevelQuit(0, analSessionEnd);
			Analytics.FlushEvents();
		}
	}

	void AddStatsToAnal(ref Dictionary<string, object> analData) {
		for (int s = 0; s < statHolders.Count; s++) analData["stat_" + statHolders[s].display.StatName] = statHolders[s].NormalizedValue;
	}

#if UNITY_EDITOR
	private void OnGUI() {
		if (GUILayout.Button("Reset PlayerPrefs")) PlayerPrefs.DeleteAll();
	}
#endif
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