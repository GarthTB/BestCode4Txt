using System.Text;
using BestCode4Txt.Linker;
using BestCode4Txt.Models;

namespace BestCode4Txt.Core;

/// <summary> 最优编码查找器 </summary>
/// <param name="dict"> 词库：词的最优编码及其开销 </param>
/// <param name="linker"> 连接器：用于连接编码及其开销 </param>
internal sealed class Finder(CcDict dict, ILinker linker)
{
    /// <summary> 分块字符数：流式处理 </summary>
    private const int CHUNK_CHARS = 1 << 16;

    /// <summary> 待编码的文本 </summary>
    private string _text = "";

    /// <summary> 各位置的最优编码及其开销，初始为null </summary>
    private CodeCost?[] _roots = [new("", 0)]; // 启动子

    /// <summary> 当前索引、最远编码末端索引 </summary>
    private int _curIdx, _maxIdx;

    /// <summary> 已固化的最优路径，用于加速拼接 </summary>
    private readonly StringBuilder _route = new(CHUNK_CHARS * 2); // 设码长为2

    /// <summary> 可复用的CodeCost集：索引=词长-1 </summary>
    private readonly List<CodeCost?> _ccsByLen = new(dict.MaxWordLen);

    /// <summary> 计算最优编码及其开销 </summary>
    /// <param name="inPath"> 待编码的文本文件路径 </param>
    /// <param name="textLen"> 文本字数 </param>
    public CodeCost Run(string inPath, out int textLen) {
        using StreamReader reader = new(inPath);
        var buf = new char[CHUNK_CHARS];
        var cnt = textLen = 0;

        while ((cnt = reader.Read(buf, 0, CHUNK_CHARS)) > 0) {
            Console.Write($"\r第 {textLen} - {textLen += cnt} 字处理中...");
            // 取出余部
            var restText = _text[_curIdx..];
            var restRoots = _roots[_curIdx..];
            // 加载新块
            _text = restText + new string(buf, 0, cnt);
            _roots = [.. restRoots, .. new CodeCost?[cnt]];
            // 循环处理
            for ((_curIdx, _maxIdx) = (0, _maxIdx - _curIdx);
                ProcCurIdx(true);
                _curIdx++) ;
        }
        for (; _curIdx < _text.Length; _curIdx++)
            _ = ProcCurIdx(false);

        Console.WriteLine($"\n处理完成，共 {textLen} 字。");
        var last = _roots[^1]!.Value;
        return new(_route.Append(last.Code).ToString(), last.Cost);
    }

    /// <summary> 处理当前索引处的编码及其开销 </summary>
    /// <param name="breakAtEnd"> 是否在到达文本末端时打断 </param>
    /// <returns> 文本是否还有未编码部分 </returns>
    private bool ProcCurIdx(bool breakAtEnd) {
        // 取出root：一定可达
        var cc1 = _roots[_curIdx]!.Value;
        // 已无其他root：固化前部，只留2码
        if (_curIdx == _maxIdx && cc1.Code.Length > 2) {
            _ = _route.Append(cc1.Code, 0, cc1.Code.Length - 2);
            cc1 = cc1 with { Code = cc1.Code[^2..] };
        }
        // 获取文本所有起始词的最优CodeCost
        if (breakAtEnd && !dict.UpdateCcs(_text.AsSpan(_curIdx), _ccsByLen))
            return false;
        // 更新最优CodeCost
        for (var i = 0; i < _ccsByLen.Count; i++) {
            var cc2 = _ccsByLen[i];
            if (cc2.HasValue)
                LinkAndUpdate(_curIdx + i + 1, cc2.Value);
        }
        // 兜底：原字填入
        if (!_roots[_curIdx + 1].HasValue) {
            CodeCost cc2 = new(_text[_curIdx].ToString(), 0);
            LinkAndUpdate(_curIdx + 1, cc2);
        }
        return true;

        void LinkAndUpdate(int endIdx, CodeCost cc2) {
            var cc = linker.Link(cc1, cc2);
            ref var tgt = ref _roots[endIdx];
            if (!(tgt?.Cost <= cc.Cost)) {
                tgt = cc;
                if (endIdx > _maxIdx)
                    _maxIdx = endIdx;
            }
        }
    }
}
