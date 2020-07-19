using System;

namespace Autotest.Internal
{
    
    public class ScriptException : Exception
    {
        public readonly string type;
        public readonly string message;

        public ScriptException(string type, string message) : base($"{type}: {message}")
        {
            this.type = type;
            this.message = message;
        }
    }
    
}


