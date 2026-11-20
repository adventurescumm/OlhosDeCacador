"""Restore exact original bytes only when they match the existing SHA-256 manifest.

Kenney OBJ exports can mix LF headers with CRLF numeric data. Some working copies
were converted to uniform CRLF. No expected checksum is replaced by this tool.
Archive fallbacks are read from Library/BemTeViSourceArchives/<pack>.zip.
Run with --apply to repair and save a per-file receipt under Docs.
"""
from pathlib import Path
import argparse
import hashlib
import json
import zipfile

ROOT=Path(__file__).resolve().parents[1]


def sha(data):
    return hashlib.sha256(data).hexdigest()


def recover_export_endings(data, expected):
    normalized=data.replace(b'\r\n',b'\n')
    if sha(normalized)==expected:return normalized
    # Exporter-written group/material headers use LF; mesh records use CRLF.
    lines=normalized.splitlines(keepends=True)
    candidate=b''.join(line if line.startswith((b'mtllib ',b'g ',b'usemtl '))
                       else line.replace(b'\n',b'\r\n') for line in lines)
    return candidate if sha(candidate)==expected else None


def main():
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--apply',action='store_true')
    args=parser.parse_args()
    manifest_path=ROOT/'Docs/asset-manifest.json'
    original_manifest=manifest_path.read_bytes()
    manifest=json.loads(original_manifest)
    receipts=[]; unresolved=[]; archives={}
    try:
        for entry in manifest['files']:
            path=(ROOT/entry['file']).resolve(); path.relative_to(ROOT)
            current=path.read_bytes()
            if sha(current)==entry['sha256']:continue
            replacement=None; source=''
            if path.suffix in ['.obj','.mtl']:
                replacement=recover_export_endings(current,entry['sha256'])
                if replacement is not None:source='restore_original_lf' if b'\r' not in replacement else 'recover_mixed_exporter_line_endings'
                if replacement is None:
                    pack=path.relative_to(ROOT/'Assets/Resources/BemTeVi/Models').parts[0]
                    archive_path=ROOT/('Library/BemTeViSourceArchives/'+pack+'.zip')
                    if archive_path.is_file():
                        if pack not in archives:archives[pack]=zipfile.ZipFile(archive_path)
                        archive=archives[pack]
                        for name in archive.namelist():
                            if name.rsplit('/',1)[-1]!=path.name:continue
                            candidate=archive.read(name)
                            if sha(candidate)==entry['sha256']:
                                replacement=candidate;source=archive_path.relative_to(ROOT).as_posix()+'!/'+name;break
            if replacement is None:
                unresolved.append(entry['file']);continue
            # A line-ending repair must not also change any text/geometry/material value.
            if current.replace(b'\r\n',b'\n') != replacement.replace(b'\r\n',b'\n'):
                raise RuntimeError('Original differs beyond line endings: '+entry['file'])
            receipts.append(dict(file=entry['file'],previous_sha256=sha(current),restored_sha256=sha(replacement),method=source))
            if args.apply:
                path.write_bytes(replacement)
                assert sha(path.read_bytes())==entry['sha256']
        assert manifest_path.read_bytes()==original_manifest,'Manifest must remain unchanged.'
        receipt_path=ROOT/'Docs/ASSET_INTEGRITY_V15.json'
        prior=json.loads(receipt_path.read_text(encoding='utf-8')) if receipt_path.is_file() else {}
        history={r['file']:r for r in prior.get('files',[])} if prior.get('manifest_sha256')==sha(original_manifest) else {}
        if args.apply:history.update({r['file']:r for r in receipts})
        result=dict(manifest_sha256=sha(original_manifest),expected_hashes_changed=0,
                    repaired=len(receipts) if args.apply else 0,repaired_total=len(history),recoverable=len(receipts),unresolved=unresolved,files=list(history.values()))
        if args.apply and receipts:
            receipt_path.write_text(json.dumps(result,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
        print(json.dumps({k:v for k,v in result.items() if k!='files'},ensure_ascii=False,indent=2))
        if unresolved:raise SystemExit(1)
    finally:
        for archive in archives.values():archive.close()


if __name__=='__main__':main()
