using System.Text.RegularExpressions;

static partial class Processing
{
    private static async Task<string> GetWiktionaryPage(string word)
    {
        word = word.ToLower();
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

    private static async Task<string> GetWordFromWiki(string word)
    {
        var regexWord = @"(?<=</span><span class=""mw-headline"" id=""Русский"">Русский</span>)[\s\S]*?</td>\s*</tr>\s*</tbody>\s*</table>\s*<p>\s*<b>(.*)</b>";
        var wikiPage = await GetWiktionaryPage(word);
        var t = Regex.Match(wikiPage, regexWord);
        var groups = t.Groups.Values.ToArray();
        try
        {
            return groups[1].Value;
        }
        catch
        {
            regexWord = @"(?<=</span><span class=""mw-headline"" id=""Русский"">Русский</span>)[\s\S]*?<p>\s*<b>(.*)</b>";
            wikiPage = await GetWiktionaryPage(word);
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

    private static async Task<string[]> GetSyllables(string word)
    {
        const string regexSyllables = @"((.+)<span.*>.</span>)?(.+)";
        var weirdString = await GetWordFromWiki(word);
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

    private static async Task<string> GetWordWithStress(string word)
    {
        return string.Join("",await GetSyllables(word));
    }
    
    private static async Task<int> GetStressedLetter(string word)
    {
        var r = (await GetWordWithStress(word)).IndexOf((char)769);
        if (r > 0)
        {
            return r - 1;
        }
        r = (await GetWordWithStress(word)).IndexOf('ё');
        if (r > 0)
        {
            return r;
        }
        const string vowels = "аеёиоуыэюя";
        for (var i = 0; i < word.Length; i++)
        {
            if (vowels.Contains(word[i]))
            {
                return i;
            }
        }

        return -1;
    }

    private static async Task<string> GetWord(string word)
    {
        var w = await GetWordWithStress(word);
        var l = await GetStressedLetter(word);
        return w.Contains((char)769) ? w[..(l+1)] + w[(l+2)..] : w;
    }

    private static async Task<List<int>> GetWordAsArray(string word) //1 vowel, 0 consonant, -1 stressed vowel
    {
        const string vowels = "аеёиоуыэюя";
        var res = new List<int>();
        foreach (var i in await GetWord(word))
        {
            res.Add(vowels.Contains(i) ? 1 : 0);
        }
        res[await GetStressedLetter(word)] = -1;
        return res;
    }

    private static async Task<int> SyllablesToLeft(string word)
    {
        var t = await GetWordAsArray(word);
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

    private static async Task<int> SyllablesToRight(string word)
    {
        return (await GetSyllables(word)).Length - await SyllablesToLeft(word) - 1;
    }
}