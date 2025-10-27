using CM = System.Runtime.InteropServices.CollectionsMarshal;

namespace BestCode4Txt.Models;

/// <summary> 词库：存储词的最优CodeCost </summary>
internal sealed class CcDict // Trie结构
{
    /// <summary> 根节点 </summary>
    private readonly Node _root = new([]);

    /// <summary> 节点：开销最小的CodeCost和子节点 </summary>
    private sealed record Node(Dictionary<char, Node> Sons)
    { public CodeCost? BestCc; } // 初始为null

    /// <summary> 将词库条目集构造为Trie </summary>
    public CcDict(IEnumerable<(string Word, CodeCost Cc)> entries) {
        foreach (var (word, cc) in entries) {
            var cur = _root;
            foreach (var c in word) {
                ref var son = ref CM.GetValueRefOrAddDefault(
                    cur.Sons, c, out var exists); // 避免多次查找
                if (!exists)
                    son = new([]);
                cur = son!;
            }
            if (cur.BestCc is null
                || cur.BestCc.Value.Cost > cc.Cost)
                cur.BestCc = cc;
        }
    }

    /// <summary> 更新文本所有起始词的最优CodeCost集 </summary>
    /// <param name="text"> 待编码的文本 </param>
    /// <param name="bestCcs"> CodeCost集：索引=词长-1 </param>
    /// <returns> 是否抵达文本末尾 </returns>
    public bool UpdateCcs(ReadOnlySpan<char> text, IList<CodeCost?> bestCcs) {
        bestCcs.Clear();
        var cur = _root;
        foreach (var c in text)
            if (cur.Sons.TryGetValue(c, out cur))
                bestCcs.Add(cur.BestCc);
            else return false;
        return true;
    }
}
