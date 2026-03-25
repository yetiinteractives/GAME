using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class CustomButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Target Image")]
    [SerializeField] private Image targetImage; // main button image

    [Header("Sprites")]
    [SerializeField] private Sprite normalSprite;
    [SerializeField] private Sprite hoverSprite;
    [SerializeField] private Sprite holdSprite;

    [Header("Hold Fill")]
    [SerializeField] private float holdDuration = 1.5f;
    [SerializeField] private bool unfillOnCancel = true;

    [Header("Events")]
    public UnityEvent onCraftComplete;

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

        // Fill settings for hold mode
        targetImage.type = Image.Type.Filled;
        targetImage.fillMethod = Image.FillMethod.Horizontal;
        targetImage.fillOrigin = (int)Image.OriginHorizontal.Left;
        targetImage.fillAmount = 1f; // full in normal state

        SetNormal();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovering = true;
        if (!isHolding) SetHover();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;

        // Optional behavior: leaving cancels hold
        if (isHolding) CancelHold();
        else SetNormal();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!isHovering || isHolding) return;

        crafted = false;
        isHolding = true;

        // hold state sprite + start empty fill
        targetImage.sprite = holdSprite != null ? holdSprite : targetImage.sprite;
        targetImage.fillAmount = 0f;

        if (holdRoutine != null) StopCoroutine(holdRoutine);
        holdRoutine = StartCoroutine(HoldFillRoutine());
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!isHolding) return;

        // If not completed, cancel/reset
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

        // Completed
        targetImage.fillAmount = 1f;
        isHolding = false;
        crafted = true;
        holdRoutine = null;

        onCraftComplete?.Invoke();

        // stay hover if pointer still on, else normal
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

        if (unfillOnCancel) targetImage.fillAmount = 0f;
        else targetImage.fillAmount = 1f;

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
}