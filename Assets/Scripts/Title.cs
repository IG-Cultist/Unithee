/*
 * TitleScript
 * Creator:êºâYçWëæ Update:2024/07/24
*/
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Title : MonoBehaviour
{
    [SerializeField] Text txt;
    [SerializeField] GameObject title;
    int cnt = 0;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //ÉNÉäÉbÉN
        if (Input.GetMouseButtonUp(0))
        {
            SceneManager.LoadScene("SelectScene");
        }

        if (cnt == 0)
        {
            title.GetComponent<Renderer>().material.color = new Color32(255, 0, 0, 255);
            txt.color = new Color(0.0f, 0.0f, 1.0f, 1.0f);
            cnt = 1;
        }
        else if (cnt == 1)
        {
            title.GetComponent<Renderer>().material.color = new Color32(0, 255, 0, 255);
            txt.color = new Color(0.0f, 1.0f, 0.0f, 1.0f);
            cnt = 2;
        }
        else if (cnt == 2)
        {
            title.GetComponent<Renderer>().material.color = new Color32(0, 0, 255, 255);
            txt.color = new Color(1.0f, 0.0f, 0.0f, 1.0f);
            cnt = 0;
        }

    }
}
