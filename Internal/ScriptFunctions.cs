using System;
using System.Threading;
using Autotest.Internal;
using NLua;
using UnityEngine;
using UnityEngine.Assertions;

namespace Autotest
{
	
	internal static class ScriptFunctions
	{
		
		public static readonly int frameDelay = 1000 / 60;

		private static void WaitForEnable(GameObject gameObject, float timeout = 0)
		{
			DateTime timeoutLimit = DateTime.UtcNow.AddSeconds(timeout);
			while (true)
			{
				if (timeout > 0 && DateTime.UtcNow > timeoutLimit)
					throw new TimeoutException($"WaitForEnable {timeout}s");
				
				Operation operation = AutotestingInternal.unityBinding.ExecuteOnMainThread(() => gameObject.activeInHierarchy);

				while (operation.status == false)
					Thread.Sleep(frameDelay);

				if ((bool) operation.result == true)
					break;
			}
		}

		private static void WaitForDisable(GameObject gameObject, float timeout = 0)
		{
			DateTime timeoutLimit = DateTime.UtcNow.AddSeconds(timeout);
			while (true)
			{
				if (timeout > 0 && DateTime.UtcNow > timeoutLimit)
					throw new TimeoutException($"WaitForDisable {timeout}s");
				
				Operation operation = AutotestingInternal.unityBinding.ExecuteOnMainThread(() => gameObject.activeInHierarchy);

				while (operation.status == false)
					Thread.Sleep(frameDelay);

				if ((bool) operation.result == false)
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
					throw new TimeoutException($"WaitForEvent {timeout}s");
				
				Thread.Sleep(frameDelay);
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
				Thread.Sleep(frameDelay);
		}

		private static void CheckForEnable(GameObject gameObject)
		{
			Operation operation = AutotestingInternal.unityBinding.ExecuteOnMainThread(() => gameObject.activeInHierarchy == true);

			while(operation.status == false)
				Thread.Sleep(frameDelay);
			
			if ((bool)operation.result == false)
				throw new AssertionException($"GameObject '{AutotestingInternal.unityBinding.GetName(gameObject)}' not enabled", "");
		}
		
		private static void CheckForDisable(GameObject gameObject)
		{
			Operation operation = AutotestingInternal.unityBinding.ExecuteOnMainThread(() => gameObject.activeInHierarchy == false);

			while(operation.status == false)
				Thread.Sleep(frameDelay);
			
			if ((bool)operation.result == false)
				throw new AssertionException($"GameObject '{AutotestingInternal.unityBinding.GetName(gameObject)}' not disabled", "");
		}

		private static GameObject SearchForGameObject(string pattern)
		{
			Operation operation = AutotestingInternal.unityBinding.ExecuteOnMainThread(() => AutotestingInternal.unityBinding.FindGameObject(pattern));
			
			while(operation.status == false)
				Thread.Sleep(frameDelay);

			return operation.result as GameObject;
		}

		private static GameObject GetGameObjectFromPath(string path)
		{
			Operation operation = AutotestingInternal.unityBinding.ExecuteOnMainThread(() => AutotestingInternal.unityBinding.GetGameObjectFromPath(path));
			
			while(operation.status == false)
				Thread.Sleep(frameDelay);

			GameObject result = operation.result as GameObject;
			if (result == null)
				throw new AssertionException($"GameObject \"{path}\" does not exists", "");
			
			return result;
		}
		
		private static Component GetComponent(GameObject gameObject, ProxyType componentType)
		{
			Operation operation = AutotestingInternal.unityBinding.ExecuteOnMainThread(() => gameObject.GetComponent(componentType.UnderlyingSystemType));
			
			while(operation.status == false)
				Thread.Sleep(frameDelay);
			
			Component result = operation.result as Component;
			if (result == null)
				throw new AssertionException($"Component \"{componentType.UnderlyingSystemType.Name}\" not found on GameObject \"{gameObject.name}\"", "");
			
			return result;
		}
		
		private static void Log(string message)
		{
			Debug.Log(message);
		}
		
		private static void Error(string type, string message)
		{
			throw new Exception($"{type}: {message}");
		}
		
		private static void Click(GameObject gameObject)
		{
			Operation operation = AutotestingInternal.unityBinding.ExecuteOnMainThread(() => AutotestingInternal.unityBinding.Click(gameObject));

			while(operation.status == false)
				Thread.Sleep(frameDelay);
		}
		
		private static void Tap(GameObject gameObject)
		{
			Operation operation = AutotestingInternal.unityBinding.ExecuteOnMainThread(() => AutotestingInternal.unityBinding.Tap(gameObject));

			while(operation.status == false)
				Thread.Sleep(frameDelay);
		}

		private static void Drag(GameObject target, GameObject origin, GameObject destination, float duration)
		{
			Operation operation = AutotestingInternal.unityBinding.ExecuteOnMainThread(() => AutotestingInternal.unityBinding.GetScreenspacePosition(origin));

			while(operation.status == false)
				Thread.Sleep(frameDelay);

			Vector2 originScreenPosition = (Vector2)operation.result;
			
			operation = AutotestingInternal.unityBinding.ExecuteOnMainThread(() => AutotestingInternal.unityBinding.GetScreenspacePosition(destination));

			while(operation.status == false)
				Thread.Sleep(frameDelay);

			Vector2 destinationScreenPosition = (Vector2)operation.result;

			DragFromPosition(target, originScreenPosition, destinationScreenPosition, duration);
		}
		
		private static void DragFromPosition(GameObject target, Vector2 origin, Vector2 destination, float duration)
		{
			Operation operation = AutotestingInternal.unityBinding.ExecuteOnMainThread(() => AutotestingInternal.unityBinding.BeginDrag(target, origin));

			while(operation.status == false)
				Thread.Sleep(frameDelay);
			
			DateTime startTime = DateTime.UtcNow;
			float t = 0;
			Vector2 previousPosition = origin;
			while (t < 1)
			{
				t = (float)(DateTime.UtcNow - startTime).TotalSeconds / duration;
				Vector2 position = Vector2.Lerp(origin, destination, t);
				
				operation = AutotestingInternal.unityBinding.ExecuteOnMainThread(() => AutotestingInternal.unityBinding.Drag(target, position));

				while(operation.status == false)
					Thread.Sleep(frameDelay);
			}

			operation = AutotestingInternal.unityBinding.ExecuteOnMainThread(() => AutotestingInternal.unityBinding.EndDrag(target, destination));

			while(operation.status == false)
				Thread.Sleep(frameDelay);
		}

	}
	
}