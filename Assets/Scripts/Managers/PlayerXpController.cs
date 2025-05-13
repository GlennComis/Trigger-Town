using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class PlayerXpController : MonoBehaviour, ISaveable
{
    [Header("XP Progression")]
    [SerializeField] private int baseXP = 100;
    [SerializeField] private float growthFactor = 1.5f;

    [Header("Animation")]
    [SerializeField] private float xpFillDuration = 1.5f;

    private int currentLevel = 1;
    private int currentXP = 0;
    private int xpToNextLevel;

    public int CurrentLevel => currentLevel;
    public int CurrentXP => currentXP;
    public int XPToNextLevel => xpToNextLevel;

    public Action<int> OnLevelUp;
    public Action<int> OnXPChanged;

    public void Initialize()
    {
        xpToNextLevel = CalculateXPForLevel(currentLevel);
    }

    public void AddXP(int amount)
    {
        Debug.Log("Adding xp");
        currentXP += amount;
        Debug.Log("Added xp, current xp is: " + currentXP);
        OnXPChanged?.Invoke(currentXP);

        while (currentXP >= xpToNextLevel)
        {
            currentXP -= xpToNextLevel;
            currentLevel++;
            OnLevelUp?.Invoke(currentLevel);
            xpToNextLevel = CalculateXPForLevel(currentLevel);
        }
    }

    public void ResetProgress()
    {
        currentLevel = 1;
        currentXP = 0;
        xpToNextLevel = CalculateXPForLevel(currentLevel);
        OnXPChanged?.Invoke(currentXP);
    }

    private int CalculateXPForLevel(int level)
    {
        return Mathf.FloorToInt(baseXP * Mathf.Pow(level, growthFactor));
    }

    public void BindXPSlider(Slider slider, bool immediate = true)
    {
        UpdateXPUI(slider, immediate);
    }

    private void UpdateXPUI(Slider slider, bool immediate)
    {
        Debug.Log("Updating xp: " + currentXP);
        float normalized = (float)currentXP / xpToNextLevel;
        if (immediate)
            slider.value = normalized;
        else
            slider.DOValue(normalized, xpFillDuration).SetEase(Ease.OutCubic);
    }

    #region Saving

    public string SaveKey => SaveKeys.PlayerXP;

    [Serializable]
    private class PlayerXPData
    {
        public int level;
        public int currentXP;
    }

    public object CaptureData()
    {
        return new PlayerXPData
        {
            level = currentLevel,
            currentXP = currentXP
        };
    }

    public void RestoreData(object data)
    {
        /*if (data is PlayerXPData saved)
        {
            currentLevel = saved.level;
            currentXP = saved.currentXP;
            xpToNextLevel = CalculateXPForLevel(currentLevel);
        }
        else
        {
            Debug.LogError("Invalid XP data");
        }*/
    }

    #endregion
}