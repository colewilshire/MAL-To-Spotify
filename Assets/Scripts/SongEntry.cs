using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SongEntry : MonoBehaviour
{
    public Button Checkbox;
    public Image AlbumArt;
    public TMP_Text Title;
    public TMP_Text Artist;

    private void Awake()
    {
        Checkbox.onClick.AddListener(() => ToggleCheckbox());
    }

    private void ToggleCheckbox()
    {
        Image image = Checkbox.GetComponent<Image>();
        if (image.color.a > 0)
        {
            image.color = new(image.color.r, image.color.g, image.color.b, 0);
        }
        else
        {
            image.color = new(image.color.r, image.color.g, image.color.b, 1);
        }
    }
}
