using System.Text;
using BestCode4Txt.Models;

namespace BestCode4Txt.Linking;

/// <summary> 用于连接CodeCost </summary>
internal interface ILinker
{
    /// <summary> 将cc中的编码追加到code，开销累加到cost </summary>
    /// <param name="code"> 先前的编码 </param>
    /// <param name="cost"> 先前的开销 </param>
    /// <param name="cc"> 新来的CodeCost </param>
    /// <returns> 累加后的开销 </returns>
    double Link(StringBuilder code, double cost, CodeCost cc);
}
