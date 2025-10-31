using UnityEngine;
using System.Collections;

/// <summary>
/// Generic indicator animation that moves and fades a sprite, then destroys it.
/// Can be configured to move up or down.
/// Attach this script to your indicator prefab (positive or negative).
/// </summary>
public class PositiveIndicatorAnimation : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private float moveDistance = 2f; // How far the sprite moves
    [SerializeField] private float animationDuration = 1f; // How long the animation takes
    [SerializeField] private AnimationCurve moveCurve = AnimationCurve.EaseInOut(0, 0, 1, 1); // Movement curve
    [SerializeField] private AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 1, 1, 0); // Fade curve

    public enum MoveDirection
    {
        Up,
        Down
    }

    [Header("Direction")]
    [SerializeField] private MoveDirection direction = MoveDirection.Up;
    
    private Vector3 startPosition;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    
    void Start()
    {
        // Get the sprite renderer component
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("PositiveIndicatorAnimation: No SpriteRenderer found on this GameObject!");
            return;
        }
        
        // Store the starting position and original color
        startPosition = transform.position;
        originalColor = spriteRenderer.color;
        
        // Start the animation
        StartCoroutine(AnimateIndicator());
    }
    
    /// <summary>
    /// Coroutine that handles movement and fade-out animation
    /// </summary>
    private IEnumerator AnimateIndicator()
    {
        float elapsedTime = 0f;
        
        while (elapsedTime < animationDuration)
        {
            // Calculate the progress (0 to 1)
            float progress = elapsedTime / animationDuration;
            
            // Apply movement curve and calculate new position
            float moveProgress = moveCurve.Evaluate(progress);
            Vector3 moveDir = GetMoveVector();
            Vector3 newPosition = startPosition + moveDir * (moveDistance * moveProgress);
            transform.position = newPosition;
            
            // Apply fade curve and update alpha
            float alphaProgress = fadeCurve.Evaluate(progress);
            Color currentColor = originalColor;
            currentColor.a = alphaProgress;
            spriteRenderer.color = currentColor;
            
            // Increment time
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // Ensure final values are set
        transform.position = startPosition + GetMoveVector() * moveDistance;
        Color finalColor = originalColor;
        finalColor.a = 0f;
        spriteRenderer.color = finalColor;
        
        // Destroy the GameObject after animation completes
        Destroy(gameObject);
    }
    
    /// <summary>
    /// Public method to customize animation parameters at runtime
    /// </summary>
    /// <param name="distance">How far to move</param>
    /// <param name="duration">How long the animation should take</param>
    public void SetAnimationParameters(float distance, float duration)
    {
        moveDistance = distance;
        animationDuration = duration;
    }

    private Vector3 GetMoveVector()
    {
        switch (direction)
        {
            case MoveDirection.Up:
                return Vector3.up;
            case MoveDirection.Down:
                return Vector3.down;
            default:
                return Vector3.up;
        }
    }
}
