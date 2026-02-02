using System.Collections.Generic;
using UnityEngine;

namespace Monopoly
{
    /// <summary>
    /// 棋盘管理器，管理40个格子的配置和玩家位置
    /// </summary>
    public class BoardManager : MonoBehaviour
    {
        [Header("棋盘配置")]
        [SerializeField] private int boardSize = 40;
        [SerializeField] private List<Tile> tiles = new List<Tile>();

        [Header("预制体配置")]
        [SerializeField] private GameObject tilePrefab; // 通用格子预制体
        [SerializeField] private GameObject propertyTilePrefab; // 地产格子预制体（可选）
        [SerializeField] private GameObject specialTilePrefab; // 特殊格子预制体（可选）

        [Header("自动加载（编辑器）")]
        [SerializeField] private bool autoLoadPrefabsFromAssets = true;

        [Header("棋盘布局配置")]
        [SerializeField] private float boardRadius = 9f; // 棋盘半径（从中心到边的距离）
        [SerializeField] private Vector3 boardCenter = Vector3.zero; // 棋盘中心位置
        


        [Header("摄像头角度调整")]
        [SerializeField] private float verticalOffset = 1f; // 垂直偏移
        [SerializeField] private float padding = 1.1f; // 边距系数 
        private Camera cam;

        /// <summary>
        /// 格子间距（根据棋盘半径自动计算）
        /// </summary>
        private float TileSpacing
        {
            get
            {
                int tilesPerSide = boardSize / 4; // 每条边的格子数（10）
                // 每条边有 (tilesPerSide - 1) 个间距
                // 边长 = 2 * boardRadius
                // 格子间距 = 边长 / (格子数 - 1)
                return (2f * boardRadius) / (tilesPerSide - 1);
            }
        }

        /// <summary>
        /// 棋盘大小（格子数量）
        /// </summary>
        public int BoardSize => boardSize;

        /// <summary>
        /// 所有格子的列表
        /// </summary>
        public IReadOnlyList<Tile> Tiles => tiles;

        private void Awake()
        {
            cam = Camera.main;
            TryLoadPrefabsFromAssets();

            if (tiles.Count == 0)
            {
                InitializeDefaultBoard();
                AdjustCamera();

            }

            
        }


        private void TryLoadPrefabsFromAssets()
        {
#if UNITY_EDITOR
            if (!autoLoadPrefabsFromAssets) return;

            const string cubePrefabPath = "Assets/Prefabs/Cube.prefab";
            GameObject cubePrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(cubePrefabPath);

            if (cubePrefab == null)
            {
                Debug.LogWarning($"BoardManager: 未能从 {cubePrefabPath} 加载 Cube.prefab（请确认文件存在）");
                return;
            }

            // 这三个预制体都读取 Cube.Prefab（按你的要求）
            tilePrefab = cubePrefab;
            propertyTilePrefab = cubePrefab;
            specialTilePrefab = cubePrefab;
#else
            // 运行时不能直接从 Assets/ 路径加载资源；需要放到 Resources/ 或使用 Addressables。
#endif
        }

        /// <summary>
        /// 初始化默认棋盘（40个格子）
        /// </summary>
        private void InitializeDefaultBoard()
        {
            tiles.Clear();

            // 创建40个格子
            for (int i = 0; i < boardSize; i++)
            {
                GameObject tileObject = null;

                // 如果有预制体，实例化预制体；否则创建空GameObject
                if (tilePrefab != null)
                {
                    tileObject = Instantiate(tilePrefab, transform);
                    tileObject.name = $"Tile_{i}";
                }
                else
                {
                    tileObject = new GameObject($"Tile_{i}");
                    tileObject.transform.SetParent(transform);
                }

                // 设置格子位置
                Vector3 position = CalculateTilePosition(i);
                tileObject.transform.position = position;

                // 根据位置创建不同类型的格子
                Tile tile = CreateTileByIndex(i, tileObject);
                tiles.Add(tile);
            }

            Debug.Log($"棋盘初始化完成，共 {tiles.Count} 个格子");
        }

        /// <summary>
        /// 根据索引创建对应类型的格子
        /// </summary>
        private Tile CreateTileByIndex(int index, GameObject tileObject)
        {
            Tile tile = tileObject.GetComponent<Tile>();

            // 如果预制体已经有Tile组件，直接使用；否则添加组件
            if (tile == null)
            {
                // 特殊格子
                if (index == 0)
                {
                    // 起点
                    SpecialTile specialTile = tileObject.AddComponent<SpecialTile>();
                    specialTile.Initialize(index, "起点", TileType.Start);
                    tile = specialTile;
                }
                else if (index == 10)
                {
                    // 监狱/路过
                    SpecialTile specialTile = tileObject.AddComponent<SpecialTile>();
                    specialTile.Initialize(index, "监狱", TileType.Jail);
                    tile = specialTile;
                }
                else if (index == 20)
                {
                    // 免费停车
                    SpecialTile specialTile = tileObject.AddComponent<SpecialTile>();
                    specialTile.Initialize(index, "免费停车", TileType.FreeParking);
                    tile = specialTile;
                }
                else if (index == 30)
                {
                    // 进监狱
                    SpecialTile specialTile = tileObject.AddComponent<SpecialTile>();
                    specialTile.Initialize(index, "进监狱", TileType.GoToJail);
                    tile = specialTile;
                }
                else
                {
                    // 默认创建地产格子（可以根据需要调整）
                    PropertyTile propertyTile = tileObject.AddComponent<PropertyTile>();
                    int price = CalculatePropertyPrice(index);
                    int rent = CalculateRent(index);
                    propertyTile.Initialize(index, $"地产 {index}", price, rent);
                    tile = propertyTile;
                }
            }
            else
            {
                // 如果预制体已有Tile组件，只需要初始化数据
                if (index == 0)
                {
                    if (tile is SpecialTile specialTile)
                    {
                        specialTile.Initialize(index, "起点", TileType.Start);
                    }
                    else
                    {
                        Destroy(tile);
                        SpecialTile newTile = tileObject.AddComponent<SpecialTile>();
                        newTile.Initialize(index, "起点", TileType.Start);
                        tile = newTile;
                    }
                }
                else if (index == 10)
                {
                    if (tile is SpecialTile specialTile)
                    {
                        specialTile.Initialize(index, "监狱", TileType.Jail);
                    }
                    else
                    {
                        Destroy(tile);
                        SpecialTile newTile = tileObject.AddComponent<SpecialTile>();
                        newTile.Initialize(index, "监狱", TileType.Jail);
                        tile = newTile;
                    }
                }
                else if (index == 20)
                {
                    if (tile is SpecialTile specialTile)
                    {
                        specialTile.Initialize(index, "免费停车", TileType.FreeParking);
                    }
                    else
                    {
                        Destroy(tile);
                        SpecialTile newTile = tileObject.AddComponent<SpecialTile>();
                        newTile.Initialize(index, "免费停车", TileType.FreeParking);
                        tile = newTile;
                    }
                }
                else if (index == 30)
                {
                    if (tile is SpecialTile specialTile)
                    {
                        specialTile.Initialize(index, "进监狱", TileType.GoToJail);
                    }
                    else
                    {
                        Destroy(tile);
                        SpecialTile newTile = tileObject.AddComponent<SpecialTile>();
                        newTile.Initialize(index, "进监狱", TileType.GoToJail);
                        tile = newTile;
                    }
                }
                else
                {
                    if (tile is PropertyTile propertyTile)
                    {
                        int price = CalculatePropertyPrice(index);
                        int rent = CalculateRent(index);
                        propertyTile.Initialize(index, $"地产 {index}", price, rent);
                    }
                    else
                    {
                        // 如果不是PropertyTile，转换为PropertyTile
                        Destroy(tile);
                        PropertyTile newPropertyTile = tileObject.AddComponent<PropertyTile>();
                        int price = CalculatePropertyPrice(index);
                        int rent = CalculateRent(index);
                        newPropertyTile.Initialize(index, $"地产 {index}", price, rent);
                        tile = newPropertyTile;
                    }
                }
            }

            return tile;
        }

        /// <summary>
        /// 计算格子在棋盘上的位置（大富翁地图布局：正方形，40个格子围成一圈）
        /// </summary>
        /// <param name="index">格子索引（0-39）</param>
        /// <returns>格子的世界坐标位置</returns>
        private Vector3 CalculateTilePosition(int index)
        {
            // 大富翁地图布局：
            // - 每条边10个格子
            // - 起点(0)在左下角，逆时针排列
            // - 底边：0-9（从左到右）
            // - 右边：10-19（从下到上）
            // - 顶边：20-29（从右到左）
            // - 左边：30-39（从上到下）

            int tilesPerSide = boardSize / 4; // 每条边的格子数（10）
            int side = index / tilesPerSide; // 哪条边（0=底边，1=右边，2=顶边，3=左边）
            int positionOnSide = index % tilesPerSide; // 在这条边上的位置（0-9）

            float tileSpacing = TileSpacing; // 使用自动计算的格子间距
            float halfSideLength = (tilesPerSide - 1) * tileSpacing * 0.5f; // 半边长
            float x = 0f;
            float z = 0f;

            switch (side)
            {
                case 0: // 底边：从左到右
                    x = -halfSideLength + positionOnSide * tileSpacing;
                    z = -boardRadius;
                    break;

                case 1: // 右边：从下到上
                    x = boardRadius;
                    z = -halfSideLength + positionOnSide * tileSpacing;
                    break;

                case 2: // 顶边：从右到左
                    x = halfSideLength - positionOnSide * tileSpacing;
                    z = boardRadius;
                    break;

                case 3: // 左边：从上到下
                    x = -boardRadius;
                    z = halfSideLength - positionOnSide * tileSpacing;
                    break;
            }

            // 应用棋盘中心偏移
            return boardCenter + new Vector3(x, 0, z);
        }

        /// <summary>
        /// 计算地产价格（根据位置）
        /// </summary>
        private int CalculatePropertyPrice(int index)
        {
            // 简单的价格计算逻辑，可以根据需要调整
            // 这里使用基于索引的简单计算
            return 100 + (index % 10) * 50;
        }

        /// <summary>
        /// 计算租金（根据位置）
        /// </summary>
        private int CalculateRent(int index)
        {
            // 简单的租金计算，通常是价格的10-20%
            int price = CalculatePropertyPrice(index);
            return price / 10;
        }

        /// <summary>
        /// 根据索引获取格子
        /// </summary>
        /// <param name="index">格子索引</param>
        /// <returns>格子对象，如果索引无效返回null</returns>
        public Tile GetTile(int index)
        {
            if (index >= 0 && index < tiles.Count)
            {
                return tiles[index];
            }
            return null;
        }

        /// <summary>
        /// 获取玩家当前位置的格子
        /// </summary>
        /// <param name="player">玩家</param>
        /// <returns>格子对象</returns>
        public Tile GetPlayerTile(Player player)
        {
            if (player != null)
            {
                return GetTile(player.CurrentPosition);
            }
            return null;
        }

        /// <summary>
        /// 计算从当前位置到目标位置的步数（考虑循环）
        /// </summary>
        /// <param name="from">起始位置</param>
        /// <param name="to">目标位置</param>
        /// <returns>步数</returns>
        public int CalculateSteps(int from, int to)
        {
            if (to >= from)
            {
                return to - from;
            }
            else
            {
                // 经过起点
                return (boardSize - from) + to;
            }
        }

        /// <summary>
        /// 设置格子（用于外部配置）
        /// </summary>
        /// <param name="index">格子索引</param>
        /// <param name="tile">格子对象</param>
        public void SetTile(int index, Tile tile)
        {
            if (index >= 0 && index < boardSize)
            {
                if (index < tiles.Count)
                {
                    // 如果已存在，先销毁旧的
                    if (tiles[index] != null)
                    {
                        Destroy(tiles[index].gameObject);
                    }
                    tiles[index] = tile;
                }
                else
                {
                    // 扩展列表
                    while (tiles.Count <= index)
                    {
                        tiles.Add(null);
                    }
                    tiles[index] = tile;
                }
            }
        }


        /// <summary>
        /// 根据棋盘大小，自适应调整摄像头角度，足以看见整个棋盘大小
        /// </summary>
        private void AdjustCamera()
        {
            if (cam == null || boardCenter == null) return;
            float requiredDistance = CalculateRequiredDistance();

            // 设置摄像头位置
            //Vector3 direction = (cam.transform.position - boardCenter).normalized;
            Vector3 direction = Vector3.zero;
            if (direction == Vector3.zero) direction = Vector3.up;

            Vector3 targetPosition = boardCenter + direction * requiredDistance;
            targetPosition.y += verticalOffset; // 添加垂直偏移

            cam.transform.position = targetPosition;
            cam.transform.LookAt(boardCenter);

        }

        private float CalculateRequiredDistance()
        {
            // 根据视野角度计算所需距离
            float halfFOV = cam.fieldOfView * 0.5f * Mathf.Deg2Rad;

            // 考虑棋盘半径和边距
            float requiredDistance = (boardRadius * padding) / Mathf.Tan(halfFOV);

            // 限制距离范围
            //return Mathf.Clamp(requiredDistance, minDistance, maxDistance);
            return requiredDistance;
        }
    }
}
