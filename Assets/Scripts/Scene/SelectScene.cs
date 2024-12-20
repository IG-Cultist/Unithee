/*
 * SelectSceneScript
 * Creator:西浦晃太 Update:2024/11/07
*/
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Threading.Tasks;
using UnityEngine.Rendering;
public class SelectScene : MonoBehaviour
{
    // ステージ情報
    [SerializeField] GameObject info;

    // ステージボタン
    [SerializeField] GameObject btnPrefab;

    // 説明ボタン
    [SerializeField] GameObject infoButton;

    // Card Explain Text
    [SerializeField] Text infoTxt;

    // デッキ種別テキスト
    [SerializeField] Text deckTxt;

    // 警告テキスト
    [SerializeField] Text warning;

    // ステージの親
    [SerializeField] GameObject stageParent;

    // カードの親
    [SerializeField] GameObject cardParent;

    // ステージ解説パネル
    [SerializeField] GameObject infoPanel;

    // デッキ構築パネル
    [SerializeField] GameObject deckBuildPanel;

    // カード参照パネル
    [SerializeField] GameObject showCardPanel;

    // ビルドパネル
    [SerializeField] GameObject buildPanel;

    // カードパネル
    [SerializeField] GameObject cardViewPanel;

    // アイコンパネル
    [SerializeField] GameObject iconPanel;

    // 詳細パネル
    [SerializeField] GameObject helpPanel;

    // カード親
    [SerializeField] GameObject cardViewParent;

    // 現在のデッキの親
    [SerializeField] GameObject activeDeckParent;

    // 現在の防衛デッキの親
    [SerializeField] GameObject activeDefenceDeckParent;

    //メインデッキの親
    [SerializeField] GameObject mainDeckPanel;

    // 防衛デッキの親
    [SerializeField] GameObject defenceDeckPanel;

    // ローディングパネル
    [SerializeField] GameObject loadingPanel;

    // ローディングアイコン
    [SerializeField] GameObject loadingIcon;

    // Deck Panel
    [SerializeField] Text infoText;

    // プレイヤー名
    [SerializeField] Text playerName;

    // Clear's SoundEffect
    AudioClip clickSE;

    public bool isClick = false;
    bool isSet;

    // デッキデータ用スクリプト
    DeckData deckData;

    AudioSource audioSource;

    GameObject nowIcon;

    Image iconImage;
    int iconNum;
    string iconName = "icon000";


    // Start is called before the first frame update
    void Start()
    {
        // 非同期処理完了まで待機させる
        Loading();
        isSet = false;

        // SetSE
        this.gameObject.AddComponent<AudioSource>();
        audioSource = GetComponent<AudioSource>();

        clickSE = (AudioClip)Resources.Load("SE/Click");

        // 全てのパネルを閉じる
        infoPanel.SetActive(false);
        info.SetActive(false);
        deckBuildPanel.SetActive(false);
        showCardPanel.SetActive(false);
        defenceDeckPanel.SetActive(false);
        helpPanel.SetActive(false);
        iconPanel.SetActive(false);

        // オブジェクトの取得
        nowIcon = GameObject.Find("MyIcon");

        // コンポーネントの取得
        iconImage = nowIcon.GetComponent<Image>();

        // デッキデータスクリプトを取得
        deckData = FindObjectOfType<DeckData>();

        // ステージ情報取得
        NetworkManager networkManager = NetworkManager.Instance;
        //if (networkManager.displayName == "")
        //{
        //    playerName.text = "未設定";
        //}else playerName.text = networkManager.displayName;

        //if (networkManager.iconName != "")
        //{
        //    Texture2D texture = Resources.Load("Icons/" + networkManager.iconName) as Texture2D;

        //    iconImage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),
        //                                   Vector2.zero);
        //}

        List<int> stageIDs = networkManager.GetID();
        StartCoroutine(NetworkManager.Instance.GetStage(stages =>
        {
            int cnt = 0;
            foreach (var stage in stages)
            {
                // Create Stage Button from Server
                GameObject btn = new GameObject();
                Destroy(btn);
                if (cnt < 5)
                {
                    btn = Instantiate(btnPrefab, new Vector2(-550 + (150 * cnt), 300), Quaternion.identity);
                }
                else
                {
                    btn = Instantiate(btnPrefab, new Vector2(-550 + (150 * (cnt - 5)), 150), Quaternion.identity);
                }

                // Rename for StageID
                btn.name =/*"Stage" + */ stage.StageID.ToString();

                //Change Button's Text for StageID
                Transform btnText = btn.transform.Find("Text");
                btnText.gameObject.GetComponent<Text>().text = stage.StageID.ToString();

                // Add Button in Canvas
                string btnName = btn.name;
                btn.transform.SetParent(this.stageParent.transform, false);
                btn.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => selectStage(btnName));

                if (stageIDs != null && stageIDs.Contains(stage.StageID))
                {
                    btn.GetComponent<Image>().color = Color.yellow;
                }
                cnt++;
            }
        }));

        StartCoroutine(NetworkManager.Instance.GetMyProfile(user =>
        {
            if (user.Length == 0)
            {
                StartCoroutine(NetworkManager.Instance.StoreProfile());
            }
            else
            {
                foreach (var profile in user)
                {
                    if (profile.Name != "")
                    {
                        playerName.text = profile.Name;
                    }

                    if (profile.IconName != "")
                    {
                        Texture2D texture = Resources.Load("Icons/" + profile.IconName) as Texture2D;

                        iconImage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),
                                                       Vector2.zero);
                    }
                }
            }
        }));
    }

    void Update()
    {
        if (Input.GetMouseButtonUp(0)) audioSource.PlayOneShot(clickSE);

        if (Input.GetMouseButtonUp(1)) CheckSomething();
    }

    /// <summary>
    /// Open Info Panel
    /// </summary>
    /// <param name="btnName"></param>
    public void selectStage(string btnName)
    {
        // Change Info's StageID
        Transform infoText = info.transform.Find("Text");
        infoText.gameObject.GetComponent<Text>().text = btnName;

        // Infoパネルのみを開き、それ以外を閉じる
        info.SetActive(true);
        deckBuildPanel.SetActive(false);
        infoPanel.SetActive(false);
        showCardPanel.SetActive(false);
    }

    /// <summary>
    /// デッキ追加処理
    /// </summary>
    void AddDeck(GameObject obj)
    {
        warning.text = "";
        List<int> activeList;
        List<string> otherList;
        int requestValue;
        if (isClick == false)
        {
            activeList = deckData.GetDeck();
            otherList = deckData.GetDefenceDeckName();
            requestValue = 1;
        }
        else
        {
            activeList = deckData.GetDefenceDeck();
            otherList = deckData.GetDeckName();
            requestValue = 2;
        }

        // 選択されたオブジェクト番号をintにする
        int cardID = deckData.ConvertName(obj.name);
        
        // 対象オブジェクトの状態を確認
        bool isSelected = deckData.CheckUsable(obj.name);

        if (otherList.Contains(obj.name))
        {
            warning.text = "そのカードは別のデッキに含まれている！";
        }
        else if (isSelected == true)
        { // 選択されていた場合
            // 状態を未選択にする
            deckData.UpdateUsable(obj.name, 0);
            // 選択カードIDリストから除去
            activeList.Remove(cardID);
        }
        else if(isSelected == false && activeList.Count < 4) 
        { // 対象オブジェクトが未選択かつ現在のデッキ数が4未満の場合
            // 選択状態にする
            deckData.UpdateUsable(obj.name, requestValue);
            // 選択カードIDリストに追加
            activeList.Add(cardID);
        }
        else if (activeList.Count == 4)
        {
            warning.text = "これ以上は追加できない！";
        }
        // スクロールビューを更新
        UpdaetView(obj);
    }

    /// <summary>
    /// カード一覧更新処理
    /// </summary>
    void UpdaetView(GameObject obj)
    {
        List<List<int>> usableCards = deckData.GetUsable();
        for (int i = 1; i <= 9; i++)
        {
            // 4回ループ
            for (int j = 1; j <= 4; j++)
            {
                // 文字列判定
                if (obj.name == i.ToString() + "," + j.ToString())
                {
                    // 選択されているなら色を変更
                    if (usableCards[i - 1][j - 1] == 1)
                    {
                        obj.GetComponent<Image>().color = Color.gray;
                    }
                    else if (usableCards[i - 1][j - 1] == 2)
                    {
                        obj.GetComponent<Image>().color =Color.blue;
                    }
                    else
                    { //色をリセット
                        obj.GetComponent<Image>().color = Color.white;
                    }
                }
            }
        }
    }

    /// <summary>
    /// デッキ表示処理
    /// </summary>
    public void UpdateDeck(List<int> id)
    {
        GameObject parent;
        if (isClick == false)
        {
            parent = activeDeckParent;
        }
        else
        {
            parent = activeDefenceDeckParent;
        }

        foreach (Transform n in parent.transform)
        {
            GameObject.Destroy(n.gameObject);
        }

        // 各カードのスタック数分ループ
        for (int i = 0; i < id.Count; i++)
        {
            if (id[i] == 0) continue;
            // 同名のカードをリソースファイルから取得
            GameObject obj = (GameObject)Resources.Load("Cards(ID)/" + id[i]);
            // 取得したカードを生成
            GameObject cards = Instantiate(obj, new Vector2(-430f + (280f * i), 0f), Quaternion.identity);

            cards.name = obj.name;
            cards.transform.localScale = new Vector2(1.7f,2.4f);
            // メインデッキパネルに生成
            cards.transform.SetParent(parent.transform, false);
        }
    }

    /// <summary>
    /// ランダムネームコンバート処理
    /// </summary>
    public void randomName()
    {   
        System.Random rand = new System.Random();
        // ファストネーム定義
        string[] firstName = new string[]{
            "Nice","Abnormal","Delicious","Difficulty","Mr",
            "Mrs","Master","Huge","Tiny","Clever",
            "Wetty","Pretty","Golden","Brave","Godly",
            "Kidly","Burning","Creepy","Fishy","Metallic",
            "Oriental","Muscly","Mudly","More","Strong",
            "Shiny","Sparkle","Legal","Hardest","Dancing"
        };
        // セカンドネーム定義
        string[] secondtName = new string[]{
            "Cake","Rock","Slime","Clover","Animal",
            "Fish","Earth","Throat","City","Dwarf",
            "Ghost","Tank","Knight","Candy","Worm",
            "Tree","Dice","Baby","Machine","Dog",
            "Thief","Bird","Cat","Water","CowBoy",
            "Skelton","Boots","Game","Card","Data"
        };
        // 1～30までの乱数を代入
        int num = rand.Next(1, 30);
        int num2 = rand.Next(1, 30);

        // 各乱数に応じた名前を代入
        playerName.text ="Name:" + firstName[num] + secondtName[num2];
    }

    void CheckSomething()
    {
        List<List<int>> usableCards = deckData.GetUsable();

        Debug.Log(usableCards[1][2]);
    }

    /// <summary>
    /// デッキ切り替え処理
    /// </summary>
    public void ChangeDeck()
    {
        if (isClick == false)
        {
            mainDeckPanel.SetActive(false);
            defenceDeckPanel.SetActive(true);
            deckTxt.text = "Defence Deck";
            isClick = true;

            // 防衛デッキを更新し、表示
            UpdateDeck(deckData.GetDefenceDeck());
        }
        else
        {
            mainDeckPanel.SetActive(true);
            defenceDeckPanel.SetActive(false);
            deckTxt.text = "Main Deck";
            isClick = false;   

            // メインデッキを更新し、表示
            UpdateDeck(deckData.GetDeck());
        }
    }

    /// <summary>
    /// カード一覧生成処理
    /// </summary>
    void CardSet()
    {
        if (isSet == true) return;
        int cnt = 0;
        List<List<int>> usableCards =  deckData.GetUsable();
        // ディクショナリー内のアイテム分ループ
        foreach (var item in deckData.cardDictionary.Keys)
        {
            // キーを文字列に変換
            string cardName = item.ToString();
            // スタック数を数字に変換
            int.TryParse(deckData.cardDictionary[cardName].Stack, out int stack);

            // 各カードのスタック数分ループ
            for (int i = 0; i < 4; i++)
            {
                // ループ数がスタック数未満の場合
                if (stack > i)
                {
                    // 同名のカードをリソースファイルから取得
                    GameObject obj = (GameObject)Resources.Load("UI/" + cardName);
                    // 取得したカードを生成
                    GameObject cards = Instantiate(obj, new Vector2(-400f + (200f * i), 125f - (250f * cnt)), Quaternion.identity);
                    // Rename
                    cards.name = (cnt+1) + "," + (i+1);
                    if (usableCards[cnt][i] == 1)
                    {
                        cards.GetComponent<Image>().color = Color.gray;
                    }
                    if (usableCards[cnt][i] == 2)
                    {
                        cards.GetComponent<Image>().color = Color.blue;
                    }
                    cards.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => AddDeck(cards));

                    // 生成カードをスクロールビューに追加
                    cards.transform.SetParent(cardViewParent.transform, false);
                }
                else // ループ数がスタック数を超えた場合、ダミーを生成して整頓する
                {
                    // 透明なダミーをリソースファイルから取得
                    GameObject obj = (GameObject)Resources.Load("UI/Dummy");
                    // 取得したダミーを生成
                    GameObject cards = Instantiate(obj, new Vector2(-400f + (200f * i), 125f - (250f * cnt)), Quaternion.identity);
                    // Rename
                    cards.name = "dummy";

                    // ダミーをスクロールビューに追加
                    cards.transform.SetParent(cardViewParent.transform, false);
                }
            }
            cnt++;
        }
        isSet = true;
    }

    /// <summary>
    /// Close Info Panel
    /// </summary>
    public void exitInfo()
    {
        info.SetActive(false);
    }

    /// <summary>
    /// Load Some Stage Scene
    /// </summary>
    public void startScene()
    {
        Transform infoText = info.transform.Find("Text");
        SceneManager.LoadScene(infoText.gameObject.GetComponent<Text>().text);
    }

    /// <summary>
    /// Load Tutprial Scene
    /// </summary>
    public void Tutorial()
    {
        SceneManager.LoadScene("Tutorial");
    }

    /// <summary>
    /// Open Stage Info Panel
    /// </summary>
    public void stageInfo()
    {
        deckBuildPanel.SetActive(false);
        infoPanel.SetActive(true);
        Transform stageInfo = info.transform.Find("Text");

        switch (stageInfo.gameObject.GetComponent<Text>().text)
        {
            case "1":
                infoTxt.text = "正しい手順で撃破を目指そう";
                break;

            case "2":
                infoTxt.text = "ここではシールドを破壊することが重要となる\n";
                break;

            case "3":
                infoTxt.text = "もたもたしていると爆破されてしまう\nどこかにシールドはないだろうか";
                break;

            case "4":
                infoTxt.text = "多彩な武器で蹂躙してくる\n攻撃の効果を覚えておこう";
                break;

            case "5":
                infoTxt.text = "毒で攻撃力が下がってしまう\n交換を駆使しよう";
                break;

            case "6":
                infoTxt.text = "こちらの行動をコピーされる\n高火力のカードを使うときは注意しよう";
                break;

            case "7":
                infoTxt.text = "反射バリアで攻撃を跳ね返してくる\n前ステージ同様カード選びは慎重に";
                break;

            case "8":
                infoTxt.text = "防御は最大の...防御？";
                break;

            case "9":
                infoTxt.text = "背筋が凍りそうだ...\n最初の猛攻を突破できれば勝ち筋は見えてくるだろう";
                break;

            case "10":
                infoTxt.text = "現バージョンでは最後の敵となる\nダイナマイトでもぶつけてやろう\n...そもそもこれを見てる人はいるのか？";
                break;
        }
    }

    /// <summary>
    /// Close Explain Panel
    /// </summary>
    public void exitExplain()
    {
        infoPanel.SetActive(false);
    }

    /// <summary>
    /// デッキ構築パネルを開く処理
    /// </summary>
    public void openDeckBuildPanel()
    {
        deckBuildPanel.SetActive(true);
        buildPanel.SetActive(true);

        if (isClick == false)
        {
            UpdateDeck(deckData.GetDeck());
        }
        else
        {
            UpdateDeck(deckData.GetDefenceDeck());
        }
        helpPanel.SetActive(false);
        cardViewPanel.SetActive(false);
        infoPanel.SetActive(false);
        showCardPanel.SetActive(false);
        iconPanel.SetActive(false);
    }

    /// <summary>
    /// デッキ構築パネルを閉じる処理
    /// </summary>
    public void closeDeckBuildPanel()
    {
        deckBuildPanel.SetActive(false);
    }

    /// <summary>
    /// カードビューパネル処理
    /// </summary>
    public void openViewPanel()
    {
        cardViewPanel.SetActive(true);

        CardSet();
        buildPanel.SetActive(false);
        infoPanel.SetActive(false);
        showCardPanel.SetActive(false);
        helpPanel.SetActive(false);
        iconPanel.SetActive(false);
    }

    /// <summary>
    /// 詳細パネル処理
    /// </summary>
    public void openHelpPanel()
    {    
        helpPanel.SetActive(true);
        
        cardViewPanel.SetActive(false);
        buildPanel.SetActive(false);
        infoPanel.SetActive(false);
        showCardPanel.SetActive(false);
        iconPanel.SetActive(false);
    }


    /// <summary>
    /// ビルド画面に戻る処理
    /// </summary>
    public void backBuildPanel()
    {
        buildPanel.SetActive(true);

        cardViewPanel.SetActive(false);
        helpPanel.SetActive(false);

        if (isClick == false)
        {
            deckData.SetDeck();
        }
        else
        {
            deckData.SetDefenceDeck();
        }
        warning.text = "";
    }

    /// <summary>
    /// 使用可能カード一覧パネル参照処理
    /// </summary>
    public void openShowCardPanel()
    {
        showCardPanel.SetActive(true);
        SetUsableView();
        deckBuildPanel.SetActive(false);
        infoPanel.SetActive(false);
        iconPanel.SetActive(false);
    }

    /// <summary>
    /// 使用可能カード一覧パネルを閉じる処理
    /// </summary>
    public void closeCardPanel()
    {
        showCardPanel.SetActive(false);
    }

    /// <summary>
    /// Load Battle Scene
    /// </summary>
    public void goFight()
    {
        SceneManager.LoadScene("Battle");
    }


    /// <summary>
    /// Check Usable Card
    /// </summary>
    /// <param name="name"></param>
    /// <param name="stack"></param>
    public void CardClick(string name, string stack)
    {
        switch (name)
        {
            case "Sword":
                infoText.text = name + ":1ダメージを与える 枚数×" + stack;
                break;

            case "S.Y.T.H":
                infoText.text = name + ":2ダメージを与える 枚数×" + stack;
                break;

            case "A.X.E":
                infoText.text = name + ":1ダメージを与える ブロックを無視＆破壊 枚数×" + stack;
                break;

            case "M.A.C.E":
                infoText.text = name + ":1ダメージに加えブロックの値分ダメージを与える 枚数×" + stack;
                break;

            case "Shield":
                infoText.text = name + ":1ブロックを受ける 枚数×" + stack;
                break;

            case "ForgeHammer":
                infoText.text = name + ":1ダメージを与える 次の行動の攻撃力+1 枚数×" + stack;
                break;

            case "Injector":
                infoText.text = name + ":1ダメージを与える 敵を出血させる(行動毎1ダメージ) 枚数×" + stack;
                break;

            case "PoisonKnife":
                infoText.text = name + ":1ダメージを与える 敵の攻撃力-1 枚数×" + stack;
                break;

            case "6mmBullet":
                infoText.text = name + ":3ダメージを与える ...銃があればの話 枚数×" + stack;
                break;

            case "SwatShield":
                infoText.text = name + ":2ブロックを受ける 枚数×" + stack;
                break;

            default:
                break;
        }
    }

    void SetUsableView()
    {
        int cnt = 0;
        // Create Stage Button from Server
        GameObject cardObj = new GameObject();

        foreach (var item in deckData.cardDictionary.Keys)
        {
            // キーを文字列に変換
            string cardName = item.ToString();
            // スタック数を数字に変換
            int.TryParse(deckData.cardDictionary[cardName].Stack, out int cardStack);

            GameObject obj = (GameObject)Resources.Load("UI/" + cardName);

            if (cnt < 5)
            {
                cardObj = Instantiate(obj, new Vector2(-400 + (200 * cnt), 100), Quaternion.identity);
            }
            else
            {
                cardObj = Instantiate(obj, new Vector2(-300 + (200 * (cnt - 5)), -130), Quaternion.identity);
            }

            cardObj.name = cardName;

            cardObj.transform.SetParent(this.cardParent.transform, false);
            cardObj.GetComponent<Button>().onClick.AddListener(() => CardClick(cardName, deckData.cardDictionary[cardName].Stack));
            cnt++;
        }
    }

    async void Loading()
    {
        loadingPanel.SetActive(true);

        float angle = 8;
        bool rot = true;

        for (int i = 0; i < 80; i++)
        {
            if (rot)
            {
                loadingIcon.transform.rotation *= Quaternion.AngleAxis(angle, Vector3.back);
            }
            else
            {
                loadingIcon.transform.rotation *= Quaternion.AngleAxis(angle, Vector3.forward);
            }
            await Task.Delay(10);
        }
        loadingPanel.SetActive(false);
    }

    public void OpenIconPanel()
    {
        iconPanel.SetActive(true);

        helpPanel.SetActive(true);
        cardViewPanel.SetActive(false);
        buildPanel.SetActive(false);
        infoPanel.SetActive(false);
        showCardPanel.SetActive(false);
    }

    public void CloseIconPanel()
    {
        iconPanel.SetActive(false);
    }

    /// <summary>
    /// 次のアイコン選択処理
    /// </summary>
    public void NextIcon()
    {  
        iconNum++;
        if (iconNum >= 10) iconNum = 0;

        // オブジェクトの取得
        Image preview = GameObject.Find("IconPreview").GetComponent<Image>();
        // リソースから、アイコンを取得
        Texture2D texture = Resources.Load("Icons/icon00" + iconNum) as Texture2D;

        iconImage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),
                                       Vector2.zero);
        preview.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),
                               Vector2.zero);

        iconName = texture.name;  
    }

    /// <summary>
    /// 前のアイコン選択処理
    /// </summary>
    public void BackIcon()
    {
        iconNum--;
        if (iconNum <= -1) iconNum = 9;

        // オブジェクトの取得
        Image preview = GameObject.Find("IconPreview").GetComponent<Image>();
        // リソースから、アイコンを取得
        Texture2D texture = Resources.Load("Icons/icon00" + iconNum) as Texture2D;

        iconImage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),
                                       Vector2.zero);
        preview.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),
                       Vector2.zero);

        iconName = texture.name;
    }

    /// <summary>
    /// プロフィール更新
    /// </summary>
    public void updateDisplayInfo()
    {
        Debug.Log("アイコン名:" + iconName);
        Debug.Log("プレイヤー名:" + playerName.text); ;
        StartCoroutine(NetworkManager.Instance.UpdateProfile(playerName.text, iconName));
    }
}
