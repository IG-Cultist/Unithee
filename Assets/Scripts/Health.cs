using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] GameObject heart;
    int enemHealth;

    public int EnemHealth
    {//敵HPのプロパティ
        get { return enemHealth; }
    }
    // Start is called before the first frame update
    void Start()
    {
        enemHealth = 4;
        Life();
    }

    // Update is called once per frame
    void Update()
    {

    }

    /// <summary>
    /// HPプレハブ生成
    /// </summary>
    public void Life()
    {
        Vector2 pos = this.transform.position;
        for (int i = 0; i < enemHealth; i++) //設定health分繰り返し
        {
            // Heartプレハブを元に、インスタンスを生成、
            Instantiate(heart, new Vector2(pos.x + (0.8f * i), pos.y), Quaternion.identity);
        }
    }
}
