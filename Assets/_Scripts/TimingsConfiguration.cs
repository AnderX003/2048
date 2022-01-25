using UnityEngine;
using DG.Tweening;

namespace _Scripts
{
    [CreateAssetMenu(fileName = "TimingsConfiguration", menuName = "Data/Timings configuration", order = 51)]
    public class TimingsConfiguration : ScriptableObject
    {
        [field:SerializeField] public Ease moveEase {get; private set;}
        [field:SerializeField] public Ease appearanceEase {get; private set;}
        [field:SerializeField] public Ease punchEase {get; private set;}
        [field:SerializeField] public float moveDuration {get; private set;}
        [field:SerializeField] public float appearDuration {get; private set;}
        [field:SerializeField] public float appearDelay {get; private set;}
        [field:SerializeField] public float colorChangingDuration {get; private set;}
        [field:SerializeField] public float punchDuration {get; private set;}
        [field:SerializeField] public float punchRatio {get; private set;}
        [field:SerializeField] public float themeChangingDuration {get; private set;}
        [field:SerializeField] public float delayAfterUndo {get; private set;}
        [field:SerializeField] public float delayAfterClearingGridAfterGameOver {get; private set;}
        [field:SerializeField] public float delayBeforeShowingAd {get; private set;}
        [field:SerializeField] public int framesBeforeDrawingUnits {get; private set;}
    }
}
