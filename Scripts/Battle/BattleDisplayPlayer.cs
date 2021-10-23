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
            error1.text = "どく";
            error1.color = Type.TypeToColor("どく");
        }
        else if (effect.error == 2)
        {
            error1.text = "もうどく" + effect.errorTurn;
            error1.color = Type.TypeToColor("どく");
        }
        else if (effect.error == 3)
        {
            error1.text = "まひ";
            error1.color = Type.TypeToColor("でんき");
        }
        else if (effect.error == 4)
        {
            error1.text = "やけど";
            error1.color = Type.TypeToColor("ほのお");
        }
        else if (effect.error == 5)
        {
            error1.text = "こおり" + effect.errorTurn;
            error1.color = Type.TypeToColor("こおり");
        }
        if (effect.error == 6)
        {
            error1.text = "ちんもく";
            error1.color = Type.TypeToColor("ゴースト");
        }
        if (effect.error == 7)
        {
            error1.text = "ねむり" + effect.errorTurn;
            error1.color = Type.TypeToColor("エスパー");
        }

        if (effect.rank[0] != 0) status.text += "" + effect.rank[0] + "\n";
        if (effect.rank[1] != 0) status.text += "こうげき" + effect.rank[1] + "\n";
        if (effect.rank[2] != 0) status.text += "ぼうぎょ" + effect.rank[2] + "\n";
        if (effect.rank[3] != 0) status.text += "とくこう" + effect.rank[3] + "\n";
        if (effect.rank[4] != 0) status.text += "とくぼう" + effect.rank[4] + "\n";
        if (effect.rank[5] != 0) status.text += "すばやさ" + effect.rank[5] + "\n";
        if (effect.rank[6] != 0) status.text += "めいちゅう" + effect.rank[6] + "\n";
        if (effect.rank[7] != 0) status.text += "かいひ" + effect.rank[7] + "\n";

        if (effect.buffer[0] > 0) error2.text += "みがわり" + "\n";
        if (effect.buffer[1] > 0) error2.text += "まもる" + "\n";
        if (effect.buffer[2] > 0) error2.text += "まるくなる" + "\n";
        if (effect.buffer[3] > 0) error2.text += "みちづれ" + "\n";
        if (effect.buffer[4] > 0) error2.text += "バーサク" + effect.buffer[4] + "\n";
        if (effect.buffer[5] > 0) error2.text += "じゅうでん" + "\n";
        if (effect.buffer[6] > 0) error2.text += "たくわえる" + effect.buffer[6] + "\n";
        if (effect.buffer[7] > 0) error2.text += "こらえる" + "\n";
        if (effect.buffer[8] > 0) error2.text += "ためる" + "\n";
        if (effect.buffer[9] > 0) error2.text += "さわぐ" + effect.buffer[9] + "\n";
        if (effect.buffer[10] > 0) error2.text += "スタン" + "\n";
        if (effect.buffer[11] > 0) error2.text += "ニードルガード" + "\n";
        if (effect.buffer[12] > 0) error2.text += "はねやすめ" + "\n";

        if (effect.error2[0] > 0) error2.text += "こんらん" + effect.error2[0] + "\n";
        if (effect.error2[1] > 0) error2.text += "ひるみ" + "\n";
        if (effect.error2[2] > 0) error2.text += "バインド" + effect.error2[2] + "\n";
        if (effect.error2[3] > 0) error2.text += "のろい" + "\n";
        if (effect.error2[4] > 0) error2.text += "メロメロ" + "\n";
        if (effect.error2[5] > 0) error2.text += "やどりぎ" + "\n";
        if (effect.error2[6] > 0) error2.text += "ねむけ" + "\n";
        if (effect.error2[7] > 0) error2.text += "ほろび" + effect.error2[7] + "\n";
        if (effect.error2[8] > 0) error2.text += "とくせいなし" + "\n";
        if (effect.error2[9] > 0) error2.text += "アンコール" + effect.error2[9] + "\n";
        if (effect.error2[10] > 0) error2.text += "いちゃもん" + "\n";
        if (effect.error2[11] > 0) error2.text += "ちょうはつ" + effect.error2[11] + "\n";
        if (effect.error2[12] > 0) error2.text += "にげられない" + effect.error2[12] + "\n";
        if (effect.error2[13] > 0) error2.text += "かなしばり" + effect.error2[13] + "\n";
        item.SetActive(effect.item);
        if (chara.sprite.name != "ゆがみん" && effect.buffer[0] > 0)
        {

            for (int i = 1; i <= 20; i++)
            {
                chara.transform.localPosition = new Vector3(0, -850 - 40 * i, 0);
                yield return new WaitForSecondsRealtime(0.01f);
            }
            yield return ImageLoad(chara, "ゆがみん");
            Debug.Log(chara.transform.localPosition.y);
            for (int i = 1; i <= 20; i++)
            {
                chara.transform.localPosition = new Vector3(0, -1650 + 40 * i, 0);
                yield return new WaitForSecondsRealtime(0.01f);
            }
        }
        if (chara.sprite.name == "ゆがみん" && effect.buffer[0] == 0)
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
                s = "ステータスアップ";
                SEManager.Instance.Play(SEPath.UP);
            }
            if (effect.effect == "down")
            {
                a = anime[1];
                s = "ステータスダウン";
                SEManager.Instance.Play(SEPath.DOWN);
            }
            if (effect.effect == "どく")
            {
                a = anime[2];
                s = "どく";
                SEManager.Instance.Play(SEPath.POISON);
            }
            if (effect.effect == "もうどく")
            {
                a = anime[2];
                s = "どく";
                SEManager.Instance.Play(SEPath.POISON);
            }
            if (effect.effect == "まひ")
            {
                a = anime[3];
                s = "まひ";
                SEManager.Instance.Play(SEPath.THUNDER);
            }
            if (effect.effect == "やけど")
            {
                a = anime[4];
                s = "やけど";
                SEManager.Instance.Play(SEPath.FIRE);
            }
            if (effect.effect == "こおり")
            {
                a = anime[5];
                s = "こおり";
                SEManager.Instance.Play(SEPath.ICE);
            }
            if (effect.effect == "ちんもく")
            {
                a = anime[6];
                s = "ちんもく";
                SEManager.Instance.Play(SEPath.SILENCE);
            }
            if (effect.effect == "ねむり")
            {
                a = anime[7];
                s = "ねむり";
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
        //0おいかぜ 1ねがいごと 2オーロラ 3こころのかべ 4リフレクター 5アクアリング 6しろいきり <> 10ステロ 11どくびし 12ねばねば
        area.text = "";

        if (a[0] > 0) area.text += "おいかぜ" + a[0] + "\n";
        if (a[1] > 0) area.text += "ねがいごと" + a[1] + "\n";
        if (a[2] > 0) area.text += "オーロラ" + a[2] + "\n";
        if (a[3] > 0) area.text += "こころのかべ" + a[3] + "\n";
        if (a[4] > 0) area.text += "リフレクター" + a[4] + "\n";
        if (a[5] > 0) area.text += "アクアリング" + a[5] + "\n";
        if (a[6] > 0) area.text += "しろいきり" + a[6] + "\n";
        if (a[10] > 0) area.text += "ステルスロック" + "\n";
        if (a[11] > 0) area.text += "どくびし" + a[11] + "\n";
        if (a[12] > 0) area.text += "ねばねばネット" +  "\n";

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

        if (chara.sprite.name != "ゆがみん" && summon.buffer[0] > 0)
        {

            for (int i = 1; i <= 20; i++)
            {
                chara.transform.localPosition = new Vector3(0, -850 - 40 * i, 0);
                yield return new WaitForSecondsRealtime(0.01f);
            }
            yield return ImageLoad(chara, "ゆがみん");
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
        if (summon.sex == "男性")
        {
            sex.text = "♂";
            sex.color = new Color(0,0,1);
        }
        else if (summon.sex == "女性")
        {
            sex.text = "♀";
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
            error1.text = "どく";
            error1.color = Type.TypeToColor("どく");
        }
        else if (summon.error == 2)
        {
            error1.text = "どくどく" + summon.errorTurn;
            error1.color = Type.TypeToColor("どく");
        }
        else if (summon.error == 3)
        {
            error1.text = "まひ";
            error1.color = Type.TypeToColor("でんき");
        }
        else if (summon.error == 4)
        {
            error1.text = "やけど";
            error1.color = Type.TypeToColor("ほのお");
        }
        else if (summon.error == 5)
        {
            error1.text = "こおり" + summon.errorTurn;
            error1.color = Type.TypeToColor("こおり");
        }
        if (summon.error == 6)
        {
            error1.text = "ちんもく";
            error1.color = Type.TypeToColor("ゴースト");
        }
        if (summon.error == 7)
        {
            error1.text = "ねむり" + summon.errorTurn;
            error1.color = Type.TypeToColor("エスパー");
        }

        if (summon.rank[0] != 0) status.text += "" + summon.rank[0] + "\n";
        if (summon.rank[1] != 0) status.text += "こうげき" + summon.rank[1] + "\n";
        if (summon.rank[2] != 0) status.text += "ぼうぎょ" + summon.rank[2] + "\n";
        if (summon.rank[3] != 0) status.text += "とくこう" + summon.rank[3] + "\n";
        if (summon.rank[4] != 0) status.text += "とくぼう" + summon.rank[4] + "\n";
        if (summon.rank[5] != 0) status.text += "すばやさ" + summon.rank[5] + "\n";
        if (summon.rank[6] != 0) status.text += "めいちゅう" + summon.rank[6] + "\n";
        if (summon.rank[7] != 0) status.text += "かいひ" + summon.rank[7] + "\n";

        if (summon.buffer[0] > 0) error2.text += "みがわり" + "\n";
        if (summon.buffer[1] > 0) error2.text += "まもる" + "\n";
        if (summon.buffer[2] > 0) error2.text += "まるくなる" + "\n";
        if (summon.buffer[3] > 0) error2.text += "みちづれ" + "\n";
        if (summon.buffer[4] > 0) error2.text += "バーサク" + summon.buffer[4] + "\n";
        if (summon.buffer[5] > 0) error2.text += "じゅうでん" + "\n";
        if (summon.buffer[6] > 0) error2.text += "たくわえる" + summon.buffer[6] + "\n";
        if (summon.buffer[7] > 0) error2.text += "こらえる" + "\n";
        if (summon.buffer[8] > 0) error2.text += "ためる" + "\n";
        if (summon.buffer[9] > 0) error2.text += "さわぐ" + summon.buffer[9] + "\n";
        if (summon.buffer[10] > 0) error2.text += "スタン" + "\n";
        if (summon.buffer[11] > 0) error2.text += "ニードルガード" + "\n";
        if (summon.buffer[12] > 0) error2.text += "はねやすめ" + "\n";

        if (summon.error2[0] > 0) error2.text += "こんらん" + summon.error2[0] + "\n";
        if (summon.error2[1] > 0) error2.text += "ひるみ" + "\n";
        if (summon.error2[2] > 0) error2.text += "バインド" + summon.error2[2] + "\n";
        if (summon.error2[3] > 0) error2.text += "のろい" + "\n";
        if (summon.error2[4] > 0) error2.text += "メロメロ" + "\n";
        if (summon.error2[5] > 0) error2.text += "やどりぎ" + "\n";
        if (summon.error2[6] > 0) error2.text += "ねむけ" + "\n";
        if (summon.error2[7] > 0) error2.text += "ほろび" + summon.error2[7] + "\n";
        if (summon.error2[8] > 0) error2.text += "とくせいなし" + "\n";
        if (summon.error2[9] > 0) error2.text += "アンコール" + summon.error2[9] + "\n";
        if (summon.error2[10] > 0) error2.text += "いちゃもん" + "\n";
        if (summon.error2[11] > 0) error2.text += "ちょうはつ" + summon.error2[11] + "\n";
        if (summon.error2[12] > 0) error2.text += "にげられない" + summon.error2[12] + "\n";
        if (summon.error2[13] > 0) error2.text += "かなしばり" + summon.error2[13] + "\n";

        yield return HpBarIE(summon.hpPer);

        if (chara.sprite.name != "ゆがみん" && summon.buffer[0] > 0)
        {

            for (int i = 1; i <= 20; i++)
            {
                chara.transform.localPosition = new Vector3(0, -850 - 40 * i, 0);
                yield return new WaitForSecondsRealtime(0.01f);
            }
            yield return ImageLoad(chara, "ゆがみん");
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
