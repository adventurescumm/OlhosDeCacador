#!/usr/bin/env python3
"""Validate source content and original asset hashes without Unity. Python standard library only."""
from pathlib import Path
import json,hashlib,sys,re
root=Path(__file__).resolve().parents[1]
LIT_SHADER_GUID='933532a4fcc9baf4fa0491de14d08ed7'
catalog=json.loads((root/'Assets/Resources/Hunter/catalog.json').read_text(encoding='utf-8'))
manifest=json.loads((root/'Docs/asset-manifest.json').read_text(encoding='utf-8'))
ids=[];roomids=[];models=set();instances=0
assert len(catalog['maps'])==3
for m in catalog['maps']:
 assert len(m['rooms'])==3
 targets=[p for r in m['rooms'] for p in r['objects'] if p['id']]
 assert len(targets)==18
 assert len([p for p in targets if p['clue']])==3
 assert m['puzzle'] in ['lock','memory','matching']
 for r in m['rooms']:
  assert r['id'] and len(r['riddle'].splitlines())==6
  roomids.append(r['id'])
  assert len([p for p in r['objects'] if p['id']])==6
  for target in (p for p in r['objects'] if p['id']):assert sum(p['asset']==target['asset'] and not p.get('showAfter') for p in r['objects'])==1,(r['id'],target['id'],'indistinguishable duplicate')
  for p in r['objects']:
   instances+=1;models.add(p['asset'])
   assert 0<p['size']<=6 and abs(p['x'])<r['width']/2 and abs(p['z'])<r['depth']/2-.1
   obj=root/'Assets/Resources/Hunter/Models'/(p['asset']+'.obj');assert obj.is_file() or obj.with_suffix('.fbx').is_file(),str(obj)
   if obj.is_file():
    mtl=obj.with_suffix('.mtl');assert mtl.is_file()
    for line in mtl.read_text().splitlines():
     if line.startswith('map_Kd '):assert (mtl.parent/line[7:].strip()).is_file()
   if p['id']:ids.append(p['id']);assert p['label'] and p['hint']
assert len(ids)==len(set(ids))==54
assert len(roomids)==len(set(roomids))==9
assert {p['destination'] for p in catalog['hub']['objects'] if p.get('action')=='world'}=={m['id'] for m in catalog['maps']}
for m in catalog['maps']:
 stages=m['rooms']+[m['junction'],m['challenge']]
 all_ids={r['id'] for r in stages}|{'quarto'}
 edges={r['id']:{p['destination'] for p in r['objects'] if p.get('action')=='exit'} for r in stages}
 for r in stages:
  assert edges[r['id']] and edges[r['id']]<=all_ids,(r['id'],'invalid physical passage')
  assert not any(p.get('action') in ['travel','junction','puzzle'] for p in r['objects'])
  for destination in edges[r['id']]-{'quarto'}:assert r['id'] in edges[destination],(r['id'],destination,'one-way dead end')
 for r in m['rooms']:assert len(edges[r['id']])>=2
 assert 'quarto' in edges[m['junction']['id']]
 assert len(m['gates'])==2 and {g['sourceRoom'] for g in m['gates']}=={0,1}
 assert len({g['id'] for g in m['gates']})==2
 for g in m['gates']:
  assert g['roomId']==m['rooms'][g['sourceRoom']+1]['id']
  assert any(p['id']==g['tokenId'] for p in m['rooms'][g['sourceRoom']]['objects'])
  assert sum(p.get('action')=='gate' and p.get('destination')==g['id'] for r in m['rooms'] for p in r['objects'])==1
 # Connected cycle once the mechanisms work; initial access contains just arrival + first search.
 def reached(blocked):
  seen={m['junction']['id']};todo=list(seen)
  while todo:
   for target in edges[todo.pop()]-blocked-{'quarto'}:
    if target not in seen:seen.add(target);todo.append(target)
  return seen
 assert reached(set())==set(edges)
 assert reached({g['roomId'] for g in m['gates']}|{m['challenge']['id']})=={m['junction']['id'],m['rooms'][0]['id']}
 puzzle=m['challenge']
 for action in {'lock':['digit'],'memory':['signal'],'matching':['supply','destination']}[m['puzzle']]:
  assert sorted(p['value'] for p in puzzle['objects'] if p.get('action')==action)==list(range(4 if action=='signal' else 3)),(m['id'],action)
 if m['puzzle']=='lock':assert sum(p.get('action')=='unlock' for p in puzzle['objects'])==1
all_stages=[r for m in catalog['maps'] for r in m['rooms']+[m['junction'],m['challenge']]]+[catalog['hub']]
for stage in all_stages:
 models.update(p['asset'] for p in stage['objects'])
 assert len(stage['camera'])==len(stage['lookAt'])==3 and 20<=stage['fieldOfView']<=60,stage['id']
 for prop in stage['objects']:
  model_path=root/'Assets/Resources/Hunter/Models'/prop['asset']
  assert model_path.with_suffix('.obj').is_file() or model_path.with_suffix('.fbx').is_file(),prop['asset']
  assert prop.get('action','') in ['', 'exit', 'gate', 'inspect', 'world', 'digit', 'unlock', 'signal', 'supply', 'destination', 'celebrate']
  if prop['id']:assert prop.get('reaction') in ['hop','tip','spin','launch']
for asset in manifest['files']:
 path=root/asset['file'];assert path.is_file(),str(path)
 assert hashlib.sha256(path.read_bytes()).hexdigest()==asset['sha256'],str(path)+' changed'
 if path.suffix in ['.obj','.fbx','.ogg']:
  kind,minimum=('ModelImporter',25) if path.suffix in ['.obj','.fbx'] else ('AudioImporter',6)
  meta=Path(str(path)+'.meta').read_text()
  block=re.search(r'^'+kind+r':\n((?:[ \t].*\n|\n)*)',meta,re.M)
  assert block,str(path)+' importer missing'
  version=re.search(r'^  serializedVersion: (\d+)$',block.group(1),re.M)
  assert version and int(version.group(1))>=minimum,str(path)+' unsupported importer version'
  if kind=='ModelImporter':
   assert all('  '+section+':\n' in meta for section in ['materials','animations','meshes','humanDescription']),str(path)+' incomplete ModelImporter'
   assert len(re.findall(r'^  serializedVersion:',meta,re.M))==1
script_meta=(root/'Assets/Hunter/Runtime/HunterGame.cs.meta').read_text()
guid=re.search(r'guid: ([a-f0-9]+)',script_meta).group(1)
assert guid in (root/'Assets/Hunter/Scenes/Main.unity').read_text()
assert all(any((root/'Assets/ThirdPartyLicenses'/p['pack']/name).is_file() for name in ['License.txt','OFL.txt']) for p in manifest['packs'])
# Every source material must map to a persistent Unity material with the original Kd and texture.
bindings=json.loads((root/'Assets/Resources/Hunter/material-bindings.json').read_text())['materials']
keys=[(b['model'],b['name']) for b in bindings];assert len(keys)==len(set(keys))
uv_tiling_models=[]
for model in sorted(models):
 obj=root/'Assets/Resources/Hunter/Models'/(model+'.obj')
 if not obj.is_file():
  assert obj.with_suffix('.fbx').is_file()
  assert any(b['model']==model and b['texture'].endswith('.jpg') for b in bindings)
  continue
 source=obj.read_text()
 used=set(re.findall(r'^usemtl (.+)$',source,re.M));expected={}
 for line in obj.with_suffix('.mtl').read_text().splitlines():
  w=line.split()
  if not w:continue
  if w[0]=='newmtl':name=' '.join(w[1:]);expected[name]={'color':[1,1,1,1],'texture':''}
  elif w[0]=='Kd':expected[name]['color']=list(map(float,w[1:4]))+[1]
  elif w[0]=='map_Kd':expected[name]['texture']=(obj.parent/' '.join(w[1:])).relative_to(root).as_posix()
 for name in used:
  b=next(b for b in bindings if b['model']==model and b['name']==name)
  assert b['color']==expected[name]['color'] and b['texture']==expected[name]['texture'],model
  mat=(root/b['material']).read_text()
  assert f'm_Shader: {{fileID: 4800000, guid: {LIT_SHADER_GUID}, type: 3}}' in mat,b['material']
  assert 'guid: d0353a89b1f911e48b9e16bdc9f2e058' in mat and '  version: 10\n' in mat,b['material']
  assert 0<=float(re.search(r'_Smoothness: ([0-9.]+)',mat).group(1))<=.5 and '_Metallic: 0' in mat,b['material']
  actual=[float(x) for x in re.search(r'_BaseColor: \{r: ([^,]+), g: ([^,]+), b: ([^,]+), a: ([^}]+)',mat).groups()]
  assert actual==b['color'],b['material']
  texture_ref=re.search(r'- _BaseMap:\s+m_Texture: (\{[^}]+\})',mat).group(1)
  assert re.search(r'- _MainTex:\s+m_Texture: (\{[^}]+\})',mat).group(1)==texture_ref,b['material']
  legacy_color=[float(x) for x in re.search(r'_Color: \{r: ([^,]+), g: ([^,]+), b: ([^,]+), a: ([^}]+)',mat).groups()]
  assert legacy_color==b['color'],b['material']
  if b['texture']:
   texguid=re.search(r'guid: ([a-f0-9]+)',(root/(b['texture']+'.meta')).read_text()).group(1)
   assert texture_ref==f'{{fileID: 2800000, guid: {texguid}, type: 3}}',b['material']
  else:assert texture_ref=='{fileID: 0}',b['material']
 if any(b['model']==model and b['texture'] for b in bindings) and any(any(float(v)<0 or float(v)>1 for v in line.split()[1:3]) for line in source.splitlines() if line.startswith('vt ')):
  uv_tiling_models.append(model)
importer=(root/'Assets/Hunter/Editor/HunterImporter.cs').read_text()
assert 'TextureWrapMode.Clamp : TextureWrapMode.Repeat' in importer
assert 'texture.wrapMode = wrap' in importer
assert 'texture.textureShape = TextureImporterShape.Texture2D' in importer
assert 'texture.maxTextureSize = 1024' in importer
assert 'texture.sRGBTexture = true' in importer
textures=[p for p in (root/'Assets/Resources/Hunter').rglob('*') if p.suffix in ['.png','.jpg','.exr']]
assert len(textures)>=17
for texture in textures:
 meta=Path(str(texture)+'.meta').read_text()
 assert re.search(r'^  textureShape: 1$',meta,re.M),str(texture)
 expected_type=8 if 'UI' in texture.parts else 0
 assert re.search(rf'^  textureType: {expected_type}$',meta,re.M),str(texture)
bank=(root/'Assets/Resources/Hunter/Narration/NarrationBank.asset').read_text()
entries=re.findall(r'  - roomId: (.+)\n    recordingScript: (.+)\n    clip: (.+)',bank)
assert len(entries)==9
scripts={roomid:json.loads(script) for roomid,script,clip in entries}
for m in catalog['maps']:
 for room in m['rooms']:assert scripts[room['id']]==room['riddle']
assert 'm_EditorVersion: 6000.6.4f1\n' in (root/'ProjectSettings/ProjectVersion.txt').read_text()
# A complete pipeline -> renderer -> package shader chain, including quality overrides.
packages=json.loads((root/'Packages/manifest.json').read_text())['dependencies']
assert packages['com.unity.render-pipelines.universal']=='17.6.0'
assert packages['com.unity.inputsystem']=='1.20.1'
player=(root/'ProjectSettings/ProjectSettings.asset').read_text()
assert re.search(r'^  activeInputHandler: 1$',player,re.M)
assert re.search(r'^  bundleVersion: 0.9.0$',player,re.M)
game=(root/'Assets/Hunter/Runtime/HunterGame.cs').read_text()
assert 'typeof(InputSystemUIInputModule)' in game
world=(root/'Assets/Hunter/Runtime/HunterGame.World.cs').read_text()
assert 'Mouse.current' in world and 'Keyboard.current' in game
for path in (root/'Assets/Hunter/Runtime').rglob('*.cs'):
 source=path.read_text()
 assert not re.search(r'\bInput\s*\.|\bStandaloneInputModule\b',source),str(path)+' uses legacy input'
assert packages['com.unity.modules.particlesystem']=='1.0.0'
assert packages['com.unity.modules.imageconversion']=='1.0.0'
def asset_guid(path):return re.search(r'guid: ([a-f0-9]{32})',(root/(path+'.meta')).read_text()).group(1)
pipeline_path='Assets/Hunter/Settings/HunterURP.asset'
renderer_path='Assets/Hunter/Settings/HunterRenderer.asset'
pipeline=(root/pipeline_path).read_text();renderer=(root/renderer_path).read_text()
pipeline_ref=f'{{fileID: 11400000, guid: {asset_guid(pipeline_path)}, type: 2}}'
renderer_ref=f'{{fileID: 11400000, guid: {asset_guid(renderer_path)}, type: 2}}'
assert 'guid: bf2edee5c58d82540a51f03df9d42094' in pipeline
assert 'guid: de640fe3d0db1804a85f9fc8f5cadab6' in renderer
assert f'm_RendererDataList:\n  - {renderer_ref}' in pipeline
assert 'm_DefaultRendererIndex: 0' in pipeline and 'm_RenderingMode: 0' in renderer
assert 'm_MSAA: 2' in pipeline and 'm_ShadowDistance: 35' in pipeline
assert 'm_SupportsHDR: 0' in pipeline
assert 'm_AdditionalLightsRenderingMode: 1' in pipeline and 'm_AdditionalLightsPerObjectLimit: 2' in pipeline
graphics=(root/'ProjectSettings/GraphicsSettings.asset').read_text()
quality=(root/'ProjectSettings/QualitySettings.asset').read_text()
assert f'm_CustomRenderPipeline: {pipeline_ref}' in graphics
quality_refs=re.findall(r'customRenderPipeline: (\{[^}]+\})',quality)
assert quality_refs and all(ref==pipeline_ref for ref in quality_refs)
assert len(quality_refs)==len(re.findall(r'^    name:',quality,re.M))
for path in (root/'Assets/Hunter/Runtime').rglob('*.cs'):
 assert 'Shader.Find("Standard")' not in path.read_text(),str(path)
# Every navigable screen has its own scene and a matching build entry.
build=(root/'ProjectSettings/EditorBuildSettings.asset').read_text()
scenes=list((root/'Assets/Hunter/Scenes').rglob('*.unity'))
expected={'Main','inicio','quarto'}|{r['id'] for m in catalog['maps'] for r in m['rooms']+[m['junction'],m['challenge']]}
assert len(scenes)==len(expected)==18 and {p.stem for p in scenes}==expected
for scene in scenes:
 assert scene.relative_to(root).as_posix() in build
 assert asset_guid(scene.relative_to(root).as_posix()) in build
 if scene.stem!='Main':assert 'stageId: '+scene.stem in scene.read_text()
# Stable .meta identifiers for every shipped Unity asset, including folders.
metas=[]
for path in (root/'Assets').rglob('*'):
 if path.suffix=='.meta':continue
 meta=Path(str(path)+'.meta');assert meta.is_file(),str(meta)+' missing'
 metas.append(re.search(r'guid: ([a-f0-9]{32})',meta.read_text()).group(1))
assert len(metas)==len(set(metas)),'Duplicate asset GUID'
print(json.dumps({'maps':3,'scenes':len(scenes),'rooms':9,'targets':54,'instances':instances,'unique_models':len(models),'verified_original_files':len(manifest['files']),'urp_material_bindings':len(bindings),'explicit_2d_texture_importers':len(textures),'navigation':'physical interconnected passages, no destination buttons','models_requiring_uv_repeat':uv_tiling_models,'narration_scripts':len(entries),'unity_version':'6000.6.4f1','urp_version':'17.6.0','input_system_version':'1.20.1','active_input_handling':'Input System Package (New)','rendering_path':'Forward','quality_pipeline_references':len(quality_refs),'unity_compile':'NOT RUN','playtest':'NOT RUN'},ensure_ascii=False,indent=2))
