namespace Oathfall.Core
{
    public readonly struct BladePoint
    {
        public readonly float X,Y,Z;
        public BladePoint(float x,float y,float z){X=x;Y=y;Z=z;}
    }
    // Actor-local samples of the authored weapon, independent of Unity in Core.
    public interface IWeaponTrace
    {
        bool SampleBlade(float normalizedTime,out BladePoint grip,out BladePoint tip);
        void SetAttackProgress(float normalizedTime);
    }
}
