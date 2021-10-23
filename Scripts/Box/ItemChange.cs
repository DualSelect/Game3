using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TypeUtil;
using KanKikuchi.AudioManager;

public class ItemChange : MonoBehaviour
{
    public Chara chara;
    public ItemMaster itemMaster;
    public GameObject info;
    public GameObject panel;
    public GameObject prefab;
    public GameObject content;
    public Text textUp;
    public Text nameUp;
    public Text textDown;
    public Text nameDown;
    Item selectItem = null;
    public List<Text> typePer; 


    public void ItemList(Chara c)
    {
        var clones = GameObject.FindGameObjectsWithTag("item");
        foreach (var clone in clones)
        {
            Destroy(clone);
        }


        chara = c;

        content.GetComponent<RectTransform>().sizeDelta = new Vector2(584,100* itemMaster.ItemList.Count);

        int num = 0;
        GameObject listItem0 = Instantiate(prefab);
        listItem0.transform.SetParent(content.transform, false);
        listItem0.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -50 - 100 * num);
        listItem0.GetComponent<ListItem>().item = null;
        listItem0.GetComponent<ListItem>().name.text = "ÇÕÇ∏Ç∑";
        num++;
        foreach (Item item in itemMaster.ItemList)
        {
            if (item.no == 0 || item.ok=="Å~") continue;
            GameObject listItem = Instantiate(prefab);
            listItem.transform.SetParent(content.transform, false);
            listItem.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -50 - 100 * num);
            listItem.GetComponent<ListItem>().item = item;
            listItem.GetComponent<ListItem>().name.text = item.ok +item.name;
            num++;
        }

        ItemDisplay();

        for (int i=0; i < 18; i++)
        {
            typePer[i].text = Type.Per(i, chara.type1, chara.type2).ToString();
        }
    }
    public void ItemDisplay()
    {
        string party = GameObject.Find("Edit").GetComponent<Edit>().partyNo;

        if (PlayerPrefs.GetInt(party + chara.no + "I", 0) == 0)
        {
            nameUp.text = "";
            textUp.text = "";
        }
        else
        {
            Item item = itemMaster.ItemList.Find(a => a.no == PlayerPrefs.GetInt(party + chara.no + "I"));
            if (item != null)
            {
                nameUp.text = item.name;
                textUp.text = item.text;
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
    public void Select(Item i)
    {
        selectItem = i;
        if (selectItem == null)
        {
            nameDown.text = "";
            textDown.text = "";
        }
        else
        {
            nameDown.text = selectItem.name;
            textDown.text = selectItem.text;
        }
    }
    public void Change()
    {
        SEManager.Instance.Play(SEPath.CLICK);
        string party = GameObject.Find("Edit").GetComponent<Edit>().partyNo;
        if (selectItem != null) PlayerPrefs.SetInt(party + chara.no + "I", selectItem.no);
        else PlayerPrefs.SetInt(party + chara.no + "I", 0);
        ItemDisplay();
    }
}
