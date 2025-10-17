using UnityEngine;

[CreateAssetMenu(fileName = "New Character Expression", menuName = "Dialogue/Character Expression")]
public class CharacterExpression : ScriptableObject
{
    [Header("Expression Info")]
    public string ExpressionName;
    public Sprite ExpressionSprite;

    [Header("Voice Blip")]
    public AudioClip VoiceBlip;
    public float BlipInterval = 0.1f;
    public float BlipVolume = 1.0f;
}
