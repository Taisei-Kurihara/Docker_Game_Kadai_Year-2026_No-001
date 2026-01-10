using UnityEngine;


namespace Common
{
    /// <summary>
    /// MonoBehaviour用シングルトン基底クラス.
    /// </summary>
    public class Singleton_MonoBehaviourBase<T> : MonoBehaviour where T : Singleton_MonoBehaviourBase<T>
    {
        protected static T instance;

        /// <summary>
        /// 本体の取得.
        /// </summary>
        /// <returns></returns>
        public static T Instance()
        {
            if (instance == null)
            {
                var gameObject = new GameObject(typeof(T).Name);
                instance = gameObject.AddComponent<T>();
                DontDestroyOnLoad(gameObject);
            }
            return instance;
        }
    }
}
