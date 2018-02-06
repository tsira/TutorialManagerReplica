using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;

public static class ABTestingWrapper
{
    const string version = "1.0.1";
    const string bucketA = "_a";
    const string bucketB = "_b";
    const string bucketDefault = "default";

    public static string PlayerBucket = PlayerPrefs.GetString("unity_analytics_ab_test_bucket");
    static string defaultTestName = "missing";
    public static string testName = defaultTestName;
    static float percentage_a;
    static float percentage_b;
    static float percentage_default;

    static bool hasLocalPercentages = false;

    public static void Configure(string tn, float pa, float pb) {
        testName = tn;
        percentage_a = pa;
        percentage_b = pb;

        hasLocalPercentages = true;
    }

    /// <summary>
    /// Shim for Remote Settings GetInt
    /// </summary>
    /// <returns>The int.</returns>
    /// <param name="key">Key.</param>
    /// <param name="defaultValue">Default value.</param>
    public static int GetInt(string key, int defaultValue = 0)
    {
        EnsureBucket();
        string bucketKey = GetBucketKey(key);
        return UnityEngine.RemoteSettings.GetInt(bucketKey, defaultValue);
    }

    /// <summary>
    /// Shim for RemoteSettings GetFloat
    /// </summary>
    /// <returns>The float.</returns>
    /// <param name="key">Key.</param>
    /// <param name="defaultValue">Default value.</param>
    public static float GetFloat(string key, float defaultValue = 0f)
    {
        EnsureBucket();
        string bucketKey = GetBucketKey(key);
        return UnityEngine.RemoteSettings.GetFloat(bucketKey, defaultValue);
    }

    /// <summary>
    /// Shim for RemoteSettings GetString
    /// </summary>
    /// <returns>The string.</returns>
    /// <param name="key">Key.</param>
    /// <param name="defaultValue">Default value.</param>
    public static string GetString(string key, string defaultValue = "")
    {
        EnsureBucket();
        string bucketKey = GetBucketKey(key);
        return UnityEngine.RemoteSettings.GetString(bucketKey, defaultValue);
    }

    /// <summary>
    /// Shim for RemoteSettings GetBool
    /// </summary>
    /// <returns><c>true</c>, if bool was gotten, <c>false</c> otherwise.</returns>
    /// <param name="key">Key.</param>
    /// <param name="defaultValue">If set to <c>true</c> default value.</param>
    public static bool GetBool(string key, bool defaultValue = false)
    {
        EnsureBucket();
        string bucketKey = GetBucketKey(key);
        return UnityEngine.RemoteSettings.GetBool(bucketKey, defaultValue);
    }

    // Make sure we have a bucket.
    public static void EnsureBucket(bool verboseWarnings = true)
    {
        if (NeedsBucket(verboseWarnings))
        {
            AssignBucket();
        }
    }

    // Do we need a bucket or does it already exist?
    private static bool NeedsBucket(bool verboseWarnings = true)
    {
        return (PercentageABKeyExists(verboseWarnings) && !PlayerBucketExists());
    }

    // Take a key and return it with the correct bucket suffix
    private static string GetBucketKey(string key)
    {
        string bucket = (PlayerBucket != bucketDefault) ? PlayerBucket : "";
        VariableABKeysExist(key);
        return (UnityEngine.RemoteSettings.HasKey(key + bucket)) ? key + bucket : key;
    }

    // Have we ever saved a bucket to PlayerPrefs?
    private static bool PlayerBucketExists()
    {
        return !string.IsNullOrEmpty(PlayerBucket);
    }

    // Do BOTH percentage_a and perentage_b exist?
    private static bool PercentageABKeyExists(bool verboseWarnings = true)
    {
        bool result = false;

        bool hasA = UnityEngine.RemoteSettings.HasKey("percentage_a");
        bool hasB = UnityEngine.RemoteSettings.HasKey("percentage_b");

        if ((hasA && hasB) || hasLocalPercentages)
        {
            result = true;
        }
        else if(verboseWarnings)
        {
            if (!hasA)
            {
                Debug.LogWarning("Warning: the 'percentage_a' key was not found in your Remote Settings. Default value will be returned");
            }
            if (!hasB)
            {
                Debug.LogWarning("Warning: the 'percentage_b' key was not found in your Remote Settings. Default value will be returned");
            }
        }
        return result;
    }

    // Do <variable>, <variable>_a and <variable>_b exist?
    private static void VariableABKeysExist(string key)
    {
        if (!UnityEngine.RemoteSettings.HasKey(key))
        {
            Debug.LogWarning("Warning: the key " + key + " was not found in your Remote Settings.  Default value will be returned");
        }

        if (!UnityEngine.RemoteSettings.HasKey(key + bucketA))
        {
            Debug.LogWarning("Warning: the key " + key + "_a was not found in your Remote Settings.  Default value will be returned");
        }

        if (!UnityEngine.RemoteSettings.HasKey(key + bucketB))
        {
            Debug.LogWarning("Warning: the key " + key + "_b was not found in your Remote Settings.  Default value will be returned");
        }
    }

    // Put this player in a random bucket
    private static void AssignBucket()
    {
        SanitizeBucketProbabilities();
        var a = new ProportionValue(percentage_a, bucketA);
        var b = new ProportionValue(percentage_b, bucketB);
        var c = new ProportionValue(percentage_default, bucketDefault);

        var bucketList = new[] {
            a, b, c
        };
        string bucket = ChooseByRandom(bucketList);
        SetBucketPlayerPrefs(bucket);
        SendBucketingEvent(bucket);
    }

    // Save the player's bucket both locally and to the Analytics server
    private static void SetBucketPlayerPrefs(string bucketSuffix)
    {
        PlayerPrefs.SetString("unity_analytics_ab_test_bucket", bucketSuffix);
        PlayerPrefs.Save();
        PlayerBucket = PlayerPrefs.GetString("unity_analytics_ab_test_bucket");
        testName = UnityEngine.RemoteSettings.GetString("ab_test_name", testName);
        if (testName == defaultTestName)
        {
            Debug.LogWarningFormat("Warning: ab_test_name key was not found in your Remote Settings.  The default value {0} will be assigned.", defaultTestName);
        }
    }

    private static void SendBucketingEvent(string bucketSuffix) {
        Analytics.CustomEvent("ab_test_bucket_assignment", new Dictionary<string, object>
        {
            { "bucket_name", bucketSuffix },
            { "ab_test_name", testName }
        });
    }

    // Ensure percentage_a and percentage_b sum up to values between 0f-1f.
    private static void SanitizeBucketProbabilities()
    {
        percentage_a = UnityEngine.RemoteSettings.GetFloat("percentage_a", percentage_a);
        percentage_b = UnityEngine.RemoteSettings.GetFloat("percentage_b", percentage_b);

        if (AreNotValidPercentages(percentage_a, percentage_b))
        {
            Debug.LogWarning("Warning:  Invalid percentage values for percentage_a and/or percentage_b.  Users will be placed in default bucket and default value will be returned.");
            percentage_a = 0f;
            percentage_b = 0f;
            percentage_default = 1f;
        }
        else
        {
            percentage_default = 1f - (percentage_a + percentage_b);
        }
    }

    // Struct to hold the likelihood of selection
    private struct ProportionValue
    {
        public float Proportion;
        public string Value;

        public ProportionValue(float proportion, string val)
        {
            this.Proportion = proportion;
            this.Value = val;
        }
    }

    // Chooses a bucket from a random value
    private static string ChooseByRandom(IEnumerable<ProportionValue> collection)
    {
        var rnd = UnityEngine.Random.value;
        foreach (var item in collection)
        {
            if (rnd <= item.Proportion)
                return item.Value;
            rnd -= item.Proportion;
        }
        return bucketDefault;
    }

    // Validates if sum of double1 + double2 values OR individual values are less than 0d or greater than 1d.
    private static bool AreNotValidPercentages(double double1, double double2)
    {
        // Using a hard coded value instead of Epsilon due to a precision error when getting the target value.
        // For instance, the target value property of 2.3 is actually 2.29999995231628
        return (double1 < 0f) || (double2 < 0f) || (double1 + double2 > 1f);
    }
}