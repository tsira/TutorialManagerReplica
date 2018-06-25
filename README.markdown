Tutorial Manager Plugin
==================

## Intro
Plugin for Tutorial Manager (follow development [here](https://jira.hq.unity3d.com/issues/?jql=project+%3D+UACD+AND+component+%3D+%22TM+Unity+Plugin%22)), please see this repo’s [tags](https://gitlab.internal.unity3d.com/ContextualDataAnalytics/tutorial-manager/tags) for the most up-to-date releases.

## What's it for?
Our hypothesis is that different players need different onboarding experiences. This plugin makes testing this hypothesis possible. By hooking a key decision point (show/don't show the tutorial) to the game, we can reinforce a model which will intelligently predict and improve. If we're successful, we can ultimately deliver many smart decisions on a per-player basis.

## Getting Started

### Implementation Instructions
[How to integrate TM in a game](https://docs.google.com/document/d/1kp1RzsRNfh8AC9Fhy-JyDCTCUxiTiho9pKg-r3a8SEA/edit)

### Test Project
[Repo](https://gitlab.internal.unity3d.com/ContextualDataAnalytics/tutorial-manager-test-project)

### Running Unit Tests
  0. In Unity, go to `Window` -> `Test Runner`
  0. Enable play mode tests in the Test Runner window
  0. Restart the editor
  0. Click `Run All` in the Test Runner window
    - Right click to run specific tests
  
  **NOTE**: if you make changes, wait for recompilation to finish before trying to run tests - otherwise, it’ll save a test scene and stop mid test

## Other Things To Know
  - While doing manual testing, to ensure a decision is made each time do the following:
    - Delete the analytics values file:
    
    ```csharp
    if (File.Exists(Application.persistentDataPath + "/Unity/" + Application.cloudProjectId + "/Analytics/values"))
    {
        File.Delete(Application.persistentDataPath + "/Unity/" + Application.cloudProjectId + "/Analytics/values");
    }
    ```
    - Delete PlayerPrefs
    
    ```csharp
    PlayerPrefs.DeleteAll();
    PlayerPrefs.Save();
    ```
    - Alternatively, just delete the key that Tutorial Manager uses to cache the decision:
    
    ```csharp
    PlayerPrefs.DeleteKey("adaptive_onboarding_show_tutorial");
    PlayerPrefs.Save();
    ```
    
## Building a .unitypackage for release
As of release `toolkit-v1` ONLY the following directories should be *included* in the build:
TutorialManager
    > Editor
    > Runtime

Further, the following directory should be *excluded*:
TutorialManager
    > Editor
        > Tests

## Still have questions?
Join the Contextual Data slack channel and hit us with your questions! #contextual-data
