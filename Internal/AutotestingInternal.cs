using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using NLua;
using UnityEngine;
using SimpleHttpServer;
using SimpleHttpServer.Models;

namespace Autotest.Internal
{
 
    public static class AutotestingInternal
    {
	    
	    private static HttpServer s_httpServer = null;
	    
	    internal static UnityBinding unityBinding = null;
	    internal static Action restartApp;
	    internal static Action<string> log = null;
	    private static Action<string, Exception> s_onScriptError = null;
	    
	    internal static Script currentScript = null;
	    
        private static Lua s_state = null;
        private static Thread s_callThread = null;
        
        internal static ConcurrentDictionary<string, byte> pendingEvents = null;
        internal static ConcurrentDictionary<string, byte> eventsAsError = null;


        
        public static void Initialize(Action<string> onLog, Action onRestartApp, Action<string, Exception> onScriptError)
        {
	        if (s_httpServer != null)
		        return;

	        restartApp = onRestartApp;
	        log = onLog;
	        s_onScriptError = onScriptError;
	        unityBinding = new GameObject("AutotestUnityBinding").AddComponent<UnityBinding>();
	        
	        pendingEvents = new ConcurrentDictionary<string, byte>();
	        eventsAsError = new ConcurrentDictionary<string, byte>();
	        
            List<Route> routes = new List<Route>();
			routes.Add(new Route("Run scripts", "POST", "run", RunScripts));

            s_httpServer = new HttpServer(Settings.port, routes);
            s_httpServer.ListenAsync();
        }
        
        private static HttpResponse RunScripts(HttpRequest request)
		{
			string content = request.Content;
			bool error = false;
			
			Script[] scripts = ExtractScripts(content);
			foreach (Script script in scripts)
			{
				RunScript(script);
				error = error || script.exception != null;
			}

			HttpResponse response = null;
			if (error == true)
				response = HttpBuilder.ExpectationFailed();
			else
				response = HttpBuilder.OK();
			
			return response;
		}

        private static Script[] ExtractScripts(string content)
        {
	        List<Script> scripts = new List<Script>();
	        Script currentScript = null;
	        
	        StringReader reader = new StringReader(content);
	        string line = null;
	        while ((line = reader.ReadLine()) != null)
	        {
		        if (line.StartsWith("LUA ") == true && line.EndsWith(":") == true)
		        {
			        currentScript = new Script();
			        currentScript.name = line.Replace("LUA ", "").Replace(":", "");
			        scripts.Add(currentScript);
		        }
		        else if (currentScript != null)
		        {
			        currentScript.lua += line;
			        currentScript.lua += Environment.NewLine;
		        }
		        else
		        {
			        throw new FormatException();
		        }
	        }

	        return scripts.ToArray();
        }

        private static void RunScript(Script script)
        {
	        currentScript = script;
	        s_state = new Lua();

	        try
	        {
		        LoadCLRAndFunctions(s_state);
		        s_state.DoString(script.lua, script.name);
		        LuaFunction runFunction = s_state.GetFunction("run");
					
		        if (runFunction != null)
		        {
			        s_callThread = new Thread(() =>
			        {
				        try
				        {
					        runFunction.Call();
				        }
				        catch (Exception e)
				        {
					        if (currentScript.exception == null)
						        currentScript.exception = e.InnerException ?? e;
					        s_onScriptError?.Invoke(script.name, currentScript.exception);
				        }
			        });
					
			        s_callThread.Start();
			        s_callThread.Join();
		        }
	        }
	        catch (Exception e)
	        {
		        if (currentScript.exception == null)
			        currentScript.exception = e.InnerException ?? e;
		        s_onScriptError?.Invoke(script.name, currentScript.exception);
	        }
				
	        currentScript.completed = true;
	        currentScript = null;
	        s_callThread = null;
				
	        pendingEvents.Clear();
	        eventsAsError.Clear();
        }
        
        private static void LoadCLRAndFunctions(Lua state)
		{
			s_state.LoadCLRPackage();

			MethodInfo[] methods = typeof(ScriptFunctions).GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			foreach (MethodInfo method in methods)
				s_state.RegisterFunction(method.Name, null, method);

			s_state.DoString(LuaUtils.dump, "debug");
		}

        public static void Event(string name)
        {
	        if (s_callThread == null)
		        return;
	        
			pendingEvents.TryRemove(name, out byte value);
			
			if (eventsAsError.ContainsKey(name) == true)
			{
				currentScript.exception = new OperationCanceledException($"Event '{name}' received and processed as an error");;
				s_callThread.Interrupt();
			}
		}

    }
    
}
