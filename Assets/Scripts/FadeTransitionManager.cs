using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages fade transitions for screen blocking UI elements.
/// Provides functionality to fade in/out a UI blocker over the screen.
/// </summary>
public class FadeTransitionManager : MonoBehaviour
{
    [Header("Fade Transition Settings")]
    [SerializeField] private Image uIScreenBlocker;
    
    private static FadeTransitionManager instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Debug.LogWarning("Multiple FadeTransitionManager instances found. Keeping the first one.");
            Destroy(this);
        }
    }

    /// <summary>
    /// Performs a fade transition (fade to black, wait, then fade back to transparent).
    /// The total duration is customizable, with 0.5s for fade in and 0.5s for fade out.
    /// </summary>
    /// <param name="totalDuration">Total duration of the fade transition (default 1.0s for 0.5s in + 0.5s out)</param>
    public void StartFadeTransition(float totalDuration = 1.0f)
    {
        if (uIScreenBlocker == null)
        {
            Debug.LogError("UIScreenBlocker is not assigned in FadeTransitionManager!");
            return;
        }

        StartCoroutine(FadeTransitionCoroutine(totalDuration));
    }

    private IEnumerator FadeTransitionCoroutine(float totalDuration)
    {
        // Ensure the blocker GameObject is active and the Image component is enabled, starting transparent
        uIScreenBlocker.gameObject.SetActive(true);
        uIScreenBlocker.enabled = true;
        Color color = uIScreenBlocker.color;
        color.a = 0f;
        uIScreenBlocker.color = color;

        // Fade in to black (0.5 seconds)
        yield return StartCoroutine(FadeAlphaCoroutine(0f, 1f, 0.5f));

        // Calculate wait time (total duration minus fade in and fade out times)
        float waitTime = totalDuration - 1.0f; // 1.0f = 0.5s in + 0.5s out
        if (waitTime > 0f)
        {
            yield return new WaitForSeconds(waitTime);
        }

        // Fade out to transparent (0.5 seconds)
        yield return StartCoroutine(FadeAlphaCoroutine(1f, 0f, 0.5f));

        // Disable the Image component and deactivate the GameObject after fade completes
        uIScreenBlocker.enabled = false;
        uIScreenBlocker.gameObject.SetActive(false);

        Debug.Log($"Completed fade transition (total duration: {totalDuration}s)");
    }

    private IEnumerator FadeAlphaCoroutine(float startAlpha, float endAlpha, float duration)
    {
        float elapsedTime = 0f;
        Color color = uIScreenBlocker.color;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;
            color.a = Mathf.Lerp(startAlpha, endAlpha, t);
            uIScreenBlocker.color = color;
            yield return null;
        }

        // Ensure we end at the exact target alpha
        color.a = endAlpha;
        uIScreenBlocker.color = color;
    }

    /// <summary>
    /// Static method to access the singleton instance.
    /// Used by DialogueYarnCommands.
    /// </summary>
    public static void TriggerFadeTransition(float totalDuration = 1.0f)
    {
        if (instance == null)
        {
            Debug.LogError("FadeTransitionManager not found in scene! Please add it to a GameObject.");
            return;
        }

        instance.StartFadeTransition(totalDuration);
    }
}

