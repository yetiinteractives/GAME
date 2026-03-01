using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RadialMenuController : MonoBehaviour
{
    [Header("Activation")]
    [SerializeField] private KeyCode holdKey = KeyCode.Q;
    [SerializeField] private bool allowClickToConfirm = true;

    [Header("Time Effect")]
    [Tooltip("1 = normal time, 0 = full stop. Example 0.08 for strong slow-mo.")]
    [Range(0f, 1f)]
    [SerializeField] private float radialTimeScale = 0.08f;
    [SerializeField] private bool useUnscaledUI = true;

    [Header("References")]
    [SerializeField] private GameObject radialRoot;          // WeaponRadialUI root
    [SerializeField] private RectTransform ringRoot;         // centered ring root
    [SerializeField] private Text centerLabel;               // optional
    [SerializeField] private List<RadialSliceUI> slices = new List<RadialSliceUI>();

    [Header("Items")]
    [SerializeField] private List<RadialItem> items = new List<RadialItem>();

    [Header("Look & Feel")]
    [SerializeField] private Color normalColor = new Color(1f, 1f, 1f, 0.35f);
    [SerializeField] private Color hoverColor = new Color(1f, 1f, 1f, 0.9f);
    [SerializeField] private float ringRadius = 180f;
    [SerializeField] private float deadZonePixels = 30f;     // near center = no selection

    // Event you can hook from your existing weapon switch script.
    public Action<RadialItem, int> OnItemSelected;

    private bool isOpen;
    private int hoveredIndex = -1;
    private float previousTimeScale = 1f;
    private float previousFixedDelta = 0.02f;

    [Serializable]
    public class RadialItem
    {
        public string id;           // e.g. "rifle", "pistol"
        public string displayName;  // UI text
        public Sprite icon;
    }

    private void Awake()
    {
        if (radialRoot != null) radialRoot.SetActive(false);
        BuildVisuals();
    }

    private void Update()
    {
        // Open on hold start
        if (Input.GetKeyDown(holdKey))
        {
            OpenRadial();
        }

        // While holding, update hover
        if (isOpen && Input.GetKey(holdKey))
        {
            UpdateHoverFromMouse();
            if (allowClickToConfirm && Input.GetMouseButtonDown(0))
            {
                ConfirmSelection();
            }
        }

        // Release key = confirm & close
        if (isOpen && Input.GetKeyUp(holdKey))
        {
            ConfirmSelection();
            CloseRadial();
        }
    }

    private void OpenRadial()
    {
        isOpen = true;
        hoveredIndex = -1;

        if (radialRoot != null) radialRoot.SetActive(true);

        previousTimeScale = Time.timeScale;
        previousFixedDelta = Time.fixedDeltaTime;

        Time.timeScale = radialTimeScale;
        Time.fixedDeltaTime = 0.02f * Time.timeScale; // keep physics stable relative to timescale

        RefreshColors();
        SetCenterText("Select Weapon");
    }

    private void CloseRadial()
    {
        isOpen = false;

        Time.timeScale = previousTimeScale;
        Time.fixedDeltaTime = previousFixedDelta;

        if (radialRoot != null) radialRoot.SetActive(false);
    }

    private void ConfirmSelection()
    {
        if (hoveredIndex < 0 || hoveredIndex >= items.Count) return;

        var selected = items[hoveredIndex];
        OnItemSelected?.Invoke(selected, hoveredIndex);
        Debug.Log($"[Radial] Selected: {selected.displayName} ({selected.id})");
    }

    private void UpdateHoverFromMouse()
    {
        if (ringRoot == null || items.Count == 0) return;

        Vector2 center = RectTransformUtility.WorldToScreenPoint(null, ringRoot.position);
        Vector2 mouse = Input.mousePosition;
        Vector2 dir = mouse - center;

        if (dir.magnitude < deadZonePixels)
        {
            hoveredIndex = -1;
            RefreshColors();
            SetCenterText("Select Weapon");
            return;
        }

        // Angle: 0 at right, CCW positive
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        if (angle < 0) angle += 360f;

        float sliceSize = 360f / items.Count;

        // Offset by half slice so "top" or exact boundaries feel nicer
        float shifted = (angle + sliceSize * 0.5f) % 360f;
        int index = Mathf.FloorToInt(shifted / sliceSize);

        index = Mathf.Clamp(index, 0, items.Count - 1);

        if (hoveredIndex != index)
        {
            hoveredIndex = index;
            RefreshColors();
            SetCenterText(items[hoveredIndex].displayName);
        }
    }

    public void BuildVisuals()
    {
        int count = Mathf.Min(items.Count, slices.Count);
        if (count == 0) return;

        float sliceSize = 360f / count;

        for (int i = 0; i < slices.Count; i++)
        {
            bool active = i < count;
            slices[i].gameObject.SetActive(active);
            if (!active) continue;

            var item = items[i];
            slices[i].SetIcon(item.icon);
            slices[i].SetLabel(item.displayName);
            slices[i].SetColor(normalColor);

            // Place each slice around ring
            float angle = i * sliceSize;
            slices[i].SetAngleAndRadius(angle, ringRadius);
        }
    }

    private void RefreshColors()
    {
        int count = Mathf.Min(items.Count, slices.Count);
        for (int i = 0; i < count; i++)
        {
            slices[i].SetColor(i == hoveredIndex ? hoverColor : normalColor);
            slices[i].SetHighlighted(i == hoveredIndex);
        }
    }

    private void SetCenterText(string t)
    {
        if (centerLabel != null) centerLabel.text = t;
    }

    // Optional external API if you want to populate from your weapon manager at runtime.
    public void SetItems(List<RadialItem> newItems)
    {
        items = newItems ?? new List<RadialItem>();
        hoveredIndex = -1;
        BuildVisuals();
        RefreshColors();
    }
}