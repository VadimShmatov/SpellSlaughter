using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static Card;

public class SpellButton : MonoBehaviour
{
    [SerializeField]
    private string spell_name;

    [SerializeField]
    private Card.Element spell_element_;

    [SerializeField]
    private float reload_speed;

    private float progress = 0f;
    private float boost = 0f;
    private bool can_be_cast = false;
    private bool is_draining = false;

    private float autocast_distance_ = -1f;

    public delegate void OnSpellActivation(string spell_name);
    public static OnSpellActivation on_spell_activation;

    void AddBoost(GameObject card)
    {
        if (card.GetComponent<Card>().GetElement() == spell_element_)
        {
            boost += 4f;
        }
        else
        {
            float rainbow_efficiency = UpgradeManager.Instance.GetParam("rainbow_recovery_rate").value / 100f;
            bool lightning_buff = GameObject.Find("Circle").GetComponent<SpellCaster>().LightningBuffActive();
            if (rainbow_efficiency > 0f && lightning_buff && spell_element_ != Element.Lightning)
            {
                boost += 4f * rainbow_efficiency;
            }
        }
    }

    void AddBoost(LetterHolder.Element element, float amount)
    {
        if (LetterHolder.ToCardElement(element) == spell_element_)
        {
            boost += 3f * amount;
        }
        else
        {
            float rainbow_efficiency = UpgradeManager.Instance.GetParam("rainbow_recovery_rate").value / 100f;
            bool lightning_buff = GameObject.Find("Circle").GetComponent<SpellCaster>().LightningBuffActive();
            if (rainbow_efficiency > 0f && lightning_buff && spell_element_ != Element.Lightning)
            {
                boost += 3f * amount * rainbow_efficiency;
            }
        }
    }

    void CheckAutocast(float distance)
    {
        if (distance < autocast_distance_)
        {
            Cast();
        }
    }

    private void OnEnable()
    {
        Card.on_card_disappearance += AddBoost;
        LetterHolder.on_spell_infusion += AddBoost;
        Enemy.on_enemy_movement += CheckAutocast;
    }

    private void OnDisable()
    {
        Card.on_card_disappearance -= AddBoost;
        LetterHolder.on_spell_infusion -= AddBoost;
        Enemy.on_enemy_movement -= CheckAutocast;
    }

    void Update()
    {
        float recovery_speed = 1f;
        switch (spell_element_)
        {
        case Element.Fire:
            recovery_speed = UpgradeManager.Instance.GetParam("fire_recovery_speed").value / 100f;
            break;
        case Element.Ice:
            recovery_speed = UpgradeManager.Instance.GetParam("cold_recovery_speed").value / 100f;
            break;
        case Element.Lightning:
            recovery_speed = UpgradeManager.Instance.GetParam("lightning_recovery_speed").value / 100f;
            break;
        }
        float boost_consumption = Mathf.Min(boost, Time.deltaTime / reload_speed);
        boost -= boost_consumption;
        if (is_draining)
        {
            boost = 0f;
            float drain_speed = 0f;
            if (spell_element_ == Element.Fire)
            {
                int number_of_recasts = Mathf.RoundToInt(UpgradeManager.Instance.GetParam("fire_recast_amount").value);
                if (number_of_recasts > 0)
                {
                    drain_speed = 1f / (2f * number_of_recasts);
                }
            }
            if (spell_element_ == Element.Lightning)
            {
                float duration = UpgradeManager.Instance.GetParam("lightning_buff_duration").value;
                drain_speed = 1f / duration;
            }
            progress -= drain_speed * Time.deltaTime;
            if (progress <= 0)
            {
                progress = 0f;
                is_draining = false;
            }
        }
        else if (progress <= 1f)
        {
            progress += reload_speed * (Time.deltaTime + boost_consumption) * recovery_speed;
            if (progress >= 1f)
            {
                progress = 1f;
                boost = 0f;
                can_be_cast = true;
            }
        }
        transform.Find("SpellProgress").GetComponent<Image>().fillAmount = progress;
        if (!is_draining)
        {
            transform.Find("SpellIconActive").GetComponent<Image>().fillAmount = progress;
        }
    }

    public void SetAutocastDistance(float distance)
    {
        autocast_distance_ = distance;
    }

    public void Cast()
    {
        if (can_be_cast)
        {
            can_be_cast = false;
            float drain_speed = 0f;
            if (spell_element_ == Element.Fire)
            {
                int number_of_recasts = Mathf.RoundToInt(UpgradeManager.Instance.GetParam("fire_recast_amount").value);
                if (number_of_recasts > 0)
                {
                    drain_speed = 1f / (2f * number_of_recasts);
                }
            }
            if (spell_element_ == Element.Lightning)
            {
                float duration = UpgradeManager.Instance.GetParam("lightning_buff_duration").value;
                drain_speed = 1f / duration;
            }
            if (drain_speed == 0f)
            {
                progress = 0f;
            }
            else
            {
                is_draining = true;
            }
            boost = 0f;
            on_spell_activation?.Invoke(spell_name);
        }
    }
}
