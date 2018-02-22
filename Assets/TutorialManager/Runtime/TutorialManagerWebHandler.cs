﻿using System.Collections;
using UnityEngine;

public class TutorialManagerWebHandler : MonoBehaviour {
    public delegate void HandleWebResponse(WWW webRequest);
    public static event HandleWebResponse WebRequestReturned;
    const string url = "https://stg-adaptive-onboarding.uca.cloud.unity3d.com/tutorial";

    void Awake()
    {
        DontDestroyOnLoad(transform.gameObject);
    }

    public void StartWebRequest(WWWForm webForm)
    {
        StartCoroutine(SendWebRequest(webForm));
    }

    IEnumerator SendWebRequest(WWWForm webForm)
    {
        using (WWW webRequest = new WWW(url, webForm))
        {
            yield return webRequest;

            if(WebRequestReturned != null)
            {
                WebRequestReturned(webRequest);
            }
        }
    }
}
