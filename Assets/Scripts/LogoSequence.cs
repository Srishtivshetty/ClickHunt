using System.Collections;
using UnityEngine;
using TMPro;

public class LogoSequence : MonoBehaviour
{
    public TextMeshProUGUI gameNameText;   
    public GameObject startButton;        

    public float delayBeforeTransition = 1.5f;  
    public float fadeDuration = 1.5f;          

    void Start()
    {
        // Ensure text starts invisible
        if (gameNameText != null)
            gameNameText.alpha = 0;

        // Hide start button at beginning
        if (startButton != null)
            startButton.SetActive(false);

        // Start sequence
        StartCoroutine(ShowTitleAndButton());
    }

    IEnumerator ShowTitleAndButton()
    {
        yield return new WaitForSeconds(delayBeforeTransition);

        float elapsed = 0f;

        // Fade in text
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);

            if (gameNameText != null)
                gameNameText.alpha = t;

            yield return null;
        }

        if (gameNameText != null)
            gameNameText.alpha = 1;

        // Show start button after fade
        if (startButton != null)
            startButton.SetActive(true);
    }
}

