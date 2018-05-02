#if UNITY_5_6_OR_NEWER
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using System.IO;

public class NewUserTests {

    GameObject testUtilGO;
    TutorialManagerWebHandlerTest testMono;

    bool requestReturned = false;

    [UnityTest]
    [PrebuildSetup(typeof(TutorialManager_NewUserPrebuild))]
	public IEnumerator CachedResponseMatchesServerResponse() {
        testUtilGO = GameObject.Find("TEST-GO");
        testMono = testUtilGO.GetComponent<TutorialManagerWebHandlerTest>();
        yield return new WaitUntil(() => testMono.IsTestFinished);
        yield return new WaitUntil(() => PlayerPrefs.HasKey("adaptive_onboarding_show_tutorial")); 
        Assert.AreEqual(PlayerPrefs.GetInt("adaptive_onboarding_show_tutorial"), testMono.toShow);
        GameObject.Destroy(testUtilGO);
	}
}

public class OldUserTests
{
    [UnityTest]
    [PrebuildSetup(typeof(TutorialManager_OldUserPrebuild))]
    public IEnumerator OldPlayer_ServerNoHit()
    {
        yield return new WaitUntil(() => Time.timeSinceLevelLoad > 10.0f);
        Assert.IsFalse(PlayerPrefs.HasKey("adaptive_onboarding_show_tutorial"));
    }
}

public class TutorialManagerWebHandlerTest : MonoBehaviour
{
    bool testFinished = false;
    public bool responseReturned = false;
    public int toShow = -1;

    private void Awake()
    {
        DontDestroyOnLoad(transform.gameObject);
        TutorialManagerWebHandler.PostRequestReturned += TestResponseCaching;
    }

    public virtual void TestResponseCaching(UnityWebRequest request)
    {
        //var deviceInfo = JsonUtility.FromJson<TutorialManager.DeviceInfo>(System.Text.Encoding.UTF8.GetString(request.uploadHandler.data));
        var response = JsonUtility.FromJson<TutorialManager.TutorialWebResponse>(request.downloadHandler.text);
        toShow = System.Convert.ToInt32(response.showTutorial);
        testFinished = true;
    }

    public bool IsTestFinished
    {
        get { return testFinished; }
    }
}

public class TutorialManager_NewUserPrebuild : IPrebuildSetup
{
    public void Setup ()
    {
        var go = new GameObject();
        go.name = "TEST-GO";
        go.AddComponent<TutorialManagerWebHandlerTest>();

        if (File.Exists(Application.persistentDataPath + "/Unity/" + Application.cloudProjectId + "/Analytics/values"))
        {
            File.Delete(Application.persistentDataPath + "/Unity/" + Application.cloudProjectId + "/Analytics/values");
        }
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
    }
}

public class TutorialManager_OldUserPrebuild : IPrebuildSetup
{
    public void Setup ()
    {
        if (!File.Exists(Application.persistentDataPath + "/Unity/" + Application.cloudProjectId + "/Analytics/values"))
        {
            File.Create(Application.persistentDataPath + "/Unity/" + Application.cloudProjectId + "/Analytics/values");
        }
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
    }
}
#endif