using System.Text;
using BestCode4Txt.Models;

namespace BestCode4Txt.Linker;

/// <summary> 用于连接编码及其开销 </summary>
internal interface ILinker
{
    /// <summary> 将cc中的编码追加到code，开销累加到cost </summary>
    /// <param name="code"> 先前的编码 </param>
    /// <param name="cost"> 先前的开销 </param>
    /// <param name="cc"> 尾部编码及其开销 </param>
    /// <returns> 累加后的开销 </returns>
    double Link(StringBuilder code, double cost, CodeCost cc);
}
