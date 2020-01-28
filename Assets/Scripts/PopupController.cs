using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class PopupController : MonoBehaviour
{
    public GameObject _HitImg;
    public GameObject _showPopupPanel;
    public Sprite[] _popupSprite;
    void Start()
    {
        _HitImg.SetActive(false);
    }

    public void ShowPanel(int n) {
        _HitImg.SetActive(true);
         _showPopupPanel.SetActive(true);
        _showPopupPanel.GetComponent<Image>().sprite = _popupSprite[n];
    }
    public void ClosePanel() {
        _HitImg.SetActive(false);
        _showPopupPanel.SetActive(false);
        //for (int i = 0; i < _showPopupPanel.Length; i++) {
        //    _showPopupPanel[i].SetActive(false);
        //}
    }
}
