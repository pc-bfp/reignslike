using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SetupNumberDisplay : MonoBehaviour {
	[SerializeField] Button buttonUp, buttonDown;
	[SerializeField] TextMeshProUGUI numDisplay;
	[SerializeField] int defaultValue = 10, incrementBy = 1, minValue = 0, maxValue = 100;
	[SerializeField] SetupNumberDisplay getMinFrom, getMaxFrom;
	[SerializeField] string prefSaveName;

	public int Value {
		get { return _value; }
		private set {
			if (getMinFrom && getMinFrom.isInitialized) minValue = getMinFrom.Value;
			if (getMaxFrom && getMaxFrom.isInitialized) maxValue = getMaxFrom.Value;
			value = Mathf.Clamp(value, minValue, maxValue);
			if (_value == value) return;
			_value = value;
			if (numDisplay) numDisplay.text = _value.ToString();
			if (!string.IsNullOrEmpty(prefSaveName)) PlayerPrefs.SetInt(prefSaveName, _value);
			OnValueChanged?.Invoke(_value);
		}
	}
	int _value = int.MaxValue;

	bool isInitialized = false;

	public delegate void ValueChangeEvent(int newValue);
	public ValueChangeEvent OnValueChanged;

	private void Awake() {
		if (!string.IsNullOrEmpty(prefSaveName) && PlayerPrefs.HasKey(prefSaveName)) Value = PlayerPrefs.GetInt(prefSaveName);
		else Value = defaultValue;
		if (buttonUp) buttonUp.onClick.AddListener(() => ChangeValue(true));
		if (buttonDown) buttonDown.onClick.AddListener(() => ChangeValue(false));
		if (getMinFrom) getMinFrom.OnValueChanged += val => Value = Value;
		if (getMaxFrom) getMaxFrom.OnValueChanged += val => Value = Value;
		isInitialized = true;
	}

	void ChangeValue(bool increment) {
		Value += (increment ? incrementBy : -incrementBy);
	}

	public void ResetToDefault() {
		Value = defaultValue;
	}
}