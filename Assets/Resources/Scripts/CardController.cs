using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;

public class CardController : MonoBehaviour
{
    [SerializeField]
    private List<GameObject> card_slots_;

    [SerializeField]
    private GameObject deck_;

    List<Card> active_cards_ = new List<Card>();
    float time_since_last_hint_ = 0f;
    float hint_interval_ = 4f;
    float hint_interval_delta_ = 0.75f;

    void OnCardClick(GameObject game_object)
    {
        Card card = game_object.GetComponent<Card>();
        if (card.IsActive())
        {
            active_cards_.Remove(card);
            card.Deactivate();
        }
        else
        {
            active_cards_.Add(card);
            card.Activate();
        }
        if (active_cards_.Count == 3)
        {
            if (IsSet(active_cards_[0].GetCode(), active_cards_[1].GetCode(), active_cards_[2].GetCode()))
            {
                active_cards_[0].Disappear();
                active_cards_[1].Disappear();
                active_cards_[2].Disappear();
                time_since_last_hint_ = 0f;
            }
            else
            {
                active_cards_[0].Flash();
                active_cards_[1].Flash();
                active_cards_[2].Flash();
            }
            active_cards_.Clear();
        }
    }

    private void OnEnable()
    {
        Card.on_card_click += OnCardClick;
    }

    private void OnDisable()
    {
        Card.on_card_click -= OnCardClick;
    }

    private bool IsSet(int a, int b, int c)
    {
        return ((a + b + c) % 3 == 0) && ((a / 3 + b / 3 + c / 3) % 3 == 0) && ((a / 9 + b / 9 + c / 9) % 3 == 0);
    }

    private bool HasSet(List<int> card_codes_1, List<int> card_codes_2)
    {
        List<int> card_codes = new List<int>();
        card_codes.AddRange(card_codes_1);
        card_codes.AddRange(card_codes_2);
        for (int i = 0; i < card_codes.Count; i++)
        {
            for (int j = i + 1; j < card_codes.Count; j++)
            {
                for (int k = j + 1; k < card_codes.Count; k++)
                {
                    if (IsSet(card_codes[i], card_codes[j], card_codes[k]))
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    void AddCards()
    {
        List<int> current_cards = new List<int>();
        foreach (GameObject card_slot in card_slots_)
        {
            if (card_slot.transform.childCount == 1)
            {
                current_cards.Add(card_slot.transform.GetChild(0).GetComponent<Card>().GetCode());
            }
        }
        if (current_cards.Count == card_slots_.Count)
        {
            return;
        }
        while (true)
        {
            List<GameObject> new_cards = new List<GameObject>();
            List<int> taken_cards = new List<int>();
            while (current_cards.Count + new_cards.Count < card_slots_.Count)
            {
                int new_card;
                do
                {
                    new_card = Random.Range(0, deck_.transform.childCount);
                }
                while (taken_cards.Contains(new_card));
                taken_cards.Add(new_card);
                new_cards.Add(deck_.transform.GetChild(new_card).gameObject);
            }
            List<int> new_card_codes = new List<int>();
            foreach (GameObject card in new_cards)
            {
                new_card_codes.Add(card.GetComponent<Card>().GetCode());
            }
            if (!HasSet(current_cards, new_card_codes))
            {
                continue;
            }
            int next_i = 0;
            foreach (GameObject card_slot in card_slots_)
            {
                if (card_slot.transform.childCount == 0)
                {
                    new_cards[next_i].transform.SetParent(card_slot.transform, false);
                    next_i++;
                }
            }
            break;
        }
    }

    void CheckHint()
    {
        time_since_last_hint_ += Time.deltaTime;
        float hint_frequency = UpgradeManager.Instance.GetParam("hint_frequency").value / 100f;
        if (time_since_last_hint_ >= hint_interval_ / hint_frequency)
        {
            time_since_last_hint_ = 0;
            hint_interval_ += hint_interval_delta_;
            for (int i = 0; i < card_slots_.Count; i++)
            {
                Card card1 = card_slots_[i].transform.GetChild(0).gameObject.GetComponent<Card>();
                int code1 = card1.GetCode();
                for (int j = i + 1; j < card_slots_.Count; j++)
                {
                    Card card2 = card_slots_[j].transform.GetChild(0).gameObject.GetComponent<Card>();
                    int code2 = card2.GetCode();
                    for (int k = j + 1; k < card_slots_.Count; k++)
                    {
                        Card card3 = card_slots_[k].transform.GetChild(0).gameObject.GetComponent<Card>();
                        int code3 = card3.GetCode();
                        if (IsSet(code1, code2, code3))
                        {
                            card1.Hint();
                            card2.Hint();
                            card3.Hint();
                            return;
                        }
                    }
                }
            }
        }
    }

    private void Update()
    {
        AddCards();
        CheckHint();
    }
}
