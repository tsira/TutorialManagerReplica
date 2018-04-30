using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TutorialManager;

public class ForceUpdate : MonoBehaviour {

    public void ForceMe() {
        TMParser.ForceUpdate();
    }

    public void ForceSave() {
        TMParser.SaveToJson();
    }
}
