using KanKikuchi.AudioManager;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using TypeUtil;
using UnityEngine;
using UnityEngine.UI;

public class SkillChange : MonoBehaviour
{
    public Chara chara;
    public SkillMaster skillMaster;
    public List<SkillInfo> skillInfo;
    public GameObject info;
    public GameObject panel;
    public GameObject prefab;
    public GameObject content;
    public Text type;
    public Text physics;
    public Text etc;
    public Text power;
    public Text hit;
    public Text sp;
    public Text text;
    public Text skillName;
    public List<Button> button;

    Skill selectSkill = null;

    public void SkillList(Chara c)
    {
        selectSkill = null;
        skillName.text = "‹Z‚ð‘I‚Ú‚¤";
        type.text = "";
        physics.text = "";
        power.text = "";
        hit.text = "";
        sp.text = "";
        text.text = "";
        etc.text = "";


        var clones = GameObject.FindGameObjectsWithTag("skill");
        foreach (var clone in clones)
        {
            Destroy(clone);
        }


        chara = c;

        DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(SkillJson));
        Debug.Log("{\"skillList\":" + chara.skill + "}");
        byte[] bytes = Encoding.UTF8.GetBytes("{\"skillList\":"+chara.skill+"}");
        MemoryStream ms = new MemoryStream(bytes);
        SkillJson skillJson = (SkillJson)serializer.ReadObject(ms);

        content.GetComponent<RectTransform>().sizeDelta = new Vector2(584, 200+100 * skillJson.skillList.Count);

        int num=0;
        GameObject listSkill0 = Instantiate(prefab);
        listSkill0.transform.SetParent(content.transform,false);
        listSkill0.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -50 -100 * num);
        listSkill0.GetComponent<ListSkill>().skill = null;
        listSkill0.GetComponent<ListSkill>().name.text = "‚Í‚¸‚·";
        num++;
        foreach (string skillNo in skillJson.skillList)
        {
            Skill skill = skillMaster.SkillList.Find(a => a.no == skillNo);
            if (skill == null) Debug.Log(skillNo);
            else
            {
                GameObject listSkill = Instantiate(prefab);
                listSkill.transform.SetParent(content.transform, false);
                listSkill.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -50 - 100 * num);
                listSkill.GetComponent<ListSkill>().skill = skill;
                listSkill.GetComponent<ListSkill>().name.text = skill.ok+skill.name;
                listSkill.GetComponent<ListSkill>().name.color = Type.TypeToColor(skill.type);
                num++;
            }
        }
        GameObject listSkill9 = Instantiate(prefab);
        listSkill9.transform.SetParent(content.transform, false);
        listSkill9.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -50 - 100 * num);
        Skill skill9 = skillMaster.SkillList.Find(a => a.no == "no69");
        listSkill9.GetComponent<ListSkill>().skill = skillMaster.SkillList.Find(a => a.no == "no69");
        listSkill9.GetComponent<ListSkill>().name.text = skill9.ok + skill9.name;
        listSkill9.GetComponent<ListSkill>().name.color = Type.TypeToColor(skill9.type);
        SkillDisplay();
    }

    public void SkillDisplay()
    {
        string party = GameObject.Find("Edit").GetComponent<Edit>().partyNo;
        for (int i = 0; i < 4; i++) {
            if(PlayerPrefs.GetString(party + chara.no + "S" + i, "") == "")
            {
                skillInfo[i].name.text = "";
                skillInfo[i].type.text = "";
                skillInfo[i].power.text = "";
                skillInfo[i].hit.text = "";
                skillInfo[i].sp.text = "";
            }
            else
            {
                Skill skill = skillMaster.SkillList.Find(a => a.no == PlayerPrefs.GetString(party + chara.no + "S" + i));
                skillInfo[i].name.text = skill.name;
                skillInfo[i].type.text = skill.type;
                skillInfo[i].type.color = Type.TypeToColor(skill.type);
                skillInfo[i].power.text = skill.power.ToString();
                if (skillInfo[i].power.text == "0" || skillInfo[i].power.text == "1") skillInfo[i].power.text = "-";
                skillInfo[i].hit.text = skill.hit.ToString();
                if (skillInfo[i].hit.text == "0") skillInfo[i].hit.text = "-";
                skillInfo[i].sp.text = skill.point.ToString();
            }
        }
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
        if(selectSkill!=null)PlayerPrefs.SetString(party + chara.no + "S" + i, selectSkill.no);
        else PlayerPrefs.SetString(party + chara.no + "S" + i, "");
        SkillDisplay();
        Already();
    }
    public void Select(Skill s)
    {
        selectSkill = s;
        Already();
        if (selectSkill == null)
        {
            skillName.text = "";
            type.text = "";
            physics.text = "";
            power.text = "";
            hit.text = "";
            sp.text = "";
            text.text = "";
            etc.text = "";
         }
        else
        {
            skillName.text = selectSkill.name;
            type.text = selectSkill.type;
            type.color = Type.TypeToColor(selectSkill.type);
            physics.text = selectSkill.physics;
            power.text = selectSkill.power.ToString();
            if (power.text == "0" || power.text == "1") power.text = "-";
            hit.text = selectSkill.hit.ToString();
            if (hit.text == "0") hit.text = "-";
            sp.text = selectSkill.point.ToString();
            text.text = selectSkill.text;
            if(selectSkill.speed!=0) text.text = "—Dæ“x"+selectSkill.speed+"B"+selectSkill.text;
            if (selectSkill.sound > 0)
            {
                etc.text = "‰¹";
            }
            else if (selectSkill.contact > 0)
            {
                etc.text = "ÚG";
            }
            else if (selectSkill.through > 0)
            {
                etc.text = "ŠÑ’Ê";
            }
            else
            {
                etc.text = "";
            }
        }
    }

    [System.Runtime.Serialization.DataContract]
    class SkillJson
    {
        [System.Runtime.Serialization.DataMember()]
        public List<string> skillList;
    }
    void Already()
    {
        foreach (Button b in button)
        {
            b.interactable = true;
        }
        if (selectSkill != null)
        {
            for (int i = 0; i < 4; i++)
            {
                if(selectSkill.name == skillInfo[i].name.text)
                {
                    foreach(Button b in button)
                    {
                        b.interactable = false;
                    }
                }
            }
        }
    }
}
