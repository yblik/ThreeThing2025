using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CutsceneSwitcher : MonoBehaviour
{
    //switches to a new scene after a specific delay e.g., switching to the main game scene after x ammount of seconds have passed
    [SerializeField]
    private float delayInSeconds = 5f;
    
    [SerializeField]
    private string nextSceneName = "";

    //start method
    void Start()
    {
        StartCoroutine(SwitchSceneAfterDelay());
    }

    private IEnumerator SwitchSceneAfterDelay()
    {
        yield return new WaitForSeconds(delayInSeconds);
        SceneManager.LoadScene(nextSceneName);
    }

    // Update method empty atm
    void Update()
    {
        
    }
}
