"""Import five original CC0 Kenney meshes, their source material/atlas and license.

Usage: python Tools/import_marble_selection.py <official Kenney Marble Kit ZIP>
The archive is read in place; no full asset pack is kept in the project.
"""
from pathlib import Path
import hashlib
import json
import sys
import zipfile
import uuid

ROOT=Path(__file__).resolve().parents[1]
SELECTED=['marble-low','marble-center-low','helix-left','funnel','banner']


def main():
    archive=Path(sys.argv[1])
    manifest_path=ROOT/'Docs/asset-manifest.json'
    manifest=json.loads(manifest_path.read_text(encoding='utf-8'))
    records=[]
    base=ROOT/'Assets/Resources/BemTeVi/Models/marble-kit'
    def write(path,raw):
        path.parent.mkdir(parents=True,exist_ok=True); path.write_bytes(raw)
        source='License.txt' if path.name=='License.txt' else 'Models/OBJ format/'+path.relative_to(base).as_posix()
        records.append(dict(file=path.relative_to(ROOT).as_posix(),source=source,sha256=hashlib.sha256(raw).hexdigest(),bytes=len(raw)))
    with zipfile.ZipFile(archive) as z:
        for name in SELECTED:
            for extension in ['obj','mtl']:
                write(base/(name+'.'+extension),z.read('Models/OBJ format/'+name+'.'+extension))
        write(base/'Textures/colormap.png',z.read('Models/OBJ format/Textures/colormap.png'))
        write(ROOT/'Assets/ThirdPartyLicenses/marble-kit/License.txt',z.read('License.txt'))
    # Complete, current Unity model importer with source material remaps populated later.
    template=(ROOT/'Assets/Resources/BemTeVi/Models/holiday-kit/lantern.obj.meta').read_text()
    import re
    for name in SELECTED:
        p=base/(name+'.obj')
        guid=uuid.uuid5(uuid.NAMESPACE_URL,'bem-te-vi/'+p.relative_to(ROOT).as_posix()).hex
        meta=re.sub(r'guid: [a-f0-9]{32}', 'guid: '+guid,template,count=1)
        meta=re.sub(r'  externalObjects:.*?(?=  materials:)', '  externalObjects: {}\n',meta,flags=re.S)
        Path(str(p)+'.meta').write_text(meta)
    manifest['packs']=[p for p in manifest['packs'] if p['pack']!='marble-kit']
    manifest['packs'].append(dict(pack='marble-kit',source='https://kenney.nl/assets/marble-kit',
        download='https://opengameart.org/sites/default/files/kenney_marble-kit.zip',
        zip_sha256=hashlib.sha256(archive.read_bytes()).hexdigest(),bytes=archive.stat().st_size))
    paths={r['file'] for r in records}
    manifest['files']=[r for r in manifest['files'] if r['file'] not in paths]+records
    manifest_path.write_text(json.dumps(manifest,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
    print('Imported',len(SELECTED),'models;',sum(r['bytes'] for r in records),'source bytes including material, atlas and license.')


if __name__=='__main__':main()
