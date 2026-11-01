using UnityEngine;
namespace Oathfall.Presentation
{
    public static class PresentationSettings
    {
        public static float Master {get=>PlayerPrefs.GetFloat("oathfall.audio.master",.8f);set{PlayerPrefs.SetFloat("oathfall.audio.master",Mathf.Clamp01(value));PlayerPrefs.Save();}}
        public static float Music {get=>PlayerPrefs.GetFloat("oathfall.audio.music",.65f);set{PlayerPrefs.SetFloat("oathfall.audio.music",Mathf.Clamp01(value));PlayerPrefs.Save();}}
        public static float Ambience {get=>PlayerPrefs.GetFloat("oathfall.audio.ambience",.8f);set{PlayerPrefs.SetFloat("oathfall.audio.ambience",Mathf.Clamp01(value));PlayerPrefs.Save();}}
        public static float Sfx {get=>PlayerPrefs.GetFloat("oathfall.audio.sfx",1);set{PlayerPrefs.SetFloat("oathfall.audio.sfx",Mathf.Clamp01(value));PlayerPrefs.Save();}}
        public static float Flash {get=>PlayerPrefs.GetFloat("oathfall.flash",.5f);set{PlayerPrefs.SetFloat("oathfall.flash",value);PlayerPrefs.Save();}}
        public static float Shake {get=>PlayerPrefs.GetFloat("oathfall.shake",.35f);set{PlayerPrefs.SetFloat("oathfall.shake",value);PlayerPrefs.Save();}}
        public static float UiScale {get=>PlayerPrefs.GetFloat("oathfall.uiScale",1);set{PlayerPrefs.SetFloat("oathfall.uiScale",value);PlayerPrefs.Save();}}
    }
}
