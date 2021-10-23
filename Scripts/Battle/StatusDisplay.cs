using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;
using TypeUtil;

public class StatusDisplay : MonoBehaviour
{
    public SkillMaster skillMaster;
    public ItemMaster itemMaster;
    public Image image;
    public new Text name;
    public List<StatusSkill> skill;
    public Text type1;
    public Text type2;
    public List<Text> status;
    public Text itemName;
    public Text itemText;
    public Text passiveName;
    public Text passiveText;
    public GameObject info;
    public GameObject panel;

    public void Display(string name, string type1, string type2, int[] status, int item, string passiveName, string passiveText, string[] skill,int[] sp)
    {
        StartCoroutine(ImageLoad(image,name+"B"));
        this.name.text = name;
        this.type1.text = type1;
        this.type1.color = Type.TypeToColor(type1);
        this.type2.text = type2;
        this.type2.color = Type.TypeToColor(type2);
        for (int i = 0; i < 6; i++)
        {
            this.status[i].text = status[i].ToString();
        }
        this.passiveName.text = passiveName;
        this.passiveText.text = passiveText;
        itemName.text = itemMaster.ItemList.Find(a => a.no == item).name;
        itemText.text = itemMaster.ItemList.Find(a => a.no == item).text;
        for(int i = 0; i < 4; i++)
        {
            Skill s = skillMaster.SkillList.Find(a => a.no == skill[i]);
            this.skill[i].name.text = s.name;
            this.skill[i].name.color = Type.TypeToColor(s.type);
            this.skill[i].power.text = s.power.ToString();
            if (this.skill[i].power.text == "0" || this.skill[i].power.text == "1") this.skill[i].power.text = "-";
            this.skill[i].hit.text = s.hit.ToString();
            if (this.skill[i].hit.text == "0") this.skill[i].hit.text = "-";
            this.skill[i].sp.text = sp[i]+"/"+s.point;
        }
        info.SetActive(true);
        panel.SetActive(true);
    }
    IEnumerator ImageLoad(Image image, string name)
    {
        var icon = Addressables.LoadAssetAsync<Sprite>(name);
        yield return icon;
        image.sprite = icon.Result;
    }

    public void Back()
    {
        info.SetActive(false);
        panel.SetActive(false);
    }
}
