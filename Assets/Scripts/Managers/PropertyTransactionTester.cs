using UnityEngine;
using Monopoly;
using Managers;

namespace Managers
{
    /// <summary>
    /// 地产交易系统测试脚本，用于测试买地、卖地、过路费功能
    /// </summary>
    public class PropertyTransactionTester : MonoBehaviour
    {
        [Header("引用")]
        [SerializeField] private GameCoreManager gameManager;
        [SerializeField] private BoardManager boardManager;

        [Header("测试参数")]
        [SerializeField] private int testTileIndex = 0; // 测试用的格子索引
        [SerializeField] private int customSellPrice = -1; // 自定义出售价格（-1表示使用默认）

        [Header("GUI")]
        [SerializeField] private bool showOnGUI = true;

        private void Start()
        {
            if (gameManager == null)
            {
                gameManager = FindObjectOfType<GameCoreManager>();

                // 如果场景中没有 GameManager，则创建一个
                if (gameManager == null)
                {
                    GameObject gmObject = new GameObject("GameManager");
                    gameManager = gmObject.AddComponent<GameCoreManager>();
                    Debug.Log("PropertyTransactionTester: 场景中未找到 GameManager，已自动创建。");
                }
            }

            if (boardManager == null)
            {
                boardManager = FindObjectOfType<BoardManager>();
            }

            // 订阅事件
            if (gameManager != null)
            {
                gameManager.OnPropertyPurchased.AddListener(OnPropertyPurchased);
                gameManager.OnPropertySold.AddListener(OnPropertySold);
                gameManager.OnRentPaid.AddListener(OnRentPaid);
            }
        }

        private void OnDestroy()
        {
            if (gameManager != null)
            {
                gameManager.OnPropertyPurchased.RemoveListener(OnPropertyPurchased);
                gameManager.OnPropertySold.RemoveListener(OnPropertySold);
                gameManager.OnRentPaid.RemoveListener(OnRentPaid);
            }
        }

        #region 事件回调

        private void OnPropertyPurchased(Player player, PropertyTile property)
        {
            Debug.Log($"[事件] {player.PlayerName} 购买了 {property.TileName}（价格: {property.PropertyPrice}）");
        }

        private void OnPropertySold(Player player, PropertyTile property, int sellPrice)
        {
            Debug.Log($"[事件] {player.PlayerName} 出售了 {property.TileName}（获得: {sellPrice}）");
        }

        private void OnRentPaid(Player payer, Player receiver, PropertyTile property, int rent)
        {
            Debug.Log($"[事件] {payer.PlayerName} 向 {receiver.PlayerName} 支付了过路费 {rent}（{property.TileName}）");
        }

        #endregion

        #region 测试方法（ContextMenu）

        [ContextMenu("测试：购买当前位置的地产")]
        public void TestBuyCurrentProperty()
        {
            if (gameManager == null)
            {
                Debug.LogError("PropertyTransactionTester: GameManager 为空");
                return;
            }

            bool success = gameManager.BuyProperty();
            Debug.Log($"[测试] 购买当前位置地产: {(success ? "成功" : "失败")}");
        }

        [ContextMenu("测试：购买指定位置的地产")]
        public void TestBuyPropertyAtTile()
        {
            if (gameManager == null || boardManager == null)
            {
                Debug.LogError("PropertyTransactionTester: GameManager 或 BoardManager 为空");
                return;
            }

            Player currentPlayer = gameManager.CurrentPlayer;
            if (currentPlayer == null)
            {
                Debug.LogError("PropertyTransactionTester: 当前玩家为空");
                return;
            }

            // 先移动到指定位置
            currentPlayer.MoveTo(testTileIndex);
            currentPlayer.UpdateVisualPosition(boardManager);

            bool success = gameManager.BuyProperty(currentPlayer, testTileIndex);
            Debug.Log($"[测试] 购买位置 {testTileIndex} 的地产: {(success ? "成功" : "失败")}");
        }

        [ContextMenu("测试：出售第一个拥有的地产")]
        public void TestSellFirstProperty()
        {
            if (gameManager == null)
            {
                Debug.LogError("PropertyTransactionTester: GameManager 为空");
                return;
            }

            Player currentPlayer = gameManager.CurrentPlayer;
            if (currentPlayer == null)
            {
                Debug.LogError("PropertyTransactionTester: 当前玩家为空");
                return;
            }

            var properties = gameManager.GetPlayerProperties(currentPlayer);
            if (properties.Count == 0)
            {
                Debug.LogWarning("PropertyTransactionTester: 当前玩家没有地产可出售");
                return;
            }

            PropertyTile firstProperty = properties[0];
            int? sellPrice = customSellPrice >= 0 ? (int?)customSellPrice : null;
            bool success = gameManager.SellProperty(currentPlayer, firstProperty, sellPrice);
            Debug.Log($"[测试] 出售地产 {firstProperty.TileName}: {(success ? "成功" : "失败")}");
        }

        [ContextMenu("测试：支付当前位置的过路费")]
        public void TestPayRent()
        {
            if (gameManager == null)
            {
                Debug.LogError("PropertyTransactionTester: GameManager 为空");
                return;
            }

            bool success = gameManager.PayRent();
            Debug.Log($"[测试] 支付当前位置过路费: {(success ? "成功" : "失败")}");
        }

        /// <summary>
        /// 测试：摇骰子并移动，然后可以进行买卖测试
        /// </summary>
        [ContextMenu("测试：摇骰子并移动")]
        public void TestRollDiceAndMove()
        {
            StartCoroutine(RunSingleTurnForCurrentPlayer(autoTrade: false));
        }

        /// <summary>
        /// 测试：当前玩家完成一整回合（摇骰子→移动→自动交易→结束回合）
        /// </summary>
        [ContextMenu("测试：当前玩家完整回合")]
        public void TestSingleTurnWithTrade()
        {
            StartCoroutine(RunSingleTurnForCurrentPlayer(autoTrade: true, endTurnAfter: true));
        }

        /// <summary>
        /// 测试：两名玩家各完成一回合（玩家1→玩家2），中途会自动支付过路费
        /// </summary>
        [ContextMenu("测试：两名玩家各完成一回合")]
        public void TestTwoPlayersOneRound()
        {
            StartCoroutine(RunRoundForTwoPlayers());
        }

        /// <summary>
        /// 为当前玩家执行一次：摇骰子→移动→（可选）自动买地→（可选）结束回合
        /// </summary>
        private System.Collections.IEnumerator RunSingleTurnForCurrentPlayer(bool autoTrade, bool endTurnAfter = false)
        {
            if (gameManager == null)
            {
                Debug.LogError("PropertyTransactionTester: GameManager 为空");
                yield break;
            }

            Player player = gameManager.CurrentPlayer;
            if (player == null)
            {
                Debug.LogError("PropertyTransactionTester: 当前玩家为空");
                yield break;
            }

            DiceResult result = gameManager.RollDiceAndMove();
            if (result == null)
            {
                Debug.LogWarning("PropertyTransactionTester: 摇骰子失败");
                yield break;
            }

            Debug.Log($"[测试] 摇骰子结果: {result.dice1} + {result.dice2} = {result.total}（双骰: {result.isDouble}，三连双骰: {result.isTripleDouble}）");

            // 等待移动动画完成（GameManager 内部使用 Player.IsMoving）
            while (player.IsMoving)
            {
                yield return null;
            }

            // 到达目标格子时，BoardManager / Tile 系统已经自动触发过路费逻辑（PropertyTile.OnLanded+PayRent）

            // 自动交易逻辑：如果当前格子是未被购买的地产，并且玩家有钱，就自动购买
            if (autoTrade)
            {
                PropertyTile purchasable = gameManager.GetPurchasableProperty(player);
                if (purchasable != null && player.HasEnoughMoney(purchasable.PropertyPrice))
                {
                    bool buyOk = gameManager.BuyProperty(player);
                    Debug.Log($"[自动交易] {player.PlayerName} 尝试购买 {purchasable.TileName}: {(buyOk ? "成功" : "失败")}");
                }
                else if (purchasable != null)
                {
                    Debug.Log($"[自动交易] {player.PlayerName} 想买 {purchasable.TileName}，但资金不足。");
                }
            }

            // 结束回合：切换到下一个玩家（由 GameManager 负责）
            if (endTurnAfter)
            {
                gameManager.EndPlayerTurn();
            }
        }

        /// <summary>
        /// 顺序执行：玩家1→玩家2 各完成一回合
        /// </summary>
        private System.Collections.IEnumerator RunRoundForTwoPlayers()
        {
            if (gameManager == null)
            {
                Debug.LogError("PropertyTransactionTester: GameManager 为空");
                yield break;
            }

            var players = gameManager.Players;
            if (players == null || players.Count < 2)
            {
                Debug.LogWarning("PropertyTransactionTester: 玩家数量小于 2，无法执行“两名玩家各完成一回合”的测试。");
                yield break;
            }

            int initialIndex = gameManager.CurrentPlayerIndex;

            // 玩家1
            Debug.Log("===== 玩家1 回合开始 =====");
            yield return RunSingleTurnForCurrentPlayer(autoTrade: true, endTurnAfter: true);

            // 玩家2（GameManager.EndPlayerTurn 已经切换到下一个玩家）
            Debug.Log("===== 玩家2 回合开始 =====");
            yield return RunSingleTurnForCurrentPlayer(autoTrade: true, endTurnAfter: true);

            Debug.Log("===== 两名玩家各完成一回合 =====");
        }

        [ContextMenu("测试：显示当前玩家信息")]
        public void TestShowPlayerInfo()
        {
            if (gameManager == null)
            {
                Debug.LogError("PropertyTransactionTester: GameManager 为空");
                return;
            }

            Player currentPlayer = gameManager.CurrentPlayer;
            if (currentPlayer == null)
            {
                Debug.LogError("PropertyTransactionTester: 当前玩家为空");
                return;
            }

            Debug.Log($"=== 玩家信息 ===");
            Debug.Log($"玩家名称: {currentPlayer.PlayerName}");
            Debug.Log($"当前金钱: {currentPlayer.Money}");
            Debug.Log($"当前位置: {currentPlayer.CurrentPosition}");
            Debug.Log($"总资产: {currentPlayer.GetTotalAssets()}");
            Debug.Log($"拥有地产数: {currentPlayer.Properties.Count}");

            var properties = gameManager.GetPlayerProperties(currentPlayer);
            foreach (var prop in properties)
            {
                Debug.Log($"  - {prop.TileName}（价格: {prop.PropertyPrice}，租金: {prop.BaseRent}）");
            }
        }

        #endregion

        private void OnGUI()
        {
            if (!showOnGUI) return;
            if (gameManager == null) return;

            Player currentPlayer = gameManager.CurrentPlayer;
            if (currentPlayer == null) return;

            GUILayout.BeginArea(new Rect(10, 280, 360, 300));
            GUILayout.Box("地产交易测试工具");

            GUILayout.Label($"当前玩家: {currentPlayer.PlayerName}");
            GUILayout.Label($"金钱: {currentPlayer.Money}");
            GUILayout.Label($"位置: {currentPlayer.CurrentPosition}");
            GUILayout.Label($"拥有地产: {currentPlayer.Properties.Count}");

            GUILayout.Space(8);
            GUILayout.Label($"测试格子索引: {testTileIndex}");

            GUILayout.Space(8);
            if (GUILayout.Button("摇骰子并移动"))
            {
                TestRollDiceAndMove();
            }

            GUILayout.Space(4);
            if (GUILayout.Button("当前玩家完整回合（摇骰子+交易+结束回合）"))
            {
                TestSingleTurnWithTrade();
            }

            if (GUILayout.Button("两名玩家各完成一回合"))
            {
                TestTwoPlayersOneRound();
            }

            GUILayout.Space(4);
            if (GUILayout.Button("购买当前位置地产"))
            {
                TestBuyCurrentProperty();
            }

            if (GUILayout.Button($"购买位置 {testTileIndex} 的地产"))
            {
                TestBuyPropertyAtTile();
            }

            if (GUILayout.Button("出售第一个拥有的地产"))
            {
                TestSellFirstProperty();
            }

            if (GUILayout.Button("支付当前位置过路费"))
            {
                TestPayRent();
            }

            if (GUILayout.Button("显示玩家信息"))
            {
                TestShowPlayerInfo();
            }

            GUILayout.EndArea();
        }
    }
}
