public class UIManager : SingletonMonoBehaviour<UIManager>
 {
     public NewsController newsController;
     public FadeTransitionController fadeTransitionController;
 
     protected override void Awake()
     {
         base.Awake();
         DontDestroyOnLoadManager.MarkDontDestroy(this.gameObject);
     }
 }