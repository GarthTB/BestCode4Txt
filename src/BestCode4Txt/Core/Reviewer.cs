using BestCode4Txt.Config;
using BestCode4Txt.Models;

namespace BestCode4Txt.Core;

/// <summary> 编码分析器 </summary>
internal static class Reviewer
{
    /// <summary> 分析编码及其开销，返回报告 </summary>
    /// <param name="cc"> 文本编码及其开销 </param>
    /// <param name="textLen"> 文本字数 </param>
    /// <param name="layout"> 键盘布局配置 </param>
    public static string[] Run(CodeCost cc, int textLen, LayoutCfg layout) {
        var (code, cost) = cc;
        ArgumentException.ThrowIfNullOrEmpty(code, nameof(cc));

        var rowCount = new int[5]; // 各排计数
        var lCount = new int[5]; // 左手各指计数
        var rCount = new int[5]; // 右手各指计数
        var leapCount = new int[3]; // 同指跨1、2、3排计数
        var repeatCount = new int[4]; // 2、3、4、5+连击计数
        var lSet = layout.Lefts.SelectMany(static x => x).ToHashSet();
        var rSet = layout.Rights.SelectMany(static x => x).ToHashSet();
        var turnCount = 0; // 左右左、右左右计数（用于计算互击）

        _ = Parallel.For(0, code.Length, i => {
            var c = code[i];
            for (var j = 0; j < 5; j++) {
                if (layout.Rows[j].Contains(c))
                    _ = Interlocked.Increment(ref rowCount[j]);
                if (layout.Lefts[j].Contains(c))
                    _ = Interlocked.Increment(ref lCount[j]);
                if (layout.Rights[j].Contains(c))
                    _ = Interlocked.Increment(ref rCount[j]);
            }
            if (i > 0)
                Count2Chars(code[i - 1], c);
            if (i > 1)
                Count3Chars(code[i - 2], code[i - 1], c);
            if (i > 2 && IsRepeat(code[i - 3], code[i - 2], code[i - 1], c))
                _ = Interlocked.Increment(ref repeatCount[2]);
            if (i > 3 && IsRepeat(code[i - 4], code[i - 3], code[i - 2], code[i - 1], c))
                _ = Interlocked.Increment(ref repeatCount[3]);
        });

        var lSum = lCount.Sum();
        var rSum = rCount.Sum();
        var bias = 100d * Math.Abs(lSum - rSum) / (lSum + rSum);
        var repeat2 = repeatCount[0] - repeatCount[1];
        var repeat3 = repeatCount[1] - repeatCount[2];
        var repeat4 = repeatCount[2] - repeatCount[3];

        return [
            "------数据------",
            $"总字数\t{textLen}",
            $"总码数\t{code.Length}",
            $"总开销\t{cost:0.####}",
            $"字均码长\t{1d * code.Length / textLen:0.####}",
            $"字均开销\t{cost / textLen:0.####}",
            $"码均开销\t{cost / code.Length:0.####}",
            $"偏倚\t{bias:0.####} %",
            "互击\t" + FormatCountNRatio(turnCount, 3),
            "----------------",
            "数字排\t" + FormatCountNRatio(rowCount[0], 1),
            "上排\t" + FormatCountNRatio(rowCount[1], 1),
            "中排\t" + FormatCountNRatio(rowCount[2], 1),
            "下排\t" + FormatCountNRatio(rowCount[3], 1),
            "底排\t" + FormatCountNRatio(rowCount[4], 1),
            "----------------",
            "总左手\t" + FormatCountNRatio(lSum, 1),
            "1指\t" + FormatCountNRatio(lCount[0], 1),
            "2指\t" + FormatCountNRatio(lCount[1], 1),
            "3指\t" + FormatCountNRatio(lCount[2], 1),
            "4指\t" + FormatCountNRatio(lCount[3], 1),
            "5指\t" + FormatCountNRatio(lCount[4], 1),
            "----------------",
            "总右手\t" + FormatCountNRatio(rSum, 1),
            "1指\t" + FormatCountNRatio(rCount[0], 1),
            "2指\t" + FormatCountNRatio(rCount[1], 1),
            "3指\t" + FormatCountNRatio(rCount[2], 1),
            "4指\t" + FormatCountNRatio(rCount[3], 1),
            "5指\t" + FormatCountNRatio(rCount[4], 1),
            "----------------",
            "同指跨1排\t" + FormatCountNRatio(leapCount[0], 2),
            "同指跨2排\t" + FormatCountNRatio(leapCount[1], 2),
            "同指跨3排\t" + FormatCountNRatio(leapCount[2], 2),
            "----------------",
            "同键2连击\t" + FormatCountNRatio(repeat2, 2),
            "同键3连击\t" + FormatCountNRatio(repeat3, 3),
            "同键4连击\t" + FormatCountNRatio(repeat4, 4),
            "------编码------",
            code
        ];

        static bool IsRepeat(params char[] cs) {
            foreach (var c in cs.AsSpan(1))
                if (c != cs[0])
                    return false;
            return true;
        }

        static bool XContains(HashSet<char> set1, HashSet<char> set2, char c1, char c2)
            => (set1.Contains(c1) && set2.Contains(c2))
            || (set1.Contains(c2) && set2.Contains(c1));

        void Count2Chars(char c1, char c2) {
            if (IsRepeat(c1, c2))
                _ = Interlocked.Increment(ref repeatCount[0]);
            else if (SameFinger(c1, c2))
                if (IsLeap1(c1, c2))
                    _ = Interlocked.Increment(ref leapCount[0]);
                else if (IsLeap2(c1, c2))
                    _ = Interlocked.Increment(ref leapCount[1]);
                else if (IsLeap3(c1, c2))
                    _ = Interlocked.Increment(ref leapCount[2]);
        }

        void Count3Chars(char c1, char c2, char c3) {
            if (IsRepeat(c1, c2, c3))
                _ = Interlocked.Increment(ref repeatCount[1]);
            else if (IsTurn(c1, c2, c3))
                _ = Interlocked.Increment(ref turnCount);
        }

        bool SameFinger(char c1, char c2) {
            foreach (var finger in layout.Lefts)
                if (finger.Contains(c1) && finger.Contains(c2))
                    return true;
            foreach (var finger in layout.Rights)
                if (finger.Contains(c1) && finger.Contains(c2))
                    return true;
            return false;
        }

        bool IsLeap1(char c1, char c2)
            => XContains(layout.Rows[0], layout.Rows[1], c1, c2)
            || XContains(layout.Rows[1], layout.Rows[2], c1, c2)
            || XContains(layout.Rows[2], layout.Rows[3], c1, c2);

        bool IsLeap2(char c1, char c2)
            => XContains(layout.Rows[0], layout.Rows[2], c1, c2)
            || XContains(layout.Rows[1], layout.Rows[3], c1, c2);

        bool IsLeap3(char c1, char c2)
            => XContains(layout.Rows[0], layout.Rows[3], c1, c2);

        bool IsTurn(char c1, char c2, char c3)
            => (lSet.Contains(c1) && rSet.Contains(c2) && lSet.Contains(c3))
            || (rSet.Contains(c1) && lSet.Contains(c2) && rSet.Contains(c3));

        string FormatCountNRatio(int count, int involvedChars) {
            var max = code.Length - involvedChars + 1;
            var ratio = 100d * count / max;
            return $"{count}\t{ratio:0.####} %";
        }
    }
}
