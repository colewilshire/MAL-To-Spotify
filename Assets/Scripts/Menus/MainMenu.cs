using UnityEngine;
using UnityEngine.UI;

public class MainMenu : Menu
{
    protected override MenuState ActiveState {get; set;} = MenuState.Main;
    [SerializeField] private Button debugMenuButton;

    protected override void Start()
    {
        base.Start();

        debugMenuButton.onClick.AddListener(() => MenuController.Instance.SetMenu(MenuState.Debug));
    }
}
