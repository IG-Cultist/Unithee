/*
 * BattleModeScript
 * Creator:西浦晃太 Update:2024/11/07
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Threading.Tasks;

public class BattleMode : MonoBehaviour
{
    // 現在のデッキ表示用親
    [SerializeField] GameObject deckParent;

    // 警告テキスト
    [SerializeField] GameObject warning;

    [SerializeField] Text warningText;

    // ローディングパネル
    [SerializeField] GameObject loadingPanel;

    // ローディングアイコン
    [SerializeField] GameObject loadingIcon;

    // デッキ構築済み確認変数
    bool isSetDeck;

    // 現在のデッキIDリスト
    List<int> activeDeckID = new List<int>();

    int[] rivalID = {0,0,0};

    // Start is called before the first frame update
    void Start()
    {
        // 非同期処理完了まで待機させる
        Loading();

        isSetDeck = false;
        warning.SetActive(false);

        StartCoroutine(NetworkManager.Instance.ShowDeck(cards =>
        {
            if (cards.Length != 4)
            {
                warningText.text = "注：デッキのカード枚数が不十分なため\r\n　　戦闘が開始できません";
                warning.SetActive(true);
            }

            foreach (var card in cards)
            {
                string strID = card.CardID.ToString();
                int.TryParse(strID, out int cardID);
                activeDeckID.Add(cardID);
            }
            SetDeck();

            StartCoroutine(NetworkManager.Instance.ShowDefenceDeck(cards =>
            {
                if (cards.Length != 4)
                {
                    warningText.text = "注：ディフェンスデッキのカード枚数が不十分なため\r\n　　戦闘が開始できません";
                    warning.SetActive(true);
                }
                else if (cards.Length == 4) isSetDeck = true;
            }));
        }));

        StartCoroutine(NetworkManager.Instance.GetProfile(users =>
        {

        }));
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// 戦闘シーンへ遷移
    /// </summary>
    public void goFight()
    {
        if (isSetDeck == false) return;
        SceneManager.LoadScene("Fight");
    }

    /// <summary>
    /// デッキ表示処理
    /// </summary>
    void SetDeck()
    {
        // 生成されているカードをすべて削除
        foreach (Transform n in deckParent.transform)
        {
            GameObject.Destroy(n.gameObject);
        }

        // 各カードのスタック数分ループ
        for (int i = 0; i < activeDeckID.Count; i++)
        {
            if (activeDeckID[i] == 0) continue;
            // 同名のカードをリソースファイルから取得
            GameObject obj = (GameObject)Resources.Load("Cards(ID)/" + activeDeckID[i]);
            // 取得したカードを生成
            GameObject cards = Instantiate(obj, new Vector2(-680f + (450f * i), 0f), Quaternion.identity);
            cards.name = activeDeckID[i].ToString();
            cards.transform.localScale = new Vector2(2.7f, 3.9f);
            // メインデッキパネルに生成
            cards.transform.SetParent(deckParent.transform, false);
        }
    }

    /// <summary>
    /// ログ確認処理
    /// </summary>
    public void CheckLog()
    {

    }

    /// <summary>
    /// セレクト画面に戻る処理
    /// </summary>
    public void exitBattleScene()
    {
        SceneManager.LoadScene("SelectScene");
    }

    /// <summary>
    /// ローディング
    /// </summary>
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
}
