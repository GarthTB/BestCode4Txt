namespace BestCode4Txt.Models;

/// <summary> 键对-开销表 </summary>
internal sealed class CostMap
{
    /// <summary> 开销表：键为键对，值为开销 </summary>
    private readonly Dictionary<(char C1, char C2), double> _costs = new(2116); // 默认46键

    /// <summary> 开销均值：作为缺省 </summary>
    private readonly double _mean;

    /// <summary> 缺失开销的键对 </summary>
    private readonly HashSet<(char C1, char C2)> _missing = [];

    /// <summary> 将键对-开销集构造为字典 </summary>
    public CostMap(IEnumerable<(char C1, char C2, double Cost)> costs) {
        var sum = 0d;
        foreach (var (c1, c2, cost) in costs) {
            sum += cost;
            if (!_costs.TryAdd((c1, c2), cost))
                throw new ArgumentException($"'{c1}{c2}' 的开销重复", nameof(costs));
        }
        if (_costs.Count == 0)
            throw new ArgumentException("键对-开销集为空", nameof(costs));
        _mean = sum / _costs.Count;
    }

    /// <summary> 缺失开销的键对 </summary>
    public IReadOnlySet<(char C1, char C2)> Missing => _missing;

    /// <summary> 获取键对的开销并记录缺失项 </summary>
    public double this[char c1, char c2]
        => _costs.TryGetValue((c1, c2), out var cost)
        ? cost
        : (_missing.Add((c1, c2)), _mean)._mean;

    /// <summary> 获取连续编码的总开销 </summary>
    public double GetCost(ReadOnlySpan<char> code) {
        ArgumentOutOfRangeException.ThrowIfZero(code.Length, nameof(code));
        var cost = 0d;
        for (var i = 1; i < code.Length; i++)
            cost += this[code[i - 1], code[i]];
        return cost;
    }
}
