# UnityDebugConsole
This tool will help you debug game features while playing without having to create a debug panel from scratch.

<p align="center">
  <img src="https://github.com/Mukarillo/UnityDebugConsole/blob/master/ExampleImage/example.gif?raw=true" alt="Example"/>
</p>

## How to use
*you can find a pratical example inside this repository in Main scene*

### 1 - Place DebugConsole folder on your project (`DebugConsole.cs` and `DebugConsoleAttribute.cs`)

### 2 - Add ```[DebugConsole("name", "description")]``` attribute to any method in any class that is referenced by `Assembly-CSharp.dll`
```c#
public class Exemple : MonoBehaviour
{
    private int lives;
    [DebugConsole("Add Lives", "This method add lives to the player.")]
    public void AddLives(int amount)
    {
        lives += amount;
    }
}
```

### 3 - Drag DebugConsole class to any GameObject and call ```DebugConsole.Instance.Open``` to open the console at anytime.
<p align="center">
  <img src="https://github.com/Mukarillo/UnityDebugConsole/blob/master/ExampleImage/lives.gif?raw=true" alt="Example"/>
</p>

## Considerations
* This tool is just for debug purposes. It heavily relies on Reflection, so it's not supposed to be used in release builds.
* There is no way to know in wich instance the method is supposed to be called, so it will call the first object it finds with `FindObjectOfType`. If the method is in a class that it doesn't inherit from MonoBehaviour, it will create another instance of that class and call that method. If the method is static, it doesn't need an instance, so it will call it normally.
* You use the DebugConsole attribute in `public`, `private` and `static` methods.
* The method can have as many parameters as you wish. But only `int`, `float`, `string` and `bool` are being considered.
  * All Custom Attribute valid parameters are:
    * Simple types (bool, byte, char, short, int, long, float, and double)
    * string
    * enums
    * object (The argument to an attribute parameter of type object must be a constant value of one of the above types.)
    * one-dimensional arrays of any of the above types
  * To be able to use the types that are not being considered go to `DebugConsole.cs` at line `133` and add another verification to that method and draw the correct GUI elements to send to the method. **(if you do that, please consider sending a pull request 😊)**
  
