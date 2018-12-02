using UnityEngine;
using DebugConsoleTool;

public class Test : MonoBehaviour
{
    public CircularGesture gesture;

    public void Start()
    {
        gesture.OnGestureDone.AddListener(DebugConsole.Instance.Open);

        DebugConsole.Instance.AddInstanceMethod("test1", "In-line method", "", this, () =>
        {
            Debug.Log("In-line method");
        });

        DebugConsole.Instance.AddInstanceMethod("test2", "Declared Method", "", this, DeclaredMethod);
    }

    public void DeclaredMethod()
    {
        Debug.Log("DeclaredMethod");
    }

    [DebugConsole("no param", "desc")]
    public static void IntTest()
    {
        Debug.Log("NO PARAM");
    }

    [DebugConsole("int test", "desc")]
    public void IntTest(int value)
    {
        Debug.Log("INT TEST: "+ value);
    }

    [DebugConsole("float test", "desc")]
    public void FloatTest(float value)
    {
        Debug.Log("FLOAT TEST: " + value);
    }

    [DebugConsole("string test", "desc")]
    public void StringTest(string value)
    {
        Debug.Log("STRING TEST: " + value);
    }

    [DebugConsole("bool test", "desc")]
    public void BoolTest(bool value)
    {
        Debug.Log("BOOL TEST: " + value);
    }

    [DebugConsole("Multiple Values test", "desc")]
    public void Multiple(string stringValue, bool boolValue, float floatValue, int intValue)
    {
        Debug.Log("STRING TEST: " + stringValue);
        Debug.Log("BOOL TEST: " + boolValue);
        Debug.Log("FLOAT TEST: " + floatValue);
        Debug.Log("INT TEST: " + intValue);
    }
}
