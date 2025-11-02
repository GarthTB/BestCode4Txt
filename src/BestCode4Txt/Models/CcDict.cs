using static System.Runtime.InteropServices.CollectionsMarshal;

namespace BestCode4Txt.Models;

/// <summary> 词库：存储词的最优CodeCost </summary>
internal sealed class CcDict // Trie结构
{
    /// <summary> 根节点 </summary>
    private readonly Node _root = new(new(4096)); // 假设覆盖常用字

    /// <summary> 节点：开销最小的CodeCost和子节点 </summary>
    private sealed record Node(Dictionary<char, Node> Sons)
    { public CodeCost? BestCc; } // 初始为null

    /// <summary> 将词库条目集构造为Trie </summary>
    public CcDict(IEnumerable<(string Word, CodeCost Cc)> entries) {
        foreach (var (word, cc) in entries) {
            if (word.Length > MaxWordLen)
                MaxWordLen = word.Length;
            var cur = _root;
            foreach (var c in word) {
                ref var son = ref GetValueRefOrAddDefault(cur.Sons, c, out var exists);
                cur = exists
                    ? son!
                    : son = new([]);
            }
            if (!(cur.BestCc?.Cost <= cc.Cost))
                cur.BestCc = cc;
        }
        if (_root.Sons.Count == 0 && !_root.BestCc.HasValue)
            throw new ArgumentException("词库条目集为空", nameof(entries));
    }

    /// <summary> 最大词长：用于初始化最优CodeCost集 </summary>
    public int MaxWordLen { get; private set; }

    /// <summary> 更新文本所有起始词的最优CodeCost集 </summary>
    /// <param name="text"> 待编码的文本 </param>
    /// <param name="ccs"> CodeCost集：索引=词长-1 </param>
    /// <returns> 是否需要加载下一块文本 </returns>
    public bool UpdateCcs(ReadOnlySpan<char> text, IList<CodeCost?> ccs) {
        ccs.Clear();
        var cur = _root;
        foreach (var c in text)
            if (cur.Sons.TryGetValue(c, out cur))
                ccs.Add(cur.BestCc);
            else return false;
        return true;
    }
}
