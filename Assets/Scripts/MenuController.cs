using UnityEngine;

public class MenuController : Singleton<MenuController>
{
    [SerializeField] private MenuState initialMenuState = MenuState.Default;
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
}
