using TMPro;
using UnityEngine;

public class UIManager : SingletonMonoBehaviour<UIManager>
 {
     [Header("Controllers")]
     public FadeTransitionController fadeTransitionController;

     [Header("Character Information")]
     [SerializeField] private TextMeshProUGUI healthLabel;
     [SerializeField] private TextMeshProUGUI currencyLabel;
     [SerializeField] private TextMeshProUGUI rankLabel;
     
     [Header("Help Elements")]
     [SerializeField]
     private GameObject glyphs;
 
     protected override void Awake()
     {
         base.Awake();
         DontDestroyOnLoadManager.MarkDontDestroy(this.gameObject);
     }

     public void SetGlyphActive(bool active = true)
     {
         glyphs.gameObject.SetActive(active);
     }

     public void SetHealth()
     {
         //healthLabel.text = PlayerManager.Instance.currentHealth + " / " + PlayerManager.Instance.maxHealth;
     }
     
     public void SetCurrency()
     {
         currencyLabel.text = CurrencyManager.Instance.CurrentAmountOfGold.ToString();
     }
 }