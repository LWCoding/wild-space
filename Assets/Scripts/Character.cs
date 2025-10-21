using UnityEngine;
using Yarn.Unity;
using System.Collections;

/// <summary>
/// Component that handles character-specific Yarn commands.
/// This should be attached to character GameObjects to enable character commands.
/// Works with the CharacterManager system for spawning and basic management.
/// </summary>
public class Character : MonoBehaviour
{
    [Header("Character Assignments")]
    [SerializeField] private GameObject positiveIndicatorPrefab;

    private Coroutine currentVoiceBlipCoroutine;
    private bool isSpeaking = false;
    private bool isVoiceBlipOn = false;
    private CharacterManager characterManager;
    private CharacterExpression lastSetExpression;

    /// <summary>
    /// Retrieves character data from the database when accessed
    /// </summary>
    private CharacterData CharacterData
    {
        get
        {
            if (CharacterDatabase.IsInitialized())
            {
                return CharacterDatabase.Instance.GetCharacter(gameObject.name);
            }
            return null;
        }
    }

    void Awake()
    {
        // Find the CharacterManager
        characterManager = FindObjectOfType<CharacterManager>();

        // Subscribe to the typewriter finished event to stop voice blips
        LinePresenter.OnTypewriterFinished += OnTypewriterFinished;
        LinePresenter.OnLineStarted += OnLineStarted;  // Resume voice blips if necessary
    }

    void OnDestroy()
    {
        // Unsubscribe to prevent memory leaks
        LinePresenter.OnTypewriterFinished -= OnTypewriterFinished;
        LinePresenter.OnLineStarted -= OnLineStarted;
    }

    /// <summary>
    /// Shows this character with a specific expression at a designated position.
    /// Usage in Yarn: <<show_character CharacterName Expression Position>>
    /// Positions: left, center, right, leftFar, rightFar
    /// </summary>
    /// <param name="expression">Expression/sprite name for the character</param>
    /// <param name="position">Position anchor (left, center, right, leftFar, rightFar)</param>
    [YarnCommand("show_character")]
    public void ShowCharacter(string expression, string position)
    {
        if (characterManager == null)
        {
            Debug.LogError("CharacterManager not found in scene!");
            return;
        }

        // Get the target anchor position
        Transform targetAnchor = characterManager.GetAnchorPosition(position);
        if (targetAnchor == null)
        {
            Debug.LogError($"Invalid position '{position}'. Valid positions are: left, center, right, leftFar, rightFar");
            return;
        }

        // Check if character data exists
        if (CharacterData == null)
        {
            Debug.LogError($"Character data not found for '{gameObject.name}'");
            return;
        }

        // Update position and rotation
        transform.position = targetAnchor.position;
        transform.rotation = targetAnchor.rotation;

        // Set the expression
        SetExpression(expression);

        // Flip Y scale if position is right or rightFar
        if (position == "right" || position == "rightFar")
        {
            Vector3 currentScale = transform.localScale;
            transform.localScale = new Vector3(-currentScale.x, currentScale.y, currentScale.z);
        }

        // Enable the character's SpriteRenderer
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
        }
    }

    /// <summary>
    /// Hides this character by disabling their SpriteRenderer.
    /// Usage in Yarn: <<hide_character CharacterName>>
    /// </summary>
    [YarnCommand("hide_character")]
    public void HideCharacter()
    {
        // Stop any active voice blips
        StopVoiceBlip();

        // Disable the character's SpriteRenderer
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = false;
        }
    }

    /// <summary>
    /// Starts voice blips for this character while they're speaking.
    /// Usage in Yarn: <<start_voice_blip CharacterName>>
    /// </summary>
    [YarnCommand("start_voice_blip")]
    public void StartVoiceBlip()
    {
        if (CharacterData == null)
        {
            Debug.LogWarning($"No character data found for '{gameObject.name}'");
            return;
        }

        // Use the last set expression, or fall back to default if none has been set
        CharacterExpression currentExpression = lastSetExpression ?? CharacterData.defaultExpression;
        if (currentExpression == null || currentExpression.VoiceBlip == null)
        {
            Debug.LogWarning($"No voice blip found for character '{gameObject.name}'");
            return;
        }

        isSpeaking = true;
        isVoiceBlipOn = true;
        currentVoiceBlipCoroutine = StartCoroutine(PlayVoiceBlips(currentExpression));
    }

    /// <summary>
    /// Stops this character from saying any more voice blips.
    /// Usage in Yarn: <<stop_voice_blip CharacterName>>
    /// </summary>
    [YarnCommand("stop_voice_blip")]
    public void StopVoiceBlip()
    {
        ShutUp();
        isVoiceBlipOn = false;
    }

    [YarnCommand("positive_indicator")]
    public void PositiveIndicator()
    {
        Instantiate(positiveIndicatorPrefab, transform.position + Vector3.up * 1, Quaternion.identity);
    }

    /// <summary>
    /// Makes the character stop playing voice blips.
    /// </summary>
    private void ShutUp()
    {
        isSpeaking = false;
        if (currentVoiceBlipCoroutine != null)
        {
            StopCoroutine(currentVoiceBlipCoroutine);
            currentVoiceBlipCoroutine = null;
        }
    }

    /// <summary>
    /// Sets the expression/sprite for this character.
    /// </summary>
    /// <param name="expressionName">The expression name</param>
    public void SetExpression(string expressionName)
    {
        if (CharacterData == null)
        {
            Debug.LogWarning($"No character data found for '{gameObject.name}'");
            return;
        }

        // Get the expression data
        CharacterExpression expression = CharacterData.GetExpression(expressionName);
        if (expression == null)
        {
            Debug.LogWarning($"No expression data found for '{expressionName}' on character '{gameObject.name}'");
            return;
        }

        // Set the sprite if the character has a SpriteRenderer
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null && expression.ExpressionSprite != null)
        {
            spriteRenderer.sprite = expression.ExpressionSprite;
        }

        // Apply default scale from character data
        transform.localScale = CharacterData.defaultScale;

        // Store the last set expression for voice blip usage
        lastSetExpression = expression;
    }

    /// <summary>
    /// Coroutine that plays voice blips repeatedly
    /// </summary>
    /// <param name="expression">The character expression containing voice blip data</param>
    private IEnumerator PlayVoiceBlips(CharacterExpression expression)
    {
        AudioSource audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        while (isSpeaking)
        {
            if (expression.VoiceBlip != null)
            {
                // Stop any currently playing voice blip to prevent volume accumulation
                audioSource.Stop();
                audioSource.clip = expression.VoiceBlip;
                audioSource.volume = expression.BlipVolume;
                audioSource.Play();
            }

            yield return new WaitForSeconds(expression.BlipInterval);
        }
    }

    /// <summary>
    /// Gets the character name.
    /// </summary>
    /// <returns>The character name</returns>
    public string GetCharacterName()
    {
        return gameObject.name;
    }

    /// <summary>
    /// Gets the character data.
    /// </summary>
    /// <returns>The character data</returns>
    public CharacterData GetCharacterData()
    {
        return CharacterData;
    }

    private void OnTypewriterFinished()
    {
        // Temporarily stop voice blips when the typewriter animation is complete
        ShutUp();
    }

    private void OnLineStarted()
    {
        if (isVoiceBlipOn)
        {
            StartVoiceBlip();
        }
    }

}
