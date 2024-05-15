using System;
using System.Collections.Generic;
using System.Text;

namespace TrieAutocomplete;

class Node
{
    public Node[] children = new Node[29];
    public bool isWordEnd = false;
}

class Trie
{
    Node root;

    private static readonly char[] harfler =
    "abcçdefgğhıijklmnoöprsştuüvyz".ToCharArray();

    // Create a dictionary to map each character to an index
    private static readonly Dictionary<char, int> harf_index = harfler
        .Select((c, i) => new { c, i })
        .ToDictionary(ci => ci.c, ci => ci.i);

    public Trie()
    {
        root = new Node();
    }

    // If not present, inserts key into trie.  If the
    // key is prefix of trie node, just marks leaf node
    public void Insert(string key)
    {
        Node pCrawl = root;
        for (int i = 0; i < key.Length; i++)
        {
            //int index = key[i] - 'a';
            int index = harf_index[key[i]];
            if (pCrawl.children[index] == null)
            {
                pCrawl.children[index] = new Node();
            }
            pCrawl = pCrawl.children[index];
        }
        // mark last node as leaf
        pCrawl.isWordEnd = true;
    }

    // Returns 0 if current node has a child
    // If all children are NULL, return 1.
    bool IsLastNode(Node root)
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

    // Recursive function to print auto-suggestions for given
    // node.
    void suggestionsRec(Node root, string currPrefix, ref int limit, ref List<string> suggestions)
    {
        if (limit <= 0) return;
        // found a string in Trie with the given prefix
        if (root.isWordEnd)
        {
            limit--;
            suggestions.Add(currPrefix);
        }
        for (int i = 0; i < 29; i++)
        {
            if (root.children[i] != null)
            {
                // child node character value
                char c = harfler[i];
                suggestionsRec(root.children[i], currPrefix + c, ref limit, ref suggestions);
            }
        }
    }

    // print suggestions for given query prefix.
    public int printAutoSuggestions(string query, int limit, ref List<string> suggestions)
    {
        Node pCrawl = root;
        for (int i = 0; i < query.Length; i++)
        {
            //int index = query[i] - 'a';
            int index = harf_index[query[i]];
            // no string in the Trie has this prefix
            if (pCrawl.children[index] == null)
            {
                return 0;
            }
            pCrawl = pCrawl.children[index];
        }
        // If prefix is present as a word, but
        // there is no subtree below the last
        // matching node.
        if (IsLastNode(pCrawl))
        {
            Console.WriteLine(query);
            return -1;
        }
        suggestionsRec(pCrawl, query, ref limit, ref suggestions);
        return 1;
    }
}

// Driver Code
class Program
{
    static void Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.InputEncoding = Encoding.UTF8;

        Trie trie = new Trie();
        Console.WriteLine("Otomatik metin tamamlama uygulaması");
        Console.WriteLine("3 milyon kelime trie veri yapısına ekleniyor, lüften bekleyiniz...");
        string filePath = "words.txt";
        using (StreamReader reader = new StreamReader(filePath, Encoding.GetEncoding("utf-8")))
        {
            string line;

            while ((line = reader.ReadLine()) != null)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                string[] words = line.Split(' ');

                foreach (string word in words)
                {
                    trie.Insert(word);
                }
            }
        }

        List<string> inputText = new List<string>();
        int text_length = 0;
        List<string> suggestions = new List<string>();
        string current_word = "";

        while (true)
        {
            char c = Console.ReadKey().KeyChar;

            if (c == ' ' || c == '\n') {
                current_word = "";
                inputText.Add(" ");
                inputText.Add("");
                text_length++;
            }
            else
            {
                if (c == (char)8) // backspace
                {
                    if(current_word.Length > 0)
                    {
                        current_word = current_word.Substring(0, current_word.Length - 1);
                        text_length--;
                    }
                }else if (char.IsDigit(c)) // accepting suggestion
                {
                    suggestions.Clear();
                    trie.printAutoSuggestions(current_word, 9, ref suggestions);
                    int index = c - '0';
                    if(suggestions.Count >= index && index > 0)
                    {
                        text_length += suggestions[index - 1].Length - current_word.Length;
                        current_word = suggestions[index - 1];
                    }
                }
                else
                {
                    current_word += c;
                    text_length++;
                }

                if(inputText.Count > 0)
                {
                    inputText.RemoveAt(inputText.Count - 1);
                }
                inputText.Add(current_word);

                // yazdir
                Console.Clear();
                Console.WriteLine("Metini yazın");
                Console.WriteLine("Metini yazın, bir öneri almak için ilgili numarayı basın");
                foreach (var str in inputText) Console.Write(str);
                Console.WriteLine("\nÖneriler");

                suggestions.Clear();

                trie.printAutoSuggestions(current_word, 9, ref suggestions);

                for(int i = 0; i < suggestions.Count; i++)
                {
                    Console.WriteLine($"{i + 1}. {suggestions[i]}");
                }
                Console.SetCursorPosition(text_length, 1);
            }
        }
    }
}
