using System.Collections;
using Managers;
using UnityEngine;

namespace Monopoly
{
    /// <summary>
    /// 简单 AI 玩家：
    /// - 回合开始时自动：摇骰子 → 移动 → 若可买地且钱够则自动买地 → 结束回合
    /// - 过路费由 PropertyTile.OnLanded 自动处理
    /// </summary>
    public class AIPlayer : Player
    {
        [Header("AI 设置")]
        [SerializeField] private float thinkDelay = 0.5f; // 开始行动前的思考时间
        [SerializeField] private float afterMoveDelay = 0.3f; // 移动结束后再买地的等待时间

        private bool isTakingTurn = false;

        /// <summary>
        /// 由 GameManager 在轮到该 AI 时调用，开始 AI 的一整回合流程
        /// </summary>
        public void StartAITurn()
        {
            if (isTakingTurn)
            {
                Debug.LogWarning($"{PlayerName} AI 正在执行回合，忽略重复调用。");
                return;
            }

            StartCoroutine(AITurnCoroutine());
        }

        private IEnumerator AITurnCoroutine()
        {
            isTakingTurn = true;

            // 确保有 GameManager
            GameCoreManager gm = GameCoreManager.Instance;
            if (gm == null)
            {
                Debug.LogError("AIPlayer: 未找到 GameManager.Instance，无法执行 AI 回合。");
                isTakingTurn = false;
                yield break;
            }

            // 简单等待，模拟“思考”
            if (thinkDelay > 0f)
            {
                yield return new WaitForSeconds(thinkDelay);
            }

            // 摇骰子并移动（使用 GameManager 的完整流程，含进监狱/过路费触发）
            DiceResult result = gm.RollDiceAndMove();
            if (result == null)
            {
                Debug.LogWarning($"AIPlayer: {PlayerName} 摇骰子失败，结束回合。");
                isTakingTurn = false;
                gm.EndPlayerTurn();
                yield break;
            }

            // 等待移动动画结束
            while (IsMoving)
            {
                yield return null;
            }

            if (afterMoveDelay > 0f)
            {
                yield return new WaitForSeconds(afterMoveDelay);
            }

            // 自动买地逻辑：如果当前位置是未被购买的地产，并且钱够，就买
            PropertyTile purchasable = gm.GetPurchasableProperty(this);
            if (purchasable != null && HasEnoughMoney(purchasable.PropertyPrice))
            {
                bool buyOk = gm.BuyProperty(this);
                Debug.Log($"[AI] {PlayerName} 自动购买 {purchasable.TileName}: {(buyOk ? "成功" : "失败")}");
            }
            else if (purchasable != null)
            {
                Debug.Log($"[AI] {PlayerName} 想买 {purchasable.TileName}，但资金不足。");
            }

            // 结束回合（不考虑双骰可以多次行动的复杂规则，保持简单）
            gm.EndPlayerTurn();

            isTakingTurn = false;
        }
    }
}

