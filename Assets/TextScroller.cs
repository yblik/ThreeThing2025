using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TextScroller : MonoBehaviour
{
    //basic script that scrolls text from the bottom Y coords to the top Y coords 
    [Header("Scroll Settings")]
    [SerializeField] private float scrollSpeed = 50f; // units per second affects the scrolling speed of text
    [SerializeField] private float startDelay = 1f; // delay before scrolling text starts
    [SerializeField] private bool loopScroll = true; // this loops the scrolling text when it reaches the end 
    
    [Header("Text Bounds")]
    [SerializeField] private float startPositionY = -300f; // Y position where text starts
    [SerializeField] private float endPositionY = 1000f; // Y position where text wraps back to start
    
    [Header("Optional: Auto-load Scene (when not looping)")]
    [SerializeField] private bool loadSceneWhenComplete = false;
    [SerializeField] private string nextSceneName = "MainMenu";
    [SerializeField] private float delayBeforeNextScene = 1f;

    private RectTransform rectTransform;
    private bool isScrolling = false;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        if (rectTransform == null)
        {
            Debug.LogError("TextScroller requires a RectTransform component. Attach this to a UI element.");
            enabled = false;
            return;
        }
        
        // Sets text to initial start position
        Vector2 pos = rectTransform.anchoredPosition;
        pos.y = startPositionY;
        rectTransform.anchoredPosition = pos;
    }

    private void Start()
    {
        StartCoroutine(StartScrolling());
    }
    //basically moves the text upwards and has the loop functionality 
    private void Update()
    {
        if (isScrolling)
        {
            
            rectTransform.anchoredPosition += Vector2.up * scrollSpeed * Time.deltaTime;

            if (rectTransform.anchoredPosition.y >= endPositionY)
            {
                if (loopScroll)
                {
                    Vector2 pos = rectTransform.anchoredPosition;
                    pos.y = startPositionY;
                    rectTransform.anchoredPosition = pos;
                }
                else
                {
                    OnScrollComplete();
                }
            }
        }
    }

    private IEnumerator StartScrolling()
    {
        // delay for text scroll
        yield return new WaitForSeconds(startDelay);
        isScrolling = true;
    }

    private void OnScrollComplete()
    {
        isScrolling = false;

        if (loadSceneWhenComplete)
        {
            StartCoroutine(LoadNextScene());
        }
    }

    private IEnumerator LoadNextScene()
    {
        yield return new WaitForSeconds(delayBeforeNextScene);
        
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.LogWarning("Next scene name is not set.");
        }
    }
// public method scrolls manually and self explanatory for the rest
    public void StartScroll()
    {
        if (!isScrolling)
        {
            StartCoroutine(StartScrolling());
        }
    }

    public void StopScroll()
    {
        isScrolling = false;
    }

    public void TogglePause()
    {
        isScrolling = !isScrolling;
    }

    public void ResetToStart()
    {
        isScrolling = false;
        Vector2 pos = rectTransform.anchoredPosition;
        pos.y = startPositionY;
        rectTransform.anchoredPosition = pos;
    }
}
