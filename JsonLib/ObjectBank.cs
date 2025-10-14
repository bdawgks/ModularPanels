using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModularPanels.JsonLib
{
    public class ObjectBank
    {
        private class TypeBank<T> where T : class
        {
            readonly Dictionary<string, StringId<T>> _ids = [];

            public StringId<T> Get(string idStr)
            {
                if (_ids.TryGetValue(idStr, out var id))
                {
                    return id;
                }
                StringId<T> newId = new(idStr);
                _ids.Add(idStr, newId);
                return newId;
            }

            /// <summary>
            /// When passed a reference to a StringId, it will either add it to the bank or
            /// reassign the ref to the existing StringId in the bank with matching ID.
            /// </summary>
            /// <param name="id">Id reference</param>
            /// <returns>true if the id was newly added, false the ref was modified to an existing id</returns>
            public bool GetOrAddId(ref StringId<T> id)
            {
                if (_ids.TryGetValue(id.Id, out var storedId))
                {
                    id = storedId;
                    return false;
                }
                _ids.Add(id.Id, id);
                return true;
            }

            public Dictionary<string, T> GetObjects()
            {
                Dictionary<string, T> map = [];
                foreach (var id in _ids.Values)
                {
                    if (!id.IsNull)
                        map.Add(id.Id, id.Get()!);
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
        /// Define the object associated with the given Id.
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="idStr">String of id.</param>
        /// <param name="obj">Object to assign to Id</param>
        /// <returns></returns>
        public bool DefineObject<T>(string idStr, T obj) where T : class
        {
            if (string.IsNullOrEmpty(idStr))
                return false;

            TypeBank<T> bank = GetType<T>();
            StringId<T> id = bank.Get(idStr);
            id.SetObject(obj);
            return true;
        }

        public bool DefineObject<T>(StringId<T> id, T obj) where T : class
        {
            return DefineObject(id.Id, obj);
        }

        /// <summary>
        /// When passed a reference to a StringId, it will either add it to the bank or
        /// reassign the ref to the existing StringId in the bank with matching ID.
        /// </summary>
        /// <param name="id">Id reference</param>
        /// <returns>true if the id was newly added, false the ref was modified to an existing id</returns>
        public bool AssignId<T>(ref StringId<T> id) where T : class
        {
            TypeBank<T> bank = GetType<T>();
            return bank.GetOrAddId(ref id);
        }

        public Dictionary<string, T> GetObjects<T>() where T : class
        {
            TypeBank<T> bank = GetType<T>();
            return bank.GetObjects();
        }
    }

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
