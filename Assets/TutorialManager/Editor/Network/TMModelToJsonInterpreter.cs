using System.Collections.Generic;
using UnityEngine.Analytics.TutorialManagerRuntime;
using UnityEngine.Analytics.TutorialManagerEditor;
using System.Linq;

namespace UnityEngine.Analytics
{
    public static class TMModelToJsonInterpreter
    {
        public static string ProcessModelToJson(TutorialManagerModel model)
        {
            var rsDataList = new RemoteSettingsData();
            rsDataList.genre = model.genre;
            rsDataList.remoteSettings = new List<TMRemoteSettingsKeyValueType>();
            string tutorialsValue = null;
            if(model.tutorials.Count == 1)
            {
                tutorialsValue = "[" + Quotify(model.tutorials[0].id) + "]";
            }
            else
            {
                tutorialsValue = "[" + model.tutorials.Select(x => x.id).Aggregate((current, next) => Quotify(current) + "," + Quotify(next)) + "]";
            }

            rsDataList.remoteSettings.Add(MakeRSJSONObject("tutorials", tutorialsValue));

            var stepsList = model.tutorials.Select(x => {
                if(x.steps.Count == 1)
                {
                    return MakeRSJSONObject(x.id, "[" + Quotify(x.steps[0]) + "]");
                }
                return MakeRSJSONObject(x.id, "[" + x.steps.Select(y => y).Aggregate((current, next) => Quotify(current) + "," + Quotify(next)) + "]");
            });
            rsDataList.remoteSettings.AddRange(stepsList);

            var contentList = model.content.Select(x =>
            {
                return MakeRSJSONObject(x.id, x.text);
            });
            rsDataList.remoteSettings.AddRange(contentList);

            return JsonUtility.ToJson(rsDataList);
        }

        static string Quotify(string text)
        {
            string retStr = text;
            if(text.StartsWith("\"", System.StringComparison.InvariantCulture) == false)
            {
                retStr = "\"" + retStr;
            }
            if(text.EndsWith("\"", System.StringComparison.InvariantCulture) == false)
            {
                retStr = retStr + "\"";
            }
            return retStr;
        }

        static TMRemoteSettingsKeyValueType MakeRSJSONObject (string key, string value)
        {
            return new TMRemoteSettingsKeyValueType(key, value, "string");
        }
    }
}
