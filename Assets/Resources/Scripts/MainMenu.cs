using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void Awake()
    {
        int locale = PlayerPrefs.GetInt("locale", 0);
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[locale];
    }

        public void StartGame()
    {
        Time.timeScale = 1.0f;
        SceneManager.LoadScene("Game");
    }

    public void Options()
    {
        SceneManager.LoadScene("Options");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
