using System;

[AttributeUsage(AttributeTargets.Method)]
public class DebugConsoleAttribute : Attribute {

    public string name;
    public string description;

    public DebugConsoleAttribute(string name, string description)
    {
        this.name = name;
        this.description = description;
    }
}
