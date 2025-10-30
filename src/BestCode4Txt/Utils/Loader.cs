using BestCode4Txt.Models;

namespace BestCode4Txt.Utils;

/// <summary> 加载器：加载键对-开销表、词库 </summary>
internal static class Loader
{
    /// <summary> 加载键对-开销表：键为键对，值为开销 </summary>
    public static CostMap LoadCosts(string path) {
        List<(char C1, char C2, double Cost)> costs = new(2116);
        foreach (var line in File.ReadLines(path))
            if (line.Split('\t') is { Length: 2 } parts // 保留空格
                && parts[0].Length == 2
                && double.TryParse(parts[1], out var cost))
                costs.Add((parts[0][0], parts[0][1], cost));
        return new(costs);
    }

    /// <summary> 加载词库：词的最优编码及其开销 </summary>
    public static CcDict LoadDict(string path, CostMap costs) {
        HashSet<string> usedCodes = [];
        var entries = File.ReadLines(path)
            .Select(static line => {
                var idx = line.IndexOf('#');
                var bare = idx < 0 ? line : line[..idx];
                var parts = bare.Split('\t');
                return parts.Length switch {
                    2 => (parts[0], parts[1], 0d),
                    3 when double.TryParse(parts[2], out var cost)
                        => (parts[0], parts[1], cost),
                    _ => ("", "", 0d)
                };
            })
            .Where(static x => x.Item1.Length > 0 && x.Item2.Length > 0)
            .OrderByDescending(static x => x.Item3) // 权重降序
            .ThenBy(static x => x.Item2.Length) // 码长升序
            .ThenBy(x => costs.GetCost(x.Item2)) // 临时开销升序
            .Select(x => {
                var (word, code, _) = x;
                DistinctCode(ref code);
                var cost = costs.GetCost(code);
                return (word, new CodeCost(code, cost));
            });
        return new(entries);

        void DistinctCode(ref string code) {
            for (var (root, i) = (code, 2); !usedCodes.Add(code);)
                (code, i) = i < 10
                    ? (root + i, i + 1) // 不到10：数字选重
                    : ((root += '=') + ' ', 2); // 到10：=翻页、空格首选
        }
    }
}
