using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.SceneManagement;
using static LetterHolder;

public class LetterController : MonoBehaviour
{
    class DAWG
    {
        private static Dictionary<char, float> letter_freq = new Dictionary<char, float>();
        private Dictionary<char, DAWG> nodes;
        private int value;
        private int word_count;

        public DAWG()
        {
            nodes = new Dictionary<char, DAWG>();
            value = word_count = 0;
        }

        public char RandomLetter(float difficulty)
        {
            while (true)
            {
                var elem = letter_freq.ElementAt(Random.Range(0, letter_freq.Count));
                if (Random.Range(0f, 1f) < elem.Value + difficulty)
                {
                    return elem.Key;
                }
            }
            
        }

        public void Load(BinaryReader stream, bool root = false)
        {
            nodes.Clear();
            value = word_count = 0;

            int count = stream.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                nodes[stream.ReadChar()] = new DAWG();
            }
            value = stream.ReadInt32();
            foreach (var node in nodes)
            {
                node.Value.Load(stream);
                word_count += node.Value.word_count;
                letter_freq[node.Key] = letter_freq.GetValueOrDefault(node.Key) + node.Value.word_count;
            }

            if (value > 0)
            {
                word_count += 1;
            }

            if (root)
            {
                Dictionary<char, float> true_freq = new Dictionary<char, float>();
                var max = letter_freq.Values.Max();
                foreach (var letter in letter_freq)
                {
                    true_freq[letter.Key] = letter.Value / max;
                }
                letter_freq = true_freq;
            }
        }

        public int Get(string key)
        {
            if (key.Length == 0)
            {
                return value;
            }
            if (!nodes.ContainsKey(key[0]))
            {
                return 0;
            }
            return nodes[key[0]].Get(key.Substring(1));
        }

        public int Count(Dictionary<char, int> letters, int max_freq)
        {
            int result = 0;
            foreach (var item in letters)
            {
                if (item.Value == 0)
                {
                    continue;
                }
                if (!nodes.ContainsKey(item.Key)) {
                    continue;
                }
                var new_vetters = new Dictionary<char, int>(letters);
                new_vetters[item.Key] -= 1;
                result += nodes[item.Key].Count(new_vetters, max_freq);
            }
            if (value <= max_freq)
            {
                result += 1;
            }
            return result;
        }

        private static int[] RandomPermutation(int n)
        {
            var result = new int[n];
            for (var i = 0; i < n; i++)
            {
                var j = Random.Range(0, i + 1);
                if (i != j)
                {
                    result[i] = result[j];
                }
                result[j] = i;
            }
            return result;
        }

        public bool RandomHint(Dictionary<char, int> letters, List<char> result)
        {
            if (value != 0)
            {
                return true;
            }

            var letter_array = new List<char>(letters.Keys);
            var order = RandomPermutation(letter_array.Count);
            foreach (var i in order)
            {
                if (letters[letter_array[order[i]]] == 0)
                {
                    continue;
                }
                if (!nodes.ContainsKey(letter_array[order[i]]))
                {
                    continue;
                }
                var new_vetters = new Dictionary<char, int>(letters);
                new_vetters[letter_array[order[i]]] -= 1;
                if (nodes[letter_array[order[i]]].RandomHint(new_vetters, result))
                {
                    result.Add(letter_array[order[i]]);
                    return true;
                }
            }
            return false;
        }
    };

    DAWG dawg = new DAWG();
    Dictionary<char, int> current_letters = new Dictionary<char, int>();

    float time_since_last_hint_ = 0f;
    float hint_interval_ = 5f;
    float hint_interval_delta_ = 1f;

    private void Awake()
    {
        string locale = LocalizationSettings.SelectedLocale.Identifier.Code;
        TextAsset asset = Resources.Load<TextAsset>("DAWG/dawg_" + locale);
        Stream stream = new MemoryStream(asset.bytes);
        BinaryReader reader = new BinaryReader(stream);
        dawg.Load(reader, true);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        LetterHolder[] letter_holders = FindObjectsOfType(typeof(LetterHolder)) as LetterHolder[];
        var letters = GenerateLetters(letter_holders.Length, true);
        for (int i = 0; i < letter_holders.Length; i++)
        {
            letter_holders[i].AssignLetter(letters[i]);
        }
    }

    public bool CheckWord(string word)
    {
        return dawg.Get(word) > 0;
    }

    private List<char> GetRandomLetters(int n, float letter_difficulty)
    {
        List<char> result = new List<char>();
        for (int i = 0; i < n; i++)
        {
            result.Add(dawg.RandomLetter(letter_difficulty));
        }
        return result;
    }

    private List<char> GenerateLetters(int letters_to_generate, bool is_initial = false)
    {
        int attempts = (int)UpgradeManager.Instance.GetParam("letter_generation_attempts").value;
        if (is_initial)
        {
            attempts *= 3;
        }
        int max_freq = (int)UpgradeManager.Instance.GetParam("word_max_frequency").value;
        float letter_difficulty = UpgradeManager.Instance.GetParam("letter_difficulty").value;
        int max_words = 0;
        List<char> result = null;
        for (int i = 0; i < attempts; i++)
        {
            var new_letters = GetRandomLetters(letters_to_generate, letter_difficulty);
            var new_dict = new Dictionary<char, int>(current_letters);
            foreach (char c in new_letters)
            {
                new_dict[c] = new_dict.GetValueOrDefault(c) + 1;
            }
            var word_count = dawg.Count(new_dict, max_freq);
            if (word_count > max_words)
            {
                max_words = word_count;
                result = new_letters;
            }
        }
        foreach (char c in result)
        {
            current_letters[c] = current_letters.GetValueOrDefault(c) + 1;
        }
        return result;
    }

    public List<char> GenerateLetters(string word_to_replace)
    {
        time_since_last_hint_ = -1f;
        foreach (char c in word_to_replace)
        {
            current_letters[c] -= 1;
            if (current_letters[c] == 0)
            {
                current_letters.Remove(c);
            }
        }
        return GenerateLetters(word_to_replace.Length);
    }

    private void Update()
    {
        time_since_last_hint_ += Time.deltaTime;
        float hint_frequency = UpgradeManager.Instance.GetParam("hint_frequency").value / 100f;
        if (time_since_last_hint_ >= hint_interval_ / hint_frequency)
        {
            time_since_last_hint_ = 0;
            hint_interval_ += hint_interval_delta_;
            List<char> hint = new List<char>();
            dawg.RandomHint(current_letters, hint);
            LetterHolder[] letter_holders = FindObjectsOfType(typeof(LetterHolder)) as LetterHolder[];
            LetterHolder prev = null;
            foreach (var target_letter in hint)
            {
                foreach (var letter_holder in letter_holders)
                {
                    var text = letter_holder.transform.Find("Letter").gameObject.GetComponent<TextMeshPro>();
                    char letter = text.text.ToLower()[0];
                    if ((letter != target_letter) || letter_holder.IsHinting())
                    {
                        continue;
                    }
                    if (prev != null)
                    {
                        letter_holder.Hint(prev);
                    }
                    prev = letter_holder;
                    break;
                }
            }
        }
    }
}
