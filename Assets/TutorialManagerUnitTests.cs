using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using System.Reflection;
using System.IO;

public class TutorialManagerUnitTests : IPrebuildSetup {

    GameObject testUtilGO;
    TutorialManagerWebHandlerTest testMono;

    bool requestReturned = false;

	[Test]
	public void TutorialManagerUnitTestsSimplePasses() {
		// Use the Assert class to test conditions.
	}

    public void Setup()
    {
        testUtilGO = new GameObject();
        testUtilGO.name = "TEST-GO";
        testUtilGO.AddComponent<TutorialManagerWebHandlerTest>();

        if(File.Exists(Application.persistentDataPath + "/Unity/" + Application.cloudProjectId + "/Analytics/values"))
        {
            File.Delete(Application.persistentDataPath + "/Unity/" + Application.cloudProjectId + "/Analytics/values");
        }
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
    }

	[UnityTest]
	public IEnumerator TutorialManagerUnitTestsWithEnumeratorPasses() {
        testUtilGO = GameObject.Find("TEST-GO");
        testMono = testUtilGO.GetComponent<TutorialManagerWebHandlerTest>();
        yield return new WaitUntil(() => testMono.IsTestFinished);
        Assert.AreEqual(PlayerPrefs.GetInt("adaptive_onboarding_show_tutorial"), testMono.toShow);
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
        toShow = System.Convert.ToInt32(response.show_tutorial);
        responseReturned = true;
    }

    private void Update()
    {
        if(responseReturned && PlayerPrefs.HasKey("adaptive_onboarding_show_tutorial"))
        {
            testFinished = true;
        }
    }

    public bool IsTestFinished
    {
        get { return testFinished; }
    }
}