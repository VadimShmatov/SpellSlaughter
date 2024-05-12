using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.UI.Image;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance { get; private set; }

    [Serializable]
    public class Param
    {
        public string name;
        public float value;
        public string upgrade_key;

        public Param Clone()
        {
            Param p = new Param();
            p.name = name;
            p.value = value;
            p.upgrade_key = upgrade_key;
            return p;
        }
    };

    [SerializeField]
    private List<Param> params_;

    [SerializeField]
    private GameObject upgrade_button_;

    [SerializeField]
    private List<GameObject> upgrade_slots_;

    [SerializeField]
    private GameObject upgrades_;

    private Dictionary<string, Param> current_params_;
    private float current_xp_ = 0f;
    private float xp_for_next_upgrade_ = 100f;
    private float xp_increase_rate_ = 1.1f;
    private float upgrade_state_ = 0f;
    private bool upgrade_ready_ = false;
    public static float slowdown = 0.001f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        else
        {
            Instance = this;
        }
        current_params_ = new Dictionary<string, Param>();
        foreach (Param param in params_)
        {
            current_params_[param.name] = param.Clone();
        }
        for (int i = 0; i < transform.childCount; i++)
        {
            GameObject obj = transform.GetChild(i).gameObject;
            if (obj.name != "UpgradeButton")
            {
                obj.SetActive(false);
            }
        }

        int difficulty = PlayerPrefs.GetInt("difficulty", 0);
        if (difficulty == 0)
        {
            UpdateParam("letter_generation_attempts", 15f);
            UpdateParam("word_max_frequency", 5000f);
            UpdateParam("letter_difficulty", 0.01f);
        }
        else if (difficulty == 1)
        {
            UpdateParam("letter_generation_attempts", 5f);
            UpdateParam("word_max_frequency", 15000f);
            UpdateParam("letter_difficulty", 0.1f);
        }
        else if (difficulty == 2)
        {
            UpdateParam("letter_generation_attempts", 3f);
            UpdateParam("word_max_frequency", 50000f);
            UpdateParam("letter_difficulty", 0.3f);
        }
    }

    public Param GetParam(string name)
    {
        if (!current_params_.ContainsKey(name))
        {
            throw new ArgumentException("Nonexistent parameter", name);
        }
        return current_params_[name];
    }

    public void UpdateParam(string name, float value)
    {
        if (!current_params_.ContainsKey(name))
        {
            throw new ArgumentException("Nonexistent parameter", name);
        }
        current_params_[name].value = value;
    }

    float CalculateProgress()
    {
        float result = 0f;
        float remaining_xp = current_xp_;
        float xp_for_next_upgrade = xp_for_next_upgrade_;
        while (remaining_xp >= xp_for_next_upgrade)
        {
            result += 1f;
            remaining_xp -= xp_for_next_upgrade;
            xp_for_next_upgrade *= xp_increase_rate_;
        }
        return result + remaining_xp / xp_for_next_upgrade;
    }

    private void UpdateButton()
    {
        float progress = CalculateProgress();
        upgrade_button_.transform.Find("UpgradeProgress").GetComponent<Image>().fillAmount = Mathf.Min(progress, 1f);
        if (progress >= 1f)
        {
            upgrade_button_.transform.Find("UpgradeIconActive").GetComponent<Image>().color = new Color(1f, 1f, 1f, 1f);
            upgrade_button_.transform.Find("UpgradeCount").GetComponent<TextMeshProUGUI>().color = new Color(1f, 1f, 1f, 1f);
            upgrade_button_.transform.Find("UpgradeCount").GetComponent<TextMeshProUGUI>().text = Mathf.FloorToInt(progress).ToString();
        }
        else
        {
            upgrade_button_.transform.Find("UpgradeIconActive").GetComponent<Image>().color = new Color(1f, 1f, 1f, 0f);
            upgrade_button_.transform.Find("UpgradeCount").GetComponent<TextMeshProUGUI>().color = new Color(1f, 1f, 1f, 0f);
        }
    }

    void GainXp(Enemy _)
    {
        current_xp_ += 1f;
        UpdateButton();
    }

    void GainXp(LetterHolder.Element _, float xp)
    {
        current_xp_ += 2f * xp;
        UpdateButton();
    }

    private void OnEnable()
    {
        Enemy.on_enemy_death += GainXp;
        LetterHolder.on_spell_infusion += GainXp;
    }

    private void OnDisable()
    {
        Enemy.on_enemy_death -= GainXp;
        LetterHolder.on_spell_infusion -= GainXp;
    }

    public void StartUpgrade()
    {
        if (current_xp_ >= xp_for_next_upgrade_)
        {
            upgrade_state_ += 0.00001f;
            Time.timeScale = slowdown;
            for (int i = 0; i < transform.childCount; i++)
            {
                GameObject obj = transform.GetChild(i).gameObject;
                if (obj.name != "UpgradeButton")
                {
                    obj.SetActive(true);
                }
            }
        }
    }

    public void UpgradeSelected()
    {
        foreach (GameObject upgrade_slot in upgrade_slots_)
        {
            upgrade_slot.transform.GetChild(0).GetComponent<Upgrade>().Disappear();
        }
        current_xp_ -= xp_for_next_upgrade_;
        xp_for_next_upgrade_ *= xp_increase_rate_;
        UpdateButton();
    }

    private void Update()
    {
        if (!upgrade_ready_ && upgrade_state_ > 0f && upgrade_state_ < 1f)
        {
            if (upgrade_state_ <= 0.00001f)
            {
                upgrade_state_ += 2 * Time.deltaTime;
            }
            else
            {
                upgrade_state_ += 2 * Time.deltaTime / slowdown;
            }
            if (upgrade_state_ >= 1f)
            {
                upgrade_ready_ = true;
                upgrade_state_ = 1f;
            }
            transform.Find("Blackout").GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.8f * upgrade_state_);
        }
        if (upgrade_ready_ && upgrade_state_ > 0f && upgrade_state_ < 1f)
        {
            upgrade_state_ -= 2 * Time.deltaTime / slowdown;
            if (upgrade_state_ <= 0f)
            {
                upgrade_ready_ = false;
                upgrade_state_ = 0f;
                for (int i = 0; i < transform.childCount; i++)
                {
                    GameObject obj = transform.GetChild(i).gameObject;
                    if (obj.name != "UpgradeButton")
                    {
                        obj.SetActive(false);
                    }
                }
                Time.timeScale = 1f;
            }
            transform.Find("Blackout").GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.8f * upgrade_state_);
        }
        if (upgrade_ready_ && upgrade_state_ == 1f)
        {
            foreach (GameObject upgrade_slot in upgrade_slots_)
            {
                if (upgrade_slot.transform.childCount == 0)
                {
                    if (current_xp_ <= xp_for_next_upgrade_)
                    {
                        upgrade_state_ -= 0.00001f;
                        return;
                    }
                    int new_upgrade = UnityEngine.Random.Range(0, upgrades_.transform.childCount);
                    GameObject upgrade = upgrades_.transform.GetChild(new_upgrade).gameObject;
                    upgrade.transform.SetParent(upgrade_slot.transform, false);
                    upgrade.GetComponent<Upgrade>().Appear();
                }
            }
        }
    }

};