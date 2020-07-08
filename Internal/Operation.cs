using System;
using System.Reflection;

namespace Autotest
{
    
    internal class Operation
    {
        public MethodInfo methodInfo = null;
        public object target = null;
        public bool status = false;
        public Exception exception = null;
        public object result = null;
    }
    
}
