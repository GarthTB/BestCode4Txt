using Tomlyn.Model;

namespace BestCode4Txt.Config;

/// <summary> 键盘布局配置 </summary>
/// <param name="Rows"> 数字、上、中、下、底排键 </param>
/// <param name="Lefts"> 左拇、食、中、无名、小指键 </param>
/// <param name="Rights"> 右拇、食、中、无名、小指键 </param>
internal sealed record LayoutCfg(
    HashSet<char>[] Rows,
    HashSet<char>[] Lefts,
    HashSet<char>[] Rights)
{
    /// <summary> 加载文件并构造为LayoutCfg </summary>
    public static LayoutCfg Load(string path) {
        var text = File.ReadAllText(path);
        var toml = Tomlyn.Toml.ToModel(text);
        return toml["rows"] is TomlArray tRows
            && Parse(tRows) is { Length: 5 } rows
            && toml["lefts"] is TomlArray tLefts
            && Parse(tLefts) is { Length: 5 } lefts
            && toml["rights"] is TomlArray tRights
            && Parse(tRights) is { Length: 5 } rights
            ? new(rows, lefts, rights)
            : throw new ArgumentException($"文件 '{path}' 解析失败", nameof(path));
    }

    /// <summary> 解析数组为HashSet </summary>
    private static HashSet<char>[] Parse(TomlArray arr)
        => [.. arr.Select(
            static obj => obj is string str
            ? str.ToHashSet()
            : throw new ArgumentException($"数组元素 '{obj}' 无效", nameof(arr)))];
}
