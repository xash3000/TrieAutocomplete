using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace TrieAutocomplete;

class Dugum
{
    public Dugum[] cocuklar = new Dugum[29];
    public bool kelimeSonu = false;
}

class Trie
{
    Dugum root;

    private static readonly char[] harfler =
    "abcçdefgğhıijklmnoöprsştuüvyz".ToCharArray();

    private static readonly Dictionary<char, int> harf_index = harfler
        .Select((c, i) => new { c, i })
        .ToDictionary(ci => ci.c, ci => ci.i);

    public Trie()
    {
        root = new Dugum();
    }

    
    public void Ekle(string kelime)
    {
        Dugum temp = root;
        for (int i = 0; i < kelime.Length; i++)
        {
            int index = harf_index[kelime[i]];
            if (temp.cocuklar[index] == null)
            {
                temp.cocuklar[index] = new Dugum();
            }
            temp = temp.cocuklar[index];
        }
        temp.kelimeSonu = true;
    }

    bool SonDugum(Dugum root)
    {
        for (int i = 0; i < 29; i++)
        {
            if (root.cocuklar[i] != null)
            {
                return false;
            }
        }
        return true;
    }

    void RecursiveOneriAl(Dugum root, string prefix, ref int limit, ref List<string> oneriler)
    {
        if (limit <= 0) return;
        
        if (root.kelimeSonu)
        {
            limit--;
            oneriler.Add(prefix);
        }
        for (int i = 0; i < 29; i++)
        {
            if (root.cocuklar[i] != null)
            {
                char c = harfler[i];
                RecursiveOneriAl(root.cocuklar[i], prefix + c, ref limit, ref oneriler);
            }
        }
    }

    public int OneriAl(string sorgu, int limit, ref List<string> oneriler)
    {
        Dugum temp = root;
        for (int i = 0; i < sorgu.Length; i++)
        {
            
            int index = harf_index[sorgu[i]];
            
            if (temp.cocuklar[index] == null)
            {
                return 0;
            }
            temp = temp.cocuklar[index];
        }
       
        if (SonDugum(temp))
        {
            oneriler.Add(sorgu);
            return -1;
        }
        RecursiveOneriAl(temp, sorgu, ref limit, ref oneriler);
        return 1;
    }
}


class Program
{

    static void Main(string[] args)
    {
        var metinTamamlama = new MetinTamamlama();
        metinTamamlama.basla();
    }
}

class MetinTamamlama
{
    private Trie trie = new Trie();
    List<string> metin = new List<string>();
    int metin_uzunlugu = 0;
    List<string> oneriler = new List<string>();
    string aktif_kelime = "";
    Dictionary<string, int> frekans = new Dictionary<string, int>();

    public void basla()
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.InputEncoding = Encoding.UTF8;

        KelimelerDosyasiOku();
        FrekansTablosuOku();
        Console.Clear();
        Console.WriteLine("Metini yazın, bir öneriyi secmek için ilgili numarayı basın");
        YazmayaBasla();
    }

    private void KelimelerDosyasiOku()
    {
        string dosya = "words.txt";
        int kelimeSayisi = 0;
        
        using (StreamReader reader = new StreamReader(dosya, Encoding.GetEncoding("utf-8")))
        {
            string satir;

            while ((satir = reader.ReadLine()) != null)
            {
                if (kelimeSayisi % 100000 == 0)
                {
                    Console.Clear();
                    Console.WriteLine("Otomatik metin tamamlama uygulaması");
                    Console.WriteLine("3.2 milyon kelime trie veri yapısına ekleniyor, lüften bekleyiniz...");
                    Console.WriteLine($"{(int)(kelimeSayisi/3284900.0f * 100)}% eklendi");
                }
                trie.Ekle(satir.Replace("\n", ""));
                kelimeSayisi++;
            }
        }
    }
    private void FrekansTablosuOku()
    {
        int kelimeSayisi = 0;
        using (StreamReader reader = new StreamReader("frekanslar.txt"))
        {
            string satir;
            while ((satir = reader.ReadLine()) != null)
            {
                if (kelimeSayisi % 100000 == 0)
                {
                    Console.Clear();
                    Console.WriteLine("Otomatik metin tamamlama uygulaması");
                    Console.WriteLine("3.2 milyon kelime karma tablosuna ekleniyor, lüften bekleyiniz...");
                    Console.WriteLine($"{(int)(kelimeSayisi / 2035629.0f * 100)}% eklendi");
                }
                string[] dizi = satir.Split(' ');

                string kelime = dizi[0];
                int siklik = int.Parse(dizi[1]);

                frekans.Add(kelime, siklik);
                kelimeSayisi++;
            }
        }
    }
    private void YazmayaBasla()
    {
        while (true)
        {
            char c = Console.ReadKey().KeyChar;

            if (c == ' ' || c == '\n')
            {
                YineKelimeyeGec();
            }
            else
            {
                IslemYap(c);
                OnerileriYazdir();
            }
        }
    }

    private void YineKelimeyeGec()
    {
        aktif_kelime = "";
        metin.Add(" ");
        metin.Add("");
        metin_uzunlugu++;
    }

    private void IslemYap(char c)
    {
        if (c == (char)8) // son harfı silmek
        {
            if (aktif_kelime.Length > 0)
            {
                aktif_kelime = aktif_kelime.Substring(0, aktif_kelime.Length - 1);
                metin_uzunlugu--;
            }
        }
        else if (char.IsDigit(c)) // öneri seçmek
        {
            int index = c - '0';
            if (index > 0 && index <= 9)
            {
                try
                {
                    metin_uzunlugu += oneriler[index - 1].Length - aktif_kelime.Length;
                    aktif_kelime = oneriler[index - 1];
                }
                catch (Exception ex)
                {
                    ;
                }
            }
        }else
        {
            aktif_kelime += c;
            metin_uzunlugu++;
        }

        if (metin.Count > 0)
        {
            metin.RemoveAt(metin.Count - 1);
        }
        metin.Add(aktif_kelime);
    }

    private void OnerileriYazdir()
    {
        Console.Clear();
        Console.WriteLine("Metini yazın, bir öneriyi secmek için ilgili numarayı basın");
        foreach (var str in metin) Console.Write(str);
        Console.WriteLine("\nÖneriler");

        oneriler.Clear();
        trie.OneriAl(aktif_kelime, 10000, ref oneriler);

        oneriler.Sort((k1, k2) =>
        {
            int frekans1 = frekans.ContainsKey(k1) ? frekans[k1] : 0;
            int frekans2 = frekans.ContainsKey(k2) ? frekans[k2] : 0;
            return frekans2.CompareTo(frekans1);
        });

        int oneri_sayisi = oneriler.Count;
        if (oneri_sayisi > 10) oneri_sayisi = 10;
        for (int i = 0; i < oneri_sayisi; i++)
        {
            Console.WriteLine($"{i + 1}. {oneriler[i]}");
        }
        if (oneriler.Count == 0) Console.WriteLine("Oneri bulunamadı");
        Console.SetCursorPosition(metin_uzunlugu, 1);
    }
}