/*
 * BattleModeScript
 * Creator:西浦晃太 Update:2024/11/27
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Threading.Tasks;
using System;

public class BattleMode : MonoBehaviour
{
    // 現在のデッキ表示用親
    [SerializeField] GameObject deckParent;

    // ローディングパネル
    [SerializeField] GameObject loadingPanel;

    // ローディングアイコン
    [SerializeField] GameObject loadingIcon;

    // ライバルパネルのプレハブ
    [SerializeField] GameObject profilePrefab;

    // ライバルデッキのパネル
    [SerializeField] GameObject[] rivalDeckPanel;

    // ライバルプロフィールのパネル
    [SerializeField] GameObject[] rivalProfilePanel;

    // 警告テキストのオブジェクト
    [SerializeField] GameObject warning;

    // 警告テキスト
    [SerializeField] Text warningText;

    // ポイントテキスト
    [SerializeField] Text pointText;

    // デッキ構築済み確認変数
    bool isSetDeck;

    // 現在のデッキIDリスト
    List<int> activeDeckID = new List<int>();

    //ライバルのデータディクショナリ
    Dictionary<int,List<int>> rivalDataDictionary = new Dictionary<int,List<int>>();

    string[] iconNames = {"","",""};

    // Start is called before the first frame update
    void Start()
    {
        // 非同期処理完了まで待機させる
        Loading();

        isSetDeck = true;
        warning.SetActive(false);

        StartCoroutine(NetworkManager.Instance.ShowDeck(cards =>
        {
            // ユーザのデッキ枚数が4枚でない場合
            if (cards.Length != 4)
            {
                isSetDeck = false;
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
                // ユーザの防衛デッキ枚数が4枚でない場合
                if (cards.Length != 4)
                {
                    isSetDeck = false;
                    warningText.text = "注：ディフェンスデッキのカード枚数が不十分なため\r\n　　戦闘が開始できません";
                    warning.SetActive(true);
                }
            }));
        }));

        StartCoroutine(NetworkManager.Instance.GetProfile(rivalData =>
        {
            // 代入用リスト
            List<int> cardList = new List<int>();
            // ユーザID保存用変数
            int userID = 0;
            int cnt = 0;
            int count = 0;
            RivalData.rivalIDList = new List<int>();
            // 所得ライバルデータ数分ループ
            foreach (var item in rivalData)
            {
                // IDをint化
                int.TryParse(item.UserID.ToString(), out int id);

                // 保存したIDと異なるかつ現在のリストカウントが4の場合
                if (userID != id && cardList.Count == 4)
                {
                    rivalDataDictionary.Add(userID, cardList);
                    RivalData.rivalIDList.Add(userID);
                    // 代入用リストをリセット
                    cardList = new List<int>();
                    cnt++;
                }
                userID = id;
                // カードIDをint化
                int.TryParse(item.CardID.ToString(), out int cardID);
                // 代入用リストに取得カードを入れる
                cardList.Add(cardID);
                count++;
            }
            rivalDataDictionary.Add(userID, cardList);
            RivalData.rivalIDList.Add(userID);

            for (int i = 0; i < RivalData.rivalIDList.Count; i++)
            {
                Debug.Log(RivalData.rivalIDList[i]);
            }


            StartCoroutine(NetworkManager.Instance.GetMultiProfile(userData =>
            {
                int cnt = 0;
                foreach (var item in userData)
                {
                    Image icon = rivalProfilePanel[cnt].transform.GetChild(0).GetComponent<Image>();

                    if (item.IconName != "")
                    {
                        // リソースから、アイコンを取得
                        Texture2D texture = Resources.Load("Icons/" + item.IconName.ToString()) as Texture2D;

                        icon.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),
                                                       Vector2.zero);
                        iconNames[cnt] = item.IconName;
                    }
                    else
                    {
                        // リソースから、アイコンを取得
                        Texture2D texture = Resources.Load("Icons/icon000") as Texture2D;

                        icon.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),
                                                       Vector2.zero);
                        iconNames[cnt] = "icon000";
                    }

                    if (item.Name != "")
                    {
                        Text name = rivalProfilePanel[cnt].transform.GetChild(1).GetComponent<Text>();
                        name.text = "Name:" + item.Name;
                    }
                    else
                    {
                        Text name = rivalProfilePanel[cnt].transform.GetChild(1).GetComponent<Text>();
                        name.text = "Name:NoName";
                    }

                    Text point = rivalProfilePanel[cnt].transform.GetChild(2).GetComponent<Text>();
                    point.text ="Point:" + item.Point.ToString();
                    cnt++;
                }

                SetRivalDeck();
            }));
        }));


        StartCoroutine(NetworkManager.Instance.GetMyProfile(userData =>
        {           
            // 所得ライバルデータ数分ループ
            foreach (var item in userData)
            {
                pointText.text = "Your Point:" + item.Point.ToString();
            }
        }));
    }

    // Update is called once per frame
    void Update()
    {
    }

    /// <summary>
    /// 戦闘シーンへ遷移
    /// </summary>
    public void goFight(List<int> cardList, int rivalID, string iconName)
    {
        if (isSetDeck == false) return;

        SetRivalData(cardList, rivalID, iconName);
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
    /// ライバルデッキ表示処理
    /// </summary>
    void SetRivalDeck()
    {
        int cnt = 0;
        foreach (var cards in rivalDataDictionary)
        {
            // 各カードの枚数分ループ
            for (int i = 0; i < cards.Value.Count; i++)
            {
                // 同名のカードをリソースファイルから取得
                GameObject obj = (GameObject)Resources.Load("Cards(ID)/" + cards.Value[i]);
                // 取得したカードを生成
                GameObject cardObj = Instantiate(obj, new Vector2(-330f + (220f * i), 0f), Quaternion.identity);
                cardObj.name = cards.Value[i].ToString();
                cardObj.transform.localScale = new Vector2(1.1f, 2f);
                // ライバルデッキパネルに生成
                cardObj.transform.SetParent(rivalDeckPanel[cnt].transform, false);
                rivalDeckPanel[cnt].name = cards.Key.ToString();
            }
            GameObject child = rivalDeckPanel[cnt].transform.GetChild(0).gameObject;
            string iconName ="icon000";
            if (cnt <= iconNames.Length)
            {
                iconName = iconNames[cnt];
            }

            child.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => goFight(cards.Value,cards.Key, iconName));
            cnt++;
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

    void SetRivalData(List<int> cardList, int rivalID, string iconName)
    {
        RivalData.cardIDList = cardList;
        RivalData.rivalID = rivalID;
        RivalData.iconName = iconName;
    }
}
