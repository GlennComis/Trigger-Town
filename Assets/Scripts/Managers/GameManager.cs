using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

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
    
    public void LoadScene(int sceneIndex, float minTransitionDelay = 2f, FadeType fadeType = FadeType.Simple)
    {
        if (UIManager.Instance == null || UIManager.Instance.fadeTransitionController == null)
        {
            Debug.LogError("FadeTransitionController not set up!");
            return;
        }
        
        //todo: temp code, remove later
        if (sceneIndex != 0)
        {
            UIManager.Instance.newsController.gameObject.SetActive(false);
            UIManager.Instance.glyphs.SetActive(false);
        }

        StartCoroutine(TransitionSequence(sceneIndex, minTransitionDelay, fadeType));
    }

    private IEnumerator TransitionSequence(int sceneIndex, float minTransitionDelay, FadeType fadeType)
    {
        bool fadeOutDone = false;
        UIManager.Instance.fadeTransitionController.FadeOut(fadeType, () => fadeOutDone = true);

        yield return new WaitUntil(() => fadeOutDone);

        AsyncOperation loadOp = SceneManager.LoadSceneAsync(sceneIndex);
        loadOp.allowSceneActivation = false;

        float timeElapsed = 0f;
        while (!loadOp.isDone)
        {
            timeElapsed += Time.deltaTime;

            if (loadOp.progress >= 0.9f && timeElapsed >= minTransitionDelay)
            {
                loadOp.allowSceneActivation = true;
            }

            yield return null;
        }

        bool fadeInDone = false;
        UIManager.Instance.fadeTransitionController.FadeIn(fadeType, () => fadeInDone = true);
        
        //todo: temp code, remove later
        if (sceneIndex == 0)
        {
            UIManager.Instance.glyphs.SetActive(true);
            UIManager.Instance.newsController.ShowNextHeadline();
            UIManager.Instance.newsController.gameObject.SetActive(true);
        }

        yield return new WaitUntil(() => fadeInDone);
    }
}