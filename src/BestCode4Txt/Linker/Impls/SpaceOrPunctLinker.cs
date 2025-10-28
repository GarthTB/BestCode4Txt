using System.Text;
using BestCode4Txt.Models;

namespace BestCode4Txt.Linker.Impls;

/// <summary> 编码及其开销连接器：空格或标点 </summary>
/// <param name="costs"> 按键组合开销表 </param>
internal sealed class SpaceOrPunctLinker(CostMap costs): ILinker
{
    public double Link(StringBuilder code, double cost, CodeCost cc) {
        if (cc.Code.Length == 0)
            throw new ArgumentException("后码为空", nameof(cc));

        // 1. 前码为空：直接返回尾部
        if (code.Length == 0)
            return (code.Append(cc.Code), cc.Cost).Cost;

        var code1L = code[^1];
        var code2F = cc.Code[0];
        // 2. 前码终为字母、后码始无空格或标点：加空格
        var needSpace = char.IsLetter(code1L) && char.IsLetterOrDigit(code2F);

        _ = needSpace
            ? code.Append(' ').Append(cc.Code)
            : code.Append(cc.Code);
        var gap = needSpace
            ? costs[code1L, ' '] + costs[' ', code2F]
            : costs[code1L, code2F];
        return cost + gap + cc.Cost;
    }
}
