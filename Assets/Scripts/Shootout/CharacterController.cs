using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public abstract class CharacterController : MonoBehaviour
{
    [Header("Animator")]
    [SerializeField]
    private Animator animator;
    private static readonly int ShootStringHash = Animator.StringToHash("Shoot");

    [Header("Health")]
    [SerializeField]
    private int health;
    [SerializeField]
    private Transform healthContainer;
    private List<GameObject> healthIndicators;
    private int lastActiveHealthIndex;

    [Header("Sound")]
    [SerializeField]
    private AudioSource audioSource;
    private const float GUN_SHOT_DELAY = .25f;
    [SerializeField]
    private List<AudioClip> gunshotClips;
    private Vector2 pitchRange = new Vector2(0.8f, 1.1f);
    
    [Header("Sprite")]
    [SerializeField]
    private SpriteRenderer spriteRenderer;
    private Material flashMaterial;
    private Coroutine flashCoroutine;
    private static readonly int FlashAmountShaderProperty = Shader.PropertyToID("_FlashAmount");
    private readonly WaitForSeconds FLASH_INTERVAL = new WaitForSeconds(0.1f);
    

    private void Awake()
    {
        flashMaterial = spriteRenderer.material;
    }

    private void Start()
    {
        Instantiate();
    }
    
    public void Instantiate()
    {
        healthIndicators = new List<GameObject>();
    
        for (int i = 0; i < health; i++)
        {
            GameObject indicator = GameObject.Instantiate(FastDrawManager.Instance.healthIndicatorPrefab, healthContainer);
            healthIndicators.Add(indicator);
        }

        lastActiveHealthIndex = healthIndicators.Count - 1;
    }
    
    public void Shoot()
    {
        if(animator != null)
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

    public void TakeDamage()
    {
        if (flashCoroutine != null)
            StopCoroutine(flashCoroutine);

        flashCoroutine = StartCoroutine(FlashEffect());
        
        health--;

        if (health <= 0)
        {
            Die();
        }
    }
    
    private IEnumerator FlashEffect()
    {
        yield return new WaitForSeconds(.5f);
        flashMaterial.SetFloat(FlashAmountShaderProperty, 1);
        yield return FLASH_INTERVAL;
        flashMaterial.SetFloat(FlashAmountShaderProperty, 0);
        yield return FLASH_INTERVAL;
        flashMaterial.SetFloat(FlashAmountShaderProperty, 1);
        yield return FLASH_INTERVAL;
        flashMaterial.SetFloat(FlashAmountShaderProperty, 0);
        
        if (lastActiveHealthIndex >= 0)
        {
            healthIndicators[lastActiveHealthIndex].SetActive(false);
            lastActiveHealthIndex--;
        }
    }

    protected virtual void Die()
    {
        Debug.Log("Character has died");
    }
}