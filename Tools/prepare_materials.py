#!/usr/bin/env python3
"""Translate the original Kenney MTL settings to explicit URP/Lit materials.
No mesh, texture, UV, source colour, or audio is created or modified.
Run from any directory; standard library only. Safe to run again.
"""
from pathlib import Path
import json, re, uuid

ROOT = Path(__file__).resolve().parents[1]
MODELS = ROOT / 'Assets/Resources/BemTeVi/Models'
# Unity Graphics 6000.6/staging, URP 17.6: Shaders/Lit.shader.meta.
LIT_SHADER_GUID = '933532a4fcc9baf4fa0491de14d08ed7'
# URP 17.6 Editor/AssetVersion.cs.meta and MaterialPostprocessor.k_Upgraders.
# Without this subasset URP can treat a freshly serialized material as an old
# LWRP material and replace _BaseMap/_BaseColor with obsolete property defaults.
VERSION_SCRIPT_GUID = 'd0353a89b1f911e48b9e16bdc9f2e058'
URP_MATERIAL_VERSION = 10

def guid(path):
    meta = Path(str(path) + '.meta')
    if meta.exists():
        match = re.search(r'guid: ([a-f0-9]+)', meta.read_text())
        if match: return match[1]
    value = uuid.uuid5(uuid.NAMESPACE_URL, 'bem-te-vi/' + path.relative_to(ROOT).as_posix()).hex
    meta.write_text('fileFormatVersion: 2\nguid: ' + value + '\n')
    return value

def main():
    records = []
    for obj in sorted(MODELS.rglob('*.obj')):
        definitions = {}; current = None
        for line in obj.with_suffix('.mtl').read_text().splitlines():
            words = line.strip().split()
            if not words: continue
            if words[0] == 'newmtl':
                current = {'color': [1, 1, 1, 1], 'texture': ''}
                definitions[' '.join(words[1:])] = current
            elif current is not None and words[0] == 'Kd':
                current['color'] = list(map(float, words[1:4])) + [1]
            elif current is not None and words[0] == 'map_Kd':
                current['texture'] = (obj.parent / ' '.join(words[1:])).relative_to(ROOT).as_posix()
        used = sorted(set(re.findall(r'^usemtl (.+)$', obj.read_text(), re.M)))
        asset = obj.relative_to(MODELS).with_suffix('').as_posix()
        for name in used:
            source = definitions[name.strip()]
            mat = ROOT / 'Assets/Resources/BemTeVi/Materials' / asset / (name.strip() + '.mat')
            mat.parent.mkdir(parents=True, exist_ok=True)
            tex = '{fileID: 0}'
            if source['texture']:
                tex = '{fileID: 2800000, guid: ' + guid(ROOT / source['texture']) + ', type: 3}'
            r, g, b, a = source['color']
            mat.write_text(f'''%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!21 &2100000
Material:
  serializedVersion: 8
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_Name: {name.strip()}
  m_Shader: {{fileID: 4800000, guid: {LIT_SHADER_GUID}, type: 3}}
  m_Parent: {{fileID: 0}}
  m_ValidKeywords: []
  m_InvalidKeywords: []
  m_LightmapFlags: 4
  m_EnableInstancingVariants: 1
  m_DoubleSidedGI: 0
  m_CustomRenderQueue: -1
  stringTagMap:
    RenderType: Opaque
  disabledShaderPasses: []
  m_SavedProperties:
    serializedVersion: 3
    m_TexEnvs:
    - _BaseMap:
        m_Texture: {tex}
        m_Scale: {{x: 1, y: 1}}
        m_Offset: {{x: 0, y: 0}}
    - _MainTex:
        m_Texture: {tex}
        m_Scale: {{x: 1, y: 1}}
        m_Offset: {{x: 0, y: 0}}
    m_Ints: []
    m_Floats:
    - _WorkflowMode: 1
    - _Smoothness: 0.18
    - _Metallic: 0
    - _Surface: 0
    - _Blend: 0
    - _Cull: 2
    - _AlphaClip: 0
    - _Cutoff: 0.5
    - _SrcBlend: 1
    - _DstBlend: 0
    - _SrcBlendAlpha: 1
    - _DstBlendAlpha: 0
    - _ZWrite: 1
    - _ReceiveShadows: 1
    - _SpecularHighlights: 1
    - _EnvironmentReflections: 1
    - _QueueOffset: 0
    m_Colors:
    - _BaseColor: {{r: {r}, g: {g}, b: {b}, a: {a}}}
    - _Color: {{r: {r}, g: {g}, b: {b}, a: {a}}}
    - _EmissionColor: {{r: 0, g: 0, b: 0, a: 1}}
--- !u!114 &11400000
MonoBehaviour:
  m_ObjectHideFlags: 11
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: 0}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: {VERSION_SCRIPT_GUID}, type: 3}}
  m_Name: 
  m_EditorClassIdentifier: 
  version: {URP_MATERIAL_VERSION}
''')
            guid(mat)
            meta = Path(str(mat) + '.meta')
            if 'NativeFormatImporter:' not in meta.read_text():
                meta.write_text(meta.read_text() + 'NativeFormatImporter:\n  externalObjects: {}\n  mainObjectFileID: 2100000\n')
            records.append({'model': asset, 'name': name.strip(), 'material': mat.relative_to(ROOT).as_posix(), **source})
    (ROOT / 'Assets/Resources/BemTeVi/material-bindings.json').write_text(json.dumps({'materials': records}, ensure_ascii=False, indent=2))
    print(f'{len(records)} original material definitions converted to URP/Lit; source art unchanged.')

if __name__ == '__main__': main()
