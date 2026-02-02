using MVC;
using Managers;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UI
{
    /// <summary>
    /// 开始游戏界面的 Controller：
    /// - 响应按钮点击
    /// - 调用 GameManager 初始化游戏
    /// </summary>
    [RequireComponent(typeof(StartGameView))]
    public class StartGameController : BaseController
    {

        public override void BindView(BaseView view)
        {
            base.BindView(view);
        }

        public override void InitializeController()
        {
            base.InitializeController();
        }

        public void OnStartGameClicked()
        {
            Debug.Log("StartGameController: 开始游戏按钮被点击");

            // 确保有 GameManager 单例
            GameCoreManager gm = GameCoreManager.Instance;
            if (gm == null)
            {
                GameObject gmObject = new GameObject("GameManager");
                gm = gmObject.AddComponent<GameCoreManager>();
            }

            // 初始化游戏（如已初始化，可以视需求略过；此处简单直接调用）
            gm.InitializeGame();

            // 切换 UI
            if (View != null)
            {
                UIManager.Instance.HideGameUI(ViewType.mainMenuPanel.ToString());
            }
        }

        public void OnQuitGameClicked()
        {
            Debug.Log("StartGameController: 退出游戏按钮被点击");

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}

