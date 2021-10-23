using KanKikuchi.AudioManager;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

public class Common : MonoBehaviour
{
    public Chara chara;
    public Image body;
    public new Text name;
    public Text group;
    public Text debue;
    public Text text;
    public Text type1;
    public Text type2;
    public Text passive;
    public Text item;
    public List<Text> skill;
    public GameObject info;
    public GameObject panel;
    public SkillMaster skillMaster;
    public ItemMaster itemMaster;
    public Status status;
    public SkillChange skillObj;
    public Passive passiveObj;
    public ItemChange itemObj;
    public WinPoint win;

    public void CommonDisplay(Chara c)
    {
        info.SetActive(true);
        panel.SetActive(true);
        ImageCenter();
        chara = c;
        name.text = chara.name;
        group.text = chara.group1;
        DateTime date = DateTime.ParseExact(chara.debue, "yyyy/MM/dd", null);
        debue.text = date.ToString("yyyy”NMŒŽd“ú");

        StartCoroutine(ImageLoad(body, chara.name + "B"));
        text.text = chara.text;
        type1.text = chara.type1;
        type1.color = TypeUtil.Type.TypeToColor(chara.type1);
        type2.text = chara.type2;
        type2.color = TypeUtil.Type.TypeToColor(chara.type2);

        string party = GameObject.Find("Edit").GetComponent<Edit>().partyNo;
        if (PlayerPrefs.GetInt(party + chara.no + "P", 1) == 1)
        {
            passive.text = chara.passiveName1;
        }
        else
        {
            passive.text = chara.passiveName2;
        }

        for(int i = 0; i < 4; i++)
        {
            if (PlayerPrefs.GetString(party + chara.no + "S" + i, "") != "")
            {
                skill[i].text = skillMaster.SkillList.Find(a => a.no == PlayerPrefs.GetString(party + chara.no + "S" + i, "")).name;
                skill[i].color = TypeUtil.Type.TypeToColor(skillMaster.SkillList.Find(a => a.no == PlayerPrefs.GetString(party + chara.no + "S" + i, "")).type);
            }
            else skill[i].text = "";
        }
        if (PlayerPrefs.GetInt(party + chara.no + "I", 0) != 0) item.text = itemMaster.ItemList.Find(a => a.no == PlayerPrefs.GetInt(party + chara.no + "I", 0)).name;
        else item.text = "";

    }
    IEnumerator ImageLoad(Image image, string name)
    {
        var icon = Addressables.LoadAssetAsync<Sprite>(name);
        yield return icon;
        image.sprite = icon.Result;
    }
    public void Back()
    {
        SEManager.Instance.Play(SEPath.CLICK);
        GameObject.Find("Edit").GetComponent<Edit>().PartyDisplay();
        info.SetActive(false);
        panel.SetActive(false);
    }

    public void StatusButton()
    {
        SEManager.Instance.Play(SEPath.CLICK);
        ImageLeft();
        status.StatusDisplay(chara);
    }
    public void SkillButton()
    {
        SEManager.Instance.Play(SEPath.CLICK);
        ImageLeft();
        skillObj.SkillList(chara);
    }
    public void PassiveButton()
    {
        SEManager.Instance.Play(SEPath.CLICK);
        ImageLeft();
        passiveObj.PassiveDisplay(chara);
    }
    public void ItemButton()
    {
        SEManager.Instance.Play(SEPath.CLICK);
        ImageLeft();
        itemObj.ItemList(chara);
    }
    public void ImageCenter()
    {
        StopAllCoroutines();
        StartCoroutine(ImageCenterIE());
    }
    IEnumerator ImageCenterIE()
    {
        while (true)
        {
            body.transform.localPosition = Vector3.MoveTowards(body.transform.localPosition, new Vector3(0, 0, 0), 20);
            if (body.transform.localPosition == new Vector3(0, 0, 0)) break;
            yield return new WaitForSecondsRealtime(0.01f);
        }
    }
    public void ImageLeft()
    {
        StopAllCoroutines();
        StartCoroutine(ImageLeftIE());
    }
    IEnumerator ImageLeftIE()
    {
        while (true)
        {
            body.transform.localPosition = Vector3.MoveTowards(body.transform.localPosition, new Vector3(-450, 0, 0), 20);
            if (body.transform.localPosition == new Vector3(-450, 0, 0)) break;
            yield return new WaitForSecondsRealtime(0.01f);
        }
    }
    public void Youtube()
    {
        SEManager.Instance.Play(SEPath.CLICK);
        var uri = new Uri(chara.youtube);
        Application.OpenURL(uri.AbsoluteUri);
    }
    public void Twitter()
    {
        SEManager.Instance.Play(SEPath.CLICK);
        var uri = new Uri(chara.twitter);
        Application.OpenURL(uri.AbsoluteUri);
    }
    public void WinPointButton()
    {
        win.WinPointDisplay(chara);
    }
}
