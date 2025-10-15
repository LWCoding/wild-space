using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// MonoBehaviour Singleton that automatically discovers and manages all CharacterData ScriptableObjects.
/// Place this component on a GameObject in your scene.
/// </summary>
public class CharacterDatabase : MonoBehaviour
{
    
    private List<CharacterData> characters = new List<CharacterData>();
    private Dictionary<string, CharacterData> characterLookup;
    
    // Singleton instance
    private static CharacterDatabase _instance;
    
    /// <summary>
    /// Singleton instance of the CharacterDatabase.
    /// </summary>
    public static CharacterDatabase Instance
    {
        get
        {
            if (_instance == null)
            {
                // Try to find existing instance in scene
                _instance = FindObjectOfType<CharacterDatabase>();
                
                if (_instance == null)
                {
                    Debug.LogError("CharacterDatabase instance not found! Make sure there's a CharacterDatabase component in the scene.");
                }
            }
            return _instance;
        }
    }
    
    /// <summary>
    /// Automatically discovers all CharacterData ScriptableObjects and builds the lookup table
    /// </summary>
    void Awake()
    {
        // Ensure only one instance exists
        if (_instance != null && _instance != this)
        {
            Debug.LogWarning("Multiple CharacterDatabase instances found! Destroying duplicate.");
            Destroy(gameObject);
            return;
        }
        
        _instance = this;
        
        // Don't destroy this object when loading new scenes
        DontDestroyOnLoad(gameObject);
        
        // Automatically discover all CharacterData ScriptableObjects
        DiscoverCharacterData();
        
        // Build the lookup table
        BuildLookupTable();
        
        Debug.Log($"CharacterDatabase initialized with {characters.Count} characters");
    }
    
    /// <summary>
    /// Automatically discovers all CharacterData ScriptableObjects in the project
    /// </summary>
    private void DiscoverCharacterData()
    {
        // Clear existing characters list
        characters.Clear();
        
        // Find all CharacterData ScriptableObjects in the project
        CharacterData[] foundCharacters = Resources.FindObjectsOfTypeAll<CharacterData>();
        
        // Filter out duplicates and add to our list
        HashSet<CharacterData> uniqueCharacters = new HashSet<CharacterData>();
        
        foreach (var character in foundCharacters)
        {
            if (character != null && !string.IsNullOrEmpty(character.characterName))
            {
                uniqueCharacters.Add(character);
            }
        }
        
        // Add unique characters to our list
        characters.AddRange(uniqueCharacters);
        
        Debug.Log($"Discovered {characters.Count} CharacterData ScriptableObjects");
    }
    
    /// <summary>
    /// Builds the lookup table for fast character access
    /// </summary>
    private void BuildLookupTable()
    {
        characterLookup = new Dictionary<string, CharacterData>();
        
        foreach (var character in characters)
        {
            if (character != null && !string.IsNullOrEmpty(character.characterName))
            {
                string key = character.characterName.ToLower();
                if (!characterLookup.ContainsKey(key))
                {
                    characterLookup[key] = character;
                }
                else
                {
                    Debug.LogWarning($"Duplicate character name found: {character.characterName}");
                }
            }
        }
    }
    
    /// <summary>
    /// Gets a character by name (case insensitive)
    /// </summary>
    /// <param name="characterName">Name of the character to find</param>
    /// <returns>The character data if found, otherwise null</returns>
    public CharacterData GetCharacter(string characterName)
    {
        if (string.IsNullOrEmpty(characterName))
            return null;
            
        if (characterLookup == null)
            BuildLookupTable();
            
        string key = characterName.ToLower();
        return characterLookup.TryGetValue(key, out CharacterData character) ? character : null;
    }
    
    /// <summary>
    /// Checks if a character exists in the database
    /// </summary>
    /// <param name="characterName">Name of the character to check</param>
    /// <returns>True if the character exists</returns>
    public bool HasCharacter(string characterName)
    {
        if (string.IsNullOrEmpty(characterName))
            return false;
            
        if (characterLookup == null)
            BuildLookupTable();
            
        return characterLookup.ContainsKey(characterName.ToLower());
    }
    
    /// <summary>
    /// Gets all character names in the database
    /// </summary>
    /// <returns>Array of character names</returns>
    public string[] GetCharacterNames()
    {
        if (characterLookup == null)
            BuildLookupTable();
            
        List<string> names = new List<string>();
        foreach (var character in characters)
        {
            if (character != null && !string.IsNullOrEmpty(character.characterName))
            {
                names.Add(character.characterName);
            }
        }
        return names.ToArray();
    }
    
    /// <summary>
    /// Checks if the singleton instance is initialized
    /// </summary>
    /// <returns>True if the singleton is initialized</returns>
    public static bool IsInitialized()
    {
        return _instance != null;
    }
    
    /// <summary>
    /// Manually refresh the character database (useful when new characters are added at runtime)
    /// </summary>
    public void RefreshDatabase()
    {
        DiscoverCharacterData();
        BuildLookupTable();
        Debug.Log($"CharacterDatabase refreshed with {characters.Count} characters");
    }
    
    /// <summary>
    /// Gets the list of all discovered characters (read-only)
    /// </summary>
    /// <returns>Read-only list of characters</returns>
    public IReadOnlyList<CharacterData> GetAllCharacters()
    {
        return characters.AsReadOnly();
    }
}
