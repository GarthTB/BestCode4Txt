using BestCode4Txt.Models;

namespace BestCode4Txt.Core;

/// <summary> 编码分析器 </summary>
internal static class Reviewer
{
    /// <summary> 分析编码及其开销，返回报告 </summary>
    /// <param name="cc"> 编码及其开销 </param>
    /// <param name="textLen"> 文本字数 </param>
    /// <param name="rowMap"> 键表：数字-底排值分别为0-4 </param>
    /// <param name="fingerMap"> 键表：左拇指-小指值分别为0-4、右拇指-小指值分别为5-9 </param>
    public static List<string> Run(
        CodeCost cc,
        int textLen,
        Dictionary<char, byte> rowMap,
        Dictionary<char, byte> fingerMap) {
        var (code, cost) = cc;
        ArgumentException.ThrowIfNullOrEmpty(code, nameof(cc));

        List<string> report = new(40) {
            "------数据------",
            $"总字数\t{textLen}",
            $"总码数\t{code.Length}",
            $"总开销\t{cost:0.######}",
            $"字均码长\t{1d * code.Length / textLen:0.######}",
            $"字均开销\t{cost / textLen:0.######}",
            $"码均开销\t{cost / code.Length:0.######}"
        };
        if (code.Length < 5) {
            report.AddRange([
                "编码太短，不详细分析",
                "------编码------",
                code]);
            return report;
        }

        var rowCnt = new int[5]; // 各排计数
        var fingerCnt = new int[10]; // 各手指计数
        var leapCnt = new int[3]; // 同指跨1、2、3排计数
        var repeatCnt = new int[4]; // 2、3、4、5+连击计数
        var turnCnt = 0; // 左右左、右左右计数（用于计算互击）

        _ = Parallel.For(0, code.Length, i => {
            CntRowFinger(code[i]);
            if (i > 0)
                CntLeapRepeat(code[i - 1], code[i]);
            if (i > 1)
                CntTurnRepeat(code[i - 2], code[i - 1], code[i]);
            if (i > 2
                && code[i] == code[i - 1]
                && code[i] == code[i - 2]
                && code[i] == code[i - 3])
                _ = Interlocked.Increment(ref repeatCnt[2]);
            if (i > 3
                && code[i] == code[i - 1]
                && code[i] == code[i - 2]
                && code[i] == code[i - 3]
                && code[i] == code[i - 4])
                _ = Interlocked.Increment(ref repeatCnt[3]);
        });

        var leftSum = fingerCnt[..4].Sum();
        var rightSum = fingerCnt[5..].Sum();
        var bias = 100d * Math.Abs(leftSum - rightSum) / (leftSum + rightSum);
        var repeat2 = repeatCnt[0] - repeatCnt[1];
        var repeat3 = repeatCnt[1] - repeatCnt[2];
        var repeat4 = repeatCnt[2] - repeatCnt[3];

        report.AddRange([
            $"偏倚\t{bias:0.######} %",
            "互击\t" + FormatCntRatio(turnCnt, 3),
            "----------------",
            "数字排\t" + FormatCntRatio(rowCnt[0], 1),
            "上排\t" + FormatCntRatio(rowCnt[1], 1),
            "中排\t" + FormatCntRatio(rowCnt[2], 1),
            "下排\t" + FormatCntRatio(rowCnt[3], 1),
            "底排\t" + FormatCntRatio(rowCnt[4], 1),
            "----------------",
            "总左手\t" + FormatCntRatio(leftSum, 1),
            "1指\t" + FormatCntRatio(fingerCnt[0], 1),
            "2指\t" + FormatCntRatio(fingerCnt[1], 1),
            "3指\t" + FormatCntRatio(fingerCnt[2], 1),
            "4指\t" + FormatCntRatio(fingerCnt[3], 1),
            "5指\t" + FormatCntRatio(fingerCnt[4], 1),
            "----------------",
            "总右手\t" + FormatCntRatio(rightSum, 1),
            "1指\t" + FormatCntRatio(fingerCnt[0], 1),
            "2指\t" + FormatCntRatio(fingerCnt[1], 1),
            "3指\t" + FormatCntRatio(fingerCnt[2], 1),
            "4指\t" + FormatCntRatio(fingerCnt[3], 1),
            "5指\t" + FormatCntRatio(fingerCnt[4], 1),
            "----------------",
            "同指跨1排\t" + FormatCntRatio(leapCnt[0], 2),
            "同指跨2排\t" + FormatCntRatio(leapCnt[1], 2),
            "同指跨3排\t" + FormatCntRatio(leapCnt[2], 2),
            "----------------",
            "同键2连击\t" + FormatCntRatio(repeat2, 2),
            "同键3连击\t" + FormatCntRatio(repeat3, 3),
            "同键4连击\t" + FormatCntRatio(repeat4, 4),
            "------编码------",
            code
        ]);
        return report;

        void CntRowFinger(char c) {
            if (rowMap.TryGetValue(c, out var rowIdx))
                _ = Interlocked.Increment(ref rowCnt[rowIdx]);
            if (fingerMap.TryGetValue(c, out var fingerIdx))
                _ = Interlocked.Increment(ref fingerCnt[fingerIdx]);
        }

        void CntLeapRepeat(char c1, char c2) {
            if (c1 == c2)
                _ = Interlocked.Increment(ref repeatCnt[0]);
            else if (fingerMap.TryGetValue(c1, out var f1)
                && fingerMap.TryGetValue(c2, out var f2)
                && f1 == f2
                && rowMap.TryGetValue(c1, out var r1) && r1 < 4 // 不是底排
                && rowMap.TryGetValue(c2, out var r2) && r2 < 4) {
                var idx = Math.Abs(r1 - r2) - 1;
                _ = Interlocked.Increment(ref leapCnt[idx]);
            }
        }

        void CntTurnRepeat(char c1, char c2, char c3) {
            if (c1 == c2 && c1 == c3)
                _ = Interlocked.Increment(ref repeatCnt[1]);
            else if (fingerMap.TryGetValue(c1, out var f1)
                && fingerMap.TryGetValue(c2, out var f2)
                && fingerMap.TryGetValue(c3, out var f3)
                && ((f1 < 5 && f2 > 4 && f3 < 5) || (f1 > 4 && f2 < 5 && f3 > 4))
                && rowMap.TryGetValue(c1, out var r1) && r1 < 4 // 不是底排
                && rowMap.TryGetValue(c2, out var r2) && r2 < 4 // 不是底排
                && rowMap.TryGetValue(c3, out var r3) && r3 < 4)
                _ = Interlocked.Increment(ref turnCnt);
        }

        string FormatCntRatio(int cnt, int involvedCnt) {
            var max = code.Length - involvedCnt + 1;
            var ratio = 100d * cnt / max;
            return $"{cnt}\t{ratio:0.######} %";
        }
    }
}
