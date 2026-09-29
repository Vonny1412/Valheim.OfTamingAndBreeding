using UnityEngine_GameObject_Alias = UnityEngine.GameObject;
using ObjectDB_Alias = ObjectDB;

namespace OfTamingAndBreeding.ValheimAPI
{
    public partial class ObjectDB : UnityEngine.MonoBehaviour
    {

        public ObjectDB(ObjectDB_Alias instance) : base(instance)
        {
        }

        public static readonly Core.Invokers.FieldMutateInvoker<System.Collections.Generic.Dictionary<int, UnityEngine_GameObject_Alias>> __IAPI_m_itemByHash_Invoker = new Core.Invokers.FieldMutateInvoker<System.Collections.Generic.Dictionary<int, UnityEngine_GameObject_Alias>>(typeof(ObjectDB_Alias), "m_itemByHash");


    }
}
