
using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace AdventOfCode18
{
    class Program
    {
        static void Main(string[] args)
        {
            using (StreamReader sr = new StreamReader("../../../D11.txt"))
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                var answer = Day11(sr.ReadToEnd());
                sw.Stop();
                Console.WriteLine($"Solution: {answer} [{sw.Elapsed}]");
            }
        }

        private static Tuple<int, int> Day1(string input)
        {
            var frequencies = input.Split(Environment.NewLine).ToList().Select(d => int.Parse(d.Substring(1)) * (d.First().Equals('-') ? -1 : 1));
            int p1 = frequencies.Sum(), p2 = 0;
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
            int twice = 0, thrice = 0;
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
                for (int j = i + 1; j < a; j++)
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
        } // 0.0155s

        private static Tuple<int, int> Day3(string input)
        {
            var claims = input.Split(Environment.NewLine).ToList().Select(d => new List<string>(d.Split(" ")));
            var fabric = new Dictionary<Tuple<int, int>, List<int>>();
            int counter = 0;
            var ids = new List<int>();
            foreach (var claim in claims)
            {
                var id = int.Parse(claim.First().Remove(0, 1));
                ids.Add(id);
                var offset = claim.Skip(2).First().Remove(claim.Skip(2).First().Length - 1).Split(",").Select(d => int.Parse(d));
                var size = claim.Last().Split("x").Select(d => int.Parse(d));
                for (int i = offset.First(), l1 = offset.First() + size.First(); i < l1; i++)
                {
                    for (int j = offset.Last(), l2 = offset.Last() + size.Last(); j < l2; j++)
                    {
                        var currPos = new Tuple<int, int>(i, j);
                        if (fabric.ContainsKey(currPos))
                        {
                            if (fabric[currPos][1]++ == 1)
                            {
                                ids.Remove(fabric[currPos][0]);
                                counter++;
                            }
                            ids.Remove(id);
                        }
                        else
                            fabric.Add(currPos, new List<int>(new int[] { id, 1 }));
                    }
                }
            }
            return new Tuple<int, int>(counter, ids.First());
        } // 0.932s

        private static Tuple<int, int> Day4(string input)
        {
            var records = input.Split(Environment.NewLine).ToList().Select(d => d.Replace("[", "").Replace("]", "").Split(" "))
                .Select(d =>
                {
                    return new
                    {
                        date = DateTime.Parse(d[0] + " " + d[1]),
                        id = d[2].Equals("Guard") ? int.Parse(d[3].Substring(1)) : (int?)null,
                        sleeping = d[3].Equals("asleep")
                    };
                }).OrderBy(x => x.date).ToList();
            var currID = records[0].id.Value;
            var sleepSchedule = new Dictionary<int, int[]>();
            for (int i = 0, rCount = records.Count - 1; i < rCount; i++)
            {
                if (records[i + 1].id != null)
                {
                    currID = records[i + 1].id.Value;
                    continue;
                }
                if (!sleepSchedule.ContainsKey(currID))
                    sleepSchedule.Add(currID, new int[60]);
                if (records[i].sleeping)
                {
                    var minutes = (records[i + 1].date - records[i].date).Minutes;
                    for (int j = 0; j < minutes; j++)
                    {
                        var startSleeping = records[i].date.Minute;
                        sleepSchedule[currID][(startSleeping + j) % 60]++;
                    }
                }
            }
            int mostSleepyID = sleepSchedule.Aggregate((l, r) => l.Value.Sum() > r.Value.Sum() ? l : r).Key;
            int sleepiestMinute = sleepSchedule[mostSleepyID].ToList().IndexOf(sleepSchedule[mostSleepyID].Max());
            int p1 = mostSleepyID * sleepiestMinute;
            var sleepiestGuard = sleepSchedule.Aggregate((l, r) => l.Value.Max() > r.Value.Max() ? l : r);
            int p2 = sleepiestGuard.Key * sleepSchedule[sleepiestGuard.Key].ToList().IndexOf(sleepiestGuard.Value.Max());
            return new Tuple<int, int>(p1, p2);
        } // 0.011s

        private static Tuple<int, int> Day5(string input)
        {
            int react(string l)
            {
                for (int i = 0; i < l.Length - 1;)
                {
                    if (l[i] != l[i + 1] && l[i].ToString().Equals(l[i + 1].ToString(), StringComparison.InvariantCultureIgnoreCase))
                    {
                        l = l.Remove(i, 2);
                        i = Math.Max(0, i - 1);
                    }
                    else i++;
                }
                return l.Length;
            }
            var results = new Dictionary<string, int> { { "", react(input) } };
            foreach (string letter in input.Select(c => c.ToString().ToLower()).Distinct().ToList())
                results.Add(letter, react(input.Replace(letter, "").Replace(letter.ToUpper(), "")));
            return new Tuple<int, int>(results[""], results.Values.Min());
        } // 3.218s

        private static Tuple<int, int> Day6(string input)
        {
            var coords = input
                .Split(Environment.NewLine)
                .Select(line => line.Split(", ")
                    .Select(num => Convert.ToInt32(num))
                    .ToArray())
                .Select(l => (x: l[0], y: l[1])).ToArray();
            int rows = coords.Max(c => c.x), cols = coords.Max(c => c.y), safeCount = 0;
            var grid = new int[rows + 2, cols + 2];
            var excludeBorder = new List<int>();
            var counts = Enumerable.Range(-1, coords.Length + 1).ToDictionary(i => i, _ => 0);
            for (int x = 0; x <= rows + 1; x++)
            {
                for (int y = 0; y <= cols + 1; y++)
                {
                    var distances = coords
                        .Select((c, i) => (i, dist: Math.Abs(c.x - x) + Math.Abs(c.y - y)))
                        .OrderBy(c => c.dist)
                        .ToArray();
                    grid[x, y] = distances[1].dist != distances[0].dist ? distances[0].i : -1;
                    if (distances.Sum(c => c.dist) < 10000)
                        safeCount++;
                    if (x == 0 || y == 0 || x == rows + 1 || y == cols + 1)
                        excludeBorder.Add(grid[x, y]);
                    counts[grid[x, y]] += 1;
                }
            }
            excludeBorder = excludeBorder.Distinct().ToList();
            var p1 = counts
                .Where(pair => !excludeBorder.Contains(pair.Key))
                .OrderByDescending(pair => pair.Value)
                .ElementAt(0)
                .Value;
            return new Tuple<int, int>(p1, safeCount);
        } // 0.761s

        private static Tuple<string, int> Day7(string input)
        {
            var req = input
                .Split(Environment.NewLine)
                .Select(l => (pre: l[5] + "", post: l[36] + ""))
                .ToList();
            var letters = req
                .Select(s => new HashSet<string> { s.pre, s.post })
                .Aggregate((l, r) => l.Union(r).ToHashSet())
                .OrderBy(l => l)
                .ToList();
            var assigned = new List<string>();
            while (assigned.Count != letters.Count)
            {
                foreach (var l in letters)
                {
                    if (assigned.Contains(l))
                        continue;
                    var dependencies = req
                        .Where(s => s.post == l)
                        .Select(s => s.pre)
                        .ToList();
                    if (!dependencies.Except(assigned).Any())
                    {
                        assigned.Add(l);
                        break;
                    }
                }
            }
            var workers = new List<int>(5) { 0, 0, 0, 0, 0 };
            var counter = 0;
            var finishing = new List<(string s, int done)>();
            while (letters.Any() || workers.Any(w => w > counter))
            {
                finishing.Where(d => d.done <= counter).ToList().ForEach(x => req.RemoveAll(d => d.pre == x.s));
                finishing.RemoveAll(d => d.done <= counter);
                var firstFree = letters.Where(s => !req.Any(d => d.post == s)).ToList();
                for (var w = 0; w < workers.Count && firstFree.Any(); w++)
                {
                    if (workers[w] <= counter)
                    {
                        workers[w] = (firstFree.First()[0] - 'A') + 61 + counter;
                        letters.Remove(firstFree.First());
                        finishing.Add((firstFree.First(), workers[w]));
                        firstFree.RemoveAt(0);
                    }
                }
                counter++;
            }
            return new Tuple<string, int>(string.Join("", assigned), counter);
        } // 0.029s

        private static (int, int) Day8(string input)
        {
            var numbers = input.Split(" ").Select(d => int.Parse(d)).ToList();
            var nodes = new Dictionary<int, (List<int> childs, List<int> metas)>();
            (int, int) processNode(int idx, int name)
            {
                int currIdx = idx, currName = name + 1, amountChildren = numbers[currIdx], amountMeta = numbers[currIdx + 1];
                var children = new List<int>();
                currIdx += 2;
                for (int j = 0; j < amountChildren; j++)
                {
                    children.Add(currName);
                    var childOutput = processNode(currIdx, currName);
                    currIdx = childOutput.Item1;
                    currName = childOutput.Item2;
                }
                nodes.Add(name, (children, numbers.GetRange(currIdx, currIdx + amountMeta > numbers.Count ? amountMeta - 1 : amountMeta).ToList()));
                return (currIdx + amountMeta, currName);
            }
            processNode(0, 0);
            int nodeValue(int n)
            {
                if (nodes[n].childs.Count == 0)
                    return nodes[n].metas.Sum();
                else
                {
                    return nodes[n].metas
                        .Where(m => 0 < m && m <= nodes[n].childs.Count)
                        .Aggregate(0, (sum, m) => sum + nodeValue(nodes[n].childs[m - 1]));
                }
            }
            return (nodes.Values.Select(v => v.metas.Sum()).Sum(), nodeValue(0));
        } // 0.017s

        private static (double, double) Day9(string input)
        {
            int playerCount = 428; double marbleCount = 70825;
            double play(double limit)
            {
                var marbles = new LinkedList<double>();
                var curr = marbles.AddFirst(0);
                var players = new double[playerCount];
                int playerIdx = 0;
                for (double i = 1; i <= limit; i++)
                {
                    if (i % 23 == 0)
                    {
                        for (int j = 0; j < 6; j++)
                            curr = curr.Previous ?? marbles.Last;
                        players[playerIdx] += i + curr.Previous.Value;
                        marbles.Remove(curr.Previous);
                    }
                    else
                        curr = marbles.AddAfter(curr.Next ?? marbles.First, i);
                    playerIdx = (playerIdx + 1) % playerCount;
                }
                return players.Max();
            }
            return (play(marbleCount), play(marbleCount * 100));
        } // 1.18s

        private static int Day10(string input)
        {
            var pos = input.Split(Environment.NewLine)
                .Select((d, i) => (i, new int[2] { int.Parse(d.Substring(10, 6)), int.Parse(d.Substring(18, 6)) }))
                .ToDictionary(v => v.i, v => v.Item2);
            var vel = input.Split(Environment.NewLine)
                .Select((d, i) => (i, new int[2] { int.Parse(d.Substring(36, 2)), int.Parse(d.Substring(40, 2)) }))
                .ToDictionary(v => v.i, v => v.Item2);
            for (int i = 0; i < 10634; i++) // trial and error
            {
                foreach (var p in pos.Keys)
                {
                    pos[p][0] += vel[p][0];
                    pos[p][1] += vel[p][1];
                }
            }
            int minR = pos.Select(p => p.Value[0]).Min(), minC = pos.Select(p => p.Value[1]).Min();
            var grid = new int[10, 62];     // trial and error
            foreach (var p in pos.Keys)
            {
                pos[p][0] -= minR;
                pos[p][1] -= minC;
                grid[pos[p][1], pos[p][0]] = 1;
            }
            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 62; j++)
                    Console.Write(grid[i, j]);
                Console.WriteLine("");
            }
            return 10634;
        } // 0.17s

        private static (string, string) Day11(string input)
        {
            var serialNumber = int.Parse(input);
            var grid = new int[300][];
            for (int x = 0; x < 300; x++)
                grid[x] = Enumerable.Range(0, 300)
                    .Select(y => ((((x + 11) * (y + 1) + serialNumber) * (x + 11) / 100) % 10) - 5)
                    .ToArray();
            int subgridSum(int x, int y, int size)
            {
                int sum = 0;
                for (int i = 0; i < size; i++)
                    for (int j = 0; j < size; j++)
                        sum += grid[x + i][y + j];
                return sum;
            }
            int mS = 0, mX = 0, mY = 0, s = 3, mSize = 0;
            for (int x = 0; x < 297; x++)
            {
                for (int y = 0; y < 297; y++)
                {
                    var sum = subgridSum(x, y, s);
                    if (sum > mS)
                    {
                        mS = sum;
                        mX = x;
                        mY = y;
                    }
                }
            }
            (mS, mX, mY, s) = (0, 0, 0, 1);
            for (; s < 300; s++) {
                for (int x = 0; x < 300-s; x++) {
                    for (int y = 0; y < 300-s; y++) {
                        var sum = subgridSum(x, y, s);
                        if (sum > mS)
                        {
                            mS = sum;
                            mX = x;
                            mY = y;
                            mSize = s;
                        }
                    }
                }
            }
            return ("(" + mX+1 + "," + (mY+1) + ")", "(" + mX + 1 + "," + (mY + 1) + "," + mSize + ")");
        }
    }
}
