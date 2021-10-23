using BattleJson;
using System.Collections;
using System.Collections.Generic;
using TypeUtil;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    public PunBattle punBattle;
    public CharaMaster charaMaster;
    public SkillMaster skillMaster;
    public ItemMaster itemMaster;
    List<CharaStatus> playerParty = new List<CharaStatus>();
    List<CharaStatus> enemyParty = new List<CharaStatus>();
    public List<CharaStatus> playerMember = new List<CharaStatus>();
    public List<CharaStatus> enemyMember = new List<CharaStatus>();
    public CharaStatus battlerP;
    public CharaStatus battlerE;
    Select playerSelect = null;
    Select enemySelect = null;
    Select playerChange = null;
    Select enemyChange = null;
    public float[] correctP = new float[3] { 0, 0, 0 };
    public float[] correctE = new float[3] { 0, 0, 0 };
    public Field field = new Field();
    bool hitCheck;
    int prevSkillDamage;
    string prevSkillNo = "";
    Skill playerSkill;
    Skill enemySkill;
    public int turn;
    public bool order;

    public List<CharaStatus> deathOrder = new List<CharaStatus>();
    IEnumerator BattleIE()
    {
        turn = 0;
        while (true)
        {
            turn++;
            if (turn == 1)
            {
                yield return Summon(true, 0,"");
                yield return Summon(false, 0,"");
                battlerP.enemy = battlerE;
                battlerE.enemy = battlerP;
                for (int i = 0; i < 6; i++)
                {
                    yield return playerParty[i].GameFirst();
                    yield return enemyParty[i].GameFirst();
                }
                if (ChangeOrder())
                {
                    yield return FirstSummonPassive(battlerP);
                    yield return FirstSummonPassive(battlerE);
                }
                else
                {
                    yield return FirstSummonPassive(battlerE);
                    yield return FirstSummonPassive(battlerP);
                }
            }
            punBattle.Send("turn", turn.ToString());

            battlerP.changeTurn = false;
            battlerE.changeTurn = false;
            //選択を要求
            playerSelect = null;
            enemySelect = null;
            if(battlerP.buffer[4] == 0 && battlerP.buffer[8] == 0 && battlerP.buffer[9] == 0 && battlerP.buffer[10] == 0) punBattle.Send("select", "", true);
            else  playerSelect = new Select() { member = false ,skill =true,num=battlerP.prevSkill,player=true };
            if (battlerE.buffer[4] == 0 && battlerP.buffer[8] == 0 && battlerE.buffer[9] == 0 && battlerE.buffer[10] == 0) punBattle.Send("select", "", false);
            else enemySelect = new Select() { member = false, skill = true, num = battlerE.prevSkill, player = false };
            while (playerSelect == null || enemySelect == null)
            {
                yield return new WaitForSecondsRealtime(0.1f);
            }
            //技と交代
            playerSkill = null;
            enemySkill = null;
            CharaStatus csP = battlerP;
            CharaStatus csE = battlerE;
            if (playerSelect.skill)
            {
                if(playerSelect.num==4) playerSkill = skillMaster.SkillList.Find(a => a.no == "0");
                else playerSkill = skillMaster.SkillList.Find(a => a.no == battlerP.skill[playerSelect.num]);
            }
            if (enemySelect.skill)
            {
                if (enemySelect.num == 4) enemySkill = skillMaster.SkillList.Find(a => a.no == "0");
                else enemySkill = skillMaster.SkillList.Find(a => a.no == battlerE.skill[enemySelect.num]);
            }
            if(playerSelect.skill && enemySelect.skill)
            {
                yield return SkillOrder(battlerP, battlerE, playerSkill, enemySkill);
                if (order)
                {
                    battlerP.fastAttack = true;
                    yield return SkillIE(true, playerSkill,playerSelect.num);
                    battlerP.firstAttack = false;
                    if (battlerE.error != 8 && battlerE == csE)
                    {
                        battlerE.fastAttack = false;
                        yield return SkillIE(false, enemySkill,enemySelect.num);
                        battlerE.firstAttack = false;
                    }
                }
                else
                {
                    battlerE.fastAttack = true;
                    yield return SkillIE(false, enemySkill, enemySelect.num);
                    battlerE.firstAttack = false;
                    if (battlerP.error != 8 && battlerP == csP)
                    {
                        battlerP.fastAttack = false;
                        yield return SkillIE(true, playerSkill, playerSelect.num);
                        battlerP.firstAttack = false;
                    }
                }
            }
            else if(playerSelect.skill && enemySelect.member)
            {
                if (playerSkill.name == "おいうち")
                {
                    yield return SkillIE(true, playerSkill, playerSelect.num);
                    battlerP.firstAttack = false;
                    if(battlerE.error!=8)yield return Summon(false, enemySelect.num,"");
                }
                else
                {
                    yield return Summon(false, enemySelect.num,"");
                    battlerP.fastAttack = false;
                    yield return SkillIE(true, playerSkill, playerSelect.num);
                    battlerP.firstAttack = false;
                }

            }
            else if(playerSelect.member && enemySelect.skill)
            {
                if (enemySkill.name == "おいうち")
                {
                    yield return SkillIE(false, enemySkill, enemySelect.num);
                    battlerE.firstAttack = false;
                    if (battlerP.error != 8) yield return Summon(true, playerSelect.num,"");
                }
                else
                {
                    yield return Summon(true, playerSelect.num,"");
                    battlerE.fastAttack = false;
                    yield return SkillIE(false, enemySkill, enemySelect.num);
                    battlerE.firstAttack = false;
                }
            }
            else if(playerSelect.member && enemySelect.member)
            {
                if (ChangeOrder())
                {
                   yield return  Summon(true, playerSelect.num,"");
                   yield return  Summon(false, enemySelect.num,"");
                }
                else
                {
                   yield return  Summon(false, enemySelect.num,"");
                   yield return  Summon(true, playerSelect.num,"");
                }
            }
            //ターン終了時
            if (ChangeOrder())
            {
                yield return battlerP.TurnEndEffect();
                yield return battlerE.TurnEndEffect();
            }
            else
            {
                yield return battlerE.TurnEndEffect();
                yield return battlerP.TurnEndEffect();
            }
            foreach (CharaStatus cs in playerMember)
            {
                yield return cs.TurnEndAll();
            }
            foreach (CharaStatus cs in enemyMember)
            {
                yield return cs.TurnEndAll();
            }
            yield return field.TurnEndEffect();
            yield return DeathChange();
            if (ChangeOrder())
            {
                yield return battlerP.TurnEndAfter();
                yield return battlerE.TurnEndAfter();
            }
            else
            {
                yield return battlerE.TurnEndAfter();
                yield return battlerP.TurnEndAfter();
            }
            yield return field.TurnEndAfter();
        }
    }
    IEnumerator SkillOrder(CharaStatus player, CharaStatus enemy, Skill playerSkill, Skill enemySkill)
    {
        bool ord;
        float playerSpeed = battlerP.RankStatus(5);
        float enemySpeed = battlerE.RankStatus(5);
        if (ItemName(player.item) == "せんせいのツメ")
        {
            if (PerCorrect(player.master, 25))
            {
                Message(ItemText(player)  + "行動が速くなった！");
                playerSpeed = 999;
                if (field.both[2] > 0) playerSpeed = 0;
            }
        }
        if (ItemName(enemy.item) == "せんせいのツメ")
        {
            if (PerCorrect(enemy.master, 25))
            {
                Message(ItemText(enemy) + "行動が速くなった！");
                enemySpeed = 999;
                if (field.both[2] > 0) enemySpeed = 0;
            }
        }
        if (ItemName(player.item) == "イバンのみ" && player.HPPer()<=0.25f)
        {
            Message(ItemText(player) + "行動が速くなった！");
            yield return ItemUse(player);
            playerSpeed = 999;
            if (field.both[2] > 0) playerSpeed = 0;
        }
        if (ItemName(enemy.item) == "イバンのみ" && enemy.HPPer() <= 0.25f)
        {
            Message(ItemText(enemy) + "行動が速くなった！");
            yield return ItemUse(enemy);
            enemySpeed = 999;
            if (field.both[2] > 0) enemySpeed = 0;
        }


        if (field.both[2] > 0)
        {
            playerSpeed *= -1;
            enemySpeed *= -1;
        }
        int playerSkillSpeed = playerSkill.speed;
        int enemySkillSpeed = enemySkill.speed;

        if (player.passiveName == "いたずらごころ" && playerSkillSpeed == 0 && playerSkill.physics == "変化")
        {
            playerSkillSpeed = 1;
            Message(PassiveText(player)+"変化技の優先度を1にした");
        }
        if (enemy.passiveName == "いたずらごころ" && enemySkillSpeed == 0 && enemySkill.physics == "変化")
        {
            enemySkillSpeed = 1;
            Message(PassiveText(enemy) + "変化技の優先度を1にした");
        }
        if (playerSkillSpeed > enemySkillSpeed) ord = true;
        else if (playerSkillSpeed < enemySkillSpeed) ord = false;
        else
        {
            if (playerSpeed == enemySpeed)
            {
                ord = Random.Range(0, 2) == 0;
            }
            else
            {
                ord = playerSpeed > enemySpeed;
            }
        }
        order = ord;
    }
    bool ChangeOrder()
    {
        bool order;
        float playerSpeed = battlerP.RankStatus(5);
        float enemySpeed = battlerE.RankStatus(5);

        if (playerSpeed == enemySpeed)
        {
            order = Random.Range(0, 2) == 0;
        }
        else
        {
            order = playerSpeed > enemySpeed;
        }

        return order;
    }
    IEnumerator SkillIE(bool master,Skill s,int selectNum)
    {
        Debug.Log(s.name);
        Skill skill = s.Copy();
        prevSkillDamage = 0;

        CharaStatus atkChara;
        CharaStatus defChara;
        if (master)
        {
            atkChara = battlerP;
            defChara = battlerE;
        }
        else
        {
            atkChara = battlerE;
            defChara = battlerP;
        }
        if (skill.no!="0" && atkChara.prevSkill!=-1 && (atkChara.error2[9] > 0 || ItemName(atkChara.item).Contains("こだわり"))) skill = skillMaster.SkillList.Find(a => a.no == atkChara.skill[atkChara.prevSkill]).Copy();
        if (atkChara.buffer[3] > 0)
        {
            atkChara.buffer[3] = 0;
            yield return EffectIE(defChara, atkChara, "");
        }
        if (skill.name == "ねぞうといびき") if (atkChara.RankStatus(3) > atkChara.RankStatus(1))  skill = skillMaster.SkillList.Find(a => a.no == "no-55").Copy();
        if (skill.name == "はきだす") if (atkChara.RankStatus(3) > atkChara.RankStatus(1))  skill = skillMaster.SkillList.Find(a => a.no == "no-60").Copy();
        bool yugamin = defChara.buffer[0] > 0;

        //命中判定
        Skill skillChange=null;
        yield return SkillHit(skill, atkChara, defChara,r=> skillChange = r.Copy(),selectNum);
        if (skillChange != null) skill = skillChange;
        if (!hitCheck)
        {
            atkChara.buffer[4] = 0;
            atkChara.buffer[9] = 0;
            atkChara.successNum = 0;
            atkChara.successPrev = false;
        }
        //技の結果
        if (hitCheck)
        {
            if (selectNum != atkChara.prevSkill) atkChara.successNum = 0;
            yield return SkillAttack(skill, atkChara, defChara,selectNum);
            atkChara.successNum += 1;
            //攻撃後効果
            if (skill.physics=="変化" || !yugamin || skill.sound>0 || skill.target== "自分" || skill.target == "場") yield return SkillEffect(skill, atkChara, defChara);
            atkChara.successPrev = true;
        }
    }
    IEnumerator SkillHit(Skill skill, CharaStatus atkChara, CharaStatus defChara,System.Action<Skill> callback, int selectNum)
    {
        for (int i = 0; i < 4; i++)
        {
            if (skill == skillMaster.SkillList.Find(a => a.no == atkChara.skill[i]))
            {
                selectNum = i;
            }
        }

        hitCheck = true;
        bool master = atkChara.master;
        if (atkChara.buffer[10] > 0)
        {
            atkChara.buffer[10] = 0;
            Effect effect = atkChara.ToEffect();
            effect.message = atkChara.nameC + "は　反動で動けない";
            punBattle.Send("effect", effect.ToString());
            hitCheck = false; yield break;
        }
        if (atkChara.error == 3)
        {
            float effectC;
            float per = 75;
            if (master) effectC = per + (100 - per) * correctP[1];
            else effectC = per + (100 - per) * correctE[1];
            if (effectC > Random.Range(0, 100))
            {
                if (master) correctP[1] -= (100 - per);
                else correctE[1] -= (100 - per);
            }
            else
            {
                if (master) correctP[1] += per;
                else correctE[1] += per;
                Effect effect = atkChara.ToEffect();
                effect.message = atkChara.nameC + "は　痺れて動けない";
                punBattle.Send("effect", effect.ToString());
                hitCheck = false; yield break;
            }
        }
        if (atkChara.error == 5)
        {
            if (skill.name == "せいなるほのお" && skill.name == "フレアドライブ")
            {
                atkChara.error = 0;
                atkChara.errorTurn = 0;
                Effect effect = atkChara.ToEffect();
                effect.message = atkChara.nameC + "の　氷が解けた！";
                punBattle.Send("effect", effect.ToString());
            }
            else
            {
                Effect effect = atkChara.ToEffect();
                effect.message = atkChara.nameC + "は　凍っていて動けない";
                punBattle.Send("effect", effect.ToString());
                hitCheck = false; yield break;
            }
        }
        if (atkChara.error == 6 && skill.physics == "変化")
        {
            float effectC;
            float per = 75;
            if (master) effectC = per + (100 - per) * correctP[1];
            else effectC = per + (100 - per) * correctE[1];
            if (effectC > Random.Range(0, 100))
            {
                if (master) correctP[1] -= (100 - per);
                else correctE[1] -= (100 - per);
            }
            else
            {
                if (master) correctP[1] += per;
                else correctE[1] += per;
                Effect effect = atkChara.ToEffect();
                effect.message = atkChara.nameC + "は　沈黙して技を使えない";
                punBattle.Send("effect", effect.ToString());
                hitCheck = false; yield break;
            }
        }
        if (atkChara.error == 7)
        {
            atkChara.errorTurn -= 1;
            Effect effect = atkChara.ToEffect();
            effect.message = atkChara.nameC + "は　眠っている";
            punBattle.Send("effect", effect.ToString());

            if (atkChara.errorTurn == 0)
            {
                atkChara.error = 0;
                effect = atkChara.ToEffect();
                effect.message = atkChara.nameC + "は　目を覚ました！";
                punBattle.Send("effect", effect.ToString());
            }
            else if (skill.name != "ねぞうといびき" && skill.name != "ねごと")
            {
                hitCheck = false; yield break;
            }
        }

        if (atkChara.error2[1] > 0)
        {
            atkChara.error2[1] = 0;
            Effect effect = atkChara.ToEffect();
            effect.message = atkChara.nameC + "は　怯んで動けない";
            punBattle.Send("effect", effect.ToString());
            hitCheck = false; yield break;
        }
        if (atkChara.error2[0] > 0)
        {
            Effect effect = atkChara.ToEffect();
            effect.message = atkChara.nameC + "は　混乱している";
            punBattle.Send("effect", effect.ToString());
            float effectC;
            float per = 70;
            if (master) effectC = per + (100 - per) * correctP[1];
            else effectC = per + (100 - per) * correctE[1];
            if (effectC > Random.Range(0, 100))
            {
                if (master) correctP[1] -= (100 - per);
                else correctE[1] -= (100 - per);
            }
            else
            {
                if (master) correctP[1] += per;
                else correctE[1] += per;

                float damageF = atkChara.RankStatus(1) / atkChara.RankStatus(3) * 40 * 2f / 5f;
                int damageI = (int)Mathf.Floor(damageF);
                if (damageI < 1) damageI = 1;
                yield return DamageIE(null, atkChara, atkChara.nameC + "は　訳も分からず自分を攻撃した", damageI);

                atkChara.error2[0] -= 1;
                effect = atkChara.ToEffect();
                if (atkChara.error2[0] == 0) effect.message = atkChara.nameC + "は　混乱が解けた！";
                punBattle.Send("effect", effect.ToString());
                hitCheck = false; yield break;
            }
        }
        if (atkChara.sp[selectNum] <= 0)
        {
            Message(atkChara.nameC + "は　SPが足りず技を出せない！");
            hitCheck = false; yield break;
        }

        if (atkChara.error2[10] > 0)
        {
            if (selectNum == atkChara.prevSkill)
            {
                Message(atkChara.nameC + "は　いちゃもんをつけられて同じ技を出せない！");
                hitCheck = false; yield break;
            }
        }
        if (atkChara.error2[11] > 0)
        {
            if (skill.physics=="変化")
            {
                Message(atkChara.nameC + "は　ちょうはつされていて変化技を出せない！");
                hitCheck = false; yield break;
            }
        }
        if (atkChara.error2[13] > 0)
        {
            if (selectNum == atkChara.lockSkill)
            {
                Message(atkChara.nameC + "は　かなしばりで技を出せない！");
                hitCheck = false; yield break;
            }
        }
        if (skill.name == "きあいパンチ" && (atkChara.damageP + atkChara.damageS) > 0)
        {
            Message(atkChara.nameC + "は　集中が途切れて技が出せない");
            hitCheck = false; yield break;
        }
        string text = atkChara.nameC + "の " + skill.name + "!";
        if (skill.name == "ねごと")
        {
            while (true)
            {
                Skill negoto = skillMaster.SkillList.Find(a => a.no == atkChara.skill[UnityEngine.Random.Range(0, 4)]);
                if (negoto == null) continue;
                if (negoto.name != "ねごと")
                {
                    skill = negoto;
                    callback(skill);
                    text = atkChara.nameC + "の " + "ねごと" + "!";
                    text += "\n" + skill.name + "がでた！";
                    Message(text);
                    text = atkChara.nameC + "の " + skill.name + "!";
                    break;
                }
            }
        }
        if (skill.name == "〇〇ージア")
        {
            skill.type = TypeUtil.Type.NumToType(UnityEngine.Random.Range(0, 18));
            skill.name = skill.type + "ージア";
            callback(skill);
            text = atkChara.nameC + "の " + "〇〇ージア" + "!";
            text += "\n" + skill.type + "ージア　がでた！";
            Message(text);
            text = atkChara.nameC + "の " + skill.name + "!";
        }
        if ((!atkChara.fastAttack || playerSkill.physics == "変化" || enemySkill.physics == "変化") && skill.name == "ふいうち")
        {
            text += "\n" + "しかしうまくいかなかった";
            Message(text);
            hitCheck = false; yield break;
        }
        if (!atkChara.firstAttack && (skill.name == "あいさつバトル" || skill.name == "であいがしら" || skill.name == "ねこだまし"))
        {
            text += "\n" + "しかしうまくいかなかった";
            Message(text);
            hitCheck = false; yield break;
        }
        if (defChara.error != 7 && skill.name == "ゆめくい")
        {
            text += "\n" + "しかしうまくいかなかった";
            Message(text);
            hitCheck = false; yield break;
        }
        if (atkChara.error != 7 && (skill.name == "ねぞうといびき" || skill.name == "ねごと"))
        {
            text += "\n" + "しかしうまくいかなかった";
            Message(text);
            hitCheck = false; yield break;
        }
        if (defChara.error == 8)
        {
            text += "\n" + "対象がいないようだ...";
            Message(text);
            hitCheck = false; yield break;
        }

        if (atkChara.prevSkill != -1 && atkChara.successNum > 0)
        {
            Skill prevSkill = skillMaster.SkillList.Find(a => a.no == atkChara.skill[atkChara.prevSkill]);
            if (prevSkill.name == "こらえる" || prevSkill.name == "まもる" || prevSkill.name == "ニードルガード" || prevSkill.name == "みちづれ")
            {
                if (prevSkill.name == skill.name)
                {
                    text += "\n" + "連続して使用することができない";
                    Message(text);
                    hitCheck = false; yield break;
                }
            }
        }

        atkChara.prevSkill = selectNum;
        prevSkillNo = skill.no;
        atkChara.sp[selectNum] -= 1;
        if(atkChara.passiveName=="金のポン") atkChara.sp[selectNum] -= 1;

        switch (atkChara.passiveName)
        {
            case "公式怪文書":
                if (skill.type == "あく")
                {
                    Message(text);
                    Message(PassiveText(atkChara)+"あくタイプの技を出せない！");
                    hitCheck = false; yield break;
                }
                break;
            case "清楚(にじさんじ)":
                if (skill.type == "あく")
                {
                    Message(text);
                    Message(PassiveText(atkChara) + "あくタイプの技を出せない！");
                    hitCheck = false; yield break;
                }
                break;
        }
        switch (defChara.passiveName)
        {
            case "音ゴリラ":
                if (skill.sound >0)
                {
                    Message(text);
                    Message(PassiveText(defChara) + "おとの技を無効化した！");
                    yield return defChara.StatusCheak(5,1);
                    hitCheck = false; yield break;
                }
                break;
            case "センブリ茶":
                if (skill.type == "みず" && skill.target != "自分" && skill.target != "場")
                {
                    Message(text);
                    Message(PassiveText(defChara) + "みずタイプの技を無効化した！");
                    yield return defChara.StatusCheak(1, 1);
                    yield return defChara.StatusCheak(3, 1);
                    if (defChara.error2[0] == 0)
                    {
                        defChara.error2[0] = 2;
                        yield return EffectIE(atkChara, defChara, defChara.nameC + "は　混乱した！");
                    }
                    else yield return EffectIE(atkChara, defChara, defChara.nameC + "は　すでに混乱している");
                    hitCheck = false; yield break;
                }
                break;
        }

 

        if (defChara.buffer[1] > 0 && skill.name != "フェイント" && skill.target != "自分" && skill.target != "場")
        {
            defChara.buffer[1] = 0;
            Effect effect = defChara.ToEffect();
            effect.message = atkChara.nameC + "の　" + skill.name + "！";
            effect.message += "\n" + defChara.nameC + "は　攻撃から身を守った";
            punBattle.Send("effect", effect.ToString());
            hitCheck = false; yield break;
        }
        if (defChara.buffer[11] > 0 && skill.name != "フェイント" && skill.target != "自分" && skill.target != "場" && skill.physics!="変化")
        {
            defChara.buffer[11] = 0;
            Effect effect = defChara.ToEffect();
            effect.message = atkChara.nameC + "の　" + skill.name + "！";
            effect.message += "\n" + defChara.nameC + "は　攻撃から身を守った";
            punBattle.Send("effect", effect.ToString());
            if (skill.contact > 0)
            {
                yield return DamageIE( defChara, atkChara, atkChara + "は　とげにふれてダメージを受けた", Mathf.FloorToInt(atkChara.status[0] / 8f));
            }
            hitCheck = false; yield break;
        }
        if (skill.name == "つのドリル" || skill.name == "アゴドリル" || skill.name == "ぜったいれいど" || skill.name == "じわれ" || skill.name == "○○す")
        {
            if(30 > Random.Range(0, 100)){
                hitCheck = true; yield break;
            }
            else
            {
                text += "\n" + "しかし　攻撃は　外れた";
                Message(text);
                hitCheck = false; yield break;
            }
        }

        if (skill.physics == "変化")
        {
            if(defChara.buffer[0]>0 && skill.sound==0 && skill.through == 0)
            {
                text += "\n" + "しかし　みがわりに防がれた";
                Message(text);
                hitCheck = false; yield break;
            }
        }
        else
        {
            int typePer = TypePer(skill,atkChara,defChara);
            if (typePer == 0)
            {
                text += "\n" + "こうかはないようだ...";
                Message(text);
                hitCheck = false; yield break;
            }
        }
        if(skill.name == "ロケットずつき" || skill.name == "メテオビーム")
        {
            if (atkChara.passiveName == "えいっ")
            {
                atkChara.buffer[8] = 1;
                Message(PassiveText(atkChara) + "ため技をすぐに発動する！");
            }
            if (atkChara.buffer[8] == 0) hitCheck = true; yield break;
        }
        if (skill.name == "ソーラービーム" || skill.name == "ソーラーブレード")
        {
            if (atkChara.passiveName == "えいっ")
            {
                atkChara.buffer[8] = 1;
                Message(PassiveText(atkChara) + "ため技をすぐに発動する！");
            }
            if (field.both[0] == 1) atkChara.buffer[8] = 1;
            if (atkChara.buffer[8] == 0) hitCheck = true; yield break;
        }
        float hit = skill.hit;
        if (skill.name == "かみなり" && field.both[0] == 2) hit = 0;
        if (skill.name == "ぼうふう" && field.both[0] == 2) hit = 0;
        if (skill.name == "ふぶき" && field.both[0] == 3) hit = 0;
        if (skill.name == "どくどく" && (atkChara.type1=="どく"|| atkChara.type2 == "どく")) hit = 0;
        if (atkChara.passiveName=="激アツ"&& turn % 10 == 7)
        {
            Message(PassiveText(atkChara) + "このターンは激アツだ！");
            hit = 0;
        }
        if (atkChara.passiveName == "方向音痴")hit *= 0.9f;
        if (defChara.passiveName == "キメラ") hit = 0;
        if (ItemName(atkChara.item) == "こうかくレンズ") hit *= 1.15f;
        if (ItemName(defChara.item) == "ひかりのこな") hit *= 0.9f;

        if (hit != 0)
        {
            float hitRank = atkChara.rank[6];
            float avoidRank = defChara.rank[7];
            if (field.both[4] > 0) avoidRank += 2;
            hit = skill.hit * (3 + atkChara.rank[6]) / (3 + defChara.rank[7]) * ((hitRank + 3) / (avoidRank + 3));
            if (hit > 100) hit = 100;
            if (hit < 0) hit = 0;
            float hitC;
            if (master) hitC = hit + (100 - hit) * correctP[0];
            else hitC = hit + (100 - hit) * correctE[0];
            if (hitC > Random.Range(0, 100))
            {
                if (master) correctP[0] -= (100 - hit);
                else correctE[0] -= (100 - hit);
            }
            else
            {
                if (master) correctP[0] += hit;
                else correctE[0] += hit;
                text += "\n" + "しかし　攻撃は　外れた";
                Message(text);
                hitCheck = false; yield break;
            }
        }
    }
    string AtkType(Skill skill ,CharaStatus atkChara)
    {
        string type = skill.type;
        switch (skill.name)
        {
            case "ウェザーボール":
                if (field.both[0] == 1) type = "ほのお";
                if (field.both[0] == 2) type = "みず";
                if (field.both[0] == 3) type = "いわ";
                if (field.both[0] == 4) type = "こおり";
                break;
        }
        return type;
    }
    int TypePer(Skill skill, CharaStatus atkChara,CharaStatus defChara)
    {
        int typePer;
        typePer = Type.Per(AtkType(skill, atkChara), defChara.type1, defChara.type2);
        if (skill.name == "フリーズドライ") typePer = Type.PerSkill(0, defChara.type1, defChara.type2);
        if (skill.name == "ふしょくえき") typePer = Type.PerSkill(1, defChara.type1, defChara.type2);
        if (skill.name == "いんのはどう") typePer = Type.PerSkill(2, defChara.type1, defChara.type2);
        if (skill.name == "ようのはどう") typePer = Type.PerSkill(3, defChara.type1, defChara.type2);
        if (skill.type == "じめん" && defChara.buffer[12] == 1) typePer = Type.PerSkill(4, defChara.type1, defChara.type2);
        if (defChara.passiveName== "ホラー担当" && skill.type == "ゴースト")
        {
            Area area = field.ToArea();
            area.message = PassiveText(defChara) + "ゴーストタイプの技を無効化する";
            punBattle.Send("area", area.ToString());
            typePer = 0;
        }
        if (defChara.passiveName == "清楚(にじさんじ)" && skill.type == "あく")
        {
            Area area = field.ToArea();
            area.message = PassiveText(defChara) + "あくタイプの技を無効化する";
            punBattle.Send("area", area.ToString());
            typePer = 0;
        }
        if (ItemName(defChara.item)=="ふうせん" && skill.type == "じめん")
        {
            Message(ItemText(defChara) + defChara.nameC + "は　地面技があたらない");
            typePer = 0;
        }
        if (field.both[5] > 0)
        {
            if (typePer > 100) typePer = 50;
            else if (typePer < 100) typePer = 200;

        }
        return typePer;
    }
    IEnumerator SkillAttack(Skill skill, CharaStatus atkChara, CharaStatus defChara)
    {
        yield return SkillAttack(skill,atkChara,defChara,-1);
    }
    IEnumerator SkillAttack(Skill skill, CharaStatus atkChara, CharaStatus defChara,int selectNum)
    {
        Damage damage = new Damage();
        bool master = atkChara.master;
        if (skill.name == "ロケットずつき" || skill.name == "メテオビーム" || skill.name == "ソーラービーム" || skill.name == "ソーラーブレード")
        {
            if(atkChara.buffer[8]==0)yield break;
        }
        if (skill.name == "かわらわり")
        {
            if (atkChara.master)
            {
                if (field.enemy[2] + field.enemy[3] + field.enemy[4] > 0)
                {
                    field.enemy[2] = 0;
                    field.enemy[3] = 0;
                    field.enemy[4] = 0;
                    yield return AreaIE("かべをわった");
                }
            }
            else
            {
                if (field.player[2] + field.player[3] + field.player[4] > 0)
                {
                    field.player[2] = 0;
                    field.player[3] = 0;
                    field.player[4] = 0;
                    yield return AreaIE("かべをわった");
                }
            }
        }
        if (skill.physics != "変化")
        {
            int typePer = TypePer(skill, atkChara, defChara);
            //ダメージ
            float atk;
            float def;
            if (skill.physics == "物理")
            {
                atk = atkChara.RankStatus(1);
                def = defChara.RankStatus(2);
                if (skill.name == "イカサマ") atk = defChara.RankStatus(1);
                if (skill.name == "からげんき" && atkChara.error == 4) atk *= 2;
                if (skill.name == "せいなるつるぎ" || skill.name == "エクスカリバー" || skill.name == "ヘルエスタセイバー") def = defChara.status[2];
            }
            else
            {
                atk = atkChara.RankStatus(3);
                def = defChara.RankStatus(4);
                if (skill.name == "サイコショック") def = defChara.RankStatus(2);
            }
            float power = SkillPower(skill, atkChara, defChara,selectNum);

            Debug.Log(atk + "/" + def + "*" + power + "*" + 2f / 5f + "*" + typePer / 100 + "=" + atk / def * power * 2f / 5f * typePer / 100);
            float damageF = atk / def * power * 2f / 5f * typePer / 100;

            //クリティカル
            float damageC = Critcul(master, skill, power, atkChara, defChara, damageF, typePer);
            bool c = false;
            if (damageF < damageC)
            {
                c = true;
                damageF = damageC;
            }
            //乱数と整数化
            int damageI = Mathf.FloorToInt(damageF) + UnityEngine.Random.Range(-10, 1);
            if (damageI < 1) damageI = 1;
            if (typePer == 0 || power==0) damageI = 0;
            damageI = SkillDamage(skill, atkChara, defChara, damageI,typePer);

            if (damageI == 0)
            {
                Message(atkChara.nameC + "の " + skill.name + "!" + "\n" + "しかしうまくいかなかった");
                yield break;
            }

            //送信内容
            string text;
            text = atkChara.nameC + "の " + skill.name + "!";
            if (skill.critical != -1)
            {
                if (c) text += " クリティカルヒット！";
                if (typePer > 100) text += "\n" + "こうかはばつぐんだ!";
                else if (100 > typePer && typePer > 0) text += "\n" + "こうかはいまひとつだ";
            }
            //ダメージ処理
            yield return DamageIE(skill, atkChara, defChara, text, damageI,typePer);
            switch (atkChara.passiveName)
            {
                case "着せ替え":
                    if (defChara.error != 8)
                    {
                        defChara.type1 = skill.type;
                        defChara.type2 = "";
                        Message(PassiveText(atkChara) + defChara.nameC + "の　タイプを" + skill.type + "に変えた！");
                    }
                    break;
                case "ねっとりチューニング":
                    if (defChara.error != 8)
                    {
                        atkChara.type1 = skill.type;
                        atkChara.type2 = "";
                        Message(PassiveText(atkChara) + atkChara.nameC + "は　タイプを" + skill.type + "に変えた！");
                    }
                    break;
                case "炎上体質":
                    if (defChara.sex =="女性")
                    {
                        Message(PassiveText(atkChara));
                        yield return atkChara.ErrorCheak(4);
                    }
                    break;
            }
        }
        else
        {
            damage.skillNo = skill.no;
            damage.player = !master;
            damage.hpPer = defChara.HPPer();
            damage.hpText = defChara.HPText();
            damage.message = atkChara.nameC + "の " + skill.name + "!";
            punBattle.Send("damage", damage.ToString());
            yield return new WaitForSeconds(0.1f);
        }

    }
    float SkillPower(Skill skill, CharaStatus atkChara, CharaStatus defChara,int selectNum)
    {
        float power = skill.power;
        int i;
        int j;
        switch (skill.name)
        {
            case "ウェザーボール":
                if (field.both[0] != 0) power *= 2;
                break;
            case "からげんき":
                if (atkChara.error != 0) power *= 2;
                break;
            case "じたばた":
                power = 200 - atkChara.HPPer() * 200;
                break;
            case "にぎりつぶす":
                power = 125 * defChara.HPPer();
                break;
            case "はきだす":
                if (atkChara.buffer[6] > 5) i = 5;
                else i = atkChara.buffer[6];
                if (i > 0)
                {
                    atkChara.buffer[6] -= i;
                    Effect effect = atkChara.ToEffect();
                    effect.message = i + "つたくわえをつかう";
                    punBattle.Send("effect", effect.ToString());
                    power = 90 * i;
                }
                break;
            case "しっとのほのお":
                j = 0;
                for (i = 1; i < 8; i++)
                {
                    if (defChara.rank[i] > 0) j += defChara.rank[i];
                }
                power += j * 30;
                break;
            case "ゆきなだれ":
                if (atkChara.damageP > 0 || atkChara.damageS > 0) power *= 2;
                break;
            case "アイスボール":
                power = 35 * Mathf.Pow(2, (atkChara.successNum - 1) % 5);
                if (atkChara.buffer[2] > 0) power *= 2;
                break;
            case "きしかいせい":
                power = 200 - atkChara.HPPer() * 200;
                break;
            case "リベンジ":
                if (atkChara.damageP > 0 || atkChara.damageS > 0) power *= 2;
                break;
            case "みせなさいよ！":
                if (defChara.sex == "男性") power *= 2;
                break;
            case "じだんだ":
                if (!atkChara.successPrev && !atkChara.firstAttack) power *= 2;
                break;
            case "アクロバット":
                if (atkChara.item == 0) power *= 2;
                break;
            case "アシストパワー":
                j = 0;
                for (i = 1; i < 8; i++)
                {
                    if (atkChara.rank[i] > 0) j += atkChara.rank[i];
                }
                power += i * 20;
                break;
            case "れんぞくぎり":
                power += atkChara.buffer[13] * 30;
                break;
            case "ころがる":
                power = 35 * Mathf.Pow(2, (atkChara.successNum - 1) % 5);
                if (atkChara.buffer[2] > 0) power *= 2;
                break;
            case "たたりめ":
                if (defChara.error != 0) power *= 2;
                break;
            case "しっぺがえし":
                if (!atkChara.fastAttack) power *= 2;
                break;
            case "つけあがる":
                j = 0;
                for (i = 1; i < 8; i++)
                {
                    if (atkChara.rank[i] > 0) j += atkChara.rank[i];
                }
                power += i * 20;
                break;
            case "はたきおとす":
                if (defChara.item != 0) power *= 1.5f;
                break;
            case "じょうおうのむち":
                if (defChara.rank[4] < 0) power -= 30 * defChara.rank[4];
                break;
            case "ジャイロボール":
                power = defChara.RankStatus(5) / atkChara.RankStatus(5) * 25;
                if (power > 150) power = 150;
                break;
        }

        switch (atkChara.passiveName)
        {
            case "エルフの森":
                if (skill.type == "くさ")
                {
                    power *= 1.5f;
                    Message(PassiveText(atkChara)+"くさ技の威力が高まる");
                    
                }
                break;
            case "公式怪文書":
                if (skill.type == "フェアリー")
                {
                    power *= 1.5f;
                    Message(PassiveText(atkChara) + "フェアリー技の威力が高まる");
                }
                break;
            case "横綱「ローションキング」":
                if (skill.name == "つっぱり")
                {
                    power *= 1.5f;
                    Message(PassiveText(atkChara) + "つっぱりの威力が高まる");
                }
                break;
            case "Darkness Eater":
                if (defChara.error==7)
                {
                    power *= 1.5f;
                    Message(PassiveText(atkChara) + "ねむり状態の相手に与えるダメージが高まる");
                }
                break;
            case "金のポン":
                power *= 1.2f;
                break;
            case "耐久歌配信":
                if (atkChara.prevSkill == selectNum)
                {
                    if (atkChara.successNum < 5) power *= 1 + 0.2f * (atkChara.successNum-1);
                    else power *= 2;
                }
                break;
            case "ザンクの力一":
                if (power <= 60)
                {
                    power *= 1.5f;
                }
                break;
            case "アルキデス":
                if (defChara.type1=="むし" || defChara.type2 == "むし")
                {
                    power *= 2f;
                    Message(PassiveText(atkChara) + "むしタイプの相手に与えるダメージが高まる");
                }
                break;
        }
        switch (defChara.passiveName)
        {
            case "エルフの森":
                if (skill.type == "ほのお")
                {
                    power *= 2f;
                    Message(PassiveText(defChara) + "ほのお技で受けるダメージが高まる");
                }
                break;
        }
        switch (ItemName(atkChara.item))
        {
            case "いのちのたま":
                power *= 1.3f;
                break;
            case "こだわりハチマキ":
                if (skill.physics == "物理") power *= 1.5f;
                break;
            case "こだわりメガネ":
                if (skill.physics == "特殊") power *= 1.5f;
                break;
            case "ちからのハチマキ":
                if (skill.physics == "物理") power *= 1.1f;
                break;
            case "ものしりメガネ":
                if (skill.physics == "特殊") power *= 1.5f;
                break;
            case "メトロノーム":
                if (atkChara.prevSkill == selectNum)
                {
                    if (atkChara.successNum < 5) power *= 1 + 0.2f * (atkChara.successNum - 1);
                    else power *= 2;
                }
                break;
        }
        if (skill.type == "でんき" && atkChara.buffer[5] > 0)
        {
            power *= 2f;
            atkChara.buffer[5] = 0;
        }
        if (field.both[0] == 1)
        {
            if (skill.type == "ほのお") power *= 1.5f;
            if (skill.type == "みず") power *= 0.75f;
            if (skill.type == "こおり") power *= 0.75f;
        }
        if (field.both[0] == 2)
        {
            if (skill.type == "ほのお") power *= 0.75f;
            if (skill.type == "みず") power *= 1.5f;
        }

        if (atkChara.type1 == AtkType(skill,atkChara) || atkChara.type2 == AtkType(skill, atkChara)) power *= 1.5f;
        Debug.Log(skill.name+":"+power);
        return power;
    }
    float Critcul(bool master, Skill skill, float power, CharaStatus atkChara, CharaStatus defChara, float damageF, int typePer)
    {
        float damageC = 0;
        if (skill.critical == -1) return damageF;
        if (skill.critical == 100)
        {
            if (master)
            {
                damageF *= 1.5f;
                float atkC;
                float defC;
                if (skill.physics == "物理")
                {
                    atkC = atkChara.status[1];
                    defC = defChara.status[2];
                }
                else
                {
                    atkC = atkChara.status[3];
                    defC = defChara.status[4];
                }
                damageC = atkC / defC * power * 2f / 5f * typePer / 100;
            }
            else
            {
                damageF *= 1.5f;
                float atkC;
                float defC;
                if (skill.physics == "物理")
                {
                    atkC = atkChara.status[1];
                    defC = defChara.status[2];
                }
                else
                {
                    atkC = atkChara.status[3];
                    defC = defChara.status[4];
                }
                damageC = atkC / defC * power * 2f / 5f * typePer / 100;
            }
            if (damageF > damageC) return damageF;
            else return damageC;
        }


        if (master)
        {
            correctP[2] += 4;
            if (skill.contact > 0) correctP[2] += 2;
            correctP[2] += 15 * skill.critical;
            if (atkChara.passiveName == "激アツ" && turn % 10 == 7) correctP[2] += 15 * 7;
            if (atkChara.passiveName == "鋭いアゴ") correctP[2] += 15;
            if (correctP[2] > Random.Range(0, 100))
            {
                correctP[2] -= 100;
                damageF *= 1.5f;
                float atkC;
                float defC;
                if (skill.physics == "物理")
                {
                    atkC = atkChara.status[1];
                    defC = defChara.status[2];
                    if (skill.name == "イカサマ") atkC = defChara.status[1];
                }
                else
                {
                    atkC = atkChara.status[3];
                    defC = defChara.status[4];
                    if (skill.name == "サイコショック") defC = defChara.status[2];
                }
                damageC = atkC / defC * power * 2f / 5f * typePer / 100;
                if (skill.name == "パチンコ") damageC *= 1.5f;
                if (skill.name == "パチンコ") damageF *= 1.5f;
            }
        }
        else
        {
            correctE[2] += 4;
            if (skill.contact > 0) correctE[2] += 2;
            correctE[2] += 15 * skill.critical;
            if (correctE[2] > Random.Range(0, 101))
            {
                correctE[2] -= 100;
                damageF *= 1.5f;
                float atkC;
                float defC;
                if (skill.physics == "物理")
                {
                    atkC = atkChara.status[1];
                    defC = defChara.status[2];
                    if (skill.name == "イカサマ") atkC = defChara.status[1];
                }
                else
                {
                    atkC = atkChara.status[3];
                    defC = defChara.status[4];
                    if (skill.name == "サイコショック") defC = defChara.status[2];
                }
                damageC = atkC / defC * power * 2f / 5f * typePer / 100;
                if (skill.name == "パチンコ") damageC *= 1.5f;
                if (skill.name == "パチンコ") damageF *= 1.5f;
            }
        }
        if (damageF > damageC) return damageF;
        else return damageC;
    }
    int SkillDamage(Skill skill, CharaStatus atkChara, CharaStatus defChara,int damagei,int typePer)
    {
        int damage = damagei;
        switch (atkChara.passiveName)
        {
            case "溶かす":
                damage += 10;
                break;
        }
        switch (defChara.passiveName)
        {
            case "鉄壁の四皇":
                if (typePer > 100)
                {
                    Message(PassiveText(defChara)+"効果抜群で受けるダメージを軽減する");
                    damage = Mathf.CeilToInt(damage * 0.75f);
                }
                break;
        }
        switch (ItemName(atkChara.item))
        {
            case "たつじんのおび":
                if (typePer > 100)
                {
                    damage = Mathf.CeilToInt(damage * 1.2f);
                }
                break;
        }


        switch (skill.name)
        {
            case "いかりのやえば":
                damage = Mathf.FloorToInt(defChara.status[0]/2f);
                break;
            case "がむしゃら":
                damage =  defChara.hp - atkChara.hp;
                break;
            case "つのドリル":
                damage = defChara.hp;
                break;
            case "アゴドリル":
                damage = defChara.hp;
                break;
            case "ぜったいれいど":
                damage = defChara.hp;
                break;
            case "カウンター":
                damage = atkChara.damageP * 2;
                break;
            case "ちきゅうなげ":
                damage = 50;
                break;
            case "じわれ":
                damage = defChara.hp;
                break;
            case "ミラーコート":
                damage = atkChara.damageS * 2;
                break;
            case "ナイトヘッド":
                damage = 50;
                break;
            case "○○す":
                damage = defChara.hp;
                break;
            case "メタルバースト":
                damage = Mathf.FloorToInt( (atkChara.damageS+atkChara.damageP) * 1.5f);
                break;
        }



        return damage;
    }
    IEnumerator SkillEffect(Skill skill, CharaStatus atkChara, CharaStatus defChara)
    {
        int damage;
        int num;
        int i;
        bool b;
        Skill s;
        Chara c1;
        Chara c2;
        List<CharaStatus> member;
        if (skill.target == "相手" && defChara.error == 8) yield break;
        if (skill.target == "自分")
        {
            if (atkChara.error==8)yield break;
            if (atkChara.master && atkChara != battlerP) yield break;
            if (!atkChara.master && atkChara != battlerE) yield break;
        }
        switch (skill.name)
        {
            case "わるあがき":
                damage = Mathf.FloorToInt(defChara.status[0] / 10f);
                yield return DamageIE(atkChara, defChara, defChara.nameC + "は　ダメージを受けた", damage);
                damage = Mathf.FloorToInt(atkChara.status[0] / 4f);
                yield return DamageIE(defChara, atkChara, atkChara.nameC + "は　反動をうけた", damage);
                break;
            case "あくび":
                if (defChara.error == 0)
                {
                    if (defChara.error2[6] == 0)
                    {
                        defChara.error2[6] = 2;
                        yield return EffectIE(atkChara, defChara, defChara.nameC + "は ねむけを誘われた!");
                    }
                    else
                    {
                        Message(defChara.nameC + "は すでにねむけ状態だ");
                    }
                }
                else
                {
                    Message(defChara.nameC + "は すでに状態異常だ");
                }
                break;
            case "あさのひざし":
                if (field.both[0] == 0) damage = Mathf.FloorToInt(atkChara.status[0] / 2f);
                else if (field.both[0] == 1) damage = Mathf.FloorToInt(atkChara.status[0] * 2 / 3f);
                else damage = Mathf.FloorToInt(atkChara.status[0] * 1 / 3f);
                yield return DamageIE( defChara, atkChara, atkChara.nameC + "は　あさのひざしを受けて回復した", -damage);
                break;
            case "あばれる":
                if (atkChara.buffer[4] == 0)
                {
                    atkChara.buffer[4] = 3;
                    yield return EffectIE(defChara, atkChara, "");
                }
                break;
            case "あまいかおり":
                field.both[4] = 5;
                yield return AreaIE("あまいかおりが漂った");
                break;
            case "アンコール":
                if (defChara.prevSkill != -1)
                {
                    if (defChara.error2[9] == 0)
                    {
                        defChara.error2[9] = 3;
                        yield return EffectIE(atkChara, defChara, defChara.nameC + "は　同じ技しか出せなくなった！");
                    }
                }
                break;
            case "いたみわけ":
                int half = Mathf.FloorToInt((atkChara.hp + defChara.hp) / 2f);
                damage = half - atkChara.hp;
                yield return DamageIE( defChara, atkChara, atkChara.nameC + "は　HPを分け合った", -damage);
                damage = half - defChara.hp;
                yield return DamageIE( atkChara, defChara, defChara.nameC + "は　HPを分け合った", -damage);
                break;
            case "いばる":
                yield return defChara.StatusCheak(1, 1);
                if (defChara.error2[0] == 0)
                {
                    defChara.error2[0] = 2;
                    yield return EffectIE(atkChara, defChara, defChara.nameC + "は　混乱した！");
                } else yield return EffectIE(atkChara, defChara, defChara.nameC + "は　すでに混乱している");
                break;
            case "ねぞうといびき":
                if (PerCorrect(atkChara.master, 30))
                {
                    defChara.error2[1] = 1;
                    yield return EffectIE(atkChara, defChara, defChara.nameC + "を　怯ませた！");
                }
                break;
            case "いやなおと":
                yield return defChara.StatusCheak(2, -3);
                break;
            case "えんまく":
                yield return defChara.StatusCheak(6, -1);
                yield return defChara.StatusCheak(7, -1);
                break;
            case "おたけび":
                yield return defChara.StatusCheak(1, -1);
                yield return defChara.StatusCheak(3, -1);
                if (atkChara.master) correctP[1] += 20;
                else correctE[1] += 20;
                break;
            case "おちゃかい":
                if (atkChara.master)
                {
                    foreach (CharaStatus cs in playerMember)
                    {
                        if (cs != battlerP && cs.error != 8)
                        {
                            cs.hp += Mathf.FloorToInt(cs.status[0] / 4f);
                            if (cs.hp > cs.status[0]) cs.hp = cs.status[0];
                        }
                    }
                    damage = Mathf.FloorToInt(atkChara.status[0] / 4f);
                    yield return DamageIE( defChara, atkChara, atkChara.nameC + "と　仲間のHPを回復した", -damage);
                }
                else
                {
                    foreach (CharaStatus cs in enemyMember)
                    {
                        if (cs != battlerE && cs.error != 8)
                        {
                            cs.hp += Mathf.FloorToInt(cs.status[0] / 4f);
                            if (cs.hp > cs.status[0]) cs.hp = cs.status[0];
                        }
                    }
                    damage = Mathf.FloorToInt(atkChara.status[0] / 4f);
                    yield return DamageIE(defChara, atkChara, atkChara.nameC + "と　仲間のHPを回復した", -damage);
                }
                break;
            case "かげぶんしん":
                yield return atkChara.StatusCheak(6, 1);
                if (!atkChara.master) correctP[0] -= 20;
                else correctE[0] -= 20;
                break;
            case "かなしばり":
                if (0 <= defChara.prevSkill && defChara.prevSkill <= 3)
                {
                    if (defChara.error2[13] == 0)
                    {
                        defChara.lockSkill = defChara.prevSkill;
                        s = skillMaster.SkillList.Find(a => a.no == defChara.skill[defChara.lockSkill]);
                        defChara.error2[13] = 4;
                        yield return EffectIE(atkChara, defChara, defChara.nameC + "は　"+ s.name +"が出せなくなった");
                    }
                    else
                    {
                        yield return EffectIE(atkChara, defChara, defChara.nameC + "は　すでにかなしばり状態だ");
                    }
                }
                else
                {
                    Message("しかしうまくいかなかった");
                }
                break;
            case "からをやぶる":
                yield return atkChara.StatusCheak(1, 2);
                yield return atkChara.StatusCheak(3, 2);
                yield return atkChara.StatusCheak(5, 2);
                yield return atkChara.StatusCheak(2, -1);
                yield return atkChara.StatusCheak(4, -1);
                yield return atkChara.StatusCheak(5, -1);
                break;
            case "くすぐる":
                yield return defChara.StatusCheak(1, -1);
                yield return defChara.StatusCheak(2, -1);
                yield return defChara.StatusCheak(7, -1);
                break;
            case "くろいまなざし":
                defChara.error2[12] = 30;
                yield return EffectIE(atkChara, defChara, defChara.nameC + "は　にげられなくなった！");
                break;
            case "こうそくスピン":
                if (atkChara.master)
                {
                    field.player[10] = 0;
                    field.player[11] = 0;
                    field.player[12] = 0;
                    yield return AreaIE("");
                }
                else
                {
                    field.enemy[10] = 0;
                    field.enemy[11] = 0;
                    field.enemy[12] = 0;
                    yield return AreaIE("");
                }
                break;
            case "コートチェンジ":
                int[] change = field.player;
                field.player = field.enemy;
                field.enemy = change;
                yield return AreaIE("場の状況が入れ替わった！");
                break;
            case "こらえる":
                atkChara.buffer[7] = 0;
                yield return EffectIE(defChara, atkChara, atkChara.nameC + "は　こらえる体勢になった！");
                break;
            case "こわいかお":
                yield return defChara.StatusCheak(1, -1);
                yield return defChara.StatusCheak(3, -1);
                yield return defChara.StatusCheak(5, -1);
                break;
            case "さわぐ":
                if (atkChara.buffer[9] == 0)
                {
                    atkChara.buffer[9] = 3;
                    yield return EffectIE(defChara, atkChara, "");
                }
                break;
            case "じこあんじ":
                atkChara.rank = defChara.rank;
                yield return EffectIE(defChara, atkChara, atkChara.nameC + "は　相手の能力変化をコピーした");
                break;
            case "じこさいせい":
                damage = Mathf.FloorToInt(atkChara.status[0] / 2f);
                yield return DamageIE( defChara, atkChara, atkChara.nameC + "は　回復した", -damage);
                break;
            case "しっぽをふる":
                yield return defChara.StatusCheak(2, -1);
                yield return defChara.StatusCheak(4, -1);
                if (atkChara.master) correctP[1] += 20;
                else correctE[1] += 20;
                break;
            case "じばく":
                yield return DamageIE( defChara, atkChara, atkChara.nameC + "は　じばくの反動を受けた", 9999);
                break;
            case "しめつける":
                if (defChara.error2[2] == 0)
                {
                    defChara.error2[2] = 4;
                    yield return EffectIE(atkChara, defChara, defChara.nameC + "は　とらわれた");
                }
                break;
            case "スイープビンタ":
                num = UnityEngine.Random.Range(2, 4);
                b = false;
                for (i = 0; i < num; i++)
                {
                    if (defChara.error == 8) b = true;
                    if (atkChara.error == 8) b = true;
                    if (atkChara.master && atkChara != battlerP) b = true;
                    if (!atkChara.master && atkChara != battlerE) b = true;
                    if (atkChara.master && defChara != battlerE) b = true;
                    if (!atkChara.master && defChara != battlerP) b = true;
                    if (b)
                    {
                        break;
                    }
                    yield return SkillAttack(skill, atkChara, defChara);
                }
                Message((i + 1) + "回あたった");
                break;
            case "ずつき":
                if (PerCorrect(atkChara.master, 30))
                {
                    defChara.error2[1] = 1;
                    yield return EffectIE(atkChara, defChara, defChara.nameC + "を　怯ませた！");
                }
                break;
            case "すてみタックル":
                damage = Mathf.FloorToInt(prevSkillDamage * 0.33f);
                yield return DamageIE( defChara, atkChara, atkChara.nameC + "は　反動を受けた", damage);
                break;
            case "スピードスター":
                if (atkChara.master) correctP[0] += 20;
                else correctE[0] += 20;
                break;
            case "せいちょう":
                if (atkChara.passiveName == "大人Ver" && atkChara.nameC == "森中花咲")
                {
                    c1 = charaMaster.CharaList.Find(a => a.name == "森中花咲");
                    c2 = charaMaster.CharaList.Find(a => a.name == "森中花咲(大人)");
                    atkChara.nameC = c2.name;
                    atkChara.PassiveChange(c2.passiveName1, c2.passiveText1);
                    atkChara.status[0] += c2.h - c1.h;
                    atkChara.status[1] += c2.a - c1.a;
                    atkChara.status[2] += c2.b - c1.b;
                    atkChara.status[3] += c2.c - c1.c;
                    atkChara.status[4] += c2.d - c1.d;
                    atkChara.status[5] += c2.s - c1.s;
                    atkChara.hp += c2.h - c1.h;
                    if (atkChara.swap[0] != 0) atkChara.swap[0] += c2.h - c1.h;
                    if (atkChara.swap[1] != 0) atkChara.swap[1] += c2.a - c1.a;
                    if (atkChara.swap[2] != 0) atkChara.swap[2] += c2.b - c1.b;
                    if (atkChara.swap[3] != 0) atkChara.swap[3] += c2.c - c1.c;
                    if (atkChara.swap[4] != 0) atkChara.swap[4] += c2.d - c1.d;
                    if (atkChara.swap[5] != 0) atkChara.swap[5] += c2.s - c1.s;
                    Summon summon = atkChara.ToSummon();
                    summon.exception = "森中花咲は　大人に変化した！";
                    summon.type = "exception";
                    punBattle.Send("summon", summon.ToString());
                }
                else
                {
                    yield return atkChara.StatusCheak(1, 1);
                    yield return atkChara.StatusCheak(3, 1);
                    if (atkChara.master) correctP[2] += 20;
                    else correctE[2] += 20;
                }
                break;
            case "たくわえる":
                atkChara.buffer[6] += 3;
                if (atkChara.buffer[6] > 9) atkChara.buffer[6] = 9;
                yield return atkChara.StatusCheak(3, 1);
                break;
            case "ちいさくなる":
                if (atkChara.passiveName == "子供Ver" && atkChara.nameC == "森中花咲(大人)")
                {
                    c1 = charaMaster.CharaList.Find(a => a.name == "森中花咲");
                    c2 = charaMaster.CharaList.Find(a => a.name == "森中花咲(大人)");
                    atkChara.nameC = c1.name;
                    atkChara.PassiveChange(c1.passiveName1, c1.passiveText1);
                    atkChara.status[0] -= c2.h - c1.h;
                    atkChara.status[1] -= c2.a - c1.a;
                    atkChara.status[2] -= c2.b - c1.b;
                    atkChara.status[3] -= c2.c - c1.c;
                    atkChara.status[4] -= c2.d - c1.d;
                    atkChara.status[5] -= c2.s - c1.s;
                    if (atkChara.hp > atkChara.status[0]) atkChara.hp = atkChara.status[0];
                    if (atkChara.swap[0] != 0) atkChara.swap[0] -= c2.h - c1.h;
                    if (atkChara.swap[1] != 0) atkChara.swap[1] -= c2.a - c1.a;
                    if (atkChara.swap[2] != 0) atkChara.swap[2] -= c2.b - c1.b;
                    if (atkChara.swap[3] != 0) atkChara.swap[3] -= c2.c - c1.c;
                    if (atkChara.swap[4] != 0) atkChara.swap[4] -= c2.d - c1.d;
                    if (atkChara.swap[5] != 0) atkChara.swap[5] -= c2.s - c1.s;
                    Summon summon = atkChara.ToSummon();
                    summon.exception = "森中花咲は　子供に変化した！";
                    summon.type = "exception";
                    punBattle.Send("summon", summon.ToString());
                    yield return atkChara.StatusCheak(7, 4);
                }
                else
                {
                    damage = Mathf.FloorToInt(atkChara.hp / 10f);
                    if (atkChara.hp > damage)
                    {
                        yield return atkChara.StatusCheak(7, 2);
                        yield return DamageIE(defChara, atkChara, atkChara.nameC + "は　ちいさくなってHPが減った", damage);
                    }
                    else Message("しかしうまくいかなかった");
                }
                break;
            case "つるぎのまい":
                yield return atkChara.StatusCheak(1, 2);
                break;
            case "にじさんじアタック":
                if (defChara.error==0)
                if (PerCorrect(atkChara.master, 20))
                {
                    i = UnityEngine.Random.Range(0, 3);
                    if (i == 0)yield return defChara.ErrorCheak(3);
                    if (i == 1)yield return defChara.ErrorCheak(4);
                    if (i == 2)yield return defChara.ErrorCheak(5);
                }
                break;
            case "なかまづくり":
                defChara.PassiveChange(atkChara.passiveName, atkChara.passiveText);
                Message(defChara.nameC + "の　とくせいを" + defChara.passiveName + "に変えた");
                break;
            case "なまける":
                damage = Mathf.FloorToInt(atkChara.status[0] / 2f);
                yield return DamageIE( defChara, atkChara, atkChara.nameC + "は　回復した", -damage);
                break;
            case "にらみつける":
                yield return defChara.StatusCheak(2, -1);
                yield return defChara.StatusCheak(5, -1);
                if (PerCorrect(atkChara.master, 20))
                {
                    defChara.error2[1] = 1;
                    yield return EffectIE(atkChara, defChara, defChara.nameC + "を　怯ませた！");
                }
                break;
            case "ねがいごと":
                if (atkChara.master)
                {
                    if (field.player[1] == 0) yield return AreaIE(atkChara.nameC + "は　未来に願いを託した！");
                    else Message("しかしうまくいかなかった");
                }
                else
                {
                    if (field.enemy[1] == 0) yield return AreaIE(atkChara.nameC + "は　未来に願いを託した！");
                    else Message("しかしうまくいかなかった");
                }
                break;
            case "あいさつバトル":
                defChara.error2[1] = 1;
                yield return EffectIE(atkChara, defChara, defChara.nameC + "を　怯ませた！");
                break;
            case "のしかかり":
                if (defChara.error == 0)
                if (PerCorrect(atkChara.master, 30))
                {
                        yield return defChara.ErrorCheak(3);
                }
                break;
            case "のみこむ":
                damage = Mathf.FloorToInt(atkChara.status[0] / 4f);
                if (atkChara.buffer[6] >= 2)
                {
                    damage = Mathf.FloorToInt(atkChara.status[0] / 2f);
                    atkChara.buffer[6] -= 2;
                    yield return EffectIE(defChara, atkChara, atkChara.nameC + "は　2つたくわえを使用した");
                }
                if (atkChara.buffer[6] == 1)
                {
                    damage = Mathf.FloorToInt(atkChara.status[0] * 3 / 4f);
                    atkChara.buffer[6] -= 1;
                    yield return EffectIE(defChara, atkChara, atkChara.nameC + "は　1つたくわえを使用した");
                }
                yield return DamageIE( defChara, atkChara, atkChara.nameC + "は　回復した", -damage);
                break;
            case "はかいこうせん":
                atkChara.buffer[10] = 1;
                break;
            case "ばくおんぱ":
                yield return atkChara.StatusCheak(3, -1);
                break;
            case "バトンタッチ":
                if (atkChara.master) member = playerMember;
                else member = enemyMember;
                int j = 0;
                for (i = 0; i < 3; i++)
                {
                    if (member[i].error != 8) j++;
                }
                if (j < 2)
                {
                    Message("しかしうまくいかなかった");
                    break;
                }
                punBattle.Send("change", "", atkChara.master);
                while (playerChange == null && enemyChange == null) {
                    yield return new WaitForSecondsRealtime(0.1f);
                }
                if (atkChara.master)
                {
                    playerMember[playerChange.num].rank = atkChara.rank;
                    playerMember[playerChange.num].error = atkChara.error;
                    playerMember[playerChange.num].buffer = atkChara.buffer;
                    yield return Summon(true, playerChange.num,"");
                }
                else
                {
                    enemyMember[enemyChange.num].rank = atkChara.rank;
                    enemyMember[enemyChange.num].error = atkChara.error;
                    enemyMember[enemyChange.num].buffer = atkChara.buffer;
                    yield return Summon(false, enemyChange.num,"");
                }
                break;
            case "ふみつけ":
                if (PerCorrect(atkChara.master, 30))
                {
                    defChara.error2[1] = 1;
                    yield return EffectIE(atkChara, defChara, defChara.nameC + "を　怯ませた！");
                }
                break;
            case "ほえる":
                playerChange = null;
                enemyChange = null;
                num = UnityEngine.Random.Range(0, 2);
                if (atkChara.master) member = enemyMember;
                else member = playerMember;
                b = false;
                if (num == 0)
                {
                    for (i = 0; i < 3; i++)
                    {
                        if (defChara != member[i])
                        {
                            b = true;
                            yield return Summon(defChara.master, i,"skill");
                            break;
                        }
                    }
                }
                else
                {
                    for (i = 2; i >= 0; i--)
                    {
                        if (defChara != member[i])
                        {
                            b = true;
                            yield return Summon(defChara.master, i, "skill");
                            break;
                        }
                    }
                }
                if (!b) Message("しかしうまくいかなかった");
                break;
            case "ほろびのうた":
                atkChara.error2[7] = 3;
                defChara.error2[7] = 3;
                yield return EffectIE(atkChara, defChara, "");
                yield return EffectIE(defChara, atkChara, "ほろびのカウントダウンが始まった");
                break;
            case "まもる":
                atkChara.buffer[1] = 1;
                yield return EffectIE(defChara, atkChara, "");
                break;
            case "まるくなる":
                atkChara.buffer[2] = 1;
                yield return EffectIE(defChara, atkChara, "");
                yield return atkChara.StatusCheak(2, 1);
                break;
            case "ゆがみん":
                damage = Mathf.FloorToInt(atkChara.status[0] / 4f);
                if (atkChara.hp > damage && atkChara.buffer[0] == 0)
                {
                    yield return DamageIE( defChara, atkChara, atkChara.nameC + "は　HPを削ってゆがみんを作った", damage);
                    atkChara.buffer[0] = damage * 2;
                    yield return EffectIE(defChara, atkChara, atkChara.nameC + "は　HPを削ってゆがみんを作った");
                }
                else Message("しかしうまくいかなかった");
                break;
            case "みだれひっかき":
                num = UnityEngine.Random.Range(3, 5);
                b = false;
                for (i = 0; i < num; i++)
                {
                    if (defChara.error == 8) b = true;
                    if (atkChara.error == 8) b = true;
                    if (atkChara.master && atkChara != battlerP) b = true;
                    if (!atkChara.master && atkChara != battlerE) b = true;
                    if (atkChara.master && defChara != battlerE) b = true;
                    if (!atkChara.master && defChara != battlerP) b = true;
                    if (b)
                    {
                        break;
                    }
                    yield return SkillAttack(skill, atkChara, defChara);
                }
                Message((i + 1) + "回あたった");
                break;
            case "ミルクのみ":
                damage = Mathf.FloorToInt(atkChara.status[0] / 2f);
                yield return DamageIE( defChara, atkChara, atkChara.nameC + "は　回復した", -damage);
                break;
            case "ロケットずつき":
                if (atkChara.buffer[8] == 0)
                {
                    atkChara.buffer[8] = 1;
                    yield return EffectIE(defChara, atkChara, "攻撃の体勢になった！");
                    yield return atkChara.StatusCheak(1, 1);
                }
                else
                {
                    atkChara.buffer[8] = 0;
                    yield return EffectIE(defChara, atkChara, "");
                }
                break;
                
            case "ものまね":
                if (prevSkillNo != "")
                {
                    s = skillMaster.SkillList.Find(a => a.no == prevSkillNo);
                    for (i = 0; i < 4; i++)
                    {
                        if (atkChara.skill[i] == "no73")
                        {
                            atkChara.skill[i] = s.no;
                            atkChara.sp[i] = s.point;
                            yield return SkillIE(atkChara.master, s,i);
                        }
                    }
                }
                else
                {
                    Message("しかしうまくいかなかった");
                }
                break;
            case "しかいぎょう":
                yield return defChara.ErrorCheak(6);
                break;
            case "ねこだまし":
                defChara.error2[1] = 1;
                yield return EffectIE(atkChara, defChara, defChara.nameC + "を　怯ませた！");
                break;
            case "あおいほのお":
                if (defChara.error == 0)
                if (PerCorrect(atkChara.master, 20))
                {
                        yield return defChara.ErrorCheak(4);
                }
                break;
            case "オーバーヒート":
                yield return atkChara.StatusCheak(3, -1);
                break;
            case "おにび":
                yield return defChara.ErrorCheak(4);
                break;
            case "かえんほうしゃ":
                if (defChara.error == 0)
                    if (PerCorrect(atkChara.master, 10))
                {
                    yield return defChara.ErrorCheak(4);
                }
                break;
            case "せいなるほのお":
                if (defChara.error == 0)
                    if (PerCorrect(atkChara.master, 50))
                {
                    yield return defChara.ErrorCheak(4);
                }
                break;
            case "だいもんじ":
                if (defChara.error == 0)
                    if (PerCorrect(atkChara.master, 10))
                {
                    yield return defChara.ErrorCheak(4);
                }
                break;
            case "ニトロチャージ":
                yield return atkChara.StatusCheak(5, 1);
                break;
            case "にほんばれ":
                if (field.both[0] != 1)
                {
                    field.both[0] = 1;
                    field.both[1] = 5;
                    if(ItemName(atkChara.item)== "てんきデッキ") field.both[1] += 3;
                    yield return AreaIE("ひでりが強くなった！");
                }
                else
                {
                    Message("すでにひでりは強い");
                }
                break;
            case "ビックリヘッド":
                damage = Mathf.FloorToInt(atkChara.status[0] / 2f);
                yield return DamageIE( defChara, atkChara, atkChara.nameC + "は　反動を受けた", damage);
                break;
            case "フレアドライブ":
                damage = Mathf.FloorToInt(prevSkillDamage * 0.33f);
                yield return DamageIE( defChara, atkChara, atkChara.nameC + "は　反動を受けた", damage);
                break;
            case "ほのおのうず":
                if (defChara.error2[2] == 0)
                {
                    defChara.error2[2] = 4;
                    yield return EffectIE(atkChara, defChara, defChara.nameC + "は　とらわれた");
                }
                break;
            case "ほのおのキバ":
                if (PerCorrect(atkChara.master, 20))
                {
                    if (defChara.error == 0) yield return defChara.ErrorCheak(4);
                    else
                    {
                        defChara.error2[1] = 1;
                        yield return EffectIE(atkChara, defChara, defChara.nameC + "を　怯ませた！");
                    }
                }
                break;
            case "ほのおのまい":
                if (PerCorrect(atkChara.master, 50))
                {
                    yield return atkChara.StatusCheak(3, 1);
                }
                break;
            case "あついムチ":
                yield return defChara.StatusCheak(4, -1);
                break;
            case "マジカルフレイム":
                yield return defChara.StatusCheak(3, -1);
                break;
            case "やきつくす":
                if (defChara.item != 0)
                {
                    defChara.item = 0;
                    yield return EffectIE(atkChara, defChara, defChara.nameC + "の　もちものを焼き尽くした！");
                }
                break;
            case "れんごく":
                yield return defChara.ErrorCheak(4);
                break;
            case "アクアリング":
                if (atkChara.master) field.player[5] = 10;
                else field.enemy[5] = 10;
                yield return AreaIE("アクアリングで場が潤った");
                break;
            case "あまごい":
                if (field.both[0] != 2)
                {
                    field.both[0] = 2;
                    field.both[1] = 5;
                    if (ItemName(atkChara.item) == "てんきデッキ") field.both[1] += 3;
                    yield return AreaIE("雨が降り始めた！");
                }
                else
                {
                    Message("すでに雨は降っている");
                }
                break;
            case "うずしお":
                if (defChara.error2[2] == 0)
                {
                    defChara.error2[2] = 4;
                    yield return EffectIE(atkChara, defChara, defChara.nameC + "は　とらわれた");
                }
                break;
            case "からにこもる":
                yield return atkChara.StatusCheak(3, 1);
                yield return atkChara.StatusCheak(3, 1);
                break;
            case "クイックターン":
                yield return EffectChange(atkChara.master, r => b = r);
                break;
            case "たきのぼり":
                if (PerCorrect(atkChara.master, 20))
                {
                    defChara.error2[1] = 1;
                    yield return EffectIE(atkChara, defChara, defChara.nameC + "を　怯ませた！");
                }
                break;
            case "ねっとう":
                if (defChara.error == 0)
                    if (PerCorrect(atkChara.master, 30))
                {
                    yield return defChara.ErrorCheak(4);
                }
                break;
            case "10まんボルト":
                if (defChara.error == 0)
                    if (PerCorrect(atkChara.master, 10))
                {
                    yield return defChara.ErrorCheak(3);
                }
                break;
            case "エレキネット":
                yield return defChara.StatusCheak(5, -1);
                break;
            case "かいでんぱ":
                if (atkChara.master)
                {
                    for (i = 0; i < 3; i++)
                    {
                        correctE[i] -= 30;
                    }
                }
                else
                {
                    for (i = 0; i < 3; i++)
                    {
                        correctP[i] -= 30;
                    }
                }
                break;
            case "かみなり":
                if (defChara.error == 0)
                    if (PerCorrect(atkChara.master, 30))
                {
                    yield return defChara.ErrorCheak(3);
                }
                break;
            case "かみなりのキバ":
                if (PerCorrect(atkChara.master, 20))
                {
                    if (defChara.error == 0) yield return defChara.ErrorCheak(3);
                    else
                    {
                        defChara.error2[1] = 1;
                        yield return EffectIE(atkChara, defChara, defChara.nameC + "を　怯ませた！");
                    }
                }
                break;
            case "じゅうでん":
                atkChara.buffer[5] = 1;
                yield return EffectIE(defChara, atkChara, "");
                yield return atkChara.StatusCheak(4, 1);
                break;
            case "チャージビーム":
                if (PerCorrect(atkChara.master, 70))
                {
                    yield return atkChara.StatusCheak(3, 1);
                }
                break;
            case "でんげきは":
                if (atkChara.master) correctP[0] += 20;
                else correctE[0] += 20;
                break;
            case "でんじは":
                yield return defChara.ErrorCheak(3);
                break;
            case "でんじほう":
                yield return defChara.ErrorCheak(3);
                break;
            case "ほうでん":
                if (defChara.error == 0)
                    if (PerCorrect(atkChara.master, 30))
                {
                    yield return defChara.ErrorCheak(3);
                }
                break;
            case "ほっぺすりすり":
                yield return defChara.ErrorCheak(3);
                break;
            case "ボルテッカー":
                damage = Mathf.FloorToInt(prevSkillDamage * 0.33f);
                yield return DamageIE( defChara, atkChara, atkChara.nameC + "は　反動を受けた", damage);
                break;
            case "ボルトチェンジ":
                yield return EffectChange(atkChara.master, r => b = r);
                break;
            case "アロマセラピー":
                if (atkChara.master)
                {
                    for (i = 0; i < 3; i++) {
                        playerMember[i].error = 0;
                        playerMember[i].errorTurn = 0;
                    }
                    yield return EffectIE(defChara, atkChara, "味方の状態異常を取り除いた");
                }
                break;
            case "ウッドハンマー":
                damage = Mathf.FloorToInt(prevSkillDamage * 0.33f);
                yield return DamageIE( defChara, atkChara, atkChara.nameC + "は　反動を受けた", damage);
                break;
            case "エナジーボール":
                if (PerCorrect(atkChara.master, 10))
                {
                    yield return defChara.StatusCheak(4, -1);
                }
                break;
            case "ギガドレイン":
                damage = Mathf.FloorToInt(prevSkillDamage * 0.5f);
                yield return DamageIE( defChara, atkChara, atkChara.nameC + "は　相手からHPを吸い取った", -damage);
                break;
            case "キノコのほうし":
                yield return defChara.ErrorCheak(6);
                break;
            case "しびれごな":
                yield return defChara.ErrorCheak(3);
                break;
            case "ソーラービーム":
                if (atkChara.buffer[8] == 0)
                {
                    atkChara.buffer[8] = 1;
                    yield return EffectIE(defChara, atkChara, "光を吸収している…");
                }
                else
                {
                    atkChara.buffer[8] = 0;
                    yield return EffectIE(defChara, atkChara, "");
                }
                break;
            case "ソーラーブレード":
                if (atkChara.buffer[8] == 0)
                {
                    atkChara.buffer[8] = 1;
                    yield return EffectIE(defChara, atkChara, "光を吸収している…");
                }
                else
                {
                    atkChara.buffer[8] = 0;
                    yield return EffectIE(defChara, atkChara, "");
                }
                break;
            case "タネマシンガン":
                num = UnityEngine.Random.Range(2, 5);
                b = false;
                for (i = 0; i < num; i++)
                {
                    if (defChara.error == 8) b = true;
                    if (atkChara.error == 8) b = true;
                    if (atkChara.master && atkChara != battlerP) b = true;
                    if (!atkChara.master && atkChara != battlerE) b = true;
                    if (atkChara.master && defChara != battlerE) b = true;
                    if (!atkChara.master && defChara != battlerP) b = true;
                    if (b)
                    {
                        break;
                    }
                    yield return SkillAttack(skill, atkChara, defChara);
                }
                Message((i + 1) + "回あたった");
                break;
            case "ちからをすいとる":
                damage = Mathf.FloorToInt(defChara.RankStatus(1));
                yield return defChara.StatusCheak(1, -1);
                yield return DamageIE( defChara, atkChara, atkChara.nameC + "は　相手からちからを吸い取った", -damage);
                break;
            case "ドラムアタック":
                yield return defChara.StatusCheak(5, -1);
                break;
            case "トロピカルキック":
                yield return defChara.StatusCheak(1, -1);
                break;
            case "なやみのタネ":
                if (defChara.error2[8] == 0)
                {
                    defChara.error2[8] = 1;
                    defChara.passiveNameDelete = defChara.passiveName;
                    defChara.passiveTextDelete = defChara.passiveText;
                    defChara.passiveName = "";
                    defChara.passiveText = "";
                    yield return EffectIE(atkChara, defChara, defChara.nameC + "の　とくせいを無効化した");
                }
                else
                {
                    Message("特性はすでに無効化されている");
                }
                yield return defChara.ErrorCheak(6);
                break;
            case "ニードルガード":
                atkChara.buffer[11] = 1;
                yield return EffectIE(defChara, atkChara, "");
                break;
            case "はなびらのまい":
                if (atkChara.buffer[4] == 0)
                {
                    atkChara.buffer[4] = 3;
                    yield return EffectIE(defChara, atkChara, "");
                }
                break;
            case "マジカルリーフ":
                if (atkChara.master) correctP[0] += 20;
                else correctE[0] += 20;
                break;
            case "やどりぎのタネ":
                defChara.error2[5] = 1;
                yield return EffectIE(atkChara, defChara, defChara.nameC + "に　やどりぎのたねを植えた");
                break;
            case "リーフストーム":
                yield return atkChara.StatusCheak(3, -1);
                break;
            case "あられ":
                if (field.both[0] != 3)
                {
                    field.both[0] = 3;
                    field.both[1] = 5;
                    if (ItemName(atkChara.item) == "てんきデッキ") field.both[1] += 3;
                    yield return AreaIE("あられが降り始めた！");
                }
                else
                {
                    Message("すでにあられは降っている");
                }
                break;
            case "オーロラベール":
                if (atkChara.master) {
                    if (field.player[2] == 0)
                    {
                        if (field.both[0] == 3) field.player[2] = 5;
                        else field.player[2] = 3;
                        if (ItemName(atkChara.item) == "ひかりのねんど") field.player[2] += 3;
                        yield return AreaIE("オーロラベールを張った");
                    }
                    else
                    {
                        Message("オーロラベールは既に張られている");
                    }
                }
                else
                {
                    if (field.enemy[2] == 0)
                    {
                        if (field.both[0] == 3) field.enemy[2] = 5;
                        else field.enemy[2] = 3;
                        if (ItemName(atkChara.item) == "ひかりのねんど") field.enemy[2] += 3;
                        yield return AreaIE("オーロラベールを張った");
                    }
                    else
                    {
                        Message("オーロラベールは既に張られている");
                    }
                }
                break;
            case "くろいきり":
                atkChara.rank = new int[8];
                defChara.rank = new int[8];
                yield return EffectIE(atkChara, defChara, "");
                yield return EffectIE(defChara, atkChara, "お互いの能力変化が元に戻った");
                break;
            case "こおりのキバ":
                if (PerCorrect(atkChara.master, 20))
                {
                    if (defChara.error == 0) yield return defChara.ErrorCheak(5);
                    else
                    {
                        defChara.error2[1] = 1;
                        yield return EffectIE(atkChara, defChara, defChara.nameC + "を　怯ませた！");
                    }
                }
                break;
            case "こごえるかぜ":
                yield return defChara.StatusCheak(5, -1);
                break;
            case "しろいきり":
                if (atkChara.master) field.player[6] = 4;
                else field.enemy[6] = 4;
                yield return AreaIE("場にしろいきりが漂う");
                break;
            case "つららおとし":
                if (PerCorrect(atkChara.master, 30))
                {
                    defChara.error2[1] = 1;
                    yield return EffectIE(atkChara, defChara, defChara.nameC + "を　怯ませた！");
                }
                break;
            case "つららばり":
                num = UnityEngine.Random.Range(1, 4);
                b = false;
                for (i = 0; i < num; i++)
                {
                    if (defChara.error == 8) b = true;
                    if (atkChara.error == 8) b = true;
                    if (atkChara.master && atkChara != battlerP) b = true;
                    if (!atkChara.master && atkChara != battlerE) b = true;
                    if (atkChara.master && defChara != battlerE) b = true;
                    if (!atkChara.master && defChara != battlerP) b = true;
                    if (b)
                    {
                        break;
                    }
                    yield return SkillAttack(skill, atkChara, defChara);
                }
                Message((i + 1) + "回あたった");
                break;
            case "ふぶき":
                if (defChara.error == 0)
                    if (PerCorrect(atkChara.master, 10))
                {
                    yield return defChara.ErrorCheak(5);
                }
                break;
            case "れいとうビーム":
                if (defChara.error == 0)
                    if (PerCorrect(atkChara.master, 10))
                {
                    yield return defChara.ErrorCheak(5);
                }
                break;
            case "アイスボール":
                if (atkChara.buffer[4] == 0) atkChara.buffer[4] = 5;
                yield return EffectIE(defChara, atkChara, "");
                break;
            case "つめたいムチ":
                yield return defChara.StatusCheak(4, -1);
                break;
            case "アームハンマー":
                yield return atkChara.StatusCheak(5, -1);
                break;
            case "けんかごし":
                yield return atkChara.StatusCheak(1, 1);
                yield return atkChara.StatusCheak(4, 1);
                break;
            case "インファイト":
                yield return atkChara.StatusCheak(2, -1);
                yield return atkChara.StatusCheak(4, -1);
                break;
            case "きあいだま":
                if (PerCorrect(atkChara.master, 10))
                {
                    yield return defChara.StatusCheak(4, -1);
                }
                break;
            case "グロウパンチ":
                yield return atkChara.StatusCheak(1, 1);
                break;
            case "つっぱり":
                num = UnityEngine.Random.Range(3, 5);
                b = false;
                for (i = 0; i < num; i++)
                {
                    if (defChara.error == 8) b = true;
                    if (atkChara.error == 8) b = true;
                    if (atkChara.master && atkChara != battlerP) b = true;
                    if (!atkChara.master && atkChara != battlerE) b = true;
                    if (atkChara.master && defChara != battlerE) b = true;
                    if (!atkChara.master && defChara != battlerP) b = true;
                    if (b)
                    {
                        break;
                    }
                    yield return SkillAttack(skill, atkChara, defChara);
                }
                Message((i + 1) + "回あたった");
                break;
            case "ともえなげ":
                playerChange = null;
                enemyChange = null;
                num = UnityEngine.Random.Range(0, 2);
                if (atkChara.master) member = enemyMember;
                else member = playerMember;
                b = false;
                if (num == 0)
                {
                    for (i = 0; i < 3; i++)
                    {
                        if (defChara != member[i])
                        {
                            b = true;
                            yield return Summon(defChara.master, i, "skill");
                            break;
                        }
                    }
                }
                else
                {
                    for (i = 2; i >= 0; i--)
                    {
                        if (defChara != member[i])
                        {
                            b = true;
                            yield return Summon(defChara.master, i, "skill");
                            break;
                        }
                    }
                }
                if (!b) Message("しかしうまくいかなかった");
                break;
            case "ばかぢから":
                yield return atkChara.StatusCheak(1, -1);
                yield return atkChara.StatusCheak(2, -1);
                break;
            case "ばくれつパンチ":
                if (defChara.error2[0] == 0)
                {
                    defChara.error2[0] = 2;
                    yield return EffectIE(atkChara, defChara, defChara.nameC + "は　混乱した！");
                }
                else yield return EffectIE(atkChara, defChara, defChara.nameC + "は　すでに混乱している");
                break;
            case "ビルドアップ":
                yield return atkChara.StatusCheak(1, 1);
                yield return atkChara.StatusCheak(2, 1);
                break;
            case "ローキック":
                yield return defChara.StatusCheak(5, -1);
                break;
            case "アシッドボム":
                yield return defChara.StatusCheak(4, -2);
                break;
            case "いえき":
                yield return defChara.StatusCheak(2, -1);
                yield return defChara.StatusCheak(4, -1);
                if (defChara.error2[8] == 0)
                {
                    defChara.error2[8] = 1;
                    defChara.passiveNameDelete = defChara.passiveName;
                    defChara.passiveTextDelete = defChara.passiveText;
                    defChara.passiveName = "";
                    defChara.passiveText = "";
                    yield return EffectIE(atkChara, defChara, defChara.nameC + "の　とくせいを無効化した");
                }
                else
                {
                    Message("特性はすでに無効化されている");
                }
                break;
            case "クリアスモッグ":
                defChara.rank = new int[8];
                yield return EffectIE(atkChara, defChara, defChara.nameC + "の　能力変化を元に戻した");
                break;
            case "じょうか":
                if (atkChara.error != 0)
                {
                    atkChara.error = 0;
                    atkChara.errorTurn = 0;
                    yield return EffectIE(defChara, atkChara, "");
                    damage = Mathf.FloorToInt(atkChara.status[0] / 4f);
                    yield return DamageIE( defChara, atkChara, atkChara.nameC + "は　回復した", -damage);
                }
                else
                {
                    Message("しかしうまくいかなかった");
                }
                break;
            case "ダストシュート":
                if (defChara.error == 0)
                    if (PerCorrect(atkChara.master, 30))
                {
                    yield return defChara.ErrorCheak(1);
                }
                break;
            case "どくどく":
                yield return defChara.ErrorCheak(2);
                break;
            case "どくどくのキバ":
                if (defChara.error == 0)
                    if (PerCorrect(atkChara.master, 50))
                {
                    yield return defChara.ErrorCheak(2);
                }
                break;
            case "どくびし":
                if (atkChara.master)
                {
                    if (field.enemy[11] < 2)
                    {
                        field.enemy[11]++;
                        if (field.enemy[11] == 1) yield return AreaIE("どくびしを撒いた");
                        if (field.enemy[11] == 2) yield return AreaIE("もうどくびしを撒いた");
                    }
                    else Message("これ以上はどくびしは撒けない");
                }
                else
                {
                    if (field.player[11] < 2)
                    {
                        field.player[11]++;
                        if (field.player[11] == 1) yield return AreaIE("どくびしを撒いた");
                        if (field.player[11] == 2) yield return AreaIE("もうどくびしを撒いた");
                    }
                    else Message("これ以上はどくびしは撒けない");
                }
                break;
            case "ふにゃる":
                yield return defChara.StatusCheak(2, 2);
                break;
            case "ヘドロウェーブ":
                if (defChara.error == 0)
                    if (PerCorrect(atkChara.master, 10))
                {
                    yield return defChara.ErrorCheak(1);
                }
                break;
            case "クソマロばくだん":
                if (defChara.error == 0)
                    if (PerCorrect(atkChara.master, 30))
                {
                    yield return defChara.ErrorCheak(1);
                }
                break;
            case "バラのムチ":
                yield return defChara.StatusCheak(4, -1);
                break;
            case "じならし":
                yield return defChara.StatusCheak(5, -1);
                break;
            case "すなじごく":
                if (defChara.error2[2] == 0)
                {
                    defChara.error2[2] = 4;
                    yield return EffectIE(atkChara, defChara, defChara.nameC + "は　とらわれた");
                }
                break;
            case "だいちのちから":
                if (PerCorrect(atkChara.master, 10))
                {
                    yield return defChara.StatusCheak(4, -1);
                }
                break;
            case "どろかけ":
                yield return defChara.StatusCheak(6, -1);
                break;
            case "マッドショット":
                yield return defChara.StatusCheak(5, -1);
                break;
            case "エアスラッシュ":
                if (PerCorrect(atkChara.master, 30))
                {
                    defChara.error2[1] = 1;
                    yield return EffectIE(atkChara, defChara, defChara.nameC + "を　怯ませた！");
                }
                break;
            case "おいかぜ":
                if (atkChara.master)
                {
                    if (field.player[0] == 0)
                    {
                        field.player[0] = 4;
                        yield return AreaIE("おいかぜがふき始めた！");
                    }
                    else
                    {
                        Message("おいかぜはもうふいている");
                    }
                }
                else
                {
                    if (field.enemy[0] == 0)
                    {
                        field.enemy[0] = 4;
                        yield return AreaIE("おいかぜがふき始めた！");
                    }
                    else
                    {
                        Message("おいかぜはもうふいている");
                    }
                }
                break;
            case "つばめがえし":
                if (atkChara.master) correctP[0] += 20;
                else correctE[0] += 20;
                break;
            case "はねやすめ":
                atkChara.buffer[12] = 1;
                yield return EffectIE(defChara, atkChara, "");
                damage = Mathf.FloorToInt(atkChara.status[0] / 2f);
                yield return DamageIE( defChara, atkChara, atkChara.nameC + "は　回復した", -damage);
                break;
            case "ブレイブバード":
                damage = Mathf.FloorToInt(prevSkillDamage * 0.33f);
                yield return DamageIE( defChara, atkChara, atkChara.nameC + "は　反動を受けた", damage);
                break;
            case "ぼうふう":
                if (PerCorrect(atkChara.master, 30))
                {
                    if (defChara.error2[0] == 0)
                    {
                        defChara.error2[0] = 2;
                        yield return EffectIE(atkChara, defChara, defChara.nameC + "は　混乱した！");
                    }
                }
                break;
            case "おしゃべり":
                if (defChara.error2[0] == 0)
                {
                    defChara.error2[0] = 2;
                    yield return EffectIE(atkChara, defChara, defChara.nameC + "は　混乱した！");
                }
                break;
            case "エアルブラスト":
                if (atkChara.sex != defChara.sex)
                {
                    if (PerCorrect(atkChara.master, 10))
                    {
                        if (defChara.error2[4] == 0)
                        {
                            defChara.error2[4] = 1;
                            yield return EffectIE(atkChara, defChara, defChara.nameC + "は　メロメロになった！");
                        }
                        else yield return EffectIE(atkChara, defChara, defChara.nameC + "は　すでにメロメロだ！");
                    }
                }
                break;
            case "いてつくしせん":
                if (defChara.error == 0)
                    if (PerCorrect(atkChara.master, 10))
                {
                    yield return defChara.ErrorCheak(5);
                }
                break;
            case "いやしのねがい":
                if (atkChara.master) field.player[7] = 1;
                else field.enemy[7] = 1;
                yield return AreaIE("");
                yield return DamageIE( defChara, atkChara, atkChara.nameC + "は　身を削って願いを託した", 9999);
                break;
            case "ガードシェア":
                if(atkChara.swap[2]==0 && atkChara.swap[4] == 0 && defChara.swap[2] == 0 && defChara.swap[4] == 0)
                {
                    atkChara.swap[2] = atkChara.status[2];
                    atkChara.swap[4] = atkChara.status[4];
                    defChara.swap[2] = defChara.status[2];
                    defChara.swap[4] = defChara.status[4];
                    i = Mathf.FloorToInt((atkChara.status[2] + defChara.status[2]) / 2f);
                    atkChara.status[2] = i;
                    defChara.status[2] = i;
                    i = Mathf.FloorToInt((atkChara.status[4] + defChara.status[4]) / 2f);
                    atkChara.status[4] = i;
                    defChara.status[4] = i;
                    Message("お互いのぼうぎょととくぼうを分かち合った！");
                }
                else
                {
                    Message("しかしうまくいかなかった");
                }
                break;
            case "こうそくいどう":
                yield return atkChara.StatusCheak(5, 2);
                break;
            case "サイコキネシス":
                yield return defChara.StatusCheak(3, -1);
                break;
            case "さいみんじゅつ":
                yield return defChara.ErrorCheak(7);
                break;
            case "しねんのずつき":
                if (PerCorrect(atkChara.master, 20))
                {
                    defChara.error2[1] = 1;
                    yield return EffectIE(atkChara, defChara, defChara.nameC + "を　怯ませた！");
                }
                break;
            case "じんつうりき":
                if (PerCorrect(atkChara.master, 10))
                {
                    defChara.error2[1] = 1;
                    yield return EffectIE(atkChara, defChara, defChara.nameC + "を　怯ませた！");
                }
                break;
            case "トリック":
                i = atkChara.item;
                atkChara.item = defChara.item;
                defChara.item = i;
                yield return EffectIE(atkChara, defChara, "");
                yield return EffectIE(defChara, atkChara, "");
                Message("お互いのアイテムを交換した！");
                break;
            case "トリックルーム":
                if (field.both[2] == 0)
                {
                    field.both[2] = 5;
                    yield return AreaIE("時空がゆがみ始めた！");
                }
                else
                {
                    field.both[2] = 0;
                    yield return AreaIE("ゆがんだ時空が元に戻った！");
                }
                break;
            case "ドわすれ":
                yield return atkChara.StatusCheak(4, 2);
                break;
            case "ねむる":
                if (atkChara.error != 7)
                {
                    i = atkChara.error;
                    num = atkChara.errorTurn;

                    atkChara.error = 0;
                    atkChara.errorTurn = 0;
                    yield return atkChara.ErrorCheak(7);
                    if(atkChara.error==7)
                    {
                        damage = atkChara.status[0] - atkChara.hp;
                        yield return DamageIE( defChara, atkChara, atkChara.nameC + "は　眠って元気になった", -damage);
                    }
                    else
                    {
                        atkChara.error = i;
                        atkChara.errorTurn = num;
                        //Message("眠ることができない！");
                    }
                }
                else
                {
                    Message("すでに眠っている…");
                }
                break;
            case "パワーシェア":
                if (atkChara.swap[1] == 0 && atkChara.swap[3] == 0 && defChara.swap[1] == 0 && defChara.swap[3] == 0)
                {
                    atkChara.swap[1] = atkChara.status[1];
                    atkChara.swap[3] = atkChara.status[3];
                    defChara.swap[1] = defChara.status[1];
                    defChara.swap[3] = defChara.status[3];
                    i = Mathf.FloorToInt((atkChara.status[1] + defChara.status[1]) / 2f);
                    atkChara.status[1] = i;
                    defChara.status[1] = i;
                    i = Mathf.FloorToInt((atkChara.status[3] + defChara.status[3]) / 2f);
                    atkChara.status[3] = i;
                    defChara.status[3] = i;
                    Message("お互いのこうげきととくこうを分かち合った！");
                }
                else
                {
                    Message("しかしうまくいかなかった");
                }
                break;
            case "みらいよち":
                if (atkChara.master)
                {
                    if (field.enemy[8] == 0)
                    {
                        field.enemy[8] = 3;
                        field.enemy[9] = Mathf.FloorToInt(atkChara.RankStatus(3) * 100 * 2f / 5f);
                        if (atkChara.type1 == "エスパー" || atkChara.type2 == "エスパー") field.enemy[9] = Mathf.FloorToInt(field.enemy[9] * 1.5f);
                        yield return AreaIE("未来に攻撃を託した");
                    }
                    else
                    {
                        Message("しかしうまくいかなかった");
                    }
                }
                else
                {
                    if (field.player[8] == 0)
                    {
                        field.player[8] = 3;
                        field.player[9] = Mathf.FloorToInt(atkChara.RankStatus(3) * 100 * 2f / 5f);
                        if (atkChara.type1 == "エスパー" || atkChara.type2 == "エスパー") field.enemy[9] = Mathf.FloorToInt(field.enemy[9] * 1.5f);
                        yield return AreaIE("未来に攻撃を託した");
                    }
                    else
                    {
                        Message("しかしうまくいかなかった");
                    }
                }
                break;
            case "めいそう":
                yield return atkChara.StatusCheak(3, 1);
                yield return atkChara.StatusCheak(4, 1);
                break;
            case "ゆめくい":
                damage = Mathf.FloorToInt(prevSkillDamage * 0.5f);
                yield return DamageIE( defChara, atkChara, atkChara.nameC + "は　相手からHPを吸い取った", -damage);
                break;
            case "リフレクター":
                if (atkChara.master)
                {
                    if (field.player[4] == 0)
                    {
                        field.player[4] = 5;
                        if (ItemName(atkChara.item) == "ひかりのねんど") field.player[4] += 3;
                        yield return AreaIE("物理攻撃を軽減する壁を作った！");
                    }
                    else
                    {
                        Message("リフレクターはもう張られている");
                    }
                }
                else
                {
                    if (field.enemy[4] == 0)
                    {
                        field.enemy[4] = 5;
                        if (ItemName(atkChara.item) == "ひかりのねんど") field.enemy[3] += 3;
                        yield return AreaIE("物理攻撃を軽減する壁を作った！");
                    }
                    else
                    {
                        Message("リフレクターはもう張られている");
                    }
                }
                break;
            case "きゅうけつ":
                damage = Mathf.FloorToInt(prevSkillDamage * 0.5f);
                yield return DamageIE( defChara, atkChara, atkChara.nameC + "は　相手からHPを吸い取った", -damage);
                break;
            case "ちょうのまい":
                yield return atkChara.StatusCheak(3, 1);
                yield return atkChara.StatusCheak(4, 1);
                yield return atkChara.StatusCheak(5, 1);
                break;
            case "とどめばり":
                if (defChara.error==8)
                {
                    yield return atkChara.StatusCheak(1, 1);
                    yield return atkChara.StatusCheak(2, 1);
                    yield return atkChara.StatusCheak(3, 1);
                    yield return atkChara.StatusCheak(4, 1);
                    yield return atkChara.StatusCheak(5, 1);
                }
                break;
            case "とびかかる":
                yield return defChara.StatusCheak(1, -1);
                break;
            case "とんぼがえり":
                yield return EffectChange(atkChara.master, r => b = r);
                break;
            case "ねばねばネット":
                if (!atkChara.master)
                {
                    if (field.player[12] == 0)
                    {
                        field.player[12] = 1;
                        yield return AreaIE("足を取る網を張った！");
                    }
                    else
                    {
                        Message("ねばねばネットはもう張られている");
                    }
                }
                else
                {
                    if (field.enemy[12] == 0)
                    {
                        field.enemy[12] = 1;
                        yield return AreaIE("足を取る網を張った！");
                    }
                    else
                    {
                        Message("ねばねばネットはもう張られている");
                    }
                }
                break;
            case "はいよるいちげき":
                yield return defChara.StatusCheak(3, -1);
                break;
            case "ぼうぎょしれい":
                yield return atkChara.StatusCheak(2, 1);
                yield return atkChara.StatusCheak(4, 1);
                break;
            case "まとわりつく":
                if (defChara.error2[2] == 0)
                {
                    defChara.error2[2] = 4;
                    yield return EffectIE(atkChara, defChara, defChara.nameC + "は　とらわれた");
                }
                break;
            case "ミサイルばり":
                num = UnityEngine.Random.Range(1, 5);
                b = false;
                for (i = 0; i < num; i++)
                {
                    if (defChara.error == 8) b = true;
                    if (atkChara.error == 8) b = true;
                    if (atkChara.master && atkChara != battlerP) b = true;
                    if (!atkChara.master && atkChara != battlerE) b = true;
                    if (atkChara.master && defChara != battlerE) b = true;
                    if (!atkChara.master && defChara != battlerP) b = true;
                    if (b)
                    {
                        break;
                    }
                    yield return SkillAttack(skill, atkChara, defChara);
                }
                Message((i + 1) + "回あたった");
                break;
            case "ささやき":
                if (PerCorrect(atkChara.master, 10))
                {
                    yield return defChara.StatusCheak(4, -1);
                }
                break;
            case "れんぞくぎり":
                atkChara.buffer[13] += 1;
                break;
            case "ムカデにんげん":
                if (defChara.error == 0)
                    if (PerCorrect(atkChara.master, 30))
                {
                    yield return defChara.ErrorCheak(6);
                }
                break;
            case "いわなだれ":
                if (PerCorrect(atkChara.master, 30))
                {
                    defChara.error2[1] = 1;
                    yield return EffectIE(atkChara, defChara, defChara.nameC + "を　怯ませた！");
                }
                break;
            case "がんせきふうじ":
                yield return defChara.StatusCheak(5, -1);
                break;
            case "げんしのちから":
                if (PerCorrect(atkChara.master, 10))
                {
                    yield return atkChara.StatusCheak(1, 1);
                    yield return atkChara.StatusCheak(2, 1);
                    yield return atkChara.StatusCheak(3, 1);
                    yield return atkChara.StatusCheak(4, 1);
                    yield return atkChara.StatusCheak(5, 1);
                }
                break;
            case "ころがる":
                if (atkChara.buffer[4] == 0) atkChara.buffer[4] = 5;
                yield return EffectIE(defChara, atkChara, "");
                break;
            case "ステルスロック":
                if (!atkChara.master)
                {
                    if (field.player[10] == 0)
                    {
                        field.player[10] = 1;
                        yield return AreaIE("鋭利な岩を設置した！");
                    }
                    else
                    {
                        Message("ステルスロックはもう張られている");
                    }
                }
                else
                {
                    if (field.enemy[10] == 0)
                    {
                        field.enemy[10] = 1;
                        yield return AreaIE("鋭利な岩を設置した！");
                    }
                    else
                    {
                        Message("ステルスロックはもう張られている");
                    }
                }
                break;
            case "すなあらし":
                if (field.both[0] != 4)
                {
                    field.both[0] = 4;
                    field.both[1] = 5;
                    if (ItemName(atkChara.item) == "てんきデッキ") field.both[1] += 3;
                    yield return AreaIE("砂嵐が吹き始めた！");
                }
                else
                {
                    Message("すでに砂嵐は吹いている");
                }
                break;
            case "もろはのずつき":
                damage = Mathf.FloorToInt(prevSkillDamage * 0.5f);
                yield return DamageIE( defChara, atkChara, atkChara.nameC + "は　反動を受けた", damage);
                break;
            case "ロックカット":
                yield return atkChara.StatusCheak(5, 1);
                break;
            case "ロックブラスト":
                num = UnityEngine.Random.Range(2, 4);
                b = false;
                for (i = 0; i < num; i++)
                {
                    if (defChara.error == 8) b = true;
                    if (atkChara.error == 8) b = true;
                    if (atkChara.master && atkChara != battlerP) b = true;
                    if (!atkChara.master && atkChara != battlerE) b = true;
                    if (atkChara.master && defChara != battlerE) b = true;
                    if (!atkChara.master && defChara != battlerP) b = true;
                    if (b)
                    {
                        break;
                    }
                    yield return SkillAttack(skill, atkChara, defChara);
                }
                Message((i + 1) + "回あたった");
                break;
            case "メテオビーム":
                if (atkChara.buffer[8] == 0)
                {
                    atkChara.buffer[8] = 1;
                    yield return EffectIE(defChara, atkChara, "惑星の力を溜めている！");
                    yield return atkChara.StatusCheak(3, 1);
                }
                else
                {
                    atkChara.buffer[8] = 0;
                    yield return EffectIE(defChara, atkChara, "");
                }
                break;
            case "あやしいひかり":
                if (defChara.error2[0] == 0)
                {
                    defChara.error2[0] = 2;
                    yield return EffectIE(atkChara, defChara, defChara.nameC + "は　混乱した！");
                }
                else yield return EffectIE(atkChara, defChara, defChara.nameC + "は　すでに混乱している");
                break;
            case "おどろかす":
                if (PerCorrect(atkChara.master, 40))
                {
                    defChara.error2[1] = 1;
                    yield return EffectIE(atkChara, defChara, defChara.nameC + "を　怯ませた！");
                }
                break;
            case "シャドーパンチ":
                if (atkChara.master) correctP[0] += 20;
                else correctE[0] += 20;
                break;
            case "シャドーボール":
                if (PerCorrect(atkChara.master, 20))
                {
                    yield return defChara.StatusCheak(4, -1);
                }
                break;
            case "のろい":
                damage = Mathf.FloorToInt(atkChara.status[0] / 2f);
                if (atkChara.hp > damage && defChara.error2[3] == 0)
                {
                    defChara.error2[3] = 1;
                    yield return EffectIE(atkChara, defChara, "");
                    yield return DamageIE( defChara, atkChara, atkChara.nameC + "は　HPを削ってのろいをかけた", damage);
                }
                else Message("しかしうまくいかなかった");
                break;
            case "みちづれ":
                atkChara.buffer[3] = 1;
                yield return EffectIE(defChara, atkChara, atkChara.nameC + "は　相手をみちづれにしようとしている");
                break;
            case "こころのかべ":
                if (atkChara.master)
                {
                    if (field.player[3] == 0)
                    {
                        field.player[3] = 5;
                        if (ItemName(atkChara.item) == "ひかりのねんど") field.player[3] += 3;
                        yield return AreaIE("特殊攻撃を軽減する壁を作った！");
                    }
                    else
                    {
                        Message("こころのかべはもう張られている");
                    }
                }
                else
                {
                    if (field.enemy[3] == 0)
                    {
                        field.enemy[3] = 5;
                        if (ItemName(atkChara.item) == "ひかりのねんど") field.enemy[3] += 3;
                        yield return AreaIE("物理攻撃を軽減する壁を作った！");
                    }
                    else
                    {
                        Message("こころのかべはもう張られている");
                    }
                }
                break;
            case "げきりん":
                if (atkChara.buffer[4] == 0)
                {
                    atkChara.buffer[4] = 3;
                    yield return EffectIE(defChara, atkChara, "");
                }
                break;
            case "ダブルチョップ":
                num = 1;
                b = false;
                for (i = 0; i < num; i++)
                {
                    if (defChara.error == 8) b = true;
                    if (atkChara.error == 8) b = true;
                    if (atkChara.master && atkChara != battlerP) b = true;
                    if (!atkChara.master && atkChara != battlerE) b = true;
                    if (atkChara.master && defChara != battlerE) b = true;
                    if (!atkChara.master && defChara != battlerP) b = true;
                    if (b)
                    {
                        break;
                    }
                    yield return SkillAttack(skill, atkChara, defChara);
                }
                Message((i + 1) + "回あたった");
                break;
            case "ドラゴンテール":
                playerChange = null;
                enemyChange = null;
                num = UnityEngine.Random.Range(0, 2);
                if (atkChara.master) member = enemyMember;
                else member = playerMember;
                b = false;
                if (num == 0)
                {
                    for (i = 0; i < 3; i++)
                    {
                        if (defChara != member[i])
                        {
                            b = true;
                            yield return Summon(defChara.master, i, "skill");
                            break;
                        }
                    }
                }
                else
                {
                    for (i = 2; i >= 0; i--)
                    {
                        if (defChara != member[i])
                        {
                            b = true;
                            yield return Summon(defChara.master, i, "skill");
                            break;
                        }
                    }
                }
                if (!b) Message("しかしうまくいかなかった");
                break;
            case "りゅうせいぐん":
                yield return atkChara.StatusCheak(3, -2);
                break;
            case "りゅうのまい":
                yield return atkChara.StatusCheak(1, 1);
                yield return atkChara.StatusCheak(5, 1);
                break;
            case "あくのはどう":
                if (PerCorrect(atkChara.master, 20))
                {
                    defChara.error2[1] = 1;
                    yield return EffectIE(atkChara, defChara, defChara.nameC + "を　怯ませた！");
                }
                break;
            case "うそなき":
                yield return defChara.StatusCheak(1, -1);
                yield return defChara.StatusCheak(2, -1);
                yield return defChara.StatusCheak(3, -1);
                yield return defChara.StatusCheak(4, -1);
                break;
            case "おきみやげ":
                yield return defChara.StatusCheak(1, -2);
                yield return defChara.StatusCheak(3, -2);
                yield return DamageIE( defChara, atkChara,"", 9999);
                break;
            case "かみくだく":
                if (PerCorrect(atkChara.master, 20))
                {
                    yield return defChara.StatusCheak(2, -1);
                }
                break;
            case "すてゼリフ":
                yield return defChara.StatusCheak(1, -1);
                yield return defChara.StatusCheak(3, -1);
                yield return EffectChange(atkChara.master, r => b = r);
                break;
            case "ちょうはつ":
                if (defChara.error2[11] == 0)
                {
                    defChara.error2[11] = 4;
                    yield return EffectIE(atkChara, defChara, defChara.nameC + "は　変化技が出せなくなった");
                }
                else
                {
                    yield return EffectIE(atkChara, defChara, defChara.nameC + "は　すでにちょうはつされている");
                }
                break;
            case "つめとぎ":
                yield return atkChara.StatusCheak(1, 1);
                yield return atkChara.StatusCheak(6, 1);
                break;
            case "どろぼう":
                if(atkChara.item == 0 && defChara.item != 0)
                {
                    atkChara.item = defChara.item;
                    defChara.item = 0;
                    yield return EffectIE(atkChara, defChara, "");
                    yield return EffectIE(defChara, atkChara, "");
                    Message("もちものを奪い取った！");
                }
                break;
            case "バークアウト":
                yield return defChara.StatusCheak(3, -1);
                break;
            case "はたきおとす":
                if (defChara.item != 0)
                {
                    defChara.item = 0;
                    yield return EffectIE(atkChara, defChara, "");
                    Message("もちものをはたき落した！");
                }
                break;
            case "ひっくりかえす":
                for (i = 0; i < 8; i++) {
                    defChara.rank[i] *= -1;
                    yield return EffectIE(atkChara, defChara, defChara.nameC + "の　能力変化を逆転させた！");
                }
                break;
            case "わるだくみ":
                yield return atkChara.StatusCheak(3, 2);
                break;
            case "あくまのキッス":
                yield return defChara.ErrorCheak(7);
                break;
            case "とぼける":
                yield return atkChara.StatusCheak(3, 1);
                yield return atkChara.StatusCheak(2, 1);
                break;
            case "いちゃもん":
                if (defChara.error2[10] == 0)
                {
                    defChara.error2[10] = 1;
                    yield return EffectIE(atkChara, defChara, defChara.nameC + "は　同じ技を連続で出せなくなった");
                }
                else
                {
                    yield return EffectIE(atkChara, defChara, defChara.nameC + "は　すでにいちゃもんされている");
                }
                break;
            case "アイアンテール":
                if (PerCorrect(atkChara.master, 30))
                {
                    yield return defChara.StatusCheak(2, -1);
                }
                break;
            case "アイアンヘッド":
                if (PerCorrect(atkChara.master, 30))
                {
                    defChara.error2[1] = 1;
                    yield return EffectIE(atkChara, defChara, defChara.nameC + "を　怯ませた！");
                }
                break;
            case "ギアソーサー":
                num = 1;
                b = false;
                for (i = 0; i < num; i++)
                {
                    if (defChara.error == 8) b = true;
                    if (atkChara.error == 8) b = true;
                    if (atkChara.master && atkChara != battlerP) b = true;
                    if (!atkChara.master && atkChara != battlerE) b = true;
                    if (atkChara.master && defChara != battlerE) b = true;
                    if (!atkChara.master && defChara != battlerP) b = true;
                    if (b)
                    {
                        break;
                    }
                    yield return SkillAttack(skill, atkChara, defChara);
                }
                Message((i + 1) + "回あたった");
                break;
            case "きんぞくおん":
                yield return defChara.StatusCheak(4, -3);
                break;
            case "コメットパンチ":
                if (PerCorrect(atkChara.master, 30))
                {
                    yield return atkChara.StatusCheak(1, 1);
                }
                break;
            case "てっぺき":
                yield return atkChara.StatusCheak(2, 2);
                break;
            case "はがねのつばさ":
                if (PerCorrect(atkChara.master, 20))
                {
                    yield return atkChara.StatusCheak(2, 1);
                }
                break;
            case "メタルクロー":
                if (PerCorrect(atkChara.master, 70))
                {
                    yield return atkChara.StatusCheak(1, 1);
                }
                break;
            case "ラスターカノン":
                if (PerCorrect(atkChara.master, 20))
                {
                    yield return defChara.StatusCheak(2, -1);
                }
                break;
            case "ぷあうあー":
                num = UnityEngine.Random.Range(0, 5);
                b = false;
                for (i = 0; i < num; i++)
                {
                    if (defChara.error == 8) b = true;
                    if (atkChara.error == 8) b = true;
                    if (atkChara.master && atkChara != battlerP) b = true;
                    if (!atkChara.master && atkChara != battlerE) b = true;
                    if (atkChara.master && defChara != battlerE) b = true;
                    if (!atkChara.master && defChara != battlerP) b = true;
                    if (b)
                    {
                        break;
                    }
                    yield return SkillAttack(skill, atkChara, defChara);
                }
                Message((i + 1) + "回あたった");
                break;
            case "あまえる":
                yield return defChara.StatusCheak(1, -2);
                if(atkChara.sex != defChara.sex) yield return defChara.StatusCheak(3, -1);
                break;
            case "じゃれつく":
                if (PerCorrect(atkChara.master, 10))
                {
                    yield return defChara.StatusCheak(1, -1);
                }
                break;
            case "てんしのキッス":
                if (defChara.error2[0] == 0)
                {
                    defChara.error2[0] = 2;
                    yield return EffectIE(atkChara, defChara, defChara.nameC + "は　混乱した！");
                }
                else yield return EffectIE(atkChara, defChara, defChara.nameC + "は　すでに混乱している");
                break;
            case "ドレインキッス":
                damage = Mathf.FloorToInt(prevSkillDamage * 0.75f);
                yield return DamageIE( defChara, atkChara, atkChara.nameC + "は　相手からHPを吸い取った", -damage);
                break;
            case "マジカルシャイン":
                if (PerCorrect(atkChara.master, 30))
                {
                    yield return defChara.StatusCheak(3, -1);
                }
                break;
            case "アキニウムこうせん":
                if (defChara.error2[4] == 0)
                {
                    defChara.error2[4] = 1;
                    yield return EffectIE(atkChara, defChara, defChara.nameC + "は　メロメロになった！");
                }
                else yield return EffectIE(atkChara, defChara, defChara.nameC + "は　すでにメロメロだ！");
                break;
        }
        yield return new WaitForSeconds(0.1f);
    }

    IEnumerator DeathChange()
    {
        while (deathOrder.Count != 0)
        {
            playerChange = null;
            enemyChange = null;
            if (deathOrder.Count == 2)
            {
                if (deathOrder[0].master && playerMember[0].error == 8 && playerMember[1].error == 8 && playerMember[2].error == 8)
                {
                    punBattle.Send("lose", "", deathOrder[0].master);
                    punBattle.Send("win", "", !deathOrder[0].master);
                    while (true)
                    {
                        yield return new WaitForSecondsRealtime(10f);
                    }
                }
                if (!deathOrder[0].master && enemyMember[0].error == 8 && enemyMember[1].error == 8 && enemyMember[2].error == 8)
                {
                    punBattle.Send("lose", "", deathOrder[0].master);
                    punBattle.Send("win", "", !deathOrder[0].master);
                    while (true)
                    {
                        yield return new WaitForSecondsRealtime(10f);
                    }
                }
                if (deathOrder[1].master && playerMember[0].error == 8 && playerMember[1].error == 8 && playerMember[2].error == 8)
                {
                    punBattle.Send("lose", "", deathOrder[1].master);
                    punBattle.Send("win", "", !deathOrder[1].master);
                    while (true)
                    {
                        yield return new WaitForSecondsRealtime(10f);
                    }
                }
                if (!deathOrder[1].master && enemyMember[0].error == 8 && enemyMember[1].error == 8 && enemyMember[2].error == 8)
                {
                    punBattle.Send("lose", "", deathOrder[1].master);
                    punBattle.Send("win", "", !deathOrder[1].master);
                    while (true)
                    {
                        yield return new WaitForSecondsRealtime(10f);
                    }
                }


                punBattle.Send("change", "", deathOrder[0].master);
                punBattle.Send("change", "", deathOrder[1].master);
                while (playerChange == null || enemyChange == null)
                {
                    yield return new WaitForSecondsRealtime(0.1f);
                }
                if (deathOrder[0].master)
                {
                    yield return Summon(true, playerChange.num, "death");
                    yield return Summon(false, enemyChange.num, "death");
                }
                else
                {
                    yield return Summon(false, enemyChange.num, "death");
                    yield return Summon(true, playerChange.num, "death");
                }
                deathOrder.RemoveAt(0);
                deathOrder.RemoveAt(0);
            }
            else if (deathOrder.Count == 1)
            {
                if (deathOrder[0].master && playerMember[0].error == 8 && playerMember[1].error == 8 && playerMember[2].error == 8)
                {
                    punBattle.Send("lose", "", deathOrder[0].master);
                    punBattle.Send("win", "", !deathOrder[0].master);
                    while (true)
                    {
                        yield return new WaitForSecondsRealtime(10f);
                    }
                }
                if (!deathOrder[0].master && enemyMember[0].error == 8 && enemyMember[1].error == 8 && enemyMember[2].error == 8)
                {
                    punBattle.Send("lose", "", deathOrder[0].master);
                    punBattle.Send("win", "", !deathOrder[0].master);
                    while (true)
                    {
                        yield return new WaitForSecondsRealtime(10f);
                    }
                }

                punBattle.Send("change", "", deathOrder[0].master);
                while (playerChange == null && enemyChange == null)
                {
                    yield return new WaitForSecondsRealtime(0.1f);
                }
                if (deathOrder[0].master)
                {
                    yield return Summon(true, playerChange.num, "death");
                }
                else
                {
                    yield return Summon(false, enemyChange.num, "death");
                }
                deathOrder.RemoveAt(0);
            }
        }
    }
    public IEnumerator EffectChange(bool master, System.Action<bool> callback)
    {
        List<CharaStatus> member;
        if (master) member = playerMember;
        else member = enemyMember;
        int j = 0;
        for(int i = 0; i < 3; i++)
        {
            if (member[i].error != 8) j++;
        }
        if (j < 2)
        {
            callback(false);
            yield break;
        }
        punBattle.Send("change", "", master);
        playerChange = null;
        enemyChange = null;
        while (playerChange == null && enemyChange == null)
        {
            yield return new WaitForSecondsRealtime(0.1f);
        }
        if (playerChange!=null)
        {
            yield return Summon(true, playerChange.num,"");
        }
        else
        {
            yield return Summon(false, enemyChange.num,"");
        }
        callback(true);
    }
    public string ItemName(int i)
    {
        if (i == 0) return "";
        Item item = itemMaster.ItemList.Find(a => a.no == i);
        return item.name;
        
    }
    public string PassiveText(CharaStatus cs)
    {
        return cs.nameC + "の　" + cs.passiveName + "！\n";
    }
    public string ItemText(CharaStatus cs)
    {
        return cs.nameC + "の　" + ItemName(cs.item) + "！\n";
    }
    public IEnumerator ItemUse(CharaStatus cs)
    {
        switch (cs.passiveName)
        {
            case "甘党王子":
                if (ItemName(cs.item).Contains("のみ"))
                {
                    yield return DamageIE(cs.enemy,cs, PassiveText(cs)+cs.nameC + "は　回復した。",-Mathf.FloorToInt(cs.status[0]/4f));
                }
                break;
        }
        cs.item = 0;
        yield return EffectIE(cs.enemy, cs, "");
    }
    bool PerCorrect(bool master ,int per)
    {
        float perC;
        if (master) perC = per + (100 - per) * correctP[1];
        else perC = per + (100 - per) * correctE[1];
        if (perC > Random.Range(0, 100))
        {
            if (master) correctP[1] -= (100 - per);
            else correctE[1] -= (100 - per);
            return true;
        }
        else
        {
            if (master) correctP[1] += per;
            else correctE[1] += per;
            return false;
        }
    }

    public IEnumerator DamageIE(CharaStatus atkChara, CharaStatus defChara, string text, int d)
    {
        yield return DamageIE(null, atkChara, defChara, text, d,0);
    }
    public IEnumerator DamageIE(Skill skill, CharaStatus atkChara, CharaStatus defChara, string text, int d,int typePer)
    {
        if (defChara.hp <= 0) yield break;

        prevSkillDamage = d;
        if (skill != null)
        {
            if (skill.sound == 0 && defChara.buffer[0] > 0)
            {
                yield return EffectIE(atkChara, defChara, text);
                defChara.buffer[0] -= d;
                string t = "みがわりが代わりに攻撃を受けた";
                if (defChara.buffer[0] <= 0)
                {
                    t += "\n" + "みがわりが消えてしまった";
                    prevSkillDamage += defChara.buffer[0];
                    defChara.buffer[0] = 0;
                }
                yield return EffectIE(atkChara, defChara, t);
                yield break;
            }
            else
            {
                if (skill.physics == "物理") defChara.damageP = d;
                if (skill.physics == "特殊") defChara.damageS = d;

                Damage damage = new Damage();
                int tasuki = 0;
                if (d > defChara.hp && defChara.passiveName== "葬儀配信" && defChara.passiveBool == 0)
                {
                    defChara.passiveBool = 1;
                    d = defChara.hp - 1;
                    tasuki = 1;
                }

                if (defChara.hp == defChara.status[0]  && d>=defChara.hp  && ItemName(defChara.item) == "きあいのタスキ")
                {
                    d = defChara.status[0] - 1;
                    yield return ItemUse(defChara);
                    tasuki = 2;
                }
                defChara.hp -= d;
                if (defChara.hp > defChara.status[0]) defChara.hp = defChara.status[0];
                if (skill != null) damage.skillNo = skill.no;
                damage.player = defChara.master;
                damage.hpPer = defChara.HPPer();
                damage.hpText = defChara.HPText();
                damage.message = text;
                damage.typePer = typePer;
                punBattle.Send("damage", damage.ToString());

                if(tasuki==1) Message( PassiveText(defChara)+"瀕死になる攻撃を耐えた！");
                if(tasuki==2) Message( ItemText(defChara) + "瀕死になる攻撃を耐えた！");


                if (defChara.hp <= 0)
                {
                    prevSkillDamage += defChara.hp;
                    defChara.hp = 0;
                    defChara.error = 8;
                    deathOrder.Add(defChara);
                    punBattle.Send("death", (defChara.master).ToString());
                }

                switch (defChara.passiveName)
                {
                    case "クソザコパンチ":
                        if (skill.contact > 0)
                        {
                            Message( PassiveText(defChara));
                            yield return atkChara.StatusCheak(1, -1);
                        }
                        break;
                    case "イチゴミルクウミウシ":
                        if (skill.contact > 0)
                        {
                            Message(PassiveText(defChara));
                            yield return atkChara.ErrorCheak(1);
                        }
                        break;
                    case "女たらし":
                        if (skill.contact > 0 && atkChara.sex =="女性")
                        {
                            if (PerCorrect(defChara.master, 30))
                            {
                                Message(PassiveText(defChara));
                                if (atkChara.error2[4] == 0)
                                {
                                    atkChara.error2[4] = 1;
                                    yield return EffectIE(defChara, atkChara, atkChara.nameC + "は　メロメロになった！");
                                }
                                else yield return EffectIE(defChara, atkChara, atkChara.nameC + "は　すでにメロメロだ！");
                            }
                        }
                        break;
                    case "隠居":
                        defChara.passiveBool = 1;
                        break;
                }
                switch (ItemName(atkChara.item))
                {
                    case "おうじゃのしるし":
                        if (PerCorrect(atkChara.master, 10))
                        {
                            defChara.error2[1] = 1;
                            yield return EffectIE(atkChara, defChara, ItemText(atkChara) + defChara.nameC + "を　怯ませた！");
                        }
                        break;
                    case "いのちのたま":
                        yield return DamageIE(defChara, atkChara, ItemText(atkChara) + "ダメージを受けた", Mathf.FloorToInt(atkChara.status[0] / 10f));
                        break;
                    case "かいがらのすず":
                        yield return DamageIE(defChara, atkChara, ItemText(atkChara) + "HPを吸収した", -Mathf.FloorToInt(d / 6f));
                        break;
                }
                switch (ItemName(defChara.item))
                {
                    case "ゴツゴツメット":
                        if (skill.contact>0)
                        {
                            yield return DamageIE(defChara, atkChara, ItemText(defChara) + atkChara.nameC + "は　ダメージを受けた", Mathf.FloorToInt(atkChara.status[0] / 6f));
                        }
                        break;
                    case "じゃくてんほけん":
                        if (typePer > 100 && defChara.error != 8)
                        {
                            Message(ItemText(defChara));
                            yield return defChara.StatusCheak(1, 2);
                            yield return defChara.StatusCheak(3, 2);
                            yield return ItemUse(defChara);
                        }
                        break;
                    case "だっしゅつボタン":
                        if (defChara.error!=8)
                        {
                            Message(ItemText(defChara) + "自分を手持ちに戻す");
                            yield return ItemUse(defChara);
                            bool b=false;
                            yield return EffectChange(defChara.master, r => b = r);
                            if (!b) Message("しかしうまくいかなかった");
                        }
                        break;
                    case "ふうせん":
                        if (defChara.error != 8)
                        {
                            Message(ItemText(defChara) + "ふうせんがわれてしまった");
                            yield return ItemUse(defChara);
                        }
                        break;
                    case "レッドカード":
                        if (defChara.error != 8)
                        {
                            Message(ItemText(defChara) + "相手を手持ちに戻す");
                            playerChange = null;
                            enemyChange = null;
                            int num = UnityEngine.Random.Range(0, 2);
                            List<CharaStatus> member = null;
                            if (defChara.master) member = enemyMember;
                            else member = playerMember;
                            bool b = false;
                            if (num == 0)
                            {
                                for (int i = 0; i < 3; i++)
                                {
                                    if (atkChara != member[i] && member[i].error != 8)
                                    {
                                        b = true;
                                        yield return Summon(atkChara.master, i, "skill");
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                for (int i = 2; i >= 0; i--)
                                {
                                    if (atkChara != member[i] && member[i].error != 8)
                                    {
                                        b = true;
                                        yield return Summon(atkChara.master, i, "skill");
                                        break;
                                    }
                                }
                            }
                            if (!b) Message("しかしうまくいかなかった");
                            yield return ItemUse(defChara);
                            break;
                        }
                        break;
                    case "レンブのみ":
                        if (skill.physics=="特殊")
                        {
                            yield return DamageIE(defChara, atkChara, ItemText(defChara) + atkChara.nameC + "は　ダメージを受けた", Mathf.FloorToInt(atkChara.status[0] / 6f));
                            yield return ItemUse(defChara);
                        }
                        break;
                    case "ジャポのみ":
                        if (skill.physics == "物理")
                        {
                            yield return DamageIE(defChara, atkChara, ItemText(defChara) + atkChara.nameC + "は　ダメージを受けた", Mathf.FloorToInt(atkChara.status[0] / 6f));
                            yield return ItemUse(defChara);
                        }
                        break;
                    case "アッキのみ":
                        if (skill.physics == "物理")
                        {
                            Message(ItemText(defChara));
                            yield return defChara.StatusCheak(2, 1);
                            yield return ItemUse(defChara);
                        }
                        break;
                    case "タラプのみ":
                        if (skill.physics == "特殊")
                        {
                            Message(ItemText(defChara));
                            yield return defChara.StatusCheak(4, 1);
                            yield return ItemUse(defChara);
                        }
                        break;
                }

            }
        }
        if (skill == null)
        {
            if(defChara.passiveName=="カシュッ" && d < 0)
            {
                Message(PassiveText(defChara)+"回復量を増加させた");
                d = Mathf.FloorToInt(d * 1.5f);
                float effectC;
                float per = 80;
                if (defChara.master) effectC = per + (100 - per) * correctP[1];
                else effectC = per + (100 - per) * correctE[1];
                if (effectC > Random.Range(0, 100))
                {
                    if (defChara.master) correctP[1] -= (100 - per);
                    else correctE[1] -= (100 - per);
                }
                else
                {
                    if (defChara.master) correctP[1] += per;
                    else correctE[1] += per;

                    if (defChara.error2[0] == 0)
                    {
                        defChara.error2[0] = 2;
                        yield return EffectIE(atkChara, defChara, defChara.nameC + "は　混乱した！");
                    }
                    else yield return EffectIE(atkChara, defChara, defChara.nameC + "は　すでに混乱している");
                }
            }

            Damage damage = new Damage();
            defChara.hp -= d;
            if (defChara.hp > defChara.status[0]) defChara.hp = defChara.status[0];
            if (skill != null) damage.skillNo = skill.no;
            damage.player = defChara.master;
            damage.hpPer = defChara.HPPer();
            damage.hpText = defChara.HPText();
            damage.message = text;
            damage.typePer = typePer;
            punBattle.Send("damage", damage.ToString());
            if (defChara.hp <=0)
            {
                defChara.hp = 0;
                defChara.error = 8;
                deathOrder.Add(defChara);
                punBattle.Send("death", (defChara.master).ToString());
            }
        }

        if (defChara.hp <= 0)
        {
            if (defChara.buffer[3] > 0)
            {
                yield return DamageIE( defChara, atkChara, defChara.nameC + "が　相手を道連れにする", 9999);
            }

            for(int i = 0; i < 3; i++)
            {
                if (playerMember[i].passiveName== "息継ぎ" && defChara!= playerMember[i])
                {

                    if (playerMember[i] == battlerP)
                    {
                        yield return DamageIE(defChara, atkChara,"とくせいの　息継ぎで回復した！", -Mathf.FloorToInt(playerMember[i].status[0] / 5f));
                    }
                    else
                    {
                        playerMember[i].hp += Mathf.FloorToInt(playerMember[i].status[0] / 5f);
                        if (playerMember[i].hp > playerMember[i].status[0]) playerMember[i].hp = playerMember[i].status[0];
                    }
                }
                if (enemyMember[i].passiveName == "息継ぎ" && defChara != enemyMember[i])
                {
                    if (enemyMember[i] == battlerE)
                    {
                        yield return DamageIE(defChara, atkChara, "とくせいの　息継ぎで回復した！", -Mathf.FloorToInt(playerMember[i].status[0] / 5f));
                    }
                    else
                    {
                        enemyMember[i].hp += Mathf.FloorToInt(enemyMember[i].status[0] / 5f);
                        if (enemyMember[i].hp > enemyMember[i].status[0]) enemyMember[i].hp = enemyMember[i].status[0];
                    }
                }
            }
            yield break;
        }

        switch (defChara.passiveName)
        {
            case "ライン越え":
                if (defChara.hp <= defChara.status[0]/2 && defChara.passiveBool==0)
                {
                    defChara.passiveBool = 1;
                    Message(PassiveText(defChara));
                    yield return defChara.StatusCheak(1, 1);
                    yield return defChara.StatusCheak(2, 1);
                    yield return defChara.StatusCheak(5, 1);
                }
                break;
        }
        switch (ItemName(defChara.item))
        {
            case "オボンのみ":
                if (defChara.HPPer()<=0.5f && d>0)
                {
                    int damage = -Mathf.FloorToInt(defChara.status[0] / 4f);
                    text = ItemText(defChara) + defChara.nameC + "は　回復した";
                    yield return DamageIE(atkChara, defChara, text, damage);
                    yield return ItemUse(defChara);
                }
                break;
        }
    }
    public IEnumerator EffectIE(CharaStatus atkChara, CharaStatus defChara, string text)
    {
        yield return EffectIE(atkChara, defChara, text, "");
    }
    public IEnumerator EffectIE(CharaStatus atkChara, CharaStatus defChara, string text,string ef)
    {
        Effect effect = defChara.ToEffect();
        effect.message = text;
        effect.effect = ef;
        punBattle.Send("effect", effect.ToString());

        switch (defChara.passiveName)
        {
            case "横綱「ローションキング」":
                if (atkChara.error2[1] > 0)
                {
                    atkChara.error2[1] = 0;
                    yield return EffectIE(atkChara, defChara, PassiveText(defChara)+"ひるみから回復した");
                }
                break;
        }
        switch (ItemName(defChara.item))
        {
            case "ラムのみ":
                if (defChara.error!=0 && defChara.error !=8)
                {
                    defChara.error = 0;
                    defChara.errorTurn = 0;
                    yield return EffectIE(atkChara,defChara, ItemText(defChara) + "状態異常を回復した");
                    yield return ItemUse(defChara);
                }
                break;
            case "メンタルハーブ":
                if (defChara.error != 8　&&(defChara.error2[0]>0 || defChara.error2[4] > 0 || defChara.error2[9] > 0 || defChara.error2[10] > 0 || defChara.error2[11] > 0 || defChara.error2[13] > 0))
                {
                    defChara.error2[0] = 0;
                    defChara.error2[4] = 0;
                    defChara.error2[9] = 0;
                    defChara.error2[10] = 0;
                    defChara.error2[11] = 0;
                    defChara.error2[13] = 0;
                    yield return EffectIE(atkChara, defChara, ItemText(defChara) + "状態異常を回復した");
                    yield return ItemUse(defChara);
                }
                break;
        }
        yield return new WaitForSeconds(0.1f);
    }
    public IEnumerator AreaIE( string text)
    {
        Area area = field.ToArea();
        area.message = text;
        punBattle.Send("area", area.ToString());
        yield return new WaitForSeconds(0.1f);
    }
    public void Message(string text)
    {
        Area area = field.ToArea();
        area.message = text;
        punBattle.Send("area", area.ToString());
    }
    IEnumerator Summon(bool master, int i,string type)
    {
        CharaStatus battler;
        if (master) battler = battlerP;
        else battler = battlerE;
        if (battler != null)
        {
            if (battler.error2[4] > 0 && battler.error != 8)
            {
                yield return EffectIE(battler.enemy, battler, battler.nameC + "は　メロメロで場を離れようとしない！");
                battler.error2[4] = 0;
                yield return EffectIE(battler.enemy, battler, battler.nameC + "は　我に返った");
                yield break;
            }

            battler.ChangeEffect();
        }
        CharaStatus cs;
        if (master)
        {
            cs = playerMember[i];
            battlerP = cs;
        }
        else
        {
            cs = enemyMember[i];
            battlerE = cs;
        }
        Summon summon = cs.ToSummon();
        summon.type = type;
        punBattle.Send("summon", summon.ToString());
        cs.changeTurn = true;
        cs.firstAttack = true;
        if (battler != null)
        {
            battlerE.enemy = battlerP;
            battlerP.enemy = battlerE;
        }
        int[] fieldPE;
        if (master) fieldPE = field.player;
        else  fieldPE = field.enemy;

        if (fieldPE[7] > 0)
        {
            int damage = cs.status[0] - cs.hp;
            string text = cs.nameC + "は　いやしのねがいを受けて回復した";
            cs.error = 0;
            cs.errorTurn = 0;
            fieldPE[7] = 0;
            yield return EffectIE(cs.enemy, cs, "");
            yield return DamageIE( cs.enemy, cs, text, -damage);
        }
        if (fieldPE[10] > 0)
        {
            int typePer = Type.Per("いわ", cs.type1, cs.type2);
            int damage = Mathf.FloorToInt(cs.status[0] / 16f * typePer / 100);
            string text = cs.nameC + "は　ステルスロックでダメージを受けた";
            yield return DamageIE( cs.enemy, cs, text, damage);
        }
        if (fieldPE[11] == 1)
        {
            if (cs.type1 != "ひこう" && cs.type2 != "ひこう" && cs.type1 != "どく" && cs.type2 != "どく" && ItemName(cs.item)!="ふうせん")
            {
                Message(cs.nameC + "は　どくびしを踏んだ");
                yield return cs.ErrorCheak(1);
            }
        }
        if (fieldPE[11] >= 2)
        {
            if (cs.type1 != "ひこう" && cs.type2 != "ひこう" && cs.type1 != "どく" && cs.type2 != "どく" && ItemName(cs.item) != "ふうせん")
            {
                Message(cs.nameC + "は　もうどくびしを踏んだ");
                yield return cs.ErrorCheak(2);
            }
        }
        if (fieldPE[12] > 0)
        {
            if (cs.type1 != "ひこう" && cs.type2 != "ひこう" && ItemName(cs.item) != "ふうせん")
                Message(cs.nameC+"は　ねばねばネットにかかった！");
                yield return cs.StatusCheak(5, -1);
        }
        if (battler != null)
        {
            switch (battler.passiveName)
            {
                case "プロデュース":
                    Message(PassiveText(battler));
                    yield return cs.StatusCheak(6, 1);
                    break;
            }
        }
        switch (cs.passiveName)
        {
            case "しばくぞ":
                if (turn != 1)
                {
                    Message(PassiveText(cs));
                    if (cs.enemy.error != 8)
                    {
                        if (cs.enemy.type1 != "ゴースト" && cs.enemy.type2 != "ゴースト") yield return cs.enemy.StatusCheak(1, -1);
                        else yield return cs.StatusCheak(1, -1);
                    }
                    else Message("しかしうまくいかなかった");
                }
                break;
            case "もっとお話ししたい":
                if (turn != 1)
                {
                    cs.error2[12] += 2;
                    cs.enemy.error2[12] += 2;
                    yield return EffectIE(cs.enemy, cs, "");
                    yield return EffectIE(cs, cs.enemy, PassiveText(cs) + "お互いを2ターンの間にげられない状態にした");
                }
                break;
            case "異界の扉":
                if (turn != 1)
                {
                    if (field.both[5] == 0)
                    {
                        field.both[5] = 3;
                        yield return AreaIE("異界の扉が開かれた");
                    }
                    else
                    {
                        Message("異界の扉はすでに開かれている");
                    }
                }
                break;
        }
        if (battler != null)
        {
            switch (cs.enemy.passiveName)
            {
                case "逃げんの？":
                        Message(PassiveText(cs.enemy));
                        yield return cs.enemy.StatusCheak(3, 1);
                    break;
            }
        }
        switch (ItemName(cs.item))
        {
            case "ふうせん":
                if (turn != 1)
                {
                    Message(ItemText(cs) + cs.nameC + "は　浮いている");
                }
                break;
        }
    }
    IEnumerator FirstSummonPassive(CharaStatus cs)
    {
        switch (cs.passiveName)
        {
            case "しばくぞ":
                Message(PassiveText(cs));
                if (cs.enemy.error != 8)
                {
                    if (cs.enemy.type1 != "ゴースト" && cs.enemy.type2 != "ゴースト") yield return cs.enemy.StatusCheak(1, -1);
                    else yield return cs.StatusCheak(1, -1);
                }
                else Message("しかしうまくいかなかった");
                break;
            case "ライトニング・ゲイボルグ":
                Message(PassiveText(cs));
                yield return cs.StatusCheak(1, 1);
                break;
            case "あやまらないよ":
                Message(PassiveText(cs) + cs.nameC + "は　寝坊している！");
                yield return cs.ErrorCheak(7);
                break;
            case "異界の扉":
                if (field.both[5] == 0)
                {
                    field.both[5] = 3;
                    yield return AreaIE("異界の扉が開かれた");
                }
                else
                {
                    Message("異界の扉はすでに開かれている");
                }
                break;
        }
        switch (ItemName(cs.item))
        {
            case "ふうせん":
                Message(ItemText(cs) + cs.nameC + "は　浮いている");
                break;
        }
        yield break;
    }


    public void PartyReceive(bool master,int[] chara, int[] item, int[] passive, int[] exp0, int[] exp1, int[] exp2, int[] exp3, int[] exp4, int[] exp5, string[] skill0, string[] skill1, string[] skill2, string[] skill3,string playerName, string playerTitle, string playerAvator)
    {
        punBattle.EnemyPartySendReceive(master,chara, playerName, playerTitle, playerAvator);
        List<CharaStatus> party = playerParty;
        if(!master) party = enemyParty;

        for(int i = 0; i < 6; i++)
        {
            CharaStatus cs = new CharaStatus();
            Chara c = charaMaster.CharaList.Find(a => a.no == chara[i]);
            cs.master = master;
            cs.nameC = c.name;
            cs.sex = c.sex;
            cs.type1 = c.type1;
            cs.type2 = c.type2;
            cs.typeDefalut1 = c.type1;
            cs.typeDefalut2 = c.type2;
            cs.hp = 50 + c.h + exp0[i];
            cs.status[0] = 50 + c.h + exp0[i];
            cs.status[1] = c.a + exp1[i];
            cs.status[2] = c.b + exp2[i];
            cs.status[3] = c.c + exp3[i];
            cs.status[4] = c.d + exp4[i];
            cs.status[5] = c.s + exp5[i];
            cs.item = item[i];
            if (passive[i] == 1)
            {
                cs.passiveName = c.passiveName1;
                cs.passiveText = c.passiveText1;
            }
            else
            {
                cs.passiveName = c.passiveName2;
                cs.passiveText = c.passiveText2;
            }
            cs.skill[0] = skill0[i];
            cs.skill[1] = skill1[i];
            cs.skill[2] = skill2[i];
            cs.skill[3] = skill3[i];
            cs.skill[4] = "0";
            Skill s0 = skillMaster.SkillList.Find(a => a.no == skill0[i]);
            Skill s1 = skillMaster.SkillList.Find(a => a.no == skill1[i]);
            Skill s2 = skillMaster.SkillList.Find(a => a.no == skill2[i]);
            Skill s3 = skillMaster.SkillList.Find(a => a.no == skill3[i]);
            cs.sp[0] = s0.point;
            cs.sp[1] = s1.point;
            cs.sp[2] = s2.point;
            cs.sp[3] = s3.point;
            cs.sp[4] = 999;
            cs.manager = this;
            party.Add(cs);
        }
    }
    public void StatusRequestAnswer(bool master,int i)
    {
        List<CharaStatus> party = playerParty;
        if (!master) party = enemyParty;
        CharaStatus cs = party[i];
        punBattle.StatusAnswer(master, cs.nameC, cs.type1, cs.type2, cs.status, cs.item, cs.passiveName, cs.passiveText, cs.skill,cs.sp);
    }
    public void MemberReceive(bool master,int[] m)
    {
        List<CharaStatus> party = playerParty;
        if (!master) party = enemyParty;
        List<CharaStatus> member = playerMember;
        if (!master) member = enemyMember;

        for(int i = 0; i < 3; i++)
        {
            member.Add(party[m[i]]);
            party[m[i]].num = i;
        }
        if (playerMember.Count > 0 && enemyMember.Count > 0)
        {
            field.manager = this;
            //FirstSummon(true, 0);
            //FirstSummon(false, 0);
            StartCoroutine(BattleIE());
        }
    }
    public void Receive(string json)
    {
        Select select = Select.ToSelect(json);
        if (select.player)
        {
            if (playerSelect == null) playerSelect = select;
            else playerChange = select;
        }
        else
        {
            if (enemySelect == null) enemySelect = select;
            else enemyChange = select;
        }
    }
    public void SkillAnswer(bool master)
    {
        bool[] select = new bool[4] {true,true,true,true };
        CharaStatus battler;
        if (master) battler = battlerP;
        else battler = battlerE;
        if (battler.error2[9] > 0 || battler.buffer[4]> 0 || ItemName(battler.item).Contains("こだわり"))
        {
            if (battler.prevSkill != -1)
            {
                for (int i = 0; i < 4; i++)
                {
                    select[i] = false;
                }
                select[battler.prevSkill] = true;
            }
        }
        if (battler.error2[10] > 0 && 0 <= battler.prevSkill && battler.prevSkill <= 3)
        {
            select[battler.prevSkill] = false;
        }
        if (battler.error2[11]>0 || ItemName(battler.item)== "とつげきチョッキ")
        {
            for (int i = 0; i < 4; i++)
            {
                if (skillMaster.SkillList.Find(a => a.no == battler.skill[i]).physics == "変化") select[i] = false;
            }
        }
        if (battler.error2[13] > 0)
        {
            select[battler.lockSkill] = false;
        }
        for (int i = 0; i < 4; i++)
        {
            if(battler.sp[i]<=0) select[i] = false;
        }
        punBattle.SkillAnswer(master, battler.skill, battler.sp,select);
        if (battler.prevSkill != -1)
        {
            Skill prevSkill = skillMaster.SkillList.Find(a => a.no == battler.skill[battler.prevSkill]);
            if (prevSkill != null)
            {
                if (prevSkill.name == "こらえる" || prevSkill.name == "まもる" || prevSkill.name == "ニードルガード" || prevSkill.name == "みちづれ") select[battler.prevSkill] = false;
            }
        }
        switch(battler.passiveName)
        {
            case "公式怪文書":
                for (int i = 0; i < 4; i++)
                {
                    Skill skill = skillMaster.SkillList.Find(a => a.no == battler.skill[i]);
                    if (skill.type == "あく") select[i] = false;
                }
                break;
            case "清楚(にじさんじ)":
                for (int i = 0; i < 4; i++)
                {
                    Skill skill = skillMaster.SkillList.Find(a => a.no == battler.skill[i]);
                    if (skill.type == "あく") select[i] = false;
                }
                break;
        }
    }
    public void MemberAnswer(bool master,bool c)
    {
        List<CharaStatus> member = playerMember;
        if (!master) member = enemyMember;
        CharaStatus battle = battlerP;
        if (!master) battle = battlerE;

        string[] name = new string[3];
        string[] sex = new string[3];
        int[] error = new int[3];
        int[] errorTurn = new int[3];
        string[] hp = new string[3];
        bool[] item = new bool[3];
        bool[] change = new bool[3];

        for(int i = 0; i < 3; i++)
        {
            name[i] = member[i].nameC;
            sex[i] = member[i].sex;
            error[i] = member[i].error;
            errorTurn[i] = member[i].errorTurn;
            hp[i] = member[i].hp+"/"+member[i].status[0];
            item[i] = member[i].item != 0;
            if (!c) change[i] = member[i].error != 8 && member[i] != battle && battle.error2[2] == 0 && battle.error2[2] == 0 && battle.error2[12] == 0;
            else change[i] = member[i].error != 8 && member[i] != battle;
        }
        punBattle.MemberAnswer(master, name, sex, item, error,errorTurn, hp, change);
    }
}

public class CharaStatus
{
    public BattleManager manager;
    public CharaStatus enemy;
    public bool master;
    public int num;
    public Chara chara;
    public string nameC;
    public string sex;
    public string type1;
    public string type2;
    public int hp;
    public int[] status = new int[6];
    public int item;
    public string passiveName;
    public string passiveText;
    public string passiveNameDelete;
    public string passiveTextDelete;
    public string[] skill = new string[5];
    public int[] sp = new int[5];
    public int prevSkill = -1;
    public int successNum;
    public int passiveBool;
    public bool changeTurn;
    public bool firstAttack;
    public bool successPrev;
    public bool fastAttack;
    public int damageP;
    public int damageS;
    public int[] swap = new int[6];
    public int[] rank = new int[8];
    public string typeDefalut1;
    public string typeDefalut2;
    public int error = 0; //0なし 1どく 2どくどく 3まひ 4やけど 5こおり 6ちんもく 7ねむり 8ひんし
    public int errorTurn = 0;
    public int[] error2 = new int[14] ;//0こんらん 1ひるみ 2バインド 3のろい 4メロメロ 5やどりぎ 6ねむけ 7ほろび 8とくせいけし 9アンコール 10いちゃもん 11ちょうはつ 12にげられない 13かなしばり
    public int lockSkill = -1;
    public int[] buffer =new int[14];//0みがわり 1まもる 2まるく 3みちづれ 4あばれる 5じゅうでん 6たくわえる 7こらえる 8ためる 9さわぐ 10スタン 11ニードルガード 12はねやすめ 13れんぞく

    public string HPText()
    {
        return hp + "/" + status[0];
    }
    public float HPPer()
    {
        return ((float)hp) / ((float)status[0]);
    }
    public float RankStatus(int i)
    {
        float f;
        if (rank[i] >= 0)
        {
            f = status[i] * (rank[i] + 2) / 2;
        }
        else
        {
            f = status[i] *  2 / (-rank[i] + 2);
        }
        if (i == 5 && error == 3) f /= 2f;
        if (i == 1 && error == 4) f /= 2f;
        if (i == 3 && error == 6) f /= 2f;
        if (error2[4] > 0)
        {
            if (i == 1 || i == 3)f *= 0.75f;
        }
        if (i == 4 && manager.field.both[0] == 4 && (type1=="いわ"||type2=="いわ") ) f *= 1.5f;
        if (i == 2 && manager.field.both[0] == 3 && (type1 == "こおり" || type2 == "こおり")) f *= 1.5f;
        if (master)
        {
            if (i == 5 && manager.field.player[0] > 0) f *= 2;
            if (i == 2 && manager.field.player[4] > 0) f *= 2;
            if (i == 4 && manager.field.player[3] > 0) f *= 2;
            if (manager.field.player[2] > 0)
            {
                if (i == 2 || i == 4) f *= 1.5f;
            }
        }
        else
        {
            if (i == 5 && manager.field.enemy[0] > 0) f *= 2;
            if (i == 2 && manager.field.enemy[4] > 0) f *= 2;
            if (i == 4 && manager.field.enemy[3] > 0) f *= 2;
            if (manager.field.enemy[2] > 0)
            {
                if (i == 2 || i == 4) f *= 1.5f;
            }
        }
        if (i == 5 && manager.ItemName(item) == "こだわりスカーフ") f *= 1.5f;
        if (i == 4 && manager.ItemName(item) == "とつげきチョッキ") f *= 1.5f;
        return f;
    }
    public Summon ToSummon()
    {
        Summon summon = new Summon();
        summon.player = master;
        summon.name = nameC;
        summon.hpPer = HPPer();
        summon.hpText = HPText();
        summon.item = item != 0;
        summon.sex = sex;
        summon.rank = rank;
        summon.error = error;
        summon.errorTurn = errorTurn;
        summon.lockSkill = lockSkill;
        summon.error2 = error2;
        summon.buffer = buffer;
        summon.exception = "";
        return summon;
    }
    public Effect ToEffect()
    {
        Effect effect = new Effect();
        effect.player = master;
        effect.name = nameC;
        effect.rank = rank;
        effect.error = error;
        effect.error2 = error2;
        effect.errorTurn = errorTurn;
        effect.lockSkill = lockSkill;
        effect.buffer = buffer;
        effect.item = item!=0;
        return effect;
    }
    public IEnumerator GameFirst()
    {
        switch (passiveName)
        {
            case "あやまらないよ":
                status[4] = Mathf.FloorToInt(status[4]*1.5f);
                break;
            case "私も入れてよ":
                bool b = false;
                for(int i = 0; i < 3; i++)
                {
                    if (master)
                    {
                        if (manager.playerMember[i] == this)
                        {
                            b = true;
                            break;
                        }
                    }
                    else
                    {
                        if (manager.enemyMember[i] == this)
                        {
                            b = true;
                            break;
                        }
                    }
                }
                if (!b)
                {
                    yield return manager.AreaIE("「私も入れてよ」");
                }
                break;
        }
    }
    public void ChangeEffect()
    {
        type1 = typeDefalut1;
        type2 = typeDefalut2;
        prevSkill = -1;
        successNum = 0;
        successPrev = false;
        damageP = 0;
        damageS = 0;
        rank = new int[8];
        error2 = new int[14];
        lockSkill = -1;
        bool ball = buffer[2] > 0;
        buffer = new int[14];
        if (ball) buffer[2] = 1;
        if (error == 2) errorTurn = 0;
        for (int i = 1; i < 6; i++)
        {
            if (swap[i] != 0)
            {
                status[i] = swap[i];
                swap[i] = 0;
            }
        }
        switch(passiveName)
        {
            case "お茶を飲みまぁす":
                if (error != 8)
                {
                    hp += Mathf.FloorToInt(status[0] / 6f);
                    if (hp > status[0]) hp = status[0];
                }
                break;
            case "隠居":
                passiveBool = 0;
                break;
        }
        if (passiveName == "")
        {
            passiveName = passiveNameDelete;
            passiveText = passiveTextDelete;
            passiveNameDelete = "";
            passiveTextDelete = "";
        }
    }
    public IEnumerator TurnEndAll()
    {
        if (error == 8) yield break;
        bool battle = this == manager.battlerE || this == manager.battlerP;
        if (error == 5)
        {
            errorTurn -= 1;
            if (errorTurn == 0)
            {
                error = 0;
                if(battle) yield return manager.EffectIE(enemy, this, nameC + "は　こおりが溶けた!");
            }
        }
        switch (passiveName)
        {
            case "古の腐女子":
                if(manager.battlerE.sex=="男性"&& manager.battlerP.sex == "男性")
                {
                    hp += Mathf.FloorToInt(status[0] / 10f);
                    if (hp > status[0]) hp = status[0];
                    error = 0;
                    errorTurn = 0;
                }
                break;
            case "積分":
                if (manager.turn <= 10)
                {
                    status[3] += 10;
                    if (swap[3] != 0) swap[3] += 10;
                }
                break;
            case "晩酌":
                if (manager.turn%3==0)
                {
                    if (battle)
                    {
                        yield return manager.DamageIE(enemy, this, manager.PassiveText(this) + "HPを　回復した", -Mathf.FloorToInt(status[0] / 10f));
                    }
                    else
                    {
                        hp += Mathf.FloorToInt(status[0] / 10f);
                        if (hp > status[0]) hp = status[0];
                    }
                }
                break;
        }
    }
    public IEnumerator ErrorCheak(int i)
    {
        Effect effect;
        if (error == 0)
        {
            switch (passiveName)
            {
                case "カンカンカンカン！！":
                    if (i == 7)
                    {
                        manager.Message(manager.PassiveText(this) + "ねむり状態にならない！");
                        yield break;
                    }
                    break;
                case "ようこそデラスへ":
                    if (i == 7)
                    {
                        manager.Message(manager.PassiveText(this) + "ねむり状態にならない！");
                        yield break;
                    }
                    break;
            }

            if ((master && manager.field.player[6] > 0) || (!master && manager.field.enemy[6] > 0))
            {
                manager.Message("しろいきりが状態異常を防いだ！");
                yield break;
            }
            if (i == 7 && (buffer[9] > 0 || enemy.buffer[9] > 0))
            {
                effect = ToEffect();
                effect.message = "さわぐ音でねむることができない！";
                manager.punBattle.Send("effect", effect.ToString());
                yield break;
            }
            error = i;
            if (error == 2) errorTurn = 0;
            if (error == 5) errorTurn = 4;
            if (error == 7)
            {
                errorTurn = 3;
                if(enemy.passiveName == "ようこそデラスへ") 
                {

                    manager.Message(manager.PassiveText(this) + "ねむりの効果を強める！");
                    errorTurn = 4;
                }
            }
            string errorName = "";
            if (error == 1) errorName = "どく";
            if (error == 2) errorName = "もうどく";
            if (error == 3) errorName = "まひ";
            if (error == 4) errorName = "やけど";
            if (error == 5) errorName = "こおり";
            if (error == 6) errorName = "ちんもく";
            if (error == 7) errorName = "ねむり";

            yield return manager.EffectIE(enemy, this,nameC+"は　"+errorName+"になった",errorName);
        }
        else
        {
            manager.Message(nameC+"は　既に状態異常だ");
        }
    }
    public IEnumerator StatusCheak(int i,int add)
    {
        string text = "";
        int prev = rank[i];

        if (i == 1) text = "こうげき";
        if (i == 2) text = "ぼうぎょ";
        if (i == 3) text = "とくこう";
        if (i == 4) text = "とくぼう";
        if (i == 5) text = "すばやさ";
        if (i == 6) text = "めいちゅう";
        if (i == 7) text = "かいひ";

        if (add > 0)
        {
            if (rank[i] == 6)
            {
                yield return manager.EffectIE(enemy, this, nameC + "の " + text + "はもう上がらない");
                yield break;
            }
            
        }
        if (add < 0)
        {
            if ((master && manager.field.player[6] > 0) || (!master && manager.field.enemy[6] > 0))
            {
                manager.Message("しろいきりが能力低下を防いだ！");
                yield break;
            }
            if (rank[i] == -6)
            {
                yield return manager.EffectIE(enemy, this, nameC + "の " + text + "はもう下がらない");
                yield break;
            }
        }
        rank[i] += add;
        if (rank[i] > 6) rank[i] = 6;
        if (rank[i] < -6) rank[i] = -6;

        if (add > 0) yield return manager.EffectIE(enemy, this, nameC + "の " + text + "が" + (rank[i]-prev) + "段階上がった！","up");
        if (add < 0) yield return manager.EffectIE(enemy, this, nameC + "の " + text + "が" + (prev-rank[i]) + "段階下がった","down");
    } 
    public IEnumerator TurnEndEffect()
    {
        damageP = 0;
        damageS = 0;
        if (buffer[7] > 0)
        {
            buffer[7] = 0;
        }
        //0なし 1どく 2どくどく 3まひ 4やけど 5こおり 6ちんもく 7ねむり 8ひんし
        if (error == 1)
        {
            int damage =  Mathf.FloorToInt(status[0] / 8f);
            string text = nameC + "は　どくでダメージを受けた";
            yield return manager.DamageIE( null, this, text, damage);
        }
        if (error == 2)
        {
            errorTurn += 1;
            int damage = Mathf.FloorToInt(errorTurn * status[0] / 16f);
            string text = nameC + "は　もうどくでダメージを受けた";
            yield return manager.DamageIE( null, this, text, damage);
        }
        if (error == 4)
        {
            int damage = Mathf.FloorToInt(status[0] / 16f);
            string text = nameC + "は　やけどでダメージを受けた";
            yield return manager.DamageIE( null, this, text, damage);
        }
        //0こんらん 1ひるみ 2バインド 3のろい 4メロメロ 5やどりぎ 6ねむけ 7ほろび 8とくせいけし 9アンコール 10いちゃもん 11ちょうはつ 12にげられない
        if (error2[1] > 0)
        {
            error2[1] = 0;
        }
        if (error2[2] > 0)
        {
            error2[2] -= 1;
            int damage = Mathf.FloorToInt(status[0] / 16f);
            string text = nameC + "は　拘束でダメージを受けた";
            yield return manager.DamageIE( null, this, text, damage);
        }
        if(error2[3] > 0)
        {
            int damage = Mathf.FloorToInt(status[0] / 4f);
            string text = nameC + "は　のろいでダメージを受けた";
            yield return manager.DamageIE( null, this, text, damage);
        }
        if (error2[3] > 0)
        {
            int damage = Mathf.FloorToInt(status[0] / 4f);
            string text = nameC + "は　のろいでダメージを受けた";
            yield return manager.DamageIE( null, this, text, damage);
        }
        if (error2[3] > 0)
        {
            int damage = Mathf.FloorToInt(status[0] / 4f);
            string text = nameC + "は　のろいでダメージを受けた";
            yield return manager.DamageIE( null, this, text, damage);
        }
        if (error2[5] > 0)
        {
            int damage = Mathf.FloorToInt(status[0] / 8f);
            if (hp < damage) damage = hp;
            string text = nameC + "は　やどりぎのたねに力を吸われた";
            yield return manager.DamageIE( null, this, text, damage);

            text = enemy.nameC + "は　やどりぎのたねで回復した";
            yield return manager.DamageIE( null, manager.battlerE, text, -damage);

        }
        if (error2[6] > 0)
        {
            error2[6] -= 1;
            yield return manager.EffectIE(enemy, this, "");
            if (error2[6] == 0 && error==0) {
                yield return ErrorCheak(7);
            }
            
        }
        if (error2[7] > 0)
        {
            error2[7] -= 1;
            yield return manager.EffectIE(enemy, this, nameC + "は　ほろびのカウントが進む");
            if (error2[7] == 0)
            {
                int damage = 9999;
                string text = nameC + "は　ほろびのカウントが0になった！";
                yield return manager.DamageIE( enemy, this, text, damage);
            }
        }
        if (error2[9] > 0)
        {
            error2[9] -= 1;
            yield return manager.EffectIE(enemy, this, "");
            if (error2[9] == 0)
            {
                yield return manager.EffectIE(enemy, this, nameC + "は　アンコールが解けた！");
            }
        }
        if (error2[11] > 0)
        {
            error2[11] -= 1;
            yield return manager.EffectIE(enemy, this, "");
            if (error2[11] == 0)
            {
                yield return manager.EffectIE(enemy, this, nameC + "は　挑発が解けた！");
            }
        }
        if (error2[13] > 0)
        {
            error2[13] -= 1;
            yield return manager.EffectIE(enemy, this, "");
            if (error2[13] == 0)
            {
                yield return manager.EffectIE(enemy, this, nameC + "は　金縛りから解放された！");
            }
        }
        buffer[1] = 0;
        buffer[11] = 0;
        buffer[12] = 0;
        yield return manager.EffectIE(enemy, this, "");
        if (buffer[4] > 0)
        {
            buffer[4] -= 1;
            yield return manager.EffectIE(enemy, this, "");
            if (buffer[4] == 0)
            {
                yield return manager.EffectIE(enemy, this, nameC + "は　バーサク状態から回復した！");
            }
        }
        if (buffer[9] > 0)
        {
            buffer[9] -= 1;
            yield return manager.EffectIE(enemy, this, "");
            if (buffer[9] == 0)
            {
                yield return manager.EffectIE(enemy, this, nameC + "は　さわぐのをやめた");
            }
        }
        switch (passiveName)
        {
            case "カンカンカンカン！！":
                List<CharaStatus> csList;
                if (master) csList = manager.playerMember;
                else csList = manager.enemyMember;
                foreach (CharaStatus cs in csList)
                {
                    if (cs.error == 7)
                    {
                        cs.errorTurn -= 1;
                        if (cs.errorTurn == 0)
                        {
                            cs.error = 0;
                            manager.Message(manager.PassiveText(this) + "仲間をねむりから起こした！");
                        }
                        else
                        {
                            manager.Message(manager.PassiveText(this) + "仲間のねむりを短くした！");
                        }
                    }
                }
                break;
            case "隠居":
                if (passiveBool == 1 && error!=8){
                    yield return manager.AreaIE(manager.PassiveText(this) + "手持ちへ戻ろうとする！");
                    bool b = false ;
                    yield return manager.EffectChange(master, r => b = r);
                    if (!b) manager.Message("しかしうまくいかなかった");
                }
                break;
            case "トラップタワー":
                if (error != 8 && item == 0)
                {
                    int i = UnityEngine.Random.Range(100,107);
                    item = i;
                    yield return manager.EffectIE(enemy,this,manager.PassiveText(this) +  manager.ItemName(item) + "を　収穫した");
                }
                break;
        }
        switch (manager.ItemName(item))
        {
            case "たべのこし":
                int damage = -Mathf.FloorToInt(status[0] / 16f);
                string text = manager.ItemText(this) + nameC + "は　回復した";
                yield return manager.DamageIE(null, this, text, damage);
                break;
            case "しろいハーブ":
                bool b = false;
                for (int j = 0; j < 8; j++)
                {
                    if (rank[j] < 0)
                    {
                        rank[j] = 0;
                        b = true;
                    }
                }
                if (b)
                {
                    yield return manager.EffectIE(enemy, this, manager.ItemText(this) + "下がった能力を元に戻した");
                    yield return manager.ItemUse(this);
                }
                break;
        }
    }
    public IEnumerator TurnEndAfter()
    {
        if (error2[12] > 0)
        {
            error2[12] -= 1;
            yield return manager.EffectIE(enemy, this, "");
            if (error2[12] == 0)
            {
                yield return manager.EffectIE(enemy, this, nameC + "は　交代できるようになった！");
            }
        }
    }
    public void PassiveChange(string name,string text)
    {
        passiveName = name;
        passiveText = text;
        passiveBool = 0;
    }
}

public class Field
{
    public BattleManager manager;
    public int[] both = new int[6];//0天気(0普通1日照り2雨3あられ4すなあらし) 1天気ターン 2トリル 3重力 4あまいかおり
    public int[] player = new int[13];//0おいかぜ 1ねがいごと 2オーロラ 3こころのかべ 4リフレクター 5アクアリング 6しろいきり <> 10ステロ 11どくびし 12ねばねば
    public int[] enemy = new int[13];
    public Area ToArea()
    {
        Area area = new Area();
        area.both = both;
        area.player = player;
        area.enemy = enemy;
        return area;
    }
    public IEnumerator TurnEndEffect()
    {
        if (both[0] != 0)
        {
            both[1] -= 1;
            if (both[0] == 3)
            {
                bool type1 = manager.battlerP.type1 == "こおり";
                bool type2 = manager.battlerP.type1 == "こおり";
                if (type1 || type2)
                {
                    int damage = Mathf.FloorToInt(manager.battlerP.status[0] / 16f);
                    string text = manager.battlerP.nameC + "は　あられでダメージを受けた";
                    yield return manager.DamageIE( manager.battlerE, manager.battlerP, text, damage);
                }
                type1 = manager.battlerE.type1 == "こおり";
                type2 = manager.battlerE.type1 == "こおり";
                if (type1 || type2)
                {
                    int damage = Mathf.FloorToInt(manager.battlerE.status[0] / 16f);
                    string text = manager.battlerE.nameC + "は　あられでダメージを受けた";
                    yield return manager.DamageIE( manager.battlerP, manager.battlerE, text, damage);
                }
            }
            if (both[0] == 4)
            {
                bool type1 = manager.battlerP.type1 == "いわ" || manager.battlerP.type1 == "はがね" || manager.battlerP.type1 == "じめん";
                bool type2 = manager.battlerP.type1 == "いわ" || manager.battlerP.type1 == "はがね" || manager.battlerP.type1 == "じめん";
                if (type1 || type2)
                {
                    int damage = Mathf.FloorToInt(manager.battlerP.status[0] / 16f);
                    string text = manager.battlerP.nameC + "は　すなあらしでダメージを受けた";
                    yield return manager.DamageIE( manager.battlerE, manager.battlerP, text, damage);
                }
                type1 = manager.battlerP.type1 == "いわ" || manager.battlerP.type1 == "はがね" || manager.battlerP.type1 == "じめん";
                type2 = manager.battlerP.type1 == "いわ" || manager.battlerP.type1 == "はがね" || manager.battlerP.type1 == "じめん";
                if (type1 || type2)
                {
                    int damage = Mathf.FloorToInt(manager.battlerE.status[0] / 16f);
                    string text = manager.battlerE.nameC + "は　すなあらしでダメージを受けた";
                    yield return manager.DamageIE( manager.battlerP, manager.battlerE, text, damage);
                }
            }
            if (both[1] == 0)
            {
                string text = "";
                if (both[0] == 1) text = "日照りが止んだ";
                if (both[0] == 2) text = "雨が止んだ";
                if (both[0] == 3) text = "あられが止んだ";
                if (both[0] == 4) text = "すなあらしが止んだ";
                both[0] = 0;
                yield return manager.AreaIE(text);
            }
        }
        if (both[2] > 0)
        {
            both[2] -= 1;
            if (both[2] == 0) yield return manager.AreaIE("エリアの歪みが消えた");
        }
        if (both[3] > 0)
        {
            both[3] -= 1;
            if (both[3] == 0) yield return manager.AreaIE("重力が元に戻った");
        }
        if (both[4] > 0)
        {
            both[4] -= 1;
            if (both[4] == 0) yield return manager.AreaIE("あまいかおりが消えた");
        }
        yield return new WaitForSeconds(0.1f);

        //0おいかぜ 1ねがいごと 2オーロラ 3こころのかべ 4リフレクター 5アクアリング 6しろいきり <> 10ステロ 11どくびし 12ねばねば

        if (player[0] > 0)
        {
            player[0] -= 1;
            if(player[0] == 0)
            {
                yield return manager.AreaIE("おいかぜがやんだ");
            }
        }
        if (enemy[0] > 0)
        {
            enemy[0] -= 1;
            if (enemy[0] == 0)
            {
                yield return manager.AreaIE("おいかぜがやんだ");
            }
        }
        if (player[1] > 0)
        {
            player[1] -= 1;
            if (player[1] == 0)
            {
                int damage = Mathf.FloorToInt(manager.battlerP.status[0] / 2f);
                string text = manager.battlerP.nameC + "は　願い事が叶って元気になった！";
                yield return manager.DamageIE( manager.battlerE, manager.battlerP, text, damage);
            }
        }
        if (enemy[1] > 0)
        {
            enemy[1] -= 1;
            if (enemy[1] == 0)
            {
                int damage = Mathf.FloorToInt(manager.battlerE.status[0] / 2f);
                string text = manager.battlerE.nameC + "は　願い事が叶って元気になった！";
                yield return manager.DamageIE( manager.battlerP, manager.battlerE, text, damage);
            }
        }
        if (player[3] > 0)
        {
            player[3] -= 1;
            if (player[3] == 0) yield return manager.AreaIE("こころのかべが消えた");
        }
        if (enemy[3] > 0)
        {
            enemy[3] -= 1;
            if (enemy[3] == 0) yield return manager.AreaIE("こころのかべが消えた");
        }
        if (player[4] > 0)
        {
            player[4] -= 1;
            if (player[4] == 0) yield return manager.AreaIE("リフレクターが消えた");
        }
        if (enemy[4] > 0)
        {
            enemy[4] -= 1;
            if (enemy[4] == 0) yield return manager.AreaIE("リフレクターが消えた");
        }
        if (player[5] > 0)
        {
            player[5] -= 1;
            int damage = Mathf.FloorToInt(-manager.battlerP.status[0] / 16f);
            string text = manager.battlerP.nameC + "は　アクアリングに癒された";
            yield return manager.DamageIE( manager.battlerE, manager.battlerP, text, damage);
            if (player[5] == 0) yield return manager.AreaIE("アクアリングが消えた");
        }
        if (enemy[5] > 0)
        {
            enemy[5] -= 1;
            int damage = Mathf.FloorToInt(-manager.battlerE.status[0] / 16f);
            string text = manager.battlerE.nameC + "は　アクアリングに癒された";
            yield return manager.DamageIE( manager.battlerP, manager.battlerE, text, damage);
            if (enemy[5] == 0) yield return manager.AreaIE("アクアリングが消えた");
        }
        if (player[8] > 0)
        {
            player[8] -= 1;
            if (player[8] == 0)
            {
                int damage = Mathf.FloorToInt(player[9] / manager.battlerP.RankStatus(4));
                yield return manager.DamageIE( manager.battlerE, manager.battlerP, manager.battlerP.nameC + "は　みらいよちのダメージを受けた",damage);
            }
        }
        if (enemy[8] > 0)
        {
            enemy[8] -= 1;
            if (enemy[8] == 0)
            {
                int damage = Mathf.FloorToInt(enemy[9] / manager.battlerE.RankStatus(4));
                yield return manager.DamageIE( manager.battlerP, manager.battlerE, manager.battlerE.nameC + "は　みらいよちのダメージを受けた", damage);
            }
        }
        yield return manager.AreaIE("");
    }
    public IEnumerator TurnEndAfter()
    {
        if (both[5] > 0)
        {
            both[5] -= 1;
            if (both[5] == 0) yield return manager.AreaIE("異界の扉が閉じられた");
        }
    }
}