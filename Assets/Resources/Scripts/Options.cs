using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Options : MonoBehaviour
{
    [SerializeField]
    private GameObject english_button_;

    [SerializeField]
    private GameObject russian_button_;

    [SerializeField]
    private GameObject easy_button_;

    [SerializeField]
    private GameObject medium_button_;

    [SerializeField]
    private GameObject hard_button_;

    private void SetSelected(GameObject button, bool selected)
    {
        var colors = button.GetComponent<Button>().colors;
        colors.normalColor = new Color(1f, 1f, 1f, selected ? 0.3f : 0f);
        button.GetComponent<Button>().colors = colors;
    }

    public void Awake()
    {
        int locale = PlayerPrefs.GetInt("locale", 0);
        if (locale == 0)
        {
            SetSelected(english_button_, true);
            SetSelected(russian_button_, false);
        }
        else if (locale == 1)
        {
            SetSelected(english_button_, false);
            SetSelected(russian_button_, true);
        }
        int difficulty = PlayerPrefs.GetInt("difficulty", 0);
        if (difficulty == 0)
        {
            SetSelected(easy_button_, true);
            SetSelected(medium_button_, false);
            SetSelected(hard_button_, false);
        }
        else if (difficulty == 1)
        {
            SetSelected(easy_button_, false);
            SetSelected(medium_button_, true);
            SetSelected(hard_button_, false);
        }
        else if (difficulty == 2)
        {
            SetSelected(easy_button_, false);
            SetSelected(medium_button_, false);
            SetSelected(hard_button_, true);
        }
    }

    public void EnglishLocale()
    {
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[0];
        PlayerPrefs.SetInt("locale", 0);
        SetSelected(english_button_, true);
        SetSelected(russian_button_, false);
    }

    public void RussianLocale()
    {
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[1];
        PlayerPrefs.SetInt("locale", 1);
        SetSelected(english_button_, false);
        SetSelected(russian_button_, true);
    }

    public void EasyDifficulty()
    {
        PlayerPrefs.SetInt("difficulty", 0);
        SetSelected(easy_button_, true);
        SetSelected(medium_button_, false);
        SetSelected(hard_button_, false);
    }

    public void MediumDifficulty()
    {
        PlayerPrefs.SetInt("difficulty", 1);
        SetSelected(easy_button_, false);
        SetSelected(medium_button_, true);
        SetSelected(hard_button_, false);
    }

    public void HardDifficulty()
    {
        PlayerPrefs.SetInt("difficulty", 2);
        SetSelected(easy_button_, false);
        SetSelected(medium_button_, false);
        SetSelected(hard_button_, true);
    }

    public void ToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
