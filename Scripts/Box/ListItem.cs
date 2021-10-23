using KanKikuchi.AudioManager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ListItem : MonoBehaviour
{
    public Item item;
    public new Text name;
    public void Click()
    {
        SEManager.Instance.Play(SEPath.CLICK);
        GameObject.Find("ItemChange").GetComponent<ItemChange>().Select(item);
    }
}
