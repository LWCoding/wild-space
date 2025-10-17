using UnityEngine;
using System.Collections.Generic;
using Yarn.Unity;

/// <summary>
/// Manager that handles character spawning and basic visibility management.
/// </summary>
public class CharacterManager : MonoBehaviour
{
    [Header("Character System")]
    [SerializeField] private Transform characterContainer;
    [SerializeField] private GameObject _characterPrefab; // Default character prefab

    // Character anchor positions
    [SerializeField] private Transform leftAnchor;
    [SerializeField] private Transform centerAnchor;
    [SerializeField] private Transform rightAnchor;
    [SerializeField] private Transform leftFarAnchor;
    [SerializeField] private Transform rightFarAnchor;

    // Track spawned characters
    private static Dictionary<string, GameObject> spawnedCharacters = new Dictionary<string, GameObject>();

    // Must be on start since singletons initialize on awake.
    void Start()
    {
        SpawnAllCharacters();
    }

    /// <summary>
    /// Spawns all registered characters at startup and hides them by default
    /// </summary>
    private void SpawnAllCharacters()
    {
        if (!CharacterDatabase.IsInitialized())
        {
            Debug.LogError("Character database singleton is not initialized!");
            return;
        }

        string[] characterNames = CharacterDatabase.Instance.GetCharacterNames();
        Debug.Log($"Spawning {characterNames.Length} characters at startup...");

        foreach (string characterName in characterNames)
        {
            CharacterData characterData = CharacterDatabase.Instance.GetCharacter(characterName);
            if (characterData != null)
            {
                SpawnCharacter(characterName, characterData);
            }
        }

        Debug.Log($"Spawned {spawnedCharacters.Count} characters. All hidden by default.");
    }

    /// <summary>
    /// Spawns a single character and hides it by default
    /// </summary>
    /// <param name="characterName">Name of the character</param>
    /// <param name="characterData">Character data</param>
    private void SpawnCharacter(string characterName, CharacterData characterData)
    {
        // Use the character's specific prefab or the default prefab
        GameObject prefabToUse = characterData.characterPrefab != null ? characterData.characterPrefab : _characterPrefab;

        if (prefabToUse == null)
        {
            Debug.LogError($"No character prefab assigned for character '{characterName}' and no default prefab set!");
            return;
        }

        // Spawn at center position initially (will be repositioned when shown)
        Transform spawnPosition = centerAnchor != null ? centerAnchor : transform;
        GameObject characterInstance = Instantiate(prefabToUse, spawnPosition.position, spawnPosition.rotation, characterContainer);
        characterInstance.name = characterName;

        // Hide the character by default
        SpriteRenderer spriteRenderer = characterInstance.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = false;
        }

        // Set default expression and scale
        SetCharacterExpression(characterInstance, characterData, characterData.defaultExpression?.expressionName);

        // Track the character
        spawnedCharacters[characterName] = characterInstance;

        Debug.Log($"Spawned character '{characterName}' (hidden by default)");
    }

    // Function for hiding all characters. Should be called by yarn command.
    public static void HideAllCharacters()
    {
        foreach (var character in spawnedCharacters.Values)
        {
            if (character != null)
            {
                SpriteRenderer spriteRenderer = character.GetComponent<SpriteRenderer>();
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
    /// Sets the expression/sprite for a character instance using character data.
    /// </summary>
    /// <param name="characterInstance">The character GameObject</param>
    /// <param name="characterData">The character's data containing expressions</param>
    /// <param name="expressionName">The expression name</param>
    private void SetCharacterExpression(GameObject characterInstance, CharacterData characterData, string expressionName)
    {
        // Get the expression data
        CharacterExpression expression = characterData.GetExpression(expressionName);
        if (expression == null)
        {
            Debug.LogWarning($"No expression data found for '{expressionName}' on character '{characterData.characterName}'");
            return;
        }

        // Set the sprite if the character has a SpriteRenderer
        SpriteRenderer spriteRenderer = characterInstance.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null && expression.expressionSprite != null)
        {
            spriteRenderer.sprite = expression.expressionSprite;
        }

        // Apply default scale from character data
        characterInstance.transform.localScale = characterData.defaultScale;
    }


    /// <summary>
    /// Gets a character instance by name
    /// </summary>
    /// <param name="characterName">Name of the character</param>
    /// <returns>The character GameObject if found, otherwise null</returns>
    public GameObject GetCharacter(string characterName)
    {
        return spawnedCharacters.TryGetValue(characterName, out GameObject character) ? character : null;
    }

}
