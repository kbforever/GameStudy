using UnityEngine;

namespace Monopoly
{
    /// <summary>
    /// 人类玩家实现，目前直接复用基类 Player 的所有逻辑。
    /// 交互（按钮/UI）由外部系统控制，例如 PropertyTransactionTester、UI 面板等。
    /// </summary>
    public class HumanPlayer : Player
    {
        // 预留扩展：可以在这里加上与输入/UI 相关的逻辑
        // 当前阶段暂不需要额外代码。
    }
}

