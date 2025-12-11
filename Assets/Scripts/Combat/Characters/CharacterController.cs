using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using Random = UnityEngine.Random;

public abstract class CharacterController : MonoBehaviour, IHealth
{
    #region Animator

    [Header("Animator")]
    [SerializeField] private Animator animator;
    private static readonly int ShootStringHash = Animator.StringToHash("Shoot");

    #endregion

    #region Health

    [Header("Health")]
    [SerializeField] protected int maxHealth = 100;
    [SerializeField] private Slider healthSlider;
    [SerializeField] private float healthBarAnimDuration = 0.5f;

    protected int currentHealth;

    #endregion

    #region Sound

    [Header("Sound")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private List<AudioClip> gunshotClips;

    private const float GUN_SHOT_DELAY = 0.25f;
    private Vector2 pitchRange = new Vector2(0.8f, 1.1f);

    #endregion

    #region Visuals

    [Header("Sprite")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private TextMeshProUGUI nameLabel;

    private Material flashMaterial;
    private Coroutine hitRoutine;
    private static readonly int FlashAmountShaderProperty = Shader.PropertyToID("_FlashAmount");
    private readonly WaitForSeconds FlashInterval = new WaitForSeconds(0.1f);

    #endregion

    #region Unity Lifecycle

    protected virtual void Awake()
    {
        SetupCharacter(maxHealth, gameObject.name);
        if (spriteRenderer != null && spriteRenderer.material != null)
            flashMaterial = spriteRenderer.material;
        else
            Debug.LogWarning("SpriteRenderer or its material is missing.", this);
    }

    protected virtual void Start()
    {
        InitHealthBar();
    }

    #endregion

    #region Health System

    protected void SetupCharacter(int maximumHealth, string name)
    {
        maxHealth = maximumHealth;
        currentHealth = maxHealth;
        if (nameLabel != null)
            nameLabel.text = name;
    }

    private void InitHealthBar()
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = maxHealth;
        }
    }

    public virtual void TakeDamage(int amount)
    {
        if (hitRoutine != null)
            StopCoroutine(hitRoutine);

        currentHealth -= amount;
        currentHealth = Mathf.Max(0, currentHealth); // Clamp health to 0
        hitRoutine = StartCoroutine(HitRoutine());
    }

    private IEnumerator HitRoutine()
    {
        yield return new WaitForSeconds(0.5f);

        flashMaterial.SetFloat(FlashAmountShaderProperty, 1);
        yield return FlashInterval;
        flashMaterial.SetFloat(FlashAmountShaderProperty, 0);
        yield return FlashInterval;
        flashMaterial.SetFloat(FlashAmountShaderProperty, 1);
        yield return FlashInterval;
        flashMaterial.SetFloat(FlashAmountShaderProperty, 0);

        if (healthSlider != null)
        {
            healthSlider.DOValue(currentHealth, healthBarAnimDuration).SetEase(Ease.OutCubic);
        }
        else
        {
            Debug.LogError("Health slider is not assigned", this.gameObject);
        }

        yield return new WaitForSeconds(healthBarAnimDuration);

        if (currentHealth <= 0)
            Die();
    }

    protected virtual void Die()
    {
        Debug.Log($"{gameObject.name} has died.");
        Destroy(gameObject);
    }

    #endregion

    #region Combat

    public virtual void Shoot()
    {
        animator?.SetTrigger(ShootStringHash);
        PlayGunShotClip();
    }

    private void PlayGunShotClip()
    {
        if (gunshotClips == null || gunshotClips.Count == 0 || audioSource == null)
        {
            Debug.LogWarning("Missing audio clip or AudioSource.");
            return;
        }

        AudioClip randomClip = gunshotClips[Random.Range(0, gunshotClips.Count)];
        audioSource.pitch = Random.Range(pitchRange.x, pitchRange.y);
        audioSource.clip = randomClip;
        audioSource.PlayDelayed(GUN_SHOT_DELAY);
    }

    #endregion
}
