using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MemberSelect : MonoBehaviour
{
    public Image image;
    public List<Button> button;
    public Text text;
    public int partyNum;
    public int selectNum;

    public void Select(int i)
    {
        GameObject.Find("BattlePlayer").GetComponent<BattlePlayer>().SelectMember(partyNum, i);
    }
    public void Status()
    {
        GameObject.Find("BattlePlayer").GetComponent<BattlePlayer>().StatusRequest(partyNum);
    }
}
