using System.Collections.Generic;
using System.Dynamic;

namespace MicroRazorHost
{
    public class ModelContainer : DynamicObject
    {
        private readonly IDictionary<string, object> _hash;

        public ModelContainer(IDictionary<string, object> hash)
        {
            _hash = hash;
        }

        public override IEnumerable<string> GetDynamicMemberNames()
        {
            return _hash.Keys;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            return _hash.TryGetValue(binder.Name, out result);
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            object original;
            if (_hash.TryGetValue(binder.Name, out original))
            {
                _hash[binder.Name] = value;
                return true;
            }
            return false;
        }
    }
}