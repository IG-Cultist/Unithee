/*
 * SelectSceneScript
 * Creator:西浦晃太 Update:2024/10/10
*/
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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

    // Deck Panel
    [SerializeField] Text infoText;

    //Clear's SoundEffect
    AudioClip clickSE;

    AudioSource audioSource;
    // Start is called before the first frame update
    void Start()
    {
        // SetSE
        this.gameObject.AddComponent<AudioSource>();
        audioSource = GetComponent<AudioSource>();

        clickSE = (AudioClip)Resources.Load("SE/Click");

        NetworkManager networkManager = NetworkManager.Instance;
        List<int> stageIDs = networkManager.GetID();

        // 全てのパネルを閉じる
        infoPanel.SetActive(false);
        info.SetActive(false);
        deckBuildPanel.SetActive(false);
        showCardPanel.SetActive(false);
        StartCoroutine(NetworkManager.Instance.GetStage(stages =>
        {
            int cnt = 0;
            foreach (var stage in stages)
            {
                // Create Stage Button from Server
                GameObject btn = new GameObject();
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

                cnt++;
            }
        }));
    }

    void Update()
    {
        if (Input.GetMouseButtonUp(0)) audioSource.PlayOneShot(clickSE);
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
    /// Open Deck Build Panel Progress
    /// </summary>
    public void openBuildPanel()
    {
        deckBuildPanel.SetActive(true);
        infoPanel.SetActive(false);
        showCardPanel.SetActive(false);
    }

    /// <summary>
    /// Close Deck Build Panel Progress
    /// </summary>
    public void closeBuildPanel()
    {
        deckBuildPanel.SetActive(false);
    }

    /// <summary>
    /// Open Show Card Panel Progress
    /// </summary>
    public void openShowCardPanel()
    {
        showCardPanel.SetActive(true);
        deckBuildPanel.SetActive(false);
        infoPanel.SetActive(false);
    }

    /// <summary>
    /// Close Deck Build Panel Progress
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
    public void CardClick(string name,string stack)
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
}
