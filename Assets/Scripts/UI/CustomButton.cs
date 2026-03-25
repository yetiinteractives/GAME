using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CustomButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Timing")]
    [SerializeField] private float holdDuration = 1f;

    [Header("Visual (optional)")]
    [SerializeField] private Image holdProgressImage; // Set to Filled type in Inspector
    [SerializeField] private bool resetProgressOnExit = true;

    [Header("Events")]
    public UnityEvent onHoverEnter;
    public UnityEvent onHoverExit;
    public UnityEvent<float> onHoldProgress; // 0..1
    public UnityEvent onHoldStart;
    public UnityEvent onHoldCanceled;
    public UnityEvent onCraftComplete; // Fires when hold reaches duration

    private bool isPointerOver;
    private bool isHolding;
    private bool craftCompleted;
    private float holdTimer;

    private void Awake()
    {
        ResetProgressVisual();
    }

    private void Update()
    {
        if (!isHolding || craftCompleted) return;

        holdTimer += Time.deltaTime;
        float progress = Mathf.Clamp01(holdTimer / holdDuration);

        onHoldProgress?.Invoke(progress);
        UpdateProgressVisual(progress);

        if (progress >= 1f)
        {
            craftCompleted = true;
            isHolding = false;
            onCraftComplete?.Invoke();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isPointerOver = true;
        onHoverEnter?.Invoke();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isPointerOver = false;
        onHoverExit?.Invoke();

        
        if (isHolding)
        {
            CancelHold();
        }
        else if (resetProgressOnExit)
        {
            ResetHold();
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!isPointerOver) return;

        craftCompleted = false;
        isHolding = true;
        holdTimer = 0f;
        onHoldStart?.Invoke();
        UpdateProgressVisual(0f);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // If released before completion, cancel
        if (!craftCompleted)
        {
            CancelHold();
        }
    }

    private void CancelHold()
    {
        isHolding = false;
        holdTimer = 0f;
        onHoldCanceled?.Invoke();
        ResetProgressVisual();
    }

    private void ResetHold()
    {
        isHolding = false;
        craftCompleted = false;
        holdTimer = 0f;
        ResetProgressVisual();
    }

    private void UpdateProgressVisual(float progress)
    {
        if (holdProgressImage != null)
            holdProgressImage.fillAmount = progress;
    }

    private void ResetProgressVisual()
    {
        if (holdProgressImage != null)
            holdProgressImage.fillAmount = 0f;
    }
}