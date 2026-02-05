using System.Collections;
using Monopoly;
using UnityEngine;
using UnityEngine.Events;

namespace Managers
{
    /// <summary>
    /// Player 移动测试脚本：
    /// - 支持立即移动 Move
    /// - 支持逐格动画移动 MoveAnimated
    /// - 支持移动到指定格 MoveTo + 刷新视觉位置
    /// </summary>
    public class PlayerMovementTester : MonoBehaviour
    {
        [Header("引用（可留空自动查找/创建）")]
        [SerializeField] private BoardManager boardManager;
        [SerializeField] private TestPlayer testPlayer;
        [SerializeField] private DiceManager diceManager;

        [Header("Prefab（可选）")]
#pragma warning disable 0649 // Unity 会通过 Inspector 赋值
        [SerializeField] private GameObject testPlayerPrefab;
#pragma warning restore 0649

        [Header("测试参数")]
        [SerializeField] private string testPlayerName = "测试玩家";
        [SerializeField] private int initialMoney = 1500;
        [SerializeField] private int testSteps = 6;
        [SerializeField] private int testTargetPosition = 10;
        [SerializeField] private bool autoCreateIfMissing = true;
        [SerializeField] private bool moveWithAnimationOnDice = true;

        [Header("GUI")]
        [SerializeField] private bool showOnGUI = true;

        [Header("骰子结果（只读）")]
        [SerializeField] private int lastDice1;
        [SerializeField] private int lastDice2;
        [SerializeField] private int lastTotal;
        [SerializeField] private bool lastIsDouble;
        [SerializeField] private bool lastIsTripleDouble;

        private void Start()
        {
            EnsureBoardManager();
            EnsureTestPlayer();
            EnsureDiceManager();
            HookPlayerEvents();

            // 初始化并放到起点
            testPlayer.Initialize(testPlayerName, initialMoney);
            testPlayer.MoveTo(0);
            testPlayer.UpdateVisualPosition(boardManager);

            Debug.Log($"PlayerMovementTester: 初始化完成，玩家位置={testPlayer.CurrentPosition}，棋盘大小={boardManager.BoardSize}");
        }

        private void OnDestroy()
        {
            UnhookPlayerEvents();
        }

        private void EnsureBoardManager()
        {
            if (boardManager != null) return;

            boardManager = FindObjectOfType<BoardManager>();
            if (boardManager != null) return;

            if (!autoCreateIfMissing)
            {
                Debug.LogError("PlayerMovementTester: 未找到 BoardManager，且 autoCreateIfMissing=false");
                return;
            }

            GameObject boardObject = new GameObject("BoardManager");
            boardManager = boardObject.AddComponent<BoardManager>();
            Debug.Log("PlayerMovementTester: 创建了 BoardManager 实例");
        }

        private void EnsureTestPlayer()
        {
            if (testPlayer != null) return;

            testPlayer = FindObjectOfType<TestPlayer>();
            if (testPlayer != null) return;

            if (!autoCreateIfMissing)
            {
                Debug.LogError("PlayerMovementTester: 未找到 TestPlayer，且 autoCreateIfMissing=false");
                return;
            }

            // 优先使用 Prefab 实例化（如果提供）
            if (testPlayerPrefab != null)
            {
                GameObject playerObject = Instantiate(testPlayerPrefab);
                playerObject.name = testPlayerPrefab.name;

                testPlayer = playerObject.GetComponent<TestPlayer>();
                if (testPlayer == null)
                {
                    testPlayer = playerObject.AddComponent<TestPlayer>();
                    Debug.LogWarning("PlayerMovementTester: Prefab 未包含 TestPlayer 组件，已自动添加");
                }

                Debug.Log($"PlayerMovementTester: 从 Prefab 实例化了 TestPlayer（{playerObject.name}）");
                return;
            }

            // 未提供 Prefab，则创建空物体
            GameObject fallbackObject = new GameObject("TestPlayer");
            testPlayer = fallbackObject.AddComponent<TestPlayer>();
            Debug.Log("PlayerMovementTester: 创建了 TestPlayer 实例（非Prefab）");
        }

        private void EnsureDiceManager()
        {
            if (diceManager != null) return;

            diceManager = FindObjectOfType<DiceManager>();
            if (diceManager != null) return;

            if (!autoCreateIfMissing)
            {
                Debug.LogError("PlayerMovementTester: 未找到 DiceManager，且 autoCreateIfMissing=false");
                return;
            }

            GameObject diceObject = new GameObject("DiceManager");
            diceManager = diceObject.AddComponent<DiceManager>();
            Debug.Log("PlayerMovementTester: 创建了 DiceManager 实例");
        }

        private void HookPlayerEvents()
        {
            if (testPlayer == null) return;

            // Player 里的 UnityEvent 字段可能未在 Inspector 初始化，这里兜底创建
            if (testPlayer.OnPlayerMoved == null) testPlayer.OnPlayerMoved = new UnityEvent<int, int>();
            if (testPlayer.OnPlayerMoveCompleted == null) testPlayer.OnPlayerMoveCompleted = new UnityEvent();

            testPlayer.OnPlayerMoved.AddListener(OnPlayerMoved);
            testPlayer.OnPlayerMoveCompleted.AddListener(OnPlayerMoveCompleted);
        }

        private void UnhookPlayerEvents()
        {
            if (testPlayer == null) return;

            if (testPlayer.OnPlayerMoved != null) testPlayer.OnPlayerMoved.RemoveListener(OnPlayerMoved);
            if (testPlayer.OnPlayerMoveCompleted != null) testPlayer.OnPlayerMoveCompleted.RemoveListener(OnPlayerMoveCompleted);
        }

        private void OnPlayerMoved(int from, int to)
        {
            Debug.Log($"[移动中] {testPlayer.PlayerName}: {from} -> {to}");
        }

        private void OnPlayerMoveCompleted()
        {
            Debug.Log($"[移动完成] {testPlayer.PlayerName}: 当前位置={testPlayer.CurrentPosition}，世界坐标={testPlayer.transform.position}");
        }

        #region 测试方法（ContextMenu）

        [ContextMenu("测试：立即移动（testSteps）")]
        public void TestMoveImmediate()
        {
            if (testPlayer == null || boardManager == null)
            {
                Debug.LogError("PlayerMovementTester: testPlayer 或 boardManager 为空");
                return;
            }

            int before = testPlayer.CurrentPosition;
            bool ok = testPlayer.Move(testSteps, boardManager.BoardSize);
            testPlayer.UpdateVisualPosition(boardManager);

            Debug.Log($"[立即移动] steps={testSteps}, ok={ok}, {before} -> {testPlayer.CurrentPosition}");
        }

        [ContextMenu("测试：动画移动（testSteps）")]
        public void TestMoveAnimated()
        {
            if (testPlayer == null || boardManager == null)
            {
                Debug.LogError("PlayerMovementTester: testPlayer 或 boardManager 为空");
                return;
            }

            if (testPlayer.IsMoving)
            {
                Debug.LogWarning("PlayerMovementTester: 玩家正在移动中，忽略本次动画移动请求");
                return;
            }

            int before = testPlayer.CurrentPosition;
            Coroutine c = testPlayer.MoveAnimated(testSteps, boardManager);
            Debug.Log($"[动画移动] steps={testSteps}, coroutine={(c != null ? "OK" : "null")}, {before} -> 目标={(before + testSteps) % boardManager.BoardSize}");
        }

        [ContextMenu("测试：移动到指定格（testTargetPosition）")]
        public void TestMoveToPosition()
        {
            if (testPlayer == null || boardManager == null)
            {
                Debug.LogError("PlayerMovementTester: testPlayer 或 boardManager 为空");
                return;
            }

            int before = testPlayer.CurrentPosition;
            int target = Mathf.Clamp(testTargetPosition, 0, boardManager.BoardSize - 1);
            testPlayer.MoveTo(target);
            testPlayer.UpdateVisualPosition(boardManager);

            Debug.Log($"[移动到指定格] {before} -> {testPlayer.CurrentPosition} (target={target})");
        }

        [ContextMenu("测试：连续动画移动三次")]
        public void TestMoveAnimatedThreeTimes()
        {
            if (testPlayer == null || boardManager == null)
            {
                Debug.LogError("PlayerMovementTester: testPlayer 或 boardManager 为空");
                return;
            }

            StartCoroutine(MoveAnimatedSequence(3, testSteps));
        }

        [ContextMenu("测试：摇骰子并移动")]
        public void TestRollDiceAndMove()
        {
            if (diceManager == null || testPlayer == null || boardManager == null)
            {
                Debug.LogError("PlayerMovementTester: diceManager / testPlayer / boardManager 为空");
                return;
            }

            if (testPlayer.IsMoving)
            {
                Debug.LogWarning("PlayerMovementTester: 玩家正在移动中，无法摇骰子并移动");
                return;
            }

            DiceResult result = diceManager.RollDice();
            CacheLastDiceResult(result);

            Debug.Log($"[摇骰子] {lastDice1}+{lastDice2}={lastTotal} double={lastIsDouble} tripleDouble={lastIsTripleDouble}");

            if (moveWithAnimationOnDice)
            {
                testPlayer.MoveAnimated(lastTotal, boardManager);
            }
            else
            {
                testPlayer.Move(lastTotal, boardManager.BoardSize);
                testPlayer.UpdateVisualPosition(boardManager);
            }
        }

        private void CacheLastDiceResult(DiceResult result)
        {
            if (result == null)
            {
                lastDice1 = 0;
                lastDice2 = 0;
                lastTotal = 0;
                lastIsDouble = false;
                lastIsTripleDouble = false;
                return;
            }

            lastDice1 = result.dice1;
            lastDice2 = result.dice2;
            lastTotal = result.total;
            lastIsDouble = result.isDouble;
            lastIsTripleDouble = result.isTripleDouble;
        }

        private IEnumerator MoveAnimatedSequence(int times, int stepsPerMove)
        {
            for (int i = 0; i < times; i++)
            {
                if (testPlayer.IsMoving)
                {
                    // 等待上一段结束
                    while (testPlayer.IsMoving) yield return null;
                }

                testPlayer.MoveAnimated(stepsPerMove, boardManager);
                while (testPlayer.IsMoving) yield return null;

                yield return new WaitForSeconds(0.1f);
            }

            Debug.Log($"[连续动画移动] 完成 times={times}, stepsPerMove={stepsPerMove}");
        }

        #endregion

        private void OnGUI()
        {
            if (!showOnGUI) return;
            if (testPlayer == null || boardManager == null) return;

            GUILayout.BeginArea(new Rect(10, 10, 360, 260));
            GUILayout.Box("Player 移动测试工具");

            GUILayout.Label($"玩家: {testPlayer.PlayerName}");
            GUILayout.Label($"位置: {testPlayer.CurrentPosition} / {boardManager.BoardSize - 1}");
            GUILayout.Label($"IsMoving: {testPlayer.IsMoving}");

            GUILayout.Space(8);
            GUILayout.Label($"testSteps: {testSteps}");
            GUILayout.Label($"testTargetPosition: {testTargetPosition}");
            GUILayout.Label($"摇骰子用动画移动: {moveWithAnimationOnDice}");

            if (diceManager != null)
            {
                GUILayout.Label($"上次骰子: {lastDice1}+{lastDice2}={lastTotal} double={lastIsDouble}");
            }

            GUILayout.Space(8);
            if (GUILayout.Button("立即移动（testSteps）")) TestMoveImmediate();
            if (GUILayout.Button("动画移动（testSteps）")) TestMoveAnimated();
            if (GUILayout.Button("移动到指定格（testTargetPosition）")) TestMoveToPosition();
            if (GUILayout.Button("连续动画移动三次")) TestMoveAnimatedThreeTimes();
            if (GUILayout.Button("摇骰子并移动")) TestRollDiceAndMove();

            GUILayout.EndArea();
        }
    }
}

