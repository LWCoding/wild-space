using UnityEngine;
using System.Collections.Generic;
using Yarn.Unity;

/// <summary>
/// Data structure for character information including character GameObject, UI, and yarn variable names
/// </summary>
[System.Serializable]
public class CharacterInfo
{
    [Header("Character Assignment")]
    public GameObject characterObject;
    
    [Header("UI Icon Assignment")]
    public UICharacterIcon icon;
    
    [Header("Yarn Variable Names")]
    public string likeVariableName;  // e.g., "catLikesYou"
    public string dislikeVariableName;  // e.g., "catDislikesYou"
}

/// <summary>
/// Manager that handles character visibility and UI icon management.
/// Characters are pre-made in the scene, not spawned at runtime.
/// </summary>
public class CharacterManager : MonoBehaviour
{
    [Header("Character System")]
    [SerializeField] private List<CharacterInfo> characterInfos = new List<CharacterInfo>();

    // Character anchor positions
    [SerializeField] private Transform leftAnchor;
    [SerializeField] private Transform centerAnchor;
    [SerializeField] private Transform rightAnchor;
    [SerializeField] private Transform leftFarAnchor;
    [SerializeField] private Transform rightFarAnchor;

    private DialogueRunner dialogueRunner;

    void Start()
    {
        dialogueRunner = FindObjectOfType<DialogueRunner>();
        
        // Hide all characters and their UI icons by default
        InitializeCharacters();
    }

    /// <summary>
    /// Initializes all characters and their UI icons to be hidden by default
    /// </summary>
    private void InitializeCharacters()
    {
        foreach (var charInfo in characterInfos)
        {
            if (charInfo?.characterObject != null)
            {
                // Hide the character sprite
                SpriteRenderer spriteRenderer = charInfo.characterObject.GetComponent<SpriteRenderer>();
                if (spriteRenderer != null)
                {
                    spriteRenderer.enabled = false;
                }
            }

            if (charInfo?.icon != null)
            {
                // Hide the UI icon
                charInfo.icon.Hide();
            }
        }
    }

    /// <summary>
    /// Shows a character by enabling their sprite and UI icon
    /// Called from Character.cs ShowCharacter method
    /// </summary>
    /// <param name="characterName">Name of the character to show</param>
    /// <param name="expression">Expression name to check for "Obscure"</param>
    public void ShowCharacter(string characterName, string expression)
    {
        CharacterInfo charInfo = GetCharacterInfo(characterName);
        if (charInfo == null) return;

        // Show the character's sprite
        if (charInfo.characterObject != null)
        {
            SpriteRenderer spriteRenderer = charInfo.characterObject.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = true;
            }
        }

        // Show and update the UI icon ONLY if the expression doesn't contain "Obscure"
        if (charInfo.icon != null && !expression.Contains("Obscure"))
        {
            // Get like and dislike counts from yarn variables
            int likes = GetYarnVariable(charInfo.likeVariableName);
            int dislikes = GetYarnVariable(charInfo.dislikeVariableName);
            
            // Show the icon with like/dislike counts
            charInfo.icon.Show(likes, dislikes);
        }
    }

    /// <summary>
    /// Hides a character by disabling their sprite and UI icon
    /// Called from Character.cs HideCharacter method
    /// </summary>
    /// <param name="characterName">Name of the character to hide</param>
    public void HideCharacter(string characterName)
    {
        CharacterInfo charInfo = GetCharacterInfo(characterName);
        if (charInfo == null) return;

        // Hide the character's sprite
        if (charInfo.characterObject != null)
        {
            SpriteRenderer spriteRenderer = charInfo.characterObject.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = false;
            }
        }

        // Hide the UI icon
        if (charInfo.icon != null)
        {
            charInfo.icon.Hide();
        }
    }

    /// <summary>
    /// Gets a Yarn variable value as an integer
    /// </summary>
    /// <param name="variableName">Name of the variable (without $ prefix)</param>
    /// <returns>Variable value as integer, 0 if not found</returns>
    private int GetYarnVariable(string variableName)
    {
        if (dialogueRunner?.VariableStorage == null || string.IsNullOrEmpty(variableName))
        {
            return 0;
        }

        // Get as float (Yarn stores numbers as floats)
        if (dialogueRunner.VariableStorage.TryGetValue<float>($"${variableName}", out float floatValue))
        {
            return Mathf.RoundToInt(floatValue);
        }
        else
        {
            // Variable doesn't exist yet, return 0
            return 0;
        }
    }

    /// <summary>
    /// Gets the CharacterInfo for a character by name
    /// </summary>
    /// <param name="characterName">Name of the character</param>
    /// <returns>The CharacterInfo if found, otherwise null</returns>
    public CharacterInfo GetCharacterInfo(string characterName)
    {
        foreach (var charInfo in characterInfos)
        {
            if (charInfo?.characterObject != null && charInfo.characterObject.name == characterName)
            {
                return charInfo;
            }
        }
        Debug.LogWarning($"CharacterInfo not found for character '{characterName}'");
        return null;
    }

    // Function for hiding all characters. Should be called by yarn command.
    public static void HideAllCharacters()
    {
        CharacterManager instance = FindObjectOfType<CharacterManager>();
        if (instance == null)
        {
            Debug.LogError("CharacterManager instance not found in scene!");
            return;
        }

        foreach (var charInfo in instance.characterInfos)
        {
            if (charInfo?.characterObject != null)
            {
                SpriteRenderer spriteRenderer = charInfo.characterObject.GetComponent<SpriteRenderer>();
                if (spriteRenderer != null)
                {
                    spriteRenderer.enabled = false;
                }
            }

            if (charInfo?.icon != null)
            {
                charInfo.icon.Hide();
            }
        }
        Debug.Log("Hid all characters");
    }

    /// <summary>
    /// Gets the transform for the specified anchor position.
    /// </summary>
    /// <param name="position">Position name</param>
    /// <returns>Transform of the anchor position, or null if invalid</returns>
    public Transform GetAnchorPosition(string position)
    {
        switch (position.ToLower())
        {
            case "left":
                return leftAnchor;
            case "center":
                return centerAnchor;
            case "right":
                return rightAnchor;
            case "leftfar":
                return leftFarAnchor;
            case "rightfar":
                return rightFarAnchor;
            default:
                return null;
        }
    }

    /// <summary>
    /// Gets a character GameObject by name
    /// </summary>
    /// <param name="characterName">Name of the character</param>
    /// <returns>The character GameObject if found, otherwise null</returns>
    public GameObject GetCharacter(string characterName)
    {
        CharacterInfo charInfo = GetCharacterInfo(characterName);
        return charInfo?.characterObject;
    }
}
