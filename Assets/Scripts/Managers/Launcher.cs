using System.Collections;
using System.Collections.Generic;
using UI;
using UnityEditor;
using UnityEngine;

public class Launcher : MonoBehaviour
{
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject gameUIPanel; // 可选：游戏内 UI 面板
    // Start is called before the first frame update
    void Start()
    {
        InitView();
    }


    void InitView()
    {
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
                mainMenuPanel.AddComponent<StartGameView>();
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
}
