using System;
using System.Collections.Generic;

namespace _Scripts
{
    public class UnitAnimationQueue
    {
        public Action Callback => StartAnimation;
        
        private readonly Queue<Action> queue = new Queue<Action>();
        private bool animationIsPlaying;

        public void AddAnimation(Action animationCall)
        {
            if (queue.Count == 0 && !animationIsPlaying)
            {
                animationCall();
                animationIsPlaying = true;
            }
            else
            {
                queue.Enqueue(animationCall);
            }
        }

        private void StartAnimation()
        {
            if (queue.Count == 0)
            {
                animationIsPlaying = false;
            }
            else
            {
                queue.Dequeue()();
            }
        }
    }
}
