using UnityEngine;

namespace Remoria.Core
{
    /// <summary>
    /// Generic singleton base class for MonoBehaviours.
    /// Inherit from this to create a manager that has exactly one instance.
    /// 
    /// Usage:
    ///   public class GameManager : Singleton<GameManager> { }
    ///   // Then access from anywhere: GameManager.Instance.DoSomething();
    /// 
    /// How it works:
    ///   - When you call .Instance, it finds or creates the singleton.
    ///   - If a duplicate is created (e.g. loading a new scene), the duplicate destroys itself.
    ///   - DontDestroyOnLoad keeps it alive across scene changes.
    /// </summary>
    public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        // The single instance. Static means it's shared across ALL objects of type T.
        private static T _instance;

        // A lock object to prevent race conditions (two things trying to create at once).
        private static readonly object _lock = new object();

        // Whether the application is quitting. Prevents creating new singletons during shutdown.
        private static bool _applicationIsQuitting = false;

        /// <summary>
        /// Access the singleton instance. Creates one if it doesn't exist yet.
        /// </summary>
        public static T Instance
        {
            get
            {
                // Don't create new instances while the game is shutting down.
                if (_applicationIsQuitting)
                {
                    // Silently return null during shutdown — this is expected behavior.
                    return null;
                }

                lock (_lock)
                {
                    if (_instance == null)
                    {
                        // Try to find an existing instance in the scene.
                        _instance = FindObjectOfType<T>();

                        if (_instance == null)
                        {
                            // None found — create a new GameObject with this component.
                            var singletonObject = new GameObject($"[Singleton] {typeof(T).Name}");
                            _instance = singletonObject.AddComponent<T>();
                            DontDestroyOnLoad(singletonObject);
                        }
                    }

                    return _instance;
                }
            }
        }

        /// <summary>
        /// Called by Unity when this object is created.
        /// If another instance already exists, this one destroys itself.
        /// </summary>
        protected virtual void Awake()
        {
            if (_instance == null)
            {
                _instance = this as T;
                DontDestroyOnLoad(gameObject);
            }
            else if (_instance != this)
            {
                // A duplicate was created — destroy it immediately.
                Debug.LogWarning($"[Singleton] Duplicate instance of {typeof(T).Name} destroyed.");
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Called when the application is quitting. Prevents ghost singletons.
        /// </summary>
        protected virtual void OnApplicationQuit()
        {
            _applicationIsQuitting = true;
        }
    }
}
