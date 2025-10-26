using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

/// <summary>
/// Component for character UI icons that display like/dislike counts.
/// </summary>
public class UICharacterIcon : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI Assignments")]
    [SerializeField] private Image heartImage;
    [SerializeField] private CanvasGroup canvasGroup;
    
    [Header("Heart Sprites (0 = worst, 3 = neutral, 6 = best)")]
    [SerializeField] private Sprite heart0Sprite;  // Very negative ratio
    [SerializeField] private Sprite heart1Sprite;
    [SerializeField] private Sprite heart2Sprite;
    [SerializeField] private Sprite heart3Sprite;  // Default/neutral
    [SerializeField] private Sprite heart4Sprite;
    [SerializeField] private Sprite heart5Sprite;
    [SerializeField] private Sprite heart6Sprite;  // Very positive ratio
    
    private int currentLikes = 0;
    private int currentDislikes = 0;
    private bool hasBeenShown = false;
    private float storedAlpha = 0f;  // Stores the alpha value before hover
    
    void OnEnable()
    {
        // Recalculate and show the correct heart sprite when enabled
        UpdateHeartSprite(currentLikes, currentDislikes);
    }

    /// <summary>
    /// Shows the UI icon and initializes it with like/dislike counts
    /// </summary>
    /// <param name="likes">Number of likes</param>
    /// <param name="dislikes">Number of dislikes</param>
    public void Show(int likes, int dislikes)
    {
        // Store current counts
        currentLikes = likes;
        currentDislikes = dislikes;

        // Update the heart sprite based on ratio
        UpdateHeartSprite(likes, dislikes);

        // If this is the first time showing, set the object active
        if (!hasBeenShown)
        {
            hasBeenShown = true;
            gameObject.SetActive(true);
        }

        // Use CanvasGroup to show at full alpha
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            storedAlpha = 1f;
        }
    }

    /// <summary>
    /// Hides the UI icon
    /// </summary>
    public void Hide()
    {
        // Use CanvasGroup to hide (but keep object active)
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0.3f;
            storedAlpha = 0.3f;
        }
    }

    /// <summary>
    /// Deactivates the icon (can be re-activated later by calling Show)
    /// </summary>
    public void Deactivate()
    {
        // Hide it immediately
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }
        
        gameObject.SetActive(false);
        Debug.Log($"{gameObject.name} icon deactivated");
    }

    /// <summary>
    /// Updates the heart sprite based on like/dislike ratio
    /// Returns a value from 0-6 based on the ratio
    /// </summary>
    /// <param name="likes">Number of likes</param>
    /// <param name="dislikes">Number of dislikes</param>
    /// <returns>Heart level from 0-6</returns>
    private int CalculateHeartLevel(int likes, int dislikes)
    {
        int total = likes + dislikes;
        
        // If no interactions yet, show neutral (3 hearts)
        if (total == 0)
        {
            return 3;
        }

        // Calculate ratio as a percentage from -100% to +100%
        // -100% = all dislikes, +100% = all likes
        float ratio = ((float)likes - dislikes) / total;
        
        // Map ratio to 0-6 scale:
        // -100% to -50%: 0 (very negative)
        // -50% to -25%: 1 (negative)
        // -25% to 0%: 2 (slightly negative)
        // 0%: 3 (neutral)
        // 0% to +25%: 4 (slightly positive)
        // +25% to +50%: 5 (positive)
        // +50% to +100%: 6 (very positive)
        
        if (ratio <= -0.5f) return 0;
        if (ratio <= -0.25f) return 1;
        if (ratio < 0f) return 2;
        if (ratio == 0f) return 3;
        if (ratio <= 0.25f) return 4;
        if (ratio <= 0.5f) return 5;
        return 6;
    }

    /// <summary>
    /// Updates the heart sprite based on like/dislike counts
    /// </summary>
    /// <param name="likes">Number of likes</param>
    /// <param name="dislikes">Number of dislikes</param>
    private void UpdateHeartSprite(int likes, int dislikes)
    {
        if (heartImage == null) return;

        int heartLevel = CalculateHeartLevel(likes, dislikes);
        
        // Select sprite based on heart level
        Sprite spriteToUse = heartLevel switch
        {
            0 => heart0Sprite,
            1 => heart1Sprite,
            2 => heart2Sprite,
            3 => heart3Sprite,
            4 => heart4Sprite,
            5 => heart5Sprite,
            6 => heart6Sprite,
            _ => heart3Sprite  // Default to neutral (middle)
        };

        heartImage.sprite = spriteToUse;
    }

    /// <summary>
    /// Updates just the heart sprite without showing/hiding the icon
    /// Useful for when the icon is already visible and counts change
    /// </summary>
    /// <param name="likes">Number of likes</param>
    /// <param name="dislikes">Number of dislikes</param>
    public void UpdateCounts(int likes, int dislikes)
    {
        // Store current counts
        currentLikes = likes;
        currentDislikes = dislikes;
        
        UpdateHeartSprite(likes, dislikes);
    }

    /// <summary>
    /// Called when the pointer enters the icon
    /// </summary>
    /// <param name="eventData">Event data</param>
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (canvasGroup != null)
        {
            // Store the current alpha before changing it
            storedAlpha = canvasGroup.alpha;
            
            // Make it fully opaque on hover
            canvasGroup.alpha = 1f;
        }
    }

    /// <summary>
    /// Called when the pointer exits the icon
    /// </summary>
    /// <param name="eventData">Event data</param>
    public void OnPointerExit(PointerEventData eventData)
    {
        if (canvasGroup != null)
        {
            // Restore the previous alpha value
            canvasGroup.alpha = storedAlpha;
        }
    }
}
