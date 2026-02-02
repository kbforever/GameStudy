using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Monopoly;
using Singleton;

namespace Managers
{
    /// <summary>
    /// 游戏管理器，单例模式，控制游戏核心流程
    /// </summary>
    public class GameCoreManager : Singleton<GameCoreManager>
    {

        [Header("游戏配置")]
        [SerializeField] private int initialMoney = 1500;
        [SerializeField] private int playerCount = 2;
        [SerializeField] private List<Player> players = new List<Player>();

        [Header("玩家配置")]
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private Color[] playerColors = new Color[]
        {
            Color.red,
            Color.blue,
            Color.green,
            Color.yellow
        };

        [Header("游戏组件引用")]
        [SerializeField] private BoardManager boardManager;
        [SerializeField] private DiceManager diceManager;

        [Header("游戏状态")]
        [SerializeField] private GameState currentState = GameState.Initializing;
        [SerializeField] private int currentPlayerIndex = 0;

        /// <summary>
        /// 当前游戏状态
        /// </summary>
        public GameState CurrentState => currentState;

        /// <summary>
        /// 当前玩家索引
        /// </summary>
        public int CurrentPlayerIndex => currentPlayerIndex;

        /// <summary>
        /// 当前玩家
        /// </summary>
        public Player CurrentPlayer
        {
            get
            {
                if (currentPlayerIndex >= 0 && currentPlayerIndex < players.Count)
                {
                    return players[currentPlayerIndex];
                }
                return null;
            }
        }

        /// <summary>
        /// 所有玩家列表
        /// </summary>
        public IReadOnlyList<Player> Players => players;

        /// <summary>
        /// 棋盘管理器
        /// </summary>
        public BoardManager BoardManager => boardManager;

        /// <summary>
        /// 骰子管理器
        /// </summary>
        public DiceManager DiceManager => diceManager;

        // 游戏事件
        public UnityEvent<Player> OnPlayerTurnStart;
        public UnityEvent<Player> OnPlayerTurnEnd;
        public UnityEvent<Player> OnPlayerMove;
        public UnityEvent<Player> OnPlayerBankrupt;
        public UnityEvent<Player> OnGameEnd;

        // 交易事件
        public UnityEvent<Player, PropertyTile> OnPropertyPurchased; // 玩家购买地产
        public UnityEvent<Player, PropertyTile, int> OnPropertySold; // 玩家出售地产（玩家，地产，出售价格）
        public UnityEvent<Player, Player, PropertyTile, int> OnRentPaid; // 支付过路费（支付者，接收者，地产，租金）

        protected override void OnSingletonAwake()
        {
            base.OnSingletonAwake();

            // 初始化事件
            if (OnPlayerTurnStart == null) OnPlayerTurnStart = new UnityEvent<Player>();
            if (OnPlayerTurnEnd == null) OnPlayerTurnEnd = new UnityEvent<Player>();
            if (OnPlayerMove == null) OnPlayerMove = new UnityEvent<Player>();
            if (OnPlayerBankrupt == null) OnPlayerBankrupt = new UnityEvent<Player>();
            if (OnGameEnd == null) OnGameEnd = new UnityEvent<Player>();
            if (OnPropertyPurchased == null) OnPropertyPurchased = new UnityEvent<Player, PropertyTile>();
            if (OnPropertySold == null) OnPropertySold = new UnityEvent<Player, PropertyTile, int>();
            if (OnRentPaid == null) OnRentPaid = new UnityEvent<Player, Player, PropertyTile, int>();

            // 自动查找组件
            if (boardManager == null)
            {
                boardManager = FindObjectOfType<BoardManager>();
                // 如果未找到，创建一个
                if (diceManager == null)
                {
                    GameObject boardManagerObject = new GameObject("BoardManager");
                    boardManager = boardManagerObject.AddComponent<BoardManager>();
                    Debug.Log("GameManager: BoardManager 创建成功");
                }
            }

            if (diceManager == null)
            {
                diceManager = FindObjectOfType<DiceManager>();
                // 如果未找到，创建一个
                if (diceManager == null)
                {
                    GameObject diceManagerObject = new GameObject("DiceManager");
                    diceManager = diceManagerObject.AddComponent<DiceManager>();
                    Debug.Log("GameManager: DiceManager 创建成功");
                }
            }

#if UNITY_EDITOR
            // 如果没有在 Inspector 指定玩家预制体，则尝试从 Assets/Prefabs/Player.prefab 加载
            if (playerPrefab == null)
            {
                const string playerPrefabPath = "Assets/Prefabs/Player.prefab";
                var prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(playerPrefabPath);
                if (prefab != null)
                {
                    playerPrefab = prefab;
                    Debug.Log($"GameManager: 从 {playerPrefabPath} 加载了 Player 预制体");
                }
                else
                {
                    Debug.LogWarning($"GameManager: 未能从 {playerPrefabPath} 加载 Player 预制体，请在 Inspector 手动指定 playerPrefab");
                }
            }
#endif
        }

        private void Start()
        {
            InitializeGame();
        }

        /// <summary>
        /// 初始化游戏
        /// </summary>
        public void InitializeGame()
        {
            currentState = GameState.Initializing;

            // 确保有棋盘管理器
            if (boardManager == null)
            {
                Debug.LogWarning("GameManager: 未找到 BoardManager，正在创建...");
                GameObject boardManagerObject = new GameObject("BoardManager");
                boardManager = boardManagerObject.AddComponent<BoardManager>();
                Debug.Log("GameManager: BoardManager 创建成功");
            }

            // 初始化玩家（如果还没有）
            if (players.Count == 0)
            {
                InitializePlayers();
            }

            // 初始化玩家位置和金钱
            foreach (var player in players)
            {
                player.Initialize($"玩家 {players.IndexOf(player) + 1}", initialMoney);
                // 更新玩家初始视觉位置
                player.UpdateVisualPosition(boardManager);
            }

            currentPlayerIndex = 0;
            currentState = GameState.Playing;

            Debug.Log("游戏初始化完成");
            StartPlayerTurn();
        }

        /// <summary>
        /// 初始化玩家列表
        /// </summary>
        private void InitializePlayers()
        {
            if (players.Count == 0)
            {
                if (playerPrefab == null)
                {
                    Debug.LogWarning("GameManager: 没有找到玩家预制体 playerPrefab，无法自动创建玩家。");
                    return;
                }

                // 自动创建 playerCount 个玩家
                for (int i = 0; i < playerCount; i++)
                {
                    GameObject playerObject = Instantiate(playerPrefab);
                    playerObject.name = $"Player_{i + 1}";

                    // 尝试获取 Player 组件；如果没有，则根据索引添加 HumanPlayer / AIPlayer
                    Player player = playerObject.GetComponent<Player>();
                    if (player == null)
                    {
                        if (i == 0)
                        {
                            player = playerObject.AddComponent<HumanPlayer>();
                            Debug.Log($"GameManager: 自动为 {playerObject.name} 添加 HumanPlayer 组件（玩家控制）。");
                        }
                        else
                        {
                            player = playerObject.AddComponent<AIPlayer>();
                            Debug.Log($"GameManager: 自动为 {playerObject.name} 添加 AIPlayer 组件（AI 控制）。");
                        }
                    }

                    // 为每个玩家设置不同的颜色（如果有 Renderer）
                    ApplyPlayerColor(playerObject, i);

                    AddPlayer(player);
                }

                Debug.Log($"GameManager: 自动创建了 {players.Count} 个玩家。");
            }
        }

        /// <summary>
        /// 根据索引为玩家应用不同的颜色
        /// </summary>
        private void ApplyPlayerColor(GameObject playerObject, int index)
        {
            if (playerObject == null || playerColors == null || playerColors.Length == 0)
            {
                return;
            }

            Color color = playerColors[index % playerColors.Length];

            // 尝试在当前物体及子物体上查找 Renderer
            var renderers = playerObject.GetComponentsInChildren<Renderer>();
            foreach (var renderer in renderers)
            {
                if (renderer.material != null)
                {
                    if (renderer.material.HasProperty("_BaseColor"))
                    {
                        renderer.material.SetColor("_BaseColor", color);
                    }
                    if (renderer.material.HasProperty("_Color"))
                    {
                        renderer.material.color = color;
                    }
                }
            }
        }

        /// <summary>
        /// 开始玩家回合
        /// </summary>
        public void StartPlayerTurn()
        {
            if (currentState != GameState.Playing)
            {
                return;
            }

            Player currentPlayer = CurrentPlayer;
            if (currentPlayer == null || currentPlayer.IsBankrupt)
            {
                // 跳过破产玩家
                NextPlayer();
                return;
            }

            OnPlayerTurnStart?.Invoke(currentPlayer);
            Debug.Log($"轮到 {currentPlayer.PlayerName} 行动");

            // 如果是 AI 玩家，自动执行一整回合
            if (currentPlayer is AIPlayer aiPlayer)
            {
                aiPlayer.StartAITurn();
            }
        }

        /// <summary>
        /// 结束当前玩家回合
        /// </summary>
        public void EndPlayerTurn()
        {
            Player currentPlayer = CurrentPlayer;
            if (currentPlayer != null)
            {
                OnPlayerTurnEnd?.Invoke(currentPlayer);
            }

            // 重置骰子管理器的连续双骰计数
            if (diceManager != null)
            {
                diceManager.ResetConsecutiveDoubles();
            }

            NextPlayer();
        }

        /// <summary>
        /// 切换到下一个玩家
        /// </summary>
        private void NextPlayer()
        {
            do
            {
                currentPlayerIndex = (currentPlayerIndex + 1) % players.Count;
            }
            while (CurrentPlayer != null && CurrentPlayer.IsBankrupt && HasActivePlayers());

            if (!HasActivePlayers())
            {
                EndGame();
                return;
            }

            StartPlayerTurn();
        }

        /// <summary>
        /// 检查是否还有活跃玩家
        /// </summary>
        private bool HasActivePlayers()
        {
            foreach (var player in players)
            {
                if (!player.IsBankrupt)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 投掷骰子并移动玩家（完整流程）
        /// </summary>
        /// <returns>骰子结果，如果投掷失败返回null</returns>
        public DiceResult RollDiceAndMove()
        {
            if (currentState != GameState.Playing)
            {
                Debug.LogWarning("游戏未在进行中，无法投掷骰子");
                return null;
            }

            Player currentPlayer = CurrentPlayer;
            if (currentPlayer == null || currentPlayer.IsBankrupt)
            {
                Debug.LogWarning("当前玩家无效或已破产，无法投掷骰子");
                return null;
            }

            if (diceManager == null)
            {
                Debug.LogError("DiceManager 未找到！");
                return null;
            }

            // 投掷骰子
            DiceResult result = diceManager.RollDice();

            // 检查连续三次双骰（进监狱）
            if (result.isTripleDouble)
            {
                Debug.LogWarning($"{currentPlayer.PlayerName} 连续三次双骰，触发进监狱规则");
                // 移动到监狱位置（索引10）
                MovePlayerToPosition(currentPlayer, 10);
                EndPlayerTurn();
                return result;
            }

            // 移动玩家（使用动画移动）
            Coroutine moveCoroutine = HandlePlayerMoveAnimated(currentPlayer, result.total, () =>
            {
                // 移动完成后的回调
                Debug.Log($"{currentPlayer.PlayerName} 移动动画完成");
            });
            
            if (moveCoroutine == null)
            {
                Debug.LogWarning($"{currentPlayer.PlayerName} 移动失败");
                return result;
            }

            // 如果不是双骰，结束回合；如果是双骰，可以再次投掷
            if (!result.isDouble)
            {
                // 注意：这里不自动结束回合，由UI或其他逻辑决定何时结束
                // EndPlayerTurn();
            }

            return result;
        }

        /// <summary>
        /// 处理玩家移动（立即移动，无动画）
        /// </summary>
        /// <param name="player">移动的玩家</param>
        /// <param name="steps">移动步数</param>
        /// <returns>是否移动成功</returns>
        public bool HandlePlayerMove(Player player, int steps)
        {
            if (player == null || currentState != GameState.Playing)
            {
                return false;
            }

            // 执行移动（验证在Player.Move中进行）
            bool moveSuccess = player.Move(steps, boardManager.BoardSize);
            
            if (!moveSuccess)
            {
                Debug.LogWarning($"{player.PlayerName} 移动失败");
                return false;
            }

            // 更新视觉位置
            player.UpdateVisualPosition(boardManager);

            OnPlayerMove?.Invoke(player);

            // 触发到达格子的事件
            Tile currentTile = boardManager.GetPlayerTile(player);
            if (currentTile != null)
            {
                currentTile.OnLanded(player);
            }

            Debug.Log($"{player.PlayerName} 移动了 {steps} 步，到达位置 {player.CurrentPosition}");
            return true;
        }

        /// <summary>
        /// 处理玩家移动（带动画）
        /// </summary>
        /// <param name="player">移动的玩家</param>
        /// <param name="steps">移动步数</param>
        /// <param name="onComplete">移动完成回调</param>
        /// <returns>移动协程</returns>
        public Coroutine HandlePlayerMoveAnimated(Player player, int steps, System.Action onComplete = null)
        {
            if (player == null || currentState != GameState.Playing)
            {
                return null;
            }

            // 开始动画移动
            Coroutine moveCoroutine = player.MoveAnimated(steps, boardManager);
            
            if (moveCoroutine != null)
            {
                // 等待移动完成后触发事件
                StartCoroutine(WaitForMoveComplete(player, moveCoroutine, onComplete));
            }
            else
            {
                // 如果动画移动失败，使用立即移动
                HandlePlayerMove(player, steps);
                onComplete?.Invoke();
            }

            return moveCoroutine;
        }

        /// <summary>
        /// 等待移动完成的协程
        /// </summary>
        private IEnumerator WaitForMoveComplete(Player player, Coroutine moveCoroutine, System.Action onComplete)
        {
            // 等待移动协程完成
            while (player.IsMoving)
            {
                yield return null;
            }

            // 更新视觉位置
            player.UpdateVisualPosition(boardManager);

            OnPlayerMove?.Invoke(player);

            // 触发到达格子的事件
            Tile currentTile = boardManager.GetPlayerTile(player);
            if (currentTile != null)
            {
                currentTile.OnLanded(player);
            }

            Debug.Log($"{player.PlayerName} 移动完成，到达位置 {player.CurrentPosition}");
            
            onComplete?.Invoke();
        }

        /// <summary>
        /// 将玩家移动到指定位置（用于特殊事件，如进监狱）
        /// </summary>
        /// <param name="player">玩家</param>
        /// <param name="position">目标位置</param>
        public void MovePlayerToPosition(Player player, int position)
        {
            if (player == null)
            {
                return;
            }

            player.MoveTo(position);
            OnPlayerMove?.Invoke(player);

            // 触发到达格子的事件
            Tile currentTile = boardManager.GetPlayerTile(player);
            if (currentTile != null)
            {
                currentTile.OnLanded(player);
            }

            Debug.Log($"{player.PlayerName} 被移动到位置 {position}");
        }

        /// <summary>
        /// 检查玩家是否破产
        /// </summary>
        /// <param name="player">要检查的玩家</param>
        public void CheckPlayerBankruptcy(Player player)
        {
            if (player != null && player.IsBankrupt)
            {
                OnPlayerBankrupt?.Invoke(player);
                Debug.Log($"{player.PlayerName} 破产了！");

                // 如果当前玩家破产，结束回合
                if (player == CurrentPlayer)
                {
                    EndPlayerTurn();
                }

                // 每当有玩家破产时，检查游戏输赢条件
                CheckHumanWinLoseCondition();
                CheckWinCondition(); // 保留通用的“只剩一人”胜利规则（兼容无真人玩家的情况）
            }
        }

        /// <summary>
        /// 检查“真人玩家 vs AI”场景下的输赢条件：
        /// - 赢：除真人玩家外的所有玩家都破产
        /// - 输：真人玩家破产
        /// </summary>
        private void CheckHumanWinLoseCondition()
        {
            // 找到第一个 HumanPlayer（假设只有一个真人玩家）
            HumanPlayer humanPlayer = null;
            foreach (var p in players)
            {
                if (p is HumanPlayer hp)
                {
                    humanPlayer = hp;
                    break;
                }
            }

            // 如果没有配置 HumanPlayer，则不应用该特殊规则
            if (humanPlayer == null)
            {
                return;
            }

            // 输的条件：真人玩家破产
            if (humanPlayer.IsBankrupt)
            {
                Debug.Log("游戏结束：真人玩家破产，判定为失败。");
                EndGame(); // 让现有逻辑根据总资产选出获胜者（通常是某个 AI）
                return;
            }

            // 赢的条件：除真人玩家外的所有玩家都破产
            bool anyOtherAlive = false;
            foreach (var p in players)
            {
                if (p == humanPlayer)
                {
                    continue;
                }

                if (!p.IsBankrupt)
                {
                    anyOtherAlive = true;
                    break;
                }
            }

            if (!anyOtherAlive)
            {
                Debug.Log("游戏结束：除真人玩家外所有玩家都破产，真人玩家获胜。");
                EndGame(humanPlayer);
            }
        }

        /// <summary>
        /// 检查胜利条件
        /// </summary>
        private void CheckWinCondition()
        {
            int activePlayerCount = 0;
            Player winner = null;

            foreach (var player in players)
            {
                if (!player.IsBankrupt)
                {
                    activePlayerCount++;
                    winner = player;
                }
            }

            if (activePlayerCount == 1 && winner != null)
            {
                EndGame(winner);
            }
        }

        /// <summary>
        /// 结束游戏
        /// </summary>
        /// <param name="winner">获胜玩家（可选）</param>
        public void EndGame(Player winner = null)
        {
            currentState = GameState.GameOver;

            if (winner == null)
            {
                // 如果没有明确的获胜者，选择总资产最多的玩家
                winner = GetPlayerWithMostAssets();
            }

            OnGameEnd?.Invoke(winner);
            Debug.Log($"游戏结束！获胜者: {winner?.PlayerName ?? "无"}");
        }

        /// <summary>
        /// 获取总资产最多的玩家
        /// </summary>
        private Player GetPlayerWithMostAssets()
        {
            Player winner = null;
            int maxAssets = 0;

            foreach (var player in players)
            {
                if (!player.IsBankrupt)
                {
                    int assets = player.GetTotalAssets();
                    if (assets > maxAssets)
                    {
                        maxAssets = assets;
                        winner = player;
                    }
                }
            }

            return winner;
        }

        /// <summary>
        /// 添加玩家
        /// </summary>
        public void AddPlayer(Player player)
        {
            if (player != null && !players.Contains(player))
            {
                players.Add(player);
            }
        }

        /// <summary>
        /// 移除玩家
        /// </summary>
        public void RemovePlayer(Player player)
        {
            if (player != null)
            {
                players.Remove(player);
            }
        }

        #region 地产交易系统

        /// <summary>
        /// 购买地产（当前玩家购买当前位置的地产）
        /// </summary>
        /// <returns>是否购买成功</returns>
        public bool BuyProperty()
        {
            return BuyProperty(CurrentPlayer, CurrentPlayer?.CurrentPosition ?? -1);
        }

        /// <summary>
        /// 购买地产（指定玩家购买指定位置的地产）
        /// </summary>
        /// <param name="player">购买者</param>
        /// <param name="tileIndex">格子索引（-1表示当前位置）</param>
        /// <returns>是否购买成功</returns>
        public bool BuyProperty(Player player, int tileIndex = -1)
        {
            if (player == null)
            {
                Debug.LogError("GameManager: 玩家为空，无法购买地产");
                return false;
            }

            if (boardManager == null)
            {
                Debug.LogError("GameManager: BoardManager 为空，无法购买地产");
                return false;
            }

            // 如果未指定位置，使用玩家当前位置
            if (tileIndex < 0)
            {
                tileIndex = player.CurrentPosition;
            }

            Tile tile = boardManager.GetTile(tileIndex);
            if (tile == null)
            {
                Debug.LogError($"GameManager: 位置 {tileIndex} 的格子不存在");
                return false;
            }

            PropertyTile propertyTile = tile as PropertyTile;
            if (propertyTile == null)
            {
                Debug.LogWarning($"GameManager: 位置 {tileIndex} 的格子不是地产格子，无法购买");
                return false;
            }

            // 执行购买
            bool success = propertyTile.Purchase(player);
            
            if (success)
            {
                OnPropertyPurchased?.Invoke(player, propertyTile);
                CheckPlayerBankruptcy(player);
            }

            return success;
        }

        /// <summary>
        /// 出售地产（当前玩家出售指定的地产）
        /// </summary>
        /// <param name="propertyTile">要出售的地产</param>
        /// <param name="sellPrice">出售价格（null表示使用默认价格：原价的一半）</param>
        /// <returns>是否出售成功</returns>
        public bool SellProperty(PropertyTile propertyTile, int? sellPrice = null)
        {
            return SellProperty(CurrentPlayer, propertyTile, sellPrice);
        }

        /// <summary>
        /// 出售地产（指定玩家出售指定的地产）
        /// </summary>
        /// <param name="player">出售者</param>
        /// <param name="propertyTile">要出售的地产</param>
        /// <param name="sellPrice">出售价格（null表示使用默认价格：原价的一半）</param>
        /// <returns>是否出售成功</returns>
        public bool SellProperty(Player player, PropertyTile propertyTile, int? sellPrice = null)
        {
            if (player == null)
            {
                Debug.LogError("GameManager: 玩家为空，无法出售地产");
                return false;
            }

            if (propertyTile == null)
            {
                Debug.LogError("GameManager: 地产为空，无法出售");
                return false;
            }

            // 执行出售
            int finalSellPrice = sellPrice ?? (propertyTile.PropertyPrice / 2);
            bool success = propertyTile.Sell(player, sellPrice);

            if (success)
            {
                OnPropertySold?.Invoke(player, propertyTile, finalSellPrice);
            }

            return success;
        }

        /// <summary>
        /// 支付过路费（当前玩家支付当前位置的过路费）
        /// </summary>
        /// <returns>是否支付成功</returns>
        public bool PayRent()
        {
            return PayRent(CurrentPlayer, CurrentPlayer?.CurrentPosition ?? -1);
        }

        /// <summary>
        /// 支付过路费（指定玩家支付指定位置的过路费）
        /// </summary>
        /// <param name="player">支付者</param>
        /// <param name="tileIndex">格子索引（-1表示当前位置）</param>
        /// <returns>是否支付成功</returns>
        public bool PayRent(Player player, int tileIndex = -1)
        {
            if (player == null)
            {
                Debug.LogError("GameManager: 玩家为空，无法支付过路费");
                return false;
            }

            if (boardManager == null)
            {
                Debug.LogError("GameManager: BoardManager 为空，无法支付过路费");
                return false;
            }

            // 如果未指定位置，使用玩家当前位置
            if (tileIndex < 0)
            {
                tileIndex = player.CurrentPosition;
            }

            Tile tile = boardManager.GetTile(tileIndex);
            if (tile == null)
            {
                Debug.LogError($"GameManager: 位置 {tileIndex} 的格子不存在");
                return false;
            }

            PropertyTile propertyTile = tile as PropertyTile;
            if (propertyTile == null)
            {
                Debug.LogWarning($"GameManager: 位置 {tileIndex} 的格子不是地产格子，无需支付过路费");
                return false;
            }

            // 执行支付过路费
            bool success = propertyTile.PayRent(player);

            if (success && propertyTile.Owner != null)
            {
                int rent = propertyTile.CalculateRent();
                OnRentPaid?.Invoke(player, propertyTile.Owner, propertyTile, rent);
                CheckPlayerBankruptcy(player);
            }

            return success;
        }

        /// <summary>
        /// 获取玩家可以购买的地产列表（当前位置的地产）
        /// </summary>
        /// <param name="player">玩家（null表示当前玩家）</param>
        /// <returns>可购买的地产，如果当前位置不是地产或已被购买则返回null</returns>
        public PropertyTile GetPurchasableProperty(Player player = null)
        {
            if (player == null)
            {
                player = CurrentPlayer;
            }

            if (player == null || boardManager == null)
            {
                return null;
            }

            Tile tile = boardManager.GetTile(player.CurrentPosition);
            PropertyTile propertyTile = tile as PropertyTile;

            if (propertyTile != null && !propertyTile.IsOwned)
            {
                return propertyTile;
            }

            return null;
        }

        /// <summary>
        /// 获取玩家拥有的所有地产列表
        /// </summary>
        /// <param name="player">玩家（null表示当前玩家）</param>
        /// <returns>玩家拥有的地产列表</returns>
        public IReadOnlyList<PropertyTile> GetPlayerProperties(Player player = null)
        {
            if (player == null)
            {
                player = CurrentPlayer;
            }

            if (player == null)
            {
                return new List<PropertyTile>();
            }

            return player.Properties;
        }

        #endregion
    }

    /// <summary>
    /// 游戏状态枚举
    /// </summary>
    public enum GameState
    {
        Initializing,   // 初始化中
        Playing,        // 进行中
        Paused,         // 暂停
        GameOver        // 游戏结束
    }
}
