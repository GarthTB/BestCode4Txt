namespace BestCode4Txt.Config;

/// <summary> 主配置 </summary>
/// <param name="InPath"> 待编码的文本输入路径 </param>
/// <param name="Strat"> 编码及其开销的连接策略 </param>
/// <param name="OutPath"> 最优编码及其分析报告输出路径（存在则覆写） </param>
/// <param name="DictPath"> 词库（RIME格式）路径 </param>
/// <param name="CostsPath"> 键对-开销（当量）路径 </param>
/// <param name="Layout"> 键盘布局配置 </param>
internal sealed record MainCfg(
    string InPath,
    Enums.LinkStrat Strat,
    string OutPath,
    string DictPath,
    string CostsPath,
    LayoutCfg Layout)
{
    /// <summary> 加载文件并构造为MainCfg </summary>
    public static MainCfg Load(string path) {
        var text = File.ReadAllText(path);
        var toml = Tomlyn.Toml.ToModel(text);
        return toml["input_path"] is string inPath
            && toml["link_strat"] is string sStrat
            && Enum.TryParse(sStrat, true, out Enums.LinkStrat strat)
            && toml["output_path"] is string outPath
            && toml["dict_path"] is string dictPath
            && toml["costs_path"] is string costsPath
            && toml["layout_path"] is string layoutPath
            && LayoutCfg.Load(layoutPath) is var layout
            ? new(inPath, strat, outPath, dictPath, costsPath, layout)
            : throw new ArgumentException($"文件 '{path}' 解析失败", nameof(path));
    }
}
