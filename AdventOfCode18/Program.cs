using System;
using System.Collections.Generic;
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
                using (StreamReader sr = new StreamReader("../../../D1.txt"))
                {
                    Stopwatch sw = new Stopwatch();
                    sw.Start();
                    var answer = Day1(sr.ReadToEnd());
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
    }
}
