using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class TutorialManagerWebHandler : MonoBehaviour
{
    public delegate void HandleWebResponse(UnityWebRequest request);
    public static event HandleWebResponse WebRequestReturned;
    //const string url = "https://stg-adaptive-onboarding.uca.cloud.unity3d.com/tutorial";
    const string url = "https://prd-adaptive-onboarding.uca.cloud.unity3d.com/tutorial";

    void Awake()
    {
        DontDestroyOnLoad(transform.gameObject);
    }

    public void StartWebRequest(string json)
    {
        var request = new UnityWebRequest(url, "POST");
        var uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(json));
        uploadHandler.contentType = "application/json";
        request.uploadHandler = uploadHandler;
        request.downloadHandler = new DownloadHandlerBuffer();
        StartCoroutine(SendWebRequest(request));
    }

    IEnumerator SendWebRequest(UnityWebRequest request)
    {
        using (request)
        {
            yield return request.Send();
            if (WebRequestReturned != null)
            {
                WebRequestReturned(request);
            }
        }
    }
}
