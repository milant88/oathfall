"""Original restrained modal music bed and rune/arrival cues. No sampled third-party music."""
import math,wave,struct,random
from pathlib import Path
OUT=Path(__file__).resolve().parents[1]/'Assets/Oathfall/Art/Resources/OathfallAudio'
rate=22050;random.seed(513)
def write(name,duration,sample):
 data=bytearray()
 for i in range(int(rate*duration)):
  t=i/rate;v=sample(t);envelope=min(1,t/.06,(duration-t)/.12);v=max(-.85,min(.85,v*envelope));data.extend(struct.pack('<h',int(v*32767)))
 with wave.open(str(OUT/(name+'.wav')),'wb') as f:f.setnchannels(1);f.setsampwidth(2);f.setframerate(rate);f.writeframes(data)
def note(f,t):return math.sin(t*math.tau*f)+.22*math.sin(t*math.tau*f*2)+.08*math.sin(t*math.tau*f*3)
def music(t):
 chords=[(82.4069,123.4708,164.8138),(73.4162,110,146.8324),(65.4064,97.9989,130.8128),(73.4162,110,164.8138)]
 c=chords[min(3,int(t/12))];q=t%12;env=math.sin(math.pi*q/12)**.6
 pad=sum(note(f,t)*(1+.04*math.sin(t*1.7+k)) for k,f in enumerate(c))*.033*env
 bellTime=t%3;pitch=c[int(q/3)%3]*4
 bell=math.sin(math.pi*min(bellTime/.04,1)) if bellTime<.04 else 1
 return pad+note(pitch,t)*math.exp(-bellTime*1.9)*.035*bell
write('OathTheme',48,music)
write('Rune',1.0,lambda t:(math.sin(math.tau*(180*t+100*t*t))*.2+random.uniform(-1,1)*.08)*math.exp(-t*3))
write('Victory',3.4,lambda t:sum(note(f,t)*math.exp(-max(0,t-j*.3)*2)*(t>=j*.3) for j,f in enumerate([329.63,392,493.88,659.25]))*.11)
write('Ward',1.8,lambda t:sum(note(f,t) for f in [196,293.66,392])*.07*math.exp(-t*2))
print('Original soundscape generated')
