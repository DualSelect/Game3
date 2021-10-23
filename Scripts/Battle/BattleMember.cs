using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;
using TypeUtil;

public class BattleMember : MonoBehaviour
{
    public List<MemberInfo> member;

    public void Display(string[] name, string[] sex, bool[] item, int[] error, int[] errorTurn, string[] hp,bool[] change)
    {
        this.gameObject.SetActive(true);
        for (int i = 0; i < 3; i++)
        {
            StartCoroutine(ImageLoad(member[i].chara, name[i] + "F"));
            member[i].name.text = name[i];
            if (sex[i] == "男性")
            {
                member[i].sex.text = "♂";
                member[i].sex.color = new Color(0, 0, 1);
            }
            else if (sex[i] == "女性")
            {
                member[i].sex.text = "♀";
                member[i].sex.color = new Color(1, 0, 0);
            }
            else
            {
                member[i].sex.text = "";
            }
            member[i].item.SetActive(item[i]);
            member[i].hp.text = hp[i];
            member[i].change.interactable = change[i];
            if (error[i] == 0)
            {
                member[i].error.text = "";
            }
            else if (error[i] == 1)
            {
                member[i].error.text = "どく";
                member[i].error.color = Type.TypeToColor("どく");
            }
            else if (error[i] == 2)
            {
                member[i].error.text = "もうどく";
                member[i].error.color = Type.TypeToColor("どく");
            }
            else if (error[i] == 3)
            {
                member[i].error.text = "まひ";
                member[i].error.color = Type.TypeToColor("でんき");
            }
            else if (error[i] == 4)
            {
                member[i].error.text = "やけど";
                member[i].error.color = Type.TypeToColor("ほのお");
            }
            else if (error[i] == 5)
            {
                member[i].error.text = "こおり" + errorTurn[i];
                member[i].error.color = Type.TypeToColor("こおり");
            }
            else if (error[i] == 6)
            {
                member[i].error.text = "ちんもく";
                member[i].error.color = Type.TypeToColor("ゴースト");
            }
            else if (error[i] == 7)
            {
                member[i].error.text = "ねむり" + errorTurn[i];
                member[i].error.color = Type.TypeToColor("エスパー");
            }
        }
    }
    public void Back()
    {
        this.gameObject.SetActive(false);
    }
    IEnumerator ImageLoad(Image image, string name)
    {
        var icon = Addressables.LoadAssetAsync<Sprite>(name);
        yield return icon;
        image.sprite = icon.Result;
    }
}
