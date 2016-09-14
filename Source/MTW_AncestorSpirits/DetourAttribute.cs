using System;
using System.Reflection;

namespace MTW_AncestorSpirits
{
    // Copied from Hospitality - MTW
    [AttributeUsage(AttributeTargets.Method)]
    internal class DetourAttribute : Attribute
    {
        public Type source;
        public BindingFlags bindingFlags;

        public DetourAttribute(Type source)
        {
            this.source = source;
        }
    }
}
