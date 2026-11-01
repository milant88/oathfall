#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using UnityEngine;
namespace Oathfall.Gameplay
{
    /// <summary>Opt-in Unity mix capture, no microphone or system-audio access.</summary>
    public sealed class RuntimeAudioCapture:MonoBehaviour
    {
        private float[] samples;private int count,channels=2,rate;private volatile bool finished;
        private readonly object captureLock=new object();
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void StartCapture(){if(Array.IndexOf(Environment.GetCommandLineArgs(),"-oathfall-audio-record")>=0)new GameObject("Audio capture bootstrap").AddComponent<AudioCaptureBootstrap>();}
        public void Initialize(){rate=AudioSettings.outputSampleRate;samples=new float[rate*2*65];}
        private void OnAudioFilterRead(float[] data,int channelCount)
        {
            lock(captureLock){if(samples==null||finished)return;channels=channelCount;int length=Math.Min(data.Length,samples.Length-count);Array.Copy(data,0,samples,count,length);count+=length;if(count==samples.Length)finished=true;}
        }
        private void Update(){if(finished&&samples!=null)Save();}
        private void Save()
        {
            float[] data;lock(captureLock){finished=true;data=samples;samples=null;}if(data==null)return;
            string file=Path.Combine(Application.persistentDataPath,"Audit","unity-mix.wav");Directory.CreateDirectory(Path.GetDirectoryName(file));
            using(var writer=new BinaryWriter(File.Create(file))){writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));writer.Write(36+count*2);writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt "));writer.Write(16);writer.Write((short)1);writer.Write((short)channels);writer.Write(rate);writer.Write(rate*channels*2);writer.Write((short)(channels*2));writer.Write((short)16);writer.Write(System.Text.Encoding.ASCII.GetBytes("data"));writer.Write(count*2);for(int i=0;i<count;i++)writer.Write((short)(Mathf.Clamp(data[i],-1,1)*32767));}Debug.Log("AUDIO capture complete: "+file);
        }
        private void OnDestroy(){finished=true;if(samples!=null&&count>0)Save();}
    }
    public sealed class AudioCaptureBootstrap:MonoBehaviour
    {
        private IEnumerator Start(){yield return null;var listener=FindFirstObjectByType<AudioListener>();if(listener!=null)listener.gameObject.AddComponent<RuntimeAudioCapture>().Initialize();Destroy(gameObject);}
    }
}
#endif
