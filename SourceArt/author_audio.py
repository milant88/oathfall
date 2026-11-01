import wave,struct,math,random
from pathlib import Path
out=Path('Assets/Oathfall/Art/Audio');out.mkdir(parents=True,exist_ok=True)
random.seed(8142)
for name,duration in [('SwordSwing',.25),('SteelImpact',.32),('Footstep',.14),('ShieldBlock',.4)]:
 rate=22050;data=[];filtered=0
 for i in range(int(rate*duration)):
  t=i/rate;n=random.uniform(-1,1);filtered=.7*filtered+.3*n
  if name=='SwordSwing':v=filtered*math.sin(math.pi*t/duration)**2*.7
  elif name=='Footstep':v=filtered*math.exp(-t*35)*.8
  else:v=(filtered*.35+math.sin(t*2*math.pi*847)*.2+math.sin(t*2*math.pi*1373)*.13)*math.exp(-t*18)
  data.append(struct.pack('<h',int(max(-1,min(1,v))*32767)))
 with wave.open(str(out/(name+'.wav')),'wb') as f:f.setnchannels(1);f.setsampwidth(2);f.setframerate(rate);f.writeframes(b''.join(data))
