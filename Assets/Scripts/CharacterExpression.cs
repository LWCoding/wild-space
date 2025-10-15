using UnityEngine;

[CreateAssetMenu(fileName = "New Character Expression", menuName = "Dialogue/Character Expression")]
public class CharacterExpression : ScriptableObject
{
    [Header("Expression Info")]
    public string expressionName;
    public Sprite expressionSprite;
    
    [Header("Voice Blip")]
    public AudioClip voiceBlip;
    [Range(0.1f, 2f)]
    public float blipInterval = 0.3f;
    
    [TextArea(2, 4)]
    public string description;
}
