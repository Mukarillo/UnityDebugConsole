using UnityEngine;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using System;
using System.Text.RegularExpressions;

namespace DebugConsoleTool
{
    internal class DebugMethod
    {
        public string id { get; private set; }
        public MethodInfo method { get; private set; }
        public object target { get; private set; }

        private readonly string methodName;
        private readonly string methodDescription;

        public string MethodName => MethodDebugConsoleAttribute != null ? MethodDebugConsoleAttribute.name : methodName;
        public string MethodDescription => MethodDebugConsoleAttribute != null ? MethodDebugConsoleAttribute.description : methodDescription;
        public DebugConsoleAttribute MethodDebugConsoleAttribute => method.GetCustomAttributes(true).FirstOrDefault(x => x is DebugConsoleAttribute) as DebugConsoleAttribute;

        public DebugMethod(MethodInfo method)
        {
            this.method = method;
        }

        public DebugMethod(string id, string methodName, string methodDescription, object target, MethodInfo method)
        {
            this.id = id;
            this.method = method;
            this.target = target;
            this.methodName = methodName;
            this.methodDescription = methodDescription;
        }
    }

    public class DebugConsole : MonoBehaviour
    {
        public static DebugConsole Instance;

        private List<Assembly> mAssembliesToSearch = new List<Assembly>();

        private List<DebugMethod> mDebugMethods = new List<DebugMethod>();
        private List<DebugMethod> mInstanceDebugMethods = new List<DebugMethod>();
        private List<DebugMethod> AllMethods => new List<DebugMethod>(mDebugMethods).Concat(mInstanceDebugMethods).ToList();
        private DebugMethod mCurrentMethod;
        private bool mShowingConsole = false;

        //UI
        private Vector2 mScrollPositionButtons;
        private Vector2 mScrollPositionParameters;

        //Method params
        private List<object> mUserParameters = new List<object>();
        private List<string> mParamsValues = new List<string>();

        private void Awake()
        {
            if(Instance != null)
            {
                Destroy(this);
                return;
            }

            Instance = this;
            mAssembliesToSearch = AppDomain.CurrentDomain.GetAssemblies()
                                   .Where(x => x.ManifestModule.Name.Contains("Assembly-CSharp.dll")).ToList();

            DontDestroyOnLoad(this);
            RefreshMethods();
        }

        public void Open()
        {
            RefreshMethods();
            mShowingConsole = true;
        }

        public void Close()
        {
            mShowingConsole = false;
        }

        public void RefreshMethods()
        {
            mDebugMethods.Clear();

            mAssembliesToSearch
               .SelectMany(x => x.GetTypes())
                .SelectMany(x => x.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static)
               .Where(z => z.GetCustomAttributes(typeof(DebugConsoleAttribute), true).Length > 0))
               .ToList().ForEach(x => mDebugMethods.Add(new DebugMethod(x)));
        }

        public bool AddInstanceMethod(string id, string methodName, string methodDescription, object target, Action method)
        {
            if (mInstanceDebugMethods.Any(x => x.id.Equals(id)))
                return false;

            mInstanceDebugMethods.Add(new DebugMethod(id, methodName, methodDescription, target, method.Method));
            return true;
        }

        public bool RemoveInstanceMethod(string id)
        {
            var debugMethod = mInstanceDebugMethods.Find(x => x.id.Equals(id));
            if (debugMethod == null)
                return false;

            mInstanceDebugMethods.Remove(debugMethod);
            return true;
        }

        private void OnGUI()
        {
            if (!mShowingConsole) return;
            GUI.Box(new Rect(0, 0, Screen.width, Screen.height), "");
            if (mCurrentMethod != null)
            {
                DrawParameters();
                return;
            }

            if (GUILayout.Button("Close", GUILayout.Height(Screen.height * 0.1f)))
                Close();

            DrawMethodButtons();
        }

        private void DrawMethodButtons()
        {
            mScrollPositionButtons = GUILayout.BeginScrollView(mScrollPositionButtons, GUILayout.Width(Screen.width), GUILayout.Height(Screen.height));
            GUILayout.BeginVertical();
            bool needEnd = false;
            var i = 0;
            foreach (var debugMethod in AllMethods)
            {
                if (!debugMethod.method.IsStatic && typeof(MonoBehaviour).IsAssignableFrom(debugMethod.method.DeclaringType))
                    if (FindObjectOfType(debugMethod.method.DeclaringType) == null)
                        continue;

                needEnd = true;
                if (i % 2 == 0 || i == 0)
                    GUILayout.BeginHorizontal();
            
                var methodTarget = debugMethod.target == null ? "null" : debugMethod.target.ToString();
                if (GUILayout.Button(string.Format("{0} ({1})", debugMethod.MethodName, methodTarget), GUILayout.Height(Screen.height * 0.2f)))
                    MethodInvoker(debugMethod);

                if (i % 2 != 0)
                {
                    GUILayout.EndHorizontal();
                    needEnd = false;
                }

                i++;
            }

            if (needEnd) GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            GUILayout.EndScrollView();
        }

        private void DrawParameters()
        {
            mScrollPositionParameters = GUILayout.BeginScrollView(mScrollPositionParameters, GUILayout.Width(Screen.width), GUILayout.Height(Screen.height));
            GUILayout.Box(mCurrentMethod.MethodDescription, GUILayout.Width(Screen.width));

            GUILayout.BeginVertical();

            var parameters = mCurrentMethod.method.GetParameters();
            for (int i = 0; i < parameters.Length; i++)
            {
                var defaultValue = GetDefaultValue(parameters[i].ParameterType);
                if (mUserParameters.Count < i + 1)
                    mUserParameters.Add(defaultValue);
                if (mParamsValues.Count < i + 1)
                    mParamsValues.Add(defaultValue == null ? "" : defaultValue.ToString());

                GUILayout.BeginHorizontal();
                GUILayout.Label(parameters[i].Name);

                if (parameters[i].ParameterType.Equals(typeof(string)))
                {
                    mParamsValues[i] = GUILayout.TextArea(mParamsValues[i]);
                    mUserParameters[i] = mParamsValues[i];
                }
                else if (parameters[i].ParameterType.Equals(typeof(int)))
                {
                    mParamsValues[i] = GUILayout.TextArea(mParamsValues[i]);
                    mParamsValues[i] = Regex.Replace(mParamsValues[i], @"[^\d-]", "");
                    int result;
                    mUserParameters[i] = int.TryParse(mParamsValues[i], out result) ? result : defaultValue;
                }
                else if (parameters[i].ParameterType.Equals(typeof(float)))
                {
                    mParamsValues[i] = GUILayout.TextArea(mParamsValues[i]);
                    mParamsValues[i] = Regex.Replace(mParamsValues[i], @"[^\d.-]", "");
                    float result;
                    mUserParameters[i] = float.TryParse(mParamsValues[i], out result) ? result : defaultValue;
                }
                else if (parameters[i].ParameterType.Equals(typeof(bool)))
                {
                    mParamsValues[i] = GUILayout.Toggle(bool.Parse(mParamsValues[i]), "").ToString();
                    mUserParameters[i] = bool.Parse(mParamsValues[i]);
                }

                GUILayout.EndHorizontal();
            }

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Cancel"))
                ResetCurrentMethod();
            if (GUILayout.Button("OK"))
                MethodInvoker(mCurrentMethod, mUserParameters.ToArray());
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
            GUILayout.EndScrollView();
        }

        private void MethodInvoker(DebugMethod debugMethod, object[] parameters = null)
        {
            if (debugMethod.method.GetParameters().Length > 0 && parameters == null)
            {
                ResetCurrentMethod();
                mCurrentMethod = debugMethod;
                return;
            }

            if (debugMethod.method.IsStatic)
                debugMethod.method.Invoke(null, parameters);
            else
            {
                if(debugMethod.target != null)
                    debugMethod.method.Invoke(debugMethod.target, parameters);
                else if (typeof(MonoBehaviour).IsAssignableFrom(debugMethod.method.DeclaringType))
                    debugMethod.method.Invoke(FindObjectOfType(debugMethod.method.DeclaringType), parameters);
                else
                    debugMethod.method.Invoke(Activator.CreateInstance(debugMethod.method.DeclaringType), parameters);
            }

            ResetCurrentMethod();
        }

        private void ResetCurrentMethod()
        {
            mCurrentMethod = null;
            mParamsValues = new List<string>();
            mUserParameters = new List<object>();
        }

        private object GetDefaultValue(Type t)
        {
            return t.IsValueType ? Activator.CreateInstance(t) : null;
        }
    }
}
