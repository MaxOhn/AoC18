
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace AdventOfCode18
{
    class Program
    {
        static void Main(string[] args)
        {
            using (StreamReader sr = new StreamReader("../../../D22.txt"))
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                var answer = Day22(sr.ReadToEnd());
                sw.Stop();
                Console.WriteLine($"Solution: {answer} [{sw.Elapsed}]");
            }
        }

        private static (int, int) Day1(string input)
        {
            var frequencies = input
                .Split(Environment.NewLine)
                .Select(d => int.Parse(d));
            int p2 = 0;
            var previousFreqs = new List<int>();
            while (true)
            {
                foreach (int f in frequencies)
                {
                    if (previousFreqs.Contains(p2))
                        return (frequencies.Sum(), p2);
                    previousFreqs.Add(p2);
                    p2 += f;
                }
            }
        } // 4.656s

        private static Tuple<int, string> Day2(string input)
        {
            var boxIDs = input
                .Split(Environment.NewLine)
                .ToList();
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

        private static (int, int) Day3(string input)
        {
            var claims = input
                .Split(Environment.NewLine)
                .Select(d => d.Split(" "));
            var fabric = new Dictionary<(int, int), int>();
            int counter = 0;
            var ids = new List<int>();
            foreach (var claim in claims)
            {
                var id = int.Parse(claim.First().Remove(0, 1));
                ids.Add(id);
                var offset = claim
                    .Skip(2)
                    .First()
                    .Remove(claim.Skip(2).First().Length - 1)
                    .Split(",")
                    .Select(d => int.Parse(d));
                var size = claim
                    .Last()
                    .Split("x")
                    .Select(d => int.Parse(d));
                for (int i = offset.First(), l1 = offset.First() + size.First(); i < l1; i++)
                {
                    for (int j = offset.Last(), l2 = offset.Last() + size.Last(); j < l2; j++)
                    {
                        var currPos = (i, j);
                        if (fabric.ContainsKey(currPos))
                        {
                            if (fabric[currPos] != -1)
                            {
                                ids.Remove(fabric[currPos]);
                                fabric[currPos] = -1;
                                counter++;
                            }
                            ids.Remove(id);
                        }
                        else
                            fabric.Add(currPos, id);
                    }
                }
            }
            return (counter, ids.First());
        } // 0.162s

        private static Tuple<int, int> Day4(string input)
        {
            var records = input.Split(Environment.NewLine).Select(d => d.Replace("[", "").Replace("]", "").Split(" "))
                .Select(d =>
                {
                    return new
                    {
                        date = DateTime.Parse(d[0] + " " + d[1]),
                        id = d[2].Equals("Guard") ? int.Parse(d[3].Substring(1)) : (int?)null,
                        sleeping = d[3].Equals("asleep")
                    };
                })
                .OrderBy(x => x.date).ToList();
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

        private static (int, int) Day5(string input)
        {
            int react(string l)
            {
                for (int i = 0; i < l.Length - 1; i++)
                {
                    if (l[i] != l[i + 1] && l[i].ToString().Equals(l[i + 1].ToString(), StringComparison.InvariantCultureIgnoreCase))
                    {
                        l = l.Remove(i, 2);
                        i = Math.Max(-1, i - 2);
                    }
                }
                return l.Length;
            }
            var results = new Dictionary<string, int> { { "", react(input) } };
            foreach (string letter in input.Select(c => c.ToString().ToLower()).Distinct())
                results.Add(letter, react(input.Replace(letter, "").Replace(letter.ToUpper(), "")));
            return (results[""], results.Values.Min());
        } // 3.218s

        private static Tuple<int, int> Day6(string input)
        {
            var coords = input
                .Split(Environment.NewLine)
                .Select(line => line.Split(", ")
                    .Select(num => Convert.ToInt32(num))
                    .ToArray())
                .Select(l => (x: l[0], y: l[1]))
                .ToArray();
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
            var numbers = input
                .Split(" ")
                .Select(d => int.Parse(d)).ToList();
            var nodes = new Dictionary<int, (List<int> childs, List<int> metas)>();
            (int, int) processNode(int idx, int name)
            {
                int currIdx = idx, currName = name + 1, amountChildren = numbers[currIdx], amountMeta = numbers[currIdx + 1];
                var children = new List<int>();
                currIdx += 2;
                for (int j = 0; j < amountChildren; j++)
                {
                    children.Add(currName);
                    (currIdx, currName) = processNode(currIdx, currName);
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
                    .Select(y => (((x + 11) * (y + 1) + serialNumber) * (x + 11) / 100 % 10) - 5)
                    .ToArray();
            var maxSums = new int[300][];
            for (int i = 0; i < 300; i++)
                Array.Copy(grid[i], 0, maxSums[i] = new int[300], 0, 300);
            int subgridSum(int x, int y, int size)
            {
                int outSum = 0;
                for (int i = 0; i < size; i++)
                    for (int j = 0; j < size; j++)
                        outSum += grid[x + i][y + j];
                return outSum;
            }
            int addNextCells(int x, int y, int s)
            {
                int addedSum = 0;
                for (int i = 0; i < s; i++)
                    addedSum += grid[x + s - 1][y + i] + grid[x + i][y + s - 1];
                return addedSum + maxSums[x][y] - grid[x + s - 1][y + s - 1];
            }
            int mX = 0, mY = 0, mS = 0, mSum = 0, p1x = 0, p1y = 0;
            for (int x = 0; x < 300 - 3; x++)
                for (int y = 0; y < 300 - 3; y++)
                    if ((mSum = subgridSum(x, y, 3)) > mS)
                        (mS, mX, mY) = (mSum, x, y);
            (mX, mY, mS, mSum, p1x, p1y) = (0, 0, 0, 0, mX, mY);
            for (int size = 2; size < 300; size++)
                for (int x = 0; x < 300-size; x++)
                    for (int y = 0; y < 300-size; y++)
                        if ((maxSums[x][y] = addNextCells(x, y, size)) > mSum)
                            (mX, mY, mS, mSum) = (x, y, size, maxSums[x][y]);
            return ("(" + (p1x+1) + "," + (p1y+1) + ")", "(" + (mX + 1) + "," + (mY + 1) + "," + mS + ")");
        } // 1.103s

        private static (long, long) Day12(string input)     // screw this stupid puzzle
        {
            var InputSplit = input
                .Split(Environment.NewLine);
            var rules = InputSplit
                .Skip(2)
                .Select(d => d.Split(" => "))
                .ToDictionary(r => r[0], r => r[1]);
            long run(long maxGens)
            {
                string currGen = InputSplit[0].Substring(15);
                int currLeft = 0;
                long score = 0, lastScore = 0, diff = 0, prevDiff = 0;
                for (long gen = 1; gen <= maxGens; gen++)
                {
                    StringBuilder nextGen = new StringBuilder();
                    for (int pos = -2; pos < currGen.Length + 2; pos++)
                    {
                        string state = string.Empty;
                        int distFromEnd = currGen.Length - pos;
                        if (pos <= 1)
                            state = new string('.', 2 - pos) + currGen.Substring(0, 3 + pos);
                        else if (distFromEnd <= 2)
                            state = currGen.Substring(pos - 2, distFromEnd + 2) + new string('.', 3 - distFromEnd);
                        else
                            state = currGen.Substring(pos - 2, 5);
                        nextGen.Append(rules.TryGetValue(state, out string newState) ? newState : ".");
                    }
                    (currGen, currLeft, score) = (nextGen.ToString(), currLeft - 2, 0);
                    for (int pos = 0; pos < currGen.Length; pos++)
                        score += currGen[pos].ToString() == "." ? 0 : pos + currLeft;
                    diff = score - lastScore;
                    if (diff == prevDiff)
                    {
                        score += (maxGens - gen) * prevDiff;
                        break;
                    }
                    (prevDiff, lastScore) = (diff, score);
                }
                return score;
            }
            return (run(20), run(50000000000));
        } // 0.0079s

        public static (string, string) Day13(string input)
        {
            var lines = input
                .Split(Environment.NewLine);
            var maxLine = lines
                .Max(x => x.Length);
            var grid = new char[lines.Length, maxLine];
            for (int i = 0; i < lines.Length; i++)
                for (int j = 0; j < lines[i].Length; j++)
                    grid[i, j] = lines[i][j];
            var cartSymbols = new[] { '^', 'v', '>', '<' };
            var carts = new List<(int x, int y, char dir, char turn, bool crashed)>();
            for (int y = 0; y < lines.Length; y++)
            {
                for (int x = 0; x < lines[y].Length; x++)
                {
                    if (!cartSymbols.Contains(grid[y, x]))
                        continue;
                    carts.Add((x, y, grid[y, x], 'l', false));
                    if (grid[y, x] == '^' || grid[y, x] == 'v')
                        grid[y, x] = '|';
                    else
                        grid[y, x] = '-';
                }
            }
            var turns = new Dictionary<(char dir, char gridSymbol), char>
            {
                { ('<', '/'), 'v' },
                { ('^', '/'), '>' },
                { ('>', '/'), '^' },
                { ('v', '/'), '<' },
                { ('<', '\\'), '^' },
                { ('^', '\\'), '<' },
                { ('>', '\\'), 'v' },
                { ('v', '\\'), '>' },
            };
            var intersections = new Dictionary<(char dir, char turn), (char dir, char turn)>
            {
                { ('<', 'l'), ('v', 's') },
                { ('<', 's'), ('<', 'r') },
                { ('<', 'r'), ('^', 'l') },
                { ('^', 'l'), ('<', 's') },
                { ('^', 's'), ('^', 'r') },
                { ('^', 'r'), ('>', 'l') },
                { ('>', 'l'), ('^', 's') },
                { ('>', 's'), ('>', 'r') },
                { ('>', 'r'), ('v', 'l') },
                { ('v', 'l'), ('>', 's') },
                { ('v', 's'), ('v', 'r') },
                { ('v', 'r'), ('<', 'l') },
            };
            (int x, int y) NextPos(int x, int y, char dir)
            {
                switch (dir)
                {
                    case '^': return (x, y - 1);
                    case 'v': return (x, y + 1);
                    case '>': return (x + 1, y);
                    case '<': return (x - 1, y);
                }
                throw new ArgumentException();
            }
            string p1 = "";
            string p2 = "";
            while (p2.Equals(""))
            {
                var orderedCarts = carts
                    .OrderBy(x => x.y)
                    .ThenBy(x => x.x)
                    .ToList();
                for (var i = 0; i < orderedCarts.Count; i++)
                {
                    var cart = orderedCarts[i];
                    if (cart.crashed)
                        continue;
                    var (x, y) = NextPos(cart.x, cart.y, cart.dir);
                    if (p1.Equals("") && orderedCarts.Any(c => c.x == x && c.y == y))
                        p1 = (x, y).ToString();
                    var crashedCartIndex = orderedCarts
                        .FindIndex(c => !c.crashed && c.x == x && c.y == y);
                    if (crashedCartIndex >= 0)
                    {
                        Console.WriteLine($"crash at ({x},{y})");
                        orderedCarts[i] = (x, y, cart.dir, cart.turn, true);
                        orderedCarts[crashedCartIndex] = (x, y, cart.dir, cart.turn, true);
                        continue;
                    }
                    var gridSymbol = grid[y, x];
                    if (gridSymbol == '\\' || gridSymbol == '/')
                        orderedCarts[i] = (x, y, turns[(cart.dir, gridSymbol)], cart.turn, cart.crashed);
                    else if (gridSymbol == '+')
                    {
                        var (dir, turn) = intersections[(cart.dir, cart.turn)];
                        orderedCarts[i] = (x, y, dir, turn, cart.crashed);
                    }
                    else
                        orderedCarts[i] = (x, y, cart.dir, cart.turn, cart.crashed);
                }
                carts = orderedCarts;
                if (carts.Count(x => !x.crashed) == 1)
                    p2 = carts
                        .Where(c => !c.crashed)
                        .Select(c => (c.x, c.y))
                        .First()
                        .ToString();
            }
            return (p1, p2);
        } // 0.049s

        public static (string, int) Day14(string input)
        {
            var scoreboard = new List<int> { 3, 7 };
            int inputNum = int.Parse(input), firstElfIdx = 0, secondElfIdx = 1, windowIndex = 0, posCheck = 0;
            void step()
            {
                if (scoreboard[firstElfIdx] + scoreboard[secondElfIdx] >= 10)
                    scoreboard.Add(1);
                scoreboard.Add((scoreboard[firstElfIdx] + scoreboard[secondElfIdx]) % 10);
                firstElfIdx = (firstElfIdx + scoreboard[firstElfIdx] + 1) % scoreboard.Count();
                secondElfIdx = (secondElfIdx + scoreboard[secondElfIdx] + 1) % scoreboard.Count();
            }
            while (scoreboard.Count() < inputNum + 10)
                step();
            string p1 = string.Join("", scoreboard.TakeLast(10));
            (scoreboard, firstElfIdx, secondElfIdx) = (new List<int> { 3, 7 }, 0, 1);
            var inputList = input.Select(digit => digit - '0').ToList();
            bool found = false;
            while (!found)
            {
                step();
                while (windowIndex + posCheck < scoreboard.Count)
                {
                    if (inputList[posCheck] == scoreboard[windowIndex + posCheck])
                    {
                        if (posCheck++ == inputList.Count - 1)
                        {
                            found = true;
                            break;
                        }
                    }
                    else
                    {
                        posCheck = 0;
                        windowIndex++;
                    }
                }
            }
            return (p1, windowIndex);
        } // 0.576s

        // heavy construction area, screw this puzzle :D
        public static (int, int) Day15(string input)
        {
            input = @"#########
#G..G..G#
#.......#
#.......#
#G..E..G#
#.......#
#.......#
#G..G..G#
#########";

            ((int, int), int) floodDistances(char[][] matrix, int sX, int sY, int tX, int tY)
            {
                var field = matrix.Select(line => line.Select(c => c + "").ToArray()).ToArray();
                field[tX][tY] = "0";
                bool foundTarget = false;
                int currDist = 0;
                var checkNext = new Dictionary<(int, int), int> { { (tX, tY), currDist++ } };
                while (!foundTarget)
                {
                    var nextIter = new HashSet<(int, int)>();
                    while (checkNext.Count > 0)
                    {
                        var ((cX, cY), dis) = checkNext.ElementAt(0);
                        checkNext.Remove((cX, cY));
                        if (field[cX + 1][cY].Equals(".") || (cX + 1 == sX && cY == sY))
                        {
                            field[cX + 1][cY] = currDist + "";
                            nextIter.Add((cX + 1, cY));
                        }
                        if (field[cX - 1][cY].Equals(".") || (cX - 1 == sX && cY == sY))
                        {
                            field[cX - 1][cY] = currDist + "";
                            nextIter.Add((cX - 1, cY));
                        }
                        if (field[cX][cY + 1].Equals(".") || (cX == sX && cY + 1 == sY))
                        {
                            field[cX][cY + 1] = currDist + "";
                            nextIter.Add((cX, cY + 1));
                        }
                        if (field[cX][cY - 1].Equals(".") || (cX == sX && cY - 1 == sY))
                        {
                            field[cX][cY - 1] = currDist + "";
                            nextIter.Add((cX, cY - 1));
                        }
                        if (nextIter.Contains((sX, sY)))
                        {
                            foundTarget = true;
                            break;
                        }
                    }
                    foreach (var pos in nextIter)
                        checkNext.Add(pos, currDist);
                    currDist++;
                }

                // TODO: sort by location if same distance
                var nearestPos = ((-1, -1), 10000);
                bool success;
                if ((success = int.TryParse(field[sX + 1][sY], out int parsed)) && parsed < nearestPos.Item2)
                    nearestPos = ((sX + 1, sY), parsed);
                if ((success = int.TryParse(field[sX - 1][sY], out parsed)) && parsed < nearestPos.Item2)
                    nearestPos = ((sX - 1, sY), parsed);
                if ((success = int.TryParse(field[sX][sY + 1], out parsed)) && parsed < nearestPos.Item2)
                    nearestPos = ((sX, sY + 1), parsed);
                if ((success = int.TryParse(field[sX][sY - 1], out parsed)) && parsed < nearestPos.Item2)
                    nearestPos = ((sX, sY - 1), parsed);
                return nearestPos;
            }

            int ELF_HP = 200;
            int ELF_ATTACK = 3;
            int GOBLIN_HP = 200;
            int GOBLIN_ATTACK = 3;

            var map = input
                .Split(Environment.NewLine)
                .Select(line => line.ToArray())
                .ToArray();
            var elves = map
                .Select((line, x) => line
                    .Select((c, y) => c.Equals('E') ? (x, y) : (-1, -1))
                    .Where(p => p.Item1 != -1).ToHashSet())
                .Aggregate((total, line) => total.Union(line).ToHashSet())
                .ToDictionary(coord => (x: coord.Item1, y: coord.Item2), _ => ELF_HP);
            var goblins = map
                .Select((line, x) => line
                    .Select((c, y) => c.Equals('G') ? (x, y) : (-1, -1))
                    .Where(p => p.Item1 != -1).ToHashSet())
                .Aggregate((total, line) => total.Union(line).ToHashSet())
                .ToDictionary(coord => (x: coord.Item1, y: coord.Item2), _ => GOBLIN_HP);

            bool ongoing = true;
            while (ongoing)
            {
                Console.WriteLine(string.Join("\n", map.Select(line => string.Join("", line))));
                Console.WriteLine("-----------------------------------------------");
                Thread.Sleep(1000);
                var nextMap = new char[map.Length][];
                for (int i = 0; i < map.Length; i++)
                {
                    var copiedLine = new char[map[i].Length];
                    Array.Copy(map[i], copiedLine, map[i].Length);
                    nextMap[i] = copiedLine;
                }
                for (int x = 0; x < map.Length; x++)
                {
                    for (int y = 0; y < map[x].Length; y++)
                    {
                        if (map[x][y].Equals('E') || map[x][y].Equals('G'))
                        {
                            if (goblins.Count == 0 || elves.Count == 0)
                            {
                                ongoing = false;
                                break;
                            }

                            var adjToEnemy = new Dictionary<(int, int), ((int, int), int)> ();
                            var destination = (-1, -1);
                            if (map[x][y].Equals('E'))
                            {
                                if (map[x + 1][y].Equals('G'))
                                    destination = (x + 1, y);
                                else if (map[x - 1][y].Equals('G'))
                                    destination = (x - 1, y);
                                else if (map[x][y + 1].Equals('G'))
                                    destination = (x, y + 1);
                                else if (map[x][y - 1].Equals('G'))
                                    destination = (x, y - 1);
                                else
                                {
                                    foreach (var goblin in goblins.Keys)
                                    {
                                        int cX; int cY;
                                        if (map[cX = goblin.x + 1][goblin.y].Equals('.') && !adjToEnemy.ContainsKey((cX, goblin.y)))
                                            adjToEnemy.Add((cX, goblin.y), floodDistances(nextMap, x, y, cX, goblin.y));
                                        if (map[cX = goblin.x - 1][goblin.y].Equals('.') && !adjToEnemy.ContainsKey((cX, goblin.y)))
                                            adjToEnemy.Add((cX, goblin.y), floodDistances(nextMap, x, y, cX, goblin.y));
                                        if (map[goblin.x][cY = goblin.y + 1].Equals('.') && !adjToEnemy.ContainsKey((goblin.x, cY)))
                                            adjToEnemy.Add((goblin.x, cY), floodDistances(nextMap, x, y, goblin.x, cY));
                                        if (map[goblin.x][cY = goblin.y - 1].Equals('.') && !adjToEnemy.ContainsKey((goblin.x, cY)))
                                            adjToEnemy.Add((goblin.x, cY), floodDistances(nextMap, x, y, goblin.x, cY));
                                    }
                                    if (adjToEnemy.Count == 0)
                                        continue;
                                }
                                if (destination.Item1 != -1)
                                {
                                    goblins[(x: destination.Item1, y: destination.Item2)] -= ELF_ATTACK;
                                    continue;
                                }
                            }
                            else
                            {
                                if (map[x + 1][y].Equals('E'))
                                    destination = (x + 1, y);
                                else if (map[x - 1][y].Equals('E'))
                                    destination = (x - 1, y);
                                else if (map[x][y + 1].Equals('E'))
                                    destination = (x, y + 1);
                                else if (map[x][y - 1].Equals('E'))
                                    destination = (x, y - 1);
                                else
                                {
                                    foreach (var elf in elves.Keys)
                                    {
                                        int cX; int cY;
                                        if (map[cX = elf.x + 1][elf.y].Equals('.') && !adjToEnemy.ContainsKey((cX, elf.y)))
                                            adjToEnemy.Add((cX, elf.y), floodDistances(nextMap, x, y, cX, elf.y));
                                        if (map[cX = elf.x - 1][elf.y].Equals('.') && !adjToEnemy.ContainsKey((cX, elf.y)))
                                            adjToEnemy.Add((cX, elf.y), floodDistances(nextMap, x, y, cX, elf.y));
                                        if (map[elf.x][cY = elf.y + 1].Equals('.') && !adjToEnemy.ContainsKey((elf.x, cY)))
                                            adjToEnemy.Add((elf.x, cY), floodDistances(nextMap, x, y, elf.x, cY));
                                        if (map[elf.x][cY = elf.y - 1].Equals('.') && !adjToEnemy.ContainsKey((elf.x, cY)))
                                            adjToEnemy.Add((elf.x, cY), floodDistances(nextMap, x, y, elf.x, cY));
                                    }
                                    if (adjToEnemy.Count == 0)
                                        continue;
                                }
                                if (destination.Item1 != -1)
                                {
                                    elves[(x: destination.Item1, y: destination.Item2)] -= GOBLIN_ATTACK;
                                    continue;
                                }
                            }
                            // TODO: sort by location if same distance
                            destination = adjToEnemy
                                .Aggregate((l, r) => l.Value.Item2 < r.Value.Item2 ? r : l)
                                .Value
                                .Item1;
                            nextMap[destination.Item1][destination.Item2] = map[x][y];
                            Console.WriteLine($"moving ({x},{y}) to ({destination.Item1},{destination.Item2})");
                            if (map[x][y].Equals('E'))
                            {
                                elves.Add(destination, elves[(x, y)]);
                                elves.Remove((x, y));
                            }
                            else
                            {
                                goblins.Add(destination, goblins[(x, y)]);
                                goblins.Remove((x, y));
                            }
                            nextMap[x][y] = '.';
                            Console.WriteLine(map[x][y]);
                        }
                    }
                    if (!ongoing)
                        break;
                }
                map = nextMap.ToArray();
            }

            return (0, 0);
        }

        public static (int, int) Day16(string input)
        {
            var instructions = input
                .Split(Environment.NewLine + Environment.NewLine + Environment.NewLine)[0]
                .Split(Environment.NewLine)
                .Where((instr, idx) => (idx + 1) % 4 != 0)
                .Select((instr, idx) => new { instr, t = idx / 3 })
                .GroupBy(p => p.t, p => p.instr)
                .Select(instr => {
                    var opinstrNums = instr.ElementAt(1).Split(" ").Select(num => int.Parse(num)).ToArray();
                    return new
                    {
                        before = instr.ElementAt(0).Substring(9, 10).Split(", ").Select(num => int.Parse(num)).ToArray(),
                        opinstr = new { category = opinstrNums[0], A = opinstrNums[1], B = opinstrNums[2], C = opinstrNums[3] },
                        after = instr.ElementAt(2).Substring(9, 10).Split(", ").Select(num => int.Parse(num)).ToArray()
                    };
                });
            int p1 = 0;
            var opinstrDict = new Dictionary<int, (HashSet<int> possible, HashSet<int> impossible)>();
            foreach (var instr in instructions)
            {
                int c = instr.opinstr.category, A = instr.opinstr.A, B = instr.opinstr.B, C = instr.opinstr.C;
                var possibleCategories = new HashSet<int>();
                var impossibleCategories = new HashSet<int>();
                if (A < 4 && B < 4 && instr.after[C] == instr.before[A] + instr.before[B])
                    possibleCategories.Add(0);
                else
                    impossibleCategories.Add(0);
                if (A < 4 && instr.after[C] == instr.before[A] + B)
                    possibleCategories.Add(1);
                else
                    impossibleCategories.Add(1);
                if (A < 4 && B < 4 && instr.after[C] == instr.before[A] * instr.before[B])
                    possibleCategories.Add(2);
                else
                    impossibleCategories.Add(2);
                if (A < 4 && instr.after[C] == instr.before[A] * B)
                    possibleCategories.Add(3);
                else
                    impossibleCategories.Add(3);
                if (A < 4 && B < 4 && instr.after[C] == (instr.before[A] & instr.before[B]))
                    possibleCategories.Add(4);
                else
                    impossibleCategories.Add(4);
                if (A < 4 && instr.after[C] == (instr.before[A] & B))
                    possibleCategories.Add(5);
                else
                    impossibleCategories.Add(5);
                if (A < 4 && B < 4 && instr.after[C] == (instr.before[A] | instr.before[B]))
                    possibleCategories.Add(6);
                else
                    impossibleCategories.Add(6);
                if (A < 4 && instr.after[C] == (instr.before[A] | B))
                    possibleCategories.Add(7);
                else
                    impossibleCategories.Add(7);
                if (A < 4 && instr.after[C] == instr.before[A])
                    possibleCategories.Add(8);
                else
                    impossibleCategories.Add(8);
                if (instr.after[C] == A)
                    possibleCategories.Add(9);
                else
                    impossibleCategories.Add(9);
                if ((B < 4 && A > instr.before[B] && instr.after[C] == 1) || instr.after[C] == 0)
                    possibleCategories.Add(10);
                else
                    impossibleCategories.Add(10);
                if ((A < 4 && instr.before[A] > B && instr.after[C] == 1) || instr.after[C] == 0)
                    possibleCategories.Add(11);
                else
                    impossibleCategories.Add(11);
                if ((A < 4 && B < 4 && instr.before[A] > instr.before[B] && instr.after[C] == 1) || instr.after[C] == 0)
                    possibleCategories.Add(12);
                else
                    impossibleCategories.Add(12);
                if ((B < 4 && A == instr.before[B] && instr.after[C] == 1) || instr.after[C] == 0)
                    possibleCategories.Add(13);
                else
                    impossibleCategories.Add(13);
                if ((A < 4 && instr.before[A] == B && instr.after[C] == 1) || instr.after[C] == 0)
                    possibleCategories.Add(14);
                else
                    impossibleCategories.Add(14);
                if ((A < 4 && B < 4 && instr.before[A] == instr.before[B] && instr.after[C] == 1) || instr.after[C] == 0)
                    possibleCategories.Add(15);
                else
                    impossibleCategories.Add(15);
                if (possibleCategories.Count > 2)
                    p1++;
                if (opinstrDict.ContainsKey(c))
                {
                    opinstrDict[c].possible.UnionWith(possibleCategories);
                    opinstrDict[c].impossible.UnionWith(impossibleCategories);
                }
                else
                    opinstrDict.Add(c, (possibleCategories, impossibleCategories));
            }
            var opMap = new Dictionary<int, int>();
            while (opMap.Count != 16)
            {
                foreach (var instr in instructions)
                {
                    if (opMap.ContainsKey(instr.opinstr.category))
                        continue;
                    var (possible, impossible) = opinstrDict[instr.opinstr.category];
                    var possibilities = possible.Except(impossible).ToHashSet();
                    if (possibilities.Count == 1)
                        opMap.Add(instr.opinstr.category, possibilities.First());
                    else
                        if ((possibilities = possibilities.Except(opMap.Values).ToHashSet()).Count == 1)
                            opMap.Add(instr.opinstr.category, possibilities.First());
                }
            }
            var testProg = input
                .Split(Environment.NewLine + Environment.NewLine + Environment.NewLine)[1]
                .Split(Environment.NewLine)
                .Skip(1)    // why in the world is there a 0 as first line oO
                .Select(line => line
                    .Split(" ")
                    .Select(num =>  int.Parse(num))
                    .ToArray());
            var r = new int[4];
            foreach (var instr in testProg)
            {
                switch (opMap[instr[0]])
                {
                    case 0: r[instr[3]] = r[instr[1]] + r[instr[2]]; break;
                    case 1: r[instr[3]] = r[instr[1]] +instr[2]; break;
                    case 2: r[instr[3]] = r[instr[1]] * r[instr[2]]; break;
                    case 3: r[instr[3]] = r[instr[1]] * instr[2]; break;
                    case 4: r[instr[3]] = r[instr[1]] & r[instr[2]]; break;
                    case 5: r[instr[3]] = r[instr[1]] & instr[2]; break;
                    case 6: r[instr[3]] = r[instr[1]] | r[instr[2]]; break;
                    case 7: r[instr[3]] = r[instr[1]] | instr[2]; break;
                    case 8: r[instr[3]] = r[instr[1]]; break;
                    case 9: r[instr[3]] = instr[1]; break;
                    case 10: r[instr[3]] = instr[1] > r[instr[2]] ? 1 : 0; break;
                    case 11: r[instr[3]] = r[instr[1]] > instr[2] ? 1 : 0; break;
                    case 12: r[instr[3]] = r[instr[1]] > r[instr[2]] ? 1 : 0; break;
                    case 13: r[instr[3]] = instr[1] == r[instr[2]] ? 1 : 0; break;
                    case 14: r[instr[3]] = r[instr[1]] == instr[2] ? 1 : 0; break;
                    case 15: r[instr[3]] = r[instr[1]] == r[instr[2]] ? 1 : 0; break;
                }
            }
            return (p1, r[0]);
        } // 0.0418s

        private static (int, int) Day17(string input)
        {
            var slices = input
                .Split(Environment.NewLine)
                .Select(line =>
                {
                    var split = line.Split(", ");
                    var xPosStr = split[0].Contains('x')
                        ? split[0].Substring(2, split[0].Length - 2)
                        : split[1].Substring(2, split[1].Length - 2);
                    var yPosStr = split[0].Contains('y')
                        ? split[0].Substring(2, split[0].Length - 2)
                        : split[1].Substring(2, split[1].Length - 2);
                    var xPos = new List<int>();
                    if (xPosStr.Contains('.'))
                    {
                        var limits = xPosStr.Split("..");
                        xPos = Enumerable.Range(int.Parse(limits[0]), int.Parse(limits[1]) - int.Parse(limits[0]) + 1)
                            .ToList();
                    }
                    else
                        xPos.Add(int.Parse(xPosStr));
                    var yPos = new List<int>();
                    if (yPosStr.Contains('.'))
                    {
                        var limits = yPosStr.Split("..");
                        yPos = Enumerable.Range(int.Parse(limits[0]), int.Parse(limits[1]) - int.Parse(limits[0]) + 1)
                            .ToList();
                    }
                    else
                        yPos.Add(int.Parse(yPosStr));
                    return (y: xPos, x: yPos);
                });
            int maxX = slices.Max(line => line.x.Max()) + 1, maxY = slices.Max(line => line.y.Max()) + 1;
            int minX = slices.Min(line => line.x.Min()) - 1, minY = slices.Min(line => line.y.Min()) - 1;
            var grid = new char[maxX - minX][];
            for (int i = 0; i < maxX - minX; i++)
                grid[i] = new char[maxY - minY];
            foreach (var slice in slices)
                foreach (var x in slice.x)
                    foreach (var y in slice.y)
                        grid[x - minX][y - minY] = '#';
            var finishedOrigins = new HashSet<(int, int)>();
            bool addWater((int x, int y) source)
            {
                var (x, y) = source;
                while (x < grid.Length && !grid[x][y].Equals('#') && !grid[x][y].Equals('~'))
                    grid[x++][y] = '|';
                if (x == grid.Length)
                    return false;
                if (grid[x][y].Equals('#'))
                    x--;
                while (true)
                {
                    int lPush = y, rPush = y;
                    while ((grid[x + 1][lPush].Equals('#') || grid[x + 1][lPush].Equals('~')) && !grid[x][lPush].Equals('#'))
                        lPush--;
                    while ((grid[x + 1][rPush].Equals('#') || grid[x + 1][rPush].Equals('~')) && !grid[x][rPush].Equals('#'))
                        rPush++;
                    for (int i = 0; i < Math.Max(lPush, rPush); i++)
                    {
                        if (y - i > lPush && !grid[x][y - i].Equals('~'))
                            if ((grid[x][y - i] = !grid[x][lPush].Equals('#') || !grid[x][rPush].Equals('#') ? '|' : '~').Equals('~'))
                                return true;
                        if (y + i < rPush && !grid[x][y + i].Equals('~'))
                            if ((grid[x][y + i] = !grid[x][lPush].Equals('#') || !grid[x][rPush].Equals('#') ? '|' : '~').Equals('~'))
                                return true;
                    }
                    if (!grid[x][lPush].Equals('#'))
                        if (!finishedOrigins.Contains((x, lPush)) && addWater((x, lPush)))
                            return true;
                        else
                            finishedOrigins.Add((x, lPush));
                    if (!grid[x][rPush].Equals('#'))
                        if (!finishedOrigins.Contains((x, rPush)) && addWater((x, rPush)))
                            return true;
                        else
                            finishedOrigins.Add((x, rPush));
                    if (!grid[x][lPush].Equals('#') || !grid[x][rPush].Equals('#'))
                        return false;
                    x--;
                }
            }
            while (addWater((1, 500 - minY))) ;
            int p2 = grid.Select(line => line.Count(c => c.Equals('~'))).Sum();
            return (grid.Select(line => line.Count(c => c.Equals('|'))).Sum() + p2, p2);
        } // 0.682s

        private static (int, int) Day18(string input)
        {
            var area = input
                .Split(Environment.NewLine)
                .Select(line => line.ToArray())
                .ToArray();
            var seenAreas = new Dictionary<int, int>();
            char[][] nextMinute()
            {
                var newArea = new char[area.Length][];
                for (int x = 0; x < area.Length; x++)
                    newArea[x] = new char[area[x].Length];
                for (int x = 0; x < area.Length; x++)
                {
                    for (int y = 0; y < area[x].Length; y++)
                    {
                        int adjTree = 0, adjLumberyard = 0, adjOpen = 0;
                        for (int i = -1; i <= 1; i++)
                        {
                            for (int j = -1; j <= 1; j++)
                            {
                                if ((i == 0 && i == j) || x + i < 0 || x + i >= area.Length || y + j < 0 || y + j >= area[x].Length)
                                    continue;
                                if (area[x + i][y + j].Equals('.'))
                                    adjOpen++;
                                else if (area[x + i][y + j].Equals('|'))
                                    adjTree++;
                                else
                                    adjLumberyard++;
                            }
                        }
                        if (area[x][y].Equals('.'))
                            newArea[x][y] = adjTree >= 3 ? '|' : '.';
                        else if (area[x][y].Equals('|'))
                            newArea[x][y] = adjLumberyard >= 3 ? '#' : '|';
                        else if (area[x][y].Equals('#'))
                            newArea[x][y] = adjLumberyard >= 1 && adjTree >= 1 ? '#' : '.';
                    }
                }
                return newArea;
            }
            // Mesmerizing *.*
            /*
            while (true)
            {
                for (int x = 0; x < area.Length; x++)
                {
                    Console.SetCursorPosition(0, x);
                    Console.WriteLine(string.Join("", area[x]));
                }
                area = nextMinute();
                Thread.Sleep(25);
            }
            //*/
            int minuteCounter = 0;
            for (; minuteCounter < 10; minuteCounter++)
                area = nextMinute();
            int p1 = area.Select(line => line.Count(c => c.Equals('|'))).Sum()
                   * area.Select(line => line.Count(c => c.Equals('#'))).Sum();
            for (; minuteCounter < 500; minuteCounter++)    // iterate sufficiently high
                area = nextMinute();
            while (true)
            {
                if ((minuteCounter++ % 28) == (1000000000 % 28))    // phase is 28
                    return (p1, area.Select(line => line.Count(c => c.Equals('|'))).Sum()
                       * area.Select(line => line.Count(c => c.Equals('#'))).Sum());
                area = nextMinute();
            }
        } // 0.0523s

        private static (int, int) Day19(string input)
        {
            var lines = input
                .Split(Environment.NewLine)
                .ToList();
            int ip = int.Parse(lines.First().Split(" ")[1]);
            var instructions = lines.Skip(1)
                .Select(line =>
                {
                    var split = line.Split(" ");
                    return (split[0], int.Parse(split[1]), int.Parse(split[2]), int.Parse(split[3])); ;
                }).ToArray();
            int run(bool p1)
            {
                var r = new int[] { p1 ? 0 : 1, 0, 0, 0, 0, 0 };
                while (r[ip] < instructions.Length)
                {
                    var (op, A, B, C) = instructions[r[ip]];
                    if (!p1 && r[ip] == 2 && r[5] != 0)
                    {
                        if (r[1] % r[5] == 0)
                            r[0] += r[5];
                        r[4] = 0;
                        r[3] = r[1];
                        r[ip] = 11;
                    }
                    switch (op)
                    {
                        case "addr": r[C] = r[A] + r[B]; break;
                        case "addi": r[C] = r[A] + B; break;
                        case "mulr": r[C] = r[A] * r[B]; break;
                        case "muli": r[C] = r[A] * B; break;
                        case "banr": r[C] = r[A] & r[B]; break;
                        case "bani": r[C] = r[A] & B; break;
                        case "borr": r[C] = r[A] | r[B]; break;
                        case "bori": r[C] = r[A] | B; break;
                        case "setr": r[C] = r[A]; break;
                        case "seti": r[C] = A; break;
                        case "gtir": r[C] = A > r[B] ? 1 : 0; break;
                        case "gtri": r[C] = r[A] > B ? 1 : 0; break;
                        case "gtrr": r[C] = r[A] > r[B] ? 1 : 0; break;
                        case "eqir": r[C] = A == r[B] ? 1 : 0; break;
                        case "eqri": r[C] = r[A] == B ? 1 : 0; break;
                        case "eqrr": r[C] = r[A] == r[B] ? 1 : 0; break;
                    }
                    r[ip]++;
                }
                return r[0];
            }
            return (run(true), run(false));
        } // 2.59s

        private static (int, int) Day20(string input)
        {
            var dir = new Dictionary<char, (int, int)>
            {
                { 'N', (-1, 0) },
                { 'S', (1, 0) },
                { 'W', (0, -1) },
                { 'E', (0, 1) },
            };
            var pos = new Stack<(int, int)>();
            var dist = new Dictionary<(int, int), int>();
            int x = 0, y = 0, pX = 0, pY = 0;
            foreach (var c in input.Skip(1).SkipLast(1))
            {
                switch (c)
                {
                    case '(': pos.Push((x, y)); break;
                    case ')': (x, y) = pos.Pop(); break;
                    case '|': (x, y) = pos.Peek(); break;
                    default:
                        var (dx, dy) = dir[c]; x += dx; y += dy;
                        dist[(x, y)] = dist.ContainsKey((x, y))
                            ? Math.Min(dist[(x, y)], dist[(pX, pY)])
                            : dist.ContainsKey((pX, pY)) ? dist[(pX, pY)] + 1 : 1;
                        break;
                }
                pX = x; pY = y;
            }
            return (dist.Values.Max(), dist.Count(kvp => kvp.Value >= 1000));
        } // 0.0198s

        private static (int, int) Day21(string input)
        {
            var lines = input
                .Split(Environment.NewLine)
                .ToList();
            int ip = int.Parse(lines.First().Split(" ")[1]);
            var instructions = lines.Skip(1)
                .Select(line =>
                {
                    var split = line.Split(" ");
                    return (split[0], int.Parse(split[1]), int.Parse(split[2]), int.Parse(split[3])); ;
                }).ToArray();
            int run(bool p1)
            {
                var possibleValues = new HashSet<int>();
                int prevReg = 0;
                var r = new int[6];
                while (r[ip] < instructions.Length)
                {
                    var (op, A, B, C) = instructions[r[ip]];
                    switch (op)
                    {
                        case "addr": r[C] = r[A] + r[B]; break;
                        case "addi": r[C] = r[A] + B; break;
                        case "mulr": r[C] = r[A] * r[B]; break;
                        case "muli": r[C] = r[A] * B; break;
                        case "banr": r[C] = r[A] & r[B]; break;
                        case "bani": r[C] = r[A] & B; break;
                        case "borr": r[C] = r[A] | r[B]; break;
                        case "bori": r[C] = r[A] | B; break;
                        case "setr": r[C] = r[A]; break;
                        case "seti": r[C] = A; break;
                        case "gtir": r[C] = A > r[B] ? 1 : 0; break;
                        case "gtri": r[C] = r[A] > B ? 1 : 0; break;
                        case "gtrr": r[C] = r[A] > r[B] ? 1 : 0; break;
                        case "eqir": r[C] = A == r[B] ? 1 : 0; break;
                        case "eqri": r[C] = r[A] == B ? 1 : 0; break;
                        case "eqrr": r[C] = r[A] == r[B] ? 1 : 0; break;
                    }
                    if (r[ip] == 28)    // only instruction 28 depends on r[0] (and r[2])
                    {
                        if (p1)
                            return r[2];
                        if (possibleValues.Contains(r[2]))
                            return prevReg;
                        possibleValues.Add(prevReg = r[2]);
                    }
                    r[ip]++;
                }
                return r[0];    // never reached
            }
            return (run(true), run(false));
        } // 35.985s

        private static (int, int) Day22(string input)
        {
            int depth = int.Parse(input.Split(Environment.NewLine)[0].Split(" ")[1]);
            var targetSplit = input
                .Split(Environment.NewLine)[1]
                .Split(" ")[1]
                .Split(",");
            (int x, int y) target = (int.Parse(targetSplit[0]), int.Parse(targetSplit[1]));
            var cave = new Dictionary<(int, int), int>();
            int getErosion(int x, int y)
            {
                if (cave.TryGetValue((x, y), out int erosion))
                    return erosion;
                if ((x, y) == (0, 0) || (x, y) == target)
                    erosion = 0;
                else if (y == 0)
                    erosion = x * 16807;
                else if (x == 0)
                    erosion = y * 48271;
                else
                    erosion = getErosion(x - 1, y) * getErosion(x, y - 1);
                erosion = (erosion + depth) % 20183;
                cave.Add((x, y), erosion);
                return erosion;
            }
            int getType(int x, int y) => getErosion(x, y) % 3;
            IEnumerable<int> allowedTools(int x, int y) => Enumerable.Range(0, 3).Where(n => n != getType(x, y));
            int run()
            {
                var Q = new Priority_Queue.SimplePriorityQueue<(int, int, int, int)>();
                var seen = new HashSet<(int, int, int)>();
                Q.Enqueue((0, 0, 1, 0), 0);
                var dist = new Dictionary<(int, int, int, int), int> { { (0, 0, 1, 0), 0 } };
                (int x, int y)[] next = { (-1, 0), (1, 0), (0, -1), (0, 1) };
                while (true)
                {
                    (int cX, int cY, int cT, int wait) = Q.Dequeue();
                    int cD = dist[(cX, cY, cT, wait)];
                    if (wait > 0)
                    {
                        if (wait != 1 || seen.Add((cX, cY, cT)))
                            Q.Enqueue((cX, cY, cT, wait - 1), dist[(cX, cY, cT, wait - 1)] = cD + 1);
                        continue;
                    }
                    if ((cX, cY) == target && cT == 1)
                        return cD;
                    foreach ((int sX, int sY) in next)
                    {
                        (int nX, int nY) = (cX + sX, cY + sY);
                        if (nX < 0 || nY < 0)
                            continue;
                        if (allowedTools(nX, nY).Contains(cT) && seen.Add((nX, nY, cT)))
                            Q.Enqueue((nX, nY, cT, 0), dist[(nX, nY, cT, 0)] = cD + 1);
                    }
                    foreach (int oT in allowedTools(cX, cY))
                        Q.Enqueue((cX, cY, oT, 6), dist[(cX, cY, oT, 6)] = cD + 1);
                }
            }
            var p1 = Enumerable.Range(0, target.x + 1)
                .Select(x => Enumerable.Range(0, target.y + 1)
                    .Select(y => getType(x, y))
                    .Sum())
                .Sum();
            return (p1, run());
        } // 19.512s
    }
}
