using KanKikuchi.AudioManager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Icon : MonoBehaviour
{
    public Chara chara;
    public Text mark;
    public void IconButton()
    {
        SEManager.Instance.Play(SEPath.CLICK);
        GameObject.Find("Edit").GetComponent<Edit>().SelectNameChange(chara);
    }
}
