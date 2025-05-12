using UnityEngine;
using UnityEngine.UI;

public class PlayerManager : SingletonMonoBehaviour<PlayerManager>
{
    [Header("Sub-systems")]
    [SerializeField] private PlayerXpController xpController;

    public int currentHealth = 100;
    public int maxHealth = 100;

    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoadManager.MarkDontDestroy(this.gameObject);
        
        if (xpController == null)
            xpController = GetComponent<PlayerXpController>();
    }

    private void Start()
    {
        xpController.Initialize();
    }

    public void AddXP(int amount)
    {
        xpController.AddXP(amount);
    }

    public void UpdateXpSlider(Slider slider, bool immediate = true)
    {
        xpController.BindXPSlider(slider, immediate);
    }

    public void ResetPlayerProgress()
    {
        xpController.ResetProgress();
    }
}