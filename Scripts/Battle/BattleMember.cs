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
            if (sex[i] == "�j��")
            {
                member[i].sex.text = "��";
                member[i].sex.color = new Color(0, 0, 1);
            }
            else if (sex[i] == "����")
            {
                member[i].sex.text = "��";
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
                member[i].error.text = "�ǂ�";
                member[i].error.color = Type.TypeToColor("�ǂ�");
            }
            else if (error[i] == 2)
            {
                member[i].error.text = "�����ǂ�";
                member[i].error.color = Type.TypeToColor("�ǂ�");
            }
            else if (error[i] == 3)
            {
                member[i].error.text = "�܂�";
                member[i].error.color = Type.TypeToColor("�ł�");
            }
            else if (error[i] == 4)
            {
                member[i].error.text = "�₯��";
                member[i].error.color = Type.TypeToColor("�ق̂�");
            }
            else if (error[i] == 5)
            {
                member[i].error.text = "������" + errorTurn[i];
                member[i].error.color = Type.TypeToColor("������");
            }
            else if (error[i] == 6)
            {
                member[i].error.text = "�������";
                member[i].error.color = Type.TypeToColor("�S�[�X�g");
            }
            else if (error[i] == 7)
            {
                member[i].error.text = "�˂ނ�" + errorTurn[i];
                member[i].error.color = Type.TypeToColor("�G�X�p�[");
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
