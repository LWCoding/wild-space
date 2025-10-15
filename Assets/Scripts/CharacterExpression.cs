using UnityEngine;

[CreateAssetMenu(fileName = "New Character Expression", menuName = "Dialogue/Character Expression")]
public class CharacterExpression : ScriptableObject
{
    [Header("Expression Info")]
    public string expressionName;
    public Sprite expressionSprite;

    [Header("Voice Blip")]
    public AudioClip voiceBlip;
    public float blipInterval = 0.1f;
}
