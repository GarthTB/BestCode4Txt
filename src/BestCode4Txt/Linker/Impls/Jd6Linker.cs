using System.Text;
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

    public double Link(StringBuilder code, double cost, CodeCost cc) {
        if (cc.Code.Length == 0)
            throw new ArgumentException("后码为空", nameof(cc));

        // 1. 后码不足4码、终为音码：后码加空格
        var endSpace = cc.Code.Length < 4 && _ym.Contains(cc.Code[^1]);
        var cost2 = endSpace
            ? cc.Cost + costs[cc.Code[^1], ' ']
            : cc.Cost;

        // 2. 前码为空：直接返回尾部
        if (code.Length == 0) {
            _ = endSpace
                ? code.Append(cc.Code).Append(' ')
                : code.Append(cc.Code);
            return cost2;
        }

        var code1L = code[^1];
        var code2F = cc.Code[0];
        // 后码不能顶功：始为形码或数字
        var notDG = _xm.Contains(code2F) || char.IsDigit(code2F);

        // 3.1. 前码终为音码、后码不能顶功：前码加空格
        if (_ym.Contains(code1L) && notDG) {
            _ = code.Append(' ');
            cost += costs[code1L, ' '];
        } else if (code.Length > 1) { // 3和4相斥
            var code1P = code[^2];
            // 3.2. 前码终为音码+形码、后码不能顶功：前码加空格
            if (_ym.Contains(code1P) && _xm.Contains(code1L) && notDG) {
                _ = code.Append(' ');
                cost += costs[code1L, ' '];
            }
            // 4(弥补1). 前码终为音码+空格、后码始为标点：前码删空格
            else if (_ym.Contains(code1P)
                && code1L == ' '
                && code2F != ' ' // 不为空格，否则吞掉连续空格
                && !char.IsLetterOrDigit(code2F)) {
                _ = code.Remove(code.Length - 1, 1);
                cost -= costs[code1P, ' '];
            }
        }

        _ = endSpace
            ? code.Append(cc.Code).Append(' ')
            : code.Append(cc.Code);
        return cost + costs[code[^1], code2F] + cost2; // code有变，不能用code1L
    }
}
