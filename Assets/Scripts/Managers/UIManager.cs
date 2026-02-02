using UnityEngine;
using Singleton;
using System.IO;
using UnityEditor;
using System;
using System.Reflection;
using System.Collections.Generic;
using static UnityEngine.Rendering.DebugUI;
using UI;

namespace Managers
{
    /// <summary>
    /// 全局 UI 管理器：
    /// - 统一管理各个 UI 面板（主菜单、游戏内 UI 等）
    /// - 提供显示/隐藏界面的公共方法
    /// </summary>
    public class UIManager : Singleton<UIManager>
    {
        [Header("Canvas 引用")]
        [SerializeField] private Canvas mainCanvas;

        [Header("UI 面板引用")]
        [SerializeField] private GameObject mainMenuPanel;
        [SerializeField] private GameObject gameUIPanel;
        [SerializeField] private GameObject playerSelectUI;

        Dictionary<string, GameObject> UIDict;

        protected override void OnSingletonAwake()
        {
            base.OnSingletonAwake();
            EnsureCanvasExists();
            UIDict = new Dictionary<string, GameObject>();
        }

        /// <summary>
        /// 确保场景中有一个 Canvas，并缓存引用
        /// </summary>
        private void EnsureCanvasExists()
        {
            if (mainCanvas != null) return;

            mainCanvas = FindObjectOfType<Canvas>();
            if (mainCanvas == null)
            {
                GameObject canvasObj = new GameObject("MainCanvas");
                mainCanvas = canvasObj.AddComponent<Canvas>();
                mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
                canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            }
        }

        /// <summary>
        /// 将指定 panel 放到 Canvas 下面
        /// </summary>
        private void AttachToCanvas(GameObject panel)
        {
            if (panel == null) return;
            EnsureCanvasExists();
            if (mainCanvas == null) return;

            panel.transform.SetParent(mainCanvas.transform, worldPositionStays: false);
        }

        /// <summary>
        /// 注册主菜单面板（通常由 StartGameView 调用）
        /// </summary>
        public void RegisterMainMenu(GameObject panel)
        {
            if (panel == null) return;
            AttachToCanvas(panel);
            mainMenuPanel = panel;
            UIDict.Add(ViewType.mainMenuPanel.ToString(), panel);
        }


        public GameObject RegisterGameUI<T>(string UIName) where T: MonoBehaviour
        {
            
            string UIPath = $"Assets/Prefabs/UI/{UIName}.prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(UIPath);

            if (prefab != null)
            {
                GameObject obj = Instantiate(prefab);
                obj.name = prefab.name;
                

                obj.AddComponent<T>();

                AttachToCanvas(obj);
                UIDict.Add(obj.name, obj);
                return obj;
            }
            else
            {
                Debug.LogWarning($"未能从 {UIPath} 加载预制体，请检查路径。");
                return null;
            }
            
        }


        /// <summary>
        /// 注册游戏内 UI 面板
        /// </summary>
        public void RegisterGameUI(GameObject panel)
        {
            if (panel == null) return;
            AttachToCanvas(panel);
            gameUIPanel = panel;
            UIDict.Add(ViewType.gameUIPanel.ToString(), panel);
        }

        /// <summary>
        /// 显示主菜单、隐藏游戏 UI
        /// </summary>
        public void ShowMainMenu()
        {
            if (mainMenuPanel != null)
            {
                mainMenuPanel.SetActive(true);
            }

            if (gameUIPanel != null)
            {
                gameUIPanel.SetActive(false);
            }
        }

        /// <summary>
        /// 显示游戏 UI
        /// </summary>
        public void ShowGameUI(string UIName)
        {
            if (UIDict.ContainsKey(UIName)) UIDict[UIName].SetActive(true);
        }

        /// <summary>
        /// 隐藏游戏UI
        /// </summary>
        /// <param name="UIName"></param>
        public void HideGameUI(string UIName)
        {
            if (UIDict.ContainsKey(UIName)) UIDict[UIName].SetActive(false);
        }

        /// <summary>
        /// 隐藏所有已注册的 UI 面板
        /// </summary>
        public void HideAll()
        {
            foreach(var obj in UIDict.Values)
            {
                obj.SetActive(false);
            }
        }
    }
}

