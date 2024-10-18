/*
 * BattleModeScript
 * Creator:êºâYçWëæ Update:2024/10/10
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BattleMode : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void exitBattleScene()
    {
        SceneManager.LoadScene("SelectScene");
    }
}
