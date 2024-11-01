/*
 * DeckDataScript
 * Creator:西浦晃太 Update:2024/10/30
*/
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeckData : MonoBehaviour
{
    // 使用可のカードオブジェクト二次元リスト
    public List<List<int>> usableObjList = new List<List<int>>();

    // 使用可能カードのディクショナリー
    public Dictionary<string, UsableCardResponse> cardDictionary = new Dictionary<string, UsableCardResponse>();

    // 現在のデッキ内カードIDリスト
    List<int> activeCardID = new List<int>();

    // 現在のデッキ内カードIDリスト
    List<int> activeDefenceCardID = new List<int>();

    // ステージ選択画面スクリプト
    SelectScene selectScene;

    // Start is called before the first frame update
    void Start()
    {
        // ステージ選択画面スクリプトを取得
        selectScene = FindObjectOfType<SelectScene>();

        StartCoroutine(NetworkManager.Instance.ShowDeck(cards =>
        {
            if (cards == null) return;
            // ユーザのデッキ情報を取得
            foreach (var card in cards)
            {
                if (card == null) continue;
                // カードIDを取得
                string strID = card.CardID.ToString();
                int.TryParse(strID, out int cardID);

                // デッキにカードIDを追加
                activeCardID.Add(cardID);
            }

            StartCoroutine(NetworkManager.Instance.ShowDefenceDeck(cards =>
            {
                if (cards == null) return;
                // ユーザの防衛デッキ情報を取得
                foreach (var card in cards)
                {
                    if (card == null) continue;
                    // カードIDを取得
                    string strID = card.CardID.ToString();
                    int.TryParse(strID, out int cardID);

                    // 防衛デッキにカードIDを追加
                    activeDefenceCardID.Add(cardID);
                }

                StartCoroutine(NetworkManager.Instance.GetUsableCard(cards =>
                {
                    foreach (var card in cards)
                    {
                        // 各情報を取得
                        string strID = card.CardID.ToString();
                        int.TryParse(strID, out int cardID);

                        string strStack = card.Stack.ToString();
                        int.TryParse(strStack, out int cardStack);

                        string cardName = card.Name.ToString();

                        // 使用可能カードをスタック数分リストに突っ込む
                        List<int> Items = new List<int>();
                        for (int i = 0; i < cardStack; i++)
                        {
                            Items.Add(0);
                        }
                        usableObjList.Add(Items);

                        // カード情報をディクショナリーにまとめる
                        cardDictionary.Add(card.Name, card);
                    }

                    foreach (var item in activeCardID)
                    {
                        int cnt = 0;
                        foreach (var id in usableObjList[item - 1])
                        {
                            if (id != 1 && id != 2)
                            {
                                usableObjList[item - 1][cnt] = 1;
                                break;
                            }
                            cnt++;
                        }
                    }

                    foreach (var item in activeDefenceCardID)
                    {
                        int cnt = 0;
                        foreach (var id in usableObjList[item - 1])
                        {
                            if (id != 1 && id != 2)
                            {
                                usableObjList[item - 1][cnt] = 2;
                                break;
                            }
                            cnt++;
                        }
                    }
                }));
            }));
        }));
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// 現在のデッキをDBに保存する処理
    /// </summary>
    /// <param name="cardID"></param>
    public void SetDeck()
    {
        List<int> list = new List<int>();

        for (int i = 0; i < 9; i++)
        {
            // 各スタック数分ループ
            for (int j = 0; j < usableObjList[i].Count; j++)
            {
                if (usableObjList[i][j] == 1)
                {
                    list.Add(i+1);
                }
            }
        }

        // 現在格納されているIDを削除
        activeCardID.Clear();

        int cnt = 0;
        // DB送信用配列
        int[] sendData = { 0, 0, 0, 0 };

        // 取得してきたIDを代入
        foreach(var id in list)
        {
            if (list == null) break;
            // 現在のデッキリストにIDを入れる
            activeCardID.Add((int)id);

            // 送信用配列にIDを入れる
            sendData[cnt] = (int)id;
            cnt++;
        }

        selectScene.UpdateDeck(activeCardID);
        // 取得したIDをDBに送信
        StartCoroutine(NetworkManager.Instance.StoreCard(sendData));
    }

    /// <summary>
    /// 現在の防衛デッキをDBに保存する処理
    /// </summary>
    /// <param name="cardID"></param>
    public void SetDefenceDeck()
    {
        List<int> list = new List<int>();

        for (int i = 0; i < 9; i++)
        {
            // 各スタック数分ループ
            for (int j = 0; j < usableObjList[i].Count; j++)
            {
                if (usableObjList[i][j] == 2)
                {
                    list.Add(i + 1);
                }
            }
        }

        // 現在格納されているIDを削除
        activeDefenceCardID.Clear();

        int cnt = 0;
        // DB送信用配列
        int[] sendData = { 0, 0, 0, 0 };

        // 取得してきたIDを代入
        foreach (var id in list)
        {
            if (list == null) break;
            // 現在のデッキリストにIDを入れる
            activeDefenceCardID.Add((int)id);

            // 送信用配列にIDを入れる
            sendData[cnt] = (int)id;
            cnt++;
        }

        selectScene.UpdateDeck(activeDefenceCardID);
        // 取得したIDをDBに送信
        StartCoroutine(NetworkManager.Instance.StoreDefenceCard(sendData));
    }

    /// <summary>
    /// 現在のデッキ内カードID取得処理
    /// </summary>
    public List<int> GetDeck()
    {
        return activeCardID;
    }

    /// <summary>
    /// 現在の防衛デッキ内カードID取得処理
    /// </summary>
    public List<int> GetDefenceDeck()
    {
        return activeDefenceCardID;
    }

    /// <summary>
    /// 選択状態リスト取得処理
    /// </summary>
    public List<List<int>> GetUsable()
    {
        return usableObjList;
    }

    /// <summary>
    /// 選択状態判別処理
    /// </summary>
    public bool CheckUsable(string requestStr)
    {
        for (int i = 1; i <= 9; i++)
        {
            // 4回ループ
            for (int j = 1; j <= 4; j++)
            {
                // 文字列判定
                if (requestStr == i.ToString() + "," + j.ToString())
                {
                    //選択されているならTrueを返す
                    if (usableObjList[i - 1][j - 1] == 1 || usableObjList[i - 1][j - 1] == 2)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    /// <summary>
    /// 選択状態更新処理
    /// </summary>
    /// <param name="requestStr"></param>
    /// <param name="val"></param>
    public void UpdateUsable(string requestStr,int val)
    {
        for (int i = 1; i <= 9; i++)
        {
            // 4回ループ
            for (int j = 1; j <= 4; j++)
            {
                // 文字列判定
                if (requestStr == i.ToString() + "," + j.ToString() )
                {
                    // 選択状態をリクエストされた値に更新
                    usableObjList[i-1][j-1] = val;
                    return;
                }
            }
        }
    }


    /// <summary>
    /// 名前をIDに変換する処理
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public int ConvertName(string requestStr)
    {
        for (int i = 1; i <= 9; i++)
        {
            // 4回ループ
            for (int j = 1; j <= 4; j++)
            {
                // 文字列判定
                if (requestStr == i.ToString() + "," + j.ToString())
                {
                    return i;
                }
            }
        }
        return 0;
    }
}
