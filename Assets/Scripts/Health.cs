/*
 * HealthScript
 * Creator:西浦晃太 Update:2024/07/25
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    //ハートのゲームオブジェクト
    [SerializeField] GameObject heart;
    //敵の体力値
    int enemHealth;
    //プレイヤーの体力値
    int playerHealth;

    /// <summary>
    /// エネミーHPのプロパティ
    /// </summary>
    public int EnemHealth
    {
        get { return enemHealth; }
    }

    /// <summary>
    /// プレイヤーHPのプロパティ
    /// </summary>
    public int PlayerHealth
    {
        get { return playerHealth; }
    }

    // Start is called before the first frame update
    void Start()
    {
        enemHealth = 3;
        playerHealth = 5;
        EnemyLife();
        PlayerLife();
    }

    // Update is called once per frame
    void Update()
    {

    }

    /// <summary>
    /// 敵HP設定
    /// </summary>
    public void EnemyLife()
    {
        Vector2 pos = this.transform.position;
        for (int i = 0; i < enemHealth; i++) //設定health分繰り返し
        {
            //プレハブのタグをエネミーのものに変更
            heart.tag = "EnemyHP";
            // Heartプレハブを元に、インスタンスを生成、
            Instantiate(heart, new Vector2(pos.x + (0.8f * i), pos.y), Quaternion.identity);
        }
    }

    /// <summary>
    /// プレイヤーHP設定
    /// </summary>
    public void PlayerLife()
    {
        for (int i = 0; i < playerHealth; i++) //設定health分繰り返し
        {
            //プレハブのタグをプレイヤーのものに変更
            heart.tag = "PlayerHP"; 
            // Heartプレハブを元に、インスタンスを生成、
            Instantiate(heart, new Vector2(-8.5f + (0.8f * i), 0.7f), Quaternion.identity);
        }
    }
}
