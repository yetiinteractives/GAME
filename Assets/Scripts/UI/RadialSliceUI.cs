using UnityEngine;
using UnityEngine.UI;

public class RadialSliceUI : MonoBehaviour
{
    [SerializeField] private RectTransform root;
    [SerializeField] private Image sliceImage;
    [SerializeField] private RectTransform iconRoot;
    [SerializeField] private Image iconImage;
    [SerializeField] private Text label;

    [Header("Animation")]
    [SerializeField] private float highlightedScale = 1.08f;
    [SerializeField] private float normalScale = 1f;

    public void SetIcon(Sprite s)
    {
        if (iconImage != null) iconImage.sprite = s;
        if (iconImage != null) iconImage.enabled = s != null;
    }

    public void SetLabel(string t)
    {
        if (label != null) label.text = t;
    }

    public void SetColor(Color c)
    {
        if (sliceImage != null) sliceImage.color = c;
    }

    public void SetHighlighted(bool on)
    {
        if (root == null) return;
        root.localScale = Vector3.one * (on ? highlightedScale : normalScale);
    }

    public void SetAngleAndRadius(float angleDeg, float radius)
    {
        if (root == null) root = transform as RectTransform;
        if (root == null) return;

        // Rotate slice object
        root.localRotation = Quaternion.Euler(0f, 0f, angleDeg);

        // Put icon outward on local +X
        if (iconRoot != null)
        {
            iconRoot.anchoredPosition = new Vector2(radius, 0f);
            // keep icon upright
            iconRoot.localRotation = Quaternion.Euler(0f, 0f, -angleDeg);
        }
    }
}