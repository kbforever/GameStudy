using UnityEngine;

namespace Singleton
{
    /// <summary>
    /// 泛型单例模式基类，适用于MonoBehaviour
    /// 使用方式：public class YourClass : Singleton<YourClass>
    /// </summary>
    /// <typeparam name="T">继承此类的类型</typeparam>
    public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T instance;
        private static readonly object lockObject = new object();
        private static bool applicationQuitting = false;

        /// <summary>
        /// 单例实例
        /// </summary>
        public static T Instance
        {
            get
            {
                if (applicationQuitting)
                {
                    Debug.LogWarning($"[Singleton] {typeof(T)} 实例已在应用程序退出时被销毁，返回null");
                    return null;
                }

                lock (lockObject)
                {
                    if (instance == null)
                    {
                        // 尝试在场景中查找现有实例
                        instance = FindObjectOfType<T>();

                        if (instance == null)
                        {
                            // 如果没有找到，创建一个新的GameObject
                            GameObject singletonObject = new GameObject();
                            instance = singletonObject.AddComponent<T>();
                            singletonObject.name = $"{typeof(T).Name}(Singleton)";

                            Debug.Log($"[Singleton] 创建了新的 {typeof(T)} 实例");
                        }
                        else
                        {
                            Debug.Log($"[Singleton] 在场景中找到了 {typeof(T)} 实例");
                        }
                    }

                    return instance;
                }
            }
        }

        protected virtual void Awake()
        {
            // 确保只有一个实例
            if (instance == null)
            {
                instance = this as T;
                DontDestroyOnLoad(gameObject);
                OnSingletonAwake();
            }
            else if (instance != this)
            {
                Debug.LogWarning($"[Singleton] 检测到重复的 {typeof(T)} 实例，销毁新实例");
                Destroy(gameObject);
            }
        }

        protected virtual void OnApplicationQuit()
        {
            applicationQuitting = true;
        }

        protected virtual void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }

        /// <summary>
        /// 单例初始化时调用，子类可以重写此方法进行初始化
        /// </summary>
        protected virtual void OnSingletonAwake()
        {
            // 子类可以重写此方法进行特定的初始化操作
        }

        /// <summary>
        /// 检查单例是否存在
        /// </summary>
        public static bool HasInstance => instance != null && !applicationQuitting;
    }
}
