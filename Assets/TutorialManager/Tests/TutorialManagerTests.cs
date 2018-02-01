using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialManagerTests : MonoBehaviour {

    static string testName = "flooble_test";
    static string tutorialKey = "show_tutorial";
    static float percentage_a = .25f;
    static float percentage_b = .25f;
    static int tutorialStep = 0;
    static bool tutorialShowDefaultValue = true;



    // Use this for initialization
    void Start()
    {
        //ABTestingWrapper.Configure(testName, percentage_a, percentage_b);

        //ABTestingWrapper.SetBucketPlayerPrefs("_c");



        //Debug.Log(PlayerPrefs.GetString("unity_analytics_ab_test_bucket"));
        //Debug.LogFormat("testName: {0}",  ABTestingWrapper.testName);

        int ab = 0;
        int bb = 0;
        int db = 0;


        int shown = 0;
        int notShown = 0;

        for (int i = 0; i < 500; i++)
        {
            bool toshow = TutorialManager.ShowTutorial();

            if (toshow)
                shown++;
            else
                notShown++;


            if (PlayerPrefs.GetString("unity_analytics_ab_test_bucket") == "_a")
                ab++;
            else if (PlayerPrefs.GetString("unity_analytics_ab_test_bucket") == "_b")
                bb++;
            else if (PlayerPrefs.GetString("unity_analytics_ab_test_bucket") == "default")
                db++;

            ABTestingWrapper.PlayerBucket = null;
                
            PlayerPrefs.DeleteKey("unity_analytics_ab_test_bucket");
            PlayerPrefs.Save();
        }

        //var a = new ABTestingWrapper.ProportionValue(percentage_a, "_a");
        //var b = new ABTestingWrapper.ProportionValue(percentage_b, "_b");
        //var c = new ABTestingWrapper.ProportionValue(1f - (percentage_a+percentage_b), "_default");

        //var bucketList = new[] {
        //    a, b, c
        //};

        //int ab = 0;
        //int bb = 0;
        //int db = 0;

        //for (int i = 0; i < 500; i++) {
        //    string bucket = ABTestingWrapper.ChooseByRandom(bucketList);
        //    if (bucket == "_a")
        //        ab++;
        //    if (bucket == "_b")
        //        bb++;
        //    if (bucket == "_default")
        //        db++;
                
        //}
       

        Debug.LogFormat("bucket: {0} {1} {2}", ab, bb, db);
        Debug.LogFormat("shown: {0} {1}", shown, notShown);
	}
}
