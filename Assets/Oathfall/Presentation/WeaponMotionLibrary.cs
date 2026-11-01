using UnityEngine;
namespace Oathfall.Presentation
{
    [System.Serializable] public sealed class WeaponMotion
    {
        public string state;
        public Vector3[] grips,tips;
        public void Sample(float time,out Vector3 grip,out Vector3 tip)
        {
            float frame=Mathf.Clamp01(time)*(tips.Length-1);int a=Mathf.FloorToInt(frame),b=Mathf.Min(a+1,tips.Length-1);
            grip=Vector3.Lerp(grips[a],grips[b],frame-a);tip=Vector3.Lerp(tips[a],tips[b],frame-a);
        }
    }
    public sealed class WeaponMotionLibrary:ScriptableObject
    {
        public WeaponMotion[] motions;
        public Vector3 handLocalTip;
    }
}
