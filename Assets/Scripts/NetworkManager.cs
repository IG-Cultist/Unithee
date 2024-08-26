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
    const string API_BASE_URL = "http://localhost:8000/api/";
    private int userID = 0;
    private string userName = "";
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
        return true;
    }
}
