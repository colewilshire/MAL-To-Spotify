using UnityEngine;

public abstract class Menu : MonoBehaviour
{
    protected abstract MenuState ActiveState {get; set;}

    protected virtual void Start()
    {
        MenuController.Instance.OnMenuStateChange += OnMenuStateChange;
    }

    protected virtual void OnDestroy()
    {
        MenuController.Instance.OnMenuStateChange -= OnMenuStateChange;
    }

    protected virtual void OnMenuStateChange(MenuState newState)
    {
        gameObject.SetActive(newState == ActiveState);
    }
}
