using TMPro;
using UnityEngine;

public class RewardRow : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI rewardTypeLabel;
    [SerializeField]
    private TextMeshProUGUI rewardAmountLabel;

    public void SeedRewardRow(RewardType rewardType, int amount)
    {
        rewardTypeLabel.text = FormatRewardType(rewardType);
        rewardAmountLabel.text = amount.ToString();
    }
    
    private static string FormatRewardType(RewardType rewardType)
    {
        string formatted = rewardType.ToString();
        for (int i = 1; i < formatted.Length; i++)
        {
            if (char.IsUpper(formatted[i])) 
            {
                formatted = formatted.Insert(i, " ");
                i++;
            }
        }
        return formatted;
    }
}