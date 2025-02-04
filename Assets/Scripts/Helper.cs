using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Helper
{
    // Note: Must not destroy the gameObject the "script which is calling this class" is attached to or it will disrupt yield return WaitForSeconds();
    // thus preventing any code after the yield return from working
    public static Coroutine StartWaitingForSeconds(this MonoBehaviour caller, float seconds, Action callback)
    {
        return caller.StartCoroutine(WaitForSeconds_Coroutine(seconds, callback));
    }

    private static IEnumerator WaitForSeconds_Coroutine(float seconds, Action callback)
    {
        if (seconds > 0) { yield return new WaitForSeconds(seconds); }

        callback?.Invoke();
    }
}
