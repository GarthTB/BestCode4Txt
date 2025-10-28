using BestCode4Txt.Models;

namespace BestCode4Txt.Linker;

/// <summary> 用于连接编码及其开销 </summary>
internal interface ILinker
{
    /// <summary> 连接编码及其开销 </summary>
    /// <param name="cc1"> 前编码及其开销 </param>
    /// <param name="cc2"> 后编码及其开销 </param>
    /// <returns> 总开销、用于获取总编码的闭包 </returns>
    (double Cost, Func<string> GetCode) Link(CodeCost cc1, CodeCost cc2);
}
