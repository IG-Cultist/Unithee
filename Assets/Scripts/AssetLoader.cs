using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AssetLoader : MonoBehaviour
{
    [SerializeField] Slider loadingSlider;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(loading());
    }
    
    IEnumerator loading()
    {
        //カタログ更新
        var handle = Addressables.UpdateCatalogs();

        yield return handle;

        //ダウンロード実行
        AsyncOperationHandle downloadHandle =
            Addressables.DownloadDependenciesAsync("default", false);

        //ダウンロード完了までのスライダーUIを更新
        while (downloadHandle.Status == AsyncOperationStatus.None)
        {
            loadingSlider.value = downloadHandle.GetDownloadStatus().Percent * 100;
            yield return null;
        }

        loadingSlider.value = 100;
        Addressables.Release(downloadHandle);

        Addressables.LoadScene("Stage 1",LoadSceneMode.Additive);
    }
}
