using BestCode4Txt.Models;

namespace BestCode4Txt.Linker.Impls;

/// <summary> 编码及其开销连接器：键道6顶功 </summary>
/// <param name="costs"> 键对-开销表 </param>
internal sealed class Jd6Linker(CostMap costs): ILinker
{
    /// <summary> 键道6的形码 </summary>
    private readonly HashSet<char> _xm = [.. "aiouvAIOUV"];

    /// <summary> 键道6的音码 </summary>
    private readonly HashSet<char> _ym
        = [.. "bcdefghjklmnpqrstwxyzBCDEFGHJKLMNPQRSTWXYZ"];

    public CodeCost Link(CodeCost cc1, CodeCost cc2) {
        var (code1, cost1) = cc1;
        var (code2, cost2) = cc2;
        ArgumentException.ThrowIfNullOrEmpty(code2, nameof(cc2));

        // 1. 后码不足4码、终为音码：后码加空格
        if (code2.Length < 4 && _ym.Contains(code2[^1])) {
            code2 += ' ';
            cost2 += costs[code2[^1], ' '];
            // 2. 前码为空：返回尾部
            if (code1.Length == 0)
                return new(code2, cost2);
        }
        // 2. 前码为空：直接返回尾部
        if (code1.Length == 0)
            return cc2;

        var code1L = code1[^1];
        var code2F = code2[0];
        // 后码不能顶功：始为形码或数字
        var notDG = _xm.Contains(code2F) || char.IsDigit(code2F);

        // 3.1. 前码终为音码、后码不能顶功：前码加空格
        if (_ym.Contains(code1L) && notDG) {
            code1 += ' ';
            cost1 += costs[code1L, ' '];
        } else if (code1.Length > 1) {
            var code1P = code1[^2];
            // 3.2. 前码终为音码+形码、后码不能顶功：前码加空格
            if (_ym.Contains(code1P) && _xm.Contains(code1L) && notDG) {
                code1 += ' ';
                cost1 += costs[code1L, ' '];
            }
            // 4(弥补1). 前码终为音码+空格、后码始为标点：前码删空格
            else if (_ym.Contains(code1P)
                && code1L == ' '
                && code2F != ' ' // 不为空格，否则吞掉连续空格
                && !char.IsLetterOrDigit(code2F)) {
                code1 = code1[..^1];
                cost1 -= costs[code1P, ' '];
            }
        }

        var gap = costs[code1[^1], code2F]; // code有变，不能用code1L
        var cost = cost1 + gap + cost2;
        return new(code1 + code2, cost);
    }
}
