using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

public static class PlayerInput
{
    public static float GetAsAxis(AxisCode code, int controllerNumber)
    {
        var axisName = GetControllerSpecificName(typeof(AxisCode), code, controllerNumber);
        var result = Input.GetAxis(axisName);
        return result;
    }
    public static float GetAsAxis(ButtonCode code, int controllerNumber)
    {
        var buttonName = GetControllerSpecificName(typeof(ButtonCode), code, controllerNumber);
        var result = Input.GetAxis(buttonName);
        return result;
    }
    //********************************************************
    public static bool GetAsButton(ButtonCode code, int controllerNumber)
    {
        var buttonName = GetControllerSpecificName(typeof(ButtonCode), code, controllerNumber);
        return Input.GetButton(buttonName);
    }
    public static bool GetAsButtonDown(ButtonCode code, int controllerNumber)
    {
        var buttonName = GetControllerSpecificName(typeof(ButtonCode), code, controllerNumber);
        return Input.GetButtonDown(buttonName);
    }
    public static bool GetAsButtonUp(ButtonCode code, int controllerNumber)
    {
        var buttonName = GetControllerSpecificName(typeof(ButtonCode), code, controllerNumber);
        return Input.GetButtonUp(buttonName);
    }
    //**********************************************************************
    public static bool GetAsButton(AxisCode code, int controllerNumber)
    {
        var axisName = GetControllerSpecificName(typeof(AxisCode), code, controllerNumber);
        return Input.GetButton(axisName);
    }
    public static bool GetAsButtonDown(AxisCode code, int controllerNumber)
    {
        var axisName = GetControllerSpecificName(typeof(AxisCode), code, controllerNumber);
        return Input.GetButtonDown(axisName);
    }
    public static bool GetAsButtonUp(AxisCode code, int controllerNumber)
    {
        var axisName = GetControllerSpecificName(typeof(AxisCode), code, controllerNumber);
        return Input.GetButtonUp(axisName);
    }
    //***************************************************************************

    static string GetControllerSpecificName(Type enumType, object value, int controllerNumber)
    {
#if UNITY_EDITOR
        if(controllerNumber == 5)
            return Enum.GetName(enumType, value) + '_' + controllerNumber;
#endif
        if (controllerNumber > 4 || controllerNumber < 0)
            throw new InvalidControllerException("controller numbers range 0-4, with 0 indicating to get first availible input");

        if(controllerNumber == 0)
        {
            for (int i = 1; i <= 4; i++)
            {
                if(IsControllerConnected(i))
                {
                    controllerNumber = i;
                    break;
                }
            }
            if (controllerNumber == 0)
            {
#if UNITY_EDITOR
            controllerNumber = 5;
#else
            throw new InvalidControllerException("there are no connected controllers");
#endif
            }
        }

        var result = Enum.GetName(enumType, value) + '_' + controllerNumber;
        return result;
    }

    public static bool IsControllerConnected(int controllerNumber)
    {
#if UNITY_EDITOR
        if (controllerNumber == 5) return true;
#endif
        if (controllerNumber > 4 || controllerNumber < 0)
            throw new InvalidControllerException("controller numbers range 0-4, with 0 indicating any availible input");

        bool result = false;
        if(controllerNumber == 0)
        {
            var connected = Input.GetJoystickNames().Where(x => !string.IsNullOrEmpty(x)).ToArray();
            result = connected.Length > 0;
        }
        else
        {
            var controllers = Input.GetJoystickNames();
            //check if controllers array is bigger than the number and the slot is not null or empty
            result = controllers.Length >= controllerNumber && !String.IsNullOrEmpty(controllers[controllerNumber - 1]);
        }
        return result;
    }
}

public class InvalidControllerException : UnityException
{
    public InvalidControllerException()
    {
    }

    public InvalidControllerException(string message) : base(message)
    {
    }

    public InvalidControllerException(string message, Exception innerException) : base(message, innerException)
    {
    }

    protected InvalidControllerException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}