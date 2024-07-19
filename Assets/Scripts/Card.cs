using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;

public class Card : MonoBehaviour
{
    //現在持っているカード
    [SerializeField] List<GameObject> card;
    //ヘルススクリプト
    private Health healthScript;
    //選択カード
    List<GameObject> selectedCard;
    //現在のデッキのカード
    GameObject[] activeCards;
    //アクティブなカード
    [SerializeField] List<GameObject> activeList;
    //順序管理
    int count;
    //敵HP
    GameObject[] enemyHP;
    //HP残量
    int enemyLife;

    // Start is called before the first frame update
    void Start()
    {
        //敵HPをタグで取得
        enemyHP = GameObject.FindGameObjectsWithTag("HP");
        //取得したHPの個数を代入
        healthScript = FindObjectOfType<Health>();
        enemyLife = healthScript.EnemHealth;
        activeList = new List<GameObject>();
        selectedCard = new List<GameObject>();
        //順序を初期化
        count = 0;
    }

    // Update is called once per frame
    void Update()
    {
        //カード選択
        if (Input.GetMouseButtonUp(0))
        {
            CardClick();
        }
    }

    /// <summary>
    /// カードクリック処理
    /// </summary>
    void CardClick()
    {
        //タッチした位置にレイを飛ばす
        Vector2 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        //第二引数 レイはどの方向に進むか(zero=指定点)
        RaycastHit2D hit2d = Physics2D.Raycast(worldPosition, Vector2.zero);

        //当たり判定
        if (hit2d)
        {
            //ヒットしたオブジェクト取得
            GameObject hitCard = hit2d.collider.gameObject;

            if (!selectedCard.Contains(hitCard)) //何も選択していなかった場合
            {
                //選択カードの色を変更
                hitCard.GetComponent<Renderer>().material.color = new Color32(127, 127, 127, 255);
                //リストに選択カードを追加
                selectedCard.Add(hitCard);

                foreach (var item in card)
                {
                    if (item.name == hitCard.name) //選択カードと現在のカードの名前が一致した場合
                    {
                        // 現在のカードプレハブを元に、インスタンスを生成、
                        GameObject obj = Instantiate(item, new Vector2(-8.0f + (2.0f * count), -3.5f), Quaternion.identity);
                        //オブジェクトの色を訂正
                        obj.GetComponent<Renderer>().material.color = new Color32(255, 255, 255, 255);
                        //クローンしたオブジェクトの名前を訂正
                        obj.name = item.name;
                        //カードのタグをクローンオブジェクトにも追加
                        obj.tag = item.tag;
                        //activeListリストに追加
                        activeList.Add(obj);
                        //順序を加算
                        count++;
                    }
                }
            }
            else //すでにカードを選択していた場合
            {
                //選択カードの色を戻す
                hitCard.GetComponent<Renderer>().material.color = new Color32(255, 255, 255, 255);
                //リストから選択カードを除去
                selectedCard.Remove(hitCard);

                foreach (var item in activeList)
                {
                    if (item.name == hitCard.name)
                    {
                        //そのカードを消す
                        Destroy(item);
                        //消したカードをリストからも除去
                        activeList.Remove(item);
                        count--;
                        cardRefresh();
                        break;
                    }
                }
            }
        }
    }

    /// <summary>
    /// ターン終了処理
    /// </summary>
    public void TurnEnd()
    {
        foreach (var item in activeList)
        {
            switch (item.tag) //カードのタグを判定
            {
                case "Attack": //カードがアタックの場合
                    Debug.Log("攻撃");
                    if (enemyLife <= 0) //HP残量が0の場合
                    {
                        Debug.Log("敵死んだ！");
                    }
                    else //まだおげんきな場合
                    {
                        //表示HPを1減らす
                        Destroy(enemyHP[(enemyLife - 1)]);
                        //内部も1減らす
                        enemyLife--;
                        Debug.Log("HP:" + enemyLife);
                        if (enemyLife <= 0) //HP残量が0の場合
                        {
                            Debug.Log("敵死んだ！");
                        }
                    }
                    Destroy(item);
                    break;

                case "Defence": //カードがディフェンスの場合
                    Debug.Log("防御");
                    Destroy(item);
                    break;

                case "Support": //カードがサポートの場合
                    Debug.Log("補助");
                    Destroy(item);
                    break;

                default:
                    break;
            }
        }

        for (int i = 0; i < selectedCard.Count; i++)
        { //選択した際の色変更を元に戻す
            selectedCard[i].GetComponent<Renderer>().material.color = new Color32(255, 255, 255, 255);
        }

        //各リスト内にあるカード全てを消す
        selectedCard.Clear();
        activeList.Clear();
        //順序をリセット
        count = 0;
    }

    private void cardRefresh()
    {
        //各リスト内にあるカード全てを消す
        selectedCard.Clear();
        activeList.Clear();
        //順序をリセット
        count = 0;

        foreach (var item in activeList)
        {
            // 現在のカードプレハブを元に、インスタンスを生成、
            GameObject obj = Instantiate(item, new Vector2(-8.0f + (2.0f * count), -3.5f), Quaternion.identity);
            //オブジェクトの色を訂正
            obj.GetComponent<Renderer>().material.color = new Color32(255, 255, 255, 255);
            //クローンしたオブジェクトの名前を訂正
            obj.name = item.name;
            //カードのタグをクローンオブジェクトにも追加
            obj.tag = item.tag;
            //順序を加算
            count++;
        }
    }
}