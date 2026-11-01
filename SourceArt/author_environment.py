"""Owned weathered stone kit for Hollowwood. Blender 5.2, meters, UV unwrapped."""
import bpy, math, random
from pathlib import Path
from mathutils import Vector
root=Path(r'D:/Projects/Unity/Oathfall');random.seed(8142)
bpy.ops.wm.read_factory_settings(use_empty=True)
mat=bpy.data.materials.new('Oath_WeatheredStone');mat.diffuse_color=(.31,.34,.29,1)
def block(name,location,scale,rot=0):
 bpy.ops.mesh.primitive_cube_add(size=1,location=location);o=bpy.context.object;o.name=name;o.scale=scale;o.rotation_euler.z=rot;bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
 bevel=o.modifiers.new('Chipped stone edges','BEVEL');bevel.width=.06;bevel.segments=2;bpy.ops.object.modifier_apply(modifier=bevel.name)
 for v in o.data.vertices:v.co+=Vector((random.uniform(-.025,.025),random.uniform(-.025,.025),random.uniform(-.025,.025)))
 o.data.materials.append(mat);return o
# Mortared jambs, individual radial voussoirs and a pronounced keystone.
for side in [-1,1]:
 for row in range(5):block('Weathered jamb',(side*2.2,0,.29+row*.58),(1.0+random.random()*.1,1.15,.54),random.uniform(-.025,.025))
for i in range(13):
 a=i*math.pi/12;radius=2.2
 o=block('Arch voussoir', (math.cos(a)*radius,0,2.6+math.sin(a)*radius),(.63,1.15,.9));o.rotation_euler.y=a-math.pi/2
block('Keystone',(0,0,4.83),(.62,1.3,.9))
def export(name):
 bpy.ops.object.select_all(action='SELECT');bpy.ops.wm.save_as_mainfile(filepath=str(root/'SourceArt'/f'{name}.blend'));bpy.ops.export_scene.fbx(filepath=str(root/'Assets/Oathfall/Art/Models'/f'{name}.fbx'),use_selection=True,add_leaf_bones=False,bake_anim=False,axis_forward='-Z',axis_up='Y')
export('ShrineStoneArch')
bpy.ops.wm.read_factory_settings(use_empty=True)
mat=bpy.data.materials.new('Oath_WeatheredStone')
for row in range(4):
 for col in range(5-row//2):block('Broken wall',(col*.94+row%2*.35,0,.26+row*.53),(.89,.8,.48),random.uniform(-.03,.03))
export('ShrineBrokenWall')
