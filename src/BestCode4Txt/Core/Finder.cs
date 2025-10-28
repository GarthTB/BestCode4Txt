using System.Text;
using BestCode4Txt.Linker;
using BestCode4Txt.Models;

namespace BestCode4Txt.Core;

/// <summary> 最优编码查找器 </summary>
internal static class Finder
{
    /// <summary> 计算最优编码及其开销 </summary>
    /// <param name="path"> 待编码的文本文件路径 </param>
    /// <param name="dict"> 词库：词的最优编码及其开销 </param>
    /// <param name="linker"> 连接器：用于连接编码及其开销 </param>
    /// <param name="textLen"> 文本字数 </param>
    public static CodeCost Run(
        string path, CcDict dict, ILinker linker, out int textLen) {
        FileInfo fi = new(path);
        if (fi.Length < 1 << 24) {
            var text = File.ReadAllText(path);
            return (textLen = text.Length) == 0
                ? throw new ArgumentException("待编码的文本为空", nameof(path))
                : CalcAll(text, dict, linker);
        } else return CalcChunks(path, dict, linker, out textLen);
    }

    /// <summary> 一次性计算最优编码及其开销 </summary>
    /// <param name="dict"> 词库：词的最优编码及其开销 </param>
    /// <param name="linker"> 连接器：用于连接编码及其开销 </param>
    private static CodeCost CalcAll(string text, CcDict dict, ILinker linker) {
        // 各位置的最优编码及其开销，初始为null
        var roots = new CodeCost?[text.Length + 1];
        // 初始化开头
        roots[0] = new("", 0);
        // 暂存索引处的root并用于拼接
        StringBuilder root = new(text.Length * 2); // 设平均码长为2
        // 可复用的CodeCost集：索引=词长-1
        List<CodeCost?> ccsByLen = new(text.Length);

        for (var i = 0; i < text.Length; i++) {
            // 取出root，不可能不可达
            var (code, cost) = roots[i]!.Value;
            // 初始化root和修补子
            _ = root.Clear().Append(code);
            var intactLen = code.Length;
            var tail = code[^1];

            // 获取文本所有起始词的最优CodeCost
            _ = dict.UpdateCcs(text.AsSpan(i), ccsByLen);
            if (ccsByLen.Count == 0) { // 词库无词：原字填入
                CodeCost cc = new(text[i].ToString(), 0);
                var newCost = linker.Link(root, cost, cc);
                UpdateMin(i + 1, newCost);
                HealRoot(intactLen, tail);
            } else for (var j = 0; j < ccsByLen.Count; j++) {
                    var cc = ccsByLen[j];
                    if (cc.HasValue) {
                        var newCost = linker.Link(root, cost, cc.Value);
                        UpdateMin(i + j + 1, newCost);
                        HealRoot(intactLen, tail);
                    }
                }
        }
        return roots[^1]!.Value;

        // 更新词尾处的最优编码
        void UpdateMin(int i, double newCost) {
            ref var tgt = ref roots[i];
            if (!tgt.HasValue || tgt.Value.Cost > newCost)
                tgt = new(root.ToString(), newCost);
        }

        // 修补root：Link方法会追加，并可能破坏末个字符
        void HealRoot(int intactLen, char tail) {
            root.Length = intactLen;
            _ = root.Append(tail);
        }
    }

    /// <summary> 分块计算最优编码及其开销 </summary>
    /// <param name="path"> 待编码的文本文件路径 </param>
    /// <param name="dict"> 词库：词的最优编码及其开销 </param>
    /// <param name="linker"> 连接器：用于连接编码及其开销 </param>
    /// <param name="textLen"> 文本字数 </param>
    private static CodeCost CalcChunks(
        string path, CcDict dict, ILinker linker, out int textLen) {
    }
}
