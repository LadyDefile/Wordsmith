﻿using Wordsmith.Data;

namespace Wordsmith.Helpers
{
    public class SpellChecker
    {
        /// <summary>
        /// Check the given string for any possible spelling mistakes.
        /// </summary>
        /// <param name="str">The string to be tested.</param>
        /// <returns>True if no spelling errors or all errors resolved.</returns>
        public static WordCorrection[] CheckString(string str)
        {
            // Take the string and split it into words, find any mistakes.
            string[] words = str.Split(" ");
            List<WordCorrection> results = new();

            for (int i = 0; i < words.Length; ++i)
            {
                // If it's a number, just skip it
                if (float.TryParse(words[i].Replace(",", ""), out float discard))
                    continue;

                string word = CleanWord(words[i].ToLower());
                if (word.Length < 1)
                    continue;

                if (word.EndsWith('-') && Wordsmith.Configuration.IgnoreWordsEndingInHyphen)
                    continue;

                string? result = Lang.WordList.FirstOrDefault(w => w.ToLower() == word || w.ToLower() == $"{word}'s");

                // Compare lowercase first then check again with capitalization
                if (result == null)
                {
                    // Found a potentially misspelled word.

                    // Check it as a possible numeral (i.e. 31st)
                    if (word.EndsWith("1st") ||
                        word.EndsWith("2nd") ||
                        word.EndsWith("3rd") ||
                        word.EndsWith("4th") ||
                        word.EndsWith("5th") ||
                        word.EndsWith("6th") ||
                        word.EndsWith("7th") ||
                        word.EndsWith("8th") ||
                        word.EndsWith("9th") ||
                        (word.EndsWith("0th") && word.Length > 3))
                    {

                        // It ends with the correct values to be a number but are there only
                        // numbers ahead of the last two letters. We can discard the float value
                        if (float.TryParse(word.Replace(",", "").Substring(0, word.Length - 2), out float val))
                            continue; // It was a number, so continue the loop.
                    }

                    // If we reached this code, we were not able to locate a proper match for the word.
                    // Add the index to the list.
                    results.Add(new() { Original = word, Index = i });
                }
            }

            // No errors so return.
            return results.ToArray();
        }

        /// <summary>
        /// Cleans the word of any punctuation marks that should not be at the beginning or end.
        /// i.e. "Hello." becomes Hello
        /// </summary>
        /// <param name="str"></param>
        /// <returns>Returns the word from of starting and ending punctuation and spaces.</returns>
        private static string CleanWord(string str)
        {
            // Remove white space at the beginning and end. There shouldn't be any but just in case.
            str = str.Trim();

            if (str.EndsWith("'s"))
                str = str[0..^2];

            // Loop
            do
            {
                // If the string is now empty, return an empty string.
                if (str.Length < 1)
                    break;

                // Check the start and end of the word against every character.
                bool doBreak = true;
                foreach (char c in Wordsmith.Configuration.PunctuationCleaningList)
                {
                    // Check the start of the string for the character
                    if(str.StartsWith(c))
                    {
                        // If the string starts with the symbol, remove the symbol and
                        // prevent exiting the loop.
                        str = str.Substring(1);
                        doBreak = false;
                    }

                    // If ignoring hyphen-ended words and the character is a hyphen, skip the 
                    // EndsWith check.
                    if (Wordsmith.Configuration.IgnoreWordsEndingInHyphen && c == '-')
                        continue;

                    // Check the ending of the string
                    if (str.EndsWith(c))
                    {
                        // Remove the last character and prevent loop breaking
                        str = str.Substring(0, str.Length - 1);
                        doBreak = false;
                    }
                }

                // If the break hasn't been prevented, break.
                if(doBreak)
                    break;
            } while (true);

            return str;
        }
    }
}
