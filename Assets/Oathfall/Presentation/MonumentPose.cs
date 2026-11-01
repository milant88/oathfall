using UnityEngine;
namespace Oathfall.Presentation
{
    public sealed class MonumentPose:MonoBehaviour
    {
        private void Start(){var animator=GetComponent<Animator>();if(animator==null)return;animator.Play("Block",0,0);animator.Update(.02f);animator.speed=0;}
    }
}
