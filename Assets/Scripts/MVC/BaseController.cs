using UnityEngine;

namespace MVC
{
    /// <summary>
    /// MVC 中的 Controller 基类：协调 Model 与 View。
    /// </summary>
    public abstract class BaseController : MonoBehaviour
    {
        /// <summary>
        /// 初始化控制器
        /// </summary>
        public virtual void InitializeController()
        {
        }
    }
}

