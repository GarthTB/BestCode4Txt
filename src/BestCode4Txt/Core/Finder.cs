using System.Text;
using BestCode4Txt.Linker;
using BestCode4Txt.Models;

namespace BestCode4Txt.Core;

/// <summary> 最优编码查找器 </summary>
/// <param name="dict"> 词库：词的最优编码及其开销 </param>
/// <param name="linker"> 连接器：用于连接编码及其开销 </param>
internal sealed class Finder(CcDict dict, ILinker linker)
{
    /// <summary> 计算最优编码及其开销 </summary>
    /// <param name="path"> 待编码的文本文件路径 </param>
    /// <param name="textLen"> 文本字数 </param>
    public CodeCost Run(string path, out int textLen) {
        var text = File.ReadAllText(path);
        textLen = text.Length;

        // 各位置的最优编码及其开销，初始为null
        var roots = new CodeCost?[text.Length + 1];
        roots[0] = new("", 0); // 开头空串
        // 最远编码末端索引
        var furthest = 0;
        // 固化最优路径以加速拼接
        StringBuilder route = new(text.Length * 2); // 设平均码长为2
        // 可复用的CodeCost集：索引=词长-1
        List<CodeCost?> ccsByLen = new(text.Length);

        for (var i = 0; i < text.Length; i++) {
            // 取出root：一定可达
            var cc1 = roots[i]!.Value;

            // 已无其他root：固化前部，只留2字
            if (i == furthest && cc1.Code.Length > 3) {
                _ = route.Append(cc1.Code, 0, cc1.Code.Length - 2);
                cc1 = cc1 with { Code = cc1.Code[^2..] };
            }

            // 获取文本所有起始词的最优CodeCost并更新最优编码
            _ = dict.UpdateCcs(text.AsSpan(i), ccsByLen);
            for (var j = 0; j < ccsByLen.Count; j++) {
                var cc2 = ccsByLen[j];
                if (cc2.HasValue) {
                    var (cost, getCode) = linker.Link(cc1, cc2.Value);
                    UpdateMin(i + j + 1, cost, getCode);
                }
            }

            // 兜底：原字填入
            if (!roots[i + 1].HasValue) {
                CodeCost cc2 = new(text[i].ToString(), 0);
                var (cost, getCode) = linker.Link(cc1, cc2);
                UpdateMin(i + 1, cost, getCode);
            }
        }

        var last = roots[^1]!.Value;
        return new(route.Append(last.Code).ToString(), last.Cost);

        // 更新词尾处的最优编码
        void UpdateMin(int i, double cost, Func<string> getCode) {
            ref var tgt = ref roots[i];
            if (!tgt.HasValue || tgt.Value.Cost > cost)
                tgt = new(getCode(), cost);
            if (i > furthest)
                furthest = i;
        }
    }
}
