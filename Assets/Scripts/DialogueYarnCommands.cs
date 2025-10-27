using UnityEngine;
using Yarn.Unity;
using TMPro;

/// <summary>
/// Adds additional Yarn command functionality for dialogue control.
/// Static methods here can be called directly from Yarn.
/// </summary>
public static class DialogueYarnCommands
{
    /// <summary>
    /// Yarn: <<hide_all_characters>>
    /// Hides all spawned characters using CharacterManager.
    /// </summary>
    [YarnCommand("hide_all_characters")]
    public static void HideAllCharacters()
    {
        CharacterManager.HideAllCharacters();
    }

    /// <summary>
    /// Yarn: <<hide_all_objects>>
    /// Hides all objects with YarnShowHideable components.
    /// </summary>
    [YarnCommand("hide_all_objects")]
    public static void HideAllObjects()
    {
        var showHideableObjects = Object.FindObjectsOfType<YarnShowHideable>();
        foreach (var obj in showHideableObjects)
        {
            if (obj != null)
            {
                obj.HideObject();
            }
        }
        Debug.Log($"Hid {showHideableObjects.Length} YarnShowHideable objects");
    }

    /// <summary>
    /// Yarn: <<hide_all_objects>>
    /// Hides all objects with YarnShowHideable components.
    /// </summary>
    [YarnCommand("hide_all_ui_objects")]
    public static void HideAllUIObjects()
    {
        var showHideableObjects = Object.FindObjectsOfType<UIYarnShowHideable>();
        foreach (var obj in showHideableObjects)
        {
            if (obj != null)
            {
                obj.HideObject();
            }
        }
        Debug.Log($"Hid {showHideableObjects.Length} UIYarnShowHideable objects");
    }

    /// <summary>
    /// Yarn: <<speed 2>> or <<speed 0.5>>
    /// Sets global dialogue typing speed multiplier.
    /// </summary>
    [YarnCommand("speed")]
    public static void SetGlobalSpeed(string multiplier)
    {
        if (double.TryParse(multiplier, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out double value))
        {
            if (value <= 0)
            {
                value = 0.0001; // clamp to tiny positive to avoid zero/negative
            }
            BasicTypewriter.GlobalSpeedMultiplier = value;
            Debug.Log($"Set global dialogue speed multiplier to {value}");
        }
        else
        {
            Debug.LogWarning($"Invalid speed value '{multiplier}'. Expected a number, e.g., 2 or 0.5");
        }
    }

    /// <summary>
    /// Yarn: <<play_music "AudioClipName">> or <<play_music "AudioClipName" 0.8>> or <<play_music "AudioClipName" 0.8 3.0>>
    /// Smoothly transitions to a new audio clip using AudioManager.
    /// Optional parameters: volume (0-1), transition time (seconds).
    /// </summary>
    [YarnCommand("play_music")]
    public static void PlayMusic(string clipName, string volume = "", string transitionTime = "")
    {
        // Use the singleton AudioManager instance
        if (AudioManager.Instance == null)
        {
            Debug.LogError($"AudioManager singleton not initialized. Cannot play music '{clipName}'");
            return;
        }

        // Load the audio clip from Resources
        AudioClip clip = Resources.Load<AudioClip>($"Audio/{clipName}");
        if (clip == null)
        {
            Debug.LogError($"Audio clip '{clipName}' not found in Resources/Audio/ folder");
            return;
        }

        // Parse optional volume
        float vol = 0.2f;
        if (!string.IsNullOrEmpty(volume))
        {
            if (float.TryParse(volume, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float volValue))
            {
                vol = Mathf.Clamp01(volValue); // Clamp between 0 and 1
            }
            else
            {
                Debug.LogWarning($"Invalid volume '{volume}'. Using default volume of 0.1.");
            }
        }

        // Parse optional transition time
        float transition = 0.5f;
        if (!string.IsNullOrEmpty(transitionTime))
        {
            if (float.TryParse(transitionTime, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float timeValue))
            {
                transition = timeValue;
            }
            else
            {
                Debug.LogWarning($"Invalid transition time '{transitionTime}'. Using AudioManager default.");
            }
        }

        // Call the AudioManager to change the clip
        AudioManager.Instance.ChangeAudioClip(clip, vol, transition);
        Debug.Log($"Playing music: {clipName} (volume: {vol}, transition: {transition}s)");
    }

    /// <summary>
    /// Yarn: <<stop_music>>
    /// Immediately stops all music without transition.
    /// </summary>
    [YarnCommand("stop_music")]
    public static void StopMusic()
    {
        if (AudioManager.Instance == null)
        {
            Debug.LogError("AudioManager singleton not initialized. Cannot stop music.");
            return;
        }

        AudioManager.Instance.StopAudio();
        Debug.Log("Stopped all music");
    }

    /// <summary>
    /// Yarn: <<play_music_start_loop "StartClipName" "LoopClipName">> or <<play_music_start_loop "StartClipName" "LoopClipName" 0.8>> or <<play_music_start_loop "StartClipName" "LoopClipName" 0.8 2.0>>
    /// Plays a start clip followed by a loopable clip with smooth transition.
    /// Optional parameters: volume (0-1), fade time (seconds).
    /// </summary>
    [YarnCommand("play_music_start_loop")]
    public static void PlayMusicStartLoop(string startClipName, string loopClipName, string volume = "", string fadeTime = "")
    {
        // Use the singleton AudioManager instance
        if (AudioManager.Instance == null)
        {
            Debug.LogError($"AudioManager singleton not initialized. Cannot play music start/loop '{startClipName}' -> '{loopClipName}'");
            return;
        }

        // Load the start audio clip from Resources
        AudioClip startClip = Resources.Load<AudioClip>($"Audio/{startClipName}");
        if (startClip == null)
        {
            Debug.LogError($"Start audio clip '{startClipName}' not found in Resources/Audio/ folder");
            return;
        }

        // Load the loop audio clip from Resources
        AudioClip loopClip = Resources.Load<AudioClip>($"Audio/{loopClipName}");
        if (loopClip == null)
        {
            Debug.LogError($"Loop audio clip '{loopClipName}' not found in Resources/Audio/ folder");
            return;
        }

        // Parse optional volume (default to 0.1)
        float vol = 0.1f;
        if (!string.IsNullOrEmpty(volume))
        {
            if (float.TryParse(volume, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float volValue))
            {
                vol = Mathf.Clamp01(volValue); // Clamp between 0 and 1
            }
            else
            {
                Debug.LogWarning($"Invalid volume '{volume}'. Using default volume of 0.1.");
            }
        }

        // Parse optional fade time (default to 0.5)
        float fade = 0.5f;
        if (!string.IsNullOrEmpty(fadeTime))
        {
            if (float.TryParse(fadeTime, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float fadeValue))
            {
                fade = Mathf.Max(0f, fadeValue); // Ensure non-negative
            }
            else
            {
                Debug.LogWarning($"Invalid fade time '{fadeTime}'. Using default fade time of 0.5s.");
            }
        }

        // Call the AudioManager to play start then loop
        AudioManager.Instance.PlayStartThenLoop(startClip, loopClip, vol, fade);
        Debug.Log($"Playing music start/loop: {startClipName} -> {loopClipName} (volume: {vol}, fade: {fade}s)");
    }

    /// <summary>
    /// Yarn: <<play_sound "SfxClipName">> or <<play_sound "SfxClipName" 0.7>>
    /// Plays a one-shot sound effect without interrupting current music.
    /// </summary>
    [YarnCommand("play_sound")]
    public static void PlaySound(string clipName, string volume = "")
    {
        if (AudioManager.Instance == null)
        {
            Debug.LogError($"AudioManager singleton not initialized. Cannot play sound '{clipName}'");
            return;
        }

        AudioClip clip = Resources.Load<AudioClip>($"Audio/SFX/{clipName}");
        if (clip == null)
        {
            Debug.LogError($"Sound effect clip '{clipName}' not found in Resources/Audio/SFX/ folder");
            return;
        }

        float vol = 1f;
        if (!string.IsNullOrEmpty(volume))
        {
            if (float.TryParse(volume, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float volValue))
            {
                vol = Mathf.Clamp01(volValue);
            }
            else
            {
                Debug.LogWarning($"Invalid volume '{volume}'. Using default volume of 1.0.");
            }
        }

        AudioManager.Instance.PlaySFXOneShot(clip, vol);
        Debug.Log($"Played sound: {clipName} (volume: {vol})");
    }

    /// <summary>
    /// Yarn: <<the_end "Ending 4/5: Loveless Fate">>
    /// Displays the game ending message on a UI text component.
    /// The ending message is fully configurable via the string parameter.
    /// </summary>
    [YarnCommand("the_end")]
    public static void ShowTheEnd(string endingMessage)
    {
        if (string.IsNullOrEmpty(endingMessage))
        {
            Debug.LogWarning("the_end command called with empty ending message");
            return;
        }

        // Look for the specific "EndingSubtext" GameObject
        GameObject endingSubtextObject = GameObject.Find("EndingSubtext");
        if (endingSubtextObject != null)
        {
            // Get the TextMeshProUGUI component directly on the EndingSubtext object
            var textComponent = endingSubtextObject.GetComponent<TextMeshProUGUI>();
            if (textComponent != null)
            {
                textComponent.text = endingMessage;
                textComponent.enabled = true;
                endingSubtextObject.SetActive(true);
                Debug.Log($"Displayed ending message on EndingSubtext: {endingMessage}");
                return;
            }
        }
        else
        {
            Debug.LogError("EndingSubtext GameObject not found in the scene. Cannot display ending message.");
        }
    }
}


