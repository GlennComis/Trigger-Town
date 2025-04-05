using UnityEngine;


    public abstract class SingletonMonoBehaviour<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T instance;

        public static T Instance => instance;

        /// <summary>
        /// We use a bool instead of a null check because a nullcheck is expensive
        /// </summary>
        public static bool instance_exists = false;

        protected virtual void Awake()
        {
            if (instance == null)
                SetSingletonInstance();
        }

        /// <summary>
        /// Sets the instance, gets called in awake unless we want to call it at another moment
        /// </summary>
        protected virtual void SetSingletonInstance()
        {
            instance = this as T;
            instance_exists = instance != null;
        }


        protected virtual void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }

            instance_exists = instance != null;
        }
    }

    


