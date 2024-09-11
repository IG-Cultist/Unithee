/*
 * EnemyScript
 * Creator:西浦晃太 Update:2024/09/02
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using System.Threading.Tasks;

public class Enemy : MonoBehaviour
{
    // Parent
    [SerializeField] GameObject parent;

    //敵HP
    GameObject[] playerHP;
    //HP残量
    int playerLife;
    //防御値
    public int block;
    //攻撃値
    public int dmg;

    int count;
    // Health Script
    Health healthScript;

    // Block Icon
    public GameObject protectIcon;

    public bool isDead;

    //Card Script
    Card cardScript;

    // Deck Panel
    [SerializeField] Text infoText;

    [SerializeField] GameObject health;

    [SerializeField] List<string> actionList;

    enum EnemyType
    {
        Viper = 4,
        Slime = 4,
        Face = 4
    }

    // Start is called before the first frame update
    void Start()
    {
        cardScript = FindObjectOfType<Card>();

        isDead = false;
        dmg = 1;
        count = 0;
        block = 0;

        // 行動アイコンを生成
        SetActions();
    }

    // Update is called once per frame
    void Update()
    {
        if(block <= 0) if (protectIcon != null) Destroy(protectIcon);
    }

    /// <summary>
    /// 行動処理
    /// </summary>
    public string Attack()
    {
        if (count == 0)
        {
            //プレイヤーHPをタグで取得
            playerHP = GameObject.FindGameObjectsWithTag("PlayerHP");
            healthScript = FindObjectOfType<Health>();
            playerLife = healthScript.PlayerHealth;
        }

        switch (actionList[count])
        {
            case "Sword":
                infoText.text = "Enemy:" + dmg + "ダメージを与える";
                break;

            case "M.A.C.E":
                dmg += block;
                infoText.text = "Enemy:" + dmg + "ダメージを与える";
                break;

            case "T.N.T":
                dmg = 999;
                infoText.text = "Enemy:ドカーン!" + dmg + "ダメージを与える";
                break;

            case "Shield":
                if (block <= 0)
                {
                    //Get Card's GameObjects from Resources Folder
                    GameObject prefab = (GameObject)Resources.Load("ProtectIcon");

                    // Create Instance from Now Turn's Cards
                    protectIcon = Instantiate(prefab, new Vector2(8.4f, 2.0f), Quaternion.identity);
                    protectIcon.name = "ProtectIcon";
                }
                block++;
                infoText.text = "Enemy:" + block + "ブロックを受ける";
                break;
        }

        for (int i = 0; i < dmg; i++)
        {
            if (cardScript.block != 0)
            {
                blockEffect();
                cardScript.block--;
                infoText.text = "Enemy:" + dmg + "ダメージを与える\nYou:攻撃をブロック";
                infoText.color = Color.white;
            }
            else
            {
                //HP残量が0の場合、処理を行わない
                if (playerLife <= 0)
                {
                    isDead = true;
                    break;
                }
                //表示HPを減らす
                Destroy(playerHP[(playerLife - 1)]);

                //内部も減らす
                playerLife--;
                damageEffect();
            }
        }

        infoText.color = Color.white;

        dmg = 1;
        if (count < (actionList.Count - 1)) count++;

        return actionList[count];
    }

    /// <summary>
    /// Damage's Effect Progress
    /// </summary>
    async void damageEffect()
    {
        //Get Card's GameObjects from Resources Folder
        GameObject prefab = (GameObject)Resources.Load("AttackFlash");

        // Create Instance from Now Turn's Cards
        GameObject obj = Instantiate(prefab, new Vector2(0.0f, 0.0f), Quaternion.identity);
        await Task.Delay(100);
        Destroy(obj);
    }

    /// <summary>
    /// Block's Effect Progress
    /// </summary>
    async void blockEffect()
    {
        //Get Card's GameObjects from Resources Folder
        GameObject prefab = (GameObject)Resources.Load("BlockFlash");

        // Create Instance from Now Turn's Cards
        GameObject obj = Instantiate(prefab, new Vector2(0.0f, 0.0f), Quaternion.identity);
        await Task.Delay(100);
        Destroy(obj);
    }

    /// <summary>
    /// Set Actions
    /// </summary>
    void SetActions()
    {
        int cnt = 0;

        foreach (var action in actionList)
        {
            //  Get Prefabs from List
            GameObject obj = (GameObject)Resources.Load("Cards/" + action);
            // Create Action Objects
            GameObject item = Instantiate(obj, new Vector2(3.4f+ (1.5f * cnt), 3.7f), Quaternion.identity);
            // Rename
            item.name = action;

            item.AddComponent<BoxCollider2D>();
            item.GetComponent<BoxCollider2D>().isTrigger = true;

            item.transform.SetParent(parent.transform, false);
            cnt++;
        }
    }
}
