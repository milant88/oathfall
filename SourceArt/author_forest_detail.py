"""Owned branching oaks, grass, shrubs and wayside details; deterministic seed and UVs."""
import bpy,math,random
from pathlib import Path
from mathutils import Vector
ROOT=Path(__file__).resolve().parents[1];OUT=ROOT/'Assets/Oathfall/Art/Models'
random.seed(5149)
def mesh(name,v,f,materials,indices=None):
 d=bpy.data.meshes.new(name);d.from_pydata(v,[],f);d.update();o=bpy.data.objects.new(name,d);bpy.context.collection.objects.link(o)
 for name in materials:d.materials.append(bpy.data.materials.get(name) or bpy.data.materials.new(name))
 uv=d.uv_layers.new(name='UVMap')
 for i,p in enumerate(d.polygons):
  p.use_smooth=True;p.material_index=indices[i] if indices else 0
  for li in p.loop_indices:
   c=d.vertices[d.loops[li].vertex_index].co;uv.data[li].uv=(c.x+c.y,c.z*.5)
 return o
def tube(v,f,points,radii,sides=10):
 start=len(v)
 for i,p in enumerate(points):
  direction=(points[min(i+1,len(points)-1)]-points[max(0,i-1)]).normalized();x=direction.cross(Vector((0,1,0))).normalized();y=direction.cross(x)
  for j in range(sides):v.append(p+(x*math.cos(j*math.tau/sides)+y*math.sin(j*math.tau/sides))*radii[i])
 for i in range(len(points)-1):
  for j in range(sides):a=start+i*sides+j;b=start+i*sides+(j+1)%sides;f.append((a,b,b+sides,a+sides))
def leaf(v,f,indices,center,direction,width,length,shade):
 direction=direction.normalized();side=direction.cross(Vector((0,0,1))).normalized();n=len(v)
 v.extend([center,center+direction*length*.45-side*width,center+direction*length,center+direction*length*.45+side*width,center+direction*length*.45+Vector((0,0,.025))])
 # Upward-facing folded leaf normals. Double-sided rendering alone does not
 # correct inverted lighting on the visible upper surface.
 f.extend([(n+4,n+1,n),(n+4,n+2,n+1),(n+4,n+3,n+2),(n+4,n,n+3)]);indices.extend([shade]*4)
def export(name):
 bpy.ops.object.select_all(action='SELECT');bpy.ops.export_scene.fbx(filepath=str(OUT/(name+'.fbx')),use_selection=True,add_leaf_bones=False,bake_anim=False,axis_forward='-Z',axis_up='Y')
for variant in range(3):
 bpy.ops.wm.read_factory_settings(use_empty=True);v=[];f=[];leaves=[];lf=[];li=[]
 height=7.5+variant*.6
 trunk=[Vector((math.sin(i*.8)*.13,math.sin(i*.9)*.08,i*height/10)) for i in range(11)]
 tube(v,f,trunk,[.43*(1-i/11)**1.1+.02 for i in range(11)],14)
 for b in range(13):
  z=2.8+b*.27;angle=b*2.4+variant;length=2.7*(1-b/18);start=Vector((0,0,z));direction=Vector((math.cos(angle),math.sin(angle),.3));end=start+direction*length+Vector((0,0,.5))
  tube(v,f,[start,start+direction*length*.5+Vector((0,0,.1)),end],[.14,.08,.025])
  for twig in range(5):
   a=angle+(twig-2)*.5;origin=Vector.lerp(start,end,.35+twig*.13);d=Vector((math.cos(a),math.sin(a),.3));tip=origin+d*(.7+random.random()*.6)
   tube(v,f,[origin,tip],[.035,.008],6)
   for j in range(28):
    p=Vector.lerp(origin,tip,random.random())+Vector((random.uniform(-.55,.55),random.uniform(-.55,.55),random.uniform(-.3,.5)))
    a=random.random()*math.tau;leaf(leaves,lf,li,p,Vector((math.cos(a),math.sin(a),random.uniform(-.6,.4))),.075+random.random()*.06,.26+random.random()*.16,random.randrange(3))
 mesh('Ridged oak bark',v,f,['Oath_RootBark']);mesh('Layered oak canopy',leaves,lf,['Canopy_Shadow','Canopy_Mid','Canopy_Light'],li)
 name='SilverOak'+str(variant);bpy.ops.wm.save_as_mainfile(filepath=str(ROOT/'SourceArt'/(name+'.blend')));export(name)
 for level,ratio in [(1,.5),(2,.32)]:
  for o in bpy.context.scene.objects:
   if o.type=='MESH':
    bpy.context.view_layer.objects.active=o;m=o.modifiers.new('Distance reduction','DECIMATE');m.ratio=ratio;bpy.ops.object.modifier_apply(modifier=m.name)
  export(name+'_LOD'+str(level))
for kind in ['WoodlandGrass','HazelShrub','WaysideFlowers']:
 bpy.ops.wm.read_factory_settings(use_empty=True);v=[];f=[];ind=[]
 for i in range(80 if kind=='WoodlandGrass' else 140):
  a=random.random()*math.tau;r=random.random()*.55;origin=Vector((math.cos(a)*r,math.sin(a)*r,0));h=random.uniform(.15,.58) if kind=='WoodlandGrass' else random.uniform(.25,1.05)
  if kind=='WoodlandGrass':
   n=len(v);s=Vector((-math.sin(a),math.cos(a),0))*.012;tip=origin+Vector((math.cos(a)*.18,math.sin(a)*.18,h));v.extend([origin-s,origin+s,tip-s*.3,tip+s*.3,tip+Vector((.025,.025,.1))]);f.extend([(n,n+1,n+3,n+2),(n+2,n+3,n+4)]);ind.extend([i%3]*2)
  else:
   p=origin+Vector((0,0,h));leaf(v,f,ind,p,Vector((math.cos(a),math.sin(a),.2)),.045,.17,i%3)
   if kind=='WaysideFlowers':
    for k in range(5):
     a2=k*math.tau/5;leaf(v,f,ind,p+Vector((0,0,.05)),Vector((math.cos(a2),math.sin(a2),.1)),.023,.07,3)
 mesh(kind,v,f,['Canopy_Shadow','Canopy_Mid','Canopy_Light','Flower_Ivory'],ind);bpy.ops.wm.save_as_mainfile(filepath=str(ROOT/'SourceArt'/(kind+'.blend')));export(kind)
print('FOREST_DETAIL_EXPORT_COMPLETE')
