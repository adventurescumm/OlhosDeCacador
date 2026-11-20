"""One-time, deterministic v12 -> v13 composition migration; preserves discovery IDs.

Run with Python from any folder. Does nothing when the catalog is already v13.
Only architecture uses primitives; every toy uses an existing source model.
"""
import copy
import json
import math
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
PATH = ROOT / 'Assets/Resources/BemTeVi/catalog.json'


def shape(name, x, y, z, w, h, d, color, rotation=0, kind='box'):
    return dict(name=name, shape=kind, x=round(x, 3), y=round(y, 3), z=round(z, 3),
                width=w, height=h, depth=d, color=color, rotation=rotation, decorative=True)


def prop(asset, x, y, z, size, action='', destination='', label='', reaction='hop'):
    return dict(asset=asset, id='', label=label, clue='', sound='', action=action,
                destination=destination, reaction=reaction, value=0, round=0,
                x=x, y=y, z=z, size=size, rotation=0)


def base(width, depth, color, floor):
    return [shape('Base de madeira', 0, -.42, 0, width, .72, depth, '#9E6847'),
            shape('Lâmina de madeira clara', 0, -.55, 0, width+.08, .06, depth+.08, '#E6C89F'),
            shape('Borda pintada da maquete', 0, -.12, 0, width+.12, .16, depth+.12, color),
            shape('Piso da maquete', 0, -.025, 0, width-.12, .05, depth-.12, floor),
            shape('Cartão de fundo', 0, 1.1, depth/2, width, 2.2, .14, color),
            shape('Acabamento do cartão', 0, 2.23, depth/2, width+.16, .10, .22, '#F1D9B2')]


def path_segment(geometry, a, b, color, name='Caminho de fita'):
    dx, dz = b[0]-a[0], b[1]-a[1]
    length = math.hypot(dx, dz)
    geometry.append(shape(name, (a[0]+b[0])/2, .018, (a[1]+b[1])/2,
                          .62, .025, round(length, 3), color,
                          round(math.degrees(math.atan2(dx, dz)), 3)))
    # Broken white centre line makes each connection readable as a continuous route.
    for i in range(1, max(2, int(length/.7))):
        t=i/max(2, int(length/.7))
        geometry.append(shape('Costura do caminho', a[0]+dx*t, .04, a[1]+dz*t,
                              .05, .012, .25, '#FFF1C9', round(math.degrees(math.atan2(dx, dz)), 3)))


def exit_to(asset, destination, title, x, z, size=1.45):
    p=prop(asset, x, 0, z, size, 'exit', destination, title)
    p['landmark']=title
    return p


def main():
    catalog=json.loads(PATH.read_text(encoding='utf-8'))
    if catalog.get('layoutRevision', 0) >= 13:
        print('Catalog already uses v13.'); return
    before={p['id'] for m in catalog['maps'] for r in m['rooms'] for p in r['objects'] if p['id']}
    for mi, m in enumerate(catalog['maps']):
        # Gate records stay supported by the save reader, but no gate exists in new content.
        m['gates']=[]
        for ri, r in enumerate(m['rooms']):
            old=copy.deepcopy(r['objects'])
            targets=[p for p in old if p['id']]
            decorations=[p for p in old if not p['id'] and not p.get('action')]
            target_assets={p['asset'] for p in targets}
            r.update(width=18, depth=12.6, camera=[0, 13.2, -19.8], lookAt=[0, .5, .4], fieldOfView=38)
            g=base(18, 12.6, r['color'], r['floor'])
            g.append(shape('Alameda livre entre as bancadas', 0, .012, .2, 17.4, .024, 1.05, '#E9D9B8'))
            objects=[]
            # Six groups, each containing one target from every round and two decoys.
            # Low platforms are spaced apart; the front half stays open to the camera.
            centers=[(x,z) for z in [-2.35, 2.25] for x in [-5.7,0,5.7]]
            slots=[(-1.22,-.65), (0,.48), (1.22,-.65), (-1.35,.85), (1.35,.85)]
            eligible=[p for p in decorations if p['asset'] not in target_assets and p['size'] < .9]
            if not eligible: eligible=[p for p in decorations if p['asset'] not in target_assets]
            for group,(cx,cz) in enumerate(centers):
                top=.18 if group<3 else .46
                g.append(shape('Bancada baixa %d' % (group+1),cx,top/2,cz,4.65,top,3.35,'#CBA575'))
                g.append(shape('Tampo de feltro %d' % (group+1),cx,top+.025,cz,4.48,.05,3.18,
                               ['#D0B981','#A3BACA','#B1C392'][mi]))
                # Interleave rounds to keep every area useful in all three searches.
                for round_index in range(3):
                    p=[t for t in targets if t['round']==round_index][group]
                    sx,sz=slots[(round_index+group)%3]
                    p.update(x=round(cx+sx,3), y=top+.05, z=round(cz+sz,3),
                             size=round([.95,.84,.73][round_index]-mi*.035,3), tiltX=0, tiltZ=0)
                    for key in ['gate','showAfter','openedOffset','openedRotation']:p.pop(key,None)
                    objects.append(p)
                if mi==2 or mi==1 and group%2==0:
                    p=copy.deepcopy(eligible[(group+4)%len(eligible)])
                    p.update(x=cx,y=top+.05,z=round(cz-1.05,3),size=.47,label='Peça decorativa adicional')
                    objects.append(p)
                for j in range(2):
                    if not eligible: continue
                    p=copy.deepcopy(eligible[(group*2+j)%len(eligible)])
                    sx,sz=slots[3+j]
                    p.update(x=round(cx+sx,3),y=top+.05,z=round(cz+sz,3),size=.58, rotation=(group*43+j*65)%360)
                    if group==0:p['asset']=['marble-kit/marble-low','marble-kit/marble-center-low'][j];p['size']=.57
                    for key in ['gate','showAfter','openedOffset','openedRotation']:p.pop(key,None)
                    objects.append(p)
            # A distinct skyline built from oversized toys, behind the search surfaces.
            motifs=[
                [['survival-kit/structure-roof','holiday-kit/present-b-cube','furniture-kit/books'],
                 ['furniture-kit/chairRounded','furniture-kit/tableCloth','survival-kit/workbench'],
                 ['space-kit/rock_largeB','survival-kit/tent','brick-kit/round-hq-brick-2x4']],
                [['space-kit/hangar_roundA','furniture-kit/computerKeyboard','space-kit/rocket_fuelA'],
                 ['space-kit/rock_crystalsLargeA','furniture-kit/pottedPlant','space-kit/meteor_half'],
                 ['space-kit/hangar_smallA','space-kit/craft_cargoA','space-kit/machine_generator']],
                [['city-kit-commercial/building-a','brick-kit/round-hq-brick-2x4','city-kit-commercial/building-d'],
                 ['city-kit-commercial/building-e','nature-kit/bridge_wood','nature-kit/tree_small'],
                 ['city-kit-commercial/building-skyscraper-a','space-kit/desk_computer','city-kit-commercial/building-skyscraper-c']]
            ][mi][ri]
            for j,asset in enumerate(motifs):
                if asset in target_assets:
                    asset=next(p['asset'] for p in decorations if p['asset'] not in target_assets and p['size']>=1)
                objects.append(prop(asset,[-6.4,0,6.4][j],0,5.12,[2.2,2.5,2][j],
                                    'inspect',label='Brinquedo de faz de conta',reaction=['somersault','squash','spin'][j]))
            # Every room has a direct, explicitly signed return to its world's map.
            title='Mapa do mundo'
            objects.append(exit_to('furniture-kit/doorwayFront',m['junction']['id'],title,0,-5.35,1.25))
            previous=m['rooms'][(ri-1)%3]
            following=m['rooms'][(ri+1)%3]
            objects.append(exit_to('survival-kit/signpost',previous['id'],previous['name'],-7.35,-5.15,1.15))
            objects.append(exit_to('survival-kit/signpost',following['id'],following['name'],7.35,-5.15,1.15))
            if ri==2:
                # The baú/comunicador route also remains reachable on the map.
                objects.append(exit_to(m['challenge']['objects'][0]['asset'],m['challenge']['id'],'Desafio',7.8,4.9,1.2))
            if r['layout']=='chest':
                # A real compartmented chest silhouette, with low front/sides for visibility.
                for x in [-8.72,8.72]:g.append(shape('Lateral do baú',x,.3,0,.23,.6,12.5,'#8E5538'))
                g.append(shape('Frente baixa do baú',0,.16,-6.18,17.5,.32,.19,'#8E5538'))
                for x in [-2.85,2.85]:g.append(shape('Divisória do baú',x,.12,0,.12,.24,10.4,'#C49D6D'))
                g.append(shape('Dobradiça do baú',0,.06,6.04,4,.12,.3,'#DAB451'))
            r.update(objects=objects, geometry=g)

        junction=m['junction']
        junction.update(name=['Mapa da Vila de Brinquedo','Mapa da Caixa Lunar','Mapa da Cidade de Blocos'][mi],
                        caption='Caminhos abertos entre todas as buscas e o desafio.',layout='map',width=20,depth=13,
                        camera=[0,15,-20.7],lookAt=[0,.4,.3],fieldOfView=38)
        g=base(20,13,junction['color'],junction['floor'])
        nodes=[(-6,2.1),(0,3.6),(6,2.1),(4.4,-2.8),(-4.4,-2.8)]
        destinations=m['rooms']+[m['challenge'],dict(id='quarto',name='Voltar ao quarto')]
        landmarks=[m['rooms'][0]['objects'][-3]['asset'],m['rooms'][1]['objects'][-2]['asset'],
                   m['rooms'][2]['objects'][-1]['asset'],m['challenge']['objects'][0]['asset'],'furniture-kit/doorwayFront']
        # Recognizable miniature entrances instead of five identical doors.
        landmarks[:3]=[['holiday-kit/train-locomotive','survival-kit/structure-roof','survival-kit/chest'],
                       ['space-kit/hangar_smallA','space-kit/rock_crystalsLargeA','space-kit/craft_cargoA'],
                       ['city-kit-commercial/building-a','nature-kit/bridge_wood','city-kit-commercial/building-f']][mi]
        landmarks[3]=['survival-kit/chest','space-kit/machine_wireless','space-kit/desk_computer'][mi]
        colors=['#BC7751','#7779BA','#6E9D86','#AA648A','#A58C5E']
        objects=[]
        for i,((x,z),destination,asset) in enumerate(zip(nodes,destinations,landmarks)):
            path_segment(g,(0,-.6),(x,z),colors[i])
            g.append(shape('Ilha do destino',x,.08,z,3.7,.16,2.6,colors[i]))
            p=exit_to(asset,destination['id'],destination['name'] if i<3 else 'Desafio' if i==3 else 'Voltar ao quarto',x,z,2.1 if i<3 else 1.65)
            p['y']=.16;objects.append(p)
        # A loop around the three searches explains their direct interconnections.
        for a,b in [(0,1),(1,2),(2,0)]:path_segment(g,nodes[a],nodes[b],'#D5C6A7','Trilha entre as buscas')
        objects.append(prop('survival-kit/signpost',0,0,-.6,1.5,'inspect',label='A encruzilhada de brinquedo',reaction='spin'))
        for x,asset in [(-8.6,'holiday-kit/candy-cane-red'),(8.6,'holiday-kit/candy-cane-green')]:
            objects.append(prop(asset,x,0,4.8,2.4,'inspect',label='Uma árvore de doce',reaction='squash'))
        for asset,x,z,size,reaction,label in [
            ('helix-left',-8,-.2,2,'spin','A torre das bolinhas de gude'),
            ('funnel',8,-.2,1.8,'somersault','Um funil espacial'),
            ('banner',1.6,-.8,1.5,'tip','A bandeira da brincadeira')]:
            objects.append(prop('marble-kit/'+asset,x,0,z,size,'inspect',label=label,reaction=reaction))
        junction.update(objects=objects,geometry=g)

        challenge=m['challenge']
        # Challenges are always visitable and have two explicit ways back.
        challenge['objects']=[p for p in challenge['objects'] if p.get('action')!='exit']
        challenge['objects'].append(exit_to('furniture-kit/doorwayFront',junction['id'],'Mapa do mundo',-4.4,-3.15,1.1))
        challenge['objects'].append(exit_to('survival-kit/signpost',m['rooms'][2]['id'],m['rooms'][2]['name'],4.4,-3.15,1.1))

    # Keep the three world entrances together on one shared play mat.
    hub=catalog['hub']
    old_x=[-3.1,0,3.1];new_x=[-2.5,0,2.5]
    for p in hub['objects']:
        if p.get('action')=='world':
            index=next(i for i,m in enumerate(catalog['maps']) if m['id']==p['destination'])
            p.update(x=new_x[index],size=1.8)
    for s in hub.get('geometry',[]):
        if any(m['id'] in s['name'] for m in catalog['maps']):
            index=next(i for i,m in enumerate(catalog['maps']) if m['id'] in s['name'])
            s['x']=new_x[index]
            s['width']=2.3 if 'Caixa' in s['name'] else 2.36
    catalog['layoutRevision']=13
    after={p['id'] for m in catalog['maps'] for r in m['rooms'] for p in r['objects'] if p['id']}
    assert before==after and len(after)==162
    PATH.write_text(json.dumps(catalog,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
    print('v13: 9 spaced search layouts, 3 connected maps, no token gates; all 162 IDs preserved.')


if __name__=='__main__':main()
