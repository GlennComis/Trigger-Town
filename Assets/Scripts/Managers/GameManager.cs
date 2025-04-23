using UnityEngine;

public class GameManager : SingletonMonoBehaviour<GameManager>
{
    [SerializeField] private SaveManager saveManager;
    [SerializeField] private CurrencyManager currencyManager;

    protected override void Awake()
    {
        saveManager.Load();
        base.Awake();
        DontDestroyOnLoadManager.MarkDontDestroy(this.gameObject);
    }

    private void OnEnable()
    {
        currencyManager.OnGoldChanged += HandleOnGoldChanged;
    }

    private void OnDisable()
    {
        currencyManager.OnGoldChanged -= HandleOnGoldChanged;
    }
    
    private void HandleOnGoldChanged(int obj)
    {
        Save();
    }

    public void Save() => saveManager.Save();
    public void Load() => saveManager.Load();
}