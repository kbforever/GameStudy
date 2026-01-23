using UnityEngine;
using Managers;

namespace Managers
{
    /// <summary>
    /// DiceManager测试脚本，用于验证骰子系统的功能
    /// </summary>
    public class DiceManagerTester : MonoBehaviour
    {
        [Header("测试配置")]
        [SerializeField] private DiceManager diceManager;
        [SerializeField] private int testRollCount = 100; // 测试投掷次数
        [SerializeField] private bool autoTestOnStart = false; // 启动时自动测试

        [Header("测试结果")]
        [SerializeField] private int totalRolls = 0;
        [SerializeField] private int doubleRolls = 0;
        [SerializeField] private int tripleDoubleCount = 0;
        [SerializeField] private int minRoll = 2;
        [SerializeField] private int maxRoll = 12;
        [SerializeField] private int[] rollDistribution = new int[13]; // 索引0-12，对应点数2-12

        private void Start()
        {
            // 自动查找DiceManager
            if (diceManager == null)
            {
                diceManager = FindObjectOfType<DiceManager>();
            }

            // 如果没有找到，创建一个
            if (diceManager == null)
            {
                GameObject diceManagerObject = new GameObject("DiceManager");
                diceManager = diceManagerObject.AddComponent<DiceManager>();
                Debug.Log("DiceManagerTester: 创建了DiceManager实例");
            }

            // 订阅事件
            if (diceManager != null)
            {
                diceManager.OnDiceRolled.AddListener(OnDiceRolled);
                diceManager.OnTripleDouble.AddListener(OnTripleDouble);
            }

            if (autoTestOnStart)
            {
                RunBasicTest();
            }
        }

        private void OnDestroy()
        {
            // 取消订阅事件
            if (diceManager != null)
            {
                diceManager.OnDiceRolled.RemoveListener(OnDiceRolled);
                diceManager.OnTripleDouble.RemoveListener(OnTripleDouble);
            }
        }

        /// <summary>
        /// 骰子投掷事件回调
        /// </summary>
        private void OnDiceRolled(int dice1, int dice2, int total, bool isDouble)
        {
            totalRolls++;
            
            if (isDouble)
            {
                doubleRolls++;
            }

            // 更新统计
            if (total >= 2 && total <= 12)
            {
                rollDistribution[total]++;
            }

            // 更新最小最大值
            if (total < minRoll) minRoll = total;
            if (total > maxRoll) maxRoll = total;
        }

        /// <summary>
        /// 连续三次双骰事件回调
        /// </summary>
        private void OnTripleDouble()
        {
            tripleDoubleCount++;
            Debug.LogWarning($"[测试] 检测到连续三次双骰！总次数: {tripleDoubleCount}");
        }

        #region 测试方法

        /// <summary>
        /// 基础测试：单次投掷
        /// </summary>
        [ContextMenu("基础测试：单次投掷")]
        public void RunBasicTest()
        {
            if (diceManager == null)
            {
                Debug.LogError("DiceManager未找到！");
                return;
            }

            Debug.Log("=== 基础测试：单次投掷 ===");
            DiceResult result = diceManager.RollDice();
            LogDiceResult(result);
        }

        /// <summary>
        /// 统计测试：多次投掷并统计结果
        /// </summary>
        [ContextMenu("统计测试：多次投掷")]
        public void RunStatisticsTest()
        {
            if (diceManager == null)
            {
                Debug.LogError("DiceManager未找到！");
                return;
            }

            Debug.Log($"=== 统计测试：投掷 {testRollCount} 次 ===");
            
            // 重置统计
            ResetStatistics();

            // 执行多次投掷
            for (int i = 0; i < testRollCount; i++)
            {
                diceManager.RollDice();
            }

            // 输出统计结果
            PrintStatistics();
        }

        /// <summary>
        /// 双骰测试：测试连续双骰功能
        /// </summary>
        [ContextMenu("双骰测试：连续双骰")]
        public void RunDoubleTest()
        {
            if (diceManager == null)
            {
                Debug.LogError("DiceManager未找到！");
                return;
            }

            Debug.Log("=== 双骰测试：测试连续双骰功能 ===");
            
            int consecutiveDoubles = 0;
            int maxAttempts = 1000; // 最大尝试次数，防止无限循环
            int attempts = 0;

            while (consecutiveDoubles < 3 && attempts < maxAttempts)
            {
                attempts++;
                DiceResult result = diceManager.RollDice();
                
                if (result.isDouble)
                {
                    consecutiveDoubles++;
                    Debug.Log($"第 {attempts} 次投掷：双骰 ({result.dice1}, {result.dice2})，连续双骰次数: {consecutiveDoubles}");
                }
                else
                {
                    if (consecutiveDoubles > 0)
                    {
                        Debug.Log($"第 {attempts} 次投掷：非双骰 ({result.dice1}, {result.dice2})，重置连续双骰计数");
                    }
                    consecutiveDoubles = 0;
                }

                // 检查是否触发连续三次双骰
                if (result.isTripleDouble)
                {
                    Debug.LogWarning($"成功触发连续三次双骰！在第 {attempts} 次投掷时触发");
                    break;
                }
            }

            if (attempts >= maxAttempts)
            {
                Debug.LogWarning($"达到最大尝试次数 {maxAttempts}，未触发连续三次双骰");
            }
        }

        /// <summary>
        /// 移动计算测试：测试位置计算功能
        /// </summary>
        [ContextMenu("移动计算测试")]
        public void RunMovementTest()
        {
            if (diceManager == null)
            {
                Debug.LogError("DiceManager未找到！");
                return;
            }

            Debug.Log("=== 移动计算测试 ===");

            int boardSize = 40;
            int[] testPositions = { 0, 10, 20, 30, 39 };
            int[] testSteps = { 5, 10, 15, 20, 35, 40, 50 };

            foreach (int startPos in testPositions)
            {
                foreach (int steps in testSteps)
                {
                    int newPos = diceManager.CalculateNewPosition(startPos, steps, boardSize);
                    bool passedStart = newPos < startPos;
                    
                    Debug.Log($"位置 {startPos} + {steps} 步 = 位置 {newPos} {(passedStart ? "[经过起点]" : "")}");
                }
            }
        }

        /// <summary>
        /// 完整测试：运行所有测试
        /// </summary>
        [ContextMenu("完整测试：运行所有测试")]
        public void RunFullTest()
        {
            Debug.Log("========================================");
            Debug.Log("开始运行完整测试套件");
            Debug.Log("========================================");

            RunBasicTest();
            Debug.Log("");
            
            RunStatisticsTest();
            Debug.Log("");
            
            RunDoubleTest();
            Debug.Log("");
            
            RunMovementTest();
            Debug.Log("");

            Debug.Log("========================================");
            Debug.Log("完整测试套件执行完毕");
            Debug.Log("========================================");
        }

        /// <summary>
        /// 重置连续双骰计数测试
        /// </summary>
        [ContextMenu("重置连续双骰计数")]
        public void TestResetConsecutiveDoubles()
        {
            if (diceManager == null)
            {
                Debug.LogError("DiceManager未找到！");
                return;
            }

            Debug.Log("=== 重置连续双骰计数测试 ===");
            Debug.Log($"重置前连续双骰次数: {diceManager.ConsecutiveDoubles}");
            Debug.Log($"重置前是否可以再次投掷: {diceManager.CanRollAgain}");
            
            diceManager.ResetConsecutiveDoubles();
            
            Debug.Log($"重置后连续双骰次数: {diceManager.ConsecutiveDoubles}");
            Debug.Log($"重置后是否可以再次投掷: {diceManager.CanRollAgain}");
        }

        #endregion

        #region 辅助方法

        /// <summary>
        /// 记录骰子结果
        /// </summary>
        private void LogDiceResult(DiceResult result)
        {
            Debug.Log($"骰子1: {result.dice1}, 骰子2: {result.dice2}, 总数: {result.total}");
            Debug.Log($"是否为双骰: {result.isDouble}");
            Debug.Log($"是否连续三次双骰: {result.isTripleDouble}");
            Debug.Log($"当前连续双骰次数: {diceManager.ConsecutiveDoubles}");
            Debug.Log($"是否可以再次投掷: {diceManager.CanRollAgain}");
        }

        /// <summary>
        /// 重置统计信息
        /// </summary>
        private void ResetStatistics()
        {
            totalRolls = 0;
            doubleRolls = 0;
            tripleDoubleCount = 0;
            minRoll = 12;
            maxRoll = 2;
            rollDistribution = new int[13];
        }

        /// <summary>
        /// 打印统计信息
        /// </summary>
        private void PrintStatistics()
        {
            Debug.Log($"总投掷次数: {totalRolls}");
            Debug.Log($"双骰次数: {doubleRolls} ({doubleRolls * 100f / totalRolls:F2}%)");
            Debug.Log($"连续三次双骰次数: {tripleDoubleCount}");
            Debug.Log($"最小点数: {minRoll}, 最大点数: {maxRoll}");

            Debug.Log("点数分布:");
            for (int i = 2; i <= 12; i++)
            {
                int count = rollDistribution[i];
                float percentage = count * 100f / totalRolls;
                string bar = new string('█', Mathf.RoundToInt(percentage / 2));
                Debug.Log($"  {i:2}: {count:4} ({percentage:5.2f}%) {bar}");
            }

            // 理论概率对比（双骰子）
            Debug.Log("\n理论概率（双骰子）:");
            int[] theoretical = { 0, 0, 1, 2, 3, 4, 5, 6, 5, 4, 3, 2, 1 }; // 2-12点的组合数
            for (int i = 2; i <= 12; i++)
            {
                float theoreticalPercent = theoretical[i] * 100f / 36f;
                Debug.Log($"  {i:2}: {theoreticalPercent:5.2f}%");
            }
        }

        #endregion

        #region Unity Editor GUI

        private void OnGUI()
        {
            if (diceManager == null) return;

            GUILayout.BeginArea(new Rect(10, 10, 300, 400));
            GUILayout.Box("DiceManager 测试工具");

            GUILayout.Label($"当前连续双骰次数: {diceManager.ConsecutiveDoubles}");
            GUILayout.Label($"可以再次投掷: {diceManager.CanRollAgain}");

            GUILayout.Space(10);

            if (GUILayout.Button("单次投掷"))
            {
                RunBasicTest();
            }

            if (GUILayout.Button($"统计测试 ({testRollCount}次)"))
            {
                RunStatisticsTest();
            }

            if (GUILayout.Button("双骰测试"))
            {
                RunDoubleTest();
            }

            if (GUILayout.Button("移动计算测试"))
            {
                RunMovementTest();
            }

            if (GUILayout.Button("重置连续双骰计数"))
            {
                TestResetConsecutiveDoubles();
            }

            GUILayout.Space(10);

            if (GUILayout.Button("运行完整测试"))
            {
                RunFullTest();
            }


            GUILayout.EndArea();
        }

        #endregion
    }
}
