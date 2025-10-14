using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModularPanels.JsonLib
{
    public class TypedId<IType, OType> where OType : class
    {
        readonly IType _id;
        OType? _object;

        public TypedId(IType id)
        {
            _id = id;
        }

        public OType? Get()
        {
            return _object;
        }
    }
}
