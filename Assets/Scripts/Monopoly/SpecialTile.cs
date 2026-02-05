using UnityEngine;

namespace Monopoly
{
    /// <summary>
    /// 特殊格子类，继承自Tile，用于处理起点、监狱等特殊格子
    /// </summary>
    public class SpecialTile : Tile
    {
        protected override void Awake()
        {
            base.Awake();
        }

        /// <summary>
        /// 初始化特殊格子
        /// </summary>
        /// <param name="index">格子索引</param>
        /// <param name="name">格子名称</param>
        /// <param name="type">格子类型</param>
        public override void Initialize(int index, string name, TileType type)
        {
            base.Initialize(index, name, type);
        }

        public override void OnLanded(Player player)
        {
            base.OnLanded(player);

            // 根据不同类型执行特殊逻辑
            switch (tileType)
            {
                case TileType.Start:
                    // 起点：经过时获得奖励（在Player.Move中已处理）
                    Debug.Log($"{player.PlayerName} 到达起点");
                    break;

                case TileType.Jail:
                    // 监狱：只是路过，不执行操作
                    Debug.Log($"{player.PlayerName} 路过监狱");
                    break;

                case TileType.GoToJail:
                    // 进监狱：移动到监狱位置
                    Debug.Log($"{player.PlayerName} 被送进监狱！");
                    // 这里可以添加移动到监狱的逻辑
                    break;

                case TileType.FreeParking:
                    // 免费停车：不执行任何操作
                    Debug.Log($"{player.PlayerName} 到达免费停车区");
                    break;

                case TileType.Chance:
                    // 机会卡：抽取机会卡（后续实现）
                    Debug.Log($"{player.PlayerName} 到达机会格，抽取机会卡");
                    break;

                case TileType.CommunityChest:
                    // 社区基金：抽取社区基金卡（后续实现）
                    Debug.Log($"{player.PlayerName} 到达社区基金格，抽取社区基金卡");
                    break;

                case TileType.Tax:
                    // 税收：支付税收（后续实现）
                    Debug.Log($"{player.PlayerName} 到达税收格，支付税收");
                    break;
            }
        }
    }
}
