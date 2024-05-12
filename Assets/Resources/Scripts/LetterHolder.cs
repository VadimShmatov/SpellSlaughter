using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;
using System.Collections;
using System.Text;
using System;

public class LetterHolder : MonoBehaviour
{
    public enum Element
    {
        Fire = 0,
        Ice = 1,
        Lightning = 2
    };

    public static Card.Element ToCardElement(Element element)
    {
        switch (element)
        {
            case Element.Fire:
                return Card.Element.Fire;
            case Element.Ice:
                return Card.Element.Ice;
            case Element.Lightning:
                return Card.Element.Lightning;
        }
        throw new ArgumentException("Invalid element");
    }

    private static Dictionary<Element, string> sphere_names_ = new Dictionary<Element, string>
    {
        { Element.Fire, "FireSphere" },
        { Element.Ice, "IceSphere" },
        { Element.Lightning, "LightningSphere" },
    };

    [SerializeField]
    private Camera main_camera_;

    [SerializeField]
    private LetterController letter_controller_;

    bool is_main = false;
    bool is_active = false;
    List<LetterHolder> letters = new List<LetterHolder>();

    float red_flash = 0f;
    float green_flash = 0f;
    float animation_time = 1f;
    float cooldown = 0f;
    float hint = 0f;

    bool is_infused = false;
    Element infusion_element = Element.Fire;
    float infusion = 0f;


    public delegate void OnLetterChained(LetterHolder letter_holder);
    public static OnLetterChained on_letter_chained;

    public delegate void OnWrongWord();
    public static OnWrongWord on_wrong_word;

    public delegate void OnCorrectWord();
    public static OnCorrectWord on_correct_word;

    public delegate void OnSpellInfusion(Element element, float infusion_power);
    public static OnSpellInfusion on_spell_infusion;

    private void OnEnable()
    {
        on_letter_chained += OnLetterChainedHandler;
        on_wrong_word += WrongWordHandler;
        on_correct_word += CorrectWordHandler;
    }

    private void OnDisable()
    {
        on_letter_chained -= OnLetterChainedHandler;
        on_wrong_word -= WrongWordHandler;
        on_correct_word -= CorrectWordHandler;
    }

    void OnLetterChainedHandler(LetterHolder letter_holder)
    {
        if (!is_active || !is_main)
        {
            return;
        }
        if (!letter_holder.is_active)
        {
            letter_holder.SetActive(true);

            var last_letter_holder = letters.Last();
            var line = last_letter_holder.transform.Find("Line");
            Vector2 holder_position = last_letter_holder.transform.position;
            Vector2 cursor_position = letter_holder.transform.position;
            var distance = (cursor_position - holder_position).magnitude;
            line.localScale = new Vector3(0.1f, distance, 1);
            line.position = (holder_position + cursor_position) / 2;
            float rotation = Vector2.SignedAngle(Vector2.right, cursor_position - holder_position);
            line.rotation = Quaternion.AngleAxis(90 + rotation, Vector3.forward);

            letters.Add(letter_holder);
        }
    }

    void WrongWordHandler()
    {
        cooldown = animation_time;
    }

    void CorrectWordHandler()
    {
        hint = 0f;
        transform.Find("HintLine").gameObject.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0f);
        cooldown = animation_time;
    }

    public void SetActive(bool active)
    {
        is_active = active;
        transform.Find("Activation").gameObject.GetComponent<SpriteRenderer>().color = new Color(0f, 1f, 0f, active ? 1f : 0f);
        var line = transform.Find("Line");
        line.gameObject.SetActive(active);
        line.position = Vector3.zero;
        line.localScale = Vector3.zero;
        if (!active)
        {
            is_main = false;
        }
    }

    public void OnMouseDown()
    {
        if (cooldown > 0f)
        {
            return;
        }
        SetActive(true);
        is_main = true;
        letters.Add(this);
    }

    public void OnMouseUp()
    {
        if (cooldown > 0f)
        {
            return;
        }
        StringBuilder word_builder = new StringBuilder();
        foreach (LetterHolder letter_holder in letters)
        {
            letter_holder.SetActive(false);
            var text = letter_holder.transform.Find("Letter").gameObject.GetComponent<TextMeshPro>();
            word_builder.Append(text.text);
        }
        string word = word_builder.ToString().ToLower();
        bool is_correct = letter_controller_.CheckWord(word);
        if (is_correct)
        {
            float fire_infusion = 0f;
            float ice_infusion = 0f;
            float lightning_infusion = 0f;
            float infusion_multiplier = 1f + 0.2f * (word.Length - 3);
            foreach (LetterHolder letter_holder in letters)
            {
                if (letter_holder.is_infused)
                {
                    switch (letter_holder.infusion_element)
                    {
                        case Element.Fire:
                            fire_infusion += 5f;
                            break;
                        case Element.Ice:
                            ice_infusion += 5f;
                            break;
                        case Element.Lightning:
                            lightning_infusion += 5f;
                            break;
                    }
                }
                else
                {
                    fire_infusion += 1f;
                    ice_infusion += 1f;
                    lightning_infusion += 1f;
                }
            }
            if (fire_infusion > 0f)
            {
                on_spell_infusion?.Invoke(Element.Fire, fire_infusion * infusion_multiplier);
            }
            if (ice_infusion > 0f)
            {
                on_spell_infusion?.Invoke(Element.Ice, ice_infusion * infusion_multiplier);
            }
            if (lightning_infusion > 0f)
            {
                on_spell_infusion?.Invoke(Element.Lightning, lightning_infusion * infusion_multiplier);
            }
        }
        List<char> new_letters = null;
        if (is_correct)
        {
            new_letters = letter_controller_.GenerateLetters(word);
        }
        for (int i = 0; i < letters.Count; i++)
        {
            if (is_correct)
            {
                letters[i].AssignLetter(new_letters[i]);
            }
            else
            {
                letters[i].OnWrongLetter();
            }
        }
        if (is_correct)
        {
            on_correct_word?.Invoke();
        }
        else
        {
            on_wrong_word?.Invoke();
        }

        letters.Clear();
    }

    public void OnMouseDrag()
    {
        if (cooldown > 0f)
        {
            return;
        }
        if (is_main)
        {
            var last_letter_holder = letters.Last();
            var line = last_letter_holder.transform.Find("Line");
            Vector2 holder_position = last_letter_holder.transform.position;
            Vector2 cursor_position = main_camera_.ScreenToWorldPoint(Input.mousePosition);
            var distance = (cursor_position - holder_position).magnitude;
            line.localScale = new Vector3(0.1f, distance, 1);
            line.position = (holder_position + cursor_position) / 2;
            float rotation = Vector2.SignedAngle(Vector2.right, cursor_position - holder_position);
            line.rotation = Quaternion.AngleAxis(90 + rotation, Vector3.forward);
        }
    }

    public void OnMouseEnter()
    {
        if (cooldown > 0f)
        {
            return;
        }
        on_letter_chained?.Invoke(this);
    }

    public void AssignLetter(char letter)
    {
        var text = transform.Find("Letter").gameObject.GetComponent<TextMeshPro>();
        if (text.text.Length == 0)
        {
            text.text = letter.ToString().ToUpper();
        }
        else
        {
            green_flash = animation_time;
            if (is_infused)
            {
                is_infused = false;
            }
            else
            {
                float fire_infusion_chance = UpgradeManager.Instance.GetParam("fire_infusion_chance").value / 100f;
                float ice_infusion_chance = UpgradeManager.Instance.GetParam("ice_infusion_chance").value / 100f;
                float lightning_infusion_chance = UpgradeManager.Instance.GetParam("lightning_infusion_chance").value / 100f;
                float rand = UnityEngine.Random.Range(0f, 1f);
                if (rand < fire_infusion_chance)
                {
                    is_infused = true;
                    infusion_element = Element.Fire;
                }
                else if (rand < fire_infusion_chance + ice_infusion_chance)
                {
                    is_infused = true;
                    infusion_element = Element.Ice;
                }
                else if (rand < fire_infusion_chance + ice_infusion_chance + lightning_infusion_chance)
                {
                    is_infused = true;
                    infusion_element = Element.Lightning;
                }
            }
            StartCoroutine(ChangeLetter(letter));
        }
    }

    public void OnWrongLetter()
    {
        red_flash = animation_time;
    }

    public void Hint(LetterHolder other)
    {
        hint = 3f;
        var line = transform.Find("HintLine");
        Vector2 holder_position = transform.position;
        Vector2 cursor_position = other.transform.position;
        var distance = (cursor_position - holder_position).magnitude;
        line.localScale = new Vector3(0.1f, distance, 1);
        line.position = (holder_position + cursor_position) / 2;
        float rotation = Vector2.SignedAngle(Vector2.right, cursor_position - holder_position);
        line.rotation = Quaternion.AngleAxis(90 + rotation, Vector3.forward);
    }

    public bool IsHinting()
    {
        return hint > 0f;
    }

    private IEnumerator ChangeLetter(char letter)
    {
        yield return new WaitForSeconds(animation_time);
        var text = transform.Find("Letter").gameObject.GetComponent<TextMeshPro>();
        text.text = letter.ToString().ToUpper();
    }

    private void Update()
    {
        if (green_flash > 0f)
        {
            red_flash = 0f;
            green_flash -= Time.deltaTime;
            if (green_flash < 0f)
            {
                green_flash = 0f;
                transform.Find("Activation").gameObject.GetComponent<SpriteRenderer>().color = new Color(0f, 1f, 0f, 0f);
            }
            else
            {
                transform.Find("Activation").gameObject.GetComponent<SpriteRenderer>().color = new Color(0f, 1f, 0f, Mathf.Abs(Mathf.Sin(2f * Mathf.PI * green_flash / animation_time)));
            }
        }
        else if (red_flash > 0f)
        {
            red_flash -= Time.deltaTime;
            if (red_flash < 0f)
            {
                red_flash = 0f;
                transform.Find("Activation").gameObject.GetComponent<SpriteRenderer>().color = new Color(0f, 1f, 0f, 0f);
            }
            else
            {
                transform.Find("Activation").gameObject.GetComponent<SpriteRenderer>().color = new Color(1f, 0f, 0f, Mathf.Abs(Mathf.Sin(2f * Mathf.PI * red_flash / animation_time)));
            }
        }
        if (cooldown > 0f)
        {
            cooldown = Mathf.Max(0f, cooldown - Time.deltaTime);
        }
        if (hint > 0f)
        {
            hint -= Time.deltaTime;
            if (hint < 0f)
            {
                transform.Find("HintLine").gameObject.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0f);
                hint = 0f;
            }
            else
            {
                transform.Find("HintLine").gameObject.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0.1f + 0.4f * Mathf.Abs(Mathf.Sin(2f * Mathf.PI * hint / (2f * animation_time))));
            }
        }
        if (is_infused && infusion < 1f)
        {
            infusion = Mathf.Min(1f, infusion + Time.deltaTime);
            transform.Find(sphere_names_[infusion_element]).gameObject.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, infusion);
        }
        else if (!is_infused && infusion > 0f)
        {
            infusion = Mathf.Max(0f, infusion - Time.deltaTime);
            transform.Find(sphere_names_[infusion_element]).gameObject.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, infusion);
        }
    }
}