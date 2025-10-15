using UnityEngine;
using System.Collections;

/// <summary>
/// Script for animating positive indicator sprites (like hearts) to move upward and fade out.
/// Attach this script to the PositiveIndicatorPrefab GameObject.
/// </summary>
public class PositiveIndicatorAnimation : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private float moveDistance = 2f; // How far up the sprite moves
    [SerializeField] private float animationDuration = 1.5f; // How long the animation takes
    [SerializeField] private AnimationCurve moveCurve = AnimationCurve.EaseInOut(0, 0, 1, 1); // Movement curve
    [SerializeField] private AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 1, 1, 0); // Fade curve
    
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
    /// Coroutine that handles the upward movement and fade-out animation
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
            Vector3 newPosition = startPosition + Vector3.up * (moveDistance * moveProgress);
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
        transform.position = startPosition + Vector3.up * moveDistance;
        Color finalColor = originalColor;
        finalColor.a = 0f;
        spriteRenderer.color = finalColor;
        
        // Destroy the GameObject after animation completes
        Destroy(gameObject);
    }
    
    /// <summary>
    /// Public method to customize animation parameters at runtime
    /// </summary>
    /// <param name="distance">How far up to move</param>
    /// <param name="duration">How long the animation should take</param>
    public void SetAnimationParameters(float distance, float duration)
    {
        moveDistance = distance;
        animationDuration = duration;
    }
}
