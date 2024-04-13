using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class Card : MonoBehaviour
{
    public enum Element
    {
        Fire = 0,
        Ice = 1,
        Lightning = 2
    };

    public enum Count
    {
        One = 0,
        Two = 3,
        Three = 6
    };

    public enum Fill
    {
        Full = 0,
        Partial = 9,
        None = 18
    };

    [SerializeField]
    private Element element_;

    [SerializeField]
    private Count count_;

    [SerializeField]
    private Fill fill_;

    private float error_time_ = 0;
    private float hint_time_ = 0;
    private bool is_active_ = false;
    private float base_color = 0.9f;
    private float disappearance_ = 0.0f;
    private bool is_disappearing_ = false;

    public delegate void OnCardClick(GameObject card);
    public static OnCardClick on_card_click;

    public delegate void OnCardDisappearace(GameObject card);
    public static OnCardDisappearace on_card_disappearance;

    public Element GetElement()
    {
        return element_;
    }

    public void Click()
    {
        if (!is_disappearing_)
        {
            on_card_click?.Invoke(gameObject);
        }
    }

    public int GetCode()
    {
        return (int)element_ + (int)count_ + (int)fill_;
    }

    public void Activate()
    {
        is_active_ = true;
        transform.localScale = new Vector3(1.1f, 1.1f, 1f);
        base_color = 1f;
    }

    public void Deactivate()
    {
        is_active_ = false;
        transform.localScale = new Vector3(1f, 1f, 1f);
        base_color = 0.9f;
    }

    public bool IsActive()
    {
        return is_active_;
    }

    public void Flash()
    {
        Deactivate();
        error_time_ += 1f;
    }

    public void Hint()
    {
        hint_time_ += 1f;
    }

    public void Disappear()
    {
        is_disappearing_ = true;
        on_card_disappearance?.Invoke(gameObject);
    }

    private void Update()
    {
        if (is_disappearing_)
        {
            disappearance_ += Time.deltaTime;
            if (disappearance_ >= 1f)
            {
                error_time_ = 0;
                is_active_ = false;
                base_color = 0.9f;
                disappearance_ = 0.0f;
                is_disappearing_ = false;
                transform.rotation = Quaternion.identity;
                transform.localScale = new Vector3(1f, 1f, 1f);
                GetComponent<UnityEngine.UI.Image>().color = new Color(255f * base_color, 255f * base_color, 255f * base_color);
                transform.SetParent(GameObject.Find("Deck").transform, false);
                return;
            }
            else
            {
                transform.rotation = Quaternion.AngleAxis(90f * disappearance_, Vector3.forward);
                transform.localScale = new Vector3(1.1f * (1f - disappearance_), 1.1f * (1f - disappearance_), 1f);
            }
        }
        error_time_ -= Mathf.Min(error_time_, Time.deltaTime);
        hint_time_ -= Mathf.Min(hint_time_, Time.deltaTime);
        float error_coef = 1f - 0.5f * Mathf.Abs(Mathf.Sin(2f * Mathf.PI * error_time_));
        float hint_coef = 0.1f * Mathf.Abs(Mathf.Sin(2f * Mathf.PI * hint_time_));
        GetComponent<UnityEngine.UI.Image>().color = new Color(base_color + hint_coef, base_color * error_coef + hint_coef, base_color * error_coef + hint_coef);
    }
}
