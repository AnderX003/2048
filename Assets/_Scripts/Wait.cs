using System;
using System.Collections;
using UnityEngine;

namespace _Scripts
{
    public class Wait : MonoBehaviour
    {
        private static Wait instance;

        private void Start()
        {
            instance = this;
        }
        
        public static void ForFrame(Action action)
        {
            instance.StartCoroutine(WaitForFrame(action));
        }

        public static void ForFrames(int frames, Action action)
        {
            instance.StartCoroutine(WaitForFrames(frames, action));
        }
        
        public static void ForSeconds(float seconds, Action action)
        {
            instance.StartCoroutine(WaitForSeconds(seconds, action));
        }

        public static void ForCondition(Func<bool> condition, Action action)
        {
            instance.StartCoroutine(WaitForCondition(condition, action));
        }

        private static IEnumerator WaitForFrame(Action action)
        {
            yield return null;
            action();
        }

        private static IEnumerator WaitForFrames(int frames, Action action)
        {
            for (int i = 0; i < frames; i++)
            {
                yield return null;
            }

            action();
        }

        private static IEnumerator WaitForSeconds(float seconds, Action action)
        {
            if (action == null)
                yield break;
            yield return new WaitForSeconds(seconds);
            action();
        }

        private static IEnumerator WaitForCondition(Func<bool> condition, Action action)
        {
            while (!condition())
            {
                yield return null;
            }

            action();
        }
    }
}
