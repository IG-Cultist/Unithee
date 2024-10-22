/*
 * BattleModeEnemyScript
 * Creator:西浦晃太 Update:2024/10/21
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;

public class BattleModeEnemy : MonoBehaviour
{
    // Parent
    [SerializeField] GameObject parent;

    //敵HP
    public GameObject[] playerHP;
    //HP残量
    public int playerLife;
    //防御値
    public int block;
    //攻撃値
    public int dmg;

    int count;
    // Health Script
    Health healthScript;
    // Block Icon
    public GameObject protectIcon;

    List<GameObject> actionObject;

    public bool isDead;
    // 出血判定
    public bool isBleeding;

    //Card Script
    BattleModePlayer playerScript;

    // Deck Panel
    [SerializeField] Text infoText;

    [SerializeField] GameObject health;

    [SerializeField] List<string> actionList;

    AudioSource audioSource;

    // Attack's SoundEffect
    AudioClip attackSE;

    // Heavy Attack's SoundEffect
    AudioClip heavyAttackSE;

    // Boom Attack's SoundEffect
    AudioClip boomAttackSE;

    //Parry's SoundEffect
    AudioClip parrySE;

    //Defence's SoundEffect
    AudioClip defenceSE;

    Transform iconTxt;

    string SEType;

    bool painted;

    private void Awake()
    {

    }
    // Start is called before the first frame update
    void Start()
    {
        painted = false;

        //プレイヤーHPをタグで取得
        playerHP = GameObject.FindGameObjectsWithTag("PlayerHP");
        healthScript = FindObjectOfType<Health>();
        playerLife = healthScript.BattleModeHealth;
        iconTxt = protectIcon.transform.Find("Text");
        // SetSE
        this.gameObject.AddComponent<AudioSource>();
        audioSource = GetComponent<AudioSource>();

        attackSE = (AudioClip)Resources.Load("SE/EnemyAttack");
        heavyAttackSE = (AudioClip)Resources.Load("SE/HeavyAttack");
        boomAttackSE = (AudioClip)Resources.Load("SE/Boom");
        parrySE = (AudioClip)Resources.Load("SE/Parry");
        defenceSE = (AudioClip)Resources.Load("SE/Defence");

        if (block != 0)
        {
            protectIcon.SetActive(true);
        }
        else
        {
            protectIcon.SetActive(false);
        }

        actionObject = new List<GameObject>();
        playerScript = FindObjectOfType<BattleModePlayer>();

        isBleeding = false;
        isDead = false;
        dmg = 0;
        count = 0;

        // 行動アイコンを生成
        SetActions();
    }

    // Update is called once per frame
    void Update()
    {
        if (isBleeding == true && painted == false)
        {
            for (int i = 0; i < playerScript.enemyHP.Length; i++)
            {
                if (playerScript.enemyHP[i] != null)
                {
                    playerScript.enemyHP[i].GetComponent<Renderer>().material.color = new Color32(255, 127, 127, 255);
                }
            }
            painted = true;

        }
        if (protectIcon != null) iconTxt.gameObject.GetComponent<Text>().text = block.ToString();

        if (block <= 0) if (protectIcon != null) protectIcon.SetActive(false);
    }

    /// <summary>
    /// 行動処理
    /// </summary>
    public string Attack()
    {
        if (count > 0) actionObject[count - 1].GetComponent<Renderer>().material.color = new Color32(255, 255, 255, 255);

        actionObject[count].GetComponent<Renderer>().material.color = new Color32(255, 0, 0, 255);
        SEType = "";

        if (isBleeding == true)
        {
            if (playerScript.enemyLife > 0)
            {
                playerScript.enemyLife--;
                Destroy(playerScript.enemyHP[(playerScript.enemyLife)]);
            }
        }

        if (playerScript.enemyLife <= 0)
        {
            playerScript.isDead = true;

            if (count < (actionList.Count - 1)) count++;
            dmg = 0;
            return actionList[count];
        }

        switch (actionList[count])
        {
            case "Sword":
                dmg += 1;
                infoText.text = "Rival:" + dmg + "ダメージを与える";
                SEType = "light";
                break;

            case "M.A.C.E":
                dmg += block;
                infoText.text = "Rival:" + dmg + "ダメージを与える";
                SEType = "heavy";
                break;

            case "DeathS.Y.T.H":
                dmg += 3;
                infoText.text = "Rival:" + dmg + "ダメージを与える";
                SEType = "heavy";
                break;

            case "S.Y.T.H":
                dmg += 2;
                infoText.text = "Rival:" + dmg + "ダメージを与える";
                SEType = "heavy";
                break;

            case "T.N.T":
                dmg = 999;
                infoText.text = "Rival:ドカーン!" + dmg + "ダメージを与える";
                SEType = "boom";
                break;

            case "A.X.E":
                dmg += 1;
                playerScript.block = 0;
                infoText.text = "Rival:シールド破壊！" + dmg + "ダメージを与える";
                SEType = "heavy";
                break;

            case "ForgeHammer":
                dmg += 1;
                infoText.text = "Rival:" + dmg + "ダメージを与える\n次の行動で攻撃力+1";
                SEType = "heavy";
                break;

            case "Injector":
                dmg += 1;
                infoText.text = "Rival:" + dmg + "ダメージを与える\nPlayerに出血を付与!";
                SEType = "light";
                break;

            case "PoisonKnife":
                dmg += 1;
                infoText.text = "Rival:" + dmg + "ダメージを与える\nPlayerは毒により攻撃力-1";
                SEType = "light";
                break;

            case "6mmBullet":
                dmg = 0;
                infoText.text = "Rival:仕方なく弾丸を投げた！\n意味がない！" + dmg + "ダメージを与える";
                SEType = "light";
                break;

            case "Shield":
                dmg = 0;
                if (block <= 0)
                {
                    protectIcon.SetActive(true);
                }
                audioSource.PlayOneShot(defenceSE);
                block++;
                iconTxt.gameObject.GetComponent<Text>().text = block.ToString();
                infoText.text = "Rival:" + 1 + "ブロックを受ける";
                break;

            case "SwatShield":
                dmg = 0;
                if (block <= 0)
                {
                    protectIcon.SetActive(true);
                }
                audioSource.PlayOneShot(defenceSE);
                block+=2;
                iconTxt.gameObject.GetComponent<Text>().text = block.ToString();
                infoText.text = "Rival:" + 2 + "ブロックを受ける";
                break;
        }

        if (playerScript.block != 0 && dmg > 0)
        {
            audioSource.PlayOneShot(parrySE);
            blockEffect();
            playerScript.block--;
            infoText.text += "\nYou:攻撃をブロック";
            infoText.color = Color.white;
        }
        else
        {
            // 武器別SE
            switch (SEType)
            {
                case "light":
                    audioSource.PlayOneShot(attackSE);
                    break;

                case "heavy":
                    audioSource.PlayOneShot(heavyAttackSE);
                    break;

                case "boom":
                    audioSource.PlayOneShot(boomAttackSE);
                    break;

                default:
                    break;
            }

            for (int i = 0; i < dmg; i++)
            {
                //表示HPを減らす
                Destroy(playerHP[(playerLife - 1)]);

                //内部も減らす
                playerLife--;
                damageEffect();

                //HP残量が0の場合、処理を行わない
                if (playerLife <= 0)
                {
                    isDead = true;
                    break;
                }
            }
        }

        if (playerScript.enemyLife <= 0)
        {
            playerScript.isDead = true;
        }

        if (count < (actionList.Count - 1)) count++;
        dmg = 0;
        return actionList[count];
    }

    /// <summary>
    /// Damage's Effect Progress
    /// </summary>
    async void damageEffect()
    {
        // Get Card's GameObjects from Resources Folder
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
        // Get Card's GameObjects from Resources Folder
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
            GameObject item = Instantiate(obj, new Vector2(3.4f + (1.5f * cnt), 3.7f), Quaternion.identity);
            // Rename
            item.name = action;
            item.tag = "EnemyAction";

            item.AddComponent<BoxCollider2D>();
            item.GetComponent<BoxCollider2D>().isTrigger = true;

            item.transform.SetParent(parent.transform, false);
            actionObject.Add(item);
            cnt++;
        }
    }

    /// <summary>
    /// EnemyExplain
    /// </summary>
    public void EnemyExplain(GameObject enemy)
    {
        infoText.text = enemy.name + ":ライバル！";
    }
}
