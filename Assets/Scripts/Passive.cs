/*
 * PassiveScript
 * Creator:êºâYçWëæ Update:2024/07/26
*/
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Passive : MonoBehaviour
{
    // PassiveList
    List<string> passiveList;

    //
    Dictionary<string, ItemResponse> passiveDictionary = new Dictionary<string, ItemResponse>();

    // Deck Panel
    public Text infoText;

    // List for Settd Passives
    List<string> activePassives;

    enum passive : int
    {
        Spike = 0,
        ArmorChip,
        Slime,
        JOKER,
        HandyDrill,
        VampireWrench,
        HandGun
    }

    [SerializeField] int[] passiveID;

    /// <summary>
    /// ActivePassive's Property
    /// </summary>
    public List<string> ActivePassives
    {
        get { return activePassives; }
    }

    void Awake()
    {
        // Set Passives
        passiveList = new List<string>();

        StartCoroutine(NetworkManager.Instance.GetItem(items =>
        {
            for (int i = 0; i < 4; i++)
            {
                passiveList.Add(items[passiveID[i]].Name.ToString());
            } 
            
            foreach (var item in items)
            {
                passiveDictionary.Add(item.Name, item);
            }

            activePassives = new List<string>();
            // Set Passives
            SetPassives();

            Card card = GameObject.Find("Manager").GetComponent<Card>();
            card.SetPassives(activePassives, passiveDictionary);
        }));

  
    }

    // Start is called before the first frame update
    void Start()
    {

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

            item.AddComponent<BoxCollider2D>();
            item.GetComponent<BoxCollider2D>().isTrigger = true;
            // Add ActiveList
            activePassives.Add(item.name);
            cnt++;
        }

        foreach (var items in activePassives)
        {
        }
    }
}
