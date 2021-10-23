using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using BattleJson;
using KanKikuchi.AudioManager;

public class BattlePlayer : MonoBehaviourPunCallbacks
{
    public string partyNo;
    public List<Image> playerParty;
    public List<Image> enemyParty;
    public CharaMaster charaMaster;
    public SkillMaster skillMaster;
    public ItemMaster itemMaster;
    public PunBattle punBattle;
    public List<MemberSelect> member;
    public Button memberOK;
    public StatusDisplay statusDisplay;
    public GameObject matching;
    public Text matchingTime;
    public Button matchingButton;
    public GameObject selectWait;
    public GameObject partyDisplay;
    public BattleDisplay battleDisplay;
    public GameObject selectPanel;
    public GameObject commonPanel;
    public GameObject skillPanel;
    public GameObject memberPanel;
    public Text message;
    public BattleSkill battleSkill;
    public BattleMember battleMember;
    List<Process> processList = new List<Process>();
    bool selectSwitch = false;
    int[] decideMember = new int[3];
    public GameObject memberBackButton;
    string enemyName;
    string enemyTitle;
    string enemyAvator;
    public Image enemyImage;
    void Start()
    {
        partyNo = PlayerPrefs.GetString("party", "A");
        for (int i = 0; i < 6; i++)
        {
            if (PlayerPrefs.GetInt(partyNo + "P" + i, 0) != 0)
            {
                Chara chara = charaMaster.CharaList.Find(a => a.no == PlayerPrefs.GetInt(partyNo + "P" + i, 0));
                StartCoroutine(ImageLoad(playerParty[i], chara.name + "F"));
                StartCoroutine(ImageLoad(member[i].image, chara.name + "F"));
            }
        }
        PhotonNetwork.ConnectUsingSettings();
        StartCoroutine(MatcingTimeIE());
    }
    IEnumerator PlayerIE()
    {
        while (true)
        {
            if (processList.Count > 0)
            {
                switch (processList[0].process)
                {
                    case "select":
                        selectSwitch = true;
                        commonPanel.SetActive(true);
                        memberBackButton.SetActive(true);
                        processList.RemoveAt(0);
                        break;
                    case "summon":
                        Summon summon = Summon.ToSummon(processList[0].content);
                        yield return battleDisplay.SummonIE(summon.player == PhotonNetwork.IsMasterClient, summon);
                        processList.RemoveAt(0);
                        break;
                    case "damage":
                        Damage damage = Damage.ToDamage(processList[0].content);
                        yield return battleDisplay.DamageIE(damage.player == PhotonNetwork.IsMasterClient, damage);
                        processList.RemoveAt(0);
                        break;
                    case "change":
                        selectSwitch = true;
                        memberBackButton.SetActive(false);
                        MemberChange();
                        processList.RemoveAt(0);
                        break;
                    case "death":
                        yield return battleDisplay.DeathIE(bool.Parse(processList[0].content) == PhotonNetwork.IsMasterClient);
                        processList.RemoveAt(0);
                        break;
                    case "effect":
                        Effect effect = Effect.ToEffect(processList[0].content);
                        yield return battleDisplay.EffectIE(effect.player == PhotonNetwork.IsMasterClient,effect);
                        processList.RemoveAt(0);
                        break;
                    case "area":
                        Area area = Area.ToArea(processList[0].content);
                        yield return battleDisplay.AreaIE(PhotonNetwork.IsMasterClient, area);
                        processList.RemoveAt(0);
                        break;
                    case "turn":
                        battleDisplay.Turn(processList[0].content);
                        processList.RemoveAt(0);
                        break;
                    case "win":
                        for (int i = 0; i < 6; i++)
                        {
                            Chara c = charaMaster.CharaList.Find(a => a.no == PlayerPrefs.GetInt(partyNo + "P" + i, 0));
                            PlayerPrefs.SetInt(c.name + "win", PlayerPrefs.GetInt(c.name + "win", 0)+1);
                            if(member[i].selectNum!=0) PlayerPrefs.SetInt(c.name + "win", PlayerPrefs.GetInt(c.name + "win", 0) + 1);
                        }
                        yield return battleDisplay.Win();
                        yield break;
                    case "lose":
                        yield return battleDisplay.Lose();
                        yield break;
                    default:
                        Debug.Log("processError:"+ processList[0].process);
                        break;
                }
            }
            yield return new WaitForSecondsRealtime(0.1f);
        }
    }
    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinRandomRoom();
        //PhotonNetwork.JoinOrCreateRoom("Text", new RoomOptions { MaxPlayers = 2 }, TypedLobby.Default);
    }
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        matchingButton.interactable=false;
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.Instantiate("PunBattle", new Vector3(0, 0, 0), Quaternion.identity).GetComponent<PunBattle>();
            PhotonNetwork.CurrentRoom.IsOpen = false;
        }
    }
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        var roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 2;

        PhotonNetwork.CreateRoom(null, roomOptions);
    }
    public void PartySend()
    {
        string playerName = PlayerPrefs.GetString("name", "player");
        string playerTitle = PlayerPrefs.GetString("title", "èâêSé“");
        string playerAvator = PlayerPrefs.GetString("avator", "îƒóp6B");

        int[] chara = new int[6];
        int[] item = new int[6];
        int[] passive = new int[6];
        int[] exp0 = new int[6];
        int[] exp1 = new int[6];
        int[] exp2 = new int[6];
        int[] exp3 = new int[6];
        int[] exp4 = new int[6];
        int[] exp5 = new int[6];
        string[] skill0 = new string[6];
        string[] skill1 = new string[6];
        string[] skill2 = new string[6];
        string[] skill3 = new string[6];

        for (int i = 0; i < 6; i++)
        {
            Chara c = charaMaster.CharaList.Find(a => a.no == PlayerPrefs.GetInt(partyNo + "P" + i, 0));
            chara[i] = c.no;
            item[i] = PlayerPrefs.GetInt(partyNo + c.no + "I");
            passive[i] = PlayerPrefs.GetInt(partyNo + c.no + "P",1);
            exp0[i] = PlayerPrefs.GetInt(partyNo + c.no + "E0", 0);
            exp1[i] = PlayerPrefs.GetInt(partyNo + c.no + "E1", 0);
            exp2[i] = PlayerPrefs.GetInt(partyNo + c.no + "E2", 0);
            exp3[i] = PlayerPrefs.GetInt(partyNo + c.no + "E3", 0);
            exp4[i] = PlayerPrefs.GetInt(partyNo + c.no + "E4", 0);
            exp5[i] = PlayerPrefs.GetInt(partyNo + c.no + "E5", 0);
            skill0[i] = PlayerPrefs.GetString(partyNo + c.no + "S0");
            skill1[i] = PlayerPrefs.GetString(partyNo + c.no + "S1");
            skill2[i] = PlayerPrefs.GetString(partyNo + c.no + "S2");
            skill3[i] = PlayerPrefs.GetString(partyNo + c.no + "S3");
        }
        punBattle.PartySendReceive(chara, item, passive, exp0, exp1, exp2, exp3, exp4, exp5, skill0, skill1, skill2, skill3, playerName, playerTitle, playerAvator);
    }
    public void EnemyPartyReceive(bool master, int[] chara,string playerName, string playerTitle, string playerAvator)
    {
        if (master != PhotonNetwork.IsMasterClient)
        {
            enemyName = playerName;
            enemyTitle = playerTitle;
            enemyAvator = playerAvator;
            for (int i = 0; i < 6; i++)
            {
                if (PlayerPrefs.GetInt(partyNo + "P" + i, 0) != 0)
                {
                    Chara c = charaMaster.CharaList.Find(a => a.no == chara[i]);
                    StartCoroutine(ImageLoad(enemyParty[i], c.name + "F"));
                }
            }
            matching.SetActive(false);
        }
    }
    public void SelectMember(int i,int j)
    {
        foreach(MemberSelect m in member)
        {
            if (m.selectNum == j)
            {
                m.selectNum = 0;
                m.text.text = "";
            }
        }
        member[i].selectNum = j;
        member[i].text.text = j.ToString();

        bool ok1=false;
        bool ok2=false;
        bool ok3=false;
        foreach (MemberSelect m in member)
        {
            if (m.selectNum == 1) ok1 = true;
            if (m.selectNum == 2) ok2 = true;
            if (m.selectNum == 3) ok3 = true;
        }
        if(ok1 && ok2 && ok3)
        {
            memberOK.interactable = true;
        }
        else
        {
            memberOK.interactable = false;
        }
    }
    public void StatusRequest(int i)
    {
        punBattle.StatusRequest(i);
    }
    public void StatusAnswer(string name, string type1, string type2, int[] status, int item, string passiveName, string passiveText, string[] skill,int[] sp)
    {
        statusDisplay.Display(name, type1, type2, status, item, passiveName, passiveText, skill,sp);
    }
    public IEnumerator FirstSummon()
    {
        Summon mySummon   = null;
        Summon yourSummon = null;
        while (true)
        {
            if (processList.Count > 0)
            {
                Summon summon = Summon.ToSummon(processList[0].content);
                if (summon.player == PhotonNetwork.IsMasterClient) mySummon = summon;
                else yourSummon = summon;
                processList.RemoveAt(0);
            }
            if (mySummon != null && yourSummon != null)
            {
                if (partyNo == "A") BGMManager.Instance.Play(BGMPath.BATTLE_A);
                if (partyNo == "B") BGMManager.Instance.Play(BGMPath.BATTLE_B);
                if (partyNo == "C") BGMManager.Instance.Play(BGMPath.BATTLE_C);
                if (partyNo == "D") BGMManager.Instance.Play(BGMPath.BATTLE_D);
                selectPanel.SetActive(false);
                yield return ImageLoad(enemyImage,enemyAvator);
                yield return battleDisplay.StartIE(enemyName,enemyTitle);


                partyDisplay.SetActive(false);
                yield return battleDisplay.FirstSummonIE(mySummon,yourSummon);
                StartCoroutine(PlayerIE());
                break;
            }

            yield return new WaitForSecondsRealtime(0.1f);
        }
    }
    public void Receive(string process ,string content)
    {
        Process p = new Process(process, content);
        processList.Add(p);
        Debug.Log(process);
    }
    public IEnumerator MatcingTimeIE()
    {
        int time = 0;
        while (true)
        {
            yield return new WaitForSecondsRealtime(1);
            time += 1;
            matchingTime.text = "åoâﬂ" + time + "ïb";
        }
    }
    IEnumerator ImageLoad(Image image, string name)
    {
        var icon = Addressables.LoadAssetAsync<Sprite>(name);
        yield return icon;
        image.sprite = icon.Result;
    }
    public void Back()
    {
        PhotonNetwork.Disconnect();
        SceneManager.LoadScene("Party");
    }
    public void SelectButton()
    {
        SEManager.Instance.Play(SEPath.CLICK);
        selectWait.SetActive(true);
        for(int i = 0; i < 6; i++)
        {
            if (member[i].selectNum == 1) decideMember[0] = i;
            if (member[i].selectNum == 2) decideMember[1] = i;
            if (member[i].selectNum == 3) decideMember[2] = i;
        }
        punBattle.MemberSend(decideMember);
        StartCoroutine(FirstSummon());
    }
    public void PartyButton()
    {
        SEManager.Instance.Play(SEPath.CLICK);
        if (partyDisplay.activeSelf)
        {
            partyDisplay.SetActive(false);
        }
        else
        {
            partyDisplay.SetActive(true);
        }
    }
    public void SkillButton()
    {
        SEManager.Instance.Play(SEPath.CLICK);
        punBattle.SkillRequest();
    }
    public void SkillAnswer(string[] skill, int[] sp,bool[] select)
    {
        battleSkill.Display(skill, sp,select);
    }
    public void SkillSelect(int i)
    {
        SEManager.Instance.Play(SEPath.CLICK);
        partyDisplay.SetActive(false);
        if (selectSwitch) {
            selectSwitch = false;
            commonPanel.SetActive(false);
            skillPanel.SetActive(false);
            Select select = new Select();
            select.player = PhotonNetwork.IsMasterClient;
            select.skill = true;
            select.num = i;
            punBattle.Receive(select.ToString());
        }
    }
    void MemberChange()
    {
        punBattle.MemberRequest(true);
    }
    public void MemberButton()
    {
        SEManager.Instance.Play(SEPath.CLICK);
        punBattle.MemberRequest(false);
    }
    public void MemberSelect(int i)
    {
        partyDisplay.SetActive(false);
        if (selectSwitch)
        {
            selectSwitch = false;
            commonPanel.SetActive(false);
            memberPanel.SetActive(false);
            Select select = new Select();
            select.player = PhotonNetwork.IsMasterClient;
            select.member = true;
            select.num = i;
            punBattle.Receive(select.ToString());
        }
    }
    public void MemberStatus(int i)
    {
        punBattle.StatusRequest(decideMember[i]);
    }
    public void MemberAnswer(string[] name, string[] sex, bool[] item, int[] error, int[] errorTurn, string[] hp, bool[] change)
    {
        battleMember.Display(name, sex, item, error,errorTurn, hp, change);
    }
    class Process
    {
        public string process;
        public string content;
        public Process(string p,string c)
        {
            process = p;
            content = c;
        }
    }
    public void End()
    {
        PhotonNetwork.Disconnect();
        SceneManager.LoadScene("Party");
    }
}