using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SelectScene : MonoBehaviour
{
    [SerializeField] GameObject info;
    // Start is called before the first frame update
    void Start()
    {
        info.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void selectStage()
    {
        info.SetActive(true);
    }

    public void exitInfo()
    {
        info.SetActive(false);
    }

    public void startScene()
    {
        SceneManager.LoadScene("Main");
    }
}
