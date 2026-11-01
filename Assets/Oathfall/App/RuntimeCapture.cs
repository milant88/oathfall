#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
namespace Oathfall.Gameplay
{
    /// <summary>Opt-in actual framebuffer capture. Exclude this run from performance evidence.</summary>
    public sealed class RuntimeCapture:MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoStart(){if(Environment.GetCommandLineArgs().Contains("-oathfall-capture"))new GameObject("Gameplay capture").AddComponent<RuntimeCapture>();}
        private IEnumerator Start()
        {
            string folder=Path.Combine(Application.persistentDataPath,"Capture-"+DateTime.UtcNow.ToString("yyyyMMdd-HHmmss"));Directory.CreateDirectory(folder);
            using(var manifest=new StreamWriter(Path.Combine(folder,"frames.txt")))
            {
                manifest.WriteLine("ffconcat version 1.0");bool full=Environment.GetCommandLineArgs().Contains("-oathfall-capture-full");float start=Time.unscaledTime,next=start,previous=start;string last=null;int frame=0;bool sawRun=false;
                while(Time.unscaledTime-start<(full?1200:65))
                {
                    yield return new WaitForEndOfFrame();if(Time.unscaledTime<next)continue;float now=Time.unscaledTime;float interval=full&&now-start>=65?.2f:(1f/30f);next+=interval;if(now-next>interval)next=now+interval;
                    if(full&&OathfallGame.Instance!=null){if(OathfallGame.Instance.RunActive)sawRun=true;else if(sawRun)break;}
                    if(last!=null){manifest.WriteLine("file '"+last+"'");manifest.WriteLine("duration "+(now-previous).ToString("F6",System.Globalization.CultureInfo.InvariantCulture));}
                    var texture=ScreenCapture.CaptureScreenshotAsTexture();last="frame-"+(frame++).ToString("D5")+".jpg";File.WriteAllBytes(Path.Combine(folder,last),texture.EncodeToJPG(82));Destroy(texture);previous=now;
                }
                if(last!=null){manifest.WriteLine("file '"+last+"'");manifest.WriteLine("duration 0.1");manifest.WriteLine("file '"+last+"'");}
            }
            Debug.Log("CAPTURE complete: "+folder);
        }
    }
}
#endif
