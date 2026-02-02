using UnityEngine;

namespace MVC
{
    /// <summary>
    /// MVC 中的 Controller 基类：协调 Model 与 View。
    /// </summary>
    public abstract class BaseController : MonoBehaviour
    {
        protected BaseView View;
        public virtual void BindView(BaseView view)
        {
            View = view;
        }

        /// <summary>
        /// 初始化控制器
        /// </summary>
        public virtual void InitializeController()
        {
        }
    }
}

