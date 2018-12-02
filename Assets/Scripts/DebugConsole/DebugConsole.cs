using UnityEngine;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using System;
using System.Text.RegularExpressions;

namespace DebugConsoleTool
{
    public class DebugConsole : MonoBehaviour
    {
        public static DebugConsole Instance;

        private List<Assembly> mAssembliesToSearch = new List<Assembly>();

        private List<MethodInfo> mStaticMethods = new List<MethodInfo>();
        private List<MethodInfo> mNonStaticMethods = new List<MethodInfo>();
        private List<MethodInfo> mAllMethods
        {
            get
            {
                var all = new List<MethodInfo>(mStaticMethods);
                all.AddRange(mNonStaticMethods);
                return all;
            }
        }
        private MethodInfo mCurrentMethod;
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

            mStaticMethods = GetConsoleDebugMethods(true);
        }

        public void Open()
        {
            RefreshMethods();
            mShowingConsole = true;
        }

        public void RefreshMethods()
        {
            mNonStaticMethods = GetConsoleDebugMethods(false);
        }

        public void Close()
        {
            mShowingConsole = false;
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
            foreach (var method in mAllMethods)
            {
                if (!method.IsStatic && typeof(MonoBehaviour).IsAssignableFrom(method.DeclaringType))
                    if (FindObjectOfType(method.DeclaringType) == null)
                        continue;

                needEnd = true;
                if (i % 2 == 0 || i == 0)
                    GUILayout.BeginHorizontal();

                DebugConsoleAttribute attribute = method.GetCustomAttributes(true)[0] as DebugConsoleAttribute;

                if (GUILayout.Button(attribute.name, GUILayout.Height(Screen.height * 0.2f)))
                    MethodInvoker(method);

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
            GUILayout.Box(((DebugConsoleAttribute)mCurrentMethod.GetCustomAttributes(true)[0]).description, GUILayout.Width(Screen.width));

            GUILayout.BeginVertical();

            var parameters = mCurrentMethod.GetParameters();
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

        private void MethodInvoker(MethodInfo method, object[] parameters = null)
        {
            if (method.GetParameters().Length > 0 && parameters == null)
            {
                ResetCurrentMethod();
                mCurrentMethod = method;
                return;
            }

            if (method.IsStatic)
                method.Invoke(null, parameters);
            else
            {
                if (typeof(MonoBehaviour).IsAssignableFrom(method.DeclaringType))
                {
                    method.Invoke(FindObjectOfType(method.DeclaringType), parameters);
                }
                else
                    method.Invoke(Activator.CreateInstance(method.DeclaringType), parameters);
            }

            ResetCurrentMethod();
        }

        private void ResetCurrentMethod()
        {
            mCurrentMethod = null;
            mParamsValues = new List<string>();
            mUserParameters = new List<object>();
        }

        private List<MethodInfo> GetConsoleDebugMethods(bool isStatic)
        {
            return mAssembliesToSearch
               .SelectMany(x => x.GetTypes())
               .SelectMany(x => x.GetMethods()
               .Where(y => y.IsStatic == isStatic)
               .Where(z => z.GetCustomAttributes(typeof(DebugConsoleAttribute), true).Length > 0))
               .ToList();
        }

        private object GetDefaultValue(Type t)
        {
            if (t.IsValueType)
                return Activator.CreateInstance(t);

            return null;
        }
    }
}
