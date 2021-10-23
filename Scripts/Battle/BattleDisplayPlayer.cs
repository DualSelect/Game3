using BattleJson;
using KanKikuchi.AudioManager;
using System.Collections;
using System.Collections.Generic;
using TypeUtil;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

public class BattleDisplayPlayer : MonoBehaviour
{
    public bool enemy;
    public new Text name;
    public Text sex;
    public GameObject item;
    public Image hp;
    public Text hpText;
    public Text error1;
    public Text error2;
    public Text status;
    public Text area;
    public Image chara;
    public Image ball;
    public List<Animator> anime;

    IEnumerator HpBarIE(float h)
    {
        for(int i = 1; i <= 50; i++)
        {
            hp.GetComponent<RectTransform>().sizeDelta = new Vector2(i*h*10, 45);
            yield return new WaitForSecondsRealtime(0.001f);
        }
    }
    IEnumerator DamageHpBarIE(Damage damage)
    {
        hpText.text = damage.hpText;
        float h = damage.hpPer * 500 - hp.GetComponent<RectTransform>().sizeDelta.x;
        for (int i = 1; i <= 50; i++)
        {
            hp.GetComponent<RectTransform>().sizeDelta = new Vector2(hp.GetComponent<RectTransform>().sizeDelta.x + h / 50, 45);
            yield return new WaitForSecondsRealtime(0.001f);
        }
    }
    public IEnumerator DamageIE(Damage damage)
    {
        StartCoroutine(DamageEffect(damage));
        StartCoroutine(DamageHpBarIE(damage));
        yield return new WaitForSecondsRealtime(0.5f);
    }
    IEnumerator DamageEffect(Damage damage)
    {
        if (damage.typePer > 100)
        {
            SEManager.Instance.Play(SEPath.DAMAGE_L);
            chara.transform.localPosition = new Vector3(-10, -850, 0);
            yield return new WaitForSecondsRealtime(0.01f);
            chara.transform.localPosition = new Vector3(10, -850, 0);
            yield return new WaitForSecondsRealtime(0.01f);
            chara.transform.localPosition = new Vector3(-10, -850, 0);
            yield return new WaitForSecondsRealtime(0.01f);
            chara.transform.localPosition = new Vector3(10, -850, 0);
            yield return new WaitForSecondsRealtime(0.01f);
            chara.transform.localPosition = new Vector3(0, -850, 0);
        }
        else if (damage.typePer == 100)
        {
            SEManager.Instance.Play(SEPath.DAMAGE_M);
            chara.transform.localPosition = new Vector3(-10, -850, 0);
            yield return new WaitForSecondsRealtime(0.01f);
            chara.transform.localPosition = new Vector3(10, -850, 0);
            yield return new WaitForSecondsRealtime(0.01f);
            chara.transform.localPosition = new Vector3(0, -850, 0);
        }
        else if (100 > damage.typePer && damage.typePer > 0)
        {
            SEManager.Instance.Play(SEPath.DAMAGE_S);
            chara.transform.localPosition = new Vector3(-5, -850, 0);
            yield return new WaitForSecondsRealtime(0.01f);
            chara.transform.localPosition = new Vector3(5, -850, 0);
            yield return new WaitForSecondsRealtime(0.01f);
            chara.transform.localPosition = new Vector3(0, -850, 0);
        }
    }
    public IEnumerator EffectIE(Effect effect)
    {
        status.text = "";
        error1.text = "";
        error2.text = "";
        if (effect.error == 0)
        {
            error1.text = "";
        }
        else if (effect.error == 1)
        {
            error1.text = "�ǂ�";
            error1.color = Type.TypeToColor("�ǂ�");
        }
        else if (effect.error == 2)
        {
            error1.text = "�����ǂ�" + effect.errorTurn;
            error1.color = Type.TypeToColor("�ǂ�");
        }
        else if (effect.error == 3)
        {
            error1.text = "�܂�";
            error1.color = Type.TypeToColor("�ł�");
        }
        else if (effect.error == 4)
        {
            error1.text = "�₯��";
            error1.color = Type.TypeToColor("�ق̂�");
        }
        else if (effect.error == 5)
        {
            error1.text = "������" + effect.errorTurn;
            error1.color = Type.TypeToColor("������");
        }
        if (effect.error == 6)
        {
            error1.text = "�������";
            error1.color = Type.TypeToColor("�S�[�X�g");
        }
        if (effect.error == 7)
        {
            error1.text = "�˂ނ�" + effect.errorTurn;
            error1.color = Type.TypeToColor("�G�X�p�[");
        }

        if (effect.rank[0] != 0) status.text += "" + effect.rank[0] + "\n";
        if (effect.rank[1] != 0) status.text += "��������" + effect.rank[1] + "\n";
        if (effect.rank[2] != 0) status.text += "�ڂ�����" + effect.rank[2] + "\n";
        if (effect.rank[3] != 0) status.text += "�Ƃ�����" + effect.rank[3] + "\n";
        if (effect.rank[4] != 0) status.text += "�Ƃ��ڂ�" + effect.rank[4] + "\n";
        if (effect.rank[5] != 0) status.text += "���΂₳" + effect.rank[5] + "\n";
        if (effect.rank[6] != 0) status.text += "�߂����イ" + effect.rank[6] + "\n";
        if (effect.rank[7] != 0) status.text += "������" + effect.rank[7] + "\n";

        if (effect.buffer[0] > 0) error2.text += "�݂����" + "\n";
        if (effect.buffer[1] > 0) error2.text += "�܂���" + "\n";
        if (effect.buffer[2] > 0) error2.text += "�܂邭�Ȃ�" + "\n";
        if (effect.buffer[3] > 0) error2.text += "�݂��Â�" + "\n";
        if (effect.buffer[4] > 0) error2.text += "�o�[�T�N" + effect.buffer[4] + "\n";
        if (effect.buffer[5] > 0) error2.text += "���イ�ł�" + "\n";
        if (effect.buffer[6] > 0) error2.text += "�����킦��" + effect.buffer[6] + "\n";
        if (effect.buffer[7] > 0) error2.text += "���炦��" + "\n";
        if (effect.buffer[8] > 0) error2.text += "���߂�" + "\n";
        if (effect.buffer[9] > 0) error2.text += "���킮" + effect.buffer[9] + "\n";
        if (effect.buffer[10] > 0) error2.text += "�X�^��" + "\n";
        if (effect.buffer[11] > 0) error2.text += "�j�[�h���K�[�h" + "\n";
        if (effect.buffer[12] > 0) error2.text += "�͂˂₷��" + "\n";

        if (effect.error2[0] > 0) error2.text += "������" + effect.error2[0] + "\n";
        if (effect.error2[1] > 0) error2.text += "�Ђ��" + "\n";
        if (effect.error2[2] > 0) error2.text += "�o�C���h" + effect.error2[2] + "\n";
        if (effect.error2[3] > 0) error2.text += "�̂낢" + "\n";
        if (effect.error2[4] > 0) error2.text += "��������" + "\n";
        if (effect.error2[5] > 0) error2.text += "��ǂ肬" + "\n";
        if (effect.error2[6] > 0) error2.text += "�˂ނ�" + "\n";
        if (effect.error2[7] > 0) error2.text += "�ق��" + effect.error2[7] + "\n";
        if (effect.error2[8] > 0) error2.text += "�Ƃ������Ȃ�" + "\n";
        if (effect.error2[9] > 0) error2.text += "�A���R�[��" + effect.error2[9] + "\n";
        if (effect.error2[10] > 0) error2.text += "���������" + "\n";
        if (effect.error2[11] > 0) error2.text += "���傤�͂�" + effect.error2[11] + "\n";
        if (effect.error2[12] > 0) error2.text += "�ɂ����Ȃ�" + effect.error2[12] + "\n";
        if (effect.error2[13] > 0) error2.text += "���Ȃ��΂�" + effect.error2[13] + "\n";
        item.SetActive(effect.item);
        if (chara.sprite.name != "�䂪�݂�" && effect.buffer[0] > 0)
        {

            for (int i = 1; i <= 20; i++)
            {
                chara.transform.localPosition = new Vector3(0, -850 - 40 * i, 0);
                yield return new WaitForSecondsRealtime(0.01f);
            }
            yield return ImageLoad(chara, "�䂪�݂�");
            Debug.Log(chara.transform.localPosition.y);
            for (int i = 1; i <= 20; i++)
            {
                chara.transform.localPosition = new Vector3(0, -1650 + 40 * i, 0);
                yield return new WaitForSecondsRealtime(0.01f);
            }
        }
        if (chara.sprite.name == "�䂪�݂�" && effect.buffer[0] == 0)
        {

            for (int i = 1; i <= 20; i++)
            {
                chara.transform.localPosition = new Vector3(0, -850 - 40 * i, 0);
                yield return new WaitForSecondsRealtime(0.01f);
            }
            yield return ImageLoad(chara, effect.name + "B");
            for (int i = 1; i <= 20; i++)
            {
                chara.transform.localPosition = new Vector3(0, -1650 + 40 * i, 0);
                yield return new WaitForSecondsRealtime(0.01f);
            }
        }
        if (effect.effect != null && effect.effect!="")
        {
            Animator a = null;
            string s = "";
            if (effect.effect == "up")
            {
                a = anime[0];
                s = "�X�e�[�^�X�A�b�v";
                SEManager.Instance.Play(SEPath.UP);
            }
            if (effect.effect == "down")
            {
                a = anime[1];
                s = "�X�e�[�^�X�_�E��";
                SEManager.Instance.Play(SEPath.DOWN);
            }
            if (effect.effect == "�ǂ�")
            {
                a = anime[2];
                s = "�ǂ�";
                SEManager.Instance.Play(SEPath.POISON);
            }
            if (effect.effect == "�����ǂ�")
            {
                a = anime[2];
                s = "�ǂ�";
                SEManager.Instance.Play(SEPath.POISON);
            }
            if (effect.effect == "�܂�")
            {
                a = anime[3];
                s = "�܂�";
                SEManager.Instance.Play(SEPath.THUNDER);
            }
            if (effect.effect == "�₯��")
            {
                a = anime[4];
                s = "�₯��";
                SEManager.Instance.Play(SEPath.FIRE);
            }
            if (effect.effect == "������")
            {
                a = anime[5];
                s = "������";
                SEManager.Instance.Play(SEPath.ICE);
            }
            if (effect.effect == "�������")
            {
                a = anime[6];
                s = "�������";
                SEManager.Instance.Play(SEPath.SILENCE);
            }
            if (effect.effect == "�˂ނ�")
            {
                a = anime[7];
                s = "�˂ނ�";
                SEManager.Instance.Play(SEPath.SLEEP);
            }
            a.gameObject.SetActive(true);
            a.Play(s);
            while (a.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f)
            {
                yield return new WaitForSeconds(0.1f);
            }
            a.gameObject.SetActive(false);

        }
    }
    public IEnumerator AreaIE(int[] a)
    {
        //0�������� 1�˂������� 2�I�[���� 3������̂��� 4���t���N�^�[ 5�A�N�A�����O 6���낢���� <> 10�X�e�� 11�ǂ��т� 12�˂΂˂�
        area.text = "";

        if (a[0] > 0) area.text += "��������" + a[0] + "\n";
        if (a[1] > 0) area.text += "�˂�������" + a[1] + "\n";
        if (a[2] > 0) area.text += "�I�[����" + a[2] + "\n";
        if (a[3] > 0) area.text += "������̂���" + a[3] + "\n";
        if (a[4] > 0) area.text += "���t���N�^�[" + a[4] + "\n";
        if (a[5] > 0) area.text += "�A�N�A�����O" + a[5] + "\n";
        if (a[6] > 0) area.text += "���낢����" + a[6] + "\n";
        if (a[10] > 0) area.text += "�X�e���X���b�N" + "\n";
        if (a[11] > 0) area.text += "�ǂ��т�" + a[11] + "\n";
        if (a[12] > 0) area.text += "�˂΂˂΃l�b�g" +  "\n";

        yield break;
    }

    public IEnumerator ChangeIE(Summon summon)
    {
        this.hpText.text = "";
        for (int i = 1; i <= 20; i++)
        {
            chara.color = new Color(1, 1, 1, 1 - i / 20f);
            yield return new WaitForSecondsRealtime(0.02f);
        }

        yield return ImageLoad(chara, summon.name + "B");

        for (int i = 1; i <= 20; i++)
        {
            chara.color = new Color(1, 1, 1, i / 20f);
            yield return new WaitForSecondsRealtime(0.02f);
        }

        name.text = summon.name;
        hpText.text = summon.hpText;
        yield return HpBarIE(summon.hpPer);

        if (chara.sprite.name != "�䂪�݂�" && summon.buffer[0] > 0)
        {

            for (int i = 1; i <= 20; i++)
            {
                chara.transform.localPosition = new Vector3(0, -850 - 40 * i, 0);
                yield return new WaitForSecondsRealtime(0.01f);
            }
            yield return ImageLoad(chara, "�䂪�݂�");
            for (int i = 1; i <= 20; i++)
            {
                chara.transform.localPosition = new Vector3(0, -1650 + 40 * i, 0);
                yield return new WaitForSecondsRealtime(0.01f);
            }
        }
    }
    public IEnumerator SummonIE(Summon summon)
    {
        this.hpText.text = "";
        status.text = "";
        error1.text = "";
        error2.text = "";
        hp.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 45);

        ball.gameObject.SetActive(true);

        for (int i = 1; i <= 20; i++)
        {
            chara.transform.localScale = new Vector3(1 - i / 20f, 1 - i / 20f, 1);
            chara.color = new Color(1, 1, 1, 1 - i / 20f);
            yield return new WaitForSecondsRealtime(0.02f);
        }

        yield return ImageLoad(chara,summon.name+"B");

        for (int i = 1; i <= 20; i++)
        {
            chara.transform.localScale = new Vector3(i / 20f,  i / 20f, 1);
            chara.color = new Color(1, 1, 1,  i / 20f);
            yield return new WaitForSecondsRealtime(0.02f);
        }

        ball.gameObject.SetActive(false);

        name.text = summon.name;
        if (summon.sex == "�j��")
        {
            sex.text = "��";
            sex.color = new Color(0,0,1);
        }
        else if (summon.sex == "����")
        {
            sex.text = "��";
            sex.color = new Color(1, 0, 0);
        }
        else
        {
            sex.text = "";
        }
        item.SetActive(summon.item);
        hpText.text = summon.hpText;
        if(summon.error == 0)
        {
            error1.text = "";
        }
        else if(summon.error == 1)       
        {
            error1.text = "�ǂ�";
            error1.color = Type.TypeToColor("�ǂ�");
        }
        else if (summon.error == 2)
        {
            error1.text = "�ǂ��ǂ�" + summon.errorTurn;
            error1.color = Type.TypeToColor("�ǂ�");
        }
        else if (summon.error == 3)
        {
            error1.text = "�܂�";
            error1.color = Type.TypeToColor("�ł�");
        }
        else if (summon.error == 4)
        {
            error1.text = "�₯��";
            error1.color = Type.TypeToColor("�ق̂�");
        }
        else if (summon.error == 5)
        {
            error1.text = "������" + summon.errorTurn;
            error1.color = Type.TypeToColor("������");
        }
        if (summon.error == 6)
        {
            error1.text = "�������";
            error1.color = Type.TypeToColor("�S�[�X�g");
        }
        if (summon.error == 7)
        {
            error1.text = "�˂ނ�" + summon.errorTurn;
            error1.color = Type.TypeToColor("�G�X�p�[");
        }

        if (summon.rank[0] != 0) status.text += "" + summon.rank[0] + "\n";
        if (summon.rank[1] != 0) status.text += "��������" + summon.rank[1] + "\n";
        if (summon.rank[2] != 0) status.text += "�ڂ�����" + summon.rank[2] + "\n";
        if (summon.rank[3] != 0) status.text += "�Ƃ�����" + summon.rank[3] + "\n";
        if (summon.rank[4] != 0) status.text += "�Ƃ��ڂ�" + summon.rank[4] + "\n";
        if (summon.rank[5] != 0) status.text += "���΂₳" + summon.rank[5] + "\n";
        if (summon.rank[6] != 0) status.text += "�߂����イ" + summon.rank[6] + "\n";
        if (summon.rank[7] != 0) status.text += "������" + summon.rank[7] + "\n";

        if (summon.buffer[0] > 0) error2.text += "�݂����" + "\n";
        if (summon.buffer[1] > 0) error2.text += "�܂���" + "\n";
        if (summon.buffer[2] > 0) error2.text += "�܂邭�Ȃ�" + "\n";
        if (summon.buffer[3] > 0) error2.text += "�݂��Â�" + "\n";
        if (summon.buffer[4] > 0) error2.text += "�o�[�T�N" + summon.buffer[4] + "\n";
        if (summon.buffer[5] > 0) error2.text += "���イ�ł�" + "\n";
        if (summon.buffer[6] > 0) error2.text += "�����킦��" + summon.buffer[6] + "\n";
        if (summon.buffer[7] > 0) error2.text += "���炦��" + "\n";
        if (summon.buffer[8] > 0) error2.text += "���߂�" + "\n";
        if (summon.buffer[9] > 0) error2.text += "���킮" + summon.buffer[9] + "\n";
        if (summon.buffer[10] > 0) error2.text += "�X�^��" + "\n";
        if (summon.buffer[11] > 0) error2.text += "�j�[�h���K�[�h" + "\n";
        if (summon.buffer[12] > 0) error2.text += "�͂˂₷��" + "\n";

        if (summon.error2[0] > 0) error2.text += "������" + summon.error2[0] + "\n";
        if (summon.error2[1] > 0) error2.text += "�Ђ��" + "\n";
        if (summon.error2[2] > 0) error2.text += "�o�C���h" + summon.error2[2] + "\n";
        if (summon.error2[3] > 0) error2.text += "�̂낢" + "\n";
        if (summon.error2[4] > 0) error2.text += "��������" + "\n";
        if (summon.error2[5] > 0) error2.text += "��ǂ肬" + "\n";
        if (summon.error2[6] > 0) error2.text += "�˂ނ�" + "\n";
        if (summon.error2[7] > 0) error2.text += "�ق��" + summon.error2[7] + "\n";
        if (summon.error2[8] > 0) error2.text += "�Ƃ������Ȃ�" + "\n";
        if (summon.error2[9] > 0) error2.text += "�A���R�[��" + summon.error2[9] + "\n";
        if (summon.error2[10] > 0) error2.text += "���������" + "\n";
        if (summon.error2[11] > 0) error2.text += "���傤�͂�" + summon.error2[11] + "\n";
        if (summon.error2[12] > 0) error2.text += "�ɂ����Ȃ�" + summon.error2[12] + "\n";
        if (summon.error2[13] > 0) error2.text += "���Ȃ��΂�" + summon.error2[13] + "\n";

        yield return HpBarIE(summon.hpPer);

        if (chara.sprite.name != "�䂪�݂�" && summon.buffer[0] > 0)
        {

            for (int i = 1; i <= 20; i++)
            {
                chara.transform.localPosition = new Vector3(0, -850 - 40 * i, 0);
                yield return new WaitForSecondsRealtime(0.01f);
            }
            yield return ImageLoad(chara, "�䂪�݂�");
            for (int i = 1; i <= 20; i++)
            {
                chara.transform.localPosition = new Vector3(0, -1650 + 40 * i, 0);
                yield return new WaitForSecondsRealtime(0.01f);
            }
        }
    }

    public IEnumerator DeathIE()
    {
        yield return new WaitForSecondsRealtime(1f);
        for (int i = 1; i <= 20; i++)
        {
            chara.transform.localPosition = new Vector3(0, -850 - 40 * i, 0);
            yield return new WaitForSecondsRealtime(0.01f);
        }
        chara.color = new Color(1, 1, 1, 0);
        chara.transform.localPosition = new Vector3(0, -850, 0);
    }
    IEnumerator ImageLoad(Image image, string name)
    {
        var icon = Addressables.LoadAssetAsync<Sprite>(name);
        yield return icon;
        image.sprite = icon.Result;
    }
}
