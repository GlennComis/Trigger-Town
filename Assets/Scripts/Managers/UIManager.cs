using UnityEngine;

public class UIManager : SingletonMonoBehaviour<UIManager>
 {
     public NewsController newsController;
     public FadeTransitionController fadeTransitionController;
     public GameObject glyphs;
 
     protected override void Awake()
     {
         base.Awake();
         DontDestroyOnLoadManager.MarkDontDestroy(this.gameObject);
     }
 }