using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using Random = UnityEngine.Random;

public abstract class CharacterController : MonoBehaviour
{
    #region Animator

    [Header("Animator")]
    [SerializeField] private Animator animator;
    private static readonly int ShootStringHash = Animator.StringToHash("Shoot");

    #endregion

    #region Health
    private int maxHealth;

    [Tooltip("Slider UI element representing health bar.")]
    [SerializeField] private Slider healthSlider;

    [Tooltip("How long the health bar animation lasts when damaged.")]
    [SerializeField] private float healthBarAnimDuration = 0.5f;

    protected int currentHealth;

    #endregion

    #region Sound

    [Header("Sound")]
    [SerializeField] private AudioSource audioSource;

    [Tooltip("Gunshot sound clips to randomly pick from.")]
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
    private readonly WaitForSeconds FLASH_INTERVAL = new WaitForSeconds(0.1f);

    #endregion

    #region Unity Lifecycle

    protected virtual void Awake()
    {
        flashMaterial = spriteRenderer.material;
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
        nameLabel.text = name;
    }

    private void InitHealthBar()
    {
        currentHealth = PlayerManager.Instance.currentHealth;

        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = maxHealth;
        }
    }

    public void TakeDamage()
    {
        if (hitRoutine != null)
            StopCoroutine(hitRoutine);
        currentHealth -= 100; //todo: change this to weapon damage + modifiers in the near future
        hitRoutine = StartCoroutine(HitRoutine());
        
    }

    private IEnumerator HitRoutine()
    {
        yield return new WaitForSeconds(0.5f);
        flashMaterial.SetFloat(FlashAmountShaderProperty, 1);
        yield return FLASH_INTERVAL;
        flashMaterial.SetFloat(FlashAmountShaderProperty, 0);
        yield return FLASH_INTERVAL;
        flashMaterial.SetFloat(FlashAmountShaderProperty, 1);
        yield return FLASH_INTERVAL;
        flashMaterial.SetFloat(FlashAmountShaderProperty, 0);

        if (healthSlider != null)
        {
            healthSlider.DOValue(currentHealth, healthBarAnimDuration).SetEase(Ease.OutCubic);
        }

        yield return new WaitForSeconds(healthBarAnimDuration);
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        Debug.Log("Character has died");
    }

    #endregion

    #region Combat

    public void Shoot()
    {
        if (animator != null)
            animator.SetTrigger(ShootStringHash);

        PlayGunShotClip();
    }

    private void PlayGunShotClip()
    {
        if (gunshotClips == null || gunshotClips.Count == 0 || audioSource == null)
        {
            Debug.LogWarning("AudioManager: No clips or AudioSource assigned!");
            return;
        }

        AudioClip randomClip = gunshotClips[Random.Range(0, gunshotClips.Count)];
        audioSource.pitch = Random.Range(pitchRange.x, pitchRange.y);

        audioSource.clip = randomClip;
        audioSource.PlayDelayed(GUN_SHOT_DELAY);
    }

    #endregion
}
