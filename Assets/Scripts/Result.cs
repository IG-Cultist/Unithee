/*
 * ResultScript
 * Creator:¼‰YW‘¾ Update:2024/07/24
*/
using UnityEngine;
using UnityEngine.SceneManagement;

public class Result : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        //ƒNƒŠƒbƒN
        if (Input.GetMouseButtonUp(0))
        {
            SceneManager.LoadScene("Title");
        }
    }
}
