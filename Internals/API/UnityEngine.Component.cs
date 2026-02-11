using UnityEngine_Component_Alias = UnityEngine.Component;
using UnityEngine_GameObject_Alias = UnityEngine.GameObject;
using UnityEngine_Transform_Alias = UnityEngine.Transform;

namespace OfTamingAndBreeding.Internals.API.UnityEngine
{
    public partial class Component : UnityEngine.Object
    {
        public Component(UnityEngine_Component_Alias instance) : base(instance)
        {
        }

        public UnityEngine_Transform_Alias transform
        {
            get => ((UnityEngine_Component_Alias)__IAPI_instance).transform;
        }
        public UnityEngine_GameObject_Alias gameObject
        {
            get => ((UnityEngine_Component_Alias)__IAPI_instance).gameObject;
        }
        public string tag
        {
            get => ((UnityEngine_Component_Alias)__IAPI_instance).tag;
            set => ((UnityEngine_Component_Alias)__IAPI_instance).tag = value;
        }

        public UnityEngine_Component_Alias GetComponent(System.Type type) => ((UnityEngine_Component_Alias)__IAPI_instance).GetComponent(type);
        public static readonly Core.Invokers.VoidMethodInvoker __IAPI_GetComponentFastPath_Invoker1 = new Core.Invokers.VoidMethodInvoker(typeof(UnityEngine_Component_Alias), "GetComponentFastPath", new Core.Signatures.ParamSig[] { new Core.Signatures.NonGenericParamSig(typeof(System.Type), false), new Core.Signatures.NonGenericParamSig(typeof(System.IntPtr), false) });
        public void GetComponentFastPath(System.Type type, System.IntPtr oneFurtherThanResultValue) => __IAPI_GetComponentFastPath_Invoker1.Invoke(((UnityEngine_Component_Alias)__IAPI_instance), new object[] { type, oneFurtherThanResultValue });
        public T GetComponent<T>() => ((UnityEngine_Component_Alias)__IAPI_instance).GetComponent<T>();
        public bool TryGetComponent(System.Type type, out UnityEngine_Component_Alias component) => ((UnityEngine_Component_Alias)__IAPI_instance).TryGetComponent(type, out component);
        public bool TryGetComponent<T>(out T component) => ((UnityEngine_Component_Alias)__IAPI_instance).TryGetComponent<T>(out component);
        public UnityEngine_Component_Alias GetComponent(string type) => ((UnityEngine_Component_Alias)__IAPI_instance).GetComponent(type);
        public UnityEngine_Component_Alias GetComponentInChildren(System.Type t, bool includeInactive) => ((UnityEngine_Component_Alias)__IAPI_instance).GetComponentInChildren(t, includeInactive);
        public UnityEngine_Component_Alias GetComponentInChildren(System.Type t) => ((UnityEngine_Component_Alias)__IAPI_instance).GetComponentInChildren(t);
        public T GetComponentInChildren<T>(bool includeInactive) => ((UnityEngine_Component_Alias)__IAPI_instance).GetComponentInChildren<T>(includeInactive);
        public T GetComponentInChildren<T>() => ((UnityEngine_Component_Alias)__IAPI_instance).GetComponentInChildren<T>();
        public UnityEngine_Component_Alias[] GetComponentsInChildren(System.Type t, bool includeInactive) => ((UnityEngine_Component_Alias)__IAPI_instance).GetComponentsInChildren(t, includeInactive);
        public UnityEngine_Component_Alias[] GetComponentsInChildren(System.Type t) => ((UnityEngine_Component_Alias)__IAPI_instance).GetComponentsInChildren(t);
        public T[] GetComponentsInChildren<T>(bool includeInactive) => ((UnityEngine_Component_Alias)__IAPI_instance).GetComponentsInChildren<T>(includeInactive);
        public void GetComponentsInChildren<T>(bool includeInactive, System.Collections.Generic.List<T> result) => ((UnityEngine_Component_Alias)__IAPI_instance).GetComponentsInChildren<T>(includeInactive, result);
        public T[] GetComponentsInChildren<T>() => ((UnityEngine_Component_Alias)__IAPI_instance).GetComponentsInChildren<T>();
        public void GetComponentsInChildren<T>(System.Collections.Generic.List<T> results) => ((UnityEngine_Component_Alias)__IAPI_instance).GetComponentsInChildren<T>(results);
        public UnityEngine_Component_Alias GetComponentInParent(System.Type t, bool includeInactive) => ((UnityEngine_Component_Alias)__IAPI_instance).GetComponentInParent(t, includeInactive);
        public UnityEngine_Component_Alias GetComponentInParent(System.Type t) => ((UnityEngine_Component_Alias)__IAPI_instance).GetComponentInParent(t);
        public T GetComponentInParent<T>(bool includeInactive) => ((UnityEngine_Component_Alias)__IAPI_instance).GetComponentInParent<T>(includeInactive);
        public T GetComponentInParent<T>() => ((UnityEngine_Component_Alias)__IAPI_instance).GetComponentInParent<T>();
        public UnityEngine_Component_Alias[] GetComponentsInParent(System.Type t, bool includeInactive) => ((UnityEngine_Component_Alias)__IAPI_instance).GetComponentsInParent(t, includeInactive);
        public UnityEngine_Component_Alias[] GetComponentsInParent(System.Type t) => ((UnityEngine_Component_Alias)__IAPI_instance).GetComponentsInParent(t);
        public T[] GetComponentsInParent<T>(bool includeInactive) => ((UnityEngine_Component_Alias)__IAPI_instance).GetComponentsInParent<T>(includeInactive);
        public void GetComponentsInParent<T>(bool includeInactive, System.Collections.Generic.List<T> results) => ((UnityEngine_Component_Alias)__IAPI_instance).GetComponentsInParent<T>(includeInactive, results);
        public T[] GetComponentsInParent<T>() => ((UnityEngine_Component_Alias)__IAPI_instance).GetComponentsInParent<T>();
        public UnityEngine_Component_Alias[] GetComponents(System.Type type) => ((UnityEngine_Component_Alias)__IAPI_instance).GetComponents(type);
        public static readonly Core.Invokers.VoidMethodInvoker __IAPI_GetComponentsForListInternal_Invoker1 = new Core.Invokers.VoidMethodInvoker(typeof(UnityEngine_Component_Alias), "GetComponentsForListInternal", new Core.Signatures.ParamSig[] { new Core.Signatures.NonGenericParamSig(typeof(System.Type), false), new Core.Signatures.NonGenericParamSig(typeof(object), false) });
        public void GetComponentsForListInternal(System.Type searchType, object resultList) => __IAPI_GetComponentsForListInternal_Invoker1.Invoke(((UnityEngine_Component_Alias)__IAPI_instance), new object[] { searchType, resultList });
        public void GetComponents(System.Type type, System.Collections.Generic.List<UnityEngine_Component_Alias> results) => ((UnityEngine_Component_Alias)__IAPI_instance).GetComponents(type, results);
        public void GetComponents<T>(System.Collections.Generic.List<T> results) => ((UnityEngine_Component_Alias)__IAPI_instance).GetComponents<T>(results);
        public T[] GetComponents<T>() => ((UnityEngine_Component_Alias)__IAPI_instance).GetComponents<T>();
        public int GetComponentIndex() => ((UnityEngine_Component_Alias)__IAPI_instance).GetComponentIndex();




    }
}
