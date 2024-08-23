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
    //PassiveList
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
        // Get and Set Passives
        passiveList = new List<string>() {"Spike","Spike","Spike","ArmorChip" };

        activePassives = new List<GameObject>();

        // Set Passives
        SetPassives();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// Set Passives
    /// </summary>
    void SetPassives()
    {
        int cnt = 0;
        Vector2 pos = this.transform.position;

        foreach (var passive in passiveList)
        {
            //  Get Prefabs from List
            GameObject obj = (GameObject)Resources.Load(passive);
            // Create Setted Passives
            GameObject item = Instantiate(obj, new Vector2(pos.x + (2.0f * cnt),pos.y), Quaternion.identity);
            // Rename
            item.name = passive;
            // Add ActiveList
            activePassives.Add(item);
            cnt++;
        }
    }
}
