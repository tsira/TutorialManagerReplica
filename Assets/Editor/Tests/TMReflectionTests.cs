#if UNITY_5_6_OR_NEWER
using NUnit.Framework;
using System;

public class TMReflectionTests
{
    [Test]
    public void TestReflectionAPIsStillValid()
    {
        var unityConnectObj = Type.GetType("UnityEditor.Connect.UnityConnect, UnityEditor");
        Assert.NotNull(unityConnectObj);

        var instanceProp = unityConnectObj.GetProperty("instance");
        Assert.NotNull(instanceProp);

        var instanceOfConnect = instanceProp.GetValue(null, null);
        Assert.NotNull(instanceOfConnect);

        var getTokenMethod = unityConnectObj.GetMethod("GetAccessToken");
        Assert.NotNull(getTokenMethod);

        var token = getTokenMethod.Invoke(instanceOfConnect, null);
        Assert.NotNull(token);
    }
}
#endif