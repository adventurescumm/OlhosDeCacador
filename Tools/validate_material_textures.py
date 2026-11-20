#!/usr/bin/env python3
"""Check the diffuse-only source bindings before Unity imports their materials."""
from pathlib import Path
import argparse
import json
import re

SLOTS = re.compile(r'^    - ([^:\n]+):\n        m_Texture: (\{[^}\n]+\})', re.M)
MAP_KEYWORDS = {
    '_NORMALMAP', '_PARALLAXMAP', '_OCCLUSIONMAP', '_METALLICSPECGLOSSMAP',
    '_SPECGLOSSMAP', '_DETAIL_MULX2', '_DETAIL_SCALED',
}


def validate(root):
    root = Path(root)
    bindings = json.loads((root / 'Assets/Resources/BemTeVi/material-bindings.json').read_text(encoding='utf-8'))['materials']
    errors = []
    for source in bindings:
        path = root / source['material']
        text = path.read_text(encoding='utf-8')
        slots = dict(SLOTS.findall(text))
        expected = '{fileID: 0}'
        if source['texture']:
            meta = (root / (source['texture'] + '.meta')).read_text(encoding='utf-8')
            guid = re.search(r'^guid: ([a-f0-9]{32})$', meta, re.M).group(1)
            expected = '{fileID: 2800000, guid: ' + guid + ', type: 3}'
        for name in ('_BaseMap', '_MainTex'):
            if slots.get(name) != expected:
                errors.append(f"{source['material']}: {name} differs from the source diffuse map")
        for name, reference in slots.items():
            if name not in ('_BaseMap', '_MainTex') and reference != '{fileID: 0}':
                errors.append(f"{source['material']}: unexpected texture in {name}: {reference}")
        keywords = set(re.findall(r'^  - (_[A-Z0-9_]+)$', text, re.M)) & MAP_KEYWORDS
        if keywords:
            errors.append(f"{source['material']}: stale map keywords: {', '.join(sorted(keywords))}")
    if errors:
        raise ValueError('\n'.join(errors))
    return len(bindings)


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--root', type=Path, default=Path(__file__).resolve().parents[1])
    args = parser.parse_args()
    try:
        print(f'{validate(args.root)} material bindings OK: source albedo only; no incompatible array slots.')
    except ValueError as error:
        parser.exit(1, str(error) + '\n')
