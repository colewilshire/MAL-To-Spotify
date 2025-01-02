using UnityEngine;

public class MenuController : Singleton<MenuController>
{
    [SerializeField] private MenuState initialMenuState = MenuState.Default;
    [SerializeField] private LoadingMenu loadingMenu;
    public delegate void OnMenuStateChangeHandler(MenuState menuState);
    public event OnMenuStateChangeHandler OnMenuStateChange;

    private void Start()
    {
        SetMenu(initialMenuState);
    }

    public void SetMenu(MenuState newState)
    {
        OnMenuStateChange.Invoke(newState);
    }

    public void UpdateProgressBar(float progress, string message = null)
    {
        loadingMenu.UpdateProgressBar(progress, message);
    }
}
