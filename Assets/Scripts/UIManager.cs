using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIManager : SingletonMonoBehaviour<UIManager>
{
    [Header("HUD")]
    [SerializeField]
    private GameObject drawContainer;
    
    [Header("Win screen")]
    [SerializeField]
    private GameObject winnerContainer;
    [SerializeField]
    private GameObject rewardRowPrefab;
    private List<GameObject> rewardRows = new List<GameObject>();
    [SerializeField]
    private Transform rewardRowParentTransform;

    [SerializeField]
    private TextMeshProUGUI totalAmountLabel;

    [Header("Defeat screen")]
    [SerializeField]
    private GameObject defeatContainer;

    public void SetDrawText(bool state)
    {
        drawContainer.SetActive(state);
    }

    public void SetEndOfFightScreen(bool hasWon)
    {
        defeatContainer.SetActive(!hasWon);
    }

    public void SetWinScreen(List<Reward> rewards)
    {
        winnerContainer.SetActive(true);
        var totalScore = 0;
        foreach (var reward in rewards)
        {
            var rewardRow = Instantiate(rewardRowPrefab, rewardRowParentTransform).GetComponent<RewardRow>();
            rewardRow.SeedRewardRow(reward.type, reward.amount);
            totalScore += reward.amount;
            rewardRows.Add(rewardRow.gameObject);
        }

        totalAmountLabel.text = totalScore.ToString();
    }

    public void SetDefeatScreen()
    {
        defeatContainer.SetActive(true);
    }
}