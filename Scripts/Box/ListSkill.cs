using KanKikuchi.AudioManager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ListSkill : MonoBehaviour
{
    public Skill skill;
    public new Text name;
    public void Click()
    {
        SEManager.Instance.Play(SEPath.CLICK);
        GameObject.Find("SkillChange").GetComponent<SkillChange>().Select(skill);
    }
}
