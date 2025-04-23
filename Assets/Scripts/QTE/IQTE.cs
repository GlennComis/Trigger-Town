public interface IQTE
{
    event System.Action<QTEResult> OnQTEComplete;
    event System.Action OnQTEStarted;

    void InitializeVisualState();
    void PlayQTEIntro();
    void StartQTE();
    void PauseQTE();
    void ResumeQTE();
}