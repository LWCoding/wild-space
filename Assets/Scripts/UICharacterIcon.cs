using UnityEngine;
using TMPro;

/// <summary>
/// Component for character UI icons that display like/dislike counts.
/// </summary>
public class UICharacterIcon : MonoBehaviour
{
    [Header("UI Assignments")]
    [SerializeField] private TextMeshProUGUI textComponent;

    void Awake()
    {
        if (textComponent == null)
        {
            Debug.LogError($"TextMeshProUGUI component not assigned in {gameObject.name}!");
        }
    }

    /// <summary>
    /// Shows the UI icon and initializes it with like/dislike counts
    /// </summary>
    /// <param name="likes">Number of likes</param>
    /// <param name="dislikes">Number of dislikes</param>
    public void Show(int likes, int dislikes)
    {
        // Update the text
        UpdateText(likes, dislikes);

        // Show the icon by setting it active
        gameObject.SetActive(true);
    }

    /// <summary>
    /// Hides the UI icon
    /// </summary>
    public void Hide()
    {
        // Hide the icon by setting it inactive
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Updates the text to display like/dislike counts
    /// </summary>
    /// <param name="likes">Number of likes</param>
    /// <param name="dislikes">Number of dislikes</param>
    private void UpdateText(int likes, int dislikes)
    {
        if (textComponent != null)
        {
            textComponent.text = $"{likes}/{dislikes}";
        }
    }

    /// <summary>
    /// Updates just the text without showing/hiding the icon
    /// Useful for when the icon is already visible and counts change
    /// </summary>
    /// <param name="likes">Number of likes</param>
    /// <param name="dislikes">Number of dislikes</param>
    public void UpdateCounts(int likes, int dislikes)
    {
        UpdateText(likes, dislikes);
    }
}
