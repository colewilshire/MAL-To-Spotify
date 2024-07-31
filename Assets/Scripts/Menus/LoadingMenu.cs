using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadingMenu : Menu
{
    [SerializeField] private Slider progressBar;
    [SerializeField] private TMP_Text loadingMessage;

    protected override MenuState ActiveState {get; set;} = MenuState.Loading;

    public void UpdateProgressBar(float progress, string message = null)
    {
        progressBar.value = progress;

        if (message != null)
        {
            loadingMessage.text = message;
        }
    }
}
