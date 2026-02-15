using System;
using System.Linq;
using System.Reflection;

namespace OfTamingAndBreeding.ValheimAPI.LowLevel.Core.Signatures
{
    public class GenericContainerParamSig : ParamSig
    {
        private readonly Type _genericDef;

        public GenericContainerParamSig(Type genericDef, bool isByRef)
        {
            _genericDef = genericDef;
            IsByRef = isByRef;
        }

        public override bool Matches(ParameterInfo p)
        {
            var pt = p.ParameterType;
            if (pt.IsByRef != IsByRef)
                return false;

            if (!pt.IsGenericType)
                return false;

            if (pt.GetGenericTypeDefinition() != _genericDef)
                return false;

            // Argumente sind egal, solange sie GenericParameter sind
            return pt.GetGenericArguments().All(a => a.IsGenericParameter);
        }
    }
}
