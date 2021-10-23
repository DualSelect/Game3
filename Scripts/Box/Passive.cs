using KanKikuchi.AudioManager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Passive : MonoBehaviour
{
    public Chara chara;
    public new List<Text> name;
    public List<Text> text;
    public List<Image> cheak;
    public List<Button> button;

    public GameObject info;
    public GameObject panel;

    public void PassiveDisplay(Chara c)
    {
        chara = c;
        foreach(Button b in button)
        {
            b.interactable = true;
        }
        name[0].text = chara.passiveName1;
        text[0].text = chara.passiveText1;
        name[1].text = chara.passiveName2;
        text[1].text = chara.passiveText2;
        if (name[1].text == "") button[1].interactable = false;
        foreach (Image i in cheak)
        {
            i.color = new Color(0, 0, 0);
        }
        string party = GameObject.Find("Edit").GetComponent<Edit>().partyNo;
        cheak[PlayerPrefs.GetInt(party + chara.no + "P", 1)-1].color = new Color(1, 1, 1);
        info.SetActive(true);
        panel.SetActive(true);
    }
    public void Back()
    {
        SEManager.Instance.Play(SEPath.CLICK);
        GameObject.Find("Common").GetComponent<Common>().CommonDisplay(chara);
        info.SetActive(false);
        panel.SetActive(false);
    }

    public void Change(int i)
    {
        SEManager.Instance.Play(SEPath.CLICK);
        string party = GameObject.Find("Edit").GetComponent<Edit>().partyNo;
        PlayerPrefs.SetInt(party + chara.no + "P", i);
        PassiveDisplay(chara);
    }
}
