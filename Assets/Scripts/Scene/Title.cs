/*
 * TitleScript
 * Creator:西浦晃太 Update:2024/10/28
*/
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
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
                    result => //After Set's Process
                    {
                        SceneManager.LoadScene("SelectScene");
                    }));
            }
            else
            {
                //トークンがない場合、トークンを生成
                if (NetworkManager.Instance.AuthToken == null)
                {
                    StartCoroutine(NetworkManager.Instance.CreateToken(
                        result => //生成後、シーン遷移
                        {
                            SceneManager.LoadScene("SelectScene");
                        }));
                }
                else SceneManager.LoadScene("SelectScene"); //既にトークンを持っている場合、そのままシーン遷移
            }
        }
    }

    //IEnumerator checkCatalog()
    //{
    //    var checkHandle = Addressables.CheckForCatalogUpdates(false);
    //    yield return checkHandle;
    //    var updates = checkHandle.Result;
    //    Addressables.Release(checkHandle);
    //    if(updates.Count >= 1)
    //    {
    //        //更新がある場合はロード画面へ
    //        SceneManager.LoadScene("LoadScene");
    //    }
    //}
}
