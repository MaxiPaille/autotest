

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

![Reference Autotest.asmdef](data:image/png;base64,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)

To enable the Autotest module, you need to define the 'AUTOTEST' scripting symbol in the *Project Settings*.

![Add AUTOTEST into Scripting symbols](data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAPoAAAAeBAMAAAD3Fx6aAAAAAXNSR0IB2cksfwAAAAlwSFlzAAAWJQAAFiUBSVIk8AAAAB5QTFRFpqamwMDAmZmZi4uLPj4+LCwsaWlpUVFRf39/tbW19c+CwgAAAixJREFUeJzt1ctzmkAcB/CfCwv2xvZh4w0JTia3jVYzvRHBR25WpXDEVom5ofF1xKnG5lY7nfy93QVz6EynJwI95DsM+9sfzHx4A5BxJCvkg0gBEGrEPWS1+FAb0XguxPuweHwlJ6dvqz0+nPIjCCmokdbfB6xaC4CAL+Vqh7dVBA2VzTHvJZORCrLtWfO69XnnGqNmOLZDoQs92f5ub+tNV7IDqKmwhd7cXjTadmXh4jzrJRPZoVsIemLdKIuh4UuzLua6uReMtVgP9jqYIA0oPg2WYmi28sugjGVKE9Ih3/cBDAaJemhM8mtD5HpjYXeZ7l3ubJfdClfah2uRmg3khBgLtpcQPoHOHjymG1jk+l107vmuj2ikfwAK9+ALTZXrHaHtlTFCvYT02tw4m3PdaUsu0wN264WBRfV5EOlS2wDdMsEBrrcXlXYL815C0aIFpJkaz6kYPvWjoLg+vozq0zzZSN6xWAz+thkn9ZS/5P9INatUGI4OJKMoXM8KJ0U10kvkFfnxWtOIEhKknf/USqzQtPNUdKVPvhKxMF4eVqPhvfMF+7fb0cZ3vFT0wg3Xydvcm2nxmtyxklwXL0j3mfGj/u5XLtbfl8iE6VjL7R5IWrouD//U/QtlvElLX91cTYjO9JMhMeIrX1KuUtKLn8gMfzOZXmxdTvm5Vz8uz6Yp6cqGaMqqRE4OpLAmJCSP/u0jKx5S0Z8b+bd+yErnX1rQMksW/9SXRPkNcYb2ZEY9HjEAAAAASUVORK5CYII=)

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