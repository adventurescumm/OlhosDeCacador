"""Recompose continuous toy worlds and walk-through junctions from the supplied longplay.

Uses original downloaded meshes; keeps all riddle IDs and save locations.
Run before rebuilding the Unity scenes. Repeat runs produce the same composition.
"""
import copy
import json
import math
import random
from pathlib import Path

ROOT=Path(__file__).resolve().parents[1]
CATALOG=ROOT/'Assets/Resources/BemTeVi/catalog.json'


def block(name,x,y,z,w,h,d,color,rotation=0,shape='box',tiltX=0,tiltZ=0):
    return dict(name=name,shape=shape,x=x,y=y,z=z,width=w,height=h,depth=d,
                color=color,rotation=rotation,tiltX=tiltX,tiltZ=tiltZ,decorative=False)


def toy(asset,x,y,z,size,rotation=0,action='',label='',reaction='hop',**extra):
    return dict(asset=asset,id='',label=label,clue='',sound='',x=x,y=y,z=z,size=size,
                rotation=rotation,action=action,reaction=reaction,**extra)


def door(asset,destination,title,x,z,size=1.6,y=0,rotation=0):
    return toy(asset,x,y,z,size,rotation,'exit',title,destination=destination,landmark=title)


def foundation(floor,trim,indoor=False):
    g=[block('Borda da maquete de madeira',0,-.36,0,18.5,.65,12.7,'#855239'),
       block('Camada de papel creme',0,-.11,0,18.6,.10,12.8,'#EFCE94'),
       block('Piso contínuo',0,-.025,0,18.3,.05,12.5,floor),
       block('Frente pintada da maquete',0,-.29,-6.36,18.55,.17,.055,trim)]
    if indoor:
        g += [block('Fundo da casa de bonecas',0,2.65,6.1,18.4,5.3,.15,trim),
              block('Lateral recortada da casa',-9.1,1.3,2.9,.16,2.6,6.4,trim),
              block('Rodapé de madeira',0,.16,5.95,18.3,.32,.15,'#E7BD87')]
        for x in [-8,-6.4,-4.8,-3.2,-1.6,0,1.6,3.2,4.8,6.4,8]:
            g.append(block('Junta das tábuas do piso',x,.007,0,.018,.014,12.3,'#99775F'))
        for x in [-8.6,-4.3,0,4.3,8.6]:
            g.append(block('Friso do painel de cartão',x,3.25,5.98,.045,3.7,.045,'#D9B384'))
    return g


def ribbon(g,a,b,width,color,name='Caminho de feltro'):
    dx,dz=b[0]-a[0],b[1]-a[1]
    g.append(block(name,(a[0]+b[0])/2,.025,(a[1]+b[1])/2,width,.05,
                   round(math.hypot(dx,dz),3),color,round(math.degrees(math.atan2(dx,dz)),3)))


FLOOR_POINTS=[(-6.5,-3.7),(-3.9,-3.3),(-1.2,-4.1),(1.2,-3.1),(4.1,-3.8),(6.5,-2.9),
              (-6.1,-.7),(-3.6,-.1),(-1,-.9),(1.6,.2),(4.3,-.9),(6.4,.5),
              (-5.8,2.1),(-3.1,2.7),(-.5,2.3),(2,2.9),(4.5,2),(6.7,3.1)]


def search_composition(mi,ri,r,m):
    indoor=(mi,ri) in [(0,1),(1,0),(1,2),(2,2)]
    floors=[['#DBB17A','#A96544','#A67C59'],['#7476A0','#CC8E75','#B5977C'],['#92AF92','#B4BA8C','#AA8B79']]
    trims=['#9E5150','#494D83','#476F76']
    g=foundation(floors[mi][ri],trims[mi],indoor)
    objects=[]; slots=[(x,.04,z) for x,z in FLOOR_POINTS]
    r.update(width=18.8,depth=13,scope='room',camera=[.5,10.6,-18.4],lookAt=[0,1.0,.6],fieldOfView=39)
    if mi==0 and ri==0:
        # A railway on a picture book; oversized books and roof pieces become a town.
        ribbon(g,(-8,-1.8),(7,-1.8),1.05,'#A47A52','Leito do trenzinho')
        for x in range(-7,8,2):objects.append(toy('toy-car-kit/track-narrow-straight',x,0,-1.8,1.9,90))
        objects += [toy('survival-kit/structure-roof',-5.7,0,4.6,4.4),
                    toy('furniture-kit/books',.3,0,5.1,4.0,90,'inspect','Montanhas de histórias','tip',idle='sway'),
                    toy('train-kit/train-tram-round',5.8,0,4.4,3.8,0,'inspect','O trem sonhador','somersault'),
                    toy('nature-kit/cactus_tall',-8,0,2.1,2.4),toy('holiday-kit/present-b-cube',8,0,3.4,2.2)]
        for x in [-8.1,8.1]:g.append(block('Lombada do livro',x,-.06,0,.10,.11,12.4,'#F4D7A0'))
    elif mi==0 and ri==1:
        r.update(camera=[.4,8.9,-17.8],lookAt=[0,1.35,1.2],fieldOfView=40)
        objects += [toy('furniture-kit/tableCloth',-4.7,0,1.4,5.4),
                    toy('furniture-kit/chairRounded',-7.8,0,3.8,2.4,25),
                    toy('survival-kit/workbench',6.7,0,4.6,2.5),
                    toy('holiday-kit/gingerbread-woman',-8.0,0,-.2,1.9,0,'inspect','A dona do salão de suco','somersault',idle='sway')]
        slots=slots[:6]+[(x,2.12,z) for z in [.5,2.25] for x in [-6.1,-4.7,-3.3]]
        for y in [1.25,2.85]:
            g.append(block('Prateleira de louças',1.5,y-.1,4.3,7.5,.2,1.65,'#C89159'))
            slots += [(x,y,4.1) for x in [-.8,1.5,3.8]]
        g += [block('Fundo do armário de brinquedo',1.5,1.8,5.25,7.7,3.6,.18,'#774E47'),
              block('Tapete do salão',0,.015,-2,12,.03,4.7,'#C68A71')]
    elif mi==0 and ri==2:
        r.update(scope='chest',layout='chest',camera=[0,14.2,-12.4],lookAt=[0,.8,.5],fieldOfView=43)
        g=foundation('#767D75','#975634')
        for x in [-8.75,8.75]:g.append(block('Lateral do baú aberto',x,.5,0,.32,1,12.5,'#82502F'))
        g += [block('Frente baixa do baú',0,.3,-6.12,17.7,.6,.26,'#82502F'),
              block('Tampa de madeira aberta',0,2.7,6.1,18,5.4,.22,'#A36A45',tiltX=-12),
              block('Forro de feltro da tampa',0,2.8,5.78,16.5,4.5,.1,'#7D5470',tiltX=-12),
              block('Divisória curta do baú',2.8,.2,2.7,.14,.4,5.2,'#B79463')]
        objects += [toy('space-kit/rock_largeB',-7.5,0,4.4,2.5),toy('space-kit/rock_crystalsLargeA',6.9,0,4.5,2.3),
                    toy('pirate-kit/ship-small',-.5,0,4.6,3.3,0,'inspect','Um navio navega no baú','orbit',idle='sway')]
    elif mi==1 and ri==0:
        g += [block('Janela circular da nave',-4,3.7,6,3.2,.12,3.2,'#8AD7DB',shape='cylinder',tiltX=90),
              block('Planeta na janela',-4.4,3.8,5.88,1.65,1.65,.12,'#F6B966',shape='sphere'),
              block('Bancada do laboratório',3.5,1,3.9,8.5,2,3.2,'#B1C7C9')]
        slots=slots[:12]+[(x,2.02,z) for z in [3.05,4.6] for x in [.6,3.2,5.8]]
        objects += [toy('space-kit/desk_computer',-6.7,0,4.4,3.0),
                    toy('space-kit/craft_cargoA',7.5,0,-.4,2.5,0,'inspect','A nave espirra estrelas','launch',idle='bob'),
                    toy('pirate-kit/cannon-mobile',-8,0,.7,2.2,0,'inspect','O canhão de bolinhas','squash')]
    elif mi==1 and ri==1:
        r.update(camera=[-.8,11.8,-18.4],lookAt=[0,.7,.6],scope='landscape')
        g += [block('Faixa de areia violeta',0,.01,0,17,.025,3.5,'#B27798',rotation=-9)]
        objects += [toy('food-kit/donut',-7.6,0,3.6,3.6,0,'inspect','A rosquinha virou um portal','spin',tiltX=72,idle='sway'),
                    toy('furniture-kit/pottedPlant',6.8,0,4.5,3.2),toy('space-kit/craft_cargoA',-.5,0,5,2.9,0,'inspect','Uma nave brotou no jardim','launch'),
                    toy('marble-kit/helix-left',-8.1,0,-.6,2.2),toy('marble-kit/funnel',7.8,0,-1.4,2.5)]
    elif mi==1 and ri==2:
        g += [block('Borda da caixa de sapatos',8.9,.9,1,.25,1.8,10.2,'#B78D5F'),
              block('Aba aberta da caixa',-9.05,.9,1.6,2.0,.14,9,'#CFB384',tiltZ=-40),
              block('Balcão da oficina',-4.6,.72,3.5,7.8,1.44,3.1,'#8E909C')]
        slots=slots[:12]+[(x,1.46,z) for z in [2.7,4.3] for x in [-7,-4.7,-2.5]]
        objects += [toy('space-kit/hangar_smallA',5.2,0,4.5,4.4),toy('space-kit/machine_generator',8,0,1.3,2.0),
                    toy('pirate-kit/cannon-mobile',7.5,0,-1.7,2,0,'inspect','O canhão de bolinhas','squash')]
    elif mi==2 and ri==0:
        r.update(scope='landscape')
        ribbon(g,(-8,-1.8),(8,-1.8),1.2,'#F0D79A','Avenida de fita')
        objects += [toy('city-kit-commercial/building-a',-6.6,0,4.8,3.5),
                    toy('city-kit-commercial/building-d',-.6,0,5,4.5),
                    toy('castle-kit/tower-square',6.2,0,4.6,3.8),
                    toy('nature-kit/tree_small',8,0,.8,2.6),
                    toy('holiday-kit/gingerbread-woman',-8.1,0,.3,2,0,'inspect','A prefeita de biscoito','somersault',idle='sway')]
        g += [block('Página aberta do livro',0,-.04,0,18.1,.08,12.3,'#CFD5AC'),
              block('Vinco do livro',.3,.015,.8,.07,.03,10,'#B5B58A')]
    elif mi==2 and ri==1:
        r.update(scope='landscape',camera=[1,11.4,-19.1],lookAt=[0,.8,.6])
        ribbon(g,(-9,.5),(9,.5),2.0,'#67B8C9','Rio de fita azul')
        # Targets stay on the two banks; the bridge and its rails are between them.
        slots=[(x,.04,z) for x,z in FLOOR_POINTS]
        slots=[(x,y, z-1.2 if -.9<=z<=.5 else z) for x,y,z in slots]
        slots[14]=(-1.5,.04,3.9)
        objects += [toy('nature-kit/bridge_wood',-.3,0,.5,3.2),
                    toy('city-kit-commercial/building-e',-6.4,0,4.8,3.9),
                    toy('city-kit-commercial/building-f',6,0,4.9,3.7),
                    toy('pirate-kit/boat-row-small',5.3,.1,.5,2.0,80,'inspect','Um barco navega na fita','orbit',idle='sway')]
    else:
        r.update(scope='cabinet',camera=[.2,8.8,-18.7],lookAt=[0,1.7,1.0],fieldOfView=39)
        slots=slots[:6]
        for y in [.85,2.4,3.95]:
            g.append(block('Prateleira do armário de heróis',0,y-.1,3.65,16.2,.2,1.7,'#D5A765'))
            slots += [(x,y,3.45) for x in [-6,-2,2,6]]
        objects += [toy('furniture-kit/radio',-7.8,0,.3,2.4,0,'inspect','O rádio da cidade sonha','squash'),
                    toy('pirate-kit/cannon-mobile',7.7,0,.1,2.3,0,'inspect','O lançador de brinquedo','spin',idle='sway')]
    # Keep the verse order while distributing all three rounds through the scene.
    targets=[copy.deepcopy(p) for p in r['objects'] if p['id']]
    for round_index in range(3):
        for index,p in enumerate([p for p in targets if p['round']==round_index]):
            x,y,z=slots[index*3+round_index]
            p.update(x=x,y=y,z=z,size=round([.96,.86,.77][round_index]-mi*.025,3),tiltX=0,tiltZ=0)
            for key in ['gate','showAfter','openedOffset','openedRotation']:p.pop(key,None)
    objects += targets
    # Finite, varied decoys selected around the targets, not thrown into the same clumps.
    known={p['asset'] for p in targets}
    decoy_assets=['marble-kit/marble-low','food-kit/banana','food-kit/donut','food-kit/cookie',
                 'survival-kit/box','survival-kit/bottle-large','food-kit/pear','survival-kit/fish',
                 'toy-car-kit/wheel-large','holiday-kit/present-b-cube','food-kit/mug','pirate-kit/flag',
                 'castle-kit/flag','holiday-kit/sock-green','nature-kit/pot_large','food-kit/strawberry']
    decoy_assets=[a for a in decoy_assets if a not in known and (ROOT/('Assets/Resources/BemTeVi/Models/'+a+'.obj')).is_file()]
    candidates=[(x,.04,z) for z in [-4.7,-3.35,-2,-.5,1,2.5,4] for x in [-7.6,-6.2,-4.8,-3.4,-2,-.6,.8,2.2,3.6,5,6.4,7.8]]
    random.Random(mi*17+ri).shuffle(candidates)
    count=0
    for point in candidates:
        x,y,z=point
        if any((x-p['x'])**2+(z-p['z'])**2 < (max(.65,p['size']*.46)+.30)**2 and p['y']<1.2 for p in objects):continue
        # Avoid placing loose objects inside the two large furniture surfaces.
        if (mi,ri)==(0,1) and (-7.6<x<-1.7 and -.2<z<3.2 or -.9<x<5.3 and z>3):continue
        if (mi,ri)==(1,0) and x>-.8 and z>2.2:continue
        if (mi,ri)==(1,2) and x<-.5 and z>1.8:continue
        if (mi,ri)==(2,1) and abs(z-.5)<1.15:continue
        asset=decoy_assets[count%len(decoy_assets)]
        objects.append(toy(asset,x,y,z,.46+(count%3)*.035,rotation=(count*47)%360))
        count+=1
        if count==12+3*mi:break
    assert count==12+3*mi,(r['id'],count)
    # The three searches form a cycle; the central map remains accessible from every room.
    titles=[['Estação','Salão','Mina no baú'],['Base','Jardim','Garagem'],['Praça','Ponte','Quartel']][mi]
    objects += [door('furniture-kit/doorwayFront',m['junction']['id'],'Caminhos',0,-5.35,1.1),
                door('survival-kit/signpost',m['rooms'][(ri-1)%3]['id'],'← '+titles[(ri-1)%3],-7.8,-5.2,1.0),
                door('survival-kit/signpost',m['rooms'][(ri+1)%3]['id'],titles[(ri+1)%3]+' →',7.8,-5.2,1.0)]
    if ri==2:objects.append(door(['survival-kit/chest','furniture-kit/radio','space-kit/desk_computer'][mi],m['challenge']['id'],'Desafio',5.6,-1.6,1.2))
    if r.get('scope')=='chest':
        for p in objects:
            if p.get('action')=='exit' and p['z']<0:p['z']=-4.35
    r.update(objects=objects,geometry=g,floor=floors[mi][ri],color=trims[mi],wall=['#796758','#343251','#435C62'][mi])


def junction_composition(mi,m):
    r=m['junction']; floor=['#C1976D','#77759B','#A4B7A3'][mi]; trim=['#A4654F','#4F477D','#598078'][mi]
    g=foundation(floor,trim,True)
    r.update(layout='map',scope='junction',name=['Salão dos Caminhos','Laboratório das Passagens','Rua dos Brinquedos'][mi],
             width=18.8,depth=13,camera=[.2,8.4,-17.6],lookAt=[0,1.7,1.3],fieldOfView=40,wall=['#6B5449','#35324F','#3D5D63'][mi])
    objects=[]
    for a,b,color in [((0,-5),(0,3.4),'#D7B787'),((0,-.8),(-6.4,2.9),'#C3A988'),((0,-.8),(6.3,2.9),'#B8C2A7')]:ribbon(g,a,b,1.1,color)
    # Portals occupy the architecture: stairs on the left, door ahead, open box on the right.
    for i,(x,title) in enumerate(zip([-6.4,0,6.4],['Estação','Salão','Mina no baú'] if mi==0 else ['Base','Jardim','Garagem'] if mi==1 else ['Praça','Ponte','Quartel'])):
        asset=[['castle-kit/stairs-stone','castle-kit/door','survival-kit/chest'],
               ['space-kit/hangar_smallA','furniture-kit/doorwayFront','space-kit/hangar_roundA'],
               ['city-kit-commercial/building-a','nature-kit/bridge_wood','city-kit-commercial/building-f']][mi][i]
        p=door(asset,m['rooms'][i]['id'],title,x,3.4,3.3 if i!=1 else 3.5)
        if mi==0 and i==1:p['rotation']=90
        objects.append(p)
        if i!=1:g.append(block('Arco colorido da passagem',x,2.4,5.86,4.2,4.8,.12,['#C18A59','#8A75AC','#83A29B'][mi]))
    objects += [door(['survival-kit/chest','furniture-kit/radio','space-kit/desk_computer'][mi],m['challenge']['id'],['Baú da senha','Rádio das estrelas','Comunicador'][mi],3.4,-3.0,2),
                door('furniture-kit/doorwayFront','quarto','Quarto',-5.4,-2.8,1.6,rotation=-10)]
    # Oversized household toys are part of the architecture, like the reference's castle/lab.
    accents=[('furniture-kit/books',-8,0,.2,2.1),('holiday-kit/lantern',-3.2,0,4.8,1.8),
             ('marble-kit/helix-left',7.8,0,-.5,2.1)]
    for index,(asset,x,y,z,size) in enumerate(accents):objects.append(toy(asset,x,y,z,size,0,'inspect','Um brinquedo ganhou vida',['tip','spin','somersault'][index],idle='sway'))
    g += [block('Tapete da entrada',0,.018,-3.5,5.1,.036,3.4,['#A35E58','#9A6E9F','#608C7D'][mi]),
          block('Faixa superior do cenário',0,5.25,6.0,18.5,.16,.3,'#E5C592')]
    if mi==1:g.append(block('Escotilha do laboratório',-3.1,3.9,5.88,2.3,.15,2.3,'#91CFDD',shape='cylinder',tiltX=90))
    if mi==2:objects.append(toy('castle-kit/tower-square-roof',8,0,4.8,2.6))
    r.update(objects=objects,geometry=g,floor=floor,color=trim)


def challenge_composition(mi,m):
    r=m['challenge']
    pieces=[copy.deepcopy(p) for p in r['objects'] if p.get('action') in ['digit','unlock','signal','replay','celebrate','supply','destination']]
    r.update(scope=['chest','room','object'][mi],wall=['#6F5444','#39304E','#344B56'][mi])
    if mi==0:
        r.update(camera=[0,7.6,-11.6],lookAt=[0,.85,.4],fieldOfView=40)
        g=[block('Bandeja de madeira do baú',0,-.3,.4,12.7,.6,8.6,'#A2724A'),
           block('Forro do baú da senha',0,.015,.4,12.1,.03,8,'#865568'),
           block('Tampa do baú da senha',0,1.7,4.5,12.7,3.4,.2,'#A2724A')]
        for p in pieces:
            if p['action']=='unlock':p.update(x=0,y=.03,z=1.4,size=3.8)
            if p['action']=='digit':p.update(x=[-3.1,0,3.1][p['value']],y=.03,z=-1.7,size=1.45)
    elif mi==1:
        r.update(camera=[.3,8.6,-15.2],lookAt=[0,1.0,.6],fieldOfView=41)
        g=foundation('#797A9A','#50446E',True)
        for p in pieces:
            if p['action']=='signal':
                i=p['value'];p.update(x=[-5.3,-1.9,1.8,5.2][i],y=[.05,.55,.05,.55][i],z=[-.5,1.0,-.5,1.0][i],size=1.65,
                                      asset=['space-kit/meteor','food-kit/orange','holiday-kit/snowflake-c','food-kit/donut'][i])
            elif p['action']=='replay':p.update(x=0,y=.05,z=3.7,size=3.2,asset='furniture-kit/radio',rotation=0)
            elif p['action']=='celebrate':p.update(x=-7.4,y=0,z=4,size=2.3)
        for p in pieces:
            if p['action']=='signal':
                bottom=p['y']+p['size']*.65
                g.append(block('Fio do móbile de estrelas',p['x'],(5+bottom)/2,p['z'],.022,5-bottom,.022,'#EBD3A8'))
        pieces += [toy('space-kit/rock_crystalsLargeA',7.3,0,4.4,2.0),toy('marble-kit/helix-left',-7.3,0,-2.2,1.7)]
    else:
        r.update(camera=[0,9.2,-10.3],lookAt=[0,.2,.2],fieldOfView=42)
        g=[block('Caixa do comunicador',0,-.3,0,14,.6,9,'#BAA078'),
           block('Circuito de feltro do comunicador',0,.025,0,13.4,.05,8.4,'#5A466D')]
        for x in [-6.4,6.4]:
            for z in [-3.7,3.7]:g.append(block('Parafuso de brinquedo',x,.08,z,.22,.12,.22,'#E4C774',shape='cylinder'))
        for p in pieces:
            if p['action'] in ['supply','destination']:
                i=p['value'];p.update(x=[-4.3,0,4.3][i],y=.05,z=-1.9 if p['action']=='supply' else 1.6,size=1.5 if p['action']=='supply' else 2.1)
            elif p['action']=='celebrate':p.update(x=-5.5 if p['x']<0 else 5.5,y=.05,z=3.1,size=.85)
        for x in [-4.3,0,4.3]:g.append(block('Fio decorativo do comunicador',x,.065,.2,.07,.025,1.0,'#CBA863'))
    pieces += [door('furniture-kit/doorwayFront',m['junction']['id'],'Caminhos',-4.9,-3.25,1.0),
               door('survival-kit/signpost',m['rooms'][2]['id'],['Mina no baú','Garagem','Quartel'][mi],4.9,-3.25,1.0)]
    r.update(objects=pieces,geometry=g)


def main():
    c=json.loads(CATALOG.read_text(encoding='utf-8'))
    before={p['id'] for m in c['maps'] for r in m['rooms'] for p in r['objects'] if p['id']}
    for mi,m in enumerate(c['maps']):
        m['gates']=[]
        for ri,r in enumerate(m['rooms']):search_composition(mi,ri,r,m)
        junction_composition(mi,m)
        challenge_composition(mi,m)
    # Match the reference's compact triangle: floor playsets around one tabletop rocket.
    h=c['hub']; h.update(camera=[0,7.7,-13.9],lookAt=[0,1.25,.9],fieldOfView=37,scope='bedroom')
    h['objects']=[p for p in h['objects'] if p.get('action')!='world' and not ('pillow' in p['asset'] and p['z']<0)]
    h['geometry']=[s for s in h['geometry'] if not any(m['id'] in s['name'] for m in c['maps'])]
    entrances=[(-2.05,.0,-.65,2.15,'survival-kit/structure-roof'),
               (0,2.26,2.6,1.65,'space-kit/craft_cargoA'),
               (2.05,.0,-.65,2.15,'city-kit-commercial/building-a')]
    for m,(x,y,z,size,asset) in zip(c['maps'],entrances):
        h['objects'].append(toy(asset,x,y,z,size,0,'world',m['title'],destination=m['id'],landmark=m['title']))
    c['layoutRevision']=14
    after={p['id'] for m in c['maps'] for r in m['rooms'] for p in r['objects'] if p['id']}
    assert before==after and len(after)==162
    CATALOG.write_text(json.dumps(c,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
    print('Recomposed 9 continuous searches and 3 architectural junctions; 162 IDs preserved.')


if __name__=='__main__':main()
