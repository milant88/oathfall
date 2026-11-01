import bpy
from pathlib import Path
root=Path(r'D:/Projects/Unity/Oathfall')
for name in ['Dawnwarden','HollowwoodRaider']:
 for level,ratio in [(1,.5),(2,.25)]:
  bpy.ops.wm.open_mainfile(filepath=str(root/'SourceArt'/f'{name}.blend'))
  for o in list(bpy.data.objects):
   if o.type!='MESH':continue
   bpy.context.view_layer.objects.active=o
   for mod in list(o.modifiers):
    if mod.type!='ARMATURE':bpy.ops.object.modifier_apply(modifier=mod.name)
   mod=o.modifiers.new('Distance mesh reduction','DECIMATE');mod.ratio=ratio;bpy.ops.object.modifier_apply(modifier=mod.name)
  bpy.ops.object.select_all(action='SELECT');bpy.ops.export_scene.fbx(filepath=str(root/'Assets/Oathfall/Art/Models'/f'{name}_LOD{level}.fbx'),use_selection=True,add_leaf_bones=False,bake_anim=False,axis_forward='-Z',axis_up='Y')
