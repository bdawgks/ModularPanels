using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModularPanels.JsonLib
{
    public class TypedId<IType, OType>(IType id) where OType : class
    {
        readonly IType _id = id;
        OType? _object;

        public OType? Get()
        {
            return _object;
        }

        internal void SetObject(OType? obj)
        {
            _object = obj;
        }
    }

    public class StringId<OType>(string id) : TypedId<string, OType>(id) where OType : class
    {
    }
}
