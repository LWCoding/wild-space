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
    private Coroutine currentVoiceBlipCoroutine;
    private bool isSpeaking = false;
    private CharacterManager characterManager;
    
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
        Debug.Log($"ShowCharacter called on '{gameObject.name}' with expression '{expression}' and position '{position}'");
        
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
        
        // Enable the character's SpriteRenderer
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
            Debug.Log($"Enabled SpriteRenderer for '{gameObject.name}', current sprite: {spriteRenderer.sprite?.name}");
        }
        
        Debug.Log($"Showed character '{gameObject.name}' with expression '{expression}' at position '{position}'");
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
        
        Debug.Log($"Hid character: {gameObject.name}");
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
        
        // Get the current expression (we'll use the default if no specific expression is set)
        CharacterExpression currentExpression = CharacterData.defaultExpression;
        if (currentExpression == null || currentExpression.voiceBlip == null)
        {
            Debug.LogWarning($"No voice blip found for character '{gameObject.name}'");
            return;
        }
        
        isSpeaking = true;
        currentVoiceBlipCoroutine = StartCoroutine(PlayVoiceBlips(currentExpression));
    }
    
    /// <summary>
    /// Stops the current voice blip for this character.
    /// Usage in Yarn: <<stop_voice_blip CharacterName>>
    /// </summary>
    [YarnCommand("stop_voice_blip")]
    public void StopVoiceBlip()
    {
        if (currentVoiceBlipCoroutine != null)
        {
            StopCoroutine(currentVoiceBlipCoroutine);
            currentVoiceBlipCoroutine = null;
        }
        isSpeaking = false;
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
        if (spriteRenderer != null && expression.expressionSprite != null)
        {
            spriteRenderer.sprite = expression.expressionSprite;
        }
        
        // Apply default scale from character data
        transform.localScale = CharacterData.defaultScale;
        
        Debug.Log($"Set expression '{expression.expressionName}' for character '{gameObject.name}'");
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
        
        audioSource.clip = expression.voiceBlip;
        audioSource.loop = false;
        
        while (isSpeaking)
        {
            if (audioSource.clip != null)
            {
                audioSource.Play();
            }
            
            yield return new WaitForSeconds(expression.blipInterval);
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
}
