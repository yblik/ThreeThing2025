using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    // Populate this list in the Inspector with the scene names you want selectable.
    // If left empty the script will auto-populate from the Scenes in Build Settings.
    [SerializeField] private List<string> scenes = new List<string>();

    // Scene names for your main menu buttons - set these in the Inspector
    [SerializeField] private string cutsceneIntroSceneName = "CutsceneIntro";
    [SerializeField] private string controlsSceneName = "Controls";
    [SerializeField] private string creditsSceneName = "Credits";

    private void Awake()
    {
        // Auto-fill from Build Settings if the list is empty.
        if (scenes == null || scenes.Count == 0)
        {
            PopulateScenesFromBuildSettings();
        }
    }

    private void PopulateScenesFromBuildSettings()
    {
        scenes = new List<string>();
        int count = SceneManager.sceneCountInBuildSettings;
        for (int i = 0; i < count; i++)
        {
            string path = SceneUtility.GetScenePathByBuildIndex(i);
            string name = Path.GetFileNameWithoutExtension(path);
            scenes.Add(name);
        }
    }

    // Wire this to your "Start" button
    public void StartGame()
    {
        LoadScene(cutsceneIntroSceneName);
    }

    // Wire this to your "Controls" button
    public void OpenControls()
    {
        LoadScene(controlsSceneName);
    }

    // Wire this to your "Credits" button
    public void OpenCredits()
    {
        LoadScene(creditsSceneName);
    }

    // Wire this to your "Exit" button
    public void ExitGame()
    {
        QuitGame();
    }

    // Generic loader by scene name. Can be wired to a Button and typed manually.
    public void LoadScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogWarning("LoadScene called with empty sceneName.");
            return;
        }
        StartCoroutine(LoadSceneAsync(sceneName));
    }

    // Call this from a Button OnClick and pass the index (0-based) of the scene in the Inspector list.
    // In the Button OnClick inspector select MainMenu -> LoadSceneByIndex and type the index.
    public void LoadSceneByIndex(int index)
    {
        if (scenes == null || index < 0 || index >= scenes.Count)
        {
            Debug.LogError($"LoadSceneByIndex: index {index} out of range (count={scenes?.Count}).");
            return;
        }
        LoadScene(scenes[index]);
    }

    // Useful if you want a "Play" button that goes to a specific cutscene scene.
    public void LoadCutsceneIntro()
    {
        LoadScene(cutsceneIntroSceneName);
    }

    // Load a random scene from the list.
    public void LoadRandomScene()
    {
        if (scenes == null || scenes.Count == 0)
        {
            Debug.LogError("LoadRandomScene: no scenes available.");
            return;
        }
        int idx = Random.Range(0, scenes.Count);
        LoadScene(scenes[idx]);
    }

    // Optional Quit button
    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private IEnumerator LoadSceneAsync(string sceneName)
    {
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        if (op == null)
        {
            Debug.LogError($"Failed to start async load for scene '{sceneName}'. Is it added to Build Settings?");
            yield break;
        }

        // Example: you can read op.progress (0..0.9) while loading and show UI.
        while (!op.isDone)
        {
            yield return null;
        }
    }
}
