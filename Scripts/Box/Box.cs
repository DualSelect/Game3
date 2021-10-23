using KanKikuchi.AudioManager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

public class Box : MonoBehaviour
{
    public GameObject icon;
    public List<GameObject> box;
    public CharaMaster charaMaster;
    public Dropdown boxName;
    void Start()
    {
        int n0 = 0;
        int n1 = 0;
        int n2 = 0;
        int n3 = 0;
        int n4 = 0;
        foreach (Chara chara in charaMaster.CharaList)
        {
            if (chara.ok == "×") continue;
            GameObject obj = Instantiate(icon);
            obj.GetComponent<Icon>().chara = chara;
            if(chara.ok == "") obj.GetComponent<Button>().interactable = false;
            obj.GetComponent<Icon>().mark.text = chara.ok;
            if (chara.ok == "△") obj.GetComponent<Icon>().mark.color = new Color(0, 0, 1);

            if (chara.group1 == "にじさんじ") {
                if (chara.group2 == "1期生" || chara.group2 == "2期生" || chara.group2 == "ゲーマーズ")
                {
                    StartCoroutine(IconLoad(obj.GetComponent<Image>(), chara.name + "F"));
                    obj.transform.SetParent(box[0].transform,false);
                    obj.GetComponent<RectTransform>().anchoredPosition = new Vector2(120+(n0%6)*140,-70-(n0/6)*140);
                    n0++;
                }
                if (chara.group2 == "SEEDs")
                {
                    StartCoroutine(IconLoad(obj.GetComponent<Image>(), chara.name + "F"));
                    obj.transform.SetParent(box[1].transform,false);
                    obj.GetComponent<RectTransform>().anchoredPosition = new Vector2(120 + (n1 % 6) * 140, -70 - (n1 / 6) * 140);
                    n1++;
                }
                if (chara.group2 == "2019年上期")
                {
                    StartCoroutine(IconLoad(obj.GetComponent<Image>(), chara.name + "F"));
                    obj.transform.SetParent(box[2].transform, false);
                    obj.GetComponent<RectTransform>().anchoredPosition = new Vector2(120 + (n2 % 6) * 140, -70 - (n2 / 6) * 140);
                    n2++;
                }
                if (chara.group2 == "2019年下期")
                {
                    StartCoroutine(IconLoad(obj.GetComponent<Image>(), chara.name + "F"));
                    obj.transform.SetParent(box[3].transform, false);
                    obj.GetComponent<RectTransform>().anchoredPosition = new Vector2(120 + (n3 % 6) * 140, -70 - (n3 / 6) * 140);
                    n3++;
                }
                if (chara.group2 == "2020年")
                {
                    StartCoroutine(IconLoad(obj.GetComponent<Image>(), chara.name + "F"));
                    obj.transform.SetParent(box[4].transform, false);
                    obj.GetComponent<RectTransform>().anchoredPosition = new Vector2(120 + (n4 % 6) * 140, -70 - (n4 / 6) * 140);
                    n4++;
                }
            }
        }
    }
    IEnumerator IconLoad(Image image,string name)
    {
        var icon = Addressables.LoadAssetAsync<Sprite>(name);
        yield return icon;
        image.sprite = icon.Result;
    }
    public void BoxChange(int i)
    {
        SEManager.Instance.Play(SEPath.CLICK);
        foreach (GameObject gameObject in box)
        {
            gameObject.SetActive(false);
        }
        box[i].SetActive(true);
    }
    public void NextBox()
    {
        if (boxName.value == boxName.options.Count - 1)
        {
            boxName.value = 0;
        }
        else
        {
            boxName.value = boxName.value + 1;
        }
    }
    public void PrevBox()
    {
        if (boxName.value == 0)
        {
            boxName.value = boxName.options.Count - 1;
        }
        else
        {
            boxName.value = boxName.value - 1;
        }
    }
}
