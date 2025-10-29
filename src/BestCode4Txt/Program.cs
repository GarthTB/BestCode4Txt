using BestCode4Txt.Config;
using BestCode4Txt.Core;
using BestCode4Txt.Linker;
using BestCode4Txt.Utils;
using static System.Console;

try {
    WriteLine("BestCode4Txt | 最优编码查找器");
    WriteLine("v1.0.0 (20251030)");
    WriteLine("作者: GarthTB | 天卜 <g-art-h@outlook.com>");
    WriteLine("仓库: https://github.com/GarthTB/BestCode4Txt");

    WriteLine("加载配置...");
    var cfg = MainCfg.Load("Config.toml");
    var costs = Loader.LoadCosts(cfg.CostsPath);
    var dict = Loader.LoadDict(cfg.DictPath, costs);
    var linker = LinkerFactory.Create(cfg.Strat, costs);
    WriteLine("配置就绪！");

    WriteLine("开始编码...");
    var bestCc = new Finder(dict, linker)
        .Run(cfg.InPath, out var textLen);
    WriteLine("分析编码...");
    var report = Reviewer.Run(bestCc, textLen, cfg.Layout);

    if (costs.Missing.Count > 0) {
        report.Add("-缺失开销的键对-");
        var missing = costs.Missing.Select(static x => $"{x.C1}{x.C2}");
        report.AddRange(missing);
    }
    File.WriteAllLines(cfg.OutPath, report);
    WriteLine($"最优编码及其分析报告已存至: {cfg.OutPath}");
} catch (Exception ex) {
    ForegroundColor = ConsoleColor.Red;
    WriteLine("运行出错，中断！");
    WriteLine("错误信息：");
    WriteLine(ex.Message);
    WriteLine("堆栈跟踪：");
    WriteLine(ex.StackTrace);
    ResetColor();
} finally { WriteLine("程序结束！"); }
