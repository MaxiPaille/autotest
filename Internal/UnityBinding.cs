using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Autotest.Internal
{

    public class UnityBinding : MonoBehaviour
    {

        public static int frameDuration { get { return 1000 / s_framerate; } }


        private ConcurrentQueue<Operation> m_pendingOperations = new ConcurrentQueue<Operation>();

        private Dictionary<GameObject, string> m_gameObjectToName = new Dictionary<GameObject, string>();

        private static int s_framerate = 60;
        
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            s_framerate = Mathf.RoundToInt(1f / Time.deltaTime);
        }

        internal void Click(GameObject gameObject)
        {
            Button button = gameObject.GetComponent<Button>();
            if (button == null)
                throw new ScriptException("Click", "No 'Button' component");

            if (button.interactable == false || button.enabled == false || button.gameObject.activeInHierarchy == false)
                throw new ScriptException("Click", "Button not interactable");

            ExecuteEvents.Execute(gameObject, new BaseEventData(EventSystem.current), ExecuteEvents.submitHandler);
        }

        internal void Tap(GameObject gameObject)
        {
            ExecuteEvents.Execute(gameObject, new BaseEventData(EventSystem.current), ExecuteEvents.submitHandler);
        }

        /// <summary>
        /// Begin drag
        /// </summary>
        /// <param name="target">GameObject on which we want to apply the drag</param>
        /// <param name="origin">Screenspace coordinate</param>
        internal void BeginDrag(GameObject target, Vector2 origin)
        {
            PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
            pointerEventData.position = origin;

            ExecuteEvents.Execute(target, pointerEventData, ExecuteEvents.pointerDownHandler);
            ExecuteEvents.Execute(target, pointerEventData, ExecuteEvents.beginDragHandler);
        }

        /// <summary>
        /// Drag
        /// </summary>
        /// <param name="target">GameObject on which we want to apply the drag</param>
        /// <param name="position">Screenspace coordinate</param>
        internal void Drag(GameObject target, Vector2 position)
        {
            PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
            pointerEventData.position = position;

            ExecuteEvents.Execute(target, pointerEventData, ExecuteEvents.dragHandler);
        }

        /// <summary>
        /// End drag
        /// </summary>
        /// <param name="target">GameObject on which we want to apply the drag</param>
        /// <param name="destination">Screenspace coordinate</param>
        internal void EndDrag(GameObject target, Vector2 destination)
        {
            PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
            pointerEventData.position = destination;

            ExecuteEvents.Execute(target, pointerEventData, ExecuteEvents.endDragHandler);
            ExecuteEvents.Execute(target, pointerEventData, ExecuteEvents.pointerUpHandler);
        }

        internal Vector2 GetScreenspacePosition(GameObject target)
        {
            RectTransform rectTransform = target.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                Vector3[] worldCorners = new Vector3[4];
                rectTransform.GetWorldCorners(worldCorners);

                Canvas canvas = rectTransform.GetComponentInParent<Canvas>();

                Vector3 a = RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, worldCorners[0]);
                Vector3 b = RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, worldCorners[2]);

                return (a + b) * 0.5f;
            }

            return Camera.main.WorldToScreenPoint(target.transform.position);
        }

        internal GameObject GetGameObjectFromPath(string path)
        {
            string[] split = path.Split('/');

            List<GameObject> roots = new List<GameObject>();
            int n = SceneManager.sceneCount;
            for (int i = 0; i < n; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                roots.AddRange(scene.GetRootGameObjects());
            }

            GameObject current = null;
            for (int i = 0; i < split.Length; i++)
            {
                if (current == null)
                {
                    foreach (GameObject candidate in roots)
                    {
                        if (candidate.name == split[i])
                        {
                            current = candidate;
                            break;
                        }
                    }

                    if (current == null)
                        return null;
                }
                else
                {
                    Transform parentTransform = current.transform;
                    current = null;
                    for (int j = 0; j < parentTransform.childCount; j++)
                    {
                        Transform candidate = parentTransform.GetChild(j);
                        if (candidate.name == split[i])
                        {
                            current = candidate.gameObject;
                            break;
                        }
                    }

                    if (current == null)
                        return null;
                }
            }

            if (current != null)
                m_gameObjectToName[current] = current.name;

            return current;
        }

        // TODO: Optimize (prefer iterate on with than on depth) 
        internal GameObject FindGameObject(string pattern)
        {
            List<GameObject> roots = new List<GameObject>();
            int n = SceneManager.sceneCount;
            for (int i = 0; i < n; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                roots.AddRange(scene.GetRootGameObjects());
            }

            string[] split = pattern.Split('/');
            string targetName = split[0];

            foreach (GameObject root in roots)
            {
                GameObject result = GetGameObject(targetName, root.transform);

                if (result != null)
                {
                    if (split.Length == 1)
                        return result;

                    result = GetGameObject(split, 0, result.transform);

                    if (result != null)
                    {
                        if (result != null)
                            m_gameObjectToName[result] = result.name;

                        return result;
                    }
                }
            }

            return null;
        }

        private GameObject GetGameObject(string name, Transform parent)
        {
            if (parent.name == name)
                return parent.gameObject;

            for (int i = 0; i < parent.childCount; i++)
            {
                GameObject result = null;
                if ((result = GetGameObject(name, parent.GetChild(i))) != null)
                    return result;
            }

            return null;
        }

        private GameObject GetGameObject(string[] split, int index, Transform parent)
        {
            string targetName = split[index];
            if (parent.name == targetName)
            {
                if (index == split.Length - 1)
                    return parent.gameObject;

                GameObject result = null;
                for (int i = 0; i < parent.childCount; i++)
                {
                    Transform child = parent.GetChild(i);
                    if ((result = GetGameObject(split, index + 1, child)) != null)
                        return result;
                }
            }

            return null;
        }

        /// <summary>
        /// Thread safe method to get the name of a GameObject. This method cache the name of the GameObject for futher usage.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        internal string GetGameObjectName(GameObject target)
        {
            m_gameObjectToName.TryGetValue(target, out string targetName);

            if (targetName == null)
            {
                targetName = ExecuteOnMainThreadAndWaitForCompletion(() =>
                {
                    m_gameObjectToName[target] = target.name;
                    return target.name;
                });
            }

            return targetName;
        }

        internal Operation ExecuteOnMainThread<T>(Func<T> function)
        {
            Operation operation = new Operation()
            {
                methodInfo = function.Method,
                target = function.Target
            };
            m_pendingOperations.Enqueue(operation);
            return operation;
        }

        internal Operation ExecuteOnMainThread(Action action)
        {
            Operation operation = new Operation()
            {
                methodInfo = action.Method,
                target = action.Target
            };
            m_pendingOperations.Enqueue(operation);
            return operation;
        }
        
        internal T ExecuteOnMainThreadAndWaitForCompletion<T>(Func<T> function)
        {
            Operation operation = new Operation()
            {
                methodInfo = function.Method,
                target = function.Target
            };
            m_pendingOperations.Enqueue(operation);
            
            WaitForOperationCompletion(operation);
            
            return (T)operation.result;
        }
        
        internal void ExecuteOnMainThreadAndWaitForCompletion(Action action)
        {
            Operation operation = new Operation()
            {
                methodInfo = action.Method,
                target = action.Target
            };
            m_pendingOperations.Enqueue(operation);
            
            WaitForOperationCompletion(operation);
        }
        
        internal void WaitForOperationCompletion(Operation operation)
        {
            while(operation.status == false)
                Thread.Sleep(frameDuration);

            if (operation.exception != null)
                throw operation.exception;
        }

        private void Update()
        {
            s_framerate = Mathf.RoundToInt(1 / Time.deltaTime);

            while (m_pendingOperations.Count > 0)
            {
                if (m_pendingOperations.TryDequeue(out Operation operation) == false)
                    return;

                try
                {
                    operation.result = operation.methodInfo.Invoke(operation.target, null);
                }
                catch (Exception e)
                {
                    operation.exception = e;
                }
                finally
                {
                    operation.status = true;
                }
            }
        }
    }

}
