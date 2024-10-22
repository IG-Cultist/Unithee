/*
 * BattleModePlayerScript
 * Creator:西浦晃太 Update:2024/10/21
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using UnityEngine.UI;
using Unity.VisualScripting;
using System;
using Unity.Collections.LowLevel.Unsafe;

public class BattleModePlayer : MonoBehaviour
{
    // カード配置の親
    [SerializeField] GameObject parentCard;

    // GameEnd Button
    [SerializeField] GameObject button;

    // TurnEnd Button
    [SerializeField] GameObject turnEndButton;

    // Pause Button
    [SerializeField] GameObject pauseButton;

    // Pause Parent
    [SerializeField] GameObject pauseParent;

    // Pause Panel
    [SerializeField] GameObject pausePanel;

    // Option Panel
    [SerializeField] GameObject optionPanel;

    //Protect Icon
    [SerializeField] GameObject protectIcon;

    // Hand's Cards
    [SerializeField] List<GameObject> card;

    // Deck Panel
    [SerializeField] Text infoText;

    // GameSpeed Slider
    [SerializeField] Slider gameSpeedSlider;

    // Attack's SoundEffect
    AudioClip attackSE;

    // Heavy Attack's SoundEffect
    AudioClip heavyAttackSE;

    // Boom Attack's SoundEffect
    AudioClip boomAttackSE;

    //Parry's SoundEffect
    AudioClip parrySE;

    // Defence's SoundEffect
    AudioClip defenceSE;

    // Clear's SoundEffect
    AudioClip clearSE;

    // Clear's SoundEffect
    AudioClip clickSE;

    // Discard Target's GameObject
    GameObject discardTarget;

    // Enemy's Texture
    GameObject enemyTexture;

    // Enemy's HP
    public GameObject[] enemyHP;

    // Passives
    List<string> passive;

    // Hand
    List<GameObject> handCard;

    // Passive Dictionary
    Dictionary<string, ItemResponse> passiveDictionary;

    // HealthScript
    Health healthScript;

    // PassiveScript
    Passive passiveScript;

    // EnemyScript
    BattleModeEnemy enemyScript;

    // Selected Cards
    List<GameObject> selectedCard;

    Transform iconTxt;

    // Active Card's List
    public List<GameObject> activeList;
    // Defence Value
    public int block;

    // Card Turn's Count
    int count;

    // Passive Turn's Count
    int passiveCnt;

    // Battle's Speed
    static int battleSpeed = 1000;

    // HP Count
    public int enemyLife;

    // Damage Value
    public int dmg;

    // SE Type
    string SEType = "";

    // Enemy's Dead
    public bool isDead;

    // 出血判定
    public bool isBleeding;

    // Pause Check
    bool isPause;

    // 武器強化フラグ
    bool isForge;

    // ピストル使用可能フラグ
    bool isReload;

    bool isFight;
    // Panel's Active Check
    bool panelActive;

    AudioSource audioSource;

    // Start is called before the first frame update
    void Start()
    {
        iconTxt = protectIcon.transform.Find("Text");
        // SetSE
        this.gameObject.AddComponent<AudioSource>();
        audioSource = GetComponent<AudioSource>();

        attackSE = (AudioClip)Resources.Load("SE/NormalAttack");
        heavyAttackSE = (AudioClip)Resources.Load("SE/HeavyAttack");
        boomAttackSE = (AudioClip)Resources.Load("SE/Boom");
        parrySE = (AudioClip)Resources.Load("SE/Parry");
        defenceSE = (AudioClip)Resources.Load("SE/Defence");
        clearSE = (AudioClip)Resources.Load("SE/Clear");
        clickSE = (AudioClip)Resources.Load("SE/Click");

        // 手札カードを設定
        handCard = new List<GameObject>();
        SetCard();

        // Set Battle Speed
        // battleSpeed = (int)Math.Ceiling(gameSpeedSlider.value);
        gameSpeedSlider.value = battleSpeed;

        // Set GameState
        isDead = false;
        isPause = false;
        isBleeding = false;
        isFight = false;
        isForge = false;
        isReload = false;

        // Set Buttons
        turnEndButton.SetActive(false);
        button.SetActive(false);
        protectIcon.SetActive(false);

        // Set Panels
        panelActive = false;

        // Get for Enemy Object from tag
        enemyTexture = GameObject.FindGameObjectWithTag("Enemy");
        // Get for Enemy Life from tag
        enemyHP = GameObject.FindGameObjectsWithTag("EnemyHP");
        // Add Health Value from got one
        healthScript = FindObjectOfType<Health>();
        enemyLife = healthScript.BattleModeHealth;

        enemyScript = FindObjectOfType<BattleModeEnemy>();

        // Set Lists
        activeList = new List<GameObject>();
        selectedCard = new List<GameObject>();

        // Set Counts
        count = 0;
        passiveCnt = 0;
        block = 0;
        dmg = 0;

        // Add Passive Value from got one
        passiveScript = FindObjectOfType<Passive>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonUp(0)) audioSource.PlayOneShot(clickSE);

        battleSpeed = (int)Math.Ceiling(gameSpeedSlider.value);
        if (protectIcon != null) iconTxt.gameObject.GetComponent<Text>().text = block.ToString();

        if (enemyScript.isDead == true)
        {
            button.SetActive(true);
        }

        if (enemyLife <= 0)
        {
            isDead = true;

            for (int i = 0; i < selectedCard.Count; i++)
            { // Reset Color
                selectedCard[i].GetComponent<Renderer>().material.color = new Color32(255, 255, 255, 255);
            }

            button.SetActive(true);

            enemyTexture.SetActive(false);
        }
        // If Selected Card's Value Will 4
        if (activeList.Count == 4 && turnEndButton != null)
        {
            // Can Use TurnEnd Button
            turnEndButton.SetActive(true);
        }
        else if (activeList.Count != 4 && turnEndButton != null)
        {
            turnEndButton.SetActive(false);
        }

        // Select Card
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
        if (isDead == true || isPause == true || enemyScript.isDead == true || isFight == true) return;

        // Shot Ray from Touch Point
        Vector2 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        // 第二引数 レイはどの方向に進むか(zero=指定点)
        RaycastHit2D hit2d = Physics2D.Raycast(worldPosition, Vector2.zero);

        // Hit Process
        if (hit2d)
        {
            //ヒットしたオブジェクト取得
            GameObject hitObj = hit2d.collider.gameObject;

            if (panelActive == true)
            {
                foreach (var item in handCard)
                {
                    item.GetComponent<Renderer>().material.color = new Color32(255, 255, 255, 255);
                }
                discardTarget = null;

                if (!selectedCard.Contains(hitObj)) //Select yet
                {
                    // Change the Card's Color
                    hitObj.GetComponent<Renderer>().material.color = new Color32(127, 127, 127, 255);
                    discardTarget = hitObj;
                }
            }
            else if (panelActive != true && hitObj.tag == "Enemy")
            {
                enemyScript.EnemyExplain(hitObj);
            }
            else if (panelActive != true && hitObj.tag == "EnemyAction")
            {
                infoText.color = Color.white;
                switch (hitObj.name)
                {
                    case "Sword":
                        infoText.text = "Sword:1ダメージを与える";
                        break;

                    case "DeathS.Y.T.H":
                        infoText.text = "DeathS.Y.T.H:3ダメージを与える顔色？の悪い大鎌";
                        break;

                    case "S.Y.T.H":
                        infoText.text = "S.Y.T.H:2ダメージを与える";
                        break;

                    case "A.X.E":
                        infoText.text = "A.X.E:1ダメージを与える\nブロックを無視＆破壊";
                        break;

                    case "M.A.C.E":
                        infoText.text = "M.A.C.E:1ダメージに加えブロックの値分ダメージを与える";
                        break;

                    case "T.N.T":
                        infoText.text = "T.N.T:壊滅的なダメージを与える...";
                        break;

                    case "Shield":
                        infoText.text = "Shield:1ブロックを受ける";
                        break;

                    case "Reflection":
                        infoText.text = "Reflection:攻撃を反射するバリアを展開";
                        break;

                    case "ForgeHammer":
                        infoText.text = hitObj.name + ":1ダメージを与える\n次の行動の攻撃力+1";
                        break;

                    case "Injector":
                        infoText.text = hitObj.name + ":1ダメージを与える\n敵を出血させる(行動毎1ダメージ)";
                        break;

                    case "PoisonKnife":
                        infoText.text = hitObj.name + ":1ダメージを与える\n敵の攻撃力-1";
                        break;

                    case "6mmBullet":
                        infoText.text = hitObj.name + ":3ダメージを与える\n...銃があればの話";
                        break;

                    case "SwatShield":
                        infoText.text = hitObj.name + ":2ブロックを受ける";
                        break;
                }
            }
            else if (panelActive != true && hitObj.tag != "passive")
            {
                if (!selectedCard.Contains(hitObj)) //Select yet
                {
                    // Change the Card's Color
                    hitObj.GetComponent<Renderer>().material.color = new Color32(127, 127, 127, 255);
                    // Add to List Selected Card
                    selectedCard.Add(hitObj);

                    // Get Card's GameObjects from Resources Folder
                    GameObject prefab = (GameObject)Resources.Load("Cards/" + hitObj.name);

                    // Create Instance from Now Turn's Cards
                    GameObject obj = Instantiate(prefab, new Vector2(-8.2f + (2.0f * count), -4.1f), Quaternion.identity);
                    // Reset Object's Color
                    obj.GetComponent<Renderer>().material.color = new Color32(255, 255, 255, 255);

                    // Rename Item
                    obj.name = hitObj.name;
                    // Add Tag for Clone Items
                    obj.tag = hitObj.tag;
                    // Add ActiveList
                    activeList.Add(obj);

                    // Add count
                    count++;
                    infoText.color = Color.white;
                    // Update Info 
                    switch (hitObj.name)
                    {
                        case "Sword":
                            infoText.text = hitObj.name + ":1ダメージを与える";
                            break;

                        case "S.Y.T.H":
                            infoText.text = hitObj.name + ":2ダメージを与える";
                            break;

                        case "A.X.E":
                            infoText.text = hitObj.name + ":1ダメージを与える\nブロックを無視＆破壊";
                            break;

                        case "M.A.C.E":
                            infoText.text = hitObj.name + ":1ダメージに加えブロックの値分ダメージを与える";
                            break;

                        case "T.N.T":
                            infoText.text = hitObj.name + ":壊滅的なダメージを与える...";
                            break;

                        case "Shield":
                            infoText.text = hitObj.name + ":1ブロックを受ける";
                            break;

                        case "ForgeHammer":
                            infoText.text = hitObj.name + ":1ダメージを与える\n次の行動の攻撃力+1";
                            break;

                        case "Injector":
                            infoText.text = hitObj.name + ":1ダメージを与える\n敵を出血させる(行動毎1ダメージ)";
                            break;

                        case "PoisonKnife":
                            infoText.text = hitObj.name + ":1ダメージを与える\n敵の攻撃力-1";
                            break;

                        case "6mmBullet":
                            infoText.text = hitObj.name + ":3ダメージを与える\n...銃があればの話";
                            break;

                        case "SwatShield":
                            infoText.text = hitObj.name + ":2ブロックを受ける";
                            break;
                    }
                }
                else //Already Selected
                {
                    // Reset Selected Card's Color
                    hitObj.GetComponent<Renderer>().material.color = new Color32(255, 255, 255, 255);
                    // Remove Selected Card from Lists
                    selectedCard.Remove(hitObj);

                    foreach (var item in activeList)
                    {
                        if (item.name == hitObj.name)
                        {
                            // Delete Card
                            Destroy(item);
                            // Remove from List
                            activeList.Remove(item);
                            // Refresh
                            cardRefresh(activeList);
                            break;
                        }
                    }
                }
            }
            else if (panelActive != true && hitObj.tag == "passive")
            {
                // タッチしたオブジェクトをディレクショナリから取得
                infoText.text = passiveDictionary[hitObj.name].Name + ":" + passiveDictionary[hitObj.name].Explain;
            }
        }
    }

    /// <summary>
    /// Turn End Process
    /// </summary>
    public async void TurnEnd()
    {
        isFight = true;
        Destroy(turnEndButton);
        if (isDead == true || isPause == true || enemyScript.isDead == true) return;
        // Reset Info
        infoText.text = "";

        foreach (var item in activeList)
        {
            if (isDead == true || enemyScript.isDead == true || enemyScript.isDead == true) return;
            bool isBlock = false;
            switch (item.tag) //Judge the Card's Tag
            {
                case "Attack": //カードタイプが攻撃の場合
                    //Active to Passives
                    passiveEffect(item);

                    // カード名別処理
                    switch (item.name)
                    {
                        case "Sword":
                            dmg += 1;
                            infoText.text = "You:" + dmg + "ダメージを与える";
                            SEType = "light";
                            break;

                        case "A.X.E":
                            dmg += 1;
                            enemyScript.block = 0;
                            infoText.text = "You:シールド破壊！\n" + dmg + "ダメージを与える";
                            SEType = "heavy";
                            break;

                        case "S.Y.T.H":
                            dmg += 2;
                            infoText.text = "You:" + dmg + "ダメージを与える";
                            SEType = "heavy";
                            break;

                        case "M.A.C.E":
                            dmg += block;
                            if (dmg < 3)
                            {
                                infoText.text = "You:" + dmg + "ダメージを与える";
                            }
                            else infoText.text = "You:渾身の一撃!\n" + dmg + "ダメージを与える!";
                            SEType = "heavy";

                            break;

                        case "T.N.T":
                            dmg += 999;
                            infoText.text = "You:ドカーン!\n" + dmg + "ダメージを与える!";
                            SEType = "boom";
                            break;

                        case "ForgeHammer":
                            dmg += 1;
                            infoText.text = "You:" + dmg + "ダメージを与える\n次の行動で攻撃力+1";
                            SEType = "heavy";
                            isForge = true;
                            break;

                        case "Injector":
                            enemyScript.isBleeding = true;
                            dmg += 1;
                            infoText.text = "You:" + dmg + "ダメージを与える\n相手に出血を付与!";
                            SEType = "light";
                            break;

                        case "PoisonKnife":
                            enemyScript.dmg -= 1;
                            dmg += 1;
                            infoText.text = "You:" + dmg + "ダメージを与える\n相手は毒により攻撃力-1";
                            SEType = "light";
                            break;

                        case "6mmBullet":
                            if (isReload == true)
                            {
                                dmg += 3;
                                infoText.text = "You:素早く弾丸を込め、トリガーを引いた！\n" + dmg + "ダメージを与える";
                                isReload = false;
                            }
                            else
                            {
                                dmg = 0;
                                infoText.text = "You:仕方なく弾丸を投げた！\n意味がない！" + dmg + "ダメージを与える";
                            }

                            SEType = "light";
                            break;
                    }

                    // 敵がブロックを持っているかつ現カードがA.X.Eではない場合
                    if (enemyScript.block != 0 && item.name != "A.X.E")
                    {
                        // ブロック値を減らす
                        enemyScript.block--;
                        if (enemyScript.block <= 0) if (enemyScript.protectIcon != false) enemyScript.protectIcon.SetActive(false);
                        blockEffect();
                        isBlock = true;

                        // 対応した音声とUIを出す
                        audioSource.PlayOneShot(parrySE);
                        infoText.text = "Rival:攻撃をブロック";
                        dmg = 0;
                    }
                    else
                    {
                        // 現攻撃値分繰り返す
                        for (int i = 0; i < dmg; i++)
                        {
                            //HP残量が0の場合、処理を行わない
                            if (enemyLife <= 0)
                            {
                                break;
                            }
                            //表示を減らす
                            Destroy(enemyHP[(enemyLife - 1)]);

                            //内部も減らす
                            enemyLife--;
                        }
                        if (enemyScript.playerLife <= 0) enemyScript.isDead = true;

                        // 敵が死んだ場合
                        if (enemyLife <= 0) isDead = true;
                    }
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
                    }

                    attackEffect(isBlock);
                    item.SetActive(false);
                    break;

                case "Defence":　//カードタイプが防御の場合
                    audioSource.PlayOneShot(defenceSE);
                    passiveEffect(item);
                    switch (item.name)
                    {
                        case "Shield":
                            block++;
                            infoText.text = "You:" + block + "ブロックを受ける";
                            break;

                        case "SwatShield":
                            block += 2;
                            infoText.text = "You:" + block + "ブロックを受ける";
                            break;

                        case "SpikeShield":
                            block++;
                            infoText.text = "You:" + block + "ブロックを受ける\nシールドバッシュをかました！" + dmg + "ダメージを与える";

                            //HP残量が0の場合、処理を行わない
                            if (enemyLife <= 0)
                            {
                                break;
                            }
                            //表示を減らす
                            Destroy(enemyHP[(enemyLife - 1)]);

                            //内部も減らす
                            enemyLife--;

                            // if Enemy Dead
                            if (enemyLife <= 0) isDead = true;

                            attackEffect(isBlock);
                            break;
                    }

                    // ブロック値が0以上かつアイコンが生成されていない場合
                    if (block > 0)
                    {
                        protectIcon.SetActive(true);
                        iconTxt.gameObject.GetComponent<Text>().text = block.ToString();
                    }
                    break;

                case "Support":
                    passiveEffect(item);
                    break;

                default:
                    break;
            }

            item.SetActive(false);
            await Task.Delay(battleSpeed);

            // 敵が死亡していた場合
            if (isDead == true)
            {
                // テキストを表示し、クリアサウンドを再生
                infoText.text = "勝利！";
                audioSource.PlayOneShot(clearSE);
                return;
            }

            // 死んだ場合テキストを表示し、リターンする
            if (enemyScript.isDead == true)
            {
                infoText.text += "\n敗北...";
                return;
            }

            // Enemy's Action
            enemyScript.Attack();

            // 敵が死亡かつプレイヤーが死亡していない場合
            if (isDead == true && enemyScript.isDead != true)
            {
                // 敵死亡時のテキスト表示分岐
                if (enemyScript.isBleeding == true)
                {
                    infoText.text = "\nRival:出血死!\n勝利！";
                }
                audioSource.PlayOneShot(clearSE);
                return;
            }

            //　プレイヤー死亡時のテキスト表示
            if (enemyScript.isDead == true)
            {
                infoText.text += "\n敗北...";
                return;
            }

            // フォージハンマーを使用した場合DMG+1
            if (isForge == true)
            {
                dmg = 1;
                isForge = false;
            }
            else dmg = 0;

            if (block <= 0) if (protectIcon != null) protectIcon.SetActive(false);
            await Task.Delay(battleSpeed);
        }

        // Reset Color
        for (int i = 0; i < selectedCard.Count; i++) selectedCard[i].GetComponent<Renderer>().material.color = new Color32(255, 255, 255, 255);
        // Delete All Item from ActiveList
        for (int i = 0; i < activeList.Count; i++) Destroy(activeList[i]);

        // Delete All List's Items
        selectedCard.Clear();
        activeList.Clear();
        // Reset Count
        count = 0;
        infoText.text = "撃破失敗...\nライバルの勝利";
        button.SetActive(true);
    }

    /// <summary>
    /// Card Refresh Process
    /// </summary>
    void cardRefresh(List<GameObject> refreshTarget)
    {
        // 一時的に現在アクティブなカードを代入するリスト
        List<GameObject> keepList = new List<GameObject>();
        foreach (var item in refreshTarget) //Drstroy All Active Card & Add Assumed List
        {
            Destroy(item);
            keepList.Add(item);
        }
        // Delete All List's Item
        refreshTarget.Clear();
        // Reset Count
        count = 0;

        foreach (var item in keepList) //並べなおす
        {
            //現在のカードプレハブを元に、インスタンスを生成、
            GameObject obj = Instantiate(item, new Vector2(-8.2f + (2.0f * count), -4.1f), Quaternion.identity);
            //オブジェクトの色を訂正
            obj.GetComponent<Renderer>().material.color = new Color32(255, 255, 255, 255);
            //クローンしたオブジェクトの名前を訂正
            obj.name = item.name;
            //カードのタグをクローンオブジェクトにも追加
            obj.tag = item.tag;
            refreshTarget.Add(obj);
            //順序を加算
            count++;
        }
    }

    /// <summary>
    /// Passive's Effect Process
    /// </summary>
    /// <param name="item"></param>
    void passiveEffect(GameObject item)
    {
        //Select to Passive's name
        switch (item.tag)
        {
            case "Attack":
                switch (passive[passiveCnt])
                {
                    case "Spike":
                        dmg++;
                        break;
                    case "Slime":
                        enemyScript.dmg--;
                        break;
                    case "VampireWrench":
                        enemyScript.playerLife++;
                        break;
                    case "HandyDrill":
                        enemyScript.block = 0;
                        break;

                    case "HandGun":
                        if (item.name == "6mmBullet")
                        {
                            isReload = true;
                        }
                        break;
                    default:
                        break;
                }
                break;
            case "Defence":
                switch (passive[passiveCnt])
                {
                    case "Spike":
                        if (item.name == "Shield")
                        {
                            dmg++;
                        }
                        break;
                    case "ArmorChip":

                        if (block <= 0)
                        {
                            protectIcon.SetActive(true);
                            iconTxt.gameObject.GetComponent<Text>().text = block.ToString();
                        }

                        block++;
                        break;
                    default:
                        break;
                }
                break;
            default:
                break;
        }
        if (passiveCnt < (passive.Count - 1)) passiveCnt++;
    }

    public void endGame()
    {
        SceneManager.LoadScene("Battle");
    }

    /// <summary>
    /// Attack's Effect Progress
    /// </summary>
    async void attackEffect(bool isBlock)
    {
        // Get Card's GameObjects from Resources Folder
        GameObject prefab = (GameObject)Resources.Load("AttackEffect");

        // Create Instance from Now Turn's Cards
        GameObject obj = Instantiate(prefab, new Vector2(0.0f, 1.4f), Quaternion.identity);
        await Task.Delay(100);
        Destroy(obj);

        if (isBlock != true)
        {
            for (int i = 0; i < 5; i++)
            {
                enemyTexture.GetComponent<Renderer>().material.color = new Color32(255, 255, 255, 0);
                await Task.Delay(10);
                enemyTexture.GetComponent<Renderer>().material.color = new Color32(255, 255, 255, 255);
                await Task.Delay(10);
            }
        }
    }

    /// <summary>
    /// Block's Effect Progress
    /// </summary>
    async void blockEffect()
    {
        //Get Card's GameObjects from Resources Folder
        GameObject prefab = (GameObject)Resources.Load("BlockEffect");

        // Create Instance from Now Turn's Cards
        GameObject obj = Instantiate(prefab, new Vector2(0.0f, 1.0f), Quaternion.identity);
        await Task.Delay(100);
        Destroy(obj);
    }

    /// <summary>
    /// Pause Button Progress
    /// </summary>
    public void pause()
    {
        if (isFight == true) return;

        infoText.text = "";
        pauseParent.SetActive(true);
        Time.timeScale = 0.0f;
        isPause = true;
        pausePanel.SetActive(true);
    }

    /// <summary>
    /// Close Pause Panel Progress
    /// </summary>
    public void closePause()
    {
        pauseParent.SetActive(false);
        Time.timeScale = 1.0f;
        isPause = false;
        pausePanel.SetActive(false);
    }

    /// <summary>
    /// Option Panel Progress
    /// </summary>
    public void openOption()
    {
        pausePanel.SetActive(false);
        optionPanel.SetActive(true);
    }

    /// <summary>
    /// Close Option Panel Progress
    /// </summary>
    public void closeOption()
    {
        pausePanel.SetActive(true);
        optionPanel.SetActive(false);
    }

    public void SetPassives(List<string> passiveList, Dictionary<string, ItemResponse> dictionary)
    {
        passive = passiveList;
        passiveDictionary = dictionary;
    }

    /// <summary>
    /// 手札カードを表示する処理
    /// </summary>
    void SetCard()
    {
        int cnt = 0;

        foreach (var cards in card)
        {
            //  Get Prefabs from List
            GameObject obj = (GameObject)Resources.Load("Cards/Card/" + cards.name);
            // Create Action Objects
            GameObject item = Instantiate(obj, new Vector2(1.17f + (2.2f * cnt), -3.4f), Quaternion.identity);
            // Rename
            item.name = cards.name;

            item.transform.SetParent(parentCard.transform, false);
            handCard.Add(item);
            cnt++;
        }
    }
}