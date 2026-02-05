using UnityEngine;

namespace Monopoly
{
    /// <summary>
    /// 用于测试的最小玩家实现（因为 Player 是 abstract，必须有一个具体类才能挂载到 GameObject）
    /// </summary>
    public class TestPlayer : Player
    {
        // Player 目前没有抽象方法，这里保持最小实现即可。
        // 如果后续需要更复杂的表现/逻辑，可以在这里扩展。
    }
}

