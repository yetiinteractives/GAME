
using UnityEngine;
using TMPro;
using UnityEngine.UI;


public class LetterUIController : MonoBehaviour
{
    public GameObject letterPanel;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI pageText;
    public TextMeshProUGUI pageCounter;
    public Button prevButton;
    public Button nextButton;
    public Image letterBackground;

    private LetterData currentData;
    private int currentPage;

    void Start()
    {
        letterPanel.SetActive(false);
        prevButton.onClick.AddListener(PrevPage);
        nextButton.onClick.AddListener(NextPage);
    }

    public void ShowLetter(LetterData data)
    {
        currentData = data;
        currentPage = 0;

        if (data.letterImage != null)
            letterBackground.sprite = data.letterImage;

        letterPanel.SetActive(true);

        RefreshPage();
    }

    public void HideLetter()
    {
        letterPanel.SetActive(false);
    }

    void RefreshPage()
    {
        titleText.text = currentData.letterTitle;
        pageText.text = currentData.pages[currentPage];
        pageCounter.text = $"Page {currentPage + 1} / {currentData.pages.Length}";
        prevButton.interactable = currentPage > 0;
        nextButton.interactable = currentPage < currentData.pages.Length - 1;
    }

    void NextPage()
    {
        if (currentPage < currentData.pages.Length - 1)
        {
            currentPage++;
            RefreshPage();
        }
    }

    void PrevPage()
    {
        if (currentPage > 0)
        {
            currentPage--;
            RefreshPage();
        }
    }

    public void OnCloseButton() => LetterManager.Instance.CloseLetter();
}