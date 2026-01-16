
using System;
using System.Reflection;

namespace OfTamingAndBreeding.Internals.API.Core.Signatures
{
    public abstract class ParamSig
    {
        public bool IsGeneric;
        public int GenericIndex; // only if IsGeneric
        public Type ConcreteType; // only if !IsGeneric
        public bool IsByRef;
        public abstract bool Matches(ParameterInfo p);
    }
}
