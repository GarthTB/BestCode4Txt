using BestCode4Txt.Models;

namespace BestCode4Txt.Linker.Impls;

/// <summary> 编码及其开销连接器：无间隔 </summary>
/// <param name="costs"> 键对-开销表 </param>
internal sealed class NoGapLinker(CostMap costs): ILinker
{
    public (double Cost, Func<string> GetCode) Link(CodeCost cc1, CodeCost cc2) {
        ArgumentException.ThrowIfNullOrEmpty(cc2.Code, nameof(cc2));

        if (cc1.Code.Length == 0)
            return (cc2.Cost, () => cc2.Code);

        var gap = costs[cc1.Code[^1], cc2.Code[0]];
        var cost = cc1.Cost + gap + cc2.Cost;
        return (cost, () => cc1.Code + cc2.Code);
    }
}
