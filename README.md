

Autotest is a cross-platform unity module that execute scripts into your application in order to run automatic testing. This module is basically an embedded HTTP server that wait for a *run* request to execute the scripts found in its body.

With all the pre-included functionalities, it is perfect for testing flows as Loading, UI or state machines. This module is also highly extensible for your custom needs.

You can find a sample project here: [https://github.com/MaxiPaille/autotest-sample](https://github.com/MaxiPaille/autotest-sample).

# Supported platforms

 - Unity Editor (Windows x64, MacOS x64)
 - Standalone Player (Windows x64, MacOS x64)
 - Android (armv7, arm64)
 - iOS (armv7, arm64)

Tested on *Unity 2019.2.x* and *Unity 2019.3.x* with *Api Compatibility Level* set to *.Net Standard 2.0*.

# Scripting

Lua is the scripting language used by the module. CSharp environment can be directly used within the lua context (thanks to the NLua framework).

A script is considered as successfull if it can reach the end without throwing any error. If an error is thrown, the script is stopped immediatly.

## Basic script

    function run()
	    -- Script here
    end

**import(string)**
Import a CSharp namespace.

**function run()**
Entry point of the script.

## Scripting API

**void Restart()**
Restart the app synchronously

**void Log(string)**
Log a message from script. You can retreive the logs at the end of the execution.

**void Error(string, string)**
Force the script to fail and write an error in the logs.

**GamObject GetGameObjectFromPath(string)**
Get a *GameObject* from an absolute path (i.e. Parent/Child/Name). An error will be throw if not found. This function is quicker than *SearchForGameObject*.

**GameObject SearchForGameObject(string)**
Search for a *GameObject* in the scene. The pattern could be used as a path (i.e. Parent/Child/Name). Null will be return if not found. Be carefull, this method is slow.

**GetComponent(GameObject, Type)**
Return a *Component* from the *GameObject*. An error will be throw if not found.

**void Wait(float)**
Wait for several seconds

**void WaitForEnabled(GameObject)**
Wait for a *GameObject* to be enabled.

**void WaitForEnabled(GameObject, float)**
Wait for a *GameObject* to be enabled with a timeout. An error will be throw if the request timeout.

**void WaitForDisabled(GameObject)**
Wait for a *GameObject* to be disabled.

**void WaitForDisabled(GameObject, float)**
Wait for a *GameObject* to be disabled with a timeout. An error will be throw if the request timeout.

**void WaitForEvent(string)**
Wait for an event to be fired.

**void WaitForEvent(string, float)**
Wait for an event to be fired with a timeout. An error will be throw if the request timeout.

**void RegisterEventAsError(string)**
Register an event into the error trigger list. If received, this event will be processed as a script error and will stop the execution.

**void UnregisterEventAsError(string)**
Remove an event from the error trigger list.

**void CheckForEnable(GameObject)**
Check if the *GameObject* is enabled and throw an error if not.

**void CheckForDisable(GameObject)**
Check if the *GameObject* is disabled and throw an error if not.

**void Tap(GameObject)**
Tap on the *GameObject*.

**void Click(GameObject)**
Tap on the *GameObject*. Unlike *Tap*, it will throw an error if the *GameObject* is not enabled or does not have a clickable *Button* component.

**void Drag(GameObject, GameObject, GameObject, float)**
Drag the first *GameObject* from the second *GameObject*'s position to the third *GameObject*'s position using the specified duration.

**void DragFromPosition(GameObject, Vector2, Vector2, float)**
Drag the *GameObject* between two screenspace positions using the specified duration.

**void InputText(GameObject, string)**
Simulate a keyboard input into an *InputField*. Throw an error if the *GameObject* does not have an *InputField* component.

# Implementation into project

Add the module into your project and reference the *Autotest.asmdef* into your game assemblies.

![](https://user-images.githubusercontent.com/6653003/87885823-92d9e200-ca18-11ea-9d41-1792300b82ce.png)

To enable the Autotest module, you need to define the 'AUTOTEST' scripting symbol in the *Project Settings*.

![](https://user-images.githubusercontent.com/6653003/87885880-00860e00-ca19-11ea-8273-599a8385dc85.png)

The module API is designed as *Conditional* and will be ignored if you do not declare the 'AUTOTEST' scripting symbol. This allows you to ship a build with the Autotest module disabled without modifiying your source code.

## CSharp API

**void Autotesting.Initialize(Action)**
Activate the module and provide a custom function to restart the app when script need it.

**void Autotesting.Event(string)**
Send an event to the running script.

# Extend scripting functionalities
You can extend functionalities by adding static methods on the *Autotest/Internal/ScriptFunctions.cs* file. Theses methods will be automatically exposed in the lua context as global functions.

> Note: the Unity's API is monothread but the lua scripts are running in a separate thread. You can use *AutotestingInternal.unityBinding.ExecuteOnMainThread* to synchronize your call to the Unity API.

# Settings

You can change settings by editing the *Settings.cs* file. 

**Port:**
Port use by the http server to process scripts.
Default value: 4679.

# Usage

Communication with the module is provided by HTTP.

**GET http://{device}:{port}/run**

*Headers*
None

*Content*
List of all scripts to run formated as bellow.
```
LUA {script_name1}:
{lua_script1}
LUA {script_name2}:
{lua_script2}
...
```
You can use the 'AutotestScripts/format.sh' bash script to automaticaly format body.

*Result*
 - 200: success
 - 417: at least one of the scripts failed
 - 500: unkown error (probably while parsing the request)

*Sample*
```curl -s -w "%{http_code}\n" http://127.0.0.1:4679/run --data-binary "$(AutotestScripts/format.sh)"```

# Externals

[NLua](https://github.com/NLua/NLua) - .Net lua integration framework

[KeraLua](https://github.com/NLua/KeraLua) - Native lua binding

[Lua](http://www.lua.org/) - Scripting backend (version 5.4)

# Information

This module is provided "as is" without warranty of any kind. I will not provide any support other than major issues fix.

Tested on *Unity 2019.2.x* and *Unity 2019.3.x*
