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

## DebugConsole `public` overview

### Methods

> `DebugConsole.Open`
- *Description*: Start rendering the console/buttons. It also will call `RefreshMethods`.

> `DebugConsole.Close`
- *Description*: Stop rendering the console/buttons.

> `DebugConsole.RefreshMethods`
- *Description*: This method will verify witch `MonoBehaviour` objects are active and have the Custom Attribute. It is supposed to be called on `OnDestroy` method in a script that has a method with DebugConsole attribute.

> `DebugConsole.AddInstanceMethod`
- *Description*: Use this to register methods at run-time. It will allow you to specify the instance wich the method willbe called.

- *Parameters* :

|name  |type  |description  |
|--|--|--|
|`id` |**string** |*Unique ID to reference the instance method.*  |
|`methodName` |**string** |*Text to be displayed in the console button title.*  |
|`methodDescription` |**string** |*Text to be displayed in the console button description.*  |
|`target` |**object** |*Instance reference to the object.*  |
|`method` |**Action** |*Method to be called.*  |

> `DebugConsole.RemoveInstanceMethod`
- *Description*: Use this to remove methods that were previous registered using its `id`.

- *Parameters* :

|name  |type  |description  |
|--|--|--|
|`id` |**string** |*Unique ID to reference the instance method.*  |

## Considerations
* This tool is just for debug purposes. It heavily relies on Reflection, so it's not supposed to be used in release builds.
* If the method was registered with the Custom Attribute, there is no way to know in wich instance the method is supposed to be called, so it will call the first object it finds with `FindObjectOfType`. If the method is in a class that it doesn't inherit from MonoBehaviour, it will create another instance of that class and call that method. If the method is static, it doesn't need an instance, so it will call it normally.
  * If you wish to invoke the method in an specific instance, register the method using `DebugConsole.Instance.AddInstanceMethod`.
* You use the DebugConsole attribute in `public`, `private` and `static` methods.
* The method can have as many parameters as you wish. But only `int`, `float`, `string` and `bool` are being considered.
  * All Custom Attribute valid parameters are:
    * Simple types (bool, byte, char, short, int, long, float, and double)
    * string
    * enums
    * object (The argument to an attribute parameter of type object must be a constant value of one of the above types.)
    * one-dimensional arrays of any of the above types
  * To be able to use the types that are not being considered go to `DebugConsole.cs` at line `133` and add another verification to that method and draw the correct GUI elements to send to the method. **(if you do that, please consider sending a pull request 😊)**
  
