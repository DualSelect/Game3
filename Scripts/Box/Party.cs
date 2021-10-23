using KanKikuchi.AudioManager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

public class Party : MonoBehaviour
{
    public Chara chara;
    public Image icon;
    public new Text name;
    public List<Text> status;
    public Text sex;
    public GameObject item;

    public void InChara(Chara c)
    {
        chara = c;
        name.text = chara.name;
        StartCoroutine(ImageLoad(icon, chara.name + "F"));
        if(chara.sex == "èóê´")
        {
            sex.text = "Åä";
            sex.color = new Color(1, 0, 0);
        }
        else if (chara.sex == "íjê´")
        {
            sex.text = "Åâ";
            sex.color = new Color(0, 0, 1);
        }
        else
        {
            sex.text = "";
        }
        string party = GameObject.Find("Edit").GetComponent<Edit>().partyNo;
        status[0].text = "H:" + PlayerPrefs.GetInt(party + chara.no + "E0", 0);
        status[1].text = "A:" + PlayerPrefs.GetInt(party + chara.no + "E1", 0);
        status[2].text = "B:" + PlayerPrefs.GetInt(party + chara.no + "E2", 0);
        status[3].text = "C:" + PlayerPrefs.GetInt(party + chara.no + "E3", 0);
        status[4].text = "D:" + PlayerPrefs.GetInt(party + chara.no + "E4", 0);
        status[5].text = "S:" + PlayerPrefs.GetInt(party + chara.no + "E5", 0);

        item.SetActive(PlayerPrefs.GetInt(party + chara.no + "I", 0) != 0);

    }

    IEnumerator ImageLoad(Image image, string name)
    {
        var icon = Addressables.LoadAssetAsync<Sprite>(name);
        yield return icon;
        image.sprite = icon.Result;
    }
    public void OutChara()
    {
        StartCoroutine(ImageLoad(icon, "Dummy"));
        chara = null;
        name.text = "";
        sex.text = "";
        item.SetActive(false);
        foreach (Text text in status)
        {
            text.text = "";
        }
    }

    public void PartyButton()
    {
        SEManager.Instance.Play(SEPath.CLICK);
        if (name.text!="")GameObject.Find("Edit").GetComponent<Edit>().SelectNameChange(chara);
    }
}
