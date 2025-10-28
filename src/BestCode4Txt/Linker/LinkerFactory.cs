using BestCode4Txt.Enums;
using BestCode4Txt.Linker.Impls;

namespace BestCode4Txt.Linker;

/// <summary> 编码及其开销连接器工厂 </summary>
internal static class LinkerFactory
{
    /// <summary> 构造指定策略的编码及其开销连接器 </summary>
    public static ILinker Create(LinkStrat strat, Models.CostMap costs)
        => strat switch {
            LinkStrat.SpaceOrPunct => new SpaceOrPunctLinker(costs),
            LinkStrat.NoGap => new NoGapLinker(costs),
            LinkStrat.Jd6 => new Jd6Linker(costs),
            _ => throw new ArgumentException($"连接策略 '{strat}' 无效", nameof(strat))
        };
}
