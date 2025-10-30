using BestCode4Txt.Models;

namespace BestCode4Txt.Linker.Impls;

/// <summary> 编码及其开销连接器：空格或标点 </summary>
/// <param name="costs"> 键对-开销表 </param>
internal sealed class SpaceOrPunctLinker(CostMap costs): ILinker
{
    public CodeCost Link(CodeCost cc1, CodeCost cc2) {
        ArgumentException.ThrowIfNullOrEmpty(cc2.Code, nameof(cc2));

        if (cc1.Code.Length == 0)
            return cc2;

        var code1L = cc1.Code[^1];
        var code2F = cc2.Code[0];

        // 前码终为字母、后码始无空格或标点：加空格
        if (char.IsLetter(code1L) && char.IsLetterOrDigit(code2F)) {
            var gap = costs[code1L, ' '] + costs[' ', code2F];
            var cost = cc1.Cost + gap + cc2.Cost;
            return new($"{cc1.Code} {cc2.Code}", cost);
        } else {
            var gap = costs[code1L, code2F];
            var cost = cc1.Cost + gap + cc2.Cost;
            return new(cc1.Code + cc2.Code, cost);
        }
    }
}
