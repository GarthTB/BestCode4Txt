using BestCode4Txt.Linker;
using BestCode4Txt.Linker.Impls;
using BestCode4Txt.Models;
using Tomlyn.Model;

namespace BestCode4Txt.Config;

/// <summary> 配置模型 </summary>
/// <param name="InPath"> 待编码的文本输入路径 </param>
/// <param name="Strat"> 编码及其开销的连接策略 </param>
/// <param name="OutPath"> 最优编码及其分析报告输出路径（存在则覆写） </param>
/// <param name="CostsPath"> 键对-开销（当量）路径 </param>
/// <param name="DictPath"> 词库（RIME格式）路径 </param>
/// <param name="Rows"> 数字、上、中、下、底排键 </param>
/// <param name="Lefts"> 左拇、食、中、无名、小指键 </param>
/// <param name="Rights"> 右拇、食、中、无名、小指键 </param>
internal sealed record Cfg(
    string InPath,
    string Strat,
    string OutPath,
    string CostsPath,
    string DictPath,
    string[] Rows,
    string[] Lefts,
    string[] Rights)
{
    /// <summary> 加载配置文件并构造为Cfg </summary>
    public static Cfg Load(string path) {
        var text = File.ReadAllText(path);
        var toml = Tomlyn.Toml.ToModel(text);
        return toml["in_path"] is string inPath
            && toml["strat"] is string strat
            && toml["out_path"] is string outPath
            && toml["costs_path"] is string costsPath
            && toml["dict_path"] is string dictPath
            && toml["layout"] is TomlTable layoutTable
            && layoutTable["rows"] is TomlArray rowsArray
            && ParseArray(rowsArray) is { Length: 5 } rows
            && layoutTable["lefts"] is TomlArray leftsArray
            && ParseArray(leftsArray) is { Length: 5 } lefts
            && layoutTable["rights"] is TomlArray rightsArray
            && ParseArray(rightsArray) is { Length: 5 } rights
            ? new(inPath, strat, outPath, dictPath, costsPath, rows, lefts, rights)
            : throw new ArgumentException($"配置文件 '{path}' 解析失败", nameof(path));

        static string[] ParseArray(TomlArray arr)
            => [.. arr.Select(
                static obj => obj is string str
                ? str
                : throw new ArgumentException($"Toml数组元素 '{obj}' 无效", nameof(path)))];
    }

    /// <summary> 根据配置构造编码及其开销连接器 </summary>
    public ILinker GetLinker()
        => Strat.ToLower() switch {
            "spaceorpunct" => new SpaceOrPunctLinker(),
            "nogap" => new NoGapLinker(),
            "jd6" => new Jd6Linker(),
            _ => throw new ArgumentException($"连接策略 '{Strat}' 无效", nameof(Strat))
        };

    /// <summary> 根据配置获取键对-开销表：键为键对，值为开销 </summary>
    public CostMap GetCosts() {
        List<(char C1, char C2, double Cost)> costs = new(2116);
        foreach (var line in File.ReadLines(CostsPath))
            if (line.Split('\t') is { Length: 2 } parts // 保留空格
                && parts[0].Length == 2
                && double.TryParse(parts[1], out var cost))
                costs.Add((parts[0][0], parts[0][1], cost));
        return new(costs);
    }

    /// <summary> 根据配置获取词库：词的最优编码及其开销 </summary>
    public CcDict GetDict(CostMap costs) {
        HashSet<string> usedCodes = [];
        var entries = File.ReadLines(DictPath)
            .Select(line => {
                var idx = line.IndexOf('#');
                var bare = idx < 0 ? line : line[..idx];
                var parts = bare.Split('\t');
                return parts.Length switch {
                    2 => (parts[0], parts[1], costs.GetCost(parts[1]), 0d),
                    3 when double.TryParse(parts[2], out var weight)
                        => (parts[0], parts[1], costs.GetCost(parts[1]), weight),
                    _ => ("", "", 0d, 0d)
                };
            })
            .Where(static x => x.Item1.Length > 0 && x.Item2.Length > 0)
            .OrderByDescending(static x => x.Item4) // 权重降序
            .ThenBy(static x => x.Item2.Length) // 码长升序
            .ThenBy(static x => x.Item3) // 临时开销升序
            .Select(x => {
                var (word, code, _, _) = x;
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

    /// <summary> 根据配置获取键表：数字-底排值分别为0-4 </summary>
    public Dictionary<char, byte> GetRowMap() {
        Dictionary<char, byte> map = new(46); // 默认46键
        for (byte i = 0; i < 5; i++)
            foreach (var c in Rows[i])
                if (!map.TryAdd(c, i))
                    throw new ArgumentException($"键 '{c}' 分在多行上", nameof(Rows));
        return map;
    }

    /// <summary> 根据配置获取键表：左拇指-小指值分别为0-4、右拇指-小指值分别为5-9 </summary>
    public Dictionary<char, byte> GetFingerMap() {
        Dictionary<char, byte> map = new(46); // 默认46键
        for (byte i = 0; i < 5; i++)
            foreach (var c in Lefts[i])
                if (!map.TryAdd(c, i))
                    throw new ArgumentException($"键 '{c}' 分在多指上", nameof(Lefts));
        for (byte i = 5; i < 10; i++)
            foreach (var c in Rights[i - 5])
                if (!map.TryAdd(c, i))
                    throw new ArgumentException($"键 '{c}' 分在多指上", nameof(Rights));
        return map;
    }
}
