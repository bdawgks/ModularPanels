using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ModularPanels.JsonLib
{
    public class ObjectBank
    {
        private class TypeBank<T> where T : class
        {
            readonly Dictionary<string, InternalKey<string,T>> _keys = [];

            private InternalKey<string, T> GetInternalKey(string keyStr)
            {
                if (_keys.TryGetValue(keyStr, out var iKey))
                {
                    return iKey;
                }
                InternalKey<string, T> newKey = new(keyStr);
                _keys.Add(keyStr, newKey);
                return newKey;
            }

            public StringKey<T> Get(string keyStr)
            {
                StringKey<T> key = new(keyStr);
                key.SetInternalKey(GetInternalKey(keyStr));
                return key;
            }

            public void RegisterKey(StringKey<T> key)
            {
                key.SetInternalKey(GetInternalKey(key.Key));
            }

            public Dictionary<string, T> GetObjects()
            {
                Dictionary<string, T> map = [];
                foreach (var key in _keys.Values)
                {
                    if (!key.IsNull)
                        map.Add(key.Key, key.Get()!);
                }

                return map;
            }
        }

        readonly Dictionary<Type, object> _allTypes = [];

        private TypeBank<T> GetType<T>() where T : class
        {
            _allTypes.TryGetValue(typeof(T), out var bank);
            if (bank is TypeBank<T> typeBank)
            {
                return typeBank;
            }
            TypeBank<T> newBank = new();
            _allTypes.Add(typeof(T), newBank);
            return newBank;
        }

        /// <summary>
        /// Define the object associated with the given Key.
        /// </summary>
        /// <typeparam name="T">Object type.</typeparam>
        /// <param name="keyStr">String key value.</param>
        /// <param name="obj">Object to assign to Key.</param>
        /// <returns></returns>
        public bool DefineObject<T>(string keyStr, T obj) where T : class
        {
            if (string.IsNullOrEmpty(keyStr))
                return false;

            TypeBank<T> bank = GetType<T>();
            StringKey<T> key = bank.Get(keyStr);
            key.InternalKey?.SetObject(obj);
            return true;
        }

        /// <summary>
        /// Define the object associated with the given Key.
        /// </summary>
        /// <typeparam name="T">Object type.</typeparam>
        /// <param name="key">Key to define.</param>
        /// <param name="obj">Object to assign to the Key.</param>
        /// <returns></returns>
        public bool DefineObject<T>(StringKey<T> key, T obj) where T : class
        {
            return DefineObject(key.Key, obj);
        }

        /// <summary>
        /// Registers the given Key to this bank. This Key will now point to the object with the
        /// associated key value if it is defined.
        /// </summary>
        /// <typeparam name="T">Object type.</typeparam>
        /// <param name="key">The Key to register.</param>
        public void RegisterKey<T>(StringKey<T> key) where T : class
        {
            TypeBank<T> bank = GetType<T>();
            bank.RegisterKey(key);
        }

        /// <summary>
        /// Returns a map of all currently defined objects and their keys.
        /// </summary>
        /// <typeparam name="T">Object type.</typeparam>
        /// <returns>Dictionary with key type and object type.</returns>
        public Dictionary<string, T> GetObjects<T>() where T : class
        {
            TypeBank<T> bank = GetType<T>();
            return bank.GetObjects();
        }

        public bool TryGetObject<T>(string keyStr, [NotNullWhen(true)] out T? obj) where T : class
        {
            obj = null;
            TypeBank<T> bank = GetType<T>();
            StringKey<T> key = bank.Get(keyStr);
            if (key.IsNull)
                return false;

            obj = key.Object!;
            return true;
        }
    }

    /// <summary>
    /// Singleton ObjectBank that persists the whole session and can be accessed globally.
    /// </summary>
    public class GlobalBank : ObjectBank
    {
        static GlobalBank? _instance;

        public static GlobalBank Instance
        {
            get
            {
                _instance ??= new GlobalBank();
                return _instance;
            }
        }
    }
}
