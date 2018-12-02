


using System;
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace AdventOfCode18
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                using (StreamReader sr = new StreamReader("../../../D2.txt"))
                {
                    Stopwatch sw = new Stopwatch();
                    sw.Start();
                    var answer = Day2(sr.ReadToEnd());
                    sw.Stop();
                    Console.WriteLine($"Solution: {answer} [{sw.Elapsed}]");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("The file could not be read: " + e.Message);
            }
        }

        private static Tuple<int, int> Day1(string input)
        {
            var frequencies = input.Split(Environment.NewLine).ToList().Select(d => int.Parse(d.Substring(1)) * (d.First().Equals('-') ? -1 : 1));
            int p1 = frequencies.Sum();
            int p2 = 0;
            var previousFreqs = new List<int>();
            while (true)
            {
                foreach (int f in frequencies)
                {
                    if (previousFreqs.Contains(p2))
                        return Tuple.Create(p1, p2);
                    previousFreqs.Add(p2);
                    p2 += f;
                }
            }
        } // 4.656s

        private static Tuple<int, string> Day2(string input)
        {
            var boxIDs = input.Split(Environment.NewLine).ToList();
            int twice = 0,  thrice = 0;
            var charOccurences = new Dictionary<char, int>();
            foreach (string boxID in boxIDs)
            {
                charOccurences.Clear();
                foreach (char c in boxID)
                {
                    if (charOccurences.ContainsKey(c))
                        charOccurences[c]++;
                    else
                        charOccurences.Add(c, 1);
                }
                twice += charOccurences.Values.Contains(2) ? 1 : 0;
                thrice += charOccurences.Values.Contains(3) ? 1 : 0;
            }
            int p1 = twice * thrice, a = boxIDs.Count();
            var currLetters = new List<char>();
            for (int i = 0; i < a; i++)
            {
                for (int j = i+1; j < a; j++)
                {
                    currLetters.Clear();
                    int differ = 0, l = boxIDs[i].Count();
                    for (int k = 0; k < l; k++)
                    {
                        currLetters.Add(boxIDs[i][k]);
                        if (!boxIDs[i][k].Equals(boxIDs[j][k]))
                            differ++;
                        if (differ > 1)
                            break;
                    }
                    if (differ == 1)
                        return Tuple.Create(p1, string.Join("", currLetters));
                }
            }
            return null;
        } // 0.0155
    }
}
