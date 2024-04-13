using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;
using UnityEngine.UIElements;
using static Card;
using static UpgradeManager;

public class Upgrade : MonoBehaviour
{
    [Serializable]
    public class UpgradeParam
    {
        public string name;
        public float delta;
    }

    [SerializeField]
    private string name_key_;

    [SerializeField]
    private string description_key_;

    [SerializeField]
    private List<UpgradeParam> upgrade_params_;

    [SerializeField]
    private int max_level_;

    static private string level_key_ = "upgrade.level";
    private int current_level_ = 0;
    private float animation_speed_ = 2f;
    private float scale_ = 1f;
    private float rotation_ = 0f;
    private float transparency_ = 0f;
    private bool is_appearing_ = false;
    private bool is_disappearing_ = false;
    private bool is_popping_ = false;

    private void UpdateTexts()
    {
        string name = LocalizationSettings.StringDatabase.GetLocalizedString(name_key_);
        string level = LocalizationSettings.StringDatabase.GetLocalizedString(level_key_, arguments: new object[] { current_level_ + 1 });
        string description = LocalizationSettings.StringDatabase.GetLocalizedString(description_key_);
        foreach (UpgradeParam upgrade_param in upgrade_params_)
        {
            Param param = UpgradeManager.Instance.GetParam(upgrade_param.name);
            float current_value = param.value;
            float next_value = param.value + upgrade_param.delta;
            description += '\n';
            description += LocalizationSettings.StringDatabase.GetLocalizedString(param.upgrade_key, arguments: new object[] { current_value, next_value });
        }
        transform.Find("Name").GetComponent<TextMeshProUGUI>().text = name;
        transform.Find("Level").GetComponent<TextMeshProUGUI>().text = level;
        transform.Find("Description").GetComponent<TextMeshProUGUI>().text = description;
    }

    public void Appear()
    {
        UpdateTexts();
        scale_ = 0f;
        rotation_ = 1f;
        transparency_ = 0f;
        is_appearing_ = true;
        is_disappearing_ = is_popping_ = false;
    }

    public void Disappear()
    {
        scale_ = 1f;
        rotation_ = 0f;
        transparency_ = 0f;
        is_disappearing_ = true;
        is_appearing_ = is_popping_ = false;
    }

    public void Pop()
    {
        scale_ = 1f;
        rotation_ = 0f;
        transparency_ = 0f;
        is_popping_ = true;
        is_appearing_ = is_disappearing_ = false;
    }

    private void UpdateImage()
    {
        transform.localScale = new Vector3(scale_, scale_, 1f);
        transform.rotation = Quaternion.AngleAxis(90f * rotation_, Vector3.forward);
        GetComponent<UnityEngine.UI.Image>().color = new Color(1f, 1f, 1f, 1f - transparency_);
    }

    private void Update()
    {
        if (is_appearing_)
        {
            scale_ += Time.deltaTime * animation_speed_ / UpgradeManager.slowdown;
            rotation_ -= Time.deltaTime * animation_speed_ / UpgradeManager.slowdown;
            if (scale_ >= 1f)
            {
                scale_ = 1f;
                rotation_ = 0f;
                is_appearing_ = false;
            }
            UpdateImage();
            return;
        }
        if (is_disappearing_)
        {
            scale_ -= Time.deltaTime * animation_speed_ / UpgradeManager.slowdown;
            rotation_ += Time.deltaTime * animation_speed_ / UpgradeManager.slowdown;
            if (scale_ <= 0f)
            {
                scale_ = 0f;
                rotation_ = 1f;
                is_disappearing_ = false;
            }
            UpdateImage();
            if (!is_disappearing_)
            {
                transform.SetParent(GameObject.Find("Upgrades").transform, false);
                if (current_level_ == max_level_)
                {
                    Destroy(gameObject);
                }
            }
            return;
        }
        if (is_popping_)
        {
            scale_ += Time.deltaTime * animation_speed_ / 3f / UpgradeManager.slowdown;
            transparency_ += Time.deltaTime * animation_speed_ / UpgradeManager.slowdown; ;
            if (transparency_ >= 1f)
            {
                scale_ = 0f;
                rotation_ = 1f;
                transparency_ = 0f;
                is_popping_ = false;
            }
            UpdateImage();
            if (!is_popping_)
            {
                transform.SetParent(GameObject.Find("Upgrades").transform, false);
                if (current_level_ == max_level_)
                {
                    Destroy(gameObject);
                }
            }
            return;
        }
    }

    public void Click()
    {
        if (!is_appearing_ && !is_disappearing_ && !is_popping_)
        {
            foreach (UpgradeParam upgrade_param in upgrade_params_)
            {
                Param param = UpgradeManager.Instance.GetParam(upgrade_param.name);
                UpgradeManager.Instance.UpdateParam(upgrade_param.name, param.value + upgrade_param.delta);
            }
            current_level_++;
            UpgradeManager.Instance.UpgradeSelected();
            Pop();
        }
    }
}
