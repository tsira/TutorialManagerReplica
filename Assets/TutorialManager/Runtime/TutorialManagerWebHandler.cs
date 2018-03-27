using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Tutorial Manager Web Handler provides helper methods to the Tutorial Manager plugin code.
/// </summary>

public class TutorialManagerWebHandler : MonoBehaviour
{
    public delegate void HandlePostResponse(UnityWebRequest request);
    public static event HandlePostResponse PostRequestReturned;

    void Awake()
    {
        DontDestroyOnLoad(transform.gameObject);
    }

    public void PostJson(string url, string json)
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
#pragma warning disable 618
            yield return request.Send();
#pragma warning restore 618
            if (PostRequestReturned != null)
            {
                PostRequestReturned(request);
            }
        }
    }
}
