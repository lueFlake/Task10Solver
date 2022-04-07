//#define MY_DEBUG
#define BETWEEN
#define FILE_IO
//#define SUDOKU

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Task10 {
    internal class Program {
        private const char s_space = '.';
        static char[][] s_arr = new char[8][];
        static MyGraph s_graph;
        private static char s_neut;

        public static int it = 0;

        static void Main(string[] args) {
#if FILE_IO
            using var sr = new StreamReader("../../../input.txt");
            using var sw = new StreamWriter("../../../output.txt");
#endif
            Console.SetIn(sr);
            Console.SetOut(sw);
            for (int i = 0; i < 8; i++) {
                s_arr[i] = new char[8];
                string input = Console.ReadLine();
                for (int j = 0; j < 8; j++) {
                    s_arr[i][j] = input[j];
                }
            }

            if (SeekForNeutral()) {
                Fill();
            }

            Console.WriteLine("Готово");
            Print();
        }

        private static void Fill() {
            while (!Full()) {
#if BETWEEN
                Console.WriteLine("Промежуточное (если хочешь убрать убери #define BETWEEN):");
                Print();
#endif
                s_graph = new MyGraph(8 * 8);
                CreateTags();
                SimpleConnect();
                TripleAssociativeConnect();

                {
                    ApplyAssociatives();
                    //ApplySudokuRule1();
#if SUDOKU
                    Console.WriteLine("Использование правил \"судоку\" (если хочешь убрать убери #define SUDOKU, и без этого робит):");
                    ApplySudokuRule2();
#endif
                    //ApplyInverseCommutatives();
                }

                if (++it > 5) {
                    Console.WriteLine("Слишком долго... Держи вот эта:");
                    Print();
                    Environment.Exit(0);
                }
            }
        }   

        [Obsolete]
        private static void ApplyInverseCommutatives() {
            for (int i = 0; i < 8; i++) {
                for (int j = 0; j < 8; j++) {
                    if (s_arr[i][j] == s_neut) {
                        s_arr[j][i] = s_neut;
                    }
                }
            }
        }

        private static void ApplySudokuRule2() {
            for (int i = 0; i < 8; i++) {
                for (char j = 'a'; j <= 'h'; j++) {
                    if (SeekForSymInRow(i, j)) {
                        Print();
                        Console.WriteLine($"Заполнен символ {j} в строке {i + 1}");
                    }
                    if (SeekForSymInCol(i, j)) { 
                        Print();
                        Console.WriteLine($"Заполнен символ {j} в столбце {i + 1}");
                    }
                }
            }
        }

        private static bool SeekForSymInCol(int col, char a) {
            for (int i = 0; i < 8; i++) {
                if (s_arr[i][col] == a) {
                    return false;
                }
            }
            bool[] used = new bool[8];
            int cnt = 0;
            for (int i = 0; i < 8; i++) {
                if (s_arr[i][col] != '.') {
                    used[i] = true;
                }
                else {
                    for (int j = 0; j < 8; j++) {
                        if (s_arr[i][j] == a) {
                            used[i] = true;
                            break;
                        }
                    }
                }
                if (!used[i])
                    cnt++;
            }
            if (cnt == 1) {
                for (int i = 0; i < 8; i++) {
                    if (!used[i]) {
                        s_arr[i][col] = a;
                        return true;
                    }
                }
            }
            return false;
        }

        private static bool SeekForSymInRow(int row, char a) {
            for (int i = 0; i < 8; i++) {
                if (s_arr[row][i] == a) {
                    return false;
                }
            }
            bool[] used = new bool[8];
            int cnt = 0;
            for (int i = 0; i < 8; i++) {
                if (s_arr[row][i] != '.') {
                    used[i] = true;
                }
                else {
                    for (int j = 0; j < 8; j++) {
                        if (s_arr[j][i] == a) {
                            used[i] = true;
                            break;
                        }
                    }
                }
                if (!used[i])
                    cnt++;
            }
            if (cnt == 1) {
                for (int i = 0; i < 8; i++) {
                    if (!used[i]) {
                        s_arr[row][i] = a;
                        return true;
                    }
                }
            }
            return false;
        }

        private static void Print() {
            for (int i = 0; i < 8; i++) {
                for (int j = 0; j < 8; j++) {
                    Console.Write(s_arr[i][j]);
                }
                Console.WriteLine();
            }
        }

        [Obsolete]
        private static void ApplySudokuRule1() {
            for (int i = 0; i < 8; i++) {
                for (int j = 0; j < 8; j++) {
                    if (s_arr[i][j] != s_space) {
                        continue;
                    }
                    bool[] used = new bool[8];
                    for (int k = 0; k < 8; k++) {
                        if (s_arr[i][k] != s_space) {
                            used[s_arr[i][k] - 'a'] = true;
                        }
                        if (s_arr[k][j] != s_space) {
                            used[s_arr[k][j] - 'a'] = true;
                        }
                    }
                    if (used.Count(x => !x) == 1) {
                        int free = -1;
                        for (int k = 0; k < 8; k++) {
                            if (!used[k])
                                free = k;
                        }
                        s_arr[i][j] = (char)(free + 'a');
                    }
                }
            }
        }

        private static void ApplyAssociatives() {
            foreach (var component in s_graph.GetAndColorComponents()) {
                foreach (var v in component.Vertices) {
                    (int i, int j) p = IntToPair(v.Id);
                    if (v.Tag != null) {
                        s_arr[p.i][p.j] = (char)v.Tag;
                    }
                }
                Console.WriteLine(component.KeyInfo);
                foreach (var edgeInfo in component.UsedEdges) {
                    Console.WriteLine(edgeInfo);
                }
                Console.WriteLine();
            }
        }

        private static bool Full() {
            for (int i = 0; i < 8; i++) {
                for (int j = 0; j < 8; j++) {
                    if (s_arr[i][j] == s_space)
                        return false;
                }
            }
            return true;
        }

        private static void CreateTags() {
            for (int i = 0; i < 8; i++) {
                for (int j = 0; j < 8; j++) {
                    if (s_arr[i][j] != s_space)
                        s_graph.Vertices[PairToInt(i, j)].Tag = s_arr[i][j];
                }
            }
        }

        private static void TripleAssociativeConnect() {
            for (int i = 0; i < 8; i++) {
                for (int j = 0; j < 8; j++) {
                    for (int k = 0; k < 8; k++) {
                        if (s_arr[i][j] != s_space && s_arr[j][k] != s_space) {
                            int l = s_arr[i][j] - 'a';
                            int r = s_arr[j][k] - 'a';
                            s_graph.AddEdge(PairToInt(l, k),
                                PairToInt(i, r), 
                                $"По ассоциативности {(char)(i + 'a')}{(char)(j + 'a')}{(char)(k + 'a')} = {(char)(i + 'a')}{(char)(r + 'a')} = {(char)(l + 'a')}{(char)(k + 'a')}");
#if MY_DEBUG
                            Console.WriteLine($"{(char)(i + 'a')}{(char)(r + 'a')} {(char)(l + 'a')}{(char)(k + 'a')}");
#endif
                        }
                    }
                }
            }
#if MY_DEBUG
            Console.WriteLine();
#endif
        }

        private static void SimpleConnect() {
            List<(int u, int v)>[] common = new List<(int, int)>[8];
            for (int i = 0; i < 8; i++)
                common[i] = new();
            for (int i = 0; i < 8; i++) {
                for (int j = 0; j < 8; j++) {
                    if (s_arr[i][j] != s_space) {
                        common[s_arr[i][j] - 'a'].Add((i, j));
                    }
                }
            }
            for (int i = 0; i < 8; i++) {
                for (int j = 1; j < common[i].Count; j++) {
                    s_graph.AddEdge(PairToInt(common[i][j].u, common[i][j].v),
                        PairToInt(common[i][j - 1].u, common[i][j - 1].v),
                        $"{(char)(common[i][j].u + 'a')}{(char)(common[i][j].v + 'a')} = {(char)(common[i][j - 1].u + 'a')}{(char)(common[i][j - 1].v + 'a')} = {(char)(i + 'a')} по уже готовым значениям таблицы. (Эта инфа может быть бесполезна).");
                }
            }
        }

        public static int PairToInt(int u, int v) {
            return u * 8 + v;
        }

        public static (int, int) IntToPair(int x) {
            return (x / 8, x % 8);
        }

        private static bool SeekForNeutral() {
            int neut = -1;
            for (int i = 0; i < 8; i++) {
                bool cond = true;
                for (int j = 0; j < 8; j++) {
                    if (!(s_arr[i][j] == j + 'a' || s_arr[i][j] == s_space)) {
                        cond = false;
                    }
                    if (!(s_arr[j][i] == j + 'a' || s_arr[j][i] == s_space)) {
                        cond = false;
                    }
                }
                if (cond) {
                    neut = i;
                    break;
                }
            }

            {
                /*if (neut == -1) {
                    for (int i = 0; i < 8; i++) {
                        bool cond = true;
                        for (int j = 0; j < 8; j++) {
                            if (s_arr[i][j] != ' ' || s_arr[j][i] != ' ')
                                cond = false;
                        }
                        if (cond) {
                            neut = i;
                            break;
                        }
                    }
                }
                */
            }

            if (neut == -1) {
                Console.WriteLine("No neutral");
                return false;
            }

            Console.WriteLine($"Нейтральный: {(char)(neut + 'a')}");
            Console.WriteLine();

            for (int i = 0; i < 8; i++) {
                s_arr[i][neut] = (char)(i + 'a');
                s_arr[neut][i] = (char)(i + 'a');
            }
            s_neut = (char)(neut + 'a');
            return true;
        }
    }
}
