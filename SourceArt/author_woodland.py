"""Owned woodland silhouettes. Reproducible Blender source; meters, UVs, no vendor edits."""
import bpy, math, random
from pathlib import Path
from mathutils import Vector, noise

ROOT=Path(__file__).resolve().parents[1]
OUT=ROOT/'Assets/Oathfall/Art/Models'
random.seed(2718)
bpy.ops.wm.read_factory_settings(use_empty=True)

def material(name):
    return bpy.data.materials.get(name) or bpy.data.materials.new(name)

def export(obj,name):
    bpy.ops.object.select_all(action='DESELECT');obj.select_set(True);bpy.context.view_layer.objects.active=obj
    bpy.ops.export_scene.fbx(filepath=str(OUT/(name+'.fbx')),use_selection=True,add_leaf_bones=False,bake_anim=False,axis_forward='-Z',axis_up='Y')

for variant in range(3):
    verts=[];faces=[]
    def leaf(a,b,width):
        along=b-a;side=Vector((-along.y,along.x,0)).normalized()*width
        center=a+along*.42+Vector((0,0,.012));n=len(verts)
        verts.extend([a,center+side,b,center-side,center+Vector((0,0,.025))])
        faces.extend([(n+4,n+1,n),(n+4,n+2,n+1),(n+4,n+3,n+2),(n+4,n,n+3)])
    for frond in range(9):
        angle=frond*math.tau/9+random.uniform(-.2,.2)
        outward=Vector((math.cos(angle),math.sin(angle),0));side=Vector((-outward.y,outward.x,0))
        length=random.uniform(.7,1.1)
        for segment in range(1,15):
            t=segment/15;origin=outward*(t*length)+Vector((0,0,.08+.5*math.sin(t*math.pi*.8)))
            extent=.23*math.sin(t*math.pi)**.7
            for sign in [-1,1]:
                end=origin+side*(extent*sign)+outward*.11+Vector((0,0,-.025))
                leaf(origin,end,.035*(1-t*.6))
    mesh=bpy.data.meshes.new('Fern fronds');mesh.from_pydata(verts,[],faces);mesh.update()
    obj=bpy.data.objects.new('WoodlandFern'+str(variant),mesh);bpy.context.collection.objects.link(obj);mesh.materials.append(material('Oath_Fern'))
    uv=mesh.uv_layers.new(name='UVMap')
    for poly in mesh.polygons:
        for li in poly.loop_indices:
            co=mesh.vertices[mesh.loops[li].vertex_index].co;uv.data[li].uv=(co.x*.5+.5,co.y*.5+.5)
    export(obj,obj.name)

for variant in range(3):
    bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=3,radius=1)
    obj=bpy.context.object;obj.name='WoodlandBoulder'+str(variant)
    for v in obj.data.vertices:
        factor=1+noise.noise_vector(v.co*2.3+Vector((variant*11,3,7))).x*.23
        v.co=Vector((v.co.x*factor*1.3,v.co.y*factor*.9,max(-.4,v.co.z*factor*.8)))
    obj.data.materials.append(material('Oath_ForestRock'))
    for p in obj.data.polygons:p.use_smooth=True
    bpy.ops.object.mode_set(mode='EDIT');bpy.ops.mesh.select_all(action='SELECT');bpy.ops.uv.smart_project(island_margin=.02);bpy.ops.object.mode_set(mode='OBJECT')
    export(obj,obj.name)

# Exposed roots/log: curved tapered tubes with actual cylindrical UVs.
verts=[];faces=[];uvs=[]
for branch in range(5):
    direction=Vector((math.cos(branch*1.1),math.sin(branch*1.1),0))
    for ring in range(13):
        t=ring/12;center=direction*t*(2.8 if branch==0 else 1.7)+Vector((0,0,.36*(1-t)**2))
        radius=(.22 if branch==0 else .13)*(1-t*.88)
        for side in range(10):
            a=side*math.tau/10;co=center+Vector((-direction.y*math.cos(a)*radius,direction.x*math.cos(a)*radius,math.sin(a)*radius))
            verts.append(co);uvs.append((side/9,t*2))
            if ring<12:
                n=branch*130+ring*10+side;nextn=branch*130+ring*10+(side+1)%10
                faces.append((n,nextn,nextn+10,n+10))
mesh=bpy.data.meshes.new('Root network');mesh.from_pydata(verts,[],faces);mesh.update();obj=bpy.data.objects.new('WoodlandRoots',mesh);bpy.context.collection.objects.link(obj);mesh.materials.append(material('Oath_RootBark'));uv=mesh.uv_layers.new(name='UVMap')
for poly in mesh.polygons:
    poly.use_smooth=True
    for li in poly.loop_indices:uv.data[li].uv=uvs[mesh.loops[li].vertex_index]
export(obj,obj.name)
bpy.ops.wm.save_as_mainfile(filepath=str(ROOT/'SourceArt/WoodlandKit.blend'))
