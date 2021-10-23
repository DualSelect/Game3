using KanKikuchi.AudioManager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

public class PlayerInfo : MonoBehaviour
{
    public Text title;
    public Text nameT;
    public InputField nameInput;
    public Image avator;
    public GameObject playerUp;
    public GameObject playerDown;

    public void Avator(int i)
    {
        PlayerPrefs.SetString("avator","汎用"+i+"B");
        StartCoroutine(ImageLoad(avator, PlayerPrefs.GetString("avator", "汎用6B")));
    }
    public void Title(string t)
    {
        PlayerPrefs.SetString("title", t);
        title.text = PlayerPrefs.GetString("title", "初心者");
    }
    public void NameChange()
    {
        PlayerPrefs.SetString("name", nameInput.text);
        nameT.text = PlayerPrefs.GetString("name", "new player");
    }
    IEnumerator ImageLoad(Image image, string name)
    {
        var icon = Addressables.LoadAssetAsync<Sprite>(name);
        yield return icon;
        image.sprite = icon.Result;
    }
    public void Open()
    {

        SEManager.Instance.Play(SEPath.CLICK);
        playerUp.SetActive(true);
        playerDown.SetActive(true);
        title.text = PlayerPrefs.GetString("title", "初心者");
        nameT.text = PlayerPrefs.GetString("name", "new player");
        StartCoroutine(ImageLoad(avator, PlayerPrefs.GetString("avator", "汎用6B")));
    }
    public void Back()
    {
        SEManager.Instance.Play(SEPath.CLICK);
        playerUp.SetActive(false);
        playerDown.SetActive(false);
    }
}
