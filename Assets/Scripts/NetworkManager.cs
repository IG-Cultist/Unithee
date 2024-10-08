using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
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
    private static NetworkManager instance;
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
    /// ユーザ登録処理
    /// </summary>
    /// <param name="name"></param>
    /// <param name="result"></param>
    /// <returns></returns>
    public IEnumerator StoreUser(string name,Action<bool> result)
    {
        //Create Object Send for Server
        StoreUserRequest requestData = new StoreUserRequest();
        requestData.Name = name;
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
            this.userName = name;
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
        this.stageList = saveData.StageList;
        return true;
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
    /// 使用可能カード一覧取得
    /// </summary>
    /// <param name="result"></param>
    /// <returns></returns>
    public IEnumerator GetUsableCard(Action<UsableCardResponse[]> result)
    {
        // ステージ一覧取得APIを実行
        UnityWebRequest request = UnityWebRequest.Get(
            API_BASE_URL + "battleMode/usableCards");

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
}
