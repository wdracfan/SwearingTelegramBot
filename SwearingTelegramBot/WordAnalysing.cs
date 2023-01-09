using System.Text.RegularExpressions;

public class Word
{
    private string InputWord { get; }
    private string[] Syllables { get; }
    private string WordWithStress { get; }
    private int StressedLetter { get; }
    public string WordW { get; }
    public List<int> WordAsArray { get; }
    public int Left { get; }
    public int Right { get; }
    
    public Word(string word)
    {
        try
        {
            InputWord = word;
            Syllables = GetSyllables().Result;
            WordWithStress = GetWordWithStress();
            StressedLetter = GetStressedLetter();
            WordW = GetWord();
            WordAsArray = GetWordAsArray();
            Left = SyllablesToLeft();
            Right = SyllablesToRight();
        }
        catch (AggregateException ae)
        {
            throw new ArgumentException(ae.InnerException?.Message, ae.InnerException);
        }
    }
    
    private async Task<string> GetWiktionaryPage()
    { 
        var word = InputWord.ToLower();
        var http = new HttpClient();
        try
        {
            var resp = await http.GetAsync($"https://ru.wiktionary.org/wiki/{word}#Русский");
            var text = await resp.Content.ReadAsStringAsync();
            return text;
        }
        catch
        {
            throw new ArgumentException("Incorrect word");
        }
    }

    private async Task<string> GetWordFromWiki()
    {
        var regexWord = @"(?<=</span><span class=""mw-headline"" id=""Русский"">Русский</span>)[\s\S]*?</td>\s*</tr>\s*</tbody>\s*</table>\s*<p>\s*<b>(.*)</b>";
        var wikiPage = await GetWiktionaryPage();
        var t = Regex.Match(wikiPage, regexWord);
        var groups = t.Groups.Values.ToArray();
        try
        {
            return groups[1].Value;
        }
        catch
        {
            regexWord =
                @"(?<=</span><span class=""mw-headline"" id=""Русский"">Русский</span>)[\s\S]*?<p>\s*<b>(.*)</b>";
            t = Regex.Match(wikiPage, regexWord);
            groups = t.Groups.Values.ToArray();
            try
            {
                return groups[1].Value;
            }
            catch
            {
                throw new ArgumentException("Failed to parse wiki");
            }
        }
    }

    private async Task<string[]> GetSyllables()
    {
        const string regexSyllables = @"((.+)<span.*>.</span>)?(.+)";
        var weirdString = await GetWordFromWiki();
        var rest = weirdString;
        var syllables = new List<string>();
        try
        {
            while (rest != "")
            {
                var t = Regex.Match(rest, regexSyllables);
                var groups = t.Groups.Values.ToArray();
                syllables.Add(groups[3].Value);
                rest = groups[2].Value;
            }
        }
        catch
        {
            throw new ArgumentException("Failed to get syllables");
        }
        syllables.Reverse();
        return syllables.ToArray();
    }

    private string GetWordWithStress()
    {
        var res = string.Join("",Syllables);
        return res;
    }
    
    private int GetStressedLetter()
    {
        var w = WordWithStress;
        var r = w.IndexOf((char)769);
        if (r > 0)
        {
            return r - 1;
        }
        r = w.IndexOf('ё');
        if (r > 0)
        {
            return r;
        }
        const string vowels = "аеёиоуыэюя";
        for (var i = 0; i < InputWord.Length; i++)
        {
            if (vowels.Contains(InputWord[i]))
            {
                return i;
            }
        }

        return -1;
    }

    private string GetWord()
    {
        var w = WordWithStress;
        var l = StressedLetter;
        var res = w.Contains((char)769) ? w[..(l+1)] + w[(l+2)..] : w;
        return res;
    }

    private List<int> GetWordAsArray() //1 vowel, 0 consonant, -1 stressed vowel
    {
        const string vowels = "аеёиоуыэюя";
        var res = new List<int>();
        foreach (var i in WordW)
        {
            res.Add(vowels.Contains(i) ? 1 : 0);
        }
        res[StressedLetter] = -1;
        return res;
    }

    private int SyllablesToLeft()
    {
        var t = WordAsArray;
        var res = 0;
        foreach (var i in t)
        {
            if (i == 1)
            {
                res++;
            }
            else if (i == -1)
            {
                break;
            }
        }
        return res;
    }

    private int SyllablesToRight()
    {
        var res = WordAsArray.Count(x => x != 0) - Left - 1;
        return res;
    }
}