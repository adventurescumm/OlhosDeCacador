# Sons e fontes — v0.10

Os arquivos foram procurados e baixados de fontes existentes. Não houve geração, recorte, alteração de pitch ou síntese dos assets sonoros. Nenhum som de I Spy foi incorporado ao jogo. O vídeo serviu como referência de respostas sonoras ligadas aos objetos e de uma interface discreta.

## Fontes originais

| Uso | Fonte | Autoria e licença |
| --- | --- | --- |
| Charadas e textos | [Itim](https://github.com/google/fonts/tree/main/ofl/itim) | Cadson Demak / Pablo Impallari — SIL OFL 1.1 |
| Títulos e botões | [Boogaloo](https://github.com/google/fonts/tree/main/ofl/boogaloo) | John Vargas Beltrán — SIL OFL 1.1 |

São os TTF estáticos originais; acentos portugueses foram conferidos. Os versos usam uma marca de ponto e cor após a coleta. Lilita One permanece no pacote para não quebrar referências antigas, mas a nova interface usa Itim/Boogaloo.

## Gravações prontas

| Fonte | Seleção incluída | Uso |
| --- | --- | --- |
| [Impact Sounds — Kenney](https://kenney.nl/assets/impact-sounds), CC0 | 12 OGG: passos de madeira, carpete, concreto e grama; impactos leves/médios de madeira, lata, metal, vidro, sino, mineração e superfície macia | Objetos, bonecos, passagens e mecanismos |
| [Sci-fi Sounds — Kenney](https://kenney.nl/assets/sci-fi-sounds), CC0 | 8 OGG: três sons de computador, porta abrindo/fechando, motor grave, campo de força e propulsor | Eletrônica, naves e passagens espaciais |
| [Ambient Bird Sounds — isaiah658](https://opengameart.org/content/ambient-bird-sounds), CC0 | `birds-isaiah658_0.ogg`, gravação original completa | Estação, entrada do faroeste e praça dos heróis |

As 21 adições somam-se aos sete efeitos UI Audio anteriores. Os nomes originais e a procedência de cada arquivo estão em `asset-manifest.json`. Todos os 28 OGG foram decodificados externamente sem erro. Isso não substitui ouvir o resultado no Unity e ajustar a mixagem em contexto.

## Relação com cada mundo

| Mundo | Passagens e ações | Ambiente |
| --- | --- | --- |
| Faroeste | Passos de madeira, encaixes de madeira, vidro nos frascos, metal nas ferramentas e sino nas peças brilhantes | Pássaros discretos na estação e entrada; interiores sem loop permanente |
| Espaço | Porta espacial, computadores, campo de força e materiais dos objetos | Motor grave intermitente, com intervalos de silêncio |
| Super-heróis | Passos de concreto, peças de lata/metal, vidro e brinquedos macios | Pássaros discretos na praça; demais áreas sem ambiente constante |
| Quarto | Brinquedos macios e madeira | Sem ambiente constante |

O mesmo tipo de objeto conserva um som reconhecível entre salas. O ambiente toca numa fonte separada e não acumula repetições. Abrir painéis ou sair do jogo interrompe o ambiente; durante a voz humana, o ambiente cai para 15% do nível configurado e os efeitos também são reduzidos. O volume Sons dos brinquedos controla efeitos e ambiente; Voz do narrador é independente.

## Editar sem gerar assets

- `Assets/Resources/Hunter/soundscapes.json`: sons por cena, ganho do ambiente e intervalo entre reproduções.
- `PropData.sound` no catálogo/Inspector: caminho relativo a Hunter/Audio, sem extensão, para resposta específica do objeto.
- `Assets/ThirdPartyLicenses`: textos de licença originais e referência da gravação de pássaros.

Os sons do vídeo não foram reutilizados. A aproximação procurada é a coerência entre brinquedo, material, movimento e som; não uma cópia exata da trilha de I Spy.
