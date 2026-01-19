using UnityEngine;

namespace MVC
{
    /// <summary>
    /// MVC 中的 View 基类：负责展示与输入，不包含具体游戏规则。
    /// </summary>
    public abstract class BaseView : MonoBehaviour
    {
        /// <summary>
        /// 初始化视图，通常在 Awake/Start 时由外部调用。
        /// </summary>
        public virtual void InitializeView()
        {
        }
    }
}

