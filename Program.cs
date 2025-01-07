using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace TrieAutocomplete;

class Node
{
    public Node[] children = new Node[29];
    public bool isEndOfWord = false;
}

class Trie
{
    Node root;

    private static readonly char[] letters =
    "abcçdefgğhıijklmnoöprsştuüvyz".ToCharArray();

    private static readonly Dictionary<char, int> letter_index = letters
        .Select((c, i) => new { c, i })
        .ToDictionary(ci => ci.c, ci => ci.i);

    public Trie()
    {
        root = new Node();
    }

    public void Add(string word)
    {
        Node temp = root;
        for (int i = 0; i < word.Length; i++)
        {
            int index = letter_index[word[i]];
            if (temp.children[index] == null)
            {
                temp.children[index] = new Node();
            }
            temp = temp.children[index];
        }
        temp.isEndOfWord = true;
    }

    bool IsLeafNode(Node root)
    {
        for (int i = 0; i < 29; i++)
        {
            if (root.children[i] != null)
            {
                return false;
            }
        }
        return true;
    }

    void RecursiveGetSuggestions(Node root, string prefix, ref int limit, ref List<string> suggestions)
    {
        if (limit <= 0) return;

        if (root.isEndOfWord)
        {
            limit--;
            suggestions.Add(prefix);
        }
        for (int i = 0; i < 29; i++)
        {
            if (root.children[i] != null)
            {
                char c = letters[i];
                RecursiveGetSuggestions(root.children[i], prefix + c, ref limit, ref suggestions);
            }
        }
    }

    public int GetSuggestions(string query, int limit, ref List<string> suggestions)
    {
        Node temp = root;
        for (int i = 0; i < query.Length; i++)
        {
            int index = letter_index[query[i]];

            if (temp.children[index] == null)
            {
                return 0;
            }
            temp = temp.children[index];
        }

        if (IsLeafNode(temp))
        {
            suggestions.Add(query);
            return -1;
        }
        RecursiveGetSuggestions(temp, query, ref limit, ref suggestions);
        return 1;
    }
}

class Program
{
    static void Main(string[] args)
    {
        var textCompletion = new TextCompletion();
        textCompletion.Start();
    }
}

class TextCompletion
{
    private Trie trie = new Trie();
    List<string> text = new List<string>();
    int text_length = 0;
    List<string> suggestions = new List<string>();
    string current_word = "";
    Dictionary<string, int> frequency = new Dictionary<string, int>();

    public void Start()
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.InputEncoding = Encoding.UTF8;

        ReadWordsFile();
        ReadFrequencyTable();
        Console.Clear();
        Console.WriteLine("Type text, press the corresponding number to select a suggestion");
        StartWriting();
    }

    private void ReadWordsFile()
    {
        string file = "words.txt";
        int wordCount = 0;

        using (StreamReader reader = new StreamReader(file, Encoding.GetEncoding("utf-8")))
        {
            string line;

            while ((line = reader.ReadLine()) != null)
            {
                if (wordCount % 100000 == 0)
                {
                    Console.Clear();
                    Console.WriteLine("Automatic text completion application");
                    Console.WriteLine("Adding 3.2 million words to trie data structure, please wait...");
                    Console.WriteLine($"{(int)(wordCount / 3284900.0f * 100)}% added");
                }
                trie.Add(line.Replace("\n", ""));
                wordCount++;
            }
        }
    }

    private void ReadFrequencyTable()
    {
        int wordCount = 0;
        using (StreamReader reader = new StreamReader("frequency.txt"))
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                if (wordCount % 100000 == 0)
                {
                    Console.Clear();
                    Console.WriteLine("Automatic text completion application");
                    Console.WriteLine("Adding 3.2 million words to hash table, please wait...");
                    Console.WriteLine($"{(int)(wordCount / 2035629.0f * 100)}% added");
                }
                string[] array = line.Split(' ');

                string word = array[0];
                int freq = int.Parse(array[1]);

                frequency.Add(word, freq);
                wordCount++;
            }
        }
    }

    private void StartWriting()
    {
        while (true)
        {
            char c = Console.ReadKey().KeyChar;

            if (c == ' ' || c == '\n')
            {
                MoveToNextWord();
            }
            else
            {
                ProcessInput(c);
                PrintSuggestions();
            }
        }
    }

    private void MoveToNextWord()
    {
        current_word = "";
        text.Add(" ");
        text.Add("");
        text_length++;
    }

    private void ProcessInput(char c)
    {
        if (c == (char)8) // delete last character
        {
            if (current_word.Length > 0)
            {
                current_word = current_word.Substring(0, current_word.Length - 1);
                text_length--;
            }
        }
        else if (char.IsDigit(c)) // select suggestion
        {
            int index = c - '0';
            if (index > 0 && index <= 9)
            {
                try
                {
                    text_length += suggestions[index - 1].Length - current_word.Length;
                    current_word = suggestions[index - 1];
                }
                catch (Exception ex)
                {
                    ;
                }
            }
        }
        else
        {
            current_word += c;
            text_length++;
        }

        if (text.Count > 0)
        {
            text.RemoveAt(text.Count - 1);
        }
        text.Add(current_word);
    }

    private void PrintSuggestions()
    {
        Console.Clear();
        Console.WriteLine("Type text, press the corresponding number to select a suggestion");
        foreach (var str in text) Console.Write(str);
        Console.WriteLine("\nSuggestions");

        suggestions.Clear();
        trie.GetSuggestions(current_word, 10000, ref suggestions);

        suggestions.Sort((w1, w2) =>
        {
            int freq1 = frequency.ContainsKey(w1) ? frequency[w1] : 0;
            int freq2 = frequency.ContainsKey(w2) ? frequency[w2] : 0;
            return freq2.CompareTo(freq1);
        });

        int suggestion_count = suggestions.Count;
        if (suggestion_count > 10) suggestion_count = 10;
        for (int i = 0; i < suggestion_count; i++)
        {
            Console.WriteLine($"{i + 1}. {suggestions[i]}");
        }
        if (suggestions.Count == 0) Console.WriteLine("No suggestions found");
        Console.SetCursorPosition(text_length, 1);
    }
}