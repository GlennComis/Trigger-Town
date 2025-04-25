using UnityEngine;

public class GameManager : SingletonMonoBehaviour<GameManager>
{
    [SerializeField] private SaveManager saveManager;
    [SerializeField] private CurrencyManager currencyManager;

    public int lastKnowBuildingIndex = 1;

    public bool hasCompletedTownTutorial;
    public bool hasCompletedSheriffTutorial;
    public bool hasCompletedShootoutTutorial;

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

    public void CompleteTownTutorial()
    {
        hasCompletedTownTutorial = true;
    }
}