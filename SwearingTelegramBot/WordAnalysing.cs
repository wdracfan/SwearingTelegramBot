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
        const string regexWord = @"(?<=</span><span class=""mw-headline"" id=""Русский"">Русский</span>)[\s\S]*?</td>\s*</tr>\s*</tbody>\s*</table>\s*<p>\s*<b>(.*)</b>";
        var wikiPage = await GetWiktionaryPage(word);
        var t = Regex.Match(wikiPage, regexWord);
        var groups = t.Groups.Values.ToArray();
        try
        {
            return groups[1].Value;
        }
        catch
        {
            throw new ArgumentException("Failed to parse wiki");
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

    private static async Task<int> GetStressedLetter(string word)
    {
        var syllables = await GetSyllables(word);
        var stressedWord = string.Join("",syllables);
        var stress = stressedWord.IndexOf((char)769);
        if (stress != -1)
        {
            return stress - 1;
        }

        stress = stressedWord.IndexOf('ё');
        if (stress != -1)
        {
            return stress;
        }

        return GetVowel(stressedWord);
    }

    private static async Task<int> GetStressedSyllable(string word)
    {
        var syllables = await GetSyllables(word);
        for (var i = 0; i < syllables.Length; i++)
        {
            if (syllables[i].Contains((char)769))
            {
                return i;
            }
        }
        
        for (var i = 0; i < syllables.Length; i++)
        {
            if (syllables[i].Contains('ё'))
            {
                return i;
            }
        }

        return 0;
    }

    private static int GetVowel(string syllable)
    {
        var vowels = new char[] {'а', 'е', 'ё', 'и', 'о', 'у', 'ы', 'э', 'ю', 'я'};
        for (var i = 0; i <= syllable.Length; i++)
        {
            if (vowels.Contains(syllable[i]))
            {
                return i;
            }
        }

        return -1;
    }
}