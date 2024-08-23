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
    //Active Cards
    [SerializeField] List<GameObject> card;

    //TurnEnd Button
    [SerializeField] GameObject button;

    //Passive's GameObject
    GameObject spike;
    GameObject armorChip;

    //Passives
    List<GameObject> passive;

    //HealthScript
    Health healthScript;

    //EnemyScript
    Enemy enemyScript;

    //Selected Cards
    List<GameObject> selectedCard;

    //Active Card's List
    [SerializeField] List<GameObject> activeList;

    //Card Turn's Count
    int count;

    //Passive Turn's Count
    int passiveCnt;

    //Enemy's HP
    GameObject[] enemyHP;

    //HP Count
    int enemyLife;

    //Damage Value
    int dmg;

    //Defence Value
    int block = 1;

    //Enemy's Defence Value
    int enemBlock;

    // Start is called before the first frame update
    void Start()
    {
        button.SetActive(false);
        //Get for Enemy Life from tag
        enemyHP = GameObject.FindGameObjectsWithTag("EnemyHP");
        //取得したHPの個数を代入
        healthScript = FindObjectOfType<Health>();
        enemyLife = healthScript.EnemHealth;

        enemyScript = FindObjectOfType<Enemy>();

        //Set Lists
        activeList = new List<GameObject>();
        selectedCard = new List<GameObject>();
        //Set Counts
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
    /// Card Click
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
                //Change the Card's Color
                hitCard.GetComponent<Renderer>().material.color = new Color32(127, 127, 127, 255);
                //Add List for Selected Card
                selectedCard.Add(hitCard);

                foreach (var item in card)
                {
                    if (item.name == hitCard.name) //選択カードと現在のカードの名前が一致した場合
                    {
                        // Create Instance from Now Turn's Cards
                        GameObject obj = Instantiate(item, new Vector2(-8.0f + (2.0f * count), -3.5f), Quaternion.identity);
                        //オブジェクトの色を訂正
                        obj.GetComponent<Renderer>().material.color = new Color32(255, 255, 255, 255);
                        //Rename Item
                        obj.name = item.name;
                        //Add Tag for Clone Items
                        obj.tag = item.tag;
                        //Add ActiveList
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
                //Remove Selected Card from Lists
                selectedCard.Remove(hitCard);

                foreach (var item in activeList)
                {
                    if (item.name == hitCard.name)
                    {
                        //Delete Card
                        Destroy(item);
                        //Remove from List
                        activeList.Remove(item);
                        //Refresh
                        cardRefresh();
                        break;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Turn End Process
    /// </summary>
    public async void TurnEnd()
    {
        if (activeList.Count != 4)
        {
            Debug.Log("Should Use 4 Cards");
        }
        else
        {
            foreach (var item in activeList)
            {
                dmg = 1;

                switch (item.tag) //Judge the Card's Tag
                {
                    case "Attack":

                        //Active to Passives
                        passiveEffect(item);

                        //Loop to Damage Values
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

                    case "Defence":
                        Debug.Log("防御");
                        passiveEffect(item);
                        block++;
                        Destroy(item);

                        await Task.Delay(1000);
                        enemyAction();
                        break;

                    case "Support":
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
            { //Reset Color
                selectedCard[i].GetComponent<Renderer>().material.color = new Color32(255, 255, 255, 255);
            }

            //Delete All List's Items
            selectedCard.Clear();
            activeList.Clear();
            //Reset Count
            count = 0;
            button.SetActive(true);
        }
    }

    /// <summary>
    /// Card Refresh Process
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
    /// Passive's Effect Process
    /// </summary>
    /// <param name="item"></param>
    private void passiveEffect(GameObject item)
    {
        //Select to Passive's name
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
    /// Enemy's Action Process
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
        SceneManager.LoadScene("SelectScene");
    }

    void cardType()
    {

    }
}