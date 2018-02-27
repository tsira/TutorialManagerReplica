using System.Collections;
using System.Net;
using UnityEngine;

public class TutorialManagerWebHandler : MonoBehaviour {
    public delegate void HandleWebResponse(bool result);
    public static event HandleWebResponse WebRequestReturned;
    //const string url = "https://stg-adaptive-onboarding.uca.cloud.unity3d.com/tutorial";
    const string url = "https://prd-adaptive-onboarding.uca.cloud.unity3d.com/tutorial";
    bool toShow = true;
    bool requestReturned = false;

    void Awake()
    {
        DontDestroyOnLoad(transform.gameObject);
    }

    public void StartWebRequest(string payload)
    {
        using(var client = new WebClient())
        {
            client.UploadDataCompleted += Client_UploadDataCompleted;
            client.Headers.Add("Content-Type", "application/json");
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(payload);
            var uri = new System.Uri(url);
            try
            {
                client.UploadDataAsync(uri, "POST", bytes);
            }
            catch (WebException err)
            {
                Debug.LogError(err);
            }
            StartCoroutine("SendWebRequest");
        }
    }

    IEnumerator SendWebRequest()
    {
        yield return new WaitUntil(() => requestReturned == true);
        if(WebRequestReturned != null)
        {
            WebRequestReturned(toShow);
        }

    }

    void Client_UploadDataCompleted(object sender, UploadDataCompletedEventArgs e)
    {
        if (e.Error != null)
        {
            Debug.LogWarning("Error received from server: " + e.Error + ". Defaulting to true.");
        }
        else if (e.Cancelled)
        {
            Debug.LogWarning("The request was canceled. Defaulting to true.");
        }
        else
        {
            toShow = JsonUtility.FromJson<TutorialManager.TutorialWebResponse>(System.Text.Encoding.UTF8.GetString(e.Result)).show_tutorial;
        }
        requestReturned = true;
    }
}
