using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class DecisionTypeBase : MonoBehaviour {
	public delegate void AnswerEvent(int answerIndex);
	public AnswerEvent OnAnswer;
}