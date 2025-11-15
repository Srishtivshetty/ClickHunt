using System.Collections;
using UnityEngine;
using TMPro;

public class LogoSequence : MonoBehaviour
{
    public TextMeshProUGUI gameNameText;   // The title text shown on startup (fades in)
    public GameObject startButton;        // The Start button that appears after the text fades in

    public float delayBeforeTransition = 1.5f;  // Time to wait before the fade begins
    public float fadeDuration = 1.5f;           // Time it takes to fade the title text in

    void Start()
    {
        // Ensure game name text starts invisible at beginning
        if (gameNameText != null)
            gameNameText.alpha = 0;

        // Hide start button at beginning
        if (startButton != null)
            startButton.SetActive(false);

        // Start sequence
        StartCoroutine(ShowTitleAndButton());
    }
    
    // Handles the delay, fade-in animation, and showing the start button.
    IEnumerator ShowTitleAndButton()
    {
        // Wait before fading in the logo text
        yield return new WaitForSeconds(delayBeforeTransition);

        float elapsed = 0f;

        // Fade in text
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            // Normalized time (0 → 1)
            float t = Mathf.Clamp01(elapsed / fadeDuration);

            if (gameNameText != null)    // Set text alpha (transparency)
                gameNameText.alpha = t;

            yield return null;           // Wait for next frame
        }

        if (gameNameText != null)        // Ensure final alpha is exactly 1
            gameNameText.alpha = 1;

        // Show start button after fade
        if (startButton != null)
            startButton.SetActive(true);
    }
}

