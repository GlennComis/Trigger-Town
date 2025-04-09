using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Building : MonoBehaviour, IEnterable
{
    [Header("Scene Loading")]
    public int sceneBuildIndex;

    [Header("Fade Overlay")]
    public Image fadeImage;
    public float fadeDuration = 1f;

    private bool isTransitioning = false;

    public void EnterBuilding()
    {
        if (isTransitioning)
            return;

        isTransitioning = true;
        Debug.Log($"Entering building: {gameObject.name}", gameObject);
        
        fadeImage.raycastTarget = true;
        fadeImage.DOFade(1f, fadeDuration).OnComplete(() =>
        {
            SceneManager.LoadScene(sceneBuildIndex);
        });
    }
}