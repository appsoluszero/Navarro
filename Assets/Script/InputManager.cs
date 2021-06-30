using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class InputManager
{
    //dictionary to determine the keycode with associated actions (in string)
    public static Dictionary<string, KeyCode> actionsMap = new Dictionary<string, KeyCode>(); 

    //initialize a default option for all keybinding
    public static void defaultInitialize() {
        actionsMap["walkLeft"] = KeyCode.A;
        actionsMap["walkRight"] = KeyCode.D;
        actionsMap["jumpingUp"] = KeyCode.W;
        actionsMap["rollingDodge"] = KeyCode.G;
        actionsMap["dropDown"] = KeyCode.S;
        actionsMap["crouchDown"] = KeyCode.LeftControl;
        actionsMap["attack"] = KeyCode.Mouse0;
        actionsMap["attackRanged"] = KeyCode.Mouse1;
    }

    //changing keybind associated with certain action
    public static void changeKeybind(string action, KeyCode key) {
        actionsMap[action] = key;
    }

    //getting keybind associated with certain action
    public static KeyCode getKeybind(string action) {
        if(actionsMap.ContainsKey(action))
            return actionsMap[action];
        return KeyCode.None;
    }
}
