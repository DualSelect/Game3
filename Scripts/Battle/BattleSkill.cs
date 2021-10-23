using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TypeUtil;

public class BattleSkill : MonoBehaviour
{
    public new List<Text> name;
    public List<Text> sp;
    public List<Button> button;
    public SkillMaster skillMaster;

    public void Display(string[] skill,int[] sp ,bool[] select)
    {
        button[4].gameObject.SetActive(false);
        for (int i = 0; i < 4; i++) {
            Skill s = skillMaster.SkillList.Find(a => a.no == skill[i]);
            name[i].text = s.name;
            name[i].color = Type.TypeToColor(s.type);
            this.sp[i].text = sp[i] + "/" + s.point;
            button[i].interactable = select[i];
        }
        if(button[0].interactable==false&& button[1].interactable == false && button[2].interactable == false && button[3].interactable == false) button[4].gameObject.SetActive(true);
        this.gameObject.SetActive(true);
    }
    public void Back()
    {
        this.gameObject.SetActive(false);
    }
}
