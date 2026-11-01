using System.Collections.Generic;
using UnityEngine;
namespace Oathfall.Presentation
{
    public enum SoundCue { Loot,Equip,Open,Close,Click,Rune,Victory,Ward }
    public sealed class Soundscape:MonoBehaviour
    {
        public static Soundscape Instance{get;private set;}
        private AudioSource ambience,music;
        private readonly AudioSource[] voices=new AudioSource[8];
        private readonly Dictionary<SoundCue,AudioClip> clips=new Dictionary<SoundCue,AudioClip>();
        private int voiceIndex;private float nextCue;
        public static void Play(SoundCue cue){if(Instance!=null)Instance.Cue(cue);}
        private void Awake()
        {
            Instance=this;ambience=Loop("Forest",.28f);music=Loop("OathTheme",.18f);
            for(int i=0;i<voices.Length;i++){voices[i]=gameObject.AddComponent<AudioSource>();voices[i].playOnAwake=false;voices[i].priority=70;}
            foreach(SoundCue cue in System.Enum.GetValues(typeof(SoundCue)))clips[cue]=Resources.Load<AudioClip>("OathfallAudio/"+cue);
        }
        private AudioSource Loop(string name,float volume)
        {
            var source=gameObject.AddComponent<AudioSource>();source.clip=Resources.Load<AudioClip>("OathfallAudio/"+name);source.loop=true;source.volume=volume;source.priority=180;source.Play();return source;
        }
        private void Cue(SoundCue cue)
        {
            if(cue==SoundCue.Rune&&Time.unscaledTime<nextCue)return;if(cue==SoundCue.Rune)nextCue=Time.unscaledTime+.1f;
            if(!clips.TryGetValue(cue,out var clip)||clip==null)return;var source=voices[voiceIndex++%voices.Length];source.Stop();source.clip=clip;source.volume=(cue==SoundCue.Rune?.28f:.5f)*PresentationSettings.Sfx;source.pitch=1;source.Play();
        }
        private void Update(){AudioListener.volume=PresentationSettings.Master;ambience.volume=.30f*PresentationSettings.Ambience;music.volume=.18f*PresentationSettings.Music;}
        private void OnDestroy(){if(Instance==this)Instance=null;AudioListener.volume=1;}
    }
}
