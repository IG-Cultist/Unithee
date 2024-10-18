/*
 * TitleScript
 * Creator:西浦晃太 Update:2024/08/26
*/
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.AddressableAssets;

public class Title : MonoBehaviour
{
    [SerializeField] Text txt;
    [SerializeField] GameObject title;
    int count;
    // Start is called before the first frame update
    void Start()
    {
        count = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (count < 150)
        {
            txt.color = new Color32(255, 255, 255, 255);
        }
        else if (count >= 150 && count < 350)
        {
            txt.color = new Color32(255, 255, 255, 0);
        }
        else 
        {
            count = 0;
        }
        count++;

        //クリック
        if (Input.GetMouseButtonUp(0))
        {
            //StartCoroutine(checkCatalog());
            bool isSuccess = NetworkManager.Instance.LoadUserData();

            if (!isSuccess)
            { //ユーザデータが保存されていなかった場合
                StartCoroutine(NetworkManager.Instance.StoreUser(
                    Guid.NewGuid().ToString(),  //Set Random Name
                    result => //After Set's Process
                    {
                        SceneManager.LoadScene("SelectScene");
                    }));
            }
            else
            {
                SceneManager.LoadScene("SelectScene");
            }
        }
    }

    IEnumerator checkCatalog()
    {
        var checkHandle = Addressables.CheckForCatalogUpdates(false);
        yield return checkHandle;
        var updates = checkHandle.Result;
        Addressables.Release(checkHandle);
        if(updates.Count >= 1)
        {
            //更新がある場合はロード画面へ
            SceneManager.LoadScene("LoadScene");
        }
    }
}
