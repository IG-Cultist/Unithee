/*
 * NetworkManagerScript
 * Creator:西浦晃太 Update:2024/10/11
*/
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
//using UnityEditor.PackageManager.Requests;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkManager : MonoBehaviour
{
    /// <summary>
    /// シングルトン
    /// </summary>
    const string API_BASE_URL = "https://api-deadlydraw.japaneast.cloudapp.azure.com/api/";
    private int userID = 0;
    private string userName = "";
    private List<int> stageList = new List<int>();
    public string displayName = "";
    public string iconName = "";
    private static NetworkManager instance;
    private string authToken;
    public string AuthToken { get { return authToken; } }
    public static NetworkManager Instance
    {
        get {
            // create yet
            if(instance == null)
            {
                GameObject gameObj = new GameObject("NetworkManger");
                instance = gameObj.AddComponent<NetworkManager>();
                DontDestroyOnLoad(gameObj);
            }
            return instance;
        }
    }

    /// <summary>
    /// トークン生成処理
    /// </summary>
    /// <param name="response"></param>
    /// <returns></returns>
    public IEnumerator CreateToken(Action<bool> response)
    {
        var requsetData = new
        {
            user_id = this.userID,
        };
        string json = JsonConvert.SerializeObject(requsetData);
        UnityWebRequest request = UnityWebRequest.Post(
            API_BASE_URL + "createToken", json, "application/json");
        yield return request.SendWebRequest();
        if(request.result == UnityWebRequest.Result.Success ) 
        {
            //通信が成功した場合、返ってきたJsonをオブジェクトに変換
            string resultJson = request.downloadHandler.text;
            StoreUserresponse resp =
                JsonConvert.DeserializeObject<StoreUserresponse>(resultJson);
            //ファイルにIDとトークンを保存
            this.authToken = resp.Token;
            this.userID = resp.UserID;
            SaveUserData();
        }
        response?.Invoke(request.result == UnityWebRequest.Result.Success);
    }

    /// <summary>
    /// デッキ登録処理
    /// </summary>
    /// <param name="cardID"></param>
    /// <returns></returns>
    public IEnumerator StoreCard(int[] cardID)
    {
        //Create Object Send for Server
        StoreDeckRequest requestData = new StoreDeckRequest();

        // 取得したIDをリクエストに入れる
        requestData.CardID_1 = cardID[0];
        requestData.CardID_2 = cardID[1];
        requestData.CardID_3 = cardID[2];
        requestData.CardID_4 = cardID[3];
        //サーバに送信するオブジェクトをJAONに変換response
        string json = JsonConvert.SerializeObject(requestData);
        //Send
        UnityWebRequest request = UnityWebRequest.Post(
            API_BASE_URL + "battleMode/deck/update", json, "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + authToken);
        yield return request.SendWebRequest();
    }

    /// <summary>
    /// 戦闘結果登録処理
    /// </summary>
    /// <param name="judge"></param>
    /// <param name="rival_id"></param>
    /// <returns></returns>
    public IEnumerator StoreResult(int judge, int rival_id)
    {
        //Create Object Send for Server
        StoreResultResponse requestData = new StoreResultResponse();

        // 取得したIDをリクエストに入れる
        requestData.Judge = judge;
        requestData.RivalID = rival_id;
        //サーバに送信するオブジェクトをJAONに変換response
        string json = JsonConvert.SerializeObject(requestData);
        //Send
        UnityWebRequest request = UnityWebRequest.Post(
            API_BASE_URL + "battleMode/result/update", json, "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + authToken);
        yield return request.SendWebRequest();
    }

    /// <summary>
    /// 防衛デッキ登録処理
    /// </summary>
    /// <param name="cardID"></param>
    /// <returns></returns>
    public IEnumerator StoreDefenceCard(int[] cardID)
    {
        //Create Object Send for Server
        StoreDeckRequest requestData = new StoreDeckRequest();
        // 取得したIDをリクエストに入れる
        requestData.CardID_1 = cardID[0];
        requestData.CardID_2 = cardID[1];
        requestData.CardID_3 = cardID[2];
        requestData.CardID_4 = cardID[3];
        //サーバに送信するオブジェクトをJAONに変換response
        string json = JsonConvert.SerializeObject(requestData);
        //Send
        UnityWebRequest request = UnityWebRequest.Post(
            API_BASE_URL + "battleMode/defenceDeck/update", json, "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + authToken);
        yield return request.SendWebRequest();
    }

    /// <summary>
    /// デッキ検索処理
    /// </summary>
    public IEnumerator ShowDeck(Action<DeckResponse[]> result)
    {
        //Send
        UnityWebRequest request = UnityWebRequest.Get(
            API_BASE_URL + "battleMode/deck/show");
        request.SetRequestHeader("Authorization", "Bearer " + authToken);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success
             && request.responseCode == 200)
        {
            //通信が成功した場合、返ってきたJsonをオブジェクトに変換
            string resultJson = request.downloadHandler.text;
            DeckResponse[] response =
                JsonConvert.DeserializeObject<DeckResponse[]>(resultJson);
            result?.Invoke(response);//ここで呼び出し元のresult処理を呼ぶ
        }
        else
        {
            result?.Invoke(null);
        }
    }

    /// <summary>
    /// 防衛デッキ検索処理
    /// </summary>
    public IEnumerator ShowDefenceDeck(Action<DeckResponse[]> result)
    {
        //Send
        UnityWebRequest request = UnityWebRequest.Get(
            API_BASE_URL + "battleMode/defenceDeck/show");
        request.SetRequestHeader("Authorization", "Bearer " + authToken);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success
             && request.responseCode == 200)
        {
            //通信が成功した場合、返ってきたJsonをオブジェクトに変換
            string resultJson = request.downloadHandler.text;
            DeckResponse[] response =
                JsonConvert.DeserializeObject<DeckResponse[]>(resultJson);
            result?.Invoke(response);//ここで呼び出し元のresult処理を呼ぶ
        }
        else
        {
            result?.Invoke(null);
        }
    }

    /// <summary>
    /// ユーザ登録処理
    /// </summary>
    /// <param name="name"></param>
    /// <param name="result"></param>
    /// <returns></returns>
    public IEnumerator StoreUser(/*string name,*/Action<bool> result)
    {
        //Create Object Send for Server
        StoreUserRequest requestData = new StoreUserRequest();
        requestData.Name = randomName(name);
        //サーバに送信するオブジェクトをJAONに変換response
        string json = JsonConvert.SerializeObject(requestData);
        //Send
        UnityWebRequest request = UnityWebRequest.Post(
            API_BASE_URL + "users/store", json, "application/json");

        yield return request.SendWebRequest();
        bool isSuccess = false;
        if(request.result == UnityWebRequest.Result.Success
             && request.responseCode == 200)
        {
            //通信が成功した場合、返ってきたJsonをオブジェクトに変換
            string resultJson = request.downloadHandler.text;
            StoreUserresponse response =
                JsonConvert.DeserializeObject<StoreUserresponse>(resultJson);
            //ファイルにユーザIDを保存
            this.userName = requestData.Name;
            this.userID = response.UserID;
            SaveUserData();
            isSuccess = true;
        }
        result?.Invoke(isSuccess);//ここで呼び出し元のresult処理を呼ぶ
    }

    /// <summary>
    /// ユーザ情報の保存
    /// </summary>
    private void SaveUserData()
    {
        SaveData saveData = new SaveData();
        saveData.Name = this.userName;
        saveData.UserID = this.userID;
        saveData.Token = this.authToken;
        saveData.StageList = this.stageList;
        string json = JsonConvert.SerializeObject(saveData);
        var writer =
            new StreamWriter(Application.persistentDataPath + "/saveData.json");
        writer.Write(json);
        writer.Flush();
        writer.Close();
    }

    /// <summary>
    /// ユーザ情報の読み込み
    /// </summary>
    /// <returns></returns>
    public bool LoadUserData()
    {
        string path = Application.persistentDataPath;
        if(!File.Exists(Application.persistentDataPath + "/saveData.json"))
        {
            return false;
        }
        var reader =
            new StreamReader(Application.persistentDataPath + "/saveData.json");
        string json = reader.ReadToEnd();
        reader.Close();
        SaveData saveData = JsonConvert.DeserializeObject<SaveData>(json);
        this.userID = saveData.UserID;
        this.userName = saveData.Name;
        this.authToken = saveData.Token;
        this.stageList = saveData.StageList;
        return true;
    }

    /// <summary>
    /// ライバルランダム取得処理
    /// </summary>
    /// <param name="result"></param>
    /// <returns></returns>
    public IEnumerator GetProfile(Action<RivalResponse[]> result)
    {
        // ステージ一覧取得APIを実行
        UnityWebRequest request = UnityWebRequest.Get(
            API_BASE_URL + "battleMode/rivals/get");
        request.SetRequestHeader("Authorization", "Bearer " + authToken);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success
             && request.responseCode == 200)
        {
            //通信が成功した場合、返ってきたJsonをオブジェクトに変換
            string resultJson = request.downloadHandler.text;
            RivalResponse[] response =
                JsonConvert.DeserializeObject<RivalResponse[]>(resultJson);
            result?.Invoke(response);//ここで呼び出し元のresult処理を呼ぶ
        }
        else
        {
            result?.Invoke(null);
        }
    }

    /// <summary>
    /// ステージ一覧取得処理
    /// </summary>
    /// <param name="result"></param>
    /// <returns></returns>
    public IEnumerator GetStage(Action<StageResponse[]> result)
    {
        // ステージ一覧取得APIを実行
        UnityWebRequest request = UnityWebRequest.Get(
            API_BASE_URL + "stages");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success
             && request.responseCode == 200)
        {
            //通信が成功した場合、返ってきたJsonをオブジェクトに変換
            string resultJson = request.downloadHandler.text;
            StageResponse[] response =
                JsonConvert.DeserializeObject<StageResponse[]>(resultJson);
            result?.Invoke(response);//ここで呼び出し元のresult処理を呼ぶ
        }
        else
        {
            result?.Invoke(null);
        }     
    }

    /// <summary>
    /// 自分のプロフィール取得処理
    /// </summary>
    /// <param name="result"></param>
    /// <returns></returns>
    public IEnumerator GetMyProfile(Action<ProfileResponse[]> result)
    {
        // プロフィール取得APIを実行
        UnityWebRequest request = UnityWebRequest.Get(
            API_BASE_URL + "battleMode/show");
            request.SetRequestHeader("Authorization", "Bearer " + authToken);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success
             && request.responseCode == 200)
        {
            //通信が成功した場合、返ってきたJsonをオブジェクトに変換
            string resultJson = request.downloadHandler.text;
            ProfileResponse[] response =
                JsonConvert.DeserializeObject<ProfileResponse[]>(resultJson);

            result?.Invoke(response);//ここで呼び出し元のresult処理を呼ぶ
        }
        else
        {
            result?.Invoke(null);
        }
    }

    /// <summary>
    /// バトルモードプロフィール登録処理
    /// </summary>
    /// <param name="cardID"></param>
    /// <returns></returns>
    public IEnumerator StoreProfile()
    {
        //Create Object Send for Server
        StoreProfileRequest requestData = new StoreProfileRequest();

        //サーバに送信するオブジェクトをJAONに変換response
        string json = JsonConvert.SerializeObject(requestData);
        //Send
        UnityWebRequest request = UnityWebRequest.Post(
            API_BASE_URL + "battleMode/store", json, "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + authToken);
        yield return request.SendWebRequest();
    }

    /// <summary>
    /// バトルモードプロフィール更新処理
    /// </summary>
    /// <param name="cardID"></param>
    /// <returns></returns>
    public IEnumerator UpdateProfile(string name, string iconName)
    {
        //Create Object Send for Server
        StoreProfileRequest requestData = new StoreProfileRequest();

        // 取得したIDをリクエストに入れる
        requestData.Name = name;
        requestData.IconName = iconName;

        //サーバに送信するオブジェクトをJAONに変換response
        string json = JsonConvert.SerializeObject(requestData);
        //Send
        UnityWebRequest request = UnityWebRequest.Post(
            API_BASE_URL + "battleMode/update", json, "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + authToken);
        yield return request.SendWebRequest();
    }

    /// <summary>
    /// 使用可能カード一覧取得
    /// </summary>
    /// <param name="result"></param>
    /// <returns></returns>
    public IEnumerator GetUsableCard(Action<UsableCardResponse[]> result)
    {
        // ステージ一覧取得APIを実行
        UnityWebRequest request = UnityWebRequest.Get(
            API_BASE_URL + "battleMode/usableCard");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success
             && request.responseCode == 200)
        {
            //通信が成功した場合、返ってきたJsonをオブジェクトに変換
            string resultJson = request.downloadHandler.text;
            UsableCardResponse[] response =
                JsonConvert.DeserializeObject<UsableCardResponse[]>(resultJson);
            result?.Invoke(response);//ここで呼び出し元のresult処理を呼ぶ
        }
        else
        {
            result?.Invoke(null);
        }
    }

    /// </summary>
    /// パッシブアイテム一覧取得
    /// </summary>
    /// <param name="result"></param>
    /// <returns></returns>
    public IEnumerator GetItem(Action<ItemResponse[]> result)
    {
        // ステージ一覧取得APIを実行
        UnityWebRequest request = UnityWebRequest.Get(
            API_BASE_URL + "items");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success
             && request.responseCode == 200)
        {
            //通信が成功した場合、返ってきたJsonをオブジェクトに変換
            string resultJson = request.downloadHandler.text;
            ItemResponse[] response =
                JsonConvert.DeserializeObject<ItemResponse[]>(resultJson);
            result?.Invoke(response);//ここで呼び出し元のresult処理を呼ぶ
        }
        else
        {
            result?.Invoke(null);
        }
    }

    /// <summary>
    /// ステージクリアプロセス
    /// </summary>
    /// <param name="stageID"></param>
    public void ClearStage(int stageID)
    {
        if (stageList == null ) stageList = new List<int>();

        if (stageList.Contains(stageID))
        {
            return;
        }
        this.stageList.Add(stageID);
        SaveUserData();
    }

    public List<int> GetID()
    {
        return this.stageList;
    }

    /// <summary>
    /// ランダムネームコンバート処理
    /// </summary>
    public string randomName(string name)
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
        return name = firstName[num] + secondtName[num2];
    }
}
