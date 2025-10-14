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
        }

        Dictionary<Type, object> _allTypes = [];

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

        public bool DefineObject<T>(string idStr, T obj) where T : class
        {
            if (string.IsNullOrEmpty(idStr))
                return false;

            TypeBank<T> bank = GetType<T>();
            StringId<T> id = bank.Get(idStr);
            id.SetObject(obj);
            return true;
        }
    }
}
