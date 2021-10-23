using KanKikuchi.AudioManager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Status : MonoBehaviour
{
    public Chara chara;
    public List<Bar> bar;
    public Bar expBar;
    public GameObject info;
    public GameObject panel;
    public List<Exp> expList;
    int expMax;

    public void StatusDisplay(Chara c)
    {
        chara = c;
        expMax = 65;
        if(chara.name== "–îŽÔ‚è‚Ë") expMax = 0;
        bar[0].basic.GetComponent<RectTransform>().sizeDelta = new Vector2(chara.h, 20);
        bar[1].basic.GetComponent<RectTransform>().sizeDelta = new Vector2(chara.a, 20);
        bar[2].basic.GetComponent<RectTransform>().sizeDelta = new Vector2(chara.b, 20);
        bar[3].basic.GetComponent<RectTransform>().sizeDelta = new Vector2(chara.c, 20);
        bar[4].basic.GetComponent<RectTransform>().sizeDelta = new Vector2(chara.d, 20);
        bar[5].basic.GetComponent<RectTransform>().sizeDelta = new Vector2(chara.s, 20);
        bar[6].basic.GetComponent<RectTransform>().sizeDelta = new Vector2(chara.h+ chara.a+ chara.b+ chara.c+ chara.d+chara.s, 20);
        bar[0].basicNum.text = chara.h.ToString();
        bar[1].basicNum.text = chara.a.ToString();
        bar[2].basicNum.text = chara.b.ToString();
        bar[3].basicNum.text = chara.c.ToString();
        bar[4].basicNum.text = chara.d.ToString();
        bar[5].basicNum.text = chara.s.ToString();
        bar[6].basicNum.text = (chara.h + chara.a + chara.b + chara.c + chara.d + chara.s).ToString();

        for(int i = 0; i < 6; i++)
        {
            ExpUpdate(i);
        }

        info.SetActive(true);
        panel.SetActive(true);
    }
    void ExpUpdate(int i)
    {
        string party = GameObject.Find("Edit").GetComponent<Edit>().partyNo;
        int exp = PlayerPrefs.GetInt(party + chara.no + "E" + i, 0);
        bar[i].plus.GetComponent<RectTransform>().sizeDelta = new Vector2(exp, 20);
        bar[i].plusNum.text = exp.ToString();
        if (exp==0)bar[i].plusNum.text = "";
        bar[i].total.text = (int.Parse(bar[i].basicNum.text) + exp).ToString();
        if(i==0) bar[i].total.text = (int.Parse(bar[i].basicNum.text) + exp + 50).ToString();

        int expTotal = 0;
        for (int j = 0; j < 6; j++)
        {
            expTotal += PlayerPrefs.GetInt(party + chara.no + "E" + j, 0);
        }
        bar[6].plus.GetComponent<RectTransform>().sizeDelta = new Vector2(expTotal, 20);
        bar[6].plusNum.text = expTotal.ToString();
        if (expTotal == 0) bar[6].plusNum.text = "";
        bar[6].total.text = (int.Parse(bar[6].basicNum.text) + expTotal).ToString();

        expBar.plus.GetComponent<RectTransform>().sizeDelta = new Vector2(expMax-expTotal, 20);
        expBar.plusNum.text = (expMax - expTotal).ToString();
        if (expMax - expTotal == 0) expBar.plusNum.text = "";
        expBar.total.text = (expMax - expTotal).ToString();

        for (int j = 0; j < 6; j++)
        {
            if(PlayerPrefs.GetInt(party + chara.no + "E" + j, 0) > 0)
            {
                expList[j].minus.interactable = true;
                expList[j].min.interactable = true;
            }
            else
            {
                expList[j].minus.interactable = false;
                expList[j].min.interactable = false;
            }
            if(PlayerPrefs.GetInt(party + chara.no + "E" + j, 0) == 32)
            {
                expList[j].plus.interactable = false;
                expList[j].max.interactable = false;
            }
            else
            {
                expList[j].plus.interactable = true;
                expList[j].max.interactable = true;
            }
        }

        if(expMax - expTotal == 0)
        {
            foreach(Exp e in expList)
            {
                e.plus.interactable = false;
                e.max.interactable = false;
            }
        }
        if (expMax - expTotal < 32)
        {
            foreach (Exp e in expList)
            {
                e.max.interactable = false;
            }
        }
    }
    public void Back()
    {
        SEManager.Instance.Play(SEPath.CLICK);
        GameObject.Find("Common").GetComponent<Common>().CommonDisplay(chara);
        info.SetActive(false);
        panel.SetActive(false);
    }

    public void Plus(int i)
    {
        string party = GameObject.Find("Edit").GetComponent<Edit>().partyNo;
        PlayerPrefs.SetInt(party + chara.no + "E" + i, PlayerPrefs.GetInt(party + chara.no + "E" + i, 0)+1);
        if(PlayerPrefs.GetInt(party + chara.no + "E" + i, 0)>32) PlayerPrefs.SetInt(party + chara.no + "E" + i, 32);
        ExpUpdate(i);
    }
    public void Minus(int i)
    {
        string party = GameObject.Find("Edit").GetComponent<Edit>().partyNo;
        PlayerPrefs.SetInt(party + chara.no + "E" + i, PlayerPrefs.GetInt(party + chara.no + "E" + i, 0) - 1);
        if (PlayerPrefs.GetInt(party + chara.no + "E" + i, 0) < 0) PlayerPrefs.SetInt(party + chara.no + "E" + i, 0);
        ExpUpdate(i);
    }
    public void Max(int i)
    {
        string party = GameObject.Find("Edit").GetComponent<Edit>().partyNo;
        PlayerPrefs.SetInt(party + chara.no + "E" + i, 32);
        ExpUpdate(i);
    }
    public void Min(int i)
    {
        string party = GameObject.Find("Edit").GetComponent<Edit>().partyNo;
        PlayerPrefs.SetInt(party + chara.no + "E" + i, 0);
        ExpUpdate(i);
    }
}
