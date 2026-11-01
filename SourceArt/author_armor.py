"""Oathfall fitted armour study. Quaternius CC0 base; owned armour, UVs and LOD derivatives.
Run with Blender 5.2 in background. Output remains an art review candidate, not final character art.
"""
import bpy, math
from pathlib import Path
from mathutils import Vector
ROOT=Path(r'D:/Projects/Unity/Oathfall')
def material(name,color,metal=0,rough=.5):
 m=bpy.data.materials.new(name);m.diffuse_color=(*color,1);m.use_nodes=True;p=m.node_tree.nodes.get('Principled BSDF');p.inputs['Base Color'].default_value=(*color,1);p.inputs['Metallic'].default_value=metal;p.inputs['Roughness'].default_value=rough;return m
def mesh(name,verts,faces,mat,bone):
 d=bpy.data.meshes.new(name);d.from_pydata(verts,[],faces);d.update();o=bpy.data.objects.new(name,d);bpy.context.collection.objects.link(o);o.data.materials.append(mat)
 vg=o.vertex_groups.new(name=bone);vg.add(list(range(len(verts))),1,'REPLACE');mod=o.modifiers.new('Skeleton','ARMATURE');mod.object=rig
 for p in d.polygons:p.use_smooth=True
 bpy.context.view_layer.objects.active=o;o.select_set(True);bpy.ops.object.mode_set(mode='EDIT');bpy.ops.mesh.select_all(action='SELECT');bpy.ops.uv.smart_project(island_margin=.03);bpy.ops.object.mode_set(mode='OBJECT');o.select_set(False);return o
def shell(name,cx,cy,rings,mat,bone,sides=32):
 verts=[];faces=[]
 for z,rx,ry in rings:
  for i in range(sides):
   a=i*2*math.pi/sides;verts.append((cx+rx*math.cos(a),cy+ry*math.sin(a),z))
 for r in range(len(rings)-1):
  for i in range(sides):a=r*sides+i;b=r*sides+(i+1)%sides;faces.append((a,b,b+sides,a+sides))
 faces+=[tuple(reversed(range(sides))),tuple((len(rings)-1)*sides+i for i in range(sides))]
 return mesh(name,verts,faces,mat,bone)
for hero in [True,False]:
 bpy.ops.wm.read_factory_settings(use_empty=True)
 bpy.ops.import_scene.fbx(filepath=str(ROOT/'Assets/Oathfall/ThirdParty/Quaternius/Characters/Superhero_Male_FullBody.fbx'))
 rig=next(o for o in bpy.data.objects if o.type=='ARMATURE')
 steel=material('Oath_Steel' if hero else 'Raider_Iron',(.33,.39,.42) if hero else (.22,.17,.13),.8,.35)
 trim=material('Oath_Brass' if hero else 'Raider_Bronze',(.58,.35,.10) if hero else (.28,.18,.08),.7,.45)
 cloth=material('Oath_Cloth' if hero else 'Raider_Leather',(.1,.16,.15) if hero else (.15,.075,.035),0,.83)
 skin=material('Skin',(.42,.27,.18),0,.65);dark=material('Blackened_Leather',(.035,.025,.018),0,.85)
 for o in list(bpy.data.objects):
  if o.type=='MESH':
   o.data.materials.clear();o.data.materials.append(cloth);o.data.materials.append(skin)
   for p in o.data.polygons:p.material_index=1 if sum(o.data.vertices[i].co.z for i in p.vertices)/len(p.vertices)>1.58 else 0
 shell('Fitted cuirass',0,-.01,[(1.03,.17,.12),(1.09,.19,.135),(1.25,.245,.18),(1.39,.23,.16),(1.45,.16,.12)],steel if hero else cloth,'spine_03')
 for z in [1.04,1.08,1.39]:shell('Cuirass piping',0,-.01,[(z,.19 if z<1.2 else .232,.14 if z<1.2 else .163),(z+.018,.19 if z<1.2 else .232,.14 if z<1.2 else .163)],trim,'spine_03')
 shell('Waist belt',0,-.04,[(.97,.19,.13),(1.015,.19,.13)],dark,'pelvis')
 shell('Oath helm' if hero else 'Raider hood',0,-.015,[(1.60,.12,.135),(1.69,.14,.155),(1.79,.115,.125),(1.87,.015,.025)],steel if hero else cloth,'Head')
 # Open dark visor band and narrow brass brow, facing character front (-Y).
 mesh('Visor', [(-.11,-.153,1.69),(.11,-.153,1.69),(.11,-.155,1.73),(-.11,-.155,1.73)],[(0,1,2,3)],dark,'Head')
 for side in [-1,1]:
  bone='upperarm_l' if side<0 else 'upperarm_r'
  shell('Layered pauldron',side*.30,0,[(1.36,.115,.14),(1.48,.155,.17),(1.56,.10,.12)],steel,bone,24)
  shell('Pauldron rim',side*.30,0,[(1.36,.118,.144),(1.38,.13,.152)],trim,bone,24)
  calf='calf_l' if side<0 else 'calf_r'
  shell('Shin armour',side*.115,-.02,[(.12,.065,.075),(.26,.079,.08),(.48,.08,.075)],steel,calf,24)
  shell('Knee band',side*.115,-.02,[(.45,.085,.08),(.49,.08,.08)],trim,calf,24)
 # A short double-sided cloth cape with UVs, weighted to the torso.
 verts=[]
 for j in range(9):
  z=1.45-j*.08;w=.18+j*.014
  for i in range(9):x=(i/8*2-1)*w;verts.append((x,.17+(.025*math.cos(i/8*math.pi*6))+.025*j,z))
 faces=[(j*9+i,j*9+i+1,(j+1)*9+i+1,(j+1)*9+i) for j in range(8) for i in range(8)]
 cape=mesh('Oath mantle',verts,faces,cloth,'spine_03');solid=cape.modifiers.new('Woven thickness','SOLIDIFY');solid.thickness=.006
 hx,hy,hz=rig.matrix_world@rig.data.bones['hand_r'].head_local
 verts=[(hx-.04,hy,hz-.12),(hx+.04,hy,hz-.12),(hx-.045,hy,hz+.7),(hx+.045,hy,hz+.7),(hx,hy,hz+.95),(hx,hy-.015,hz+.3),(hx,hy+.015,hz+.3)]
 mesh('Forged blade',verts,[(0,1,5),(0,5,2),(2,5,4),(4,5,3),(3,5,1),(0,6,1),(0,2,6),(2,4,6),(4,3,6),(3,1,6)],steel,'hand_r')
 shell('Sword grip',hx,hy,[(hz-.18,.025,.025),(hz,.025,.025)],dark,'hand_r',12)
 shell('Sword crossguard',hx,hy,[(hz,.16,.03),(hz+.035,.16,.03)],trim,'hand_r',12)
 if hero:
  hx,hy,hz=rig.matrix_world@rig.data.bones['hand_l'].head_local
  v=[(hx-.27,hy-.12,hz+.25),(hx+.27,hy-.12,hz+.25),(hx+.24,hy-.12,hz-.13),(hx,hy-.12,hz-.43),(hx-.24,hy-.12,hz-.13),(hx,hy-.22,hz)]
  mesh('Oath kite shield',v,[(0,1,5),(1,2,5),(2,3,5),(3,4,5),(4,0,5)],steel,'hand_l')
 name='Dawnwarden' if hero else 'HollowwoodRaider'
 bpy.ops.object.select_all(action='SELECT');bpy.context.view_layer.objects.active=rig
 bpy.ops.wm.save_as_mainfile(filepath=str(ROOT/'SourceArt'/f'{name}.blend'))
 bpy.ops.export_scene.fbx(filepath=str(ROOT/'Assets/Oathfall/Art/Models'/f'{name}.fbx'),use_selection=True,add_leaf_bones=False,bake_anim=False,axis_forward='-Z',axis_up='Y',use_mesh_modifiers=True)
 print('AUTHORED',name)
