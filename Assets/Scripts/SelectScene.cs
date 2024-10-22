/*
 * SelectSceneScript
 * Creator:西浦晃太 Update:2024/10/10
*/
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static UnityEditor.Progress;

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

    // カード親
    [SerializeField] GameObject cardViewParent;

    // 現在のデッキの親
    [SerializeField] GameObject activeDeckParent;

    // 防衛デッキの親
    [SerializeField] GameObject defenceDeckPanel;

    // Deck Panel
    [SerializeField] Text infoText;

    // プレイヤー名
    [SerializeField] Text playerName;

    // デッキカードリスト
    public List<GameObject> deckCards = new List<GameObject>();

    // Clear's SoundEffect
    AudioClip clickSE;

    // 使用可能カードのディクショナリー
    Dictionary<string, UsableCardResponse> cardDictionary = new Dictionary<string, UsableCardResponse>();

    GameObject mainDeck;

    int deckCount = 0;
    bool isSet = false;
    bool isClick = false;


    AudioSource audioSource;
    // Start is called before the first frame update
    void Start()
    {
        // SetSE
        this.gameObject.AddComponent<AudioSource>();
        audioSource = GetComponent<AudioSource>();
        mainDeck = GameObject.Find("MainDeckFrame");

        clickSE = (AudioClip)Resources.Load("SE/Click");

        NetworkManager networkManager = NetworkManager.Instance;
        List<int> stageIDs = networkManager.GetID();

        // 全てのパネルを閉じる
        infoPanel.SetActive(false);
        info.SetActive(false);
        deckBuildPanel.SetActive(false);
        showCardPanel.SetActive(false);
        defenceDeckPanel.SetActive(false);
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
                btn.GetComponent<Button>().onClick.AddListener(() => selectStage(btnName));

                if (stageIDs != null && stageIDs.Contains(stage.StageID))
                {
                    btn.GetComponent<Image>().color = Color.yellow;
                }
                cnt++;
            }
        }));

        StartCoroutine(NetworkManager.Instance.GetUsableCard(cards =>
        {
            int cnt = 0;
            // Create Stage Button from Server
            GameObject cardObj = new GameObject();
            foreach (var card in cards)
            {
                // 名前を取得
                string cardName = card.Name.ToString();
                string cardStack = card.Stack.ToString();
                // 取得した名前に一致するプレハブを取得
                GameObject obj = (GameObject)Resources.Load("UI/" + cardName);
                // 取得したプレハブを生成
                if (cnt < 5)
                {
                    cardObj = Instantiate(obj, new Vector2(-400 + (200 * cnt), 100), Quaternion.identity);
                }
                else
                {
                    cardObj = Instantiate(obj, new Vector2(-300 + (200 * (cnt - 5)), -130), Quaternion.identity);
                }

                // 生成したプレハブをリネーム
                cardObj.name = cardName;
                // 各種別に応じたタグを付与
                switch (card.Type.ToString())
                {
                    case "Attack":
                        cardObj.tag = "Attack";
                        break;
                    case "Defence":
                        cardObj.tag = "Defence";
                        break;
                    default:
                        break;
                }
                // 生成したプレハブを親に入れる
                cardObj.transform.SetParent(this.cardParent.transform, false);
                cardObj.GetComponent<Button>().onClick.AddListener(() => CardClick(cardName, cardStack));

                // カード情報をディクショナリーにまとめる
                cardDictionary.Add(card.Name, card);

                cnt++;
            }
        }));
    }

    void Update()
    {
        if (Input.GetMouseButtonUp(0)) audioSource.PlayOneShot(clickSE);

        if(Input.GetMouseButtonUp(1)) CheckSomething();
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

        cardViewPanel.SetActive(false);
        infoPanel.SetActive(false);
        showCardPanel.SetActive(false);

        DeckRefresh();
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
    public void openAttackCardPanel()
    {
        cardViewPanel.SetActive(true);

        buildPanel.SetActive(false);
        infoPanel.SetActive(false);
        showCardPanel.SetActive(false);

        if (isSet == true) return;
        int cnt = 0;
        // ディクショナリー内のアイテム分ループ
        foreach (var item in cardDictionary.Keys)
        {
            // キーを文字列に変換
            string cardName = item.ToString();
            // スタック数を数字に変換
            int.TryParse(cardDictionary[cardName].Stack, out int stack);

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
                    cards.name = cardName;
                    cards.GetComponent<Button>().onClick.AddListener(() => AddDeck(cards));

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
    /// ビルド画面に戻る処理
    /// </summary>
    public void backBuildPanel()
    {
        buildPanel.SetActive(true);

        cardViewPanel.SetActive(false);

        DeckRefresh();
    }

    /// <summary>
    /// 使用可能カード一覧パネル参照処理
    /// </summary>
    public void openShowCardPanel()
    {
        showCardPanel.SetActive(true);
        deckBuildPanel.SetActive(false);
        infoPanel.SetActive(false);
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

    /// <summary>
    /// デッキ追加処理
    /// </summary>
    void AddDeck(GameObject obj)
    {
        // 選択(デッキに追加)された場合かつ現在のデッキが4枚以下の場合
        if (!deckCards.Contains(obj) && deckCards.Count < 4)
        {
            obj.GetComponent<Image>().color = Color.gray;

            // 同名のカードをリソースファイルから取得
            GameObject resource = (GameObject)Resources.Load("UI/" + obj.name);
            // 取得したカードを生成
            GameObject cards = Instantiate(resource, new Vector2(-450f + (300f * deckCount), 0f), Quaternion.identity);
            cards.transform.localScale = new Vector2(1.8f, 2.5f);
            // Rename
            cards.name = obj.name;
            // 生成したカードを親に追加
            cards.transform.SetParent(activeDeckParent.transform, false);
            // デッキリストに追加
            deckCards.Add(obj);
            deckCount++;
        }
        else // 再度選択(削除)された場合
        {
            // 現在生成されているオブジェクトを削除
            foreach (Transform n in activeDeckParent.transform)
            {
                // タッチしたオブジェクトとリスト内のオブジェクトの名前が一致した場合
                if (obj.name == n.name)
                {
                    // 対象を削除
                    GameObject.Destroy(n.gameObject);
                    deckCount--;
                    break;
                }
            }

            // 色を元に戻す
            obj.GetComponent<Image>().color = Color.white;
            // デッキリストから削除
            deckCards.Remove(obj);
        }
    }

    /// <summary>
    /// デッキ読み込み処理
    /// </summary>
    void DeckRefresh()
    {
        if (deckCards.Count <= 0) return;
        int cnt = 0;
        List<GameObject> keepList = new List<GameObject>();

        foreach (GameObject obj in deckCards)
        {
            keepList.Add(obj);
        }

        foreach (Transform n in activeDeckParent.transform)
        {
            GameObject.Destroy(n.gameObject);
        }
        deckCards.Clear();

        foreach (var item in keepList)
        {
            // 同名のカードをリソースファイルから取得
            GameObject obj = (GameObject)Resources.Load("UI/" + item.name);
            // 取得したカードを生成
            GameObject cards = Instantiate(obj, new Vector2(-450f + (300f * cnt), 0f), Quaternion.identity);
            cards.transform.localScale = new Vector2(1.8f, 2.5f);
            // Rename
            cards.name = item.name;
            // 再度リストに追加
            deckCards.Add(item);
            // 生成したカードを親に追加
            cards.transform.SetParent(activeDeckParent.transform, false);
            cnt++;
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
        foreach (var deck in deckCards)
        {
            Debug.Log(deck);
        }
    }

    /// <summary>
    /// デッキ切り替え処理
    /// </summary>
    public void ChangeDeck()
    {
        if (isClick == false)
        {
            mainDeck.SetActive(false);
            defenceDeckPanel.SetActive(true);
            deckTxt.text = "Defence Deck";
            isClick = true;
        }
        else
        {
            mainDeck.SetActive(true);
            defenceDeckPanel.SetActive(false);
            deckTxt.text = "Main Deck";
            isClick = false;
        }
    }
}
