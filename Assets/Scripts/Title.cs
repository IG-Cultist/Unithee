/*
 * TitleScript
 * Creator:西浦晃太 Update:2024/07/24
*/
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Title : MonoBehaviour
{
    [SerializeField] Text txt;
    [SerializeField] GameObject title;
    int cnt = 0;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //クリック
        if (Input.GetMouseButtonUp(0))
        {
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

        if (cnt == 0)
        {
            title.GetComponent<Renderer>().material.color = new Color32(255, 0, 0, 255);
            txt.color = new Color(0.0f, 0.0f, 1.0f, 1.0f);
            cnt = 1;
        }
        else if (cnt == 1)
        {
            title.GetComponent<Renderer>().material.color = new Color32(0, 255, 0, 255);
            txt.color = new Color(0.0f, 1.0f, 0.0f, 1.0f);
            cnt = 2;
        }
        else if (cnt == 2)
        {
            title.GetComponent<Renderer>().material.color = new Color32(0, 0, 255, 255);
            txt.color = new Color(1.0f, 0.0f, 0.0f, 1.0f);
            cnt = 0;
        }

    }
}
