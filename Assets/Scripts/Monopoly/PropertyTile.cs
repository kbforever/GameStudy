using UnityEngine;

namespace Monopoly
{
    /// <summary>
    /// 地产格子类，继承自Tile，表示可购买的地产
    /// </summary>
    public class PropertyTile : Tile
    {
        [Header("地产属性")]
        [SerializeField] private int propertyPrice;
        [SerializeField] private int baseRent;
        [SerializeField] private Player owner;

        /// <summary>
        /// 地产价格
        /// </summary>
        public int PropertyPrice => propertyPrice;

        /// <summary>
        /// 基础租金
        /// </summary>
        public int BaseRent => baseRent;

        /// <summary>
        /// 地产所有者
        /// </summary>
        public Player Owner
        {
            get => owner;
            set => owner = value;
        }

        /// <summary>
        /// 是否已被购买
        /// </summary>
        public bool IsOwned => owner != null;

        protected override void Awake()
        {
            base.Awake();
            tileType = TileType.Property;
        }

        /// <summary>
        /// 初始化地产格子
        /// </summary>
        /// <param name="index">格子索引</param>
        /// <param name="name">地产名称</param>
        /// <param name="price">地产价格</param>
        /// <param name="rent">基础租金</param>
        public void Initialize(int index, string name, int price, int rent)
        {
            base.Initialize(index, name, TileType.Property);
            propertyPrice = price;
            baseRent = rent;
            owner = null;
        }

        /// <summary>
        /// 购买地产
        /// </summary>
        /// <param name="buyer">购买者</param>
        /// <returns>是否购买成功</returns>
        public bool Purchase(Player buyer)
        {
            if (IsOwned)
            {
                Debug.LogWarning($"{tileName} 已经被 {owner.PlayerName} 拥有，无法购买");
                return false;
            }

            if (buyer.Money < propertyPrice)
            {
                Debug.LogWarning($"{buyer.PlayerName} 资金不足，无法购买 {tileName}");
                return false;
            }

            // 扣除金钱
            buyer.PayMoney(propertyPrice);
            owner = buyer;
            buyer.AddProperty(this);

            Debug.Log($"{buyer.PlayerName} 购买了 {tileName}，花费 {propertyPrice}");
            return true;
        }

        /// <summary>
        /// 计算租金
        /// </summary>
        /// <returns>租金金额</returns>
        public int CalculateRent()
        {
            // 基础租金计算，可以根据后续扩展（如房屋数量）进行调整
            return baseRent;
        }

        /// <summary>
        /// 支付租金
        /// </summary>
        /// <param name="payer">支付租金的玩家</param>
        /// <returns>是否支付成功</returns>
        public bool PayRent(Player payer)
        {
            if (!IsOwned)
            {
                Debug.LogWarning($"{tileName} 未被购买，无需支付租金");
                return false;
            }

            if (owner == payer)
            {
                Debug.Log($"{payer.PlayerName} 拥有 {tileName}，无需支付租金");
                return true;
            }

            int rent = CalculateRent();
            if (payer.Money < rent)
            {
                Debug.LogWarning($"{payer.PlayerName} 资金不足，无法支付租金 {rent}");
                return false;
            }

            payer.PayMoney(rent);
            owner.ReceiveMoney(rent);

            Debug.Log($"{payer.PlayerName} 向 {owner.PlayerName} 支付租金 {rent}（{tileName}）");
            return true;
        }

        /// <summary>
        /// 出售地产
        /// </summary>
        /// <param name="seller">出售者</param>
        /// <param name="sellPrice">出售价格（默认按原价的一半）</param>
        /// <returns>是否出售成功</returns>
        public bool Sell(Player seller, int? sellPrice = null)
        {
            if (!IsOwned)
            {
                Debug.LogWarning($"{tileName} 未被购买，无法出售");
                return false;
            }

            if (owner != seller)
            {
                Debug.LogWarning($"{seller.PlayerName} 不是 {tileName} 的所有者，无法出售");
                return false;
            }

            // 默认出售价格为原价的一半（可以调整）
            int finalSellPrice = sellPrice ?? (propertyPrice / 2);

            // 返还金钱给卖家
            seller.ReceiveMoney(finalSellPrice);

            // 清除所有权
            Player previousOwner = owner;
            owner = null;
            previousOwner.RemoveProperty(this);

            Debug.Log($"{seller.PlayerName} 出售了 {tileName}，获得 {finalSellPrice}（原价: {propertyPrice}）");
            return true;
        }

        public override void OnLanded(Player player)
        {
            base.OnLanded(player);

            if (IsOwned)
            {
                // 如果已有所有者，支付租金
                if (owner != player)
                {
                    PayRent(player);
                }
            }
            // 如果未被购买，由玩家决定是否购买（在UI或玩家逻辑中处理）
        }
    }
}
