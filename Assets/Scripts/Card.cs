/*
 * CardScript
 * Creator:西浦晃太 Update:2024/09/02
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

public class Card : MonoBehaviour
{
    // カード配置の親
    [SerializeField] GameObject parentCard;

    // GameEnd Button
    [SerializeField] GameObject button;

    // TurnEnd Button
    [SerializeField] GameObject turnEndButton;

    // Retry Button
    [SerializeField] GameObject retryButton;

    // Pause Button
    [SerializeField] GameObject pauseButton;

    // Pause Parent
    [SerializeField] GameObject pauseParent;

    // Pause Panel
    [SerializeField] GameObject pausePanel;

    // Option Panel
    [SerializeField] GameObject optionPanel;

    // Deck Panel
    [SerializeField] GameObject deckPanel;

    //Protect Icon
    [SerializeField] GameObject protectIcon;


    // Deck Card's Parent
    [SerializeField] GameObject deckCardParent;

     // Hand's Cards
    [SerializeField] List<GameObject> card;
    
    // Deck's Cards
    [SerializeField] List<GameObject> deckCard;

    // Deck Card's Name
    [SerializeField] string[] deckCardName;

    // Discard's CountText
    [SerializeField] Text discardText;

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

    //Defence's SoundEffect
    AudioClip defenceSE;

    //Clear's SoundEffect
    AudioClip clearSE;

    //Clear's SoundEffect
    AudioClip clickSE;

    //Defence's SoundEffect
    AudioClip reflectSE;

    // Discard's Count
    [SerializeField] int discardCnt;

    // Discard Target's GameObject
    GameObject discardTarget;

    // Enemy's Texture
    GameObject enemyTexture;

    // Enemy's HP
    GameObject[] enemyHP;

    // Passives
    List<string> passive;

    // Hand
    List<GameObject> handCard;

    //Passive Dictionary
    Dictionary<string, ItemResponse> passiveDictionary; 

    // HealthScript
    Health healthScript;

    // PassiveScript
    Passive passiveScript;

    // EnemyScript
    Enemy enemyScript;

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

    //Battle's Speed
    static int battleSpeed = 1000;

    // HP Count
    public int enemyLife;

    // Damage Value
    public int dmg;

    // SE Type
    string SEType = "";

    // Enemy's Dead
    public bool isDead;

    // Pause Check
    bool isPause;

    // Panel's Active Check
    bool panelActive;

    AudioSource audioSource;

    // Start is called before the first frame update
    void Start()
    {
        if (SceneManager.GetActiveScene().name == "Tutorial") infoText.text += "チュートリアルへようこそ！気になるアイコンをタップして詳細を確認しよう";
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
        reflectSE = (AudioClip)Resources.Load("SE/Reflect");
        clickSE = (AudioClip)Resources.Load("SE/Click");

        handCard = new List<GameObject>();
        SetCard();

        // Set Battle Speed
        // battleSpeed = (int)Math.Ceiling(gameSpeedSlider.value);
        gameSpeedSlider.value = battleSpeed;

        // Set GameState
        isDead = false;
        isPause = false;

        // Set Buttons
        retryButton.SetActive(false);
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
        enemyLife = healthScript.EnemHealth;

        enemyScript = FindObjectOfType<Enemy>();

        // Set Lists
        activeList = new List<GameObject>();
        selectedCard = new List<GameObject>();
        deckCard = new List<GameObject>();
        passive = new List<string>();
        // Set Counts
        count = 0;
        passiveCnt = 0;
        block = 0;
        dmg = 0;

        // Add Passive Value from got one
        passiveScript = FindObjectOfType<Passive>();


        // Set Deck's Cards From DeckPanel
        for (int i = 0; i < deckCardName.Length; i++)
        {
            // Get Card's GameObjects from Resources Folder
            GameObject prefab = (GameObject)Resources.Load("Cards/Card/" + deckCardName[i]);

            // Create Instance from Now Turn's Cards
            GameObject obj = Instantiate(prefab, new Vector2(-0.38f + (0.25f * i ), 0.14f), Quaternion.identity);
            obj.transform.localScale = new Vector2(0.035f,0.08f);
            obj.name = prefab.name;

            // Add Tag 
            if (prefab.tag == "Attack") obj.tag = "DeckAttack";
            else if (prefab.tag == "Defence") obj.tag = "DeckDefence";
            obj.transform.SetParent(deckCardParent.transform, false);
            deckCard.Add(obj);
            deckCard[i].GetComponent<BoxCollider2D>().enabled = false;
        }

        deckPanel.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonUp(0)) audioSource.PlayOneShot(clickSE);

        battleSpeed = (int)Math.Ceiling(gameSpeedSlider.value);
        discardText.text = "" + discardCnt;
        if (protectIcon != null) iconTxt.gameObject.GetComponent<Text>().text = block.ToString();

        if (enemyScript.isDead == true)
        {
            button.SetActive(true);
            retryButton.SetActive(true);
        }

        if(enemyLife <= 0)
        {
            isDead = true;

            for (int i = 0; i < selectedCard.Count; i++)
            { // Reset Color
                selectedCard[i].GetComponent<Renderer>().material.color = new Color32(255, 255, 255, 255);
            }

            retryButton.SetActive(true);
            button.SetActive(true);

            enemyTexture.SetActive(false);
        }
        // If Selected Card's Value Will 4
        if (activeList.Count == 4 && turnEndButton != null)
        {
            // Can Use TurnEnd Button
            turnEndButton.SetActive(true);
        }
        else if(activeList.Count != 4 && turnEndButton != null)
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
        if (isDead == true || isPause == true || enemyScript.isDead == true) return;

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
                    case "Wait":
                        infoText.text = "Wait:待機する";
                        break;

                    case "Destruction":
                        infoText.text = "Destruction:プレイヤーもろとも爆発する";
                        break;

                    case "Copy":
                        infoText.text = "Copy:プレイヤーの行動をコピーする";
                        break;

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
                        infoText.text = "M.A.C.E:1+ブロックの値分ダメージを与える";
                        break;

                    case "T.N.T":
                        infoText.text = "T.N.T:壊滅的なダメージを与える...";
                        break;

                    case "Poison":
                        infoText.text = "Poison:相手のダメージを1減らす";
                        break;

                    case "Shield":
                        infoText.text = "Shield:1ブロックを受ける";
                        break;

                    case "Reflection":
                        infoText.text = "Reflection:攻撃を反射するバリアを展開";
                        break;
                }
                if (SceneManager.GetActiveScene().name == "Tutorial") infoText.text += "\nプレイヤー同様敵も左から順に行動する";
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
                            infoText.text = "Sword:1ダメージを与える";
                            break;

                        case "S.Y.T.H":
                            infoText.text = "S.Y.T.H:2ダメージを与える";
                            break;

                        case "A.X.E":
                            infoText.text = "A.X.E:1ダメージを与える\nブロックを無視＆破壊";
                            break;

                        case "M.A.C.E":
                            infoText.text = "M.A.C.E:1+ブロックの値分ダメージを与える";
                            break;

                        case "T.N.T":
                            infoText.text = "T.N.T:壊滅的なダメージを与える...";
                            break;

                        case "Shield":
                            infoText.text = "Shield:1ブロックを受ける";
                            break;
                    }
                    if (activeList.Count ==4 && SceneManager.GetActiveScene().name == "Tutorial") infoText.text += "\n四枚選択した状態だと戦闘開始ボタンが押せる\n攻撃は必ずプレイヤーが先だと覚えておこう";
                    else if (SceneManager.GetActiveScene().name == "Tutorial") infoText.text += "\n手札は左から順に使用される\nもう一度タップすることで選択を解除できる";
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
                infoText.color = Color.white;
                switch (hitObj.name)
                {
                    case "Spike":
                        infoText.text = passiveDictionary["Spike"].Explain;
                        break;

                    case "ArmorChip":
                        infoText.text = passiveDictionary["ArmorChip"].Explain;
                        break;

                    case "Slime":
                        infoText.text = passiveDictionary["Slime"].Explain;
                        break;

                    case "HandGun":
                        infoText.text = passiveDictionary["HandGun"].Explain;
                        break;
                }
                if (SceneManager.GetActiveScene().name == "Tutorial") infoText.text += "\nこれらはパッシブといい\nカードに上記の効果を付与する";
            }
        }
    }

    /// <summary>
    /// Turn End Process
    /// </summary>
    public async void TurnEnd()
    {
        Destroy(turnEndButton);
        if(isDead == true || isPause == true || enemyScript.isDead == true) return;
        // Reset Info
        infoText.text = "";

        foreach (var item in activeList)
        {
            if (isDead == true || enemyScript.isDead == true ||  enemyScript.isDead == true) return;
            bool isBlock = false;
            switch (item.tag) //Judge the Card's Tag
            {
                case "Attack":
                    //Active to Passives
                    passiveEffect(item);

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
                            }else infoText.text = "You:渾身の一撃!\n" + dmg + "ダメージを与える!";
                            SEType = "heavy";

                            break;

                        case "T.N.T":
                            dmg += 999;
                            infoText.text = "You:ドカーン!\n" + dmg + "ダメージを与える!";
                            SEType = "boom";
                            break;
                    }

                    // If Enemy has Block
                    if (enemyScript.block != 0 && item.name != "A.X.E")
                    {
                        enemyScript.block--;
                        if (enemyScript.block <= 0) if (enemyScript.protectIcon != false) enemyScript.protectIcon.SetActive(false);
                        blockEffect();
                        isBlock = true;

                        audioSource.PlayOneShot(parrySE);
                        infoText.text = "Enemy:攻撃をブロック";
                    }
                    else
                    {
                        if (enemyScript.isReflect == true)
                        {
                            audioSource.PlayOneShot(reflectSE);
                            infoText.text += "\nしかし、攻撃は反射された";
                            for (int i = 0; i < dmg; i++)
                            {
                                //HP残量が0の場合、処理を行わない
                                if (enemyScript.playerLife <= 0)
                                {
                                    enemyScript.isDead = true;
                                    break;
                                }
                                //表示を減らす
                                Destroy(enemyScript.playerHP[(enemyScript.playerLife - 1)]);

                                //内部も減らす
                                enemyScript.playerLife--;
                            }
                            dmg = 0;
                            enemyScript.isReflect = false;
                        }
                        else
                        {
                            //Loop to Damage Values
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
                                Debug.Log("Enemy's HP:" + enemyLife);
                            }
                            dmg = 0;
                        }
                        if (enemyScript.playerLife <= 0) enemyScript.isDead = true;

                        // if Enemy Dead
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

                case "Defence":
                    audioSource.PlayOneShot(defenceSE);
                    passiveEffect(item);
                    switch (item.name)
                    {
                        case "Shield":
                            block++;
                            infoText.text = "You:" + block + "ブロックを受ける";
                            break;

                        case "SpikeShield":
                            block++;
                            infoText.text = "You:" + block +"ブロックを受ける\nシールドバッシュをかました！" + dmg + "ダメージを与える";

                            //HP残量が0の場合、処理を行わない
                            if (enemyLife <= 0)
                            {
                                break;
                            }
                            //表示を減らす
                            Destroy(enemyHP[(enemyLife - 1)]);

                            //内部も減らす
                            enemyLife--;
                            Debug.Log("Enemy's HP:" + enemyLife);

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
                    Debug.Log("補助");
                    passiveEffect(item);
                    break;

                default:
                    break;
            }

            item.SetActive(false);
            await Task.Delay(battleSpeed);

            if (isDead == true)
            {
                if (SceneManager.GetActiveScene().name != "Tutorial")
                {
                    NetworkManager networkManager = NetworkManager.Instance;
                    networkManager.ClearStage(int.Parse(SceneManager.GetActiveScene().name));
                }

                infoText.text = "敵を倒した！";
                audioSource.PlayOneShot(clearSE);
                return;
            } 
            if (enemyScript.isDead == true)
            {
                infoText.text += "\n死んでしまった...";
                return;
            }
            // Enemy's Action
            enemyScript.Attack();
            if (enemyScript.isDead == true)
            {
                infoText.text += "\n死んでしまった...";
                return;
            }

            // 
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
        if (SceneManager.GetActiveScene().name == "Tutorial") infoText.text += "失敗した？右に表示された矢印ボタンでやり直そう";
        button.SetActive(true);
        retryButton.SetActive(true);
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
                if (passive[passiveCnt] == "Spike")
                {
                    dmg++;
                }
                else if (passive[passiveCnt] == "Slime")
                {
                    enemyScript.dmg--;
                }
                break;
            case "Defence":

                if (passive[passiveCnt] == "Spike" && item.name == "Shield")
                {
                    dmg++;
                }
                if (passive[passiveCnt] == "ArmorChip")
                {
                    Debug.Log("アーマーチップ発動 DEF+1");

                    if (block <= 0)
                    {
                        //Get Card's GameObjects from Resources Folder
                        GameObject prefab = (GameObject)Resources.Load("ProtectIcon");

                        // Create Instance from Now Turn's Cards
                        protectIcon = Instantiate(prefab, new Vector2(-4.35f, -1.2f), Quaternion.identity);
                    }

                    block++;
                }
                break;
            default:
                break;
        }
        if (passiveCnt < (passive.Count - 1)) passiveCnt++;
    }

    public void endGame()
    {
        SceneManager.LoadScene("SelectScene");
    }
    public void retryGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    /// <summary>
    /// Attack's Effect Progress
    /// </summary>
    async void attackEffect(bool isBlock)
    {
        //Get Card's GameObjects from Resources Folder
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
    /// ShowDeck Progress
    /// </summary>
    public void showDeck()
    {
        if (isDead == true || isPause == true) return;
        // Reset Info

        if (SceneManager.GetActiveScene().name == "Tutorial") infoText.text = "このパネルは山札を参照できる\n右下のアイコンをタップすることで手札と交換ができる";
        else infoText.text = "";
        // Panel Active yet
        if (panelActive == false)
        {
            deckPanel.SetActive(true);
            panelActive = true;

            // Reset Selected Card
            for (int i = 0; i < selectedCard.Count; i++)
            { //Reset Color
                selectedCard[i].GetComponent<Renderer>().material.color = new Color32(255, 255, 255, 255);
            }

            for (int i = 0; i < activeList.Count; i++)
            { //Delete All Item from ActiveList
                Destroy(activeList[i]);
            }

            // Delete All List's Items
            selectedCard.Clear();
            activeList.Clear();
            count = 0;
        }
        else
        { //Panel is Already Active 
            deckPanel.SetActive(false);
            panelActive = false;

            // Reset Selected Card's Color
            foreach (var item in handCard)
            {
                item.GetComponent<Renderer>().material.color = new Color32(255, 255, 255, 255);
            }
            discardTarget = null;
        }
    }

    /// <summary>
    /// Discard Progress
    /// </summary>
    public void discard()
    {
        if (discardCnt > 0 && discardTarget != null)
        {
            int cnt = 0;
            foreach (var item in handCard)
            {
                if (item== discardTarget) //選択カードと現在のカードの名前が一致した場合
                {
                    // 各必要なオブジェクトのTransformを取得
                    Vector2 pos = item.transform.position;
                    Vector2 scale = item.transform.localScale;
                    // 山札の左端のカードを生成
                    GameObject obj = Instantiate(deckCard[0], new Vector2(pos.x, pos.y), Quaternion.identity);
                    // コライダーを設定
                    obj.AddComponent<BoxCollider2D>();
                    obj.GetComponent<BoxCollider2D>().isTrigger = true;
                    // スケールを調整
                    obj.transform.localScale = scale;
                    // 親に入れる
                    obj.transform.SetParent(parentCard.transform, false);
                    // 名前を修正
                    obj.name = deckCard[0].name;

                    // タグに応じた新たなタグを付与
                    switch (deckCard[0].tag)
                    {
                        case "DeckAttack": //攻撃タグの場合

                            obj.tag = "Attack";
                            break;

                        case "DeckDefence": //防御タグの場合

                            obj.tag = "Defence";
                            break;

                        default:
                            break;
                    }
                    obj.GetComponent<BoxCollider2D>().enabled = true;

                    // 交換先のカードの後ろへ追加し、交換先のカードを消す
                    handCard.Insert(cnt,obj);
                    handCard.Remove(item);
                    item.SetActive(false);

                    // Reset Selected Card's Color
                    foreach (var items in handCard)
                    {
                        items.GetComponent<Renderer>().material.color = new Color32(255, 255, 255, 255);
                    }

                    // Delete Card
                    Destroy(deckCard[0]);
                    // Remove from List
                    deckCard.Remove(deckCard[0]);

                    // 一時的に現在アクティブなカードを代入するリスト
                    List<GameObject> keepList = new List<GameObject>();
                    foreach (var card in deckCard) //Drstroy All Active Card & Add Assumed List
                    {
                        Destroy(card);
                        keepList.Add(card);
                    }
                    // Delete All List's Item
                    deckCard.Clear();

                    int refreshCnt = 0;
                    foreach (var card in keepList) //並べなおす
                    {
                        // 現在のカードプレハブを元に、インスタンスを生成、
                        GameObject objKeep = Instantiate(card, new Vector2(-0.38f + (0.25f * refreshCnt), 0.14f), Quaternion.identity);

                        // スケールを修正
                        objKeep.transform.localScale = new Vector2(0.035f, 0.08f);
                        // クローンしたオブジェクトの名前を訂正
                        objKeep.name = card.name;
                        // カードのタグをクローンオブジェクトにも追加
                        objKeep.tag = card.tag;
                        // 親に突っ込む
                        objKeep.transform.SetParent(deckCardParent.transform, false);

                        deckCard.Add(objKeep);
                        // 順序を加算
                        refreshCnt++;
                    }

                    deckPanel.SetActive(false);
                    panelActive = false;
                    discardTarget = null;
                    break;
                }
                cnt++;
            }
            // 交換可能回数を減らす
            discardCnt--;
        }
        else return;
    }

    /// <summary>
    /// Pause Button Progress
    /// </summary>
    public void pause()
    {
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