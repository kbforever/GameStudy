using UnityEngine;
using UnityEngine.Events;

namespace Monopoly
{
    /// <summary>
    /// 格子基类，所有格子类型的父类
    /// </summary>
    public abstract class Tile : MonoBehaviour
    {
        [Header("格子基本信息")]
        [SerializeField] protected string tileName;
        [SerializeField] protected int tileIndex;
        [SerializeField] protected TileType tileType;

        /// <summary>
        /// 格子名称
        /// </summary>
        public string TileName => tileName;

        /// <summary>
        /// 格子在棋盘上的索引（0-39）
        /// </summary>
        public int TileIndex => tileIndex;

        /// <summary>
        /// 格子类型
        /// </summary>
        public TileType TileType => tileType;

        /// <summary>
        /// 玩家到达此格子时触发的事件
        /// </summary>
        public UnityEvent<Player> OnPlayerLanded;

        protected virtual void Awake()
        {
            if (OnPlayerLanded == null)
            {
                OnPlayerLanded = new UnityEvent<Player>();
            }
        }

        /// <summary>
        /// 当玩家到达此格子时调用
        /// </summary>
        /// <param name="player">到达的玩家</param>
        public virtual void OnLanded(Player player)
        {
            OnPlayerLanded?.Invoke(player);
        }

        /// <summary>
        /// 初始化格子数据
        /// </summary>
        /// <param name="index">格子索引</param>
        /// <param name="name">格子名称</param>
        /// <param name="type">格子类型</param>
        public virtual void Initialize(int index, string name, TileType type)
        {
            tileIndex = index;
            tileName = name;
            tileType = type;
        }
    }

    /// <summary>
    /// 格子类型枚举
    /// </summary>
    public enum TileType
    {
        Start,          // 起点
        Property,       // 地产
        Tax,            // 税收
        Chance,         // 机会
        CommunityChest, // 社区基金
        Jail,           // 监狱
        GoToJail,       // 进监狱
        FreeParking,    // 免费停车
        Utility,        // 公共事业
        Railroad        // 铁路
    }
}
