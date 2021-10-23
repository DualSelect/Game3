using BattleJson;
using KanKikuchi.AudioManager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

public class BattleDisplay : MonoBehaviour
{
    public Text turn;
    public Text area0;
    public Text message;
    public List<BattleDisplayPlayer> displayPlayer;
    public GameObject end;
    public GameObject win;
    public GameObject lose;
    float wait = 2f;
    public GameObject start;

    public IEnumerator SummonIE(bool own, Summon summon)
    {
        if (summon.type == "")
        {
            if (own)
            {
                message.text = "�߂�!�@" + displayPlayer[0].name.text + "!!";
                message.text += "\n" + "�����I " + summon.name + "!!";
                yield return displayPlayer[0].SummonIE(summon);
            }
            else
            {
                message.text = "����́@" + displayPlayer[1].name.text + "�@���������߂�";
                message.text += "\n" + "����́@" + summon.name + "�@���J��o����";
                yield return displayPlayer[1].SummonIE(summon);
            }
        }
        else if(summon.type == "exception")
        {
            if (own)
            {
                message.text = summon.exception;
                yield return displayPlayer[0].ChangeIE(summon);
            }
            else
            {
                message.text = summon.exception;
                yield return displayPlayer[1].ChangeIE(summon);
            }
        }
        else if (summon.type == "skill")
        {
            if (own)
            {
                message.text = summon.name + "���@��������o���ꂽ!!";
                yield return displayPlayer[0].SummonIE(summon);
            }
            else
            {
                message.text = summon.name + "���@��������o����!!";
                yield return displayPlayer[1].SummonIE(summon);
            }
        }
        else if (summon.type == "death")
        {
            if (own)
            {
                message.text = "�����l�@" + displayPlayer[0].name.text + "!!";
                message.text += "\n" + "�����I�@" + summon.name + "!!";
                yield return displayPlayer[0].SummonIE(summon);
            }
            else
            {
                message.text = "����́@" + displayPlayer[1].name.text + "�@���������߂�";
                message.text += "\n" + "����́@" + summon.name + "�@���J��o����";
                yield return displayPlayer[1].SummonIE(summon);
            }
        }
        yield return new WaitForSecondsRealtime(wait);
    }
    public IEnumerator StartIE(string name,string title)
    {
        message.text = title + "�́@" + name + "��" + "\n" + "�������d�|���Ă����I";
        start.SetActive(true);
        yield return new WaitForSecondsRealtime(wait*2);
        start.SetActive(false);
    }
    public IEnumerator EffectIE(bool own, Effect effect)
    {
        message.text = effect.message;
        if (own)
        {
            yield return displayPlayer[0].EffectIE(effect);
        }
        else
        {
            yield return displayPlayer[1].EffectIE(effect);
        }
        if(effect.message!="")yield return new WaitForSecondsRealtime(wait);
    }
    public IEnumerator AreaIE(bool master,Area area)
    {
        message.text = area.message;
        area0.text = "";
        if (area.both[0] == 1)
        {
            area0.text += "�Ђ���" + area.both[1]  + "\n";
        }
        else if(area.both[0] == 2)
        {
            area0.text += "����" + area.both[1] + "\n";
        }
        else if (area.both[0] == 3)
        {
            area0.text += "�����" + area.both[1] + "\n";
        }
        else if (area.both[0] == 4)
        {
            area0.text += "���Ȃ��炵" + area.both[1] + "\n";
        }
        if (area.both[2] > 0) area0.text += "�g���b�N���[��" + area.both[2] + "\n";
        if (area.both[3] > 0) area0.text += "���イ��傭" + area.both[3] + "\n";
        if (area.both[4] > 0) area0.text += "���܂�������" + area.both[4] + "\n";
        if (area.both[5] > 0) area0.text += "������" + area.both[5] + "\n";

        if (master)
        {
            yield return displayPlayer[0].AreaIE(area.player);
            yield return displayPlayer[1].AreaIE(area.enemy);
        }
        else
        {
            yield return displayPlayer[1].AreaIE(area.player);
            yield return displayPlayer[0].AreaIE(area.enemy);
        }


        yield return new WaitForSecondsRealtime(wait);
    }
    public IEnumerator FirstSummonIE(Summon my,Summon your)
    {
        message.text = "����� " + your.name + " ���J��o����";
        yield return displayPlayer[1].SummonIE(your);
        yield return new WaitForSecondsRealtime(wait);

        message.text += "\n" + "�����I " + my.name + "!!";
        yield return displayPlayer[0].SummonIE(my);
        yield return new WaitForSecondsRealtime(wait);
    }
    public IEnumerator DamageIE(bool own,Damage damage)
    {
        message.text = damage.message;



        if (own)yield return displayPlayer[0].DamageIE(damage);
        else yield return displayPlayer[1].DamageIE(damage);
        yield return new WaitForSecondsRealtime(wait);
    }
    public IEnumerator DeathIE(bool own)
    {
        if (own)
        {
            message.text = displayPlayer[0].name.text + " �͓|�ꂽ";
            yield return displayPlayer[0].DeathIE();
        }
        else
        {
            message.text = displayPlayer[1].name.text + " �͓|�ꂽ";
            yield return displayPlayer[1].DeathIE();
        }
    }
    public void Turn(string t)
    {
        turn.text = t;
    }
    public IEnumerator Win()
    {
        message.text = "���Ȃ��̏������I";
        end.SetActive(true);
        win.SetActive(true);
        yield return new WaitForSecondsRealtime(wait);
    }
    public IEnumerator Lose()
    {
        message.text = "�ΐ�ɕ����Ă��܂���...";
        end.SetActive(true);
        lose.SetActive(true);
        yield return new WaitForSecondsRealtime(wait);
    }
}
