static partial class Processing
{
    public static async Task<string> Reduplicate(string word)
    {
        word = TrimBrackets(word);
        var checkBased = BasedAnswers(word);
        if (checkBased != "")
        {
            return checkBased;
        }

        var syllables = await GetSyllables(word);
        var stressedSyllable = await GetStressedSyllable(word);
        var syllsToLeft = stressedSyllable;
        var stressedLetter = await GetStressedLetter(word);
        var newLetter = word[stressedLetter] switch
        {
            'а' => 'я',
            'о' => 'ё',
            'у' => 'ю',
            'ы' => 'и',
            _ => word[stressedLetter]
        };
        if (syllsToLeft > 0)
        {
            var previous = syllables[stressedSyllable - 1];
            var w = syllables.Select(x => new string(x.Where(y => (int)y != 769).ToArray())).ToArray();
            return "хуе" +
                   previous[(GetVowel(previous) + 1)..] +
                   string.Join("", w[stressedSyllable..]);
        }
        else
        {
            return "ху" +
                   newLetter +
                   word[(stressedLetter + 1)..];
        }
    }

    private static string TrimBrackets(string word)
    {
        word = word.Trim();
        if (word[^1] == ')')
        {
            while (word[^1] == ')')
            {
                word = word[..^1];
            }
        }
        else if (word[^1] == '(')
        {
            while (word[^1] == '(')
            {
                word = word[..^1];
            }
        }

        return word;
    }
}