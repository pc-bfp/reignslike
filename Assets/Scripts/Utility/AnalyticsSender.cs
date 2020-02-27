using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
//using UnityEngine.Analytics;
using Firebase.Analytics;

public class AnalyticsSender {
	static bool sceneSetupDone = false;

	public static void Event(string eventName, Dictionary<string, object> eventParams) {
		if (!sceneSetupDone) {
			SceneManager.sceneUnloaded += SceneEnded;
			sceneSetupDone = true;
		}

		// Unity
		//Analytics.CustomEvent(eventName, eventParams);

		// Google
		List<Parameter> paramList = new List<Parameter>();
		foreach (var ep in eventParams) {
			System.Type paramType = ep.Value.GetType();
			if (paramType == typeof(int) || paramType == typeof(long))
				paramList.Add(new Parameter(ep.Key, (int)ep.Value));
			else if (paramType == typeof(float) || paramType == typeof(double))
				paramList.Add(new Parameter(ep.Key, (float)ep.Value));
			else paramList.Add(new Parameter(ep.Key, ep.Value.ToString()));
		}
		FirebaseAnalytics.LogEvent(eventName, paramList.ToArray());
	}

	static void SceneEnded(Scene arg0) {
		//Analytics.FlushEvents();
	}
}