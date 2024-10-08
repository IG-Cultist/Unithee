using System.Collections;
using System.Collections.Generic;
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

    [SerializeField] Text infoTxt;

    // 親
    [SerializeField] GameObject stageParent;

    // ステージ解説パネル
    [SerializeField] GameObject infoPanel;

    // デッキ構築パネル
    [SerializeField] GameObject deckBuildPanel;

    // カード参照パネル
    [SerializeField] GameObject showCardPanel;

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
                    btn = Instantiate(btnPrefab, new Vector2(-550 + (150 * (cnt-5)), 150), Quaternion.identity);
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

                if(stageIDs!= null && stageIDs.Contains(stage.StageID))
                {
                    btn.GetComponent<Image>().color = Color.yellow;
                }
                cnt++;
            }
        }));
    }

    private void Update()
    {
        if (Input.GetMouseButtonUp(0)) audioSource.PlayOneShot(clickSE);
    }

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

    public void exitInfo()
    {
        info.SetActive(false);
    }

    public void startScene()
    {
        Transform infoText = info.transform.Find("Text");
        SceneManager.LoadScene(infoText.gameObject.GetComponent<Text>().text);
    }

    public void Tutorial()
    {
        SceneManager.LoadScene("Tutorial");
    }

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
                infoTxt.text = "現バージョンでは最後の敵となる\nダイナマイトでもぶつけてやろう";
                break;
        }
    }

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
}
