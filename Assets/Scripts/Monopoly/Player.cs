using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Monopoly
{
    /// <summary>
    /// 玩家基类，包含玩家基本数据和操作
    /// </summary>
    public abstract class Player : MonoBehaviour
    {
        [Header("玩家基本信息")]
        [SerializeField] protected string playerName;
        [SerializeField] protected int money;
        [SerializeField] protected int currentPosition;
        [SerializeField] protected bool isBankrupt;

        [Header("移动设置")]
        [SerializeField] protected float moveSpeed = 5f; // 移动速度
        [SerializeField] protected float moveDelay = 0.2f; // 每格移动间隔
        [SerializeField] protected bool useAnimatedMovement = true; // 是否使用动画移动

        /// <summary>
        /// 玩家移动事件（从位置，到位置）
        /// </summary>
        public UnityEvent<int, int> OnPlayerMoved;

        /// <summary>
        /// 玩家移动完成事件
        /// </summary>
        public UnityEvent OnPlayerMoveCompleted;

        private bool isMoving = false; // 是否正在移动
        private Coroutine moveCoroutine; // 移动协程

        /// <summary>
        /// 玩家拥有的房产列表
        /// </summary>
        protected List<PropertyTile> properties = new List<PropertyTile>();

        /// <summary>
        /// 玩家名称
        /// </summary>
        public string PlayerName => playerName;

        /// <summary>
        /// 玩家当前金钱
        /// </summary>
        public int Money => money;

        /// <summary>
        /// 玩家当前位置（格子索引）
        /// </summary>
        public int CurrentPosition => currentPosition;

        /// <summary>
        /// 是否破产
        /// </summary>
        public bool IsBankrupt => isBankrupt;

        /// <summary>
        /// 玩家拥有的房产列表（只读）
        /// </summary>
        public IReadOnlyList<PropertyTile> Properties => properties;

        protected virtual void Awake()
        {
            isBankrupt = false;
            currentPosition = 0; // 起始位置为0（起点）
        }

        /// <summary>
        /// 初始化玩家
        /// </summary>
        /// <param name="name">玩家名称</param>
        /// <param name="initialMoney">初始金钱</param>
        public virtual void Initialize(string name, int initialMoney = 1500)
        {
            playerName = name;
            money = initialMoney;
            currentPosition = 0;
            isBankrupt = false;
            properties.Clear();
        }

        /// <summary>
        /// 移动玩家到指定位置
        /// </summary>
        /// <param name="newPosition">新位置（格子索引）</param>
        public virtual void MoveTo(int newPosition)
        {
            currentPosition = newPosition;
        }

        /// <summary>
        /// 根据步数移动玩家（立即移动，无动画）
        /// </summary>
        /// <param name="steps">移动步数</param>
        /// <param name="boardSize">棋盘大小（默认40）</param>
        /// <returns>是否移动成功</returns>
        public virtual bool Move(int steps, int boardSize = 40)
        {
            // 验证移动步数
            if (steps < 0)
            {
                Debug.LogWarning($"{playerName} 移动步数不能为负数: {steps}");
                return false;
            }

            // 验证当前位置
            if (currentPosition < 0 || currentPosition >= boardSize)
            {
                Debug.LogWarning($"{playerName} 当前位置无效: {currentPosition}，棋盘大小: {boardSize}");
                return false;
            }

            // 计算新位置
            int newPosition = (currentPosition + steps) % boardSize;
            
            // 如果经过起点，获得起点奖励
            if (newPosition < currentPosition)
            {
                ReceiveMoney(200);
                Debug.Log($"{playerName} 经过起点，获得 200");
            }

            currentPosition = newPosition;
            UpdateVisualPosition();
            return true;
        }

        /// <summary>
        /// 根据步数移动玩家（带动画）
        /// </summary>
        /// <param name="steps">移动步数</param>
        /// <param name="boardManager">棋盘管理器</param>
        /// <returns>协程，用于等待移动完成</returns>
        public virtual Coroutine MoveAnimated(int steps, BoardManager boardManager)
        {
            if (isMoving)
            {
                Debug.LogWarning($"{playerName} 正在移动中，无法执行新的移动");
                return null;
            }

            if (boardManager == null)
            {
                Debug.LogError("BoardManager 未提供，无法执行动画移动");
                return null;
            }

            // 停止之前的移动协程
            if (moveCoroutine != null)
            {
                StopCoroutine(moveCoroutine);
            }

            moveCoroutine = StartCoroutine(MoveAnimatedCoroutine(steps, boardManager));
            return moveCoroutine;
        }

        /// <summary>
        /// 移动动画协程
        /// </summary>
        private IEnumerator MoveAnimatedCoroutine(int steps, BoardManager boardManager)
        {
            isMoving = true;

            // 验证移动步数
            if (steps < 0)
            {
                Debug.LogWarning($"{playerName} 移动步数不能为负数: {steps}");
                isMoving = false;
                yield break;
            }

            int startPosition = currentPosition;
            int boardSize = boardManager.BoardSize;

            // 计算目标位置
            int targetPosition = (currentPosition + steps) % boardSize;

            // 如果经过起点，获得起点奖励
            if (targetPosition < currentPosition)
            {
                ReceiveMoney(200);
                Debug.Log($"{playerName} 经过起点，获得 200");
            }

            // 逐步移动每一格
            int currentStep = 0;
            while (currentStep < steps)
            {
                int nextPosition = (startPosition + currentStep + 1) % boardSize;
                
                // 移动到下一格
                Tile nextTile = boardManager.GetTile(nextPosition);
                if (nextTile != null)
                {
                    Vector3 targetPos = nextTile.transform.position;
                    yield return StartCoroutine(MoveToPosition(targetPos));
                }

                currentStep++;
                currentPosition = nextPosition;
                OnPlayerMoved?.Invoke(currentPosition - 1 < 0 ? boardSize - 1 : currentPosition - 1, currentPosition);

                // 每格之间的延迟
                if (currentStep < steps)
                {
                    yield return new WaitForSeconds(moveDelay);
                }
            }

            // 更新最终位置
            currentPosition = targetPosition;
            UpdateVisualPosition();

            isMoving = false;
            OnPlayerMoveCompleted?.Invoke();
            Debug.Log($"{playerName} 移动完成，到达位置 {currentPosition}");
        }

        /// <summary>
        /// 移动到指定世界坐标位置
        /// </summary>
        private IEnumerator MoveToPosition(Vector3 targetPosition)
        {
            Vector3 startPosition = transform.position;
            float distance = Vector3.Distance(startPosition, targetPosition);
            float elapsedTime = 0f;

            while (elapsedTime < distance / moveSpeed)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / (distance / moveSpeed);
                transform.position = Vector3.Lerp(startPosition, targetPosition, t);
                yield return null;
            }

            transform.position = targetPosition;
        }

        /// <summary>
        /// 更新玩家视觉位置（根据当前格子位置）
        /// </summary>
        public virtual void UpdateVisualPosition()
        {
            // 这个方法应该由子类或外部系统调用，根据BoardManager更新位置
            // 如果已经有BoardManager引用，可以在这里实现
        }

        /// <summary>
        /// 更新玩家视觉位置（根据BoardManager）
        /// </summary>
        /// <param name="boardManager">棋盘管理器</param>
        public virtual void UpdateVisualPosition(BoardManager boardManager)
        {
            if (boardManager == null)
            {
                return;
            }

            Tile currentTile = boardManager.GetTile(currentPosition);
            if (currentTile != null)
            {
                Vector3 tilePosition = currentTile.transform.position;
                // 在格子上方稍微抬高一点，避免重叠
                transform.position = tilePosition + Vector3.up * 0.5f;
            }
        }

        /// <summary>
        /// 是否正在移动
        /// </summary>
        public bool IsMoving => isMoving;

        /// <summary>
        /// 支付金钱
        /// </summary>
        /// <param name="amount">支付金额</param>
        /// <returns>是否支付成功</returns>
        public virtual bool PayMoney(int amount)
        {
            if (amount < 0)
            {
                Debug.LogWarning($"支付金额不能为负数: {amount}");
                return false;
            }

            if (money < amount)
            {
                Debug.LogWarning($"{playerName} 资金不足，无法支付 {amount}（当前资金: {money}）");
                // 检查是否破产
                CheckBankruptcy();
                return false;
            }

            money -= amount;
            return true;
        }

        /// <summary>
        /// 获得金钱
        /// </summary>
        /// <param name="amount">获得金额</param>
        public virtual void ReceiveMoney(int amount)
        {
            if (amount < 0)
            {
                Debug.LogWarning($"获得金额不能为负数: {amount}");
                return;
            }

            money += amount;
        }

        /// <summary>
        /// 检查余额是否足够
        /// </summary>
        /// <param name="amount">需要检查的金额</param>
        /// <returns>是否有足够余额</returns>
        public virtual bool HasEnoughMoney(int amount)
        {
            return money >= amount;
        }

        /// <summary>
        /// 添加房产
        /// </summary>
        /// <param name="property">房产</param>
        public virtual void AddProperty(PropertyTile property)
        {
            if (property != null && !properties.Contains(property))
            {
                properties.Add(property);
            }
        }

        /// <summary>
        /// 移除房产
        /// </summary>
        /// <param name="property">房产</param>
        public virtual void RemoveProperty(PropertyTile property)
        {
            if (property != null)
            {
                properties.Remove(property);
            }
        }

        /// <summary>
        /// 检查是否破产
        /// </summary>
        protected virtual void CheckBankruptcy()
        {
            if (money < 0)
            {
                isBankrupt = true;
                Debug.Log($"{playerName} 破产了！");
            }
        }

        /// <summary>
        /// 获取玩家总资产（金钱 + 房产价值）
        /// </summary>
        /// <returns>总资产</returns>
        public virtual int GetTotalAssets()
        {
            int totalAssets = money;
            foreach (var property in properties)
            {
                totalAssets += property.PropertyPrice;
            }
            return totalAssets;
        }
    }
}
