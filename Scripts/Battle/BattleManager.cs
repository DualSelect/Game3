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
            //�I����v��
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
            //�Z�ƌ��
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
                if (playerSkill.name == "��������")
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
                if (enemySkill.name == "��������")
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
            //�^�[���I����
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
        if (ItemName(player.item) == "���񂹂��̃c��")
        {
            if (PerCorrect(player.master, 25))
            {
                Message(ItemText(player)  + "�s���������Ȃ����I");
                playerSpeed = 999;
                if (field.both[2] > 0) playerSpeed = 0;
            }
        }
        if (ItemName(enemy.item) == "���񂹂��̃c��")
        {
            if (PerCorrect(enemy.master, 25))
            {
                Message(ItemText(enemy) + "�s���������Ȃ����I");
                enemySpeed = 999;
                if (field.both[2] > 0) enemySpeed = 0;
            }
        }
        if (ItemName(player.item) == "�C�o���̂�" && player.HPPer()<=0.25f)
        {
            Message(ItemText(player) + "�s���������Ȃ����I");
            yield return ItemUse(player);
            playerSpeed = 999;
            if (field.both[2] > 0) playerSpeed = 0;
        }
        if (ItemName(enemy.item) == "�C�o���̂�" && enemy.HPPer() <= 0.25f)
        {
            Message(ItemText(enemy) + "�s���������Ȃ����I");
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

        if (player.passiveName == "�������炲����" && playerSkillSpeed == 0 && playerSkill.physics == "�ω�")
        {
            playerSkillSpeed = 1;
            Message(PassiveText(player)+"�ω��Z�̗D��x��1�ɂ���");
        }
        if (enemy.passiveName == "�������炲����" && enemySkillSpeed == 0 && enemySkill.physics == "�ω�")
        {
            enemySkillSpeed = 1;
            Message(PassiveText(enemy) + "�ω��Z�̗D��x��1�ɂ���");
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
        if (skill.no!="0" && atkChara.prevSkill!=-1 && (atkChara.error2[9] > 0 || ItemName(atkChara.item).Contains("�������"))) skill = skillMaster.SkillList.Find(a => a.no == atkChara.skill[atkChara.prevSkill]).Copy();
        if (atkChara.buffer[3] > 0)
        {
            atkChara.buffer[3] = 0;
            yield return EffectIE(defChara, atkChara, "");
        }
        if (skill.name == "�˂����Ƃ��т�") if (atkChara.RankStatus(3) > atkChara.RankStatus(1))  skill = skillMaster.SkillList.Find(a => a.no == "no-55").Copy();
        if (skill.name == "�͂�����") if (atkChara.RankStatus(3) > atkChara.RankStatus(1))  skill = skillMaster.SkillList.Find(a => a.no == "no-60").Copy();
        bool yugamin = defChara.buffer[0] > 0;

        //��������
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
        //�Z�̌���
        if (hitCheck)
        {
            if (selectNum != atkChara.prevSkill) atkChara.successNum = 0;
            yield return SkillAttack(skill, atkChara, defChara,selectNum);
            atkChara.successNum += 1;
            //�U�������
            if (skill.physics=="�ω�" || !yugamin || skill.sound>0 || skill.target== "����" || skill.target == "��") yield return SkillEffect(skill, atkChara, defChara);
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
            effect.message = atkChara.nameC + "�́@�����œ����Ȃ�";
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
                effect.message = atkChara.nameC + "�́@Ⴢ�ē����Ȃ�";
                punBattle.Send("effect", effect.ToString());
                hitCheck = false; yield break;
            }
        }
        if (atkChara.error == 5)
        {
            if (skill.name == "�����Ȃ�ق̂�" && skill.name == "�t���A�h���C�u")
            {
                atkChara.error = 0;
                atkChara.errorTurn = 0;
                Effect effect = atkChara.ToEffect();
                effect.message = atkChara.nameC + "�́@�X���������I";
                punBattle.Send("effect", effect.ToString());
            }
            else
            {
                Effect effect = atkChara.ToEffect();
                effect.message = atkChara.nameC + "�́@�����Ă��ē����Ȃ�";
                punBattle.Send("effect", effect.ToString());
                hitCheck = false; yield break;
            }
        }
        if (atkChara.error == 6 && skill.physics == "�ω�")
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
                effect.message = atkChara.nameC + "�́@���ق��ċZ���g���Ȃ�";
                punBattle.Send("effect", effect.ToString());
                hitCheck = false; yield break;
            }
        }
        if (atkChara.error == 7)
        {
            atkChara.errorTurn -= 1;
            Effect effect = atkChara.ToEffect();
            effect.message = atkChara.nameC + "�́@�����Ă���";
            punBattle.Send("effect", effect.ToString());

            if (atkChara.errorTurn == 0)
            {
                atkChara.error = 0;
                effect = atkChara.ToEffect();
                effect.message = atkChara.nameC + "�́@�ڂ��o�܂����I";
                punBattle.Send("effect", effect.ToString());
            }
            else if (skill.name != "�˂����Ƃ��т�" && skill.name != "�˂���")
            {
                hitCheck = false; yield break;
            }
        }

        if (atkChara.error2[1] > 0)
        {
            atkChara.error2[1] = 0;
            Effect effect = atkChara.ToEffect();
            effect.message = atkChara.nameC + "�́@����œ����Ȃ�";
            punBattle.Send("effect", effect.ToString());
            hitCheck = false; yield break;
        }
        if (atkChara.error2[0] > 0)
        {
            Effect effect = atkChara.ToEffect();
            effect.message = atkChara.nameC + "�́@�������Ă���";
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
                yield return DamageIE(null, atkChara, atkChara.nameC + "�́@��������炸�������U������", damageI);

                atkChara.error2[0] -= 1;
                effect = atkChara.ToEffect();
                if (atkChara.error2[0] == 0) effect.message = atkChara.nameC + "�́@�������������I";
                punBattle.Send("effect", effect.ToString());
                hitCheck = false; yield break;
            }
        }
        if (atkChara.sp[selectNum] <= 0)
        {
            Message(atkChara.nameC + "�́@SP�����肸�Z���o���Ȃ��I");
            hitCheck = false; yield break;
        }

        if (atkChara.error2[10] > 0)
        {
            if (selectNum == atkChara.prevSkill)
            {
                Message(atkChara.nameC + "�́@���������������ē����Z���o���Ȃ��I");
                hitCheck = false; yield break;
            }
        }
        if (atkChara.error2[11] > 0)
        {
            if (skill.physics=="�ω�")
            {
                Message(atkChara.nameC + "�́@���傤�͂���Ă��ĕω��Z���o���Ȃ��I");
                hitCheck = false; yield break;
            }
        }
        if (atkChara.error2[13] > 0)
        {
            if (selectNum == atkChara.lockSkill)
            {
                Message(atkChara.nameC + "�́@���Ȃ��΂�ŋZ���o���Ȃ��I");
                hitCheck = false; yield break;
            }
        }
        if (skill.name == "�������p���`" && (atkChara.damageP + atkChara.damageS) > 0)
        {
            Message(atkChara.nameC + "�́@�W�����r�؂�ċZ���o���Ȃ�");
            hitCheck = false; yield break;
        }
        string text = atkChara.nameC + "�� " + skill.name + "!";
        if (skill.name == "�˂���")
        {
            while (true)
            {
                Skill negoto = skillMaster.SkillList.Find(a => a.no == atkChara.skill[UnityEngine.Random.Range(0, 4)]);
                if (negoto == null) continue;
                if (negoto.name != "�˂���")
                {
                    skill = negoto;
                    callback(skill);
                    text = atkChara.nameC + "�� " + "�˂���" + "!";
                    text += "\n" + skill.name + "���ł��I";
                    Message(text);
                    text = atkChara.nameC + "�� " + skill.name + "!";
                    break;
                }
            }
        }
        if (skill.name == "�Z�Z�[�W�A")
        {
            skill.type = TypeUtil.Type.NumToType(UnityEngine.Random.Range(0, 18));
            skill.name = skill.type + "�[�W�A";
            callback(skill);
            text = atkChara.nameC + "�� " + "�Z�Z�[�W�A" + "!";
            text += "\n" + skill.type + "�[�W�A�@���ł��I";
            Message(text);
            text = atkChara.nameC + "�� " + skill.name + "!";
        }
        if ((!atkChara.fastAttack || playerSkill.physics == "�ω�" || enemySkill.physics == "�ω�") && skill.name == "�ӂ�����")
        {
            text += "\n" + "���������܂������Ȃ�����";
            Message(text);
            hitCheck = false; yield break;
        }
        if (!atkChara.firstAttack && (skill.name == "�������o�g��" || skill.name == "�ł���������" || skill.name == "�˂����܂�"))
        {
            text += "\n" + "���������܂������Ȃ�����";
            Message(text);
            hitCheck = false; yield break;
        }
        if (defChara.error != 7 && skill.name == "��߂���")
        {
            text += "\n" + "���������܂������Ȃ�����";
            Message(text);
            hitCheck = false; yield break;
        }
        if (atkChara.error != 7 && (skill.name == "�˂����Ƃ��т�" || skill.name == "�˂���"))
        {
            text += "\n" + "���������܂������Ȃ�����";
            Message(text);
            hitCheck = false; yield break;
        }
        if (defChara.error == 8)
        {
            text += "\n" + "�Ώۂ����Ȃ��悤��...";
            Message(text);
            hitCheck = false; yield break;
        }

        if (atkChara.prevSkill != -1 && atkChara.successNum > 0)
        {
            Skill prevSkill = skillMaster.SkillList.Find(a => a.no == atkChara.skill[atkChara.prevSkill]);
            if (prevSkill.name == "���炦��" || prevSkill.name == "�܂���" || prevSkill.name == "�j�[�h���K�[�h" || prevSkill.name == "�݂��Â�")
            {
                if (prevSkill.name == skill.name)
                {
                    text += "\n" + "�A�����Ďg�p���邱�Ƃ��ł��Ȃ�";
                    Message(text);
                    hitCheck = false; yield break;
                }
            }
        }

        atkChara.prevSkill = selectNum;
        prevSkillNo = skill.no;
        atkChara.sp[selectNum] -= 1;
        if(atkChara.passiveName=="���̃|��") atkChara.sp[selectNum] -= 1;

        switch (atkChara.passiveName)
        {
            case "����������":
                if (skill.type == "����")
                {
                    Message(text);
                    Message(PassiveText(atkChara)+"�����^�C�v�̋Z���o���Ȃ��I");
                    hitCheck = false; yield break;
                }
                break;
            case "���^(�ɂ�����)":
                if (skill.type == "����")
                {
                    Message(text);
                    Message(PassiveText(atkChara) + "�����^�C�v�̋Z���o���Ȃ��I");
                    hitCheck = false; yield break;
                }
                break;
        }
        switch (defChara.passiveName)
        {
            case "���S����":
                if (skill.sound >0)
                {
                    Message(text);
                    Message(PassiveText(defChara) + "���Ƃ̋Z�𖳌��������I");
                    yield return defChara.StatusCheak(5,1);
                    hitCheck = false; yield break;
                }
                break;
            case "�Z���u����":
                if (skill.type == "�݂�" && skill.target != "����" && skill.target != "��")
                {
                    Message(text);
                    Message(PassiveText(defChara) + "�݂��^�C�v�̋Z�𖳌��������I");
                    yield return defChara.StatusCheak(1, 1);
                    yield return defChara.StatusCheak(3, 1);
                    if (defChara.error2[0] == 0)
                    {
                        defChara.error2[0] = 2;
                        yield return EffectIE(atkChara, defChara, defChara.nameC + "�́@���������I");
                    }
                    else yield return EffectIE(atkChara, defChara, defChara.nameC + "�́@���łɍ������Ă���");
                    hitCheck = false; yield break;
                }
                break;
        }

 

        if (defChara.buffer[1] > 0 && skill.name != "�t�F�C���g" && skill.target != "����" && skill.target != "��")
        {
            defChara.buffer[1] = 0;
            Effect effect = defChara.ToEffect();
            effect.message = atkChara.nameC + "�́@" + skill.name + "�I";
            effect.message += "\n" + defChara.nameC + "�́@�U������g�������";
            punBattle.Send("effect", effect.ToString());
            hitCheck = false; yield break;
        }
        if (defChara.buffer[11] > 0 && skill.name != "�t�F�C���g" && skill.target != "����" && skill.target != "��" && skill.physics!="�ω�")
        {
            defChara.buffer[11] = 0;
            Effect effect = defChara.ToEffect();
            effect.message = atkChara.nameC + "�́@" + skill.name + "�I";
            effect.message += "\n" + defChara.nameC + "�́@�U������g�������";
            punBattle.Send("effect", effect.ToString());
            if (skill.contact > 0)
            {
                yield return DamageIE( defChara, atkChara, atkChara + "�́@�Ƃ��ɂӂ�ă_���[�W���󂯂�", Mathf.FloorToInt(atkChara.status[0] / 8f));
            }
            hitCheck = false; yield break;
        }
        if (skill.name == "�̃h����" || skill.name == "�A�S�h����" || skill.name == "���������ꂢ��" || skill.name == "�����" || skill.name == "������")
        {
            if(30 > Random.Range(0, 100)){
                hitCheck = true; yield break;
            }
            else
            {
                text += "\n" + "�������@�U���́@�O�ꂽ";
                Message(text);
                hitCheck = false; yield break;
            }
        }

        if (skill.physics == "�ω�")
        {
            if(defChara.buffer[0]>0 && skill.sound==0 && skill.through == 0)
            {
                text += "\n" + "�������@�݂����ɖh���ꂽ";
                Message(text);
                hitCheck = false; yield break;
            }
        }
        else
        {
            int typePer = TypePer(skill,atkChara,defChara);
            if (typePer == 0)
            {
                text += "\n" + "�������͂Ȃ��悤��...";
                Message(text);
                hitCheck = false; yield break;
            }
        }
        if(skill.name == "���P�b�g����" || skill.name == "���e�I�r�[��")
        {
            if (atkChara.passiveName == "������")
            {
                atkChara.buffer[8] = 1;
                Message(PassiveText(atkChara) + "���ߋZ�������ɔ�������I");
            }
            if (atkChara.buffer[8] == 0) hitCheck = true; yield break;
        }
        if (skill.name == "�\�[���[�r�[��" || skill.name == "�\�[���[�u���[�h")
        {
            if (atkChara.passiveName == "������")
            {
                atkChara.buffer[8] = 1;
                Message(PassiveText(atkChara) + "���ߋZ�������ɔ�������I");
            }
            if (field.both[0] == 1) atkChara.buffer[8] = 1;
            if (atkChara.buffer[8] == 0) hitCheck = true; yield break;
        }
        float hit = skill.hit;
        if (skill.name == "���݂Ȃ�" && field.both[0] == 2) hit = 0;
        if (skill.name == "�ڂ��ӂ�" && field.both[0] == 2) hit = 0;
        if (skill.name == "�ӂԂ�" && field.both[0] == 3) hit = 0;
        if (skill.name == "�ǂ��ǂ�" && (atkChara.type1=="�ǂ�"|| atkChara.type2 == "�ǂ�")) hit = 0;
        if (atkChara.passiveName=="���A�c"&& turn % 10 == 7)
        {
            Message(PassiveText(atkChara) + "���̃^�[���͌��A�c���I");
            hit = 0;
        }
        if (atkChara.passiveName == "�������s")hit *= 0.9f;
        if (defChara.passiveName == "�L����") hit = 0;
        if (ItemName(atkChara.item) == "�������������Y") hit *= 1.15f;
        if (ItemName(defChara.item) == "�Ђ���̂���") hit *= 0.9f;

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
                text += "\n" + "�������@�U���́@�O�ꂽ";
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
            case "�E�F�U�[�{�[��":
                if (field.both[0] == 1) type = "�ق̂�";
                if (field.both[0] == 2) type = "�݂�";
                if (field.both[0] == 3) type = "����";
                if (field.both[0] == 4) type = "������";
                break;
        }
        return type;
    }
    int TypePer(Skill skill, CharaStatus atkChara,CharaStatus defChara)
    {
        int typePer;
        typePer = Type.Per(AtkType(skill, atkChara), defChara.type1, defChara.type2);
        if (skill.name == "�t���[�Y�h���C") typePer = Type.PerSkill(0, defChara.type1, defChara.type2);
        if (skill.name == "�ӂ��傭����") typePer = Type.PerSkill(1, defChara.type1, defChara.type2);
        if (skill.name == "����̂͂ǂ�") typePer = Type.PerSkill(2, defChara.type1, defChara.type2);
        if (skill.name == "�悤�̂͂ǂ�") typePer = Type.PerSkill(3, defChara.type1, defChara.type2);
        if (skill.type == "���߂�" && defChara.buffer[12] == 1) typePer = Type.PerSkill(4, defChara.type1, defChara.type2);
        if (defChara.passiveName== "�z���[�S��" && skill.type == "�S�[�X�g")
        {
            Area area = field.ToArea();
            area.message = PassiveText(defChara) + "�S�[�X�g�^�C�v�̋Z�𖳌�������";
            punBattle.Send("area", area.ToString());
            typePer = 0;
        }
        if (defChara.passiveName == "���^(�ɂ�����)" && skill.type == "����")
        {
            Area area = field.ToArea();
            area.message = PassiveText(defChara) + "�����^�C�v�̋Z�𖳌�������";
            punBattle.Send("area", area.ToString());
            typePer = 0;
        }
        if (ItemName(defChara.item)=="�ӂ�����" && skill.type == "���߂�")
        {
            Message(ItemText(defChara) + defChara.nameC + "�́@�n�ʋZ��������Ȃ�");
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
        if (skill.name == "���P�b�g����" || skill.name == "���e�I�r�[��" || skill.name == "�\�[���[�r�[��" || skill.name == "�\�[���[�u���[�h")
        {
            if(atkChara.buffer[8]==0)yield break;
        }
        if (skill.name == "�������")
        {
            if (atkChara.master)
            {
                if (field.enemy[2] + field.enemy[3] + field.enemy[4] > 0)
                {
                    field.enemy[2] = 0;
                    field.enemy[3] = 0;
                    field.enemy[4] = 0;
                    yield return AreaIE("���ׂ������");
                }
            }
            else
            {
                if (field.player[2] + field.player[3] + field.player[4] > 0)
                {
                    field.player[2] = 0;
                    field.player[3] = 0;
                    field.player[4] = 0;
                    yield return AreaIE("���ׂ������");
                }
            }
        }
        if (skill.physics != "�ω�")
        {
            int typePer = TypePer(skill, atkChara, defChara);
            //�_���[�W
            float atk;
            float def;
            if (skill.physics == "����")
            {
                atk = atkChara.RankStatus(1);
                def = defChara.RankStatus(2);
                if (skill.name == "�C�J�T�}") atk = defChara.RankStatus(1);
                if (skill.name == "���炰��" && atkChara.error == 4) atk *= 2;
                if (skill.name == "�����Ȃ�邬" || skill.name == "�G�N�X�J���o�[" || skill.name == "�w���G�X�^�Z�C�o�[") def = defChara.status[2];
            }
            else
            {
                atk = atkChara.RankStatus(3);
                def = defChara.RankStatus(4);
                if (skill.name == "�T�C�R�V���b�N") def = defChara.RankStatus(2);
            }
            float power = SkillPower(skill, atkChara, defChara,selectNum);

            Debug.Log(atk + "/" + def + "*" + power + "*" + 2f / 5f + "*" + typePer / 100 + "=" + atk / def * power * 2f / 5f * typePer / 100);
            float damageF = atk / def * power * 2f / 5f * typePer / 100;

            //�N���e�B�J��
            float damageC = Critcul(master, skill, power, atkChara, defChara, damageF, typePer);
            bool c = false;
            if (damageF < damageC)
            {
                c = true;
                damageF = damageC;
            }
            //�����Ɛ�����
            int damageI = Mathf.FloorToInt(damageF) + UnityEngine.Random.Range(-10, 1);
            if (damageI < 1) damageI = 1;
            if (typePer == 0 || power==0) damageI = 0;
            damageI = SkillDamage(skill, atkChara, defChara, damageI,typePer);

            if (damageI == 0)
            {
                Message(atkChara.nameC + "�� " + skill.name + "!" + "\n" + "���������܂������Ȃ�����");
                yield break;
            }

            //���M���e
            string text;
            text = atkChara.nameC + "�� " + skill.name + "!";
            if (skill.critical != -1)
            {
                if (c) text += " �N���e�B�J���q�b�g�I";
                if (typePer > 100) text += "\n" + "�������͂΂���!";
                else if (100 > typePer && typePer > 0) text += "\n" + "�������͂��܂ЂƂ�";
            }
            //�_���[�W����
            yield return DamageIE(skill, atkChara, defChara, text, damageI,typePer);
            switch (atkChara.passiveName)
            {
                case "�����ւ�":
                    if (defChara.error != 8)
                    {
                        defChara.type1 = skill.type;
                        defChara.type2 = "";
                        Message(PassiveText(atkChara) + defChara.nameC + "�́@�^�C�v��" + skill.type + "�ɕς����I");
                    }
                    break;
                case "�˂��Ƃ�`���[�j���O":
                    if (defChara.error != 8)
                    {
                        atkChara.type1 = skill.type;
                        atkChara.type2 = "";
                        Message(PassiveText(atkChara) + atkChara.nameC + "�́@�^�C�v��" + skill.type + "�ɕς����I");
                    }
                    break;
                case "����̎�":
                    if (defChara.sex =="����")
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
            damage.message = atkChara.nameC + "�� " + skill.name + "!";
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
            case "�E�F�U�[�{�[��":
                if (field.both[0] != 0) power *= 2;
                break;
            case "���炰��":
                if (atkChara.error != 0) power *= 2;
                break;
            case "�����΂�":
                power = 200 - atkChara.HPPer() * 200;
                break;
            case "�ɂ���Ԃ�":
                power = 125 * defChara.HPPer();
                break;
            case "�͂�����":
                if (atkChara.buffer[6] > 5) i = 5;
                else i = atkChara.buffer[6];
                if (i > 0)
                {
                    atkChara.buffer[6] -= i;
                    Effect effect = atkChara.ToEffect();
                    effect.message = i + "�����킦������";
                    punBattle.Send("effect", effect.ToString());
                    power = 90 * i;
                }
                break;
            case "�����Ƃ̂ق̂�":
                j = 0;
                for (i = 1; i < 8; i++)
                {
                    if (defChara.rank[i] > 0) j += defChara.rank[i];
                }
                power += j * 30;
                break;
            case "�䂫�Ȃ���":
                if (atkChara.damageP > 0 || atkChara.damageS > 0) power *= 2;
                break;
            case "�A�C�X�{�[��":
                power = 35 * Mathf.Pow(2, (atkChara.successNum - 1) % 5);
                if (atkChara.buffer[2] > 0) power *= 2;
                break;
            case "������������":
                power = 200 - atkChara.HPPer() * 200;
                break;
            case "���x���W":
                if (atkChara.damageP > 0 || atkChara.damageS > 0) power *= 2;
                break;
            case "�݂��Ȃ�����I":
                if (defChara.sex == "�j��") power *= 2;
                break;
            case "������":
                if (!atkChara.successPrev && !atkChara.firstAttack) power *= 2;
                break;
            case "�A�N���o�b�g":
                if (atkChara.item == 0) power *= 2;
                break;
            case "�A�V�X�g�p���[":
                j = 0;
                for (i = 1; i < 8; i++)
                {
                    if (atkChara.rank[i] > 0) j += atkChara.rank[i];
                }
                power += i * 20;
                break;
            case "��񂼂�����":
                power += atkChara.buffer[13] * 30;
                break;
            case "���낪��":
                power = 35 * Mathf.Pow(2, (atkChara.successNum - 1) % 5);
                if (atkChara.buffer[2] > 0) power *= 2;
                break;
            case "�������":
                if (defChara.error != 0) power *= 2;
                break;
            case "�����؂�����":
                if (!atkChara.fastAttack) power *= 2;
                break;
            case "��������":
                j = 0;
                for (i = 1; i < 8; i++)
                {
                    if (atkChara.rank[i] > 0) j += atkChara.rank[i];
                }
                power += i * 20;
                break;
            case "�͂������Ƃ�":
                if (defChara.item != 0) power *= 1.5f;
                break;
            case "���傤�����̂ނ�":
                if (defChara.rank[4] < 0) power -= 30 * defChara.rank[4];
                break;
            case "�W���C���{�[��":
                power = defChara.RankStatus(5) / atkChara.RankStatus(5) * 25;
                if (power > 150) power = 150;
                break;
        }

        switch (atkChara.passiveName)
        {
            case "�G���t�̐X":
                if (skill.type == "����")
                {
                    power *= 1.5f;
                    Message(PassiveText(atkChara)+"�����Z�̈З͂����܂�");
                    
                }
                break;
            case "����������":
                if (skill.type == "�t�F�A���[")
                {
                    power *= 1.5f;
                    Message(PassiveText(atkChara) + "�t�F�A���[�Z�̈З͂����܂�");
                }
                break;
            case "���j�u���[�V�����L���O�v":
                if (skill.name == "���ς�")
                {
                    power *= 1.5f;
                    Message(PassiveText(atkChara) + "���ς�̈З͂����܂�");
                }
                break;
            case "Darkness Eater":
                if (defChara.error==7)
                {
                    power *= 1.5f;
                    Message(PassiveText(atkChara) + "�˂ނ��Ԃ̑���ɗ^����_���[�W�����܂�");
                }
                break;
            case "���̃|��":
                power *= 1.2f;
                break;
            case "�ϋv�̔z�M":
                if (atkChara.prevSkill == selectNum)
                {
                    if (atkChara.successNum < 5) power *= 1 + 0.2f * (atkChara.successNum-1);
                    else power *= 2;
                }
                break;
            case "�U���N�̗͈�":
                if (power <= 60)
                {
                    power *= 1.5f;
                }
                break;
            case "�A���L�f�X":
                if (defChara.type1=="�ނ�" || defChara.type2 == "�ނ�")
                {
                    power *= 2f;
                    Message(PassiveText(atkChara) + "�ނ��^�C�v�̑���ɗ^����_���[�W�����܂�");
                }
                break;
        }
        switch (defChara.passiveName)
        {
            case "�G���t�̐X":
                if (skill.type == "�ق̂�")
                {
                    power *= 2f;
                    Message(PassiveText(defChara) + "�ق̂��Z�Ŏ󂯂�_���[�W�����܂�");
                }
                break;
        }
        switch (ItemName(atkChara.item))
        {
            case "���̂��̂���":
                power *= 1.3f;
                break;
            case "�������n�`�}�L":
                if (skill.physics == "����") power *= 1.5f;
                break;
            case "������胁�K�l":
                if (skill.physics == "����") power *= 1.5f;
                break;
            case "������̃n�`�}�L":
                if (skill.physics == "����") power *= 1.1f;
                break;
            case "���̂��胁�K�l":
                if (skill.physics == "����") power *= 1.5f;
                break;
            case "���g���m�[��":
                if (atkChara.prevSkill == selectNum)
                {
                    if (atkChara.successNum < 5) power *= 1 + 0.2f * (atkChara.successNum - 1);
                    else power *= 2;
                }
                break;
        }
        if (skill.type == "�ł�" && atkChara.buffer[5] > 0)
        {
            power *= 2f;
            atkChara.buffer[5] = 0;
        }
        if (field.both[0] == 1)
        {
            if (skill.type == "�ق̂�") power *= 1.5f;
            if (skill.type == "�݂�") power *= 0.75f;
            if (skill.type == "������") power *= 0.75f;
        }
        if (field.both[0] == 2)
        {
            if (skill.type == "�ق̂�") power *= 0.75f;
            if (skill.type == "�݂�") power *= 1.5f;
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
                if (skill.physics == "����")
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
                if (skill.physics == "����")
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
            if (atkChara.passiveName == "���A�c" && turn % 10 == 7) correctP[2] += 15 * 7;
            if (atkChara.passiveName == "�s���A�S") correctP[2] += 15;
            if (correctP[2] > Random.Range(0, 100))
            {
                correctP[2] -= 100;
                damageF *= 1.5f;
                float atkC;
                float defC;
                if (skill.physics == "����")
                {
                    atkC = atkChara.status[1];
                    defC = defChara.status[2];
                    if (skill.name == "�C�J�T�}") atkC = defChara.status[1];
                }
                else
                {
                    atkC = atkChara.status[3];
                    defC = defChara.status[4];
                    if (skill.name == "�T�C�R�V���b�N") defC = defChara.status[2];
                }
                damageC = atkC / defC * power * 2f / 5f * typePer / 100;
                if (skill.name == "�p�`���R") damageC *= 1.5f;
                if (skill.name == "�p�`���R") damageF *= 1.5f;
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
                if (skill.physics == "����")
                {
                    atkC = atkChara.status[1];
                    defC = defChara.status[2];
                    if (skill.name == "�C�J�T�}") atkC = defChara.status[1];
                }
                else
                {
                    atkC = atkChara.status[3];
                    defC = defChara.status[4];
                    if (skill.name == "�T�C�R�V���b�N") defC = defChara.status[2];
                }
                damageC = atkC / defC * power * 2f / 5f * typePer / 100;
                if (skill.name == "�p�`���R") damageC *= 1.5f;
                if (skill.name == "�p�`���R") damageF *= 1.5f;
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
            case "�n����":
                damage += 10;
                break;
        }
        switch (defChara.passiveName)
        {
            case "�S�ǂ̎l�c":
                if (typePer > 100)
                {
                    Message(PassiveText(defChara)+"���ʔ��Q�Ŏ󂯂�_���[�W���y������");
                    damage = Mathf.CeilToInt(damage * 0.75f);
                }
                break;
        }
        switch (ItemName(atkChara.item))
        {
            case "������̂���":
                if (typePer > 100)
                {
                    damage = Mathf.CeilToInt(damage * 1.2f);
                }
                break;
        }


        switch (skill.name)
        {
            case "������̂₦��":
                damage = Mathf.FloorToInt(defChara.status[0]/2f);
                break;
            case "���ނ����":
                damage =  defChara.hp - atkChara.hp;
                break;
            case "�̃h����":
                damage = defChara.hp;
                break;
            case "�A�S�h����":
                damage = defChara.hp;
                break;
            case "���������ꂢ��":
                damage = defChara.hp;
                break;
            case "�J�E���^�[":
                damage = atkChara.damageP * 2;
                break;
            case "�����イ�Ȃ�":
                damage = 50;
                break;
            case "�����":
                damage = defChara.hp;
                break;
            case "�~���[�R�[�g":
                damage = atkChara.damageS * 2;
                break;
            case "�i�C�g�w�b�h":
                damage = 50;
                break;
            case "������":
                damage = defChara.hp;
                break;
            case "���^���o�[�X�g":
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
        if (skill.target == "����" && defChara.error == 8) yield break;
        if (skill.target == "����")
        {
            if (atkChara.error==8)yield break;
            if (atkChara.master && atkChara != battlerP) yield break;
            if (!atkChara.master && atkChara != battlerE) yield break;
        }
        switch (skill.name)
        {
            case "��邠����":
                damage = Mathf.FloorToInt(defChara.status[0] / 10f);
                yield return DamageIE(atkChara, defChara, defChara.nameC + "�́@�_���[�W���󂯂�", damage);
                damage = Mathf.FloorToInt(atkChara.status[0] / 4f);
                yield return DamageIE(defChara, atkChara, atkChara.nameC + "�́@������������", damage);
                break;
            case "������":
                if (defChara.error == 0)
                {
                    if (defChara.error2[6] == 0)
                    {
                        defChara.error2[6] = 2;
                        yield return EffectIE(atkChara, defChara, defChara.nameC + "�� �˂ނ���U��ꂽ!");
                    }
                    else
                    {
                        Message(defChara.nameC + "�� ���łɂ˂ނ���Ԃ�");
                    }
                }
                else
                {
                    Message(defChara.nameC + "�� ���łɏ�Ԉُ킾");
                }
                break;
            case "�����̂Ђ���":
                if (field.both[0] == 0) damage = Mathf.FloorToInt(atkChara.status[0] / 2f);
                else if (field.both[0] == 1) damage = Mathf.FloorToInt(atkChara.status[0] * 2 / 3f);
                else damage = Mathf.FloorToInt(atkChara.status[0] * 1 / 3f);
                yield return DamageIE( defChara, atkChara, atkChara.nameC + "�́@�����̂Ђ������󂯂ĉ񕜂���", -damage);
                break;
            case "���΂��":
                if (atkChara.buffer[4] == 0)
                {
                    atkChara.buffer[4] = 3;
                    yield return EffectIE(defChara, atkChara, "");
                }
                break;
            case "���܂�������":
                field.both[4] = 5;
                yield return AreaIE("���܂������肪�Y����");
                break;
            case "�A���R�[��":
                if (defChara.prevSkill != -1)
                {
                    if (defChara.error2[9] == 0)
                    {
                        defChara.error2[9] = 3;
                        yield return EffectIE(atkChara, defChara, defChara.nameC + "�́@�����Z�����o���Ȃ��Ȃ����I");
                    }
                }
                break;
            case "�����݂킯":
                int half = Mathf.FloorToInt((atkChara.hp + defChara.hp) / 2f);
                damage = half - atkChara.hp;
                yield return DamageIE( defChara, atkChara, atkChara.nameC + "�́@HP�𕪂�������", -damage);
                damage = half - defChara.hp;
                yield return DamageIE( atkChara, defChara, defChara.nameC + "�́@HP�𕪂�������", -damage);
                break;
            case "���΂�":
                yield return defChara.StatusCheak(1, 1);
                if (defChara.error2[0] == 0)
                {
                    defChara.error2[0] = 2;
                    yield return EffectIE(atkChara, defChara, defChara.nameC + "�́@���������I");
                } else yield return EffectIE(atkChara, defChara, defChara.nameC + "�́@���łɍ������Ă���");
                break;
            case "�˂����Ƃ��т�":
                if (PerCorrect(atkChara.master, 30))
                {
                    defChara.error2[1] = 1;
                    yield return EffectIE(atkChara, defChara, defChara.nameC + "���@���܂����I");
                }
                break;
            case "����Ȃ���":
                yield return defChara.StatusCheak(2, -3);
                break;
            case "����܂�":
                yield return defChara.StatusCheak(6, -1);
                yield return defChara.StatusCheak(7, -1);
                break;
            case "��������":
                yield return defChara.StatusCheak(1, -1);
                yield return defChara.StatusCheak(3, -1);
                if (atkChara.master) correctP[1] += 20;
                else correctE[1] += 20;
                break;
            case "�����Ⴉ��":
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
                    yield return DamageIE( defChara, atkChara, atkChara.nameC + "�Ɓ@���Ԃ�HP���񕜂���", -damage);
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
                    yield return DamageIE(defChara, atkChara, atkChara.nameC + "�Ɓ@���Ԃ�HP���񕜂���", -damage);
                }
                break;
            case "�����Ԃ񂵂�":
                yield return atkChara.StatusCheak(6, 1);
                if (!atkChara.master) correctP[0] -= 20;
                else correctE[0] -= 20;
                break;
            case "���Ȃ��΂�":
                if (0 <= defChara.prevSkill && defChara.prevSkill <= 3)
                {
                    if (defChara.error2[13] == 0)
                    {
                        defChara.lockSkill = defChara.prevSkill;
                        s = skillMaster.SkillList.Find(a => a.no == defChara.skill[defChara.lockSkill]);
                        defChara.error2[13] = 4;
                        yield return EffectIE(atkChara, defChara, defChara.nameC + "�́@"+ s.name +"���o���Ȃ��Ȃ���");
                    }
                    else
                    {
                        yield return EffectIE(atkChara, defChara, defChara.nameC + "�́@���łɂ��Ȃ��΂��Ԃ�");
                    }
                }
                else
                {
                    Message("���������܂������Ȃ�����");
                }
                break;
            case "�������Ԃ�":
                yield return atkChara.StatusCheak(1, 2);
                yield return atkChara.StatusCheak(3, 2);
                yield return atkChara.StatusCheak(5, 2);
                yield return atkChara.StatusCheak(2, -1);
                yield return atkChara.StatusCheak(4, -1);
                yield return atkChara.StatusCheak(5, -1);
                break;
            case "��������":
                yield return defChara.StatusCheak(1, -1);
                yield return defChara.StatusCheak(2, -1);
                yield return defChara.StatusCheak(7, -1);
                break;
            case "���낢�܂Ȃ���":
                defChara.error2[12] = 30;
                yield return EffectIE(atkChara, defChara, defChara.nameC + "�́@�ɂ����Ȃ��Ȃ����I");
                break;
            case "���������X�s��":
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
            case "�R�[�g�`�F���W":
                int[] change = field.player;
                field.player = field.enemy;
                field.enemy = change;
                yield return AreaIE("��̏󋵂�����ւ�����I");
                break;
            case "���炦��":
                atkChara.buffer[7] = 0;
                yield return EffectIE(defChara, atkChara, atkChara.nameC + "�́@���炦��̐��ɂȂ����I");
                break;
            case "���킢����":
                yield return defChara.StatusCheak(1, -1);
                yield return defChara.StatusCheak(3, -1);
                yield return defChara.StatusCheak(5, -1);
                break;
            case "���킮":
                if (atkChara.buffer[9] == 0)
                {
                    atkChara.buffer[9] = 3;
                    yield return EffectIE(defChara, atkChara, "");
                }
                break;
            case "��������":
                atkChara.rank = defChara.rank;
                yield return EffectIE(defChara, atkChara, atkChara.nameC + "�́@����̔\�͕ω����R�s�[����");
                break;
            case "������������":
                damage = Mathf.FloorToInt(atkChara.status[0] / 2f);
                yield return DamageIE( defChara, atkChara, atkChara.nameC + "�́@�񕜂���", -damage);
                break;
            case "�����ۂ��ӂ�":
                yield return defChara.StatusCheak(2, -1);
                yield return defChara.StatusCheak(4, -1);
                if (atkChara.master) correctP[1] += 20;
                else correctE[1] += 20;
                break;
            case "���΂�":
                yield return DamageIE( defChara, atkChara, atkChara.nameC + "�́@���΂��̔������󂯂�", 9999);
                break;
            case "���߂���":
                if (defChara.error2[2] == 0)
                {
                    defChara.error2[2] = 4;
                    yield return EffectIE(atkChara, defChara, defChara.nameC + "�́@�Ƃ��ꂽ");
                }
                break;
            case "�X�C�[�v�r���^":
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
                Message((i + 1) + "�񂠂�����");
                break;
            case "����":
                if (PerCorrect(atkChara.master, 30))
                {
                    defChara.error2[1] = 1;
                    yield return EffectIE(atkChara, defChara, defChara.nameC + "���@���܂����I");
                }
                break;
            case "���Ă݃^�b�N��":
                damage = Mathf.FloorToInt(prevSkillDamage * 0.33f);
                yield return DamageIE( defChara, atkChara, atkChara.nameC + "�́@�������󂯂�", damage);
                break;
            case "�X�s�[�h�X�^�[":
                if (atkChara.master) correctP[0] += 20;
                else correctE[0] += 20;
                break;
            case "�������傤":
                if (atkChara.passiveName == "��lVer" && atkChara.nameC == "�X���ԍ�")
                {
                    c1 = charaMaster.CharaList.Find(a => a.name == "�X���ԍ�");
                    c2 = charaMaster.CharaList.Find(a => a.name == "�X���ԍ�(��l)");
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
                    summon.exception = "�X���ԍ�́@��l�ɕω������I";
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
            case "�����킦��":
                atkChara.buffer[6] += 3;
                if (atkChara.buffer[6] > 9) atkChara.buffer[6] = 9;
                yield return atkChara.StatusCheak(3, 1);
                break;
            case "���������Ȃ�":
                if (atkChara.passiveName == "�q��Ver" && atkChara.nameC == "�X���ԍ�(��l)")
                {
                    c1 = charaMaster.CharaList.Find(a => a.name == "�X���ԍ�");
                    c2 = charaMaster.CharaList.Find(a => a.name == "�X���ԍ�(��l)");
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
                    summon.exception = "�X���ԍ�́@�q���ɕω������I";
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
                        yield return DamageIE(defChara, atkChara, atkChara.nameC + "�́@���������Ȃ���HP��������", damage);
                    }
                    else Message("���������܂������Ȃ�����");
                }
                break;
            case "�邬�̂܂�":
                yield return atkChara.StatusCheak(1, 2);
                break;
            case "�ɂ����񂶃A�^�b�N":
                if (defChara.error==0)
                if (PerCorrect(atkChara.master, 20))
                {
                    i = UnityEngine.Random.Range(0, 3);
                    if (i == 0)yield return defChara.ErrorCheak(3);
                    if (i == 1)yield return defChara.ErrorCheak(4);
                    if (i == 2)yield return defChara.ErrorCheak(5);
                }
                break;
            case "�Ȃ��܂Â���":
                defChara.PassiveChange(atkChara.passiveName, atkChara.passiveText);
                Message(defChara.nameC + "�́@�Ƃ�������" + defChara.passiveName + "�ɕς���");
                break;
            case "�Ȃ܂���":
                damage = Mathf.FloorToInt(atkChara.status[0] / 2f);
                yield return DamageIE( defChara, atkChara, atkChara.nameC + "�́@�񕜂���", -damage);
                break;
            case "�ɂ�݂���":
                yield return defChara.StatusCheak(2, -1);
                yield return defChara.StatusCheak(5, -1);
                if (PerCorrect(atkChara.master, 20))
                {
                    defChara.error2[1] = 1;
                    yield return EffectIE(atkChara, defChara, defChara.nameC + "���@���܂����I");
                }
                break;
            case "�˂�������":
                if (atkChara.master)
                {
                    if (field.player[1] == 0) yield return AreaIE(atkChara.nameC + "�́@�����Ɋ肢��������I");
                    else Message("���������܂������Ȃ�����");
                }
                else
                {
                    if (field.enemy[1] == 0) yield return AreaIE(atkChara.nameC + "�́@�����Ɋ肢��������I");
                    else Message("���������܂������Ȃ�����");
                }
                break;
            case "�������o�g��":
                defChara.error2[1] = 1;
                yield return EffectIE(atkChara, defChara, defChara.nameC + "���@���܂����I");
                break;
            case "�̂�������":
                if (defChara.error == 0)
                if (PerCorrect(atkChara.master, 30))
                {
                        yield return defChara.ErrorCheak(3);
                }
                break;
            case "�݂̂���":
                damage = Mathf.FloorToInt(atkChara.status[0] / 4f);
                if (atkChara.buffer[6] >= 2)
                {
                    damage = Mathf.FloorToInt(atkChara.status[0] / 2f);
                    atkChara.buffer[6] -= 2;
                    yield return EffectIE(defChara, atkChara, atkChara.nameC + "�́@2�����킦���g�p����");
                }
                if (atkChara.buffer[6] == 1)
                {
                    damage = Mathf.FloorToInt(atkChara.status[0] * 3 / 4f);
                    atkChara.buffer[6] -= 1;
                    yield return EffectIE(defChara, atkChara, atkChara.nameC + "�́@1�����킦���g�p����");
                }
                yield return DamageIE( defChara, atkChara, atkChara.nameC + "�́@�񕜂���", -damage);
                break;
            case "�͂�����������":
                atkChara.buffer[10] = 1;
                break;
            case "�΂������":
                yield return atkChara.StatusCheak(3, -1);
                break;
            case "�o�g���^�b�`":
                if (atkChara.master) member = playerMember;
                else member = enemyMember;
                int j = 0;
                for (i = 0; i < 3; i++)
                {
                    if (member[i].error != 8) j++;
                }
                if (j < 2)
                {
                    Message("���������܂������Ȃ�����");
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
            case "�ӂ݂�":
                if (PerCorrect(atkChara.master, 30))
                {
                    defChara.error2[1] = 1;
                    yield return EffectIE(atkChara, defChara, defChara.nameC + "���@���܂����I");
                }
                break;
            case "�ق���":
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
                if (!b) Message("���������܂������Ȃ�����");
                break;
            case "�ق�т̂���":
                atkChara.error2[7] = 3;
                defChara.error2[7] = 3;
                yield return EffectIE(atkChara, defChara, "");
                yield return EffectIE(defChara, atkChara, "�ق�т̃J�E���g�_�E�����n�܂���");
                break;
            case "�܂���":
                atkChara.buffer[1] = 1;
                yield return EffectIE(defChara, atkChara, "");
                break;
            case "�܂邭�Ȃ�":
                atkChara.buffer[2] = 1;
                yield return EffectIE(defChara, atkChara, "");
                yield return atkChara.StatusCheak(2, 1);
                break;
            case "�䂪�݂�":
                damage = Mathf.FloorToInt(atkChara.status[0] / 4f);
                if (atkChara.hp > damage && atkChara.buffer[0] == 0)
                {
                    yield return DamageIE( defChara, atkChara, atkChara.nameC + "�́@HP������Ă䂪�݂�������", damage);
                    atkChara.buffer[0] = damage * 2;
                    yield return EffectIE(defChara, atkChara, atkChara.nameC + "�́@HP������Ă䂪�݂�������");
                }
                else Message("���������܂������Ȃ�����");
                break;
            case "�݂���Ђ�����":
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
                Message((i + 1) + "�񂠂�����");
                break;
            case "�~���N�̂�":
                damage = Mathf.FloorToInt(atkChara.status[0] / 2f);
                yield return DamageIE( defChara, atkChara, atkChara.nameC + "�́@�񕜂���", -damage);
                break;
            case "���P�b�g����":
                if (atkChara.buffer[8] == 0)
                {
                    atkChara.buffer[8] = 1;
                    yield return EffectIE(defChara, atkChara, "�U���̑̐��ɂȂ����I");
                    yield return atkChara.StatusCheak(1, 1);
                }
                else
                {
                    atkChara.buffer[8] = 0;
                    yield return EffectIE(defChara, atkChara, "");
                }
                break;
                
            case "���̂܂�":
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
                    Message("���������܂������Ȃ�����");
                }
                break;
            case "���������傤":
                yield return defChara.ErrorCheak(6);
                break;
            case "�˂����܂�":
                defChara.error2[1] = 1;
                yield return EffectIE(atkChara, defChara, defChara.nameC + "���@���܂����I");
                break;
            case "�������ق̂�":
                if (defChara.error == 0)
                if (PerCorrect(atkChara.master, 20))
                {
                        yield return defChara.ErrorCheak(4);
                }
                break;
            case "�I�[�o�[�q�[�g":
                yield return atkChara.StatusCheak(3, -1);
                break;
            case "���ɂ�":
                yield return defChara.ErrorCheak(4);
                break;
            case "������ق�����":
                if (defChara.error == 0)
                    if (PerCorrect(atkChara.master, 10))
                {
                    yield return defChara.ErrorCheak(4);
                }
                break;
            case "�����Ȃ�ق̂�":
                if (defChara.error == 0)
                    if (PerCorrect(atkChara.master, 50))
                {
                    yield return defChara.ErrorCheak(4);
                }
                break;
            case "��������":
                if (defChara.error == 0)
                    if (PerCorrect(atkChara.master, 10))
                {
                    yield return defChara.ErrorCheak(4);
                }
                break;
            case "�j�g���`���[�W":
                yield return atkChara.StatusCheak(5, 1);
                break;
            case "�ɂق�΂�":
                if (field.both[0] != 1)
                {
                    field.both[0] = 1;
                    field.both[1] = 5;
                    if(ItemName(atkChara.item)== "�Ă񂫃f�b�L") field.both[1] += 3;
                    yield return AreaIE("�Ђł肪�����Ȃ����I");
                }
                else
                {
                    Message("���łɂЂł�͋���");
                }
                break;
            case "�r�b�N���w�b�h":
                damage = Mathf.FloorToInt(atkChara.status[0] / 2f);
                yield return DamageIE( defChara, atkChara, atkChara.nameC + "�́@�������󂯂�", damage);
                break;
            case "�t���A�h���C�u":
                damage = Mathf.FloorToInt(prevSkillDamage * 0.33f);
                yield return DamageIE( defChara, atkChara, atkChara.nameC + "�́@�������󂯂�", damage);
                break;
            case "�ق̂��̂���":
                if (defChara.error2[2] == 0)
                {
                    defChara.error2[2] = 4;
                    yield return EffectIE(atkChara, defChara, defChara.nameC + "�́@�Ƃ��ꂽ");
                }
                break;
            case "�ق̂��̃L�o":
                if (PerCorrect(atkChara.master, 20))
                {
                    if (defChara.error == 0) yield return defChara.ErrorCheak(4);
                    else
                    {
                        defChara.error2[1] = 1;
                        yield return EffectIE(atkChara, defChara, defChara.nameC + "���@���܂����I");
                    }
                }
                break;
            case "�ق̂��̂܂�":
                if (PerCorrect(atkChara.master, 50))
                {
                    yield return atkChara.StatusCheak(3, 1);
                }
                break;
            case "�������`":
                yield return defChara.StatusCheak(4, -1);
                break;
            case "�}�W�J���t���C��":
                yield return defChara.StatusCheak(3, -1);
                break;
            case "�₫����":
                if (defChara.item != 0)
                {
                    defChara.item = 0;
                    yield return EffectIE(atkChara, defChara, defChara.nameC + "�́@�������̂��Ă��s�������I");
                }
                break;
            case "��񂲂�":
                yield return defChara.ErrorCheak(4);
                break;
            case "�A�N�A�����O":
                if (atkChara.master) field.player[5] = 10;
                else field.enemy[5] = 10;
                yield return AreaIE("�A�N�A�����O�ŏꂪ������");
                break;
            case "���܂���":
                if (field.both[0] != 2)
                {
                    field.both[0] = 2;
                    field.both[1] = 5;
                    if (ItemName(atkChara.item) == "�Ă񂫃f�b�L") field.both[1] += 3;
                    yield return AreaIE("�J���~��n�߂��I");
                }
                else
                {
                    Message("���łɉJ�͍~���Ă���");
                }
                break;
            case "��������":
                if (defChara.error2[2] == 0)
                {
                    defChara.error2[2] = 4;
                    yield return EffectIE(atkChara, defChara, defChara.nameC + "�́@�Ƃ��ꂽ");
                }
                break;
            case "����ɂ�����":
                yield return atkChara.StatusCheak(3, 1);
                yield return atkChara.StatusCheak(3, 1);
                break;
            case "�N�C�b�N�^�[��":
                yield return EffectChange(atkChara.master, r => b = r);
                break;
            case "�����̂ڂ�":
                if (PerCorrect(atkChara.master, 20))
                {
                    defChara.error2[1] = 1;
                    yield return EffectIE(atkChara, defChara, defChara.nameC + "���@���܂����I");
                }
                break;
            case "�˂��Ƃ�":
                if (defChara.error == 0)
                    if (PerCorrect(atkChara.master, 30))
                {
                    yield return defChara.ErrorCheak(4);
                }
                break;
            case "10�܂�{���g":
                if (defChara.error == 0)
                    if (PerCorrect(atkChara.master, 10))
                {
                    yield return defChara.ErrorCheak(3);
                }
                break;
            case "�G���L�l�b�g":
                yield return defChara.StatusCheak(5, -1);
                break;
            case "�����ł��":
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
            case "���݂Ȃ�":
                if (defChara.error == 0)
                    if (PerCorrect(atkChara.master, 30))
                {
                    yield return defChara.ErrorCheak(3);
                }
                break;
            case "���݂Ȃ�̃L�o":
                if (PerCorrect(atkChara.master, 20))
                {
                    if (defChara.error == 0) yield return defChara.ErrorCheak(3);
                    else
                    {
                        defChara.error2[1] = 1;
                        yield return EffectIE(atkChara, defChara, defChara.nameC + "���@���܂����I");
                    }
                }
                break;
            case "���イ�ł�":
                atkChara.buffer[5] = 1;
                yield return EffectIE(defChara, atkChara, "");
                yield return atkChara.StatusCheak(4, 1);
                break;
            case "�`���[�W�r�[��":
                if (PerCorrect(atkChara.master, 70))
                {
                    yield return atkChara.StatusCheak(3, 1);
                }
                break;
            case "�ł񂰂���":
                if (atkChara.master) correctP[0] += 20;
                else correctE[0] += 20;
                break;
            case "�ł񂶂�":
                yield return defChara.ErrorCheak(3);
                break;
            case "�ł񂶂ق�":
                yield return defChara.ErrorCheak(3);
                break;
            case "�ق��ł�":
                if (defChara.error == 0)
                    if (PerCorrect(atkChara.master, 30))
                {
                    yield return defChara.ErrorCheak(3);
                }
                break;
            case "�ق��؂��肷��":
                yield return defChara.ErrorCheak(3);
                break;
            case "�{���e�b�J�[":
                damage = Mathf.FloorToInt(prevSkillDamage * 0.33f);
                yield return DamageIE( defChara, atkChara, atkChara.nameC + "�́@�������󂯂�", damage);
                break;
            case "�{���g�`�F���W":
                yield return EffectChange(atkChara.master, r => b = r);
                break;
            case "�A���}�Z���s�[":
                if (atkChara.master)
                {
                    for (i = 0; i < 3; i++) {
                        playerMember[i].error = 0;
                        playerMember[i].errorTurn = 0;
                    }
                    yield return EffectIE(defChara, atkChara, "�����̏�Ԉُ����菜����");
                }
                break;
            case "�E�b�h�n���}�[":
                damage = Mathf.FloorToInt(prevSkillDamage * 0.33f);
                yield return DamageIE( defChara, atkChara, atkChara.nameC + "�́@�������󂯂�", damage);
                break;
            case "�G�i�W�[�{�[��":
                if (PerCorrect(atkChara.master, 10))
                {
                    yield return defChara.StatusCheak(4, -1);
                }
                break;
            case "�M�K�h���C��":
                damage = Mathf.FloorToInt(prevSkillDamage * 0.5f);
                yield return DamageIE( defChara, atkChara, atkChara.nameC + "�́@���肩��HP���z�������", -damage);
                break;
            case "�L�m�R�̂ق���":
                yield return defChara.ErrorCheak(6);
                break;
            case "���тꂲ��":
                yield return defChara.ErrorCheak(3);
                break;
            case "�\�[���[�r�[��":
                if (atkChara.buffer[8] == 0)
                {
                    atkChara.buffer[8] = 1;
                    yield return EffectIE(defChara, atkChara, "�����z�����Ă���c");
                }
                else
                {
                    atkChara.buffer[8] = 0;
                    yield return EffectIE(defChara, atkChara, "");
                }
                break;
            case "�\�[���[�u���[�h":
                if (atkChara.buffer[8] == 0)
                {
                    atkChara.buffer[8] = 1;
                    yield return EffectIE(defChara, atkChara, "�����z�����Ă���c");
                }
                else
                {
                    atkChara.buffer[8] = 0;
                    yield return EffectIE(defChara, atkChara, "");
                }
                break;
            case "�^�l�}�V���K��":
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
                Message((i + 1) + "�񂠂�����");
                break;
            case "������������Ƃ�":
                damage = Mathf.FloorToInt(defChara.RankStatus(1));
                yield return defChara.StatusCheak(1, -1);
                yield return DamageIE( defChara, atkChara, atkChara.nameC + "�́@���肩�炿������z�������", -damage);
                break;
            case "�h�����A�^�b�N":
                yield return defChara.StatusCheak(5, -1);
                break;
            case "�g���s�J���L�b�N":
                yield return defChara.StatusCheak(1, -1);
                break;
            case "�Ȃ�݂̃^�l":
                if (defChara.error2[8] == 0)
                {
                    defChara.error2[8] = 1;
                    defChara.passiveNameDelete = defChara.passiveName;
                    defChara.passiveTextDelete = defChara.passiveText;
                    defChara.passiveName = "";
                    defChara.passiveText = "";
                    yield return EffectIE(atkChara, defChara, defChara.nameC + "�́@�Ƃ������𖳌�������");
                }
                else
                {
                    Message("�����͂��łɖ���������Ă���");
                }
                yield return defChara.ErrorCheak(6);
                break;
            case "�j�[�h���K�[�h":
                atkChara.buffer[11] = 1;
                yield return EffectIE(defChara, atkChara, "");
                break;
            case "�͂Ȃт�̂܂�":
                if (atkChara.buffer[4] == 0)
                {
                    atkChara.buffer[4] = 3;
                    yield return EffectIE(defChara, atkChara, "");
                }
                break;
            case "�}�W�J�����[�t":
                if (atkChara.master) correctP[0] += 20;
                else correctE[0] += 20;
                break;
            case "��ǂ肬�̃^�l":
                defChara.error2[5] = 1;
                yield return EffectIE(atkChara, defChara, defChara.nameC + "�Ɂ@��ǂ肬�̂��˂�A����");
                break;
            case "���[�t�X�g�[��":
                yield return atkChara.StatusCheak(3, -1);
                break;
            case "�����":
                if (field.both[0] != 3)
                {
                    field.both[0] = 3;
                    field.both[1] = 5;
                    if (ItemName(atkChara.item) == "�Ă񂫃f�b�L") field.both[1] += 3;
                    yield return AreaIE("����ꂪ�~��n�߂��I");
                }
                else
                {
                    Message("���łɂ����͍~���Ă���");
                }
                break;
            case "�I�[�����x�[��":
                if (atkChara.master) {
                    if (field.player[2] == 0)
                    {
                        if (field.both[0] == 3) field.player[2] = 5;
                        else field.player[2] = 3;
                        if (ItemName(atkChara.item) == "�Ђ���̂˂��") field.player[2] += 3;
                        yield return AreaIE("�I�[�����x�[���𒣂���");
                    }
                    else
                    {
                        Message("�I�[�����x�[���͊��ɒ����Ă���");
                    }
                }
                else
                {
                    if (field.enemy[2] == 0)
                    {
                        if (field.both[0] == 3) field.enemy[2] = 5;
                        else field.enemy[2] = 3;
                        if (ItemName(atkChara.item) == "�Ђ���̂˂��") field.enemy[2] += 3;
                        yield return AreaIE("�I�[�����x�[���𒣂���");
                    }
                    else
                    {
                        Message("�I�[�����x�[���͊��ɒ����Ă���");
                    }
                }
                break;
            case "���낢����":
                atkChara.rank = new int[8];
                defChara.rank = new int[8];
                yield return EffectIE(atkChara, defChara, "");
                yield return EffectIE(defChara, atkChara, "���݂��̔\�͕ω������ɖ߂���");
                break;
            case "������̃L�o":
                if (PerCorrect(atkChara.master, 20))
                {
                    if (defChara.error == 0) yield return defChara.ErrorCheak(5);
                    else
                    {
                        defChara.error2[1] = 1;
                        yield return EffectIE(atkChara, defChara, defChara.nameC + "���@���܂����I");
                    }
                }
                break;
            case "�������邩��":
                yield return defChara.StatusCheak(5, -1);
                break;
            case "���낢����":
                if (atkChara.master) field.player[6] = 4;
                else field.enemy[6] = 4;
                yield return AreaIE("��ɂ��낢���肪�Y��");
                break;
            case "��炨�Ƃ�":
                if (PerCorrect(atkChara.master, 30))
                {
                    defChara.error2[1] = 1;
                    yield return EffectIE(atkChara, defChara, defChara.nameC + "���@���܂����I");
                }
                break;
            case "���΂�":
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
                Message((i + 1) + "�񂠂�����");
                break;
            case "�ӂԂ�":
                if (defChara.error == 0)
                    if (PerCorrect(atkChara.master, 10))
                {
                    yield return defChara.ErrorCheak(5);
                }
                break;
            case "�ꂢ�Ƃ��r�[��":
                if (defChara.error == 0)
                    if (PerCorrect(atkChara.master, 10))
                {
                    yield return defChara.ErrorCheak(5);
                }
                break;
            case "�A�C�X�{�[��":
                if (atkChara.buffer[4] == 0) atkChara.buffer[4] = 5;
                yield return EffectIE(defChara, atkChara, "");
                break;
            case "�߂������`":
                yield return defChara.StatusCheak(4, -1);
                break;
            case "�A�[���n���}�[":
                yield return atkChara.StatusCheak(5, -1);
                break;
            case "���񂩂���":
                yield return atkChara.StatusCheak(1, 1);
                yield return atkChara.StatusCheak(4, 1);
                break;
            case "�C���t�@�C�g":
                yield return atkChara.StatusCheak(2, -1);
                yield return atkChara.StatusCheak(4, -1);
                break;
            case "����������":
                if (PerCorrect(atkChara.master, 10))
                {
                    yield return defChara.StatusCheak(4, -1);
                }
                break;
            case "�O���E�p���`":
                yield return atkChara.StatusCheak(1, 1);
                break;
            case "���ς�":
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
                Message((i + 1) + "�񂠂�����");
                break;
            case "�Ƃ����Ȃ�":
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
                if (!b) Message("���������܂������Ȃ�����");
                break;
            case "�΂�������":
                yield return atkChara.StatusCheak(1, -1);
                yield return atkChara.StatusCheak(2, -1);
                break;
            case "�΂���p���`":
                if (defChara.error2[0] == 0)
                {
                    defChara.error2[0] = 2;
                    yield return EffectIE(atkChara, defChara, defChara.nameC + "�́@���������I");
                }
                else yield return EffectIE(atkChara, defChara, defChara.nameC + "�́@���łɍ������Ă���");
                break;
            case "�r���h�A�b�v":
                yield return atkChara.StatusCheak(1, 1);
                yield return atkChara.StatusCheak(2, 1);
                break;
            case "���[�L�b�N":
                yield return defChara.StatusCheak(5, -1);
                break;
            case "�A�V�b�h�{��":
                yield return defChara.StatusCheak(4, -2);
                break;
            case "������":
                yield return defChara.StatusCheak(2, -1);
                yield return defChara.StatusCheak(4, -1);
                if (defChara.error2[8] == 0)
                {
                    defChara.error2[8] = 1;
                    defChara.passiveNameDelete = defChara.passiveName;
                    defChara.passiveTextDelete = defChara.passiveText;
                    defChara.passiveName = "";
                    defChara.passiveText = "";
                    yield return EffectIE(atkChara, defChara, defChara.nameC + "�́@�Ƃ������𖳌�������");
                }
                else
                {
                    Message("�����͂��łɖ���������Ă���");
                }
                break;
            case "�N���A�X���b�O":
                defChara.rank = new int[8];
                yield return EffectIE(atkChara, defChara, defChara.nameC + "�́@�\�͕ω������ɖ߂���");
                break;
            case "���傤��":
                if (atkChara.error != 0)
                {
                    atkChara.error = 0;
                    atkChara.errorTurn = 0;
                    yield return EffectIE(defChara, atkChara, "");
                    damage = Mathf.FloorToInt(atkChara.status[0] / 4f);
                    yield return DamageIE( defChara, atkChara, atkChara.nameC + "�́@�񕜂���", -damage);
                }
                else
                {
                    Message("���������܂������Ȃ�����");
                }
                break;
            case "�_�X�g�V���[�g":
                if (defChara.error == 0)
                    if (PerCorrect(atkChara.master, 30))
                {
                    yield return defChara.ErrorCheak(1);
                }
                break;
            case "�ǂ��ǂ�":
                yield return defChara.ErrorCheak(2);
                break;
            case "�ǂ��ǂ��̃L�o":
                if (defChara.error == 0)
                    if (PerCorrect(atkChara.master, 50))
                {
                    yield return defChara.ErrorCheak(2);
                }
                break;
            case "�ǂ��т�":
                if (atkChara.master)
                {
                    if (field.enemy[11] < 2)
                    {
                        field.enemy[11]++;
                        if (field.enemy[11] == 1) yield return AreaIE("�ǂ��т����T����");
                        if (field.enemy[11] == 2) yield return AreaIE("�����ǂ��т����T����");
                    }
                    else Message("����ȏ�͂ǂ��т��͎T���Ȃ�");
                }
                else
                {
                    if (field.player[11] < 2)
                    {
                        field.player[11]++;
                        if (field.player[11] == 1) yield return AreaIE("�ǂ��т����T����");
                        if (field.player[11] == 2) yield return AreaIE("�����ǂ��т����T����");
                    }
                    else Message("����ȏ�͂ǂ��т��͎T���Ȃ�");
                }
                break;
            case "�ӂɂ��":
                yield return defChara.StatusCheak(2, 2);
                break;
            case "�w�h���E�F�[�u":
                if (defChara.error == 0)
                    if (PerCorrect(atkChara.master, 10))
                {
                    yield return defChara.ErrorCheak(1);
                }
                break;
            case "�N�\�}���΂�����":
                if (defChara.error == 0)
                    if (PerCorrect(atkChara.master, 30))
                {
                    yield return defChara.ErrorCheak(1);
                }
                break;
            case "�o���̃��`":
                yield return defChara.StatusCheak(4, -1);
                break;
            case "���Ȃ炵":
                yield return defChara.StatusCheak(5, -1);
                break;
            case "���Ȃ�����":
                if (defChara.error2[2] == 0)
                {
                    defChara.error2[2] = 4;
                    yield return EffectIE(atkChara, defChara, defChara.nameC + "�́@�Ƃ��ꂽ");
                }
                break;
            case "�������̂�����":
                if (PerCorrect(atkChara.master, 10))
                {
                    yield return defChara.StatusCheak(4, -1);
                }
                break;
            case "�ǂ납��":
                yield return defChara.StatusCheak(6, -1);
                break;
            case "�}�b�h�V���b�g":
                yield return defChara.StatusCheak(5, -1);
                break;
            case "�G�A�X���b�V��":
                if (PerCorrect(atkChara.master, 30))
                {
                    defChara.error2[1] = 1;
                    yield return EffectIE(atkChara, defChara, defChara.nameC + "���@���܂����I");
                }
                break;
            case "��������":
                if (atkChara.master)
                {
                    if (field.player[0] == 0)
                    {
                        field.player[0] = 4;
                        yield return AreaIE("�����������ӂ��n�߂��I");
                    }
                    else
                    {
                        Message("���������͂����ӂ��Ă���");
                    }
                }
                else
                {
                    if (field.enemy[0] == 0)
                    {
                        field.enemy[0] = 4;
                        yield return AreaIE("�����������ӂ��n�߂��I");
                    }
                    else
                    {
                        Message("���������͂����ӂ��Ă���");
                    }
                }
                break;
            case "�΂߂�����":
                if (atkChara.master) correctP[0] += 20;
                else correctE[0] += 20;
                break;
            case "�͂˂₷��":
                atkChara.buffer[12] = 1;
                yield return EffectIE(defChara, atkChara, "");
                damage = Mathf.FloorToInt(atkChara.status[0] / 2f);
                yield return DamageIE( defChara, atkChara, atkChara.nameC + "�́@�񕜂���", -damage);
                break;
            case "�u���C�u�o�[�h":
                damage = Mathf.FloorToInt(prevSkillDamage * 0.33f);
                yield return DamageIE( defChara, atkChara, atkChara.nameC + "�́@�������󂯂�", damage);
                break;
            case "�ڂ��ӂ�":
                if (PerCorrect(atkChara.master, 30))
                {
                    if (defChara.error2[0] == 0)
                    {
                        defChara.error2[0] = 2;
                        yield return EffectIE(atkChara, defChara, defChara.nameC + "�́@���������I");
                    }
                }
                break;
            case "������ׂ�":
                if (defChara.error2[0] == 0)
                {
                    defChara.error2[0] = 2;
                    yield return EffectIE(atkChara, defChara, defChara.nameC + "�́@���������I");
                }
                break;
            case "�G�A���u���X�g":
                if (atkChara.sex != defChara.sex)
                {
                    if (PerCorrect(atkChara.master, 10))
                    {
                        if (defChara.error2[4] == 0)
                        {
                            defChara.error2[4] = 1;
                            yield return EffectIE(atkChara, defChara, defChara.nameC + "�́@���������ɂȂ����I");
                        }
                        else yield return EffectIE(atkChara, defChara, defChara.nameC + "�́@���łɃ����������I");
                    }
                }
                break;
            case "���Ă�������":
                if (defChara.error == 0)
                    if (PerCorrect(atkChara.master, 10))
                {
                    yield return defChara.ErrorCheak(5);
                }
                break;
            case "���₵�̂˂���":
                if (atkChara.master) field.player[7] = 1;
                else field.enemy[7] = 1;
                yield return AreaIE("");
                yield return DamageIE( defChara, atkChara, atkChara.nameC + "�́@�g������Ċ肢�������", 9999);
                break;
            case "�K�[�h�V�F�A":
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
                    Message("���݂��̂ڂ�����ƂƂ��ڂ��𕪂����������I");
                }
                else
                {
                    Message("���������܂������Ȃ�����");
                }
                break;
            case "�����������ǂ�":
                yield return atkChara.StatusCheak(5, 2);
                break;
            case "�T�C�R�L�l�V�X":
                yield return defChara.StatusCheak(3, -1);
                break;
            case "�����݂񂶂��":
                yield return defChara.ErrorCheak(7);
                break;
            case "���˂�̂���":
                if (PerCorrect(atkChara.master, 20))
                {
                    defChara.error2[1] = 1;
                    yield return EffectIE(atkChara, defChara, defChara.nameC + "���@���܂����I");
                }
                break;
            case "������肫":
                if (PerCorrect(atkChara.master, 10))
                {
                    defChara.error2[1] = 1;
                    yield return EffectIE(atkChara, defChara, defChara.nameC + "���@���܂����I");
                }
                break;
            case "�g���b�N":
                i = atkChara.item;
                atkChara.item = defChara.item;
                defChara.item = i;
                yield return EffectIE(atkChara, defChara, "");
                yield return EffectIE(defChara, atkChara, "");
                Message("���݂��̃A�C�e�������������I");
                break;
            case "�g���b�N���[��":
                if (field.both[2] == 0)
                {
                    field.both[2] = 5;
                    yield return AreaIE("���󂪂䂪�ݎn�߂��I");
                }
                else
                {
                    field.both[2] = 0;
                    yield return AreaIE("�䂪�񂾎��󂪌��ɖ߂����I");
                }
                break;
            case "�h�킷��":
                yield return atkChara.StatusCheak(4, 2);
                break;
            case "�˂ނ�":
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
                        yield return DamageIE( defChara, atkChara, atkChara.nameC + "�́@�����Č��C�ɂȂ���", -damage);
                    }
                    else
                    {
                        atkChara.error = i;
                        atkChara.errorTurn = num;
                        //Message("���邱�Ƃ��ł��Ȃ��I");
                    }
                }
                else
                {
                    Message("���łɖ����Ă���c");
                }
                break;
            case "�p���[�V�F�A":
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
                    Message("���݂��̂��������ƂƂ������𕪂����������I");
                }
                else
                {
                    Message("���������܂������Ȃ�����");
                }
                break;
            case "�݂炢�悿":
                if (atkChara.master)
                {
                    if (field.enemy[8] == 0)
                    {
                        field.enemy[8] = 3;
                        field.enemy[9] = Mathf.FloorToInt(atkChara.RankStatus(3) * 100 * 2f / 5f);
                        if (atkChara.type1 == "�G�X�p�[" || atkChara.type2 == "�G�X�p�[") field.enemy[9] = Mathf.FloorToInt(field.enemy[9] * 1.5f);
                        yield return AreaIE("�����ɍU���������");
                    }
                    else
                    {
                        Message("���������܂������Ȃ�����");
                    }
                }
                else
                {
                    if (field.player[8] == 0)
                    {
                        field.player[8] = 3;
                        field.player[9] = Mathf.FloorToInt(atkChara.RankStatus(3) * 100 * 2f / 5f);
                        if (atkChara.type1 == "�G�X�p�[" || atkChara.type2 == "�G�X�p�[") field.enemy[9] = Mathf.FloorToInt(field.enemy[9] * 1.5f);
                        yield return AreaIE("�����ɍU���������");
                    }
                    else
                    {
                        Message("���������܂������Ȃ�����");
                    }
                }
                break;
            case "�߂�����":
                yield return atkChara.StatusCheak(3, 1);
                yield return atkChara.StatusCheak(4, 1);
                break;
            case "��߂���":
                damage = Mathf.FloorToInt(prevSkillDamage * 0.5f);
                yield return DamageIE( defChara, atkChara, atkChara.nameC + "�́@���肩��HP���z�������", -damage);
                break;
            case "���t���N�^�[":
                if (atkChara.master)
                {
                    if (field.player[4] == 0)
                    {
                        field.player[4] = 5;
                        if (ItemName(atkChara.item) == "�Ђ���̂˂��") field.player[4] += 3;
                        yield return AreaIE("�����U�����y������ǂ�������I");
                    }
                    else
                    {
                        Message("���t���N�^�[�͂��������Ă���");
                    }
                }
                else
                {
                    if (field.enemy[4] == 0)
                    {
                        field.enemy[4] = 5;
                        if (ItemName(atkChara.item) == "�Ђ���̂˂��") field.enemy[3] += 3;
                        yield return AreaIE("�����U�����y������ǂ�������I");
                    }
                    else
                    {
                        Message("���t���N�^�[�͂��������Ă���");
                    }
                }
                break;
            case "���イ����":
                damage = Mathf.FloorToInt(prevSkillDamage * 0.5f);
                yield return DamageIE( defChara, atkChara, atkChara.nameC + "�́@���肩��HP���z�������", -damage);
                break;
            case "���傤�̂܂�":
                yield return atkChara.StatusCheak(3, 1);
                yield return atkChara.StatusCheak(4, 1);
                yield return atkChara.StatusCheak(5, 1);
                break;
            case "�Ƃǂ߂΂�":
                if (defChara.error==8)
                {
                    yield return atkChara.StatusCheak(1, 1);
                    yield return atkChara.StatusCheak(2, 1);
                    yield return atkChara.StatusCheak(3, 1);
                    yield return atkChara.StatusCheak(4, 1);
                    yield return atkChara.StatusCheak(5, 1);
                }
                break;
            case "�Ƃт�����":
                yield return defChara.StatusCheak(1, -1);
                break;
            case "�Ƃ�ڂ�����":
                yield return EffectChange(atkChara.master, r => b = r);
                break;
            case "�˂΂˂΃l�b�g":
                if (!atkChara.master)
                {
                    if (field.player[12] == 0)
                    {
                        field.player[12] = 1;
                        yield return AreaIE("�������Ԃ𒣂����I");
                    }
                    else
                    {
                        Message("�˂΂˂΃l�b�g�͂��������Ă���");
                    }
                }
                else
                {
                    if (field.enemy[12] == 0)
                    {
                        field.enemy[12] = 1;
                        yield return AreaIE("�������Ԃ𒣂����I");
                    }
                    else
                    {
                        Message("�˂΂˂΃l�b�g�͂��������Ă���");
                    }
                }
                break;
            case "�͂���邢������":
                yield return defChara.StatusCheak(3, -1);
                break;
            case "�ڂ����債�ꂢ":
                yield return atkChara.StatusCheak(2, 1);
                yield return atkChara.StatusCheak(4, 1);
                break;
            case "�܂Ƃ���":
                if (defChara.error2[2] == 0)
                {
                    defChara.error2[2] = 4;
                    yield return EffectIE(atkChara, defChara, defChara.nameC + "�́@�Ƃ��ꂽ");
                }
                break;
            case "�~�T�C���΂�":
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
                Message((i + 1) + "�񂠂�����");
                break;
            case "�����₫":
                if (PerCorrect(atkChara.master, 10))
                {
                    yield return defChara.StatusCheak(4, -1);
                }
                break;
            case "��񂼂�����":
                atkChara.buffer[13] += 1;
                break;
            case "���J�f�ɂ񂰂�":
                if (defChara.error == 0)
                    if (PerCorrect(atkChara.master, 30))
                {
                    yield return defChara.ErrorCheak(6);
                }
                break;
            case "����Ȃ���":
                if (PerCorrect(atkChara.master, 30))
                {
                    defChara.error2[1] = 1;
                    yield return EffectIE(atkChara, defChara, defChara.nameC + "���@���܂����I");
                }
                break;
            case "���񂹂��ӂ���":
                yield return defChara.StatusCheak(5, -1);
                break;
            case "���񂵂̂�����":
                if (PerCorrect(atkChara.master, 10))
                {
                    yield return atkChara.StatusCheak(1, 1);
                    yield return atkChara.StatusCheak(2, 1);
                    yield return atkChara.StatusCheak(3, 1);
                    yield return atkChara.StatusCheak(4, 1);
                    yield return atkChara.StatusCheak(5, 1);
                }
                break;
            case "���낪��":
                if (atkChara.buffer[4] == 0) atkChara.buffer[4] = 5;
                yield return EffectIE(defChara, atkChara, "");
                break;
            case "�X�e���X���b�N":
                if (!atkChara.master)
                {
                    if (field.player[10] == 0)
                    {
                        field.player[10] = 1;
                        yield return AreaIE("�s���Ȋ��ݒu�����I");
                    }
                    else
                    {
                        Message("�X�e���X���b�N�͂��������Ă���");
                    }
                }
                else
                {
                    if (field.enemy[10] == 0)
                    {
                        field.enemy[10] = 1;
                        yield return AreaIE("�s���Ȋ��ݒu�����I");
                    }
                    else
                    {
                        Message("�X�e���X���b�N�͂��������Ă���");
                    }
                }
                break;
            case "���Ȃ��炵":
                if (field.both[0] != 4)
                {
                    field.both[0] = 4;
                    field.both[1] = 5;
                    if (ItemName(atkChara.item) == "�Ă񂫃f�b�L") field.both[1] += 3;
                    yield return AreaIE("�����������n�߂��I");
                }
                else
                {
                    Message("���łɍ����͐����Ă���");
                }
                break;
            case "����͂̂���":
                damage = Mathf.FloorToInt(prevSkillDamage * 0.5f);
                yield return DamageIE( defChara, atkChara, atkChara.nameC + "�́@�������󂯂�", damage);
                break;
            case "���b�N�J�b�g":
                yield return atkChara.StatusCheak(5, 1);
                break;
            case "���b�N�u���X�g":
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
                Message((i + 1) + "�񂠂�����");
                break;
            case "���e�I�r�[��":
                if (atkChara.buffer[8] == 0)
                {
                    atkChara.buffer[8] = 1;
                    yield return EffectIE(defChara, atkChara, "�f���̗͂𗭂߂Ă���I");
                    yield return atkChara.StatusCheak(3, 1);
                }
                else
                {
                    atkChara.buffer[8] = 0;
                    yield return EffectIE(defChara, atkChara, "");
                }
                break;
            case "���₵���Ђ���":
                if (defChara.error2[0] == 0)
                {
                    defChara.error2[0] = 2;
                    yield return EffectIE(atkChara, defChara, defChara.nameC + "�́@���������I");
                }
                else yield return EffectIE(atkChara, defChara, defChara.nameC + "�́@���łɍ������Ă���");
                break;
            case "���ǂ납��":
                if (PerCorrect(atkChara.master, 40))
                {
                    defChara.error2[1] = 1;
                    yield return EffectIE(atkChara, defChara, defChara.nameC + "���@���܂����I");
                }
                break;
            case "�V���h�[�p���`":
                if (atkChara.master) correctP[0] += 20;
                else correctE[0] += 20;
                break;
            case "�V���h�[�{�[��":
                if (PerCorrect(atkChara.master, 20))
                {
                    yield return defChara.StatusCheak(4, -1);
                }
                break;
            case "�̂낢":
                damage = Mathf.FloorToInt(atkChara.status[0] / 2f);
                if (atkChara.hp > damage && defChara.error2[3] == 0)
                {
                    defChara.error2[3] = 1;
                    yield return EffectIE(atkChara, defChara, "");
                    yield return DamageIE( defChara, atkChara, atkChara.nameC + "�́@HP������Ă̂낢��������", damage);
                }
                else Message("���������܂������Ȃ�����");
                break;
            case "�݂��Â�":
                atkChara.buffer[3] = 1;
                yield return EffectIE(defChara, atkChara, atkChara.nameC + "�́@������݂��Â�ɂ��悤�Ƃ��Ă���");
                break;
            case "������̂���":
                if (atkChara.master)
                {
                    if (field.player[3] == 0)
                    {
                        field.player[3] = 5;
                        if (ItemName(atkChara.item) == "�Ђ���̂˂��") field.player[3] += 3;
                        yield return AreaIE("����U�����y������ǂ�������I");
                    }
                    else
                    {
                        Message("������̂��ׂ͂��������Ă���");
                    }
                }
                else
                {
                    if (field.enemy[3] == 0)
                    {
                        field.enemy[3] = 5;
                        if (ItemName(atkChara.item) == "�Ђ���̂˂��") field.enemy[3] += 3;
                        yield return AreaIE("�����U�����y������ǂ�������I");
                    }
                    else
                    {
                        Message("������̂��ׂ͂��������Ă���");
                    }
                }
                break;
            case "�������":
                if (atkChara.buffer[4] == 0)
                {
                    atkChara.buffer[4] = 3;
                    yield return EffectIE(defChara, atkChara, "");
                }
                break;
            case "�_�u���`���b�v":
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
                Message((i + 1) + "�񂠂�����");
                break;
            case "�h���S���e�[��":
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
                if (!b) Message("���������܂������Ȃ�����");
                break;
            case "��イ��������":
                yield return atkChara.StatusCheak(3, -2);
                break;
            case "��イ�̂܂�":
                yield return atkChara.StatusCheak(1, 1);
                yield return atkChara.StatusCheak(5, 1);
                break;
            case "�����̂͂ǂ�":
                if (PerCorrect(atkChara.master, 20))
                {
                    defChara.error2[1] = 1;
                    yield return EffectIE(atkChara, defChara, defChara.nameC + "���@���܂����I");
                }
                break;
            case "�����Ȃ�":
                yield return defChara.StatusCheak(1, -1);
                yield return defChara.StatusCheak(2, -1);
                yield return defChara.StatusCheak(3, -1);
                yield return defChara.StatusCheak(4, -1);
                break;
            case "�����݂₰":
                yield return defChara.StatusCheak(1, -2);
                yield return defChara.StatusCheak(3, -2);
                yield return DamageIE( defChara, atkChara,"", 9999);
                break;
            case "���݂�����":
                if (PerCorrect(atkChara.master, 20))
                {
                    yield return defChara.StatusCheak(2, -1);
                }
                break;
            case "���ă[���t":
                yield return defChara.StatusCheak(1, -1);
                yield return defChara.StatusCheak(3, -1);
                yield return EffectChange(atkChara.master, r => b = r);
                break;
            case "���傤�͂�":
                if (defChara.error2[11] == 0)
                {
                    defChara.error2[11] = 4;
                    yield return EffectIE(atkChara, defChara, defChara.nameC + "�́@�ω��Z���o���Ȃ��Ȃ���");
                }
                else
                {
                    yield return EffectIE(atkChara, defChara, defChara.nameC + "�́@���łɂ��傤�͂���Ă���");
                }
                break;
            case "�߂Ƃ�":
                yield return atkChara.StatusCheak(1, 1);
                yield return atkChara.StatusCheak(6, 1);
                break;
            case "�ǂ�ڂ�":
                if(atkChara.item == 0 && defChara.item != 0)
                {
                    atkChara.item = defChara.item;
                    defChara.item = 0;
                    yield return EffectIE(atkChara, defChara, "");
                    yield return EffectIE(defChara, atkChara, "");
                    Message("�������̂�D��������I");
                }
                break;
            case "�o�[�N�A�E�g":
                yield return defChara.StatusCheak(3, -1);
                break;
            case "�͂������Ƃ�":
                if (defChara.item != 0)
                {
                    defChara.item = 0;
                    yield return EffectIE(atkChara, defChara, "");
                    Message("�������̂��͂����������I");
                }
                break;
            case "�Ђ����肩����":
                for (i = 0; i < 8; i++) {
                    defChara.rank[i] *= -1;
                    yield return EffectIE(atkChara, defChara, defChara.nameC + "�́@�\�͕ω����t�]�������I");
                }
                break;
            case "��邾����":
                yield return atkChara.StatusCheak(3, 2);
                break;
            case "�����܂̃L�b�X":
                yield return defChara.ErrorCheak(7);
                break;
            case "�Ƃڂ���":
                yield return atkChara.StatusCheak(3, 1);
                yield return atkChara.StatusCheak(2, 1);
                break;
            case "���������":
                if (defChara.error2[10] == 0)
                {
                    defChara.error2[10] = 1;
                    yield return EffectIE(atkChara, defChara, defChara.nameC + "�́@�����Z��A���ŏo���Ȃ��Ȃ���");
                }
                else
                {
                    yield return EffectIE(atkChara, defChara, defChara.nameC + "�́@���łɂ�������񂳂�Ă���");
                }
                break;
            case "�A�C�A���e�[��":
                if (PerCorrect(atkChara.master, 30))
                {
                    yield return defChara.StatusCheak(2, -1);
                }
                break;
            case "�A�C�A���w�b�h":
                if (PerCorrect(atkChara.master, 30))
                {
                    defChara.error2[1] = 1;
                    yield return EffectIE(atkChara, defChara, defChara.nameC + "���@���܂����I");
                }
                break;
            case "�M�A�\�[�T�[":
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
                Message((i + 1) + "�񂠂�����");
                break;
            case "���񂼂�����":
                yield return defChara.StatusCheak(4, -3);
                break;
            case "�R���b�g�p���`":
                if (PerCorrect(atkChara.master, 30))
                {
                    yield return atkChara.StatusCheak(1, 1);
                }
                break;
            case "�Ă��؂�":
                yield return atkChara.StatusCheak(2, 2);
                break;
            case "�͂��˂̂΂�":
                if (PerCorrect(atkChara.master, 20))
                {
                    yield return atkChara.StatusCheak(2, 1);
                }
                break;
            case "���^���N���[":
                if (PerCorrect(atkChara.master, 70))
                {
                    yield return atkChara.StatusCheak(1, 1);
                }
                break;
            case "���X�^�[�J�m��":
                if (PerCorrect(atkChara.master, 20))
                {
                    yield return defChara.StatusCheak(2, -1);
                }
                break;
            case "�Ղ������[":
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
                Message((i + 1) + "�񂠂�����");
                break;
            case "���܂���":
                yield return defChara.StatusCheak(1, -2);
                if(atkChara.sex != defChara.sex) yield return defChara.StatusCheak(3, -1);
                break;
            case "������":
                if (PerCorrect(atkChara.master, 10))
                {
                    yield return defChara.StatusCheak(1, -1);
                }
                break;
            case "�Ă񂵂̃L�b�X":
                if (defChara.error2[0] == 0)
                {
                    defChara.error2[0] = 2;
                    yield return EffectIE(atkChara, defChara, defChara.nameC + "�́@���������I");
                }
                else yield return EffectIE(atkChara, defChara, defChara.nameC + "�́@���łɍ������Ă���");
                break;
            case "�h���C���L�b�X":
                damage = Mathf.FloorToInt(prevSkillDamage * 0.75f);
                yield return DamageIE( defChara, atkChara, atkChara.nameC + "�́@���肩��HP���z�������", -damage);
                break;
            case "�}�W�J���V���C��":
                if (PerCorrect(atkChara.master, 30))
                {
                    yield return defChara.StatusCheak(3, -1);
                }
                break;
            case "�A�L�j�E����������":
                if (defChara.error2[4] == 0)
                {
                    defChara.error2[4] = 1;
                    yield return EffectIE(atkChara, defChara, defChara.nameC + "�́@���������ɂȂ����I");
                }
                else yield return EffectIE(atkChara, defChara, defChara.nameC + "�́@���łɃ����������I");
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
        return cs.nameC + "�́@" + cs.passiveName + "�I\n";
    }
    public string ItemText(CharaStatus cs)
    {
        return cs.nameC + "�́@" + ItemName(cs.item) + "�I\n";
    }
    public IEnumerator ItemUse(CharaStatus cs)
    {
        switch (cs.passiveName)
        {
            case "�Ó}���q":
                if (ItemName(cs.item).Contains("�̂�"))
                {
                    yield return DamageIE(cs.enemy,cs, PassiveText(cs)+cs.nameC + "�́@�񕜂����B",-Mathf.FloorToInt(cs.status[0]/4f));
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
                string t = "�݂���肪����ɍU�����󂯂�";
                if (defChara.buffer[0] <= 0)
                {
                    t += "\n" + "�݂���肪�����Ă��܂���";
                    prevSkillDamage += defChara.buffer[0];
                    defChara.buffer[0] = 0;
                }
                yield return EffectIE(atkChara, defChara, t);
                yield break;
            }
            else
            {
                if (skill.physics == "����") defChara.damageP = d;
                if (skill.physics == "����") defChara.damageS = d;

                Damage damage = new Damage();
                int tasuki = 0;
                if (d > defChara.hp && defChara.passiveName== "���V�z�M" && defChara.passiveBool == 0)
                {
                    defChara.passiveBool = 1;
                    d = defChara.hp - 1;
                    tasuki = 1;
                }

                if (defChara.hp == defChara.status[0]  && d>=defChara.hp  && ItemName(defChara.item) == "�������̃^�X�L")
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

                if(tasuki==1) Message( PassiveText(defChara)+"�m���ɂȂ�U����ς����I");
                if(tasuki==2) Message( ItemText(defChara) + "�m���ɂȂ�U����ς����I");


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
                    case "�N�\�U�R�p���`":
                        if (skill.contact > 0)
                        {
                            Message( PassiveText(defChara));
                            yield return atkChara.StatusCheak(1, -1);
                        }
                        break;
                    case "�C�`�S�~���N�E�~�E�V":
                        if (skill.contact > 0)
                        {
                            Message(PassiveText(defChara));
                            yield return atkChara.ErrorCheak(1);
                        }
                        break;
                    case "�����炵":
                        if (skill.contact > 0 && atkChara.sex =="����")
                        {
                            if (PerCorrect(defChara.master, 30))
                            {
                                Message(PassiveText(defChara));
                                if (atkChara.error2[4] == 0)
                                {
                                    atkChara.error2[4] = 1;
                                    yield return EffectIE(defChara, atkChara, atkChara.nameC + "�́@���������ɂȂ����I");
                                }
                                else yield return EffectIE(defChara, atkChara, atkChara.nameC + "�́@���łɃ����������I");
                            }
                        }
                        break;
                    case "�B��":
                        defChara.passiveBool = 1;
                        break;
                }
                switch (ItemName(atkChara.item))
                {
                    case "��������̂��邵":
                        if (PerCorrect(atkChara.master, 10))
                        {
                            defChara.error2[1] = 1;
                            yield return EffectIE(atkChara, defChara, ItemText(atkChara) + defChara.nameC + "���@���܂����I");
                        }
                        break;
                    case "���̂��̂���":
                        yield return DamageIE(defChara, atkChara, ItemText(atkChara) + "�_���[�W���󂯂�", Mathf.FloorToInt(atkChara.status[0] / 10f));
                        break;
                    case "��������̂���":
                        yield return DamageIE(defChara, atkChara, ItemText(atkChara) + "HP���z������", -Mathf.FloorToInt(d / 6f));
                        break;
                }
                switch (ItemName(defChara.item))
                {
                    case "�S�c�S�c���b�g":
                        if (skill.contact>0)
                        {
                            yield return DamageIE(defChara, atkChara, ItemText(defChara) + atkChara.nameC + "�́@�_���[�W���󂯂�", Mathf.FloorToInt(atkChara.status[0] / 6f));
                        }
                        break;
                    case "���Ⴍ�Ă�ق���":
                        if (typePer > 100 && defChara.error != 8)
                        {
                            Message(ItemText(defChara));
                            yield return defChara.StatusCheak(1, 2);
                            yield return defChara.StatusCheak(3, 2);
                            yield return ItemUse(defChara);
                        }
                        break;
                    case "��������{�^��":
                        if (defChara.error!=8)
                        {
                            Message(ItemText(defChara) + "�������莝���ɖ߂�");
                            yield return ItemUse(defChara);
                            bool b=false;
                            yield return EffectChange(defChara.master, r => b = r);
                            if (!b) Message("���������܂������Ȃ�����");
                        }
                        break;
                    case "�ӂ�����":
                        if (defChara.error != 8)
                        {
                            Message(ItemText(defChara) + "�ӂ����񂪂��Ă��܂���");
                            yield return ItemUse(defChara);
                        }
                        break;
                    case "���b�h�J�[�h":
                        if (defChara.error != 8)
                        {
                            Message(ItemText(defChara) + "������莝���ɖ߂�");
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
                            if (!b) Message("���������܂������Ȃ�����");
                            yield return ItemUse(defChara);
                            break;
                        }
                        break;
                    case "�����u�̂�":
                        if (skill.physics=="����")
                        {
                            yield return DamageIE(defChara, atkChara, ItemText(defChara) + atkChara.nameC + "�́@�_���[�W���󂯂�", Mathf.FloorToInt(atkChara.status[0] / 6f));
                            yield return ItemUse(defChara);
                        }
                        break;
                    case "�W���|�̂�":
                        if (skill.physics == "����")
                        {
                            yield return DamageIE(defChara, atkChara, ItemText(defChara) + atkChara.nameC + "�́@�_���[�W���󂯂�", Mathf.FloorToInt(atkChara.status[0] / 6f));
                            yield return ItemUse(defChara);
                        }
                        break;
                    case "�A�b�L�̂�":
                        if (skill.physics == "����")
                        {
                            Message(ItemText(defChara));
                            yield return defChara.StatusCheak(2, 1);
                            yield return ItemUse(defChara);
                        }
                        break;
                    case "�^���v�̂�":
                        if (skill.physics == "����")
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
            if(defChara.passiveName=="�J�V���b" && d < 0)
            {
                Message(PassiveText(defChara)+"�񕜗ʂ𑝉�������");
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
                        yield return EffectIE(atkChara, defChara, defChara.nameC + "�́@���������I");
                    }
                    else yield return EffectIE(atkChara, defChara, defChara.nameC + "�́@���łɍ������Ă���");
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
                yield return DamageIE( defChara, atkChara, defChara.nameC + "���@����𓹘A��ɂ���", 9999);
            }

            for(int i = 0; i < 3; i++)
            {
                if (playerMember[i].passiveName== "���p��" && defChara!= playerMember[i])
                {

                    if (playerMember[i] == battlerP)
                    {
                        yield return DamageIE(defChara, atkChara,"�Ƃ������́@���p���ŉ񕜂����I", -Mathf.FloorToInt(playerMember[i].status[0] / 5f));
                    }
                    else
                    {
                        playerMember[i].hp += Mathf.FloorToInt(playerMember[i].status[0] / 5f);
                        if (playerMember[i].hp > playerMember[i].status[0]) playerMember[i].hp = playerMember[i].status[0];
                    }
                }
                if (enemyMember[i].passiveName == "���p��" && defChara != enemyMember[i])
                {
                    if (enemyMember[i] == battlerE)
                    {
                        yield return DamageIE(defChara, atkChara, "�Ƃ������́@���p���ŉ񕜂����I", -Mathf.FloorToInt(playerMember[i].status[0] / 5f));
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
            case "���C���z��":
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
            case "�I�{���̂�":
                if (defChara.HPPer()<=0.5f && d>0)
                {
                    int damage = -Mathf.FloorToInt(defChara.status[0] / 4f);
                    text = ItemText(defChara) + defChara.nameC + "�́@�񕜂���";
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
            case "���j�u���[�V�����L���O�v":
                if (atkChara.error2[1] > 0)
                {
                    atkChara.error2[1] = 0;
                    yield return EffectIE(atkChara, defChara, PassiveText(defChara)+"�Ђ�݂���񕜂���");
                }
                break;
        }
        switch (ItemName(defChara.item))
        {
            case "�����̂�":
                if (defChara.error!=0 && defChara.error !=8)
                {
                    defChara.error = 0;
                    defChara.errorTurn = 0;
                    yield return EffectIE(atkChara,defChara, ItemText(defChara) + "��Ԉُ���񕜂���");
                    yield return ItemUse(defChara);
                }
                break;
            case "�����^���n�[�u":
                if (defChara.error != 8�@&&(defChara.error2[0]>0 || defChara.error2[4] > 0 || defChara.error2[9] > 0 || defChara.error2[10] > 0 || defChara.error2[11] > 0 || defChara.error2[13] > 0))
                {
                    defChara.error2[0] = 0;
                    defChara.error2[4] = 0;
                    defChara.error2[9] = 0;
                    defChara.error2[10] = 0;
                    defChara.error2[11] = 0;
                    defChara.error2[13] = 0;
                    yield return EffectIE(atkChara, defChara, ItemText(defChara) + "��Ԉُ���񕜂���");
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
                yield return EffectIE(battler.enemy, battler, battler.nameC + "�́@���������ŏ�𗣂�悤�Ƃ��Ȃ��I");
                battler.error2[4] = 0;
                yield return EffectIE(battler.enemy, battler, battler.nameC + "�́@��ɕԂ���");
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
            string text = cs.nameC + "�́@���₵�̂˂������󂯂ĉ񕜂���";
            cs.error = 0;
            cs.errorTurn = 0;
            fieldPE[7] = 0;
            yield return EffectIE(cs.enemy, cs, "");
            yield return DamageIE( cs.enemy, cs, text, -damage);
        }
        if (fieldPE[10] > 0)
        {
            int typePer = Type.Per("����", cs.type1, cs.type2);
            int damage = Mathf.FloorToInt(cs.status[0] / 16f * typePer / 100);
            string text = cs.nameC + "�́@�X�e���X���b�N�Ń_���[�W���󂯂�";
            yield return DamageIE( cs.enemy, cs, text, damage);
        }
        if (fieldPE[11] == 1)
        {
            if (cs.type1 != "�Ђ���" && cs.type2 != "�Ђ���" && cs.type1 != "�ǂ�" && cs.type2 != "�ǂ�" && ItemName(cs.item)!="�ӂ�����")
            {
                Message(cs.nameC + "�́@�ǂ��т��𓥂�");
                yield return cs.ErrorCheak(1);
            }
        }
        if (fieldPE[11] >= 2)
        {
            if (cs.type1 != "�Ђ���" && cs.type2 != "�Ђ���" && cs.type1 != "�ǂ�" && cs.type2 != "�ǂ�" && ItemName(cs.item) != "�ӂ�����")
            {
                Message(cs.nameC + "�́@�����ǂ��т��𓥂�");
                yield return cs.ErrorCheak(2);
            }
        }
        if (fieldPE[12] > 0)
        {
            if (cs.type1 != "�Ђ���" && cs.type2 != "�Ђ���" && ItemName(cs.item) != "�ӂ�����")
                Message(cs.nameC+"�́@�˂΂˂΃l�b�g�ɂ��������I");
                yield return cs.StatusCheak(5, -1);
        }
        if (battler != null)
        {
            switch (battler.passiveName)
            {
                case "�v���f���[�X":
                    Message(PassiveText(battler));
                    yield return cs.StatusCheak(6, 1);
                    break;
            }
        }
        switch (cs.passiveName)
        {
            case "���΂���":
                if (turn != 1)
                {
                    Message(PassiveText(cs));
                    if (cs.enemy.error != 8)
                    {
                        if (cs.enemy.type1 != "�S�[�X�g" && cs.enemy.type2 != "�S�[�X�g") yield return cs.enemy.StatusCheak(1, -1);
                        else yield return cs.StatusCheak(1, -1);
                    }
                    else Message("���������܂������Ȃ�����");
                }
                break;
            case "�����Ƃ��b��������":
                if (turn != 1)
                {
                    cs.error2[12] += 2;
                    cs.enemy.error2[12] += 2;
                    yield return EffectIE(cs.enemy, cs, "");
                    yield return EffectIE(cs, cs.enemy, PassiveText(cs) + "���݂���2�^�[���̊Ԃɂ����Ȃ���Ԃɂ���");
                }
                break;
            case "�يE�̔�":
                if (turn != 1)
                {
                    if (field.both[5] == 0)
                    {
                        field.both[5] = 3;
                        yield return AreaIE("�يE�̔����J���ꂽ");
                    }
                    else
                    {
                        Message("�يE�̔��͂��łɊJ����Ă���");
                    }
                }
                break;
        }
        if (battler != null)
        {
            switch (cs.enemy.passiveName)
            {
                case "������́H":
                        Message(PassiveText(cs.enemy));
                        yield return cs.enemy.StatusCheak(3, 1);
                    break;
            }
        }
        switch (ItemName(cs.item))
        {
            case "�ӂ�����":
                if (turn != 1)
                {
                    Message(ItemText(cs) + cs.nameC + "�́@�����Ă���");
                }
                break;
        }
    }
    IEnumerator FirstSummonPassive(CharaStatus cs)
    {
        switch (cs.passiveName)
        {
            case "���΂���":
                Message(PassiveText(cs));
                if (cs.enemy.error != 8)
                {
                    if (cs.enemy.type1 != "�S�[�X�g" && cs.enemy.type2 != "�S�[�X�g") yield return cs.enemy.StatusCheak(1, -1);
                    else yield return cs.StatusCheak(1, -1);
                }
                else Message("���������܂������Ȃ�����");
                break;
            case "���C�g�j���O�E�Q�C�{���O":
                Message(PassiveText(cs));
                yield return cs.StatusCheak(1, 1);
                break;
            case "����܂�Ȃ���":
                Message(PassiveText(cs) + cs.nameC + "�́@�Q�V���Ă���I");
                yield return cs.ErrorCheak(7);
                break;
            case "�يE�̔�":
                if (field.both[5] == 0)
                {
                    field.both[5] = 3;
                    yield return AreaIE("�يE�̔����J���ꂽ");
                }
                else
                {
                    Message("�يE�̔��͂��łɊJ����Ă���");
                }
                break;
        }
        switch (ItemName(cs.item))
        {
            case "�ӂ�����":
                Message(ItemText(cs) + cs.nameC + "�́@�����Ă���");
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
        if (battler.error2[9] > 0 || battler.buffer[4]> 0 || ItemName(battler.item).Contains("�������"))
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
        if (battler.error2[11]>0 || ItemName(battler.item)== "�Ƃ����`���b�L")
        {
            for (int i = 0; i < 4; i++)
            {
                if (skillMaster.SkillList.Find(a => a.no == battler.skill[i]).physics == "�ω�") select[i] = false;
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
                if (prevSkill.name == "���炦��" || prevSkill.name == "�܂���" || prevSkill.name == "�j�[�h���K�[�h" || prevSkill.name == "�݂��Â�") select[battler.prevSkill] = false;
            }
        }
        switch(battler.passiveName)
        {
            case "����������":
                for (int i = 0; i < 4; i++)
                {
                    Skill skill = skillMaster.SkillList.Find(a => a.no == battler.skill[i]);
                    if (skill.type == "����") select[i] = false;
                }
                break;
            case "���^(�ɂ�����)":
                for (int i = 0; i < 4; i++)
                {
                    Skill skill = skillMaster.SkillList.Find(a => a.no == battler.skill[i]);
                    if (skill.type == "����") select[i] = false;
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
    public int error = 0; //0�Ȃ� 1�ǂ� 2�ǂ��ǂ� 3�܂� 4�₯�� 5������ 6������� 7�˂ނ� 8�Ђ�
    public int errorTurn = 0;
    public int[] error2 = new int[14] ;//0������ 1�Ђ�� 2�o�C���h 3�̂낢 4�������� 5��ǂ肬 6�˂ނ� 7�ق�� 8�Ƃ��������� 9�A���R�[�� 10��������� 11���傤�͂� 12�ɂ����Ȃ� 13���Ȃ��΂�
    public int lockSkill = -1;
    public int[] buffer =new int[14];//0�݂���� 1�܂��� 2�܂邭 3�݂��Â� 4���΂�� 5���イ�ł� 6�����킦�� 7���炦�� 8���߂� 9���킮 10�X�^�� 11�j�[�h���K�[�h 12�͂˂₷�� 13��񂼂�

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
        if (i == 4 && manager.field.both[0] == 4 && (type1=="����"||type2=="����") ) f *= 1.5f;
        if (i == 2 && manager.field.both[0] == 3 && (type1 == "������" || type2 == "������")) f *= 1.5f;
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
        if (i == 5 && manager.ItemName(item) == "�������X�J�[�t") f *= 1.5f;
        if (i == 4 && manager.ItemName(item) == "�Ƃ����`���b�L") f *= 1.5f;
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
            case "����܂�Ȃ���":
                status[4] = Mathf.FloorToInt(status[4]*1.5f);
                break;
            case "��������Ă�":
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
                    yield return manager.AreaIE("�u��������Ă�v");
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
            case "���������݂܂���":
                if (error != 8)
                {
                    hp += Mathf.FloorToInt(status[0] / 6f);
                    if (hp > status[0]) hp = status[0];
                }
                break;
            case "�B��":
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
                if(battle) yield return manager.EffectIE(enemy, this, nameC + "�́@�����肪�n����!");
            }
        }
        switch (passiveName)
        {
            case "�Â̕����q":
                if(manager.battlerE.sex=="�j��"&& manager.battlerP.sex == "�j��")
                {
                    hp += Mathf.FloorToInt(status[0] / 10f);
                    if (hp > status[0]) hp = status[0];
                    error = 0;
                    errorTurn = 0;
                }
                break;
            case "�ϕ�":
                if (manager.turn <= 10)
                {
                    status[3] += 10;
                    if (swap[3] != 0) swap[3] += 10;
                }
                break;
            case "�ӎ�":
                if (manager.turn%3==0)
                {
                    if (battle)
                    {
                        yield return manager.DamageIE(enemy, this, manager.PassiveText(this) + "HP���@�񕜂���", -Mathf.FloorToInt(status[0] / 10f));
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
                case "�J���J���J���J���I�I":
                    if (i == 7)
                    {
                        manager.Message(manager.PassiveText(this) + "�˂ނ��ԂɂȂ�Ȃ��I");
                        yield break;
                    }
                    break;
                case "�悤�����f���X��":
                    if (i == 7)
                    {
                        manager.Message(manager.PassiveText(this) + "�˂ނ��ԂɂȂ�Ȃ��I");
                        yield break;
                    }
                    break;
            }

            if ((master && manager.field.player[6] > 0) || (!master && manager.field.enemy[6] > 0))
            {
                manager.Message("���낢���肪��Ԉُ��h�����I");
                yield break;
            }
            if (i == 7 && (buffer[9] > 0 || enemy.buffer[9] > 0))
            {
                effect = ToEffect();
                effect.message = "���킮���ł˂ނ邱�Ƃ��ł��Ȃ��I";
                manager.punBattle.Send("effect", effect.ToString());
                yield break;
            }
            error = i;
            if (error == 2) errorTurn = 0;
            if (error == 5) errorTurn = 4;
            if (error == 7)
            {
                errorTurn = 3;
                if(enemy.passiveName == "�悤�����f���X��") 
                {

                    manager.Message(manager.PassiveText(this) + "�˂ނ�̌��ʂ����߂�I");
                    errorTurn = 4;
                }
            }
            string errorName = "";
            if (error == 1) errorName = "�ǂ�";
            if (error == 2) errorName = "�����ǂ�";
            if (error == 3) errorName = "�܂�";
            if (error == 4) errorName = "�₯��";
            if (error == 5) errorName = "������";
            if (error == 6) errorName = "�������";
            if (error == 7) errorName = "�˂ނ�";

            yield return manager.EffectIE(enemy, this,nameC+"�́@"+errorName+"�ɂȂ���",errorName);
        }
        else
        {
            manager.Message(nameC+"�́@���ɏ�Ԉُ킾");
        }
    }
    public IEnumerator StatusCheak(int i,int add)
    {
        string text = "";
        int prev = rank[i];

        if (i == 1) text = "��������";
        if (i == 2) text = "�ڂ�����";
        if (i == 3) text = "�Ƃ�����";
        if (i == 4) text = "�Ƃ��ڂ�";
        if (i == 5) text = "���΂₳";
        if (i == 6) text = "�߂����イ";
        if (i == 7) text = "������";

        if (add > 0)
        {
            if (rank[i] == 6)
            {
                yield return manager.EffectIE(enemy, this, nameC + "�� " + text + "�͂����オ��Ȃ�");
                yield break;
            }
            
        }
        if (add < 0)
        {
            if ((master && manager.field.player[6] > 0) || (!master && manager.field.enemy[6] > 0))
            {
                manager.Message("���낢���肪�\�͒ቺ��h�����I");
                yield break;
            }
            if (rank[i] == -6)
            {
                yield return manager.EffectIE(enemy, this, nameC + "�� " + text + "�͂���������Ȃ�");
                yield break;
            }
        }
        rank[i] += add;
        if (rank[i] > 6) rank[i] = 6;
        if (rank[i] < -6) rank[i] = -6;

        if (add > 0) yield return manager.EffectIE(enemy, this, nameC + "�� " + text + "��" + (rank[i]-prev) + "�i�K�オ�����I","up");
        if (add < 0) yield return manager.EffectIE(enemy, this, nameC + "�� " + text + "��" + (prev-rank[i]) + "�i�K��������","down");
    } 
    public IEnumerator TurnEndEffect()
    {
        damageP = 0;
        damageS = 0;
        if (buffer[7] > 0)
        {
            buffer[7] = 0;
        }
        //0�Ȃ� 1�ǂ� 2�ǂ��ǂ� 3�܂� 4�₯�� 5������ 6������� 7�˂ނ� 8�Ђ�
        if (error == 1)
        {
            int damage =  Mathf.FloorToInt(status[0] / 8f);
            string text = nameC + "�́@�ǂ��Ń_���[�W���󂯂�";
            yield return manager.DamageIE( null, this, text, damage);
        }
        if (error == 2)
        {
            errorTurn += 1;
            int damage = Mathf.FloorToInt(errorTurn * status[0] / 16f);
            string text = nameC + "�́@�����ǂ��Ń_���[�W���󂯂�";
            yield return manager.DamageIE( null, this, text, damage);
        }
        if (error == 4)
        {
            int damage = Mathf.FloorToInt(status[0] / 16f);
            string text = nameC + "�́@�₯�ǂŃ_���[�W���󂯂�";
            yield return manager.DamageIE( null, this, text, damage);
        }
        //0������ 1�Ђ�� 2�o�C���h 3�̂낢 4�������� 5��ǂ肬 6�˂ނ� 7�ق�� 8�Ƃ��������� 9�A���R�[�� 10��������� 11���傤�͂� 12�ɂ����Ȃ�
        if (error2[1] > 0)
        {
            error2[1] = 0;
        }
        if (error2[2] > 0)
        {
            error2[2] -= 1;
            int damage = Mathf.FloorToInt(status[0] / 16f);
            string text = nameC + "�́@�S���Ń_���[�W���󂯂�";
            yield return manager.DamageIE( null, this, text, damage);
        }
        if(error2[3] > 0)
        {
            int damage = Mathf.FloorToInt(status[0] / 4f);
            string text = nameC + "�́@�̂낢�Ń_���[�W���󂯂�";
            yield return manager.DamageIE( null, this, text, damage);
        }
        if (error2[3] > 0)
        {
            int damage = Mathf.FloorToInt(status[0] / 4f);
            string text = nameC + "�́@�̂낢�Ń_���[�W���󂯂�";
            yield return manager.DamageIE( null, this, text, damage);
        }
        if (error2[3] > 0)
        {
            int damage = Mathf.FloorToInt(status[0] / 4f);
            string text = nameC + "�́@�̂낢�Ń_���[�W���󂯂�";
            yield return manager.DamageIE( null, this, text, damage);
        }
        if (error2[5] > 0)
        {
            int damage = Mathf.FloorToInt(status[0] / 8f);
            if (hp < damage) damage = hp;
            string text = nameC + "�́@��ǂ肬�̂��˂ɗ͂��z��ꂽ";
            yield return manager.DamageIE( null, this, text, damage);

            text = enemy.nameC + "�́@��ǂ肬�̂��˂ŉ񕜂���";
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
            yield return manager.EffectIE(enemy, this, nameC + "�́@�ق�т̃J�E���g���i��");
            if (error2[7] == 0)
            {
                int damage = 9999;
                string text = nameC + "�́@�ق�т̃J�E���g��0�ɂȂ����I";
                yield return manager.DamageIE( enemy, this, text, damage);
            }
        }
        if (error2[9] > 0)
        {
            error2[9] -= 1;
            yield return manager.EffectIE(enemy, this, "");
            if (error2[9] == 0)
            {
                yield return manager.EffectIE(enemy, this, nameC + "�́@�A���R�[�����������I");
            }
        }
        if (error2[11] > 0)
        {
            error2[11] -= 1;
            yield return manager.EffectIE(enemy, this, "");
            if (error2[11] == 0)
            {
                yield return manager.EffectIE(enemy, this, nameC + "�́@�������������I");
            }
        }
        if (error2[13] > 0)
        {
            error2[13] -= 1;
            yield return manager.EffectIE(enemy, this, "");
            if (error2[13] == 0)
            {
                yield return manager.EffectIE(enemy, this, nameC + "�́@�����肩�������ꂽ�I");
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
                yield return manager.EffectIE(enemy, this, nameC + "�́@�o�[�T�N��Ԃ���񕜂����I");
            }
        }
        if (buffer[9] > 0)
        {
            buffer[9] -= 1;
            yield return manager.EffectIE(enemy, this, "");
            if (buffer[9] == 0)
            {
                yield return manager.EffectIE(enemy, this, nameC + "�́@���킮�̂���߂�");
            }
        }
        switch (passiveName)
        {
            case "�J���J���J���J���I�I":
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
                            manager.Message(manager.PassiveText(this) + "���Ԃ��˂ނ肩��N�������I");
                        }
                        else
                        {
                            manager.Message(manager.PassiveText(this) + "���Ԃ̂˂ނ��Z�������I");
                        }
                    }
                }
                break;
            case "�B��":
                if (passiveBool == 1 && error!=8){
                    yield return manager.AreaIE(manager.PassiveText(this) + "�莝���֖߂낤�Ƃ���I");
                    bool b = false ;
                    yield return manager.EffectChange(master, r => b = r);
                    if (!b) manager.Message("���������܂������Ȃ�����");
                }
                break;
            case "�g���b�v�^���[":
                if (error != 8 && item == 0)
                {
                    int i = UnityEngine.Random.Range(100,107);
                    item = i;
                    yield return manager.EffectIE(enemy,this,manager.PassiveText(this) +  manager.ItemName(item) + "���@���n����");
                }
                break;
        }
        switch (manager.ItemName(item))
        {
            case "���ׂ̂���":
                int damage = -Mathf.FloorToInt(status[0] / 16f);
                string text = manager.ItemText(this) + nameC + "�́@�񕜂���";
                yield return manager.DamageIE(null, this, text, damage);
                break;
            case "���낢�n�[�u":
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
                    yield return manager.EffectIE(enemy, this, manager.ItemText(this) + "���������\�͂����ɖ߂���");
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
                yield return manager.EffectIE(enemy, this, nameC + "�́@���ł���悤�ɂȂ����I");
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
    public int[] both = new int[6];//0�V�C(0����1���Ƃ�2�J3�����4���Ȃ��炵) 1�V�C�^�[�� 2�g���� 3�d�� 4���܂�������
    public int[] player = new int[13];//0�������� 1�˂������� 2�I�[���� 3������̂��� 4���t���N�^�[ 5�A�N�A�����O 6���낢���� <> 10�X�e�� 11�ǂ��т� 12�˂΂˂�
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
                bool type1 = manager.battlerP.type1 == "������";
                bool type2 = manager.battlerP.type1 == "������";
                if (type1 || type2)
                {
                    int damage = Mathf.FloorToInt(manager.battlerP.status[0] / 16f);
                    string text = manager.battlerP.nameC + "�́@�����Ń_���[�W���󂯂�";
                    yield return manager.DamageIE( manager.battlerE, manager.battlerP, text, damage);
                }
                type1 = manager.battlerE.type1 == "������";
                type2 = manager.battlerE.type1 == "������";
                if (type1 || type2)
                {
                    int damage = Mathf.FloorToInt(manager.battlerE.status[0] / 16f);
                    string text = manager.battlerE.nameC + "�́@�����Ń_���[�W���󂯂�";
                    yield return manager.DamageIE( manager.battlerP, manager.battlerE, text, damage);
                }
            }
            if (both[0] == 4)
            {
                bool type1 = manager.battlerP.type1 == "����" || manager.battlerP.type1 == "�͂���" || manager.battlerP.type1 == "���߂�";
                bool type2 = manager.battlerP.type1 == "����" || manager.battlerP.type1 == "�͂���" || manager.battlerP.type1 == "���߂�";
                if (type1 || type2)
                {
                    int damage = Mathf.FloorToInt(manager.battlerP.status[0] / 16f);
                    string text = manager.battlerP.nameC + "�́@���Ȃ��炵�Ń_���[�W���󂯂�";
                    yield return manager.DamageIE( manager.battlerE, manager.battlerP, text, damage);
                }
                type1 = manager.battlerP.type1 == "����" || manager.battlerP.type1 == "�͂���" || manager.battlerP.type1 == "���߂�";
                type2 = manager.battlerP.type1 == "����" || manager.battlerP.type1 == "�͂���" || manager.battlerP.type1 == "���߂�";
                if (type1 || type2)
                {
                    int damage = Mathf.FloorToInt(manager.battlerE.status[0] / 16f);
                    string text = manager.battlerE.nameC + "�́@���Ȃ��炵�Ń_���[�W���󂯂�";
                    yield return manager.DamageIE( manager.battlerP, manager.battlerE, text, damage);
                }
            }
            if (both[1] == 0)
            {
                string text = "";
                if (both[0] == 1) text = "���Ƃ肪�~��";
                if (both[0] == 2) text = "�J���~��";
                if (both[0] == 3) text = "����ꂪ�~��";
                if (both[0] == 4) text = "���Ȃ��炵���~��";
                both[0] = 0;
                yield return manager.AreaIE(text);
            }
        }
        if (both[2] > 0)
        {
            both[2] -= 1;
            if (both[2] == 0) yield return manager.AreaIE("�G���A�̘c�݂�������");
        }
        if (both[3] > 0)
        {
            both[3] -= 1;
            if (both[3] == 0) yield return manager.AreaIE("�d�͂����ɖ߂���");
        }
        if (both[4] > 0)
        {
            both[4] -= 1;
            if (both[4] == 0) yield return manager.AreaIE("���܂������肪������");
        }
        yield return new WaitForSeconds(0.1f);

        //0�������� 1�˂������� 2�I�[���� 3������̂��� 4���t���N�^�[ 5�A�N�A�����O 6���낢���� <> 10�X�e�� 11�ǂ��т� 12�˂΂˂�

        if (player[0] > 0)
        {
            player[0] -= 1;
            if(player[0] == 0)
            {
                yield return manager.AreaIE("�������������");
            }
        }
        if (enemy[0] > 0)
        {
            enemy[0] -= 1;
            if (enemy[0] == 0)
            {
                yield return manager.AreaIE("�������������");
            }
        }
        if (player[1] > 0)
        {
            player[1] -= 1;
            if (player[1] == 0)
            {
                int damage = Mathf.FloorToInt(manager.battlerP.status[0] / 2f);
                string text = manager.battlerP.nameC + "�́@�肢���������Č��C�ɂȂ����I";
                yield return manager.DamageIE( manager.battlerE, manager.battlerP, text, damage);
            }
        }
        if (enemy[1] > 0)
        {
            enemy[1] -= 1;
            if (enemy[1] == 0)
            {
                int damage = Mathf.FloorToInt(manager.battlerE.status[0] / 2f);
                string text = manager.battlerE.nameC + "�́@�肢���������Č��C�ɂȂ����I";
                yield return manager.DamageIE( manager.battlerP, manager.battlerE, text, damage);
            }
        }
        if (player[3] > 0)
        {
            player[3] -= 1;
            if (player[3] == 0) yield return manager.AreaIE("������̂��ׂ�������");
        }
        if (enemy[3] > 0)
        {
            enemy[3] -= 1;
            if (enemy[3] == 0) yield return manager.AreaIE("������̂��ׂ�������");
        }
        if (player[4] > 0)
        {
            player[4] -= 1;
            if (player[4] == 0) yield return manager.AreaIE("���t���N�^�[��������");
        }
        if (enemy[4] > 0)
        {
            enemy[4] -= 1;
            if (enemy[4] == 0) yield return manager.AreaIE("���t���N�^�[��������");
        }
        if (player[5] > 0)
        {
            player[5] -= 1;
            int damage = Mathf.FloorToInt(-manager.battlerP.status[0] / 16f);
            string text = manager.battlerP.nameC + "�́@�A�N�A�����O�ɖ����ꂽ";
            yield return manager.DamageIE( manager.battlerE, manager.battlerP, text, damage);
            if (player[5] == 0) yield return manager.AreaIE("�A�N�A�����O��������");
        }
        if (enemy[5] > 0)
        {
            enemy[5] -= 1;
            int damage = Mathf.FloorToInt(-manager.battlerE.status[0] / 16f);
            string text = manager.battlerE.nameC + "�́@�A�N�A�����O�ɖ����ꂽ";
            yield return manager.DamageIE( manager.battlerP, manager.battlerE, text, damage);
            if (enemy[5] == 0) yield return manager.AreaIE("�A�N�A�����O��������");
        }
        if (player[8] > 0)
        {
            player[8] -= 1;
            if (player[8] == 0)
            {
                int damage = Mathf.FloorToInt(player[9] / manager.battlerP.RankStatus(4));
                yield return manager.DamageIE( manager.battlerE, manager.battlerP, manager.battlerP.nameC + "�́@�݂炢�悿�̃_���[�W���󂯂�",damage);
            }
        }
        if (enemy[8] > 0)
        {
            enemy[8] -= 1;
            if (enemy[8] == 0)
            {
                int damage = Mathf.FloorToInt(enemy[9] / manager.battlerE.RankStatus(4));
                yield return manager.DamageIE( manager.battlerP, manager.battlerE, manager.battlerE.nameC + "�́@�݂炢�悿�̃_���[�W���󂯂�", damage);
            }
        }
        yield return manager.AreaIE("");
    }
    public IEnumerator TurnEndAfter()
    {
        if (both[5] > 0)
        {
            both[5] -= 1;
            if (both[5] == 0) yield return manager.AreaIE("�يE�̔�������ꂽ");
        }
    }
}