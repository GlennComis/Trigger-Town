using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class ChamberUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private RectTransform labelCanvas;   // The text container that should NOT rotate
    [SerializeField] private TMP_Text keyText;
    [SerializeField] private Image bulletIcon;

    [Header("Visual Settings")]
    [SerializeField] private Color defaultColor = Color.white;
    [SerializeField] private Color correctColor = Color.green;

    private Vector3 originalScale;

    private void Awake()
    {
        originalScale = transform.localScale;
    }
    
    private void LateUpdate()
    {
        if (labelCanvas != null)
        {
            // Counter-rotate to stay upright against parent rotation
            labelCanvas.rotation = Quaternion.identity;
        }
    }


    public void SetKey(KeyCode key, bool isBullet)
    {
        keyText.text = key.ToString();
        bulletIcon.enabled = isBullet;
        keyText.color = defaultColor;
        keyText.alpha = 1f;
    }

    public void MarkCorrectAndFade()
    {
        keyText.color = correctColor;
        keyText.DOFade(0f, 0.25f);
    }

    public void SetHighlight(bool active)
    {
        transform.DOScale(active ? originalScale * 1.15f : originalScale, 0.15f)
            .SetEase(Ease.OutBack);
    }

    public void PlayFlash()
    {
        transform.DOPunchScale(Vector3.one * 0.1f, 0.2f, 6, 1.0f);
    }
    
}