using UnityEngine;

public class ProgressBarController : Singleton<ProgressBarController>
{
    [SerializeField] private LoadingMenu loadingMenu;

    public void UpdateProgressBar(float progress, string message = null)
    {
        loadingMenu.UpdateProgressBar(progress, message);
    }
}
