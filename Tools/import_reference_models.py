"""Import a small CC0 toy selection from the Kenney archives, retaining source bytes."""
from pathlib import Path
import hashlib,json,re,uuid,zipfile

ROOT=Path(__file__).resolve().parents[1]
PACKS={
    'pirate-kit':['ship-small','boat-row-small','flag','cannon-mobile'],
    'castle-kit':['tower-square','tower-square-roof','flag','door','stairs-stone']
}


def main():
    manifest_path=ROOT/'Docs/asset-manifest.json'
    manifest=json.loads(manifest_path.read_text(encoding='utf-8'))
    template=(ROOT/'Assets/Resources/BemTeVi/Models/holiday-kit/lantern.obj.meta').read_text(encoding='utf-8')
    total=0
    for pack,names in PACKS.items():
        archive=ROOT/('Temp/VideoReferenceV14/'+pack+'.zip')
        records=[]; base=ROOT/('Assets/Resources/BemTeVi/Models/'+pack)
        with zipfile.ZipFile(archive) as z:
            selected=[('Models/OBJ format/'+name+'.'+ext,base/(name+'.'+ext)) for name in names for ext in ['obj','mtl']]
            selected += [('Models/OBJ format/Textures/colormap.png',base/'Textures/colormap.png'),
                         ('License.txt',ROOT/('Assets/ThirdPartyLicenses/'+pack+'/License.txt'))]
            for source,target in selected:
                raw=z.read(source); target.parent.mkdir(parents=True,exist_ok=True); target.write_bytes(raw)
                records.append(dict(file=target.relative_to(ROOT).as_posix(),source=source,sha256=hashlib.sha256(raw).hexdigest(),bytes=len(raw)))
                total+=len(raw)
        for name in names:
            target=base/(name+'.obj'); meta=Path(str(target)+'.meta')
            if meta.exists():continue
            guid=uuid.uuid5(uuid.NAMESPACE_URL,'bem-te-vi/'+target.relative_to(ROOT).as_posix()).hex
            source=re.sub(r'guid: [a-f0-9]{32}','guid: '+guid,template,count=1)
            source=re.sub(r'  externalObjects:.*?(?=  materials:)','  externalObjects: {}\n',source,flags=re.S)
            meta.write_text(source,encoding='utf-8')
        paths={r['file'] for r in records}
        manifest['files']=[r for r in manifest['files'] if r['file'] not in paths]+records
        manifest['packs']=[r for r in manifest['packs'] if r['pack']!=pack]+[
            dict(pack=pack,source='https://kenney.nl/assets/'+pack,
                 download='https://opengameart.org/sites/default/files/kenney_'+pack+('_2.1' if pack=='pirate-kit' else '')+'.zip',
                 zip_sha256=hashlib.sha256(archive.read_bytes()).hexdigest(),bytes=archive.stat().st_size)]
    manifest_path.write_text(json.dumps(manifest,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
    print('Imported',sum(map(len,PACKS.values())),'models;',total,'source bytes including materials, atlases and licenses.')


if __name__=='__main__':main()
