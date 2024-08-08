/*
 * EnemyScript
 * Creator:西浦晃太 Update:2024/07/25
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Progress;
using UnityEngine.UI;

public class Enemy : MonoBehaviour
{
    //敵HP
    GameObject[] playerHP;
    //HP残量
    int playerLife;
    //防御値
    int block;
    //攻撃値
    int dmg;

    int count;
    //ヘルススクリプト
    Health healthScript;
    [SerializeField] GameObject health;
    [SerializeField] Text action;

    List<string> actionList = new List<string>() { "attack", "attack", "defence", "attack" };

    enum EnemyType
    {
        Viper = 4,
        Slime = 4,
        Face = 4
    }

    // Start is called before the first frame update
    void Start()
    {
        count = 0;
        block = 0;
    }

    // Update is called once per frame
    void Update()
    {

    }

    /// <summary>
    /// 行動処理
    /// </summary>
    public string Attack()
    {
        dmg = 1;
        if (count == 0)
        {
            //プレイヤーHPをタグで取得
            playerHP = GameObject.FindGameObjectsWithTag("PlayerHP");
            healthScript = FindObjectOfType<Health>();
            playerLife = healthScript.PlayerHealth;
        }

        if (actionList[count] == "attack")
        {
            for (int i = 0; i < dmg; i++)
            {
                Debug.Log(playerLife);
                //HP残量が0の場合、処理を行わない
                if (playerLife <= 0)
                {
                    Debug.Log("You Deadadad！");
                    break;
                }
                action.text = "敵の攻撃！";
                //表示HPを減らす
                Destroy(playerHP[(playerLife - 1)]);

                //内部も減らす
                playerLife--;
                Debug.Log("Player's HP:" + playerLife);
            }
        }
        else if (actionList[count] == "defence")
        {
            action.text = "敵は防御した！";
            block++;
        }

        if(count < (actionList.Count - 1)) count++;   
        
        return actionList[count];
    }
}
