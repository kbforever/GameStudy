using UnityEngine;

namespace MVC
{
    /// <summary>
    /// MVC 中的 Model 基类：只负责数据，不直接依赖具体 View。
    /// </summary>
    public abstract class BaseModel : ScriptableObject
    {
        // 预留：通用数据逻辑（存档、重置等）可以在这里扩展
    }
}

