using KanKikuchi.AudioManager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Exp : MonoBehaviour
{
    public Status status;
    public int i;
    public Button plus;
    public Button minus;
    public Button max;
    public Button min;

    public void Plus()
    {
        SEManager.Instance.Play(SEPath.CLICK);
        status.Plus(i);
    }
    public void Minus()
    {
        SEManager.Instance.Play(SEPath.CLICK);
        status.Minus(i);
    }
    public void Max()
    {
        SEManager.Instance.Play(SEPath.CLICK);
        status.Max(i);
    }
    public void Min()
    {
        SEManager.Instance.Play(SEPath.CLICK);
        status.Min(i);
    }
}
