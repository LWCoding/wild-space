using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Character", menuName = "Dialogue/Character Data")]
public class CharacterData : ScriptableObject
{
    [Header("Character Info")]
    public string characterName;
    public GameObject characterPrefab;
    
    [Header("Expressions")]
    public List<CharacterExpression> expressions = new List<CharacterExpression>();
    
    [Header("Default Settings")]
    public CharacterExpression defaultExpression;
    public Vector3 defaultScale = Vector3.one;
    
    /// <summary>
    /// Gets an expression by name (case insensitive)
    /// </summary>
    /// <param name="expressionName">Name of the expression to find</param>
    /// <returns>The expression if found, otherwise the default expression</returns>
    public CharacterExpression GetExpression(string expressionName)
    {
        if (string.IsNullOrEmpty(expressionName))
            return defaultExpression;
            
        foreach (var expression in expressions)
        {
            if (expression != null && expression.expressionName.ToLower() == expressionName.ToLower())
            {
                return expression;
            }
        }
        
        Debug.LogWarning($"Expression '{expressionName}' not found for character '{characterName}'. Using default expression.");
        return defaultExpression;
    }
    
    /// <summary>
    /// Checks if a character has a specific expression
    /// </summary>
    /// <param name="expressionName">Name of the expression to check</param>
    /// <returns>True if the expression exists</returns>
    public bool HasExpression(string expressionName)
    {
        if (string.IsNullOrEmpty(expressionName))
            return false;
            
        foreach (var expression in expressions)
        {
            if (expression != null && expression.expressionName.ToLower() == expressionName.ToLower())
            {
                return true;
            }
        }
        return false;
    }
    
    /// <summary>
    /// Gets all available expression names for this character
    /// </summary>
    /// <returns>Array of expression names</returns>
    public string[] GetExpressionNames()
    {
        List<string> names = new List<string>();
        foreach (var expression in expressions)
        {
            if (expression != null && !string.IsNullOrEmpty(expression.expressionName))
            {
                names.Add(expression.expressionName);
            }
        }
        return names.ToArray();
    }
}
