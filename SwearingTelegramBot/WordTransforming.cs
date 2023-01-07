using System.Diagnostics;

public partial class Processing
{
    public static string Reduplicate(string word)
    {
        var time = new Stopwatch(); time.Reset(); time.Start();
        
        word = TrimBrackets(word);
        if (word.Contains(' '))
        {
            throw new ArgumentException("Several words in the input");
        }
        
        var checkBased = BasedAnswers(word);
        if (checkBased != "")
        {
            return checkBased;
        }

        var wordW = new Word(word);

        if (word != wordW.WordW)
        {
            throw new FormatException("Probably plural");
        }

        var right = wordW.Right;
        var left = wordW.Left;
        
        //сколько "слогов" справа оставляем
        int syllables = right + 1 + (left >= 2 ? 1 : 0);
        //берём ли согласные от предыдущего "слога"
        bool middle = left >= 2 || (left == 1 && right == 0);

        var cnt = 0;
        var arr = wordW.WordAsArray;
        int index = -1; //индекс буквы, начиная с которой берём концовку
        for (var i = word.Length - 1; i >= 0; i--)
        {
            if (arr[i] != 0)
            {
                cnt++;
                if (middle is false && cnt == syllables)
                {
                    index = i;
                    break;
                }
                if (middle && cnt == syllables + 1)
                {
                    index = i + 1;
                    break;
                }
            }
        }

        string start;
        if (middle)
        {
            start = word[index] switch
            {
                'а' => "хуя",
                'о' => "хуё",
                'у' => "хую",
                'ы' => "хуи",
                'е' or 'ё' or 'и' or 'э' or 'ю' or 'я' => "ху" + word[index],
                'й' => "ху" + word[index], 
                _ when arr[index+1] != 0 => "хуй" + word[index],
                _ => "хуе" + word[index]
            };
        }
        else
        {
            start = word[index] switch
            {
                'а' => "хуя",
                'о' => "хуё",
                'у' => "хую",
                'ы' => "хуи",
                _ => "ху" + word[index]
            };
        }
        
        var res = start + word[(index+1)..];
        
        time.Stop(); Console.WriteLine($"Reduplicate {time.Elapsed}");
        return res;
    }

    private static string TrimBrackets(string word)
    {
        var time = new Stopwatch(); time.Reset(); time.Start();
        
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
        
        time.Stop(); Console.WriteLine($"TrimBrackets {time.Elapsed}");
        return word;
    }
}