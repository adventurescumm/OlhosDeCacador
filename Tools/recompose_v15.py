"""Place searches in irregular scenic niches and rewrite observation riddles.

All 162 discovery IDs, labels, puzzle clues and save locations are retained.
Cover props reuse existing meshes. No target is removed or made noninteractive.
"""
import copy,json,math,random,re
from pathlib import Path
import recompose_v14 as base

ROOT=Path(__file__).resolve().parents[1]
PATH=ROOT/'Assets/Resources/BemTeVi/catalog.json'

# Six anchors per round, in the same order as the six narrated verses.
# Heights are actual support surfaces, not rows in a display grid.
ANCHORS={
 'faroeste_estacao':[
  (-6.1,.04,3.15),(-4.8,.04,2.15),(4.35,.04,2.95),(-.9,.04,3.05),(-3.35,.04,-.55),(2.55,.04,-.85),
  (-6.9,.04,1.75),(-1.8,.04,.85),(6.85,.04,2.0),(-6.55,.04,4.15),(3.4,.04,1.25),(.9,.04,3.5),
  (4.4,.04,4.1),(-5.5,.04,1.35),(.15,.04,3.1),(-.5,1.15,3.85),(1.05,1.85,3.85),(-3.9,.04,3.65)],
 'faroeste_salao':[
  (6.0,.04,2.55),(-3.3,2.12,1.6),(-5.8,2.12,.45),(1.35,1.25,4.0),(-4.6,2.12,2.35),(-1.85,.04,-2.45),
  (-6.3,2.12,2.0),(6.8,.04,3.3),(-4.35,2.12,.35),(3.7,2.85,4.25),(5.15,.04,2.8),(-6.5,.04,-1.55),
  (-3.55,2.12,2.55),(2.4,1.25,4.0),(1.25,2.85,4.0),(-.5,1.25,3.9),(3.0,1.25,4.05),(-.2,2.85,4.2)],
 'faroeste_mina':[
  (-6.4,.04,3.3),(-3.9,.04,-.45),(.1,.04,3.65),(-5.1,.04,1.6),(5.65,.04,3.75),(-2.6,.04,1.3),
  (2.2,.04,.3),(-1.9,.04,-2.2),(4.55,.04,1.15),(-6.7,.04,.05),(2.75,.04,3.9),(.55,.04,1.1),
  (6.3,.04,3.6),(-.65,.04,3.15),(-5.65,.04,2.3),(3.9,.04,-1.7),(-3.0,.04,3.55),(1.15,.04,-1.55)],
 'espaco_base':[
  (-5.7,.04,3.75),(4.95,2.02,4.1),(-3.3,.04,-.6),(1.0,2.02,3.1),(-6.8,.04,1.2),(3.35,2.02,3.35),
  (-3.7,.04,2.25),(2.1,2.02,4.5),(5.6,2.02,2.9),(-1.9,.04,.3),(2.7,2.02,4.25),(6.2,.04,-2.1),
  (-5.95,.04,2.2),(-4.6,3.2,5.8),(.25,2.02,4.45),(-6.55,.04,-1.75),(4.15,2.02,3.5),(-.7,.04,2.25)],
 'espaco_jardim':[
  (5.95,.04,2.9),(-5.7,.04,2.25),(1.35,.04,3.1),(-2.1,.04,.2),(-5.05,.04,-1.15),(5.5,.04,-.1),
  (2.9,.04,-2.0),(-.9,.04,3.9),(4.2,.04,1.4),(-3.8,.04,2.15),(-5.85,.04,.6),(2.35,.04,2.8),
  (-6.2,.04,1.3),(3.9,.04,-1.5),(.1,.04,2.8),(5.6,.04,3.85),(-1.75,.04,-1.8),(-3.15,.04,3.2)],
 'espaco_garagem':[
  (4.2,.04,3.35),(-3.45,1.46,3.2),(5.1,.04,2.5),(-5.8,.04,-1.35),(-6.15,1.46,3.3),(-1.9,1.46,2.8),
  (-4.6,1.46,4.3),(6.3,.04,2.4),(-2.45,1.46,4.25),(-3.5,.04,.3),(-6.6,1.46,2.5),(2.3,.04,-1.4),
  (-5.15,1.46,3.4),(3.85,.04,1.9),(-1.6,1.46,4.45),(-6.0,.04,.9),(6.4,.04,.8),(1.2,.04,1.1)],
 'herois_praca':[
  (-5.3,.04,3.4),(5.35,.04,2.6),(-2.4,.04,-.75),(2.25,.04,.9),(-.3,.04,3.0),(-6.35,.04,.6),
  (3.75,.04,2.8),(-4.3,.04,1.9),(1.0,.04,-2.2),(-6.9,.04,2.45),(4.2,.04,-.8),(-1.6,.04,3.8),
  (6.05,.04,3.2),(-5.4,.04,.2),(2.9,.04,1.8),(-2.1,.04,2.25),(.5,1.15,3.3),(3.0,.04,-1.55)],
 'herois_ponte':[
  (-2.0,.04,2.75),(3.7,.04,-.9),(-4.55,.04,3.1),(5.7,.04,2.5),(-5.6,.04,-1.4),(1.55,.04,-2.55),
  (-6.45,.04,2.6),(2.3,.04,3.6),(-2.7,.04,-2.0),(4.9,.04,-2.5),(-4.75,.04,1.9),(-.2,.04,3.2),
  (6.05,.04,3.4),(-3.8,.04,-1.3),(2.0,.04,2.55),(5.4,.04,-.9),(-1.35,.04,4.0),(-6.75,.04,-2.3)],
 'herois_quartel':[
  (-4.55,.85,3.4),(2.45,2.4,3.45),(-3.3,.04,-2.0),(4.8,3.95,3.45),(5.45,.85,3.5),(-.65,2.4,3.45),
  (-5.8,3.95,3.45),(1.5,.04,-1.0),(-.3,.85,3.45),(-4.0,2.4,3.45),(2.95,3.95,3.4),(6.0,.04,-2.65),
  (-2.1,3.95,3.45),(3.7,.85,3.45),(.25,3.95,3.4),(-1.95,.85,3.5),(6.0,2.4,3.45),(1.55,2.4,3.45)]
}

# Fine placement at the exposed lip of a container or the front face of a book.
for room,changes in {
 'faroeste_estacao':{11:(.9,.04,2.85),15:(-.5,1.15,2.99),16:(1.05,1.85,2.68)},
 'faroeste_salao':{4:(-4.8,2.12,2.4),12:(-3.4,2.12,2.0)},
 'faroeste_mina':{6:(2.2,.49,.3)},
 'espaco_base':{6:(-3.3,.04,1.3),13:(-4.6,3.2,5.60)},
 'espaco_garagem':{0:(3.8,.04,1.95),9:(-4.35,.04,-.05),11:(1.8,.04,-1.85),13:(2.7,.04,1.6)},
 'herois_praca':{11:(-2.0,.04,3.4)},
 'herois_quartel':{12:(-1.4,3.95,3.25)}
}.items():
    for index,point in changes.items():ANCHORS[room][index]=point

VERSES={
 'bucket':'Uma alça faz ponte sobre um poço de levar água',
 'bottle':'Uma garrafinha verde, de pescoço fino e comprido',
 'bottle-large':'A garrafa mais alta, que não cresce nem um fio',
 'wheel-medium':'Uma roda sem carro, esperando para rodar',
 'wheel-small':'A rodinha pequena que perdeu seu carrinho',
 'apple':'Uma maçã redondinha com um cabinho no alto',
 'donut-sprinkles':'Um anel coberto de pequenos confeitos',
 'rubber_duck_toy_1k':'Um nadador de borracha que tem bico de pato',
 'pear':'Uma pera de barriga larga e pescoço estreito',
 'mug':'Uma caneca guarda um abraço em sua alça',
 'box':'Um caixote de madeira, com quinas para contar',
 'lantern':'Uma lanterninha com janelas e alça para levar',
 'train-locomotive':'O primeiro do trenzinho, que puxa sem cansar',
 'gingerbread-woman':'Uma boneca de biscoito com saia para rodar',
 'nutcracker':'Um soldadinho duro que nasceu para quebrar nozes',
 'candy-cane-red':'Um doce de bengala com listrinhas vermelhas',
 'candy-cane-green':'Uma bengala doce com listrinhas verdes',
 'present-a-round':'Um presente que esqueceu de ter quinas',
 'present-b-cube':'Um presente quadradinho vestido com um laço',
 'sock-green':'Uma meia verde esperando um pé de brinquedo',
 'snowflake-c':'Uma estrelinha de inverno que nunca derrete',
 'reindeer':'Uma rena de brinquedo, com galhinhos na cabeça',
 'tool-hammer':'Uma cabeça pesada presa num cabo de consertar',
 'fish':'Um peixe fora da água, parado sem se afogar',
 'cup-tea':'Uma xícara pequena que trouxe seu próprio pires',
 'cookie-chocolate':'Um biscoito com pintinhas de chocolate',
 'banana':'Uma fruta amarela que desenha uma meia-lua',
 'books':'Várias histórias apertadinhas numa pilha de livros',
 'carrot':'Uma raiz comprida que veste folhas no cabelo',
 'barrel-open':'Um barril de boca aberta que esqueceu sua tampa',
 'snowman-hat':'Três bolas de neve que saíram de chapéu',
 'train-tram-round':'Um bonde arredondado que passeia sem trilhos',
 'train-carriage-tank':'Um vagão com uma barriga de tanque',
 'tool-pickaxe':'Duas pontas de cavar se equilibram num cabo',
 'item-coin-gold':'Um pequeno sol de ouro que cabe no bolso',
 'mushroom':'Um chapéu de cogumelo que cresceu sem chover',
 'rock_crystals':'Pedrinhas pontudas reunidas num só cristal',
 'train-wagon':'Um vagão espera alguém puxar a brincadeira',
 'astronautA':'Um astronauta veste sua roupa de visitar estrelas',
 'astronautB':'Um piloto de capacete pronto para outra galáxia',
 'satelliteDish':'Uma tigela de antena escuta o céu em silêncio',
 'satelliteDish_detailed':'Um comunicador aponta sua antena para longe',
 'vehicle-racer':'Um carrinho de corrida que nunca pede gasolina',
 'vehicle-speedster':'Um carro baixinho com pressa de brincar',
 'vehicle-truck':'Um caminhão pequeno leva uma carga imaginária',
 'computerMouse':'Um rato de computador sem bigodes nem rabo',
 'computerKeyboard':'Um teclado tem muitas teclas e nenhuma música',
 'lollypop':'Um doce redondinho se equilibra num palito',
 'lampRoundTable':'Um abajur de chapéu redondo clareia a brincadeira',
 'lampSquareTable':'Um abajur com chapéu de quatro cantinhos',
 'cherries':'Duas frutinhas dividem o mesmo cabinho',
 'ice-cream-cup':'Um sorvete no copinho que não quer derreter',
 'eggplant':'Uma berinjela veste um gorro de folhas',
 'alien':'Um visitante de outro mundo perdeu sua nave',
 'meteor_detailed':'Um meteoro marcado caiu sem fazer barulho',
 'radio':'Uma caixa de rádio espera uma canção',
 'orange':'Uma laranja redondinha brinca de ser um sol',
 'watermelon':'Uma fatia de melancia sorri com suas sementinhas',
 'craft_speederA':'Uma nave pequenina tem pressa de decolar',
 'pillowBlue':'Uma almofada azul espera um cochilo de boneco',
 'character-g':'A Capitã Faísca está pronta para outra aventura',
 'character-h':'O Robô Violeta não precisa descansar',
 'character-d':'Um ajudante amarelinho veio fazer companhia',
 'cupcake':'Um bolinho de festa usa um chapéu de cobertura',
 'strawberry':'Um morango tem pontinhos e uma coroa de folhas',
 'cookie':'Um biscoito redondo faz de conta que é uma roda',
 'ginger-bread':'Um biscoito tem braços e duas perninhas para andar',
 'item-cone':'Um cone de trânsito tem ponta e pé quadrado',
 'popsicle':'Um picolé no palito que não faz nenhuma poça'
}


def dimensions(asset,size):
    p=ROOT/('Assets/Resources/BemTeVi/Models/'+asset+'.obj')
    if not p.is_file():return (size,size,size)
    vertices=[tuple(map(float,line.split()[1:4])) for line in p.read_text(encoding='utf-8').splitlines() if line.startswith('v ')]
    span=[max(v[i] for v in vertices)-min(v[i] for v in vertices) for i in range(3)]
    return tuple(s/max(span)*size for s in span)


def lean_offset(asset,size,tilt_x,tilt_z):
    dx,dy,dz=dimensions(asset,size);rx,rz=map(math.radians,[tilt_x,tilt_z])
    return -min(math.cos(rx)*math.sin(rz)*x+math.cos(rx)*math.cos(rz)*y-math.sin(rx)*z
                for x in [-dx/2,dx/2] for y in [0,dy] for z in [-dz/2,dz/2])


def compose_room(mi,ri,r):
    rng=random.Random(501+mi*13+ri)
    targets=[p for round in range(3) for p in r['objects'] if p['id'] and p['round']==round]
    assert len(targets)==18
    target_assets={p['asset'] for p in targets}
    # Closed cases, barrels and books add a coherent context around the hiding places.
    cover_pool=[a for a in ['survival-kit/barrel','survival-kit/box-large','furniture-kit/books','survival-kit/box','survival-kit/fence','furniture-kit/pottedPlant'] if a not in target_assets]
    supports=[];covers=[]
    for i,(p,anchor) in enumerate(zip(targets,ANCHORS[r['id']])):
        x,y,z=anchor;round=p['round'];size=[.80,.72,.65][round]-mi*.015
        angle=rng.choice([-32,-17,11,27,41,63,102,164])
        if any(k in p['asset'] for k in ['character-','ginger','reindeer','astronaut','nutcracker']):angle=rng.choice([-28,-12,14,32])
        if p['asset'].endswith('/radio'):angle=0
        tx=tz=0
        _,height,depth=dimensions(p['asset'],size)
        if height<size*.3:tx=rng.choice([18,28,58,74])
        if any(k in p['asset'] for k in ['candy-cane','sock-','snowflake']):tz=rng.choice([-26,19,34])
        if r['id']=='faroeste_estacao' and i in [15,16]:tx=76;angle=0
        p.update(x=x,y=round_float(y+lean_offset(p['asset'],size,tx,tz)),z=z,size=round_float(size),rotation=angle,tiltX=tx,tiltZ=tz)
        p['searchExposure']=round_float([.76,.64,.53][round] + (i%3-1)*.06)
        # Flat objects hide by shape/colour among neighbours; taller pieces also peek past a cover.
        _,h,d=dimensions(p['asset'],size)
        wall_mounted=(r['id']=='faroeste_estacao' and i in [15,16] or r['id']=='espaco_base' and i==13 or r['id']=='herois_praca' and i==16)
        if h<.23 or i in [0,5,7,12] or wall_mounted or r['id']=='faroeste_mina' and i==6:continue
        asset=cover_pool[(i+ri)%len(cover_pool)]
        cover_size=min(1.05,max(.43,h*.78+.22))
        cover=base.toy(asset,x+.12,y,z-.57,cover_size,rng.randrange(0,50),conceals=p['id'])
        covers.append(cover)
    # Move the old loose extras into small, irregular neighbourhoods around the scenic anchors.
    extras=[p for p in r['objects'] if not p['id'] and not p.get('action') and p['size']<.56]
    for index,p in enumerate(extras):
        t=targets[(index*5+ri)%18]
        # Keep neighbours on the same support plane; no extra row along the front of the room.
        a=ANCHORS[r['id']][(index*5+ri)%18]
        direction=1 if index%2 else -1
        if 'wheel' in p['asset'] and any('wheel' in asset for asset in target_assets):p['asset']='nature-kit/pot_large'
        p.update(x=round_float(t['x']+direction*(.55+.1*(index%3))),y=a[1],
                 z=round_float(t['z']+(.25 if index%3 else -.4)),size=.37+(index%3)*.05,rotation=(index*61+mi*19)%360)
        if abs(p['x'])>8.4:p['x']=math.copysign(8.4,p['x'])
        if r['id']=='faroeste_estacao' and a[1]>0 or r['id']=='espaco_base' and a[1]>3 or r['id']=='herois_praca' and a[1]>0:
            p['y']=.04;p['z']-=.7
    # Extra surroundings break the sparse, isolated silhouettes without duplicating any target.
    clusters={
      'faroeste_estacao':[('survival-kit/box-large',-3.4,0,-.1,1.35),('survival-kit/barrel',2.6,0,-.4,1.1)],
      'faroeste_salao':[('food-kit/mug',-4.1,2.12,1.95,1.05),('survival-kit/box-large',6.8,0,2.55,1.15)],
      'faroeste_mina':[('space-kit/rocks_smallB',-4.8,0,2.5,1.7),('survival-kit/box-open',2.2,0,.3,1.4),('survival-kit/box',2.2,.04,.3,.45),('pirate-kit/boat-row-small',-.7,0,-1.9,1.7)],
      'espaco_base':[('space-kit/barrels',-4.2,0,2.4,1.7),('space-kit/desk_chairStool',-2.3,0,-.5,1.4)],
      'espaco_jardim':[('space-kit/craterLarge',-4.8,0,1,2.5),('space-kit/rocks_smallB',3.4,0,2.8,1.9)],
      'espaco_garagem':[('survival-kit/box-open',-3.4,0,.35,1.35),('space-kit/barrels_rail',2.7,0,-1,1.5)],
      'herois_praca':[('survival-kit/box-large',-1.5,0,-.6,1.35),('nature-kit/fence_simple',3.4,0,1.5,1.6)],
      'herois_ponte':[('survival-kit/box-large',-4.7,0,2.5,1.2),('nature-kit/tree_small',5.8,0,1.8,1.4)],
      'herois_quartel':[('survival-kit/box-open',-1.1,.85,3.5,1.0),('furniture-kit/books',-2.5,3.95,3.6,1.25)]}
    for asset,x,y,z,size in clusters[r['id']]:
        if asset not in target_assets:supports.append(base.toy(asset,x,y,z,size))
    r['objects'] += supports+covers
    r['riddles']=['\n'.join(VERSES[p['asset'].split('/')[-1]]+('.' if i==5 else ';')
                    for i,p in enumerate([p for p in targets if p['round']==round])) for round in range(3)]
    r['riddle']=r['riddles'][0]


def round_float(value):return round(value,4)


def main():
    # Recreate the established architecture first; a second invocation never accumulates props.
    base.main()
    c=json.loads(PATH.read_text(encoding='utf-8'))
    before={p['id'] for m in c['maps'] for r in m['rooms'] for p in r['objects'] if p['id']}
    for mi,m in enumerate(c['maps']):
        for ri,r in enumerate(m['rooms']):compose_room(mi,ri,r)
    after={p['id'] for m in c['maps'] for r in m['rooms'] for p in r['objects'] if p['id']}
    assert before==after and len(after)==162
    c['layoutRevision']=15
    PATH.write_text(json.dumps(c,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
    scripts={(r['id'],i):r['riddles'][i] for m in c['maps'] for r in m['rooms'] for i in range(3)}
    bank=ROOT/'Assets/Resources/BemTeVi/Narration/NarrationBank.asset'
    text=bank.read_text(encoding='utf-8')
    pattern=r'(  - roomId: ([^\n]+)\n    round: (\d+)\n    recordingScript: )[^\n]+'
    text,count=re.subn(pattern,lambda match:match[1]+json.dumps(scripts[(match[2],int(match[3]))],ensure_ascii=False),text)
    assert count==27
    bank.write_text(text,encoding='utf-8')
    print('Recomposed 162 targets in scenic niches; rewrote 27 six-line riddles, preserving IDs and puzzle clues.')


if __name__=='__main__':main()
