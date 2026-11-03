using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Data structure for character information including character GameObject
/// </summary>
[System.Serializable]
public class CharacterInfo
{
    [Header("Character Assignment")]
    public GameObject characterObject;
}

/// <summary>
/// Manager that handles character visibility.
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

    void Start()
    {
        // Hide all characters by default
        InitializeCharacters();
    }

    /// <summary>
    /// Initializes all characters to be hidden by default
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
        }
    }

    /// <summary>
    /// Shows a character by enabling their sprite
    /// Called from Character.cs ShowCharacter method
    /// </summary>
    /// <param name="characterName">Name of the character to show</param>
    /// <param name="expression">Expression name</param>
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
    }

    /// <summary>
    /// Hides a character by disabling their sprite
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
