#!/usr/bin/env python3
"""Repair incomplete .meta importers with Unity CLOSED, preserving GUIDs.

Only the models/audio in Docs/asset-manifest.json are considered. Existing
supported importer blocks are left intact. Original art is never written.
Schema references: Unity-Technologies/BoatAttack (official sample),
Assets/Objects/UI/meshes/checkpoint.fbx.meta (19300) and
Assets/Audio/Music/MainTrack.wav.meta (6), adapted for static OBJ / UI sounds.
Unity may upgrade these supported schemas to its current format on import.
"""
from pathlib import Path
import json, re, shutil, argparse
ROOT=Path(__file__).resolve().parents[1]
MODEL="""  serializedVersion: 19300
  internalIDToNameTable: []
  externalObjects: {}
  materials:
    materialImportMode: 1
    materialName: 0
    materialSearch: 1
    materialLocation: 1
  animations:
    legacyGenerateAnimations: 4
    bakeSimulation: 0
    resampleCurves: 1
    optimizeGameObjects: 0
    motionNodeName: 
    rigImportErrors: 
    rigImportWarnings: 
    animationImportErrors: 
    animationImportWarnings: 
    animationRetargetingWarnings: 
    animationDoRetargetingWarnings: 0
    importAnimatedCustomProperties: 0
    importConstraints: 0
    animationCompression: 1
    animationRotationError: 0.5
    animationPositionError: 0.5
    animationScaleError: 0.5
    animationWrapMode: 0
    extraExposedTransformPaths: []
    extraUserProperties: []
    clipAnimations: []
    isReadable: 0
  meshes:
    lODScreenPercentages: []
    globalScale: 1
    meshCompression: 0
    addColliders: 0
    useSRGBMaterialColor: 1
    sortHierarchyByName: 0
    importVisibility: 0
    importBlendShapes: 0
    importCameras: 0
    importLights: 0
    swapUVChannels: 0
    generateSecondaryUV: 0
    useFileUnits: 1
    keepQuads: 0
    weldVertices: 1
    preserveHierarchy: 0
    skinWeightsMode: 0
    maxBonesPerVertex: 4
    minBoneWeight: 0.001
    meshOptimizationFlags: -1
    indexFormat: 0
    secondaryUVAngleDistortion: 8
    secondaryUVAreaDistortion: 15.000001
    secondaryUVHardAngle: 88
    secondaryUVPackMargin: 4
    useFileScale: 1
  tangentSpace:
    normalSmoothAngle: 60
    normalImportMode: 0
    tangentImportMode: 3
    normalCalculationMode: 4
    legacyComputeAllNormalsFromSmoothingGroupsWhenMeshHasBlendShapes: 0
    blendShapeNormalImportMode: 2
    normalSmoothingSource: 0
  referencedClips: []
  importAnimation: 0
  humanDescription:
    serializedVersion: 3
    human: []
    skeleton: []
    armTwist: 0.5
    foreArmTwist: 0.5
    upperLegTwist: 0.5
    legTwist: 0.5
    armStretch: 0.05
    legStretch: 0.05
    feetSpacing: 0
    globalScale: 1
    rootMotionBoneName: 
    hasTranslationDoF: 0
    hasExtraRoot: 0
    skeletonHasParents: 1
  lastHumanDescriptionAvatarSource: {instanceID: 0}
  autoGenerateAvatarMappingIfUnspecified: 1
  animationType: 0
  humanoidOversampling: 1
  avatarSetup: 0
  additionalBone: 0
  userData: 
  assetBundleName: 
  assetBundleVariant: 
"""
AUDIO="""  externalObjects: {}
  serializedVersion: 6
  defaultSettings:
    loadType: 0
    sampleRateSetting: 0
    sampleRateOverride: 44100
    compressionFormat: 1
    quality: 0.6
    conversionMode: 0
  platformSettingOverrides: {}
  forceToMono: 1
  normalize: 1
  preloadAudioData: 1
  loadInBackground: 0
  ambisonic: 0
  3D: 0
  userData:
  assetBundleName:
  assetBundleVariant:
"""

def main():
 parser=argparse.ArgumentParser(description=__doc__)
 parser.add_argument('--check',action='store_true',help='Only validate; never write')
 args=parser.parse_args()
 files=json.loads((ROOT/'Docs/asset-manifest.json').read_text())['files']
 changed=[]
 for entry in files:
  asset=ROOT/entry['file']; ext=asset.suffix.lower()
  if ext not in {'.obj','.fbx','.ogg','.wav','.mp3'}: continue
  kind,minimum,body=('ModelImporter',25,MODEL) if ext in {'.obj','.fbx'} else ('AudioImporter',6,AUDIO)
  meta=Path(str(asset)+'.meta'); text=meta.read_text()
  guid=re.search(r'^guid: ([a-f0-9]{32})$',text,re.M)
  if not guid: raise ValueError('GUID absent; refusing to replace: '+str(meta))
  block=re.search(r'^'+kind+r':\n((?:[ \t].*\n|\n)*)',text,re.M)
  version=re.search(r'^  serializedVersion: (\d+)$',block.group(1),re.M) if block else None
  if version and int(version.group(1))>=minimum: continue
  changed.append(meta.relative_to(ROOT).as_posix())
  if args.check: continue
  backup=ROOT/'ImporterMetadataBackup'/meta.relative_to(ROOT)
  backup.parent.mkdir(parents=True,exist_ok=True)
  if not backup.exists(): shutil.copy2(meta,backup)
  # Keep the identity; regenerate only an unsupported/incomplete importer block.
  meta.write_text('fileFormatVersion: 2\nguid: '+guid.group(1)+'\n'+kind+':\n'+body)
 if args.check and changed: raise SystemExit('Unsupported importers: '+', '.join(changed))
 print(('Invalid importers: ' if args.check else 'Repaired importers: ')+str(len(changed)))
if __name__=='__main__': main()
