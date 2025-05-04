using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

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
    
    [SerializeField]
    private TextMeshProUGUI countdownLabel;

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
    
    public void ShowCountdownNumber(int number)
    {
        countdownLabel.text = number.ToString();
        countdownLabel.gameObject.SetActive(true);
        countdownLabel.transform.localScale = Vector3.one * 2f;

        countdownLabel.DOKill();
        countdownLabel.transform.DOScale(1f, 0.4f)
            .SetEase(Ease.OutBack);

        countdownLabel.DOFade(1f, 0.05f);
    }
    
    

    public void HideCountdown()
    {
        countdownLabel.gameObject.SetActive(false);
    }


    public void SetDefeatScreen()
    {
        defeatContainer.SetActive(true);
    }

    public void LoadTown()
    {
        SceneManager.LoadScene(0);
    }
}