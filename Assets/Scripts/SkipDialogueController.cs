using System;
using UnityEngine;
using UnityEngine.UI;
using Yarn.Unity;

public class SkipDialogueController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Toggle skipToggle;

    [Header("Yarn Components")]
    [SerializeField] private LinePresenter linePresenter;
    [SerializeField] private DialogueRunner dialogueRunner;

    [Header("Skip Settings")]
    [SerializeField] private float skipAutoAdvanceDelaySeconds = 0.2f;
    [SerializeField] private int instantLettersPerSecond = 100000;
    [SerializeField] private int instantWordsPerSecond = 100000;

    // Cached originals to restore when skip is turned off
    private bool originalAutoAdvance;
    private float originalAutoAdvanceDelay;
    private int originalLettersPerSecond;
    private int originalWordsPerSecond;
    private bool hasCachedOriginals = false;

    private void Awake()
    {
        if (linePresenter == null)
        {
            linePresenter = FindObjectOfType<LinePresenter>();
        }
        if (dialogueRunner == null)
        {
            dialogueRunner = FindObjectOfType<DialogueRunner>();
        }
        if (skipToggle != null)
        {
            skipToggle.onValueChanged.AddListener(OnSkipToggled);
        }
    }

    private void OnDestroy()
    {
        if (skipToggle != null)
        {
            skipToggle.onValueChanged.RemoveListener(OnSkipToggled);
        }
    }

    private void OnSkipToggled(bool isOn)
    {
        if (linePresenter == null)
        {
            Debug.LogWarning("SkipDialogueController: No LinePresenter assigned/found.");
            return;
        }

        if (isOn)
        {
            EnableSkip();
        }
        else
        {
            DisableSkip();
        }
    }

    private void EnableSkip()
    {
        if (!hasCachedOriginals)
        {
            originalAutoAdvance = linePresenter.autoAdvance;
            originalAutoAdvanceDelay = linePresenter.autoAdvanceDelay;
            originalLettersPerSecond = linePresenter.lettersPerSecond;
            originalWordsPerSecond = linePresenter.wordsPerSecond;
            hasCachedOriginals = true;
        }

        // Make typewriter effectively instant
        linePresenter.lettersPerSecond = instantLettersPerSecond;
        linePresenter.wordsPerSecond = instantWordsPerSecond;
        TrySetRuntimeTypewriterSpeed(instantLettersPerSecond, instantWordsPerSecond);

        // Auto-advance lines shortly after they finish revealing
        linePresenter.autoAdvance = true;
        linePresenter.autoAdvanceDelay = skipAutoAdvanceDelaySeconds;
    }

    private void DisableSkip()
    {
        if (!hasCachedOriginals)
        {
            return;
        }

        // Restore original speeds
        linePresenter.lettersPerSecond = originalLettersPerSecond;
        linePresenter.wordsPerSecond = originalWordsPerSecond;
        TrySetRuntimeTypewriterSpeed(originalLettersPerSecond, originalWordsPerSecond);

        // Restore auto-advance settings
        linePresenter.autoAdvance = originalAutoAdvance;
        linePresenter.autoAdvanceDelay = originalAutoAdvanceDelay;
    }

    // Attempts to update the active typewriter's runtime speed so changes take effect immediately
    private void TrySetRuntimeTypewriterSpeed(int lettersPerSecond, int wordsPerSecond)
    {
        if (linePresenter.typewriter == null)
        {
            return;
        }

        var basic = linePresenter.typewriter as BasicTypewriter;
        if (basic != null)
        {
            basic.CharactersPerSecond = lettersPerSecond;
            return;
        }

        var word = linePresenter.typewriter as WordTypewriter;
        if (word != null)
        {
            word.WordsPerSecond = wordsPerSecond;
        }
    }
}



