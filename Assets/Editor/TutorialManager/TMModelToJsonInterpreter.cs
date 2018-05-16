using UnityEngine;
using UnityEditor;
using UnityEngine.Analytics.TutorialManagerRuntime;
using System.Linq;

namespace UnityEngine.Analytics
{
    public static class TMModelToJsonInterpreter
    {
        public static string ProcessModelToJson(TutorialManagerModel model)
        {
            string tutorials = Quotify("tutorials") + ":" + Quotify(
                "[" + model.tutorials.Select(x => x.id).Aggregate((current, next) => current + "," + next) + "]"
            );

            string steps = model.tutorials.Select(x => {
                return Quotify(x.id) + ":" + Quotify(
                    "[" + x.steps.Select(y => y).Aggregate((current, next) => current + "," + next) + "]");
            }).Aggregate((current, next) => current + "," + next);

            string content = string.Empty;
            if (model.content.Count > 0) {
                content = model.content.Select(x => {
                    return Quotify(x.id) + ":" + Quotify(x.text);
                }).Aggregate((current, next) => current + "," + next);
            }

            string retv = string.Empty;
            const string contentPattern = "\"remoteSettings\":{{{0},{1},{2}}}";
            const string noContentPattern = "\"remoteSettings\":{{{0},{1}}}";

            if (content == string.Empty) {
                retv = string.Format(noContentPattern, tutorials, steps);
            } else {
                retv = string.Format(contentPattern, tutorials, steps, content);
            }

            return retv;
        }

        static string Quotify(string text)
        {
            return "\"" + text + "\"";
        }
    }
}
