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
using UnityEngine.SceneManagement;

public class Enemy : MonoBehaviour
{
    // Parent
    [SerializeField] GameObject parent;

    //敵HP
    public GameObject[] playerHP;
    //HP残量
    public int playerLife;
    //防御値
    public int block;

    //反射するか
    public bool isReflect;

    //攻撃値
    public int dmg;

    int count;
    // Health Script
    Health healthScript;

    // Block Icon
    public GameObject protectIcon;

    List<GameObject> actionObject;

    public bool isDead;

    //Card Script
    Card cardScript;

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

    // Boom Attack's SoundEffect
    AudioClip poisonSE;

    //Parry's SoundEffect
    AudioClip copySE;

    //Parry's SoundEffect
    AudioClip waitSE;

    Transform iconTxt;

    string SEType;

    private void Awake()
    {

    }
    // Start is called before the first frame update
    void Start()
    {        //プレイヤーHPをタグで取得
        playerHP = GameObject.FindGameObjectsWithTag("PlayerHP");
        healthScript = FindObjectOfType<Health>();
        playerLife = healthScript.PlayerHealth;
        iconTxt = protectIcon.transform.Find("Text");
        // SetSE
        this.gameObject.AddComponent<AudioSource>(); 
        audioSource = GetComponent<AudioSource>();

        attackSE = (AudioClip)Resources.Load("SE/EnemyAttack");
        heavyAttackSE = (AudioClip)Resources.Load("SE/HeavyAttack");
        boomAttackSE = (AudioClip)Resources.Load("SE/Boom");
        parrySE = (AudioClip)Resources.Load("SE/Parry");
        defenceSE = (AudioClip)Resources.Load("SE/Defence");

        poisonSE = (AudioClip)Resources.Load("SE/Poison");
        copySE = (AudioClip)Resources.Load("SE/Copy");
        waitSE = (AudioClip)Resources.Load("SE/Wait");

        if (block != 0)
        {
            protectIcon.SetActive(true);
        }
        else
        {
            protectIcon.SetActive(false);
        }

        actionObject = new List<GameObject>();
        cardScript = FindObjectOfType<Card>();

        isDead = false;
        dmg = 0;
        count = 0;

        // 行動アイコンを生成
        SetActions();
    }

    // Update is called once per frame
    void Update()
    {
        if (protectIcon != null) iconTxt.gameObject.GetComponent<Text>().text = block.ToString();

        if (isReflect == false) this.gameObject.GetComponent<Renderer>().material.color = new Color32(255, 255, 255, 255);
        if (block <= 0 ) if (protectIcon != null) protectIcon.SetActive(false);
    }

    /// <summary>
    /// 行動処理
    /// </summary>
    public string Attack()
    {
        if(count > 0) actionObject[count-1].GetComponent<Renderer>().material.color = new Color32(255, 255, 255, 255);

        isReflect = false;
        actionObject[count].GetComponent<Renderer>().material.color = new Color32(255, 0, 0, 255);
        SEType = "";
        switch (actionList[count])
        {
            case "Wait":
                dmg = 0;
                infoText.text = "Enemy:様子をうかがっている";
                SEType = "wait";
                break;

            case "Copy":
                SEType = "copy";
                if (cardScript.activeList[count].tag == "Attack")
                {
                    if (cardScript.activeList[count].name == "A.X.E")
                    {
                        dmg = 1;
                        cardScript.block = 0;
                    }
                    else if (cardScript.activeList[count].name == "M.A.C.E")
                    {
                        dmg += block;
                    }
                    else
                    {
                        dmg = cardScript.dmg;
                    }
                }
                else
                {
                    if (block <= 0)
                    {
                        protectIcon.SetActive(true);
                    }
                    block++;
                    iconTxt.gameObject.GetComponent<Text>().text = block.ToString();
                }

                infoText.text = "Enemy:敵はプレイヤーの行動をコピーした！";
                break;

            case "Sword":
                dmg += 1;
                infoText.text = "Enemy:" + dmg + "ダメージを与える";
                SEType = "light";
                break;

            case "Poison":
                dmg = 0;
                cardScript.dmg -= 1;
                infoText.text = "Enemy:" + dmg + "ダメージを与える\nPlayerは毒によりDMG-1";
                SEType = "poison";
                break;

            case "Destruction":
                dmg = 999;
                audioSource.PlayOneShot(boomAttackSE);
                cardScript.isDead = true;
                cardScript.enemyLife = 0;
                infoText.text = "Enemy:自爆した！";

                break;

            case "M.A.C.E":
                dmg += block;
                infoText.text = "Enemy:" + dmg + "ダメージを与える";
                SEType = "heavy";
                break;

            case "DeathS.Y.T.H":
                dmg += 3;
                infoText.text = "Enemy:" + dmg + "ダメージを与える";
                SEType = "heavy";
                break;

            case "S.Y.T.H":
                dmg += 2;
                infoText.text = "Enemy:" + dmg + "ダメージを与える";
                SEType = "heavy";
                break;

            case "T.N.T":
                dmg = 999;
                infoText.text = "Enemy:ドカーン!" + dmg + "ダメージを与える";
                SEType = "boom";
                break;

            case "A.X.E":
                dmg += 1;
                cardScript.block = 0;
                infoText.text = "Enemy:シールド破壊！" + dmg + "ダメージを与える";
                SEType = "heavy";
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
                infoText.text = "Enemy:" + 1 + "ブロックを受ける";
                break;

            case "Reflection":
                dmg = 0;
                isReflect = true;
                if (isReflect == true)
                {
                    this.gameObject.GetComponent<Renderer>().material.color = new Color32(0, 255, 0, 255);
                }
                infoText.text = "Enemy:反射バリアを展開";
                audioSource.PlayOneShot(defenceSE);
                break;
        }

        if (cardScript.block != 0 && dmg > 0)
        {
            audioSource.PlayOneShot(parrySE);
            blockEffect();
            cardScript.block--;
            infoText.text += "\nYou:攻撃をブロック";
            infoText.color = Color.white;
        }
        else
        {
            // 武器別SE
            switch (SEType)
            {
                case "wait":
                    audioSource.PlayOneShot(waitSE);
                    break;

                case "copy":
                    audioSource.PlayOneShot(copySE);
                    break;

                case "light":
                    audioSource.PlayOneShot(attackSE);
                    break;

                case "heavy":
                    audioSource.PlayOneShot(heavyAttackSE);
                    break;

                case "boom":
                    audioSource.PlayOneShot(boomAttackSE);
                    break;

                case "poison":
                    audioSource.PlayOneShot(poisonSE);
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

        infoText.color = Color.white;

        if (count < (actionList.Count - 1)) count++;
        dmg = 0;
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
        infoText.color = Color.white;
        switch (enemy.GetComponent<SpriteRenderer>().sprite.name)
        {        
            case "OffensiveSlime":
                infoText.text = enemy.GetComponent<SpriteRenderer>().sprite.name + ":こう見えて何人も殺している凶暴なスライム";
                break;
            case "SwatSlime":
                infoText.text = enemy.GetComponent<SpriteRenderer>().sprite.name + ":たまたま拾った先進的な装備で身を固めたスライム";
                break;
            case "Lich":
                infoText.text = enemy.GetComponent<SpriteRenderer>().sprite.name + ":何故か鎌ではなくダイナマイトで死の宣告を行う変なリッチ";
                break;
            case "Knight":
                infoText.text = enemy.GetComponent<SpriteRenderer>().sprite.name + ":多彩な剣技を使う上級兵士\n無駄に高給取り";
                break;
            case "Spider":
                infoText.text = enemy.GetComponent<SpriteRenderer>().sprite.name + ":神経毒で脱力発作を起こそうとする邪悪な蜘蛛";
                break;
            case "Sn@il":
                infoText.text = enemy.GetComponent<SpriteRenderer>().sprite.name + ":何の変哲もない無害なカタツムリ";
                break;
            case "CopyBot":
                infoText.text = enemy.GetComponent<SpriteRenderer>().sprite.name + ":こちらの行動をほぼ完ぺきにコピーしてくる迷惑なボット";
                break;
            case "MirrorBot":
                infoText.text = enemy.GetComponent<SpriteRenderer>().sprite.name + ":バリア展開に特化したバリアフリーじゃないボット";
                break;
            case "Mine":
                infoText.text = enemy.GetComponent<SpriteRenderer>().sprite.name + ":気性の荒い地雷\n非常にデリケート";
                break;
            case "Ghost":
                infoText.text = enemy.GetComponent<SpriteRenderer>().sprite.name + ":リッチ以上の地位を保有している顔色の悪いゴースト";
                break;
            case "KingSlime":
                infoText.text = enemy.GetComponent<SpriteRenderer>().sprite.name + ":無駄に地位のあるただの老いぼれスライム";
                break;
        }
        if (SceneManager.GetActiveScene().name == "Tutorial") infoText.text += "\n右のハートを攻撃で減らせばプレイヤーの勝利となる";
    }
}
