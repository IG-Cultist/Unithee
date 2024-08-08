/*
 * CardScript
 * Creator:西浦晃太 Update:2024/07/25
*/
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;
using System.Threading.Tasks;

public class Card : MonoBehaviour
{
    //現在持っているカード
    [SerializeField] List<GameObject> card;

    //終了ボタン
    [SerializeField] GameObject button;

    //パッシブゲームオブジェクト(仮)
    GameObject spike;
    GameObject armorChip;

    //現在持っているカード
    List<GameObject> passive;

    //ヘルススクリプト
    Health healthScript;

    //ヘルススクリプト
    Enemy enemyScript;

    //選択カード
    List<GameObject> selectedCard;

    //アクティブなカード
    [SerializeField] List<GameObject> activeList;

    //順序管理
    int count;

    //パッシブ順序管理
    int passiveCnt;

    //敵HP
    GameObject[] enemyHP;

    //HP残量
    int enemyLife;

    //攻撃値
    int dmg;

    //防御値
    int block = 1;

    int enemBlock;

    // Start is called before the first frame update
    void Start()
    {
        button.SetActive(false);
        //敵HPをタグで取得
        enemyHP = GameObject.FindGameObjectsWithTag("EnemyHP");
        //取得したHPの個数を代入
        healthScript = FindObjectOfType<Health>();
        enemyLife = healthScript.EnemHealth;

        enemyScript = FindObjectOfType<Enemy>();

        //各リストをnewする
        activeList = new List<GameObject>();
        selectedCard = new List<GameObject>();
        //順序を初期化
        count = 0;
        passiveCnt = 0;
        enemBlock = 0;

        //各パッシブの設定(仮)
        spike = new GameObject();
        spike.name = "spike";
        armorChip = new GameObject();
        armorChip.name = "armorChip";
        passive = new List<GameObject>();
        for (int i = 1; i < 3; i++) passive.Add(spike);
        passive.Add(armorChip);
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
                        //activeListに追加
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
                        //並び直し
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
    public async void TurnEnd()
    {
        if (activeList.Count != 4)
        {
            Debug.Log("カードが足りない");
        }
        else
        {
            foreach (var item in activeList)
            {
                dmg = 1;

                switch (item.tag) //カードのタグを判定
                {
                    case "Attack": //カードがアタックの場合

                        //パッシブを反映
                        passiveEffect(item);

                        //ダメージの数値分繰り返す
                        for (int i = 0; i < dmg; i++)
                        {
                            if (enemBlock != 0)
                            {
                                Debug.Log("防がれた！");
                                enemBlock--;
                            }
                            else
                            {
                                //HP残量が0の場合、処理を行わない
                                if (enemyLife <= 0)
                                {
                                    Debug.Log("敵死んだ！");
                                    break;
                                }
                                //表示を減らす
                                Destroy(enemyHP[(enemyLife - 1)]);

                                //内部も減らす
                                enemyLife--;
                                Debug.Log("Enemy's HP:" + enemyLife);

                            }
                        }
                        Destroy(item);

                        await Task.Delay(1000);
                        enemyAction();
                        break;

                    case "Defence": //カードがディフェンスの場合
                        Debug.Log("防御");
                        passiveEffect(item);
                        block++;
                        Destroy(item);

                        await Task.Delay(1000);
                        enemyAction();
                        break;

                    case "Support": //カードがサポートの場合
                        Debug.Log("補助");
                        passiveEffect(item);
                        Destroy(item);

                        await Task.Delay(1000);
                        enemyAction();
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
            button.SetActive(true);
        }
    }

    /// <summary>
    /// カードの並び直し処理
    /// </summary>
    private void cardRefresh()
    {
        //一時的に現在アクティブなカードを代入するリスト
        List<GameObject> keepList = new List<GameObject>();
        foreach (var item in activeList) //アクティブなカードを破壊し、仮リストに追加
        {
            Destroy(item);
            keepList.Add(item);
        }
        //リスト内にあるカード全てを消す
        activeList.Clear();
        //順序をリセット
        count = 0;

        foreach (var item in keepList) //並べなおす
        {
            //現在のカードプレハブを元に、インスタンスを生成、
            GameObject obj = Instantiate(item, new Vector2(-8.0f + (2.0f * count), -3.5f), Quaternion.identity);
            //オブジェクトの色を訂正
            obj.GetComponent<Renderer>().material.color = new Color32(255, 255, 255, 255);
            //クローンしたオブジェクトの名前を訂正
            obj.name = item.name;
            //カードのタグをクローンオブジェクトにも追加
            obj.tag = item.tag;
            activeList.Add(obj);
            //順序を加算
            count++;
        }
    }

    /// <summary>
    /// パッシブ効果処理
    /// </summary>
    /// <param name="item"></param>
    private void passiveEffect(GameObject item)
    {
        //パッシブの名前で識別
        switch (passive[passiveCnt].name)
        {
            case "spike":
                if (item.tag == "Attack")
                {
                    Debug.Log("とげ発動 DMG+1");
                    dmg++;
                }
                break;
            case "armorChip":
                if (item.tag == "Defence")
                {
                    Debug.Log("アーマーチップ発動 DEF+1");
                    block++;
                }
                break;
        }

        if (passiveCnt < (passive.Count - 1)) passiveCnt++;
    }

    /// <summary>
    /// 敵の行動
    /// </summary>
    async void enemyAction()
    {
        if (enemyLife <= 0)
        {
            return;
        }
        //enemyScript.Attack();

        if (enemyScript.Attack() == "defence")
        {
            enemBlock++;
        }
        await Task.Delay(1000);
    }
    public void endGame()
    {
        SceneManager.LoadScene("Result");
    }

    void cardType()
    {

    }
}