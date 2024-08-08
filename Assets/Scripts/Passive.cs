/*
 * PassiveScript
 * Creator:西浦晃太 Update:2024/07/26
*/
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Passive : MonoBehaviour
{
    //パッシブリスト
    [SerializeField] List<string> passiveList;

    //セットされたパッシブを入れるリスト
    List<GameObject> activePassives;

    /// <summary>
    /// アクティブなパッシブのプロパティ
    /// </summary>
    public List<GameObject> ActivePassives
    {
        get { return activePassives; }
    }
    // Start is called before the first frame update
    void Start()
    {
        //取得してきたパッシブを突っ込む予定
        passiveList = new List<string>() {"Spike","Spike","Spike","ArmorChip" };

        activePassives = new List<GameObject>();

        //取得したパッシブを設置
        SetPassives();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// パッシブ設置処理
    /// </summary>
    void SetPassives()
    {
        int cnt = 0;
        Vector2 pos = this.transform.position;

        foreach (var passive in passiveList)
        {
            // リスト内にある名前と同じプレハブを取得
            GameObject obj = (GameObject)Resources.Load(passive);
            // 設定されたパッシブを生成、
            GameObject item = Instantiate(obj, new Vector2(pos.x + (2.0f * cnt),pos.y), Quaternion.identity);
            // 名前を訂正
            item.name = passive;
            // アクティブリストに追加
            activePassives.Add(item);
            cnt++;
        }
    }
}
