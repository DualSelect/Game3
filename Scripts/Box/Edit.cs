using KanKikuchi.AudioManager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Edit : MonoBehaviour
{
    public string partyNo;
    public Chara selectChara;
    public Text select;
    public List<Party> partyList;
    public List<Button> button;
    public Common common;
    public CharaMaster charaMaster;
    public Text selectParty;
    public List<Text> selectPartyColor;

    public GameObject errorObj;
    public Text errorMessage;

    void Start()
    {
        PartyChange(PlayerPrefs.GetString("party", "A"));
        PartyDisplay();
    }
    public void PartyDisplay()
    {
        for (int i = 0; i < 6; i++)
        {
            if (PlayerPrefs.GetInt(partyNo + "P" + i, 0) != 0) partyList[i].InChara(charaMaster.CharaList.Find(a => a.no == PlayerPrefs.GetInt(partyNo + "P" + i, 0)));
            else partyList[i].OutChara();
        }
    }
    public void SelectNameChange(Chara chara)
    {
        selectChara = chara;
        select.text = chara.name;
        bool b = false;
        button[0].interactable = true;
        foreach(Party party in partyList)
        {
            if (select.text == party.name.text) b = true;
        }
        if (b)
        {
            button[1].interactable = false;button[2].interactable = true;
        }
        else
        {
            button[2].interactable = false;
            foreach (Party party in partyList)
            {
                if (party.name.text == "") button[1].interactable = true;
            }
        }
    }
    public void InParty()
    {
        SEManager.Instance.Play(SEPath.CLICK);
        int i = 0;
        foreach (Party party in partyList)
        {
            if (party.name.text == "")
            {
                party.InChara(selectChara);
                button[1].interactable = false;
                button[2].interactable = true;
                PlayerPrefs.SetInt(partyNo + "P" + i,selectChara.no);
                break;
            }
            i++;
        }
    }
    public void OutParty()
    {
        SEManager.Instance.Play(SEPath.CLICK);
        int i = 0;
        foreach (Party party in partyList)
        {
            if (select.text == party.name.text)
            {
                party.OutChara();
                button[1].interactable = true;
                button[2].interactable = false;
                PlayerPrefs.SetInt(partyNo + "P" + i, 0);
                break;
            }
            i++;
        }
    }
    public void CommonOpen()
    {
        SEManager.Instance.Play(SEPath.CLICK);
        common.CommonDisplay(selectChara);
    }

    public void PartyChange(string s)
    {
        SEManager.Instance.Play(SEPath.CLICK);
        partyNo = s;
        if (s == "A") BGMManager.Instance.Play(BGMPath.BOX_A);
        if (s == "B") BGMManager.Instance.Play(BGMPath.BOX_B);
        if (s == "C") BGMManager.Instance.Play(BGMPath.BOX_C);
        if (s == "D") BGMManager.Instance.Play(BGMPath.BOX_D);
        PartyDisplay();
        foreach(Text text in selectPartyColor)
        {
            text.color = new Color(1, 1, 1);
        }
        if (partyNo == "A") selectPartyColor[0].color = new Color(1, 0, 0);
        if (partyNo == "B") selectPartyColor[1].color = new Color(1, 0, 0);
        if (partyNo == "C") selectPartyColor[2].color = new Color(1, 0, 0);
        if (partyNo == "D") selectPartyColor[3].color = new Color(1, 0, 0);
        selectParty.text = partyNo;
        PlayerPrefs.SetString("party", partyNo);
    }
    public void BattleButton()
    {
        SEManager.Instance.Play(SEPath.CLICK);
        for (int i = 0; i < 6; i++)
        {
            int charaNo = PlayerPrefs.GetInt(partyNo + "P" + i, 0);
            if (charaNo == 0)
            {
                ErrorDisplay("パーティーに空きがあります");
                return;
            }
            int expTotal = 0;
            for (int j = 0; j < 6; j++)
            {
                expTotal += PlayerPrefs.GetInt(partyNo + charaNo + "E" + j, 0);
            }
            if(expTotal != 65)
            {
                if (charaMaster.CharaList.Find(a => a.no == charaNo).name != "矢車りね")
                {
                    ErrorDisplay("能力を振り終えていないキャラがいます");
                    return;
                }
            }
            for (int j = 0; j < 4; j++)
            {
                if(PlayerPrefs.GetString(partyNo + charaNo + "S" + j, "") == "")
                {
                    ErrorDisplay("技に空きがあるキャラがいます");
                    return;
                }
                for (int k = j+1; k < 4; k++)
                {
                    if (PlayerPrefs.GetString(partyNo + charaNo + "S" + j, "") == PlayerPrefs.GetString(partyNo + charaNo + "S" + k, ""))
                    {
                        ErrorDisplay("同じ技を持つキャラがいます");
                        return;
                    }

                }
            }

            if (PlayerPrefs.GetInt(partyNo + PlayerPrefs.GetInt(partyNo + "P" + i, 0) + "I", 0) == 0)
            {
                ErrorDisplay("アイテムを持たせていないキャラがいます");
                return;
            }
            for (int j = i+1; j < 6; j++)
            {
                if (PlayerPrefs.GetInt(partyNo + PlayerPrefs.GetInt(partyNo + "P" + i, 0) + "I", 0) == PlayerPrefs.GetInt(partyNo + PlayerPrefs.GetInt(partyNo + "P" + j, 0) + "I", 0))
                {
                    ErrorDisplay("同じアイテムを持ったキャラがいます");
                    return;
                }
            }
        }
        SceneManager.LoadScene("Battle");
    }
    public void ErrorDisplay(string s)
    {
        errorMessage.text = s;
        errorObj.SetActive(true);
    }
    public void ErrorClose()
    {
        SEManager.Instance.Play(SEPath.CLICK);
        errorObj.SetActive(false);
    }

}
