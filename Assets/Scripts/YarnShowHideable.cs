using UnityEngine;
using Yarn.Unity;

/// <summary>
/// Component that allows any GameObject to be shown or hidden via Yarn commands.
/// Attach this to any GameObject you want to control with <<show>> and <<hide>> commands.
/// </summary>
public class YarnShowHideable : MonoBehaviour
{

    [Header("Should Start Visible?")]
    [SerializeField] private bool _startVisible = true;

    private void Awake()
    {
        if (_startVisible)
        {
            ShowObject();
        }
        else
        {
            HideObject();
        }
    }

    /// <summary>
    /// Shows this GameObject.
    /// Usage in Yarn: <<show ObjectName>>
    /// </summary>
    [YarnCommand("show")]
    public void ShowObject()
    {
        GetComponent<SpriteRenderer>().enabled = true;
        TryGetComponent<Collider2D>(out var collider);
        if (collider != null)
        {
            collider.enabled = true;
        }
        Debug.Log($"Showed object: {gameObject.name}");
        
        // Also show any children under this GameObject
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// Hides this GameObject.
    /// Usage in Yarn: <<hide ObjectName>>
    /// </summary>
    [YarnCommand("hide")]
    public void HideObject()
    {
        GetComponent<SpriteRenderer>().enabled = false;
        TryGetComponent<Collider2D>(out var collider);
        if (collider != null)
        {
            collider.enabled = false;
        }
        Debug.Log($"Hid object: {gameObject.name}");

        // Also hide any children under this GameObject
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(false);
        }
    }

}
