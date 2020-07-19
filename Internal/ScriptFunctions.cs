using System;
using System.Threading;
using NLua;
using UnityEngine;
using UnityEngine.UI;

#pragma warning disable 0169

namespace Autotest.Internal
{
	
	internal static class ScriptFunctions
	{
		
		private static void WaitForEnable(GameObject gameObject, float timeout = 0)
		{
			DateTime timeoutLimit = DateTime.UtcNow.AddSeconds(timeout);
			while (true)
			{
				if (timeout > 0 && DateTime.UtcNow > timeoutLimit)
					throw new ScriptException("WaitForEnable", $"Timeout {timeout}s");

				bool result = AutotestingInternal.unityBinding.ExecuteOnMainThreadAndWaitForCompletion(() => gameObject.activeInHierarchy);

				if (result == true)
					break;
			}
		}

		private static void WaitForDisable(GameObject gameObject, float timeout = 0)
		{
			DateTime timeoutLimit = DateTime.UtcNow.AddSeconds(timeout);
			while (true)
			{
				if (timeout > 0 && DateTime.UtcNow > timeoutLimit)
					throw new ScriptException("WaitForDisable", $"Timeout {timeout}s");
				
				bool result = AutotestingInternal.unityBinding.ExecuteOnMainThreadAndWaitForCompletion(() => gameObject.activeInHierarchy);

				if (result == false)
					break;
			}
		}
		
		private static void WaitForEvent(string name, float timeout = 0)
		{
			DateTime timeoutLimit = DateTime.UtcNow.AddSeconds(timeout);
			AutotestingInternal.pendingEvents.TryAdd(name, 0);

			while (AutotestingInternal.pendingEvents.ContainsKey(name) == true)
			{
				if (timeout > 0 && DateTime.UtcNow > timeoutLimit)
					throw new ScriptException("WaitForEvent", $"Timeout {timeout}s");
				
				Thread.Sleep(UnityBinding.frameDuration);
			}
		}
		
		private static void RegisterEventAsError(string name)
		{
			AutotestingInternal.eventsAsError.TryAdd(name, 0);
		}

		private static void UnregisterEventAsError(string name)
		{
			AutotestingInternal.eventsAsError.TryRemove(name, out byte value);
		}

		private static void Wait(float duration)
		{
			DateTime timeoutLimit = DateTime.UtcNow.AddSeconds(duration);

			while (DateTime.UtcNow < timeoutLimit)
				Thread.Sleep(UnityBinding.frameDuration);
		}

		private static void CheckForEnable(GameObject gameObject)
		{
			bool result = AutotestingInternal.unityBinding.ExecuteOnMainThreadAndWaitForCompletion(() => gameObject.activeInHierarchy == true);

			if (result == false)
				throw new ScriptException("CheckForEnable", $"GameObject '{AutotestingInternal.unityBinding.GetGameObjectName(gameObject)}' not enabled");
		}
		
		private static void CheckForDisable(GameObject gameObject)
		{
			bool result = AutotestingInternal.unityBinding.ExecuteOnMainThreadAndWaitForCompletion(() => gameObject.activeInHierarchy == false);

			if (result == false)
				throw new ScriptException("CheckForDisable", $"GameObject '{AutotestingInternal.unityBinding.GetGameObjectName(gameObject)}' not disabled");
		}

		private static GameObject SearchForGameObject(string pattern)
		{
			return AutotestingInternal.unityBinding.ExecuteOnMainThreadAndWaitForCompletion(() => AutotestingInternal.unityBinding.FindGameObject(pattern));
		}

		private static GameObject GetGameObjectFromPath(string path)
		{
			GameObject result  = AutotestingInternal.unityBinding.ExecuteOnMainThreadAndWaitForCompletion(() => AutotestingInternal.unityBinding.GetGameObjectFromPath(path));

			if (result == null)
				throw new ScriptException("GetGameObjectFromPath", $"GameObject \"{path}\" does not exists");
			
			return result;
		}
		
		private static Component GetComponent(GameObject gameObject, ProxyType componentType)
		{
			Component result = AutotestingInternal.unityBinding.ExecuteOnMainThreadAndWaitForCompletion(() => gameObject.GetComponent(componentType.UnderlyingSystemType));

			if (result == null)
				throw new ScriptException("GetComponent", $"Component \"{componentType.UnderlyingSystemType.Name}\" not found on GameObject \"{gameObject.name}\"");
			
			return result;
		}
		
		private static void Log(string message)
		{
			AutotestingInternal.log?.Invoke($"[Autotest] {message}");
			AutotestingInternal.currentScript.logs += $"{message}{Environment.NewLine}";
		}
		
		private static void Error(string type, string message)
		{
			AutotestingInternal.currentScript.logs += $"[Error] {type} - {message}{Environment.NewLine}";
			throw new ScriptException(type, message);
		}
		
		private static void Click(GameObject gameObject)
		{
			AutotestingInternal.unityBinding.ExecuteOnMainThreadAndWaitForCompletion(() => AutotestingInternal.unityBinding.Click(gameObject));
		}

		private static void Tap(GameObject gameObject)
		{
			AutotestingInternal.unityBinding.ExecuteOnMainThreadAndWaitForCompletion(() => AutotestingInternal.unityBinding.Tap(gameObject));
		}
		
		private static void InputText(GameObject gameObject, string message)
		{
			InputField inputField = null;
			
			// Activate input field (set focus)
			AutotestingInternal.unityBinding.ExecuteOnMainThreadAndWaitForCompletion(() =>
			{
				inputField = gameObject.GetComponent<InputField>();
				
				if (inputField == null)
					throw new ScriptException("InputText", $"No input field on the GameObject {gameObject.name}");
				
				inputField.ActivateInputField();
			});
			
			// Wait some frames to let Unity apply the focus on the inputField
			Thread.Sleep(3 * UnityBinding.frameDuration);

			// Set text, trigger onValueChanged and onEndEdit, then unfocus the component
			AutotestingInternal.unityBinding.ExecuteOnMainThreadAndWaitForCompletion(() =>
			{
				inputField.text = message;
				inputField.DeactivateInputField();
			});
		}

		private static void Drag(GameObject target, GameObject origin, GameObject destination, float duration)
		{
			Operation operationA = AutotestingInternal.unityBinding.ExecuteOnMainThread(() => AutotestingInternal.unityBinding.GetScreenspacePosition(origin));
			Operation operationB = AutotestingInternal.unityBinding.ExecuteOnMainThread(() => AutotestingInternal.unityBinding.GetScreenspacePosition(destination));

			AutotestingInternal.unityBinding.WaitForOperationCompletion(operationA);
			AutotestingInternal.unityBinding.WaitForOperationCompletion(operationB);

			Vector2 originScreenPosition = (Vector2)operationA.result;
			Vector2 destinationScreenPosition = (Vector2)operationB.result;

			DragFromPosition(target, originScreenPosition, destinationScreenPosition, duration);
		}
		
		private static void DragFromPosition(GameObject target, Vector2 origin, Vector2 destination, float duration)
		{
			AutotestingInternal.unityBinding.ExecuteOnMainThreadAndWaitForCompletion(() => AutotestingInternal.unityBinding.BeginDrag(target, origin));

			DateTime startTime = DateTime.UtcNow;
			float t = 0;
			while (t < 1)
			{
				t = (float)(DateTime.UtcNow - startTime).TotalSeconds / duration;
				Vector2 position = Vector2.Lerp(origin, destination, t);
				
				AutotestingInternal.unityBinding.ExecuteOnMainThreadAndWaitForCompletion(() => AutotestingInternal.unityBinding.Drag(target, position));
			}

			AutotestingInternal.unityBinding.ExecuteOnMainThreadAndWaitForCompletion(() => AutotestingInternal.unityBinding.EndDrag(target, destination));
		}

		private static void Restart()
		{
			if (AutotestingInternal.restartApp == null)
				throw new NotSupportedException("Unable to call Restart because the application does not support it.");

			AutotestingInternal.unityBinding.ExecuteOnMainThreadAndWaitForCompletion(() => AutotestingInternal.restartApp?.Invoke());
		}

	}
	
}

#pragma warning restore 0169