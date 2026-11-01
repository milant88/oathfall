"""Owned mesh additions and gear studies. CC0 humanoid base remains separately licensed.
Meters, named bone weights, UVs, bevels; exports reviewed in the Unity player.
"""
import bpy, bmesh, math, random
from pathlib import Path
from mathutils import Vector
ROOT=Path(__file__).resolve().parents[1]
OUT=ROOT/'Assets/Oathfall/Art/Models'
rig=None

def mat(name):
 return bpy.data.materials.get(name) or bpy.data.materials.new(name)

def mesh(name,verts,faces,material,bone=None):
 d=bpy.data.meshes.new(name);d.from_pydata(verts,[],faces);d.update()
 bm=bmesh.new();bm.from_mesh(d);bmesh.ops.recalc_face_normals(bm,faces=list(bm.faces));bm.to_mesh(d);bm.free()
 o=bpy.data.objects.new(name,d);bpy.context.collection.objects.link(o);d.materials.append(mat(material))
 if bone:
  g=o.vertex_groups.new(name=bone);g.add(list(range(len(verts))),1,'REPLACE');m=o.modifiers.new('Deformation','ARMATURE');m.object=rig
 for p in d.polygons:p.use_smooth=True
 bpy.ops.object.select_all(action='DESELECT');o.select_set(True);bpy.context.view_layer.objects.active=o
 bpy.ops.object.mode_set(mode='EDIT');bpy.ops.mesh.select_all(action='SELECT');bpy.ops.uv.smart_project(island_margin=.025);bpy.ops.object.mode_set(mode='OBJECT')
 return o

def rings(name,center,sections,material,bone=None,sides=32):
 v=[];f=[]
 for z,rx,ry in sections:
  for i in range(sides):
   a=i*math.tau/sides;v.append((center[0]+math.cos(a)*rx,center[1]+math.sin(a)*ry,z))
 for j in range(len(sections)-1):
  for i in range(sides):a=j*sides+i;b=j*sides+(i+1)%sides;f.append((a,b,b+sides,a+sides))
 f.extend([tuple(reversed(range(sides))),tuple((len(sections)-1)*sides+i for i in range(sides))])
 return mesh(name,v,f,material,bone)

def box(name,center,size,material,bone=None,bevel=.015):
 bpy.ops.mesh.primitive_cube_add(size=1,location=center);o=bpy.context.object;o.name=name;o.scale=size
 bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
 b=o.modifiers.new('Forged edges','BEVEL');b.width=bevel;b.segments=3;bpy.ops.object.modifier_apply(modifier=b.name)
 bpy.ops.object.transform_apply(location=True,rotation=False,scale=False)
 o.data.materials.append(mat(material))
 if bone:
  g=o.vertex_groups.new(name=bone);g.add(list(range(len(o.data.vertices))),1,'REPLACE');m=o.modifiers.new('Deformation','ARMATURE');m.object=rig
 bpy.ops.object.mode_set(mode='EDIT');bpy.ops.mesh.select_all(action='SELECT');bpy.ops.uv.smart_project(island_margin=.02);bpy.ops.object.mode_set(mode='OBJECT')
 return o

def export(name,selection=None):
 bpy.ops.object.select_all(action='DESELECT')
 for o in selection or list(bpy.context.scene.objects):o.select_set(True)
 bpy.ops.export_scene.fbx(filepath=str(OUT/(name+'.fbx')),use_selection=True,add_leaf_bones=False,bake_anim=False,axis_forward='-Z',axis_up='Y',use_mesh_modifiers=True)

def hide(names):
 for o in list(bpy.data.objects):
  if any(n in o.name for n in names):bpy.data.objects.remove(o,do_unlink=True)

for role in ['Raider','Hexblade','Breaker','Guardian']:
 bpy.ops.wm.open_mainfile(filepath=str(ROOT/'SourceArt/HollowwoodRaider.blend'))
 rig=next(o for o in bpy.data.objects if o.type=='ARMATURE')
 hide(['Oath mantle','Raider hood','Visor'])
 if role=='Raider':
  hide(['Layered pauldron','Pauldron rim'])
  rings('Ragged cowl',(0,.015),[(1.55,.14,.13),(1.7,.17,.15),(1.85,.12,.13),(1.92,.025,.05)],'Raider_Leather','Head')
  rings('Single scavenged shoulder',(-.31,0),[(1.36,.12,.14),(1.48,.18,.19),(1.58,.10,.13)],'Raider_Iron','upperarm_l')
  for i in range(5):box('Cross body strap',(-.15+i*.07,-.175,1.39-i*.067),(.09,.025,.075),'Blackened_Leather','spine_03')
  box('Provision pouch',(.21,-.02,.98),(.14,.16,.2),'Raider_Leather','pelvis',.03)
 elif role=='Hexblade':
  hide(['Layered pauldron','Pauldron rim','Shin armour','Knee band','Fitted cuirass','Cuirass piping','Forged blade','Sword grip','Sword crossguard'])
  rings('Seer hood',(0,.02),[(1.52,.16,.15),(1.69,.19,.18),(1.86,.14,.14),(2.04,.02,.05)],'Hex_Cloth','Head')
  # Split robe panels retain leg ownership rather than a rigid floor-length cone.
  for side in [-1,1]:
   rings('Split ritual skirt',(.11*side,.025),[(.23,.17,.18),(.60,.155,.16),(.99,.13,.14)],'Hex_Cloth','thigh_l' if side<0 else 'thigh_r')
  rings('Ritual collar',(0,0),[(1.35,.25,.18),(1.46,.29,.21),(1.51,.18,.15)],'Hex_Cloth','spine_03')
  for z in [1.03,1.12,1.21]:box('Seer clasp',(0,-.19,z),(.07,.03,.04),'Oath_Brass','spine_03')
  h=rig.matrix_world@rig.data.bones['hand_r'].head_local
  rings('Ritual staff',(h.x,h.y),[(h.z-.65,.026,.026),(h.z+.72,.023,.023)],'Blackened_Leather','hand_r')
  rings('Forged blade',(h.x,h.y),[(h.z+.72,.09,.065),(h.z+.85,.13,.08),(h.z+1.04,.025,.025)],'Hex_Gem','hand_r',12)
 elif role=='Breaker':
  hide(['Oath helm','Fitted cuirass','Cuirass piping'])
  rings('Iron bucket helm',(0,-.015),[(1.57,.14,.16),(1.81,.15,.17),(1.87,.09,.12)],'Raider_Iron','Head')
  box('Barred visor',(0,-.187,1.71),(.23,.025,.06),'Blackened_Leather','Head')
  for x in [-.08,-.04,0,.04,.08]:box('Visor bars',(x,-.204,1.71),(.009,.012,.065),'Oath_Brass','Head',.003)
  for i in range(4):rings('Overlapping lamellar',(0,-.025),[(1.04+i*.085,.235+i*.012,.17),(1.13+i*.085,.24+i*.012,.18)],'Raider_Iron','spine_03')
  for side in [-1,1]:
   rings('Broad segmented shoulder',(side*.31,0),[(1.33,.15,.18),(1.46,.22,.23),(1.6,.12,.16)],'Raider_Iron','upperarm_l' if side<0 else 'upperarm_r')
  h=rig.matrix_world@rig.data.bones['hand_r'].head_local
  box('Breaker hammer',(h.x,h.y,h.z+.69),(.40,.22,.23),'Raider_Iron','hand_r',.035)
  hide(['Forged blade'])
  rings('Forged blade',(h.x,h.y),[(h.z-.1,.027,.027),(h.z+.78,.027,.027)],'Blackened_Leather','hand_r',16)
 else:
  rings('Crown helm',(0,-.01),[(1.59,.13,.15),(1.78,.15,.17),(1.87,.10,.12)],'Guardian_Stone','Head')
  for i in range(7):
   a=i*math.tau/7;x=.145*math.cos(a);y=.16*math.sin(a)
   rings('Broken crown tine',(x,y),[(1.80,.035,.035),(2.02+(i%3)*.05,.018,.018),(2.10+(i%3)*.05,.002,.002)],'Oath_Brass','Head',8)
  for side in [-1,1]:
   rings('Carved sentinel shoulder',(side*.31,0),[(1.31,.15,.19),(1.48,.23,.24),(1.65,.13,.16)],'Guardian_Stone','upperarm_l' if side<0 else 'upperarm_r')
  for i in range(5):box('Chest oath bands',(0,-.205,1.08+i*.07),(.3-i*.015,.035,.022),'Oath_Brass','spine_03',.005)
  rings('Crown mantle',(0,.03),[(.63,.3,.17),(1.05,.25,.17),(1.43,.23,.18)],'Hex_Cloth','spine_03')
  box('Forehead seal',(0,-.184,1.76),(.07,.035,.13),'Hex_Gem','Head')
 name='Enemy'+role
 bpy.ops.wm.save_as_mainfile(filepath=str(ROOT/'SourceArt'/f'{name}.blend'));export(name)
 # Each LOD is reduced from the previous source, keeping named deforming mesh parts.
 for level,ratio in [(1,.55),(2,.5)]:
  for o in list(bpy.data.objects):
   if o.type!='MESH':continue
   bpy.context.view_layer.objects.active=o
   for mod in list(o.modifiers):
    if mod.type!='ARMATURE':bpy.ops.object.modifier_apply(modifier=mod.name)
   mod=o.modifiers.new('LOD reduction','DECIMATE');mod.ratio=ratio;bpy.ops.object.modifier_apply(modifier=mod.name)
  export(name+'_LOD'+str(level))

# Six standalone display/drop items; geometry is independent of a character's pose.
for slot in ['Weapon','Offhand','Head','Body','Boots','Charm']:
 bpy.ops.wm.read_factory_settings(use_empty=True);rig=None
 if slot=='Weapon':
  mesh('Tempered fuller blade',[(-.055,0,.23),(.055,0,.23),(-.065,0,1.05),(.065,0,1.05),(0,0,1.30),(0,-.025,.7),(0,.025,.7)],[(0,1,5),(0,5,2),(2,5,4),(4,5,3),(3,5,1),(0,6,1),(0,2,6),(2,4,6),(4,3,6),(3,1,6)],'Gear_Steel')
  rings('Bound leather grip',(0,0),[(0,.032,.032),(.23,.032,.032)],'Blackened_Leather',sides=20)
  for z in [.025+i*.025 for i in range(8)]:rings('Grip binding',(0,0),[(z,.034,.034),(z+.005,.034,.034)],'Gear_Brass',sides=16)
  box('Swept crossguard',(0,0,.23),(.34,.06,.045),'Gear_Brass')
  rings('Pommel',(0,0),[(-.09,.01,.01),(-.06,.06,.045),(-.01,.035,.035)],'Gear_Brass')
 elif slot=='Offhand':
  v=[(-.34,0,.85),(.34,0,.85),(.30,0,.35),(0,0,0),(-.30,0,.35),(0,-.12,.45)]
  o=mesh('Convex kite shield',v,[(0,1,5),(1,2,5),(2,3,5),(3,4,5),(4,0,5)],'Gear_Steel');m=o.modifiers.new('Shield thickness','SOLIDIFY');m.thickness=.025
  box('Oath cross',(0,-.13,.5),(.36,.028,.045),'Gear_Brass');box('Oath spine',(0,-.135,.5),(.045,.028,.48),'Gear_Brass')
  for a in [-1,1]:box('Leather arm strap',(a*.16,.055,.47),(.045,.045,.32),'Blackened_Leather')
 elif slot=='Head':
  rings('Warden helm',(0,0),[(0,.18,.18),(.25,.205,.22),(.40,.13,.15),(.47,.02,.03)],'Gear_Steel')
  box('Dark visor',(0,-.22,.23),(.30,.025,.07),'Blackened_Leather')
  box('Nasal guard',(0,-.24,.19),(.04,.035,.23),'Gear_Brass')
  rings('Helm brow',(0,0),[(.27,.21,.225),(.3,.205,.22)],'Gear_Brass')
 elif slot=='Body':
  rings('Forged breastplate',(0,0),[(0,.23,.15),(.17,.25,.18),(.44,.31,.22),(.62,.29,.19),(.72,.19,.13)],'Gear_Steel')
  for z in [.06,.13,.57]:rings('Embossed ribs',(0,0),[(z,.26 if z<.2 else .30,.19 if z<.2 else .205),(z+.018,.26 if z<.2 else .30,.19 if z<.2 else .205)],'Gear_Brass')
  box('Heraldic inlay',(0,-.225,.40),(.065,.025,.23),'Gear_Brass')
  for a in [-1,1]:box('Leather side fastening',(a*.25,-.12,.2),(.08,.07,.08),'Blackened_Leather')
 elif slot=='Boots':
  for a in [-1,1]:
   rings('Articulated greave',(a*.15,0),[(.08,.10,.10),(.32,.10,.09),(.59,.12,.105)],'Gear_Steel')
   box('Leather boot',(a*.15,-.07,.055),(.20,.35,.11),'Blackened_Leather',bevel=.035)
   for z in [.19,.44]:rings('Greave strap',(a*.15,0),[(z,.106,.106),(z+.035,.106,.106)],'Gear_Brass')
 else:
  rings('Oath pendant',(0,0),[(0,.015,.025),(.12,.16,.055),(.26,.12,.055),(.34,.015,.025)],'Gear_Brass',sides=12)
  rings('Cut moonstone',(0,-.045),[(.07,.02,.025),(.16,.09,.055),(.25,.02,.025)],'Hex_Gem',sides=8)
  for i in range(10):
   z=.34+i*.035;rings('Chain link',(math.sin(i*.45)*.025,0),[(z,.025,.012),(z+.025,.025,.012)],'Gear_Brass',sides=12)
 bpy.ops.wm.save_as_mainfile(filepath=str(ROOT/'SourceArt'/('Gear'+slot+'.blend')));export('Gear'+slot)
print('REVISION5_MODEL_EXPORT_COMPLETE')
