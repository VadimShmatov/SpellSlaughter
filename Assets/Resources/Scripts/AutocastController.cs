using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

public class AutocastController : MonoBehaviour
{
    [SerializeField]
    string param_name_;

    [SerializeField]
    SpellButton spell_button_;

    float min_distance_ = 1f;
    float max_distance_ = 5f;

    string far_key_ = "auto_cast.far";
    string medium_key_ = "auto_cast.medium";
    string near_key_ = "auto_cast.near";

    void UpdateText(float distance)
    {
        float ratio = (distance - min_distance_) / (max_distance_ - min_distance_);
        if (ratio < 1f / 3f)
        {
            transform.Find("SliderGroup").Find("AutocastComment").GetComponent<TextMeshProUGUI>().text = LocalizationSettings.StringDatabase.GetLocalizedString(near_key_);
        }
        else if (ratio < 2f / 3f)
        {
            transform.Find("SliderGroup").Find("AutocastComment").GetComponent<TextMeshProUGUI>().text = LocalizationSettings.StringDatabase.GetLocalizedString(medium_key_);
        }
        else
        {
            transform.Find("SliderGroup").Find("AutocastComment").GetComponent<TextMeshProUGUI>().text = LocalizationSettings.StringDatabase.GetLocalizedString(far_key_);
        }
    }

    private void Awake()
    {
        float distance = PlayerPrefs.GetFloat(param_name_, -1f);
        spell_button_.SetAutocastDistance(distance);
        if (distance < 0f)
        {
            transform.Find("Toggle").GetComponent<Toggle>().isOn = false;
            OnCheckboxChange(false);
        }
        else
        {
            transform.Find("SliderGroup").Find("Slider").GetComponent<Slider>().value = 1f - (distance - min_distance_) / (max_distance_ - min_distance_);
            UpdateText(distance);
            transform.Find("Toggle").GetComponent<Toggle>().isOn = true;
            OnCheckboxChange(true);
        }
    }

    public void OnSliderChange(float value)
    {
        float distance = (value - 1f) * (min_distance_ - max_distance_) + min_distance_;
        spell_button_.SetAutocastDistance(distance);
        UpdateText(distance);
        PlayerPrefs.SetFloat(param_name_, distance);
    }

    public void OnCheckboxChange(bool value)
    {
        transform.Find("SliderGroup").gameObject.SetActive(value);
        if (value)
        {
            float distance = (transform.Find("SliderGroup").Find("Slider").GetComponent<Slider>().value - 1f) * (min_distance_ - max_distance_) + min_distance_;
            UpdateText(distance);
            spell_button_.SetAutocastDistance(distance);
            PlayerPrefs.SetFloat(param_name_, distance);
        }
        else
        {
            spell_button_.SetAutocastDistance(-1f);
            PlayerPrefs.SetFloat(param_name_, -1f);
        }
    }
}
