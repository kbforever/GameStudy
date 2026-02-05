using UnityEngine;
using UnityEngine.Events;
using Monopoly;

namespace Managers
{
    /// <summary>
    /// 骰子投掷结果数据结构
    /// </summary>
    [System.Serializable]
    public class DiceResult
    {
        public int dice1;
        public int dice2;
        public int total;
        public bool isDouble;
        public bool isTripleDouble; // 连续三次双骰

        public DiceResult(int d1, int d2)
        {
            dice1 = d1;
            dice2 = d2;
            total = d1 + d2;
            isDouble = d1 == d2;
            isTripleDouble = false; // 由DiceManager设置
        }
    }

    /// <summary>
    /// 骰子系统，管理双骰子投掷逻辑和移动计算
    /// </summary>
    public class DiceManager : MonoBehaviour
    {
        [Header("骰子配置")]
        [SerializeField] private int minValue = 1;
        [SerializeField] private int maxValue = 6;
        [SerializeField] private int maxDoubleRolls = 3; // 连续双骰最大次数（超过则进监狱）

        [Header("当前回合状态")]
        [SerializeField] private int consecutiveDoubles = 0; // 连续双骰次数
        [SerializeField] private bool canRollAgain = false; // 是否可以再次投掷（双骰时）

        /// <summary>
        /// 骰子投掷结果事件（dice1, dice2, total, isDouble）
        /// </summary>
        public UnityEvent<int, int, int, bool> OnDiceRolled;

        /// <summary>
        /// 连续三次双骰事件（触发进监狱）
        /// </summary>
        public UnityEvent OnTripleDouble;

        /// <summary>
        /// 当前连续双骰次数
        /// </summary>
        public int ConsecutiveDoubles => consecutiveDoubles;

        /// <summary>
        /// 是否可以再次投掷（双骰时）
        /// </summary>
        public bool CanRollAgain => canRollAgain;

        private void Awake()
        {
            if (OnDiceRolled == null)
            {
                OnDiceRolled = new UnityEvent<int, int, int, bool>();
            }

            if (OnTripleDouble == null)
            {
                OnTripleDouble = new UnityEvent();
            }
        }

        /// <summary>
        /// 投掷双骰子（完整流程）
        /// </summary>
        /// <returns>骰子结果</returns>
        public DiceResult RollDice()
        {
            // 投掷骰子
            int dice1 = Random.Range(minValue, maxValue + 1);
            int dice2 = Random.Range(minValue, maxValue + 1);
            bool isDouble = IsDouble(dice1, dice2);

            DiceResult result = new DiceResult(dice1, dice2);

            // 检查连续双骰
            if (isDouble)
            {
                consecutiveDoubles++;
                canRollAgain = true;

                // 连续三次双骰，触发进监狱
                if (consecutiveDoubles >= maxDoubleRolls)
                {
                    result.isTripleDouble = true;
                    consecutiveDoubles = 0;
                    canRollAgain = false;
                    OnTripleDouble?.Invoke();
                    Debug.LogWarning($"连续 {maxDoubleRolls} 次双骰！触发进监狱规则");
                }
                else
                {
                    Debug.Log($"双骰！({dice1}, {dice2})，连续双骰次数: {consecutiveDoubles}，可以再次投掷");
                }
            }
            else
            {
                // 不是双骰，重置连续双骰计数
                consecutiveDoubles = 0;
                canRollAgain = false;
            }

            // 触发事件
            OnDiceRolled?.Invoke(dice1, dice2, result.total, isDouble);
            Debug.Log($"投掷骰子: {dice1} + {dice2} = {result.total}，双骰: {isDouble}");

            return result;
        }

        /// <summary>
        /// 投掷双骰子（简化版本，返回总点数）
        /// </summary>
        /// <returns>总点数</returns>
        public int RollDiceSimple()
        {
            DiceResult result = RollDice();
            return result.total;
        }

        /// <summary>
        /// 投掷骰子并返回详细结果（兼容旧接口）
        /// </summary>
        /// <param name="dice1">第一个骰子点数</param>
        /// <param name="dice2">第二个骰子点数</param>
        /// <returns>总点数</returns>
        public int RollDice(out int dice1, out int dice2)
        {
            DiceResult result = RollDice();
            dice1 = result.dice1;
            dice2 = result.dice2;
            return result.total;
        }

        /// <summary>
        /// 检查是否为双骰（两个骰子点数相同）
        /// </summary>
        /// <param name="dice1">第一个骰子点数</param>
        /// <param name="dice2">第二个骰子点数</param>
        /// <returns>是否为双骰</returns>
        public bool IsDouble(int dice1, int dice2)
        {
            return dice1 == dice2;
        }

        /// <summary>
        /// 重置连续双骰计数（回合结束时调用）
        /// </summary>
        public void ResetConsecutiveDoubles()
        {
            consecutiveDoubles = 0;
            canRollAgain = false;
            Debug.Log("重置连续双骰计数");
        }

        /// <summary>
        /// 计算移动后的位置
        /// </summary>
        /// <param name="currentPosition">当前位置</param>
        /// <param name="steps">移动步数</param>
        /// <param name="boardSize">棋盘大小</param>
        /// <returns>新位置</returns>
        public int CalculateNewPosition(int currentPosition, int steps, int boardSize = 40)
        {
            if (steps < 0)
            {
                Debug.LogWarning($"移动步数不能为负数: {steps}");
                return currentPosition;
            }

            int newPosition = (currentPosition + steps) % boardSize;
            return newPosition;
        }


    }
}
