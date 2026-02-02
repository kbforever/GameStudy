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

            //// 如果没有 UIManager，则退回到本地控制
            //if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
            //if (gameUIPanel != null) gameUIPanel.SetActive(true);
        }
    }
}

