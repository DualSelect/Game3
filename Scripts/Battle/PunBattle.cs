using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BattleJson;
public class PunBattle : MonoBehaviourPunCallbacks, IPunInstantiateMagicCallback
{
    BattlePlayer battlePlayer;
    BattleManager battleManager;
    void IPunInstantiateMagicCallback.OnPhotonInstantiate(PhotonMessageInfo info)
    {
        battleManager = GameObject.Find("BattleManager").GetComponent<BattleManager>();
        battleManager.punBattle = this;
        battlePlayer = GameObject.Find("BattlePlayer").GetComponent<BattlePlayer>();
        battlePlayer.punBattle = this;
        battlePlayer.PartySend();
    }
    public void PartySendReceive(int[] chara,int[] item,int[] passive,int[] exp0, int[] exp1, int[] exp2, int[] exp3, int[] exp4, int[] exp5,string[] skill0, string[] skill1, string[] skill2, string[] skill3, string playerName, string playerTitle, string playerAvator)
    {
        photonView.RPC(nameof(RPCPartySendReceive), RpcTarget.MasterClient, PhotonNetwork.IsMasterClient, chara,item,passive,exp0, exp1,exp2,exp3,exp4, exp5, skill0, skill1, skill2, skill3, playerName, playerTitle, playerAvator);
    }
    [PunRPC]
    void RPCPartySendReceive(bool master, int[] chara, int[] item, int[] passive, int[] exp0, int[] exp1, int[] exp2, int[] exp3, int[] exp4, int[] exp5, string[] skill0, string[] skill1, string[] skill2, string[] skill3,string playerName, string playerTitle, string playerAvator)
    {
        battleManager.PartyReceive(master,chara, item, passive, exp0, exp1, exp2, exp3, exp4, exp5, skill0, skill1, skill2, skill3, playerName, playerTitle, playerAvator);
    }
    public void EnemyPartySendReceive(bool master,int[] chara, string playerName, string playerTitle, string playerAvator)
    {
        photonView.RPC(nameof(RPCEnemyPartySendReceive), RpcTarget.All,master, chara, playerName, playerTitle, playerAvator);
    }
    [PunRPC]
    void RPCEnemyPartySendReceive(bool master, int[] chara, string playerName, string playerTitle, string playerAvator)
    {
        battlePlayer.EnemyPartyReceive(master, chara, playerName, playerTitle, playerAvator);
    }
    public void StatusRequest(int i)
    {
        photonView.RPC(nameof(RPCStatusRequest), RpcTarget.MasterClient, PhotonNetwork.IsMasterClient, i);
    }
    [PunRPC]
    void RPCStatusRequest(bool master, int i)
    {
        battleManager.StatusRequestAnswer(master, i);
    }
    public void StatusAnswer(bool master, string name, string type1, string type2, int[] status, int item, string passiveName, string passiveText, string[] skill, int[] sp)
    {
        if(master)photonView.RPC(nameof(RPCStatusAnswer), RpcTarget.MasterClient, name, type1, type2, status,item, passiveName, passiveText, skill,sp);
        else photonView.RPC(nameof(RPCStatusAnswer), RpcTarget.Others, name, type1, type2, status, item, passiveName, passiveText, skill,sp);
    }
    [PunRPC]
    void RPCStatusAnswer( string name, string type1, string type2, int[] status, int item, string passiveName, string passiveText, string[] skill,int[] sp)
    {
        battlePlayer.StatusAnswer(name, type1, type2, status, item, passiveName, passiveText, skill,sp);
    }
    public void MemberSend(int[] i)
    {
        photonView.RPC(nameof(RPCMemberSend), RpcTarget.MasterClient, PhotonNetwork.IsMasterClient, i);
    }
    [PunRPC]
    void RPCMemberSend(bool master, int[] i)
    {
        battleManager.MemberReceive(master, i);
    }
    public void Send(string process ,string content)
    {
        photonView.RPC(nameof(RPCSend), RpcTarget.All, process,content);
    }
    public void Send(string process, string content,bool master)
    {
        if(master)photonView.RPC(nameof(RPCSend), RpcTarget.MasterClient, process, content);
        else photonView.RPC(nameof(RPCSend), RpcTarget.Others, process, content);
    }
    [PunRPC]
    void RPCSend(string process, string content)
    {
        battlePlayer.Receive(process,content);
    }
    public void Receive(string select)
    {
        photonView.RPC(nameof(RPCReceive), RpcTarget.MasterClient, select);
    }
    [PunRPC]
    void RPCReceive(string select)
    {
        battleManager.Receive(select);
    }
    public void SkillRequest()
    {
        photonView.RPC(nameof(RPCSkillRequest), RpcTarget.MasterClient, PhotonNetwork.IsMasterClient);
    }
    [PunRPC]
    void RPCSkillRequest(bool master)
    {
        battleManager.SkillAnswer(master);
    }
    public void MemberRequest(bool change)
    {
        photonView.RPC(nameof(RPCMemberRequest), RpcTarget.MasterClient, PhotonNetwork.IsMasterClient,change);
    }
    [PunRPC]
    void RPCMemberRequest(bool master,bool change)
    {
        battleManager.MemberAnswer(master,change);
    }
    public void SkillAnswer(bool master,string[] skill,int[] sp,bool[] select)
    {
        if(master)photonView.RPC(nameof(RPCSkillAnswer), RpcTarget.MasterClient, skill,sp,select);
        else photonView.RPC(nameof(RPCSkillAnswer), RpcTarget.Others, skill, sp,select);
    }
    [PunRPC]
    void RPCSkillAnswer(string[] skill, int[] sp,bool[] select)
    {
        battlePlayer.SkillAnswer(skill, sp,select);
    }
    public void MemberAnswer(bool master, string[] name, string[] sex, bool[] item, int[] error, int[] errorTurn, string[] hp, bool[] change)
    {
        if (master) photonView.RPC(nameof(RPCMemberAnswer), RpcTarget.MasterClient, name,sex,item,error, errorTurn, hp,change);
        else photonView.RPC(nameof(RPCMemberAnswer), RpcTarget.Others, name, sex, item, error,errorTurn, hp, change);
    }
    [PunRPC]
    void RPCMemberAnswer(string[] name, string[] sex, bool[] item, int[] error, int[] errorTurn, string[] hp, bool[] change)
    {
        battlePlayer.MemberAnswer(name, sex, item, error,errorTurn, hp, change);
    }

}
