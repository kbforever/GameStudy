using MVC;
using Managers;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UI
{
    /// <summary>
    /// 开始游戏界面的 View：
    /// - 显示标题
    /// - 开始游戏按钮
    /// - 退出游戏按钮
    /// </summary>
    public class StartGameView : BaseView
    {
        [Header("UI 引用")]
        [SerializeField] private Button startButton;
        [SerializeField] private Button quitButton;
        [SerializeField] private GameObject mainMenuPanel;
        [SerializeField] private GameObject gameUIPanel; // 可选：游戏内 UI 面板

        private StartGameController controller;

        private void Start()
        {
            AutoWireUI();
            InitializeView();
        }

        /// <summary>
        /// 自动查找按钮和面板：
        /// - 按名字查找子节点上的 Button（\"start\" / \"quit\"）
        /// - 如果面板为空，则从 Assets/Prefabs/UI 下加载对应 prefab 并实例化
        /// </summary>
        private void AutoWireUI()
        {
            // 查找 start 按钮
            if (startButton == null)
            {
                Transform t = FindChildRecursive(transform, "start");
                if (t != null)
                {
                    startButton = t.GetComponent<Button>();
                }
            }

            // 查找 quit 按钮
            if (quitButton == null)
            {
                Transform t = FindChildRecursive(transform, "quit");
                if (t != null)
                {
                    quitButton = t.GetComponent<Button>();
                }
            }

#if UNITY_EDITOR
            // 自动从 Assets/Prefabs/UI/mainMenuPanel.prefab 实例化主菜单面板
            if (mainMenuPanel == null)
            {
                const string mainMenuPath = "Assets/Prefabs/UI/mainMenuPanel.prefab";
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(mainMenuPath);
                if (prefab != null)
                {
                    mainMenuPanel = Instantiate(prefab);
                    mainMenuPanel.name = prefab.name;
                }
                else
                {
                    Debug.LogWarning($"StartGameView: 未能从 {mainMenuPath} 加载 mainMenuPanel 预制体，请检查路径。");
                }
            }

            // 自动从 Assets/Prefabs/UI/gameUIPanel.prefab 实例化游戏 UI 面板
            if (gameUIPanel == null)
            {
                const string gameUIPath = "Assets/Prefabs/UI/gameUIPanel.prefab";
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(gameUIPath);
                if (prefab != null)
                {
                    gameUIPanel = Instantiate(prefab);
                    gameUIPanel.name = prefab.name;
                    gameUIPanel.SetActive(false); // 初始隐藏，开始游戏后再显示
                }
                else
                {
                    Debug.LogWarning($"StartGameView: 未能从 {gameUIPath} 加载 gameUIPanel 预制体，请检查路径。");
                }
            }
#else
            if (mainMenuPanel == null || gameUIPanel == null)
            {
                Debug.LogWarning("StartGameView: mainMenuPanel 或 gameUIPanel 未设置（运行时无法直接从 Assets 路径加载 prefab，请在 Inspector 里手动指定或改用 Resources）。");
            }
#endif

            // 将面板注册到全局 UIManager 中，方便统一管理
            // UIManager.Instance 会自动创建实例（如果不存在），因为它是 Singleton
            var uiManager = Managers.UIManager.Instance;
            if (uiManager != null)
            {
                uiManager.RegisterMainMenu(mainMenuPanel);
                uiManager.RegisterGameUI(gameUIPanel);
                uiManager.ShowMainMenu();
            }
            else
            {
                Debug.LogWarning("StartGameView: 无法获取 UIManager.Instance，请检查 Singleton 基类是否正确初始化。");
            }
        }

        /// <summary>
        /// 递归查找指定名字的子节点
        /// </summary>
        private Transform FindChildRecursive(Transform parent, string targetName)
        {
            if (parent.name == targetName)
            {
                return parent;
            }

            for (int i = 0; i < parent.childCount; i++)
            {
                Transform child = parent.GetChild(i);
                Transform result = FindChildRecursive(child, targetName);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        public override void InitializeView()
        {
            base.InitializeView();

            controller = GetComponent<StartGameController>();
            if (controller == null)
            {
                controller = gameObject.AddComponent<StartGameController>();
            }

            controller.BindView(this);

            if (startButton != null)
            {
                startButton.onClick.AddListener(controller.OnStartGameClicked);
            }

            if (quitButton != null)
            {
                quitButton.onClick.AddListener(controller.OnQuitGameClicked);
            }
        }

        /// <summary>
        /// 切换到游戏界面（隐藏主菜单，显示游戏 UI）
        /// </summary>
        public void ShowGameUI()
        {
            // 优先交给 UIManager 统一管理
            var uiManager = Managers.UIManager.Instance ?? FindObjectOfType<Managers.UIManager>();
            if (uiManager != null)
            {
                uiManager.ShowGameUI();
                return;
            }

            // 如果没有 UIManager，则退回到本地控制
            if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
            if (gameUIPanel != null) gameUIPanel.SetActive(true);
        }
    }
}

