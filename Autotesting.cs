
using System;
using System.Diagnostics;

#if AUTOTEST
using Autotest.Internal;
#endif

namespace Autotest
{
 
    public static class Autotesting
    {
	    
	    /// <summary>
	    /// Initialize the Autotest module when it's available
	    /// </summary>
	    /// <param name="restartApp">Function called when a script ask for an app restart</param>
        [Conditional("AUTOTEST")]
        public static void Initialize(Action<string> log, Action restartApp, Action<string, Exception> onScriptError)
        {
#if AUTOTEST
			AutotestingInternal.Initialize(log, restartApp, onScriptError);
#endif
        }
        
	    /// <summary>
	    /// Send an events to the current running script
	    /// </summary>
	    /// <param name="name">Name of the event</param>
		[Conditional("AUTOTEST")]
		public static void Event(string name)
		{
#if AUTOTEST
			AutotestingInternal.Event(name);
#endif
		}

    }
    
}
