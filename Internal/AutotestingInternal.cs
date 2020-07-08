using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using NLua;
using UnityEngine;
using SimpleHttpServer;
using SimpleHttpServer.Models;
using Debug = UnityEngine.Debug;

namespace Autotest.Internal
{
 
    public static class AutotestingInternal
    {
	    
	    public static event Action onRun;

        private static HttpServer s_httpServer = null;
        private static Lua s_state = null;
		private static Thread s_runningThread = null;
        
        internal static UnityBinding unityBinding = null;
        internal static ConcurrentDictionary<string, byte> pendingEvents = null;
        internal static ConcurrentDictionary<string, byte> eventsAsError = null;
        
        public static void Initialize()
        {
	        pendingEvents = new ConcurrentDictionary<string, byte>();
	        eventsAsError = new ConcurrentDictionary<string, byte>();
	        unityBinding = new GameObject("AutotestUnityBinding").AddComponent<UnityBinding>();

            List<Route> routes = new List<Route>();
			routes.Add(new Route("Run script", "POST", "run", RunScript));

            s_httpServer = new HttpServer(Settings.port, routes);
            s_httpServer.ListenAsync();
        }
        
        private static HttpResponse RunScript(HttpRequest request)
		{
			string lua = request.Content;

			try
			{
				s_state = new Lua();
				LoadCLRAndFunctions(s_state);
				s_state.DoString(lua, "script");
				LuaFunction runFunction = s_state.GetFunction("run");
				if (runFunction != null)
				{
					onRun?.Invoke();
					
					s_runningThread = new Thread(RunScript);
					s_runningThread.Start(runFunction);
				}
			}
			catch (Exception e)
			{
				Debug.LogError(e.ToString());
				HttpResponse response = HttpBuilder.InternalServerError();
				response.ContentAsUTF8 = e.ToString();
				return response;
			}

			return HttpBuilder.OK();
		}

        private static void LoadCLRAndFunctions(Lua state)
		{
			s_state.LoadCLRPackage();

			MethodInfo[] methods = typeof(ScriptFunctions).GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			foreach (MethodInfo method in methods)
				s_state.RegisterFunction(method.Name, null, method);

			s_state.DoString(LuaUtils.dump, "debug");
		}

		private static void RunScript(object data)
		{
			LuaFunction runFunction = (LuaFunction) data;
			runFunction.Call();
		}
		
		public static void Event(string name)
		{
			pendingEvents.TryRemove(name, out byte value);


			if (eventsAsError.ContainsKey(name) == true)
			{
				OperationCanceledException exception = new OperationCanceledException($"Event '{name}' received and processed as an error");
				Debug.LogError(exception);
				s_runningThread.Abort(exception);
			}
		}

    }
    
}
