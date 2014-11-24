/// (c) 2014 Rei Ando
/// Released under MIT license.
/// https://raw.githubusercontent.com/neone1/MahjongClaculation/master/LICENSE

using System;
using System.Collections.Generic;
using System.Linq;

namespace Rei
{
    class Program
    {
        static void Main(string[] args)
        {
            //パターン数で計算するか (falseなら同一種4枚を区別して組み合わせを計算、以下同じ)
            var isSingle = false;

            //4Cnを計算
            var array = Enumerable.Range(0, 5).Select(x => Combination(4, x)).ToArray();
            //数牌3n枚の組み合わせ数を先に計算
            var cNum = Enumerable.Range(0, 5)
                .Select(x => (long)(Num(x * 3).Sum(y => isSingle ? 1 : Enumerable.Range(1, 9).Select(z => array[y[z]]).Aggregate((z, w) => z * w))))
                .ToArray();
            //字牌3n枚の組み合わせ数を先に計算
            var cZi = Enumerable.Range(0, 5)
                .Select(x => Combination(7, x) * (isSingle ? 1 : Pow(Combination(4, 1), x)))
                .ToArray();

            //3を数牌1-3、字牌に振り分ける振り分け方
            var seq3 = Enumerable.Range(0, 4)
                .SelectMany(x => Enumerable.Range(0, 4 - x).SelectMany(y => Enumerable.Range(0, 4 - x - y).Select(z => new { n0 = x, n1 = y, n2 = z, z = 3 - x - y - z })));
            //4を数牌1-3、字牌に振り分ける振り分け方
            var seq4 = Enumerable.Range(0, 5)
                .SelectMany(x => Enumerable.Range(0, 5 - x).SelectMany(y => Enumerable.Range(0, 5 - x - y).Select(z => new { n0 = x, n1 = y, n2 = z, z = 4 - x - y - z })));

            //数牌1が3n+1枚、数牌2,3,字牌が3n枚
            var p1000 = seq4.Select(x => NumWaitPattern(x.n0 * 3 + 1, isSingle) * cNum[x.n1] * cNum[x.n2] * cZi[x.z] * 3)
                .Sum((x, y) => x + y);
            //数牌1-3が3n枚、字牌が3n+1枚
            var p0001 = seq4.Select(x => ZiWaitPattern(x.z * 3 + 1, isSingle) * cNum[x.n0] * cNum[x.n1] * cNum[x.n2])
                .Sum((x, y) => x + y);
            //数牌1が3n+2枚(雀頭有)、数牌2が3n+2枚(雀頭無)、数牌3,字牌が3n枚
            var p2t2f00 = seq3.Select(x => NumWaitPattern(x.n0 * 3 + 2, isSingle, false) * NumWaitPattern(x.n1 * 3 + 2, isSingle).Total() * cNum[x.n2] * cZi[x.z] * Factorial(3))
                .Sum((x, y) => x + y);
            //数牌1,2が3n+2枚(雀頭有)、数牌3,字牌が3n枚
            var p2t2t00 = seq3.Select(x => NumWaitPattern(x.n0 * 3 + 2, isSingle) * NumWaitPattern(x.n1 * 3 + 2, isSingle) * cNum[x.n2] * cZi[x.z] * 3)
                .Sum((x, y) => x + y);
            //数牌1が3n+2枚(雀頭無)、数牌2,3が3n枚、字牌が3n+2枚
            var p2f002 = seq3.Select(x => NumWaitPattern(x.n0 * 3 + 2, isSingle, false) * ZiWaitPattern(x.z * 3 + 2, isSingle).Total() * cNum[x.n1] * cNum[x.n2] * 3)
                .Sum((x, y) => x + y);
            //数牌1が3n+2枚(雀頭有)、数牌2,3が3n枚、字牌が3n+2枚
            var p2t002 = seq3.Select(x => NumWaitPattern(x.n0 * 3 + 2, isSingle) * ZiWaitPattern(x.z * 3 + 2, isSingle) * cNum[x.n1] * cNum[x.n2] * 3)
                .Sum((x, y) => x + y);
            //国士無双
            var p13orphan = ThirteenOrphanPattern(isSingle);
            //七対子(二盃口でない)
            var p7pairs = SevenPairsPattern(isSingle);

            var pAll = p1000 + p0001 + p2t2f00 + p2t2t00 + p2f002 + p2t002 + p13orphan + p7pairs;

            for (int i = 0; i < 14; i++)
                for (int j = 0; j < 40; j++)
                    if (pAll[i, j] > 0)
                        Console.WriteLine("{0}面 {1}枚 {2}通り", i.PadSpace(2), j.PadSpace(2), pAll[i, j].PadSpace(16));
            Console.WriteLine("合計 {0}通り", pAll.Total());
        }

        /// <summary>
        /// 与えられた枚数の１種の数牌で聴牌又は上がりの手牌の形を列挙します。
        /// </summary>
        /// <param name="c">枚数</param>
        /// <param name="isComp">3n+2枚のとき、n面子+雀頭の形であるか。 nullなら両方出力</param>
        /// <returns>条件を満たす手牌形</returns>
        static IEnumerable<Group> Num(int c, bool? isComp = true)
        {
            switch (c % 3)
            {
                case 0:
                    return NumSub(c);
                case 1:
                    return Num(c + 1).SelectMany(x => Enumerable.Range(1, 9).Where(y => x[y] > 0).Select(y => x.Remove(y))).Distinct();
                default:
                    if (isComp == null)
                        return NumSub(c + 1).SelectMany(x => Enumerable.Range(1, 9).Where(y => x[y] > 0).Select(y => x.Remove(y))).Distinct();
                    if (isComp == true)
                        return NumSub(c - 2).SelectMany(x => Enumerable.Range(1, 9).Where(y => x[y] <= 2).Select(y => x.Add(y, y))).Distinct();
                    else
                        return Num(c, null).Except(Num(c));
            }
        }

        /// <summary>
        /// １種の数牌3n枚で構成された、n面子の手牌の形を列挙します。
        /// </summary>
        /// <param name="c">与えられた枚数</param>
        /// <returns>n面子の手牌形</returns>
        static IEnumerable<Group> NumSub(int c)
        {
            return NumCore(1, c, 0, 0, 0).Distinct();
        }

        /// <summary>
        /// NumSubの本体です。
        /// 与えられた条件下で、面子のみで構成された１種の数牌c枚の手牌を列挙します。
        /// </summary>
        /// <param name="n">次に加えることのできる最小の数</param>
        /// <param name="c">枚数</param>
        /// <param name="a0">既に加えた数牌nの枚数</param>
        /// <param name="a1">既に加えた数牌n+1の枚数</param>
        /// <param name="a2">既に加えた数牌n+2の枚数</param>
        /// <returns></returns>
        static IEnumerable<Group> NumCore(int n, int c, int a0, int a1, int a2)
        {
            //不適格なものは弾く
            if (a0 > 4 || a1 > 4 || a2 > 4 || c < 0 || n > 9)
                yield break;
            //残りゼロなら0のみ
            if (c == 0)
            {
                yield return new Group { Value = 0 };
                yield break;
            }
            //次の数へ
            foreach (var item in NumCore(n + 1, c, a1, a2, 0))
                yield return item;
            //刻子を加えるパターン
            foreach (var item in NumCore(n, c - 3, a0 + 3, a1, a2))
                yield return item.Add(n, n, n);
            //順子を加えるパターン
            if (n <= 7)
                foreach (var item in NumCore(n, c - 3, a0 + 1, a1 + 1, a2 + 1))
                    yield return item.Add(n, n + 1, n + 2);
        }

        /// <summary>
        /// 3n+1枚又は3n+2枚の１種の数牌で構成された部分の待ちパターンを返します。
        /// </summary>
        /// <param name="c">枚数</param>
        /// <param name="isSingle">パターン数で計算するか</param>
        /// <param name="isComp">3n+2枚のとき、n面子+雀頭の形であるか</param>
        /// <returns>待ち種類数・枚数とその通り数</returns>
        static Pattern NumWaitPattern(int c, bool isSingle, bool isComp = true)
        {
            var result = new Pattern();
            if (c % 3 != 0)
            {
                var target = new SortedSet<Group>(Num(c + 1));
                var array = Enumerable.Range(0, 5).Select(x => Combination(4, x)).ToArray();
                foreach (var item in Num(c, isComp))
                {
                    var pattern = 0;
                    var total = 0;
                    foreach (var n in Enumerable.Range(1, 9).Where(x => item[x] < 4 && target.Contains(item.Add(x))))
                    {
                        pattern++;
                        total += 4 - item[n];
                    }
                    if (pattern == 6 && total == 20)
                        Console.WriteLine(string.Concat(Enumerable.Range(1, 9).Select(x => new string((char)('0' + x), item[x]))));
                    if (pattern > 0)
                        result[pattern, total] += isSingle ? 1 : Enumerable.Range(1, 9).Select(x => array[item[x]]).Aggregate((x, y) => x * y);
                }
            }
            return result;
        }

        /// <summary>
        /// 3n+1枚又は3n+2枚の字牌で構成された部分の待ちパターンを返します。
        /// </summary>
        /// <param name="c">枚数</param>
        /// <param name="isSingle">パターン数で計算するか</param>
        /// <returns>待ち種類数・枚数とその通り数</returns>
        static Pattern ZiWaitPattern(int c, bool isSingle)
        {
            var result = new Pattern();
            switch (c % 3)
            {
                case 1:
                    result[1, 3] = 7 * Combination(6, c / 3) * (isSingle ? 1 : Pow(Combination(4, 1), c / 3 + 1));
                    if (c > 3)
                        result[2, 4] = Combination(7, 2) * Combination(5, c / 3 - 1) * (isSingle ? 1 : Pow(Combination(4, 2), 2) * Pow(Combination(4, 1), c / 3 - 1));
                    break;
                case 2:
                    result[1, 2] = 7 * Combination(6, c / 3) * (isSingle ? 1 : Combination(4, 2) * Pow(Combination(4, 1), c / 3));
                    break;
            }
            return result;
        }

        /// <summary>
        /// 国士無双形の待ちパターンを返します。
        /// </summary>
        /// <param name="isSingle">パターン数で計算するか</param>
        /// <returns>待ちパターン</returns>
        static Pattern ThirteenOrphanPattern(bool isSingle)
        {
            var result = new Pattern();
            result[1, 4] = 13 * 12 * (isSingle ? 1 : Combination(4, 2) * Pow(Combination(4, 1), 11));
            result[13, 39] = isSingle ? 1 : Pow(Combination(4, 1), 13);
            return result;
        }

        /// <summary>
        /// 七対子(二盃口形を除く)の待ちパターンを返します。
        /// </summary>
        /// <param name="isSingle">パターン数で計算するか</param>
        /// <returns>待ちパターン</returns>
        static Pattern SevenPairsPattern(bool isSingle)
        {
            var result = new Pattern();
            var array = Enumerable.Range(1, 34).Select(y => y + (y - 1) / 9).Select(y => y >= 30 ? y + (y - 31) : y).ToArray();
            var spcount = Enumerate(7, 34, 1)
                .Select(x => x.Select(y => array[y - 1]).ToArray())
                .Select(x => Enumerable.Range(0, 6).Select(y => x[y] - x[y + 1] != 1).ToArray())
                .Where(x => (x[0] || x[1] || x[4] || (x[3] && x[5])) && (x[1] || x[2] || x[4] || x[5]))
                .Count();
            result[1, 3] = spcount * 7 * (isSingle ? 1 : Combination(4, 1) * Pow(Combination(4, 2), 6));
            return result;
        }

        //階乗( x! )
        static long Factorial(int x)
        {
            return x <= 1 ? 1 : Factorial(x - 1) * x;
        }

        //コンビネーション( xCy )
        static int Combination(int x, int y)
        {
            return (int)(Factorial(x) / Factorial(y) / Factorial(x - y));
        }

        //累乗(整数バージョン)
        static long Pow(int x, int y)
        {
            return y == 0 ? 1 : Pow(x, y - 1) * x;
        }

        //組み合わせ生成
        static IEnumerable<IEnumerable<int>> Enumerate(int count, int max, int min)
        {
            if (max - count + 1 < min)
                yield break;
            if (count == 1)
                foreach (var item in Enumerable.Range(min, max - min + 1))
                    yield return new[] { item };
            else
                foreach (var item in Enumerable.Range(min, max - min + 1).SelectMany(x => Enumerate(count - 1, max, x + 1).Select(y => y.Concat(new[] { x }))))
                    yield return item;
        }
    }

    //待ち面数・牌数ごとのパターン数を格納
    class Pattern
    {
        long[,] value = new long[14, 40];

        //p面c枚待ち
        public long this[int p, int c]
        {
            get
            {
                if (p > 13 || p <= 0 || c > 39 || c < 0)
                    return 0;
                return value[p - 1, c];
            }
            set
            {
                if (p > 13 || p <= 0 || c > 39 || c < 0)
                    return;
                this.value[p - 1, c] = value;
            }
        }

        public static Pattern operator +(Pattern x, Pattern y)
        {
            var result = new Pattern();
            for (int i = 0; i < 14; i++)
                for (int j = 0; j < 40; j++)
                    result[i, j] = x[i, j] + y[i, j];
            return result;
        }

        //スカラー積
        public static Pattern operator *(Pattern x, long y)
        {
            var result = new Pattern();
            for (int i = 0; i < 14; i++)
                for (int j = 0; j < 40; j++)
                    result[i, j] = x[i, j] * y;
            return result;
        }

        //要素ごとの積
        public static Pattern operator *(Pattern x, Pattern y)
        {
            var result = new Pattern();
            for (int i = 0; i < 14; i++)
                for (int j = 0; j < 40; j++)
                    if (x[i, j] > 0)
                        for (int k = 0; k < 14; k++)
                            for (int l = 0; l < 40; l++)
                                if (y[k, l] > 0)
                                    result[i + k, j + l] += x[i, j] * y[k, l];
            return result;
        }

        public long Total()
        {
            var result = 0L;
            for (int i = 0; i < 14; i++)
                for (int j = 0; j < 40; j++)
                    result += this[i, j];
            return result;
        }
    }

    //手牌部分
    struct Group : IEquatable<Group>, IComparable<Group>
    {
        public int Value;
        public int this[int n]
        {
            get
            {
                if (n < 1 || n > 9)
                    return -1;
                return (Value >> (27 - 3 * n)) & 7;
            }
        }

        public Group Add(params int[] n)
        {
            return new Group { Value = this.Value + n.Select(x => (1 << (27 - 3 * x))).Sum() };
        }

        public Group Remove(int n)
        {
            return new Group { Value = this.Value - (1 << (27 - 3 * n)) };
        }

        bool IEquatable<Group>.Equals(Group obj)
        {
            return this.Value == obj.Value;
        }

        int IComparable<Group>.CompareTo(Group other)
        {
            return this.Value - other.Value;
        }
    }

    public static class Ext
    {
        //型Tのリストの総和を取ります。
        public static T Sum<T>(this IEnumerable<T> list, Func<T, T, T> f) where T : new()
        {
            var e = list.GetEnumerator();
            if (!e.MoveNext())
                return new T();
            var sum = e.Current;
            while (e.MoveNext())
            {
                sum = f(sum, e.Current);
            }
            return sum;
        }

        //指定した幅になるまで左に空白を入れます。
        public static string PadSpace<T>(this T value, int count)
        {
            return value.ToString().PadLeft(count);
        }
    }
}
