using System.Text;
using BestCode4Txt.Models;

namespace BestCode4Txt.Linker.Impls;

/// <summary> 编码及其开销连接器：无间隔 </summary>
/// <param name="costs"> 按键组合开销表 </param>
internal sealed class NoGapLinker(CostMap costs): ILinker
{
    public double Link(StringBuilder code, double cost, CodeCost cc) {
        if (cc.Code.Length == 0)
            throw new ArgumentException("后码为空", nameof(cc));

        _ = code.Append(cc.Code);
        return code.Length == 0
            ? cc.Cost
            : cost + costs[code[^1], cc.Code[0]] + cc.Cost;
    }
}
