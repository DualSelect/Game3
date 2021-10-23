using KanKikuchi.AudioManager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WinPoint : MonoBehaviour
{
    public GameObject down;
    public Text point;
    public Button b1;
    public Button b10;
    public Button b30;
    public Button b50;
    public Button b80;
    public Button b100;
    public Button b150;
    public Button b200;
    Chara chara;
    public void WinPointDisplay(Chara c)
    {
        chara = c;
        point.text = "勝利ポイント：" + PlayerPrefs.GetInt(chara.name+"win", 0);
        if (PlayerPrefs.GetInt(chara.name + "win", 0) >= 1) b1.interactable = true;
        if (PlayerPrefs.GetInt(chara.name + "win", 0) >= 10) b10.interactable = true;
        if (PlayerPrefs.GetInt(chara.name + "win", 0) >= 30) b30.interactable = true;
        if (PlayerPrefs.GetInt(chara.name + "win", 0) >= 50) b50.interactable = true;
        if (PlayerPrefs.GetInt(chara.name + "win", 0) >= 80) b80.interactable = true;
        if (PlayerPrefs.GetInt(chara.name + "win", 0) >= 100) b100.interactable = true;
        if (PlayerPrefs.GetInt(chara.name + "win", 0) >= 150) b150.interactable = true;
        if (PlayerPrefs.GetInt(chara.name + "win", 0) >= 200) b200.interactable = true;
        down.SetActive(true);
    }
    public void Avator(string v)
    {
        PlayerPrefs.SetString("avator", chara.name+v);
    }
    public void Title(string t)
    {
        PlayerPrefs.SetString("title", chara.name+t);
    }
    public void Back()
    {
        SEManager.Instance.Play(SEPath.CLICK);
        down.SetActive(false);
    }
}
