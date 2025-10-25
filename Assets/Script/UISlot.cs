using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UISlot : MonoBehaviour
{
    public Image iconImage;
    public TMP_Text countText;
    public TMP_Text labelText; // optional (can be null)

    private int count = 0;

    public void SetIcon(Sprite s)
    {
        if (iconImage)
        {
            iconImage.sprite = s;
            iconImage.enabled = (s != null);
        }
    }

    public void SetCount(int c)
    {
        count = Mathf.Max(0, c);
        UpdateView();
    }

    public void Add(int delta)
    {
        count = Mathf.Max(0, count + delta);
        UpdateView();
    }

    public void SetLabel(string label)
    {
        if (labelText) labelText.text = label;
    }

    private void UpdateView()
    {
        if (countText) countText.text = $"x{count}";
    }
}
