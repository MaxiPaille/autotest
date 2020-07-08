
using System;
using System.Diagnostics;

#if AUTOTEST
using Autotest.Internal;
#endif

namespace Autotest
{
 
    public static class Autotesting
    {
	    
        [Conditional("AUTOTEST")]
        public static void Initialize()
        {
#if AUTOTEST
			AutotestingInternal.Initialize();
#endif
        }
        
		[Conditional("AUTOTEST")]
		public static void Event(string name)
		{
#if AUTOTEST
			AutotestingInternal.Event(name);
#endif
		}

		[Conditional("AUTOTEST")]
		public static void RegisterOnRun(Action callback)
		{
#if AUTOTEST
			AutotestingInternal.onRun += callback;
#endif
		}

    }
    
}
