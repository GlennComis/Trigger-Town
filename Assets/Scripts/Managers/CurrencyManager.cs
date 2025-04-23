using UnityEngine;
using System;

public class CurrencyManager : SingletonMonoBehaviour<CurrencyManager>, ISaveable
{
    private const int STARTING_GOLD = 0;
    private int gold;

    public event Action<int> OnGoldChanged;

    public string SaveKey => SaveKeys.Currency;

    protected override void Awake()
    {
        base.Awake();
        gold = STARTING_GOLD;
    }

    public int CurrentAmountOfGold => gold;

    public bool CanAfford(int amount) => gold >= amount;

    public void Add(int amount)
    {
        if (amount <= 0) return;
        gold += amount;
        OnGoldChanged?.Invoke(gold);
    }

    public bool Spend(int amount)
    {
        if (!CanAfford(amount)) return false;
        gold -= amount;
        OnGoldChanged?.Invoke(gold);
        return true;
    }

    public void Set(int newAmount)
    {
        gold = Mathf.Max(0, newAmount);
        OnGoldChanged?.Invoke(gold);
    }
    
    [Serializable]
    private class GoldSaveData
    {
        public int gold;
    }

    public object CaptureData()
    {
        return new GoldSaveData { gold = this.gold };
    }

    public void RestoreData(object data)
    {
        if (data is string json)
        {
            GoldSaveData saved = JsonUtility.FromJson<GoldSaveData>(json);
            gold = saved.gold;
            OnGoldChanged?.Invoke(gold);
        }
        else
        {
            Debug.LogError("Invalid save data for GoldManager.");
        }
    }
}