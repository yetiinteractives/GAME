using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Image))]
public class CustomButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    public enum CraftableItem
    {
        Medikit,
        Bandage,
        Silencer,
        ShotgunShell,
        Grenade,
        Landmine
    }

    public static event Action<CustomButton> OnHoveredCraftItem;
    public static event Action<CustomButton> OnUnhoveredCraftItem;
    public static event Action<CustomButton> OnCraftItemHold;
    public static event Action<CustomButton> OnCraftItemRelease;
    public static event Action<CustomButton> OnCraftCompleted;
    public static event Action<CustomButton> OnCraftDenied;
    public static event Action<CustomButton> OnCraftCanceled;

    [Header("Item")]
    [SerializeField] private CraftableItem itemToCraft;
    public CraftableItem ItemToCraft => itemToCraft;

    [Header("UI")]
    [SerializeField] private Image targetImage;
    public Image TargetImage => targetImage;

    [Header("Hover Text (TMP)")]
    [SerializeField] private TMP_Text itemText;
    [SerializeField] private TMP_Text itemDescription;

    [Header("Hover Text Content")]
    [SerializeField] private string itemTextValue;
    [SerializeField] private string itemDescriptionValue;

    [Header("Sprites")]
    [SerializeField] private Sprite normalSprite;
    [SerializeField] private Sprite hoverSprite;
    [SerializeField] private Sprite holdSprite;

    [Header("Hold Fill (Linear)")]
    [SerializeField] private float holdDuration = 1.5f;
    [SerializeField] private bool unfillOnCancel = true;

    [Header("Gate")]
    [SerializeField] private InventoryHandler inventoryHandler;

    private Coroutine holdRoutine;
    private bool isHovering;
    private bool isHolding;
    private bool crafted;

    private void Reset()
    {
        targetImage = GetComponent<Image>();
    }

    private void Awake()
    {
        if (targetImage == null) targetImage = GetComponent<Image>();

        targetImage.type = Image.Type.Filled;
        targetImage.fillMethod = Image.FillMethod.Horizontal;
        targetImage.fillOrigin = (int)Image.OriginHorizontal.Left;
        targetImage.fillAmount = 1f;

        SetNormal();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovering = true;
        OnHoveredCraftItem?.Invoke(this);
        if (!isHolding) SetHover();

        SetHoverText();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
        OnUnhoveredCraftItem?.Invoke(this);

        if (isHolding) CancelHold();
        else SetNormal();

        // DO NOT restore original text here
        // Keep last hovered item visible
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!isHovering || isHolding) return;

        if (inventoryHandler != null && !inventoryHandler.CanCraft(itemToCraft))
        {
            OnCraftDenied?.Invoke(this);
            return;
        }

        crafted = false;
        isHolding = true;
        OnCraftItemHold?.Invoke(this);

        if (holdSprite != null) targetImage.sprite = holdSprite;
        targetImage.fillAmount = 0f;

        if (holdRoutine != null) StopCoroutine(holdRoutine);
        holdRoutine = StartCoroutine(HoldFillRoutine());
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!isHolding) return;

        OnCraftItemRelease?.Invoke(this);

        if (!crafted) CancelHold();
    }

    private IEnumerator HoldFillRoutine()
    {
        float t = 0f;
        while (t < holdDuration)
        {
            t += Time.deltaTime;
            targetImage.fillAmount = Mathf.Clamp01(t / holdDuration);
            yield return null;
        }

        targetImage.fillAmount = 1f;
        isHolding = false;
        crafted = true;
        holdRoutine = null;

        OnCraftCompleted?.Invoke(this);

        if (isHovering) SetHover();
        else SetNormal();
    }

    private void CancelHold()
    {
        isHolding = false;

        if (holdRoutine != null)
        {
            StopCoroutine(holdRoutine);
            holdRoutine = null;
        }

        targetImage.fillAmount = unfillOnCancel ? 0f : 1f;
        OnCraftCanceled?.Invoke(this);

        if (isHovering) SetHover();
        else SetNormal();
    }

    private void SetNormal()
    {
        if (normalSprite != null) targetImage.sprite = normalSprite;
        targetImage.fillAmount = 1f;
    }

    private void SetHover()
    {
        if (hoverSprite != null) targetImage.sprite = hoverSprite;
        targetImage.fillAmount = 1f;
    }

    private void SetHoverText()
    {
        if (itemText != null) itemText.text = itemTextValue;
        if (itemDescription != null) itemDescription.text = itemDescriptionValue;
    }

    private void OnDisable()
    {
        isHovering = false;

        if (isHolding)
            OnCraftCanceled?.Invoke(this);

        isHolding = false;
        crafted = false;

        if (holdRoutine != null)
        {
            StopCoroutine(holdRoutine);
            holdRoutine = null;
        }

        SetNormal();
        // keep last hovered text; do not clear
    }
}