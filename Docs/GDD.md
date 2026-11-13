# Olhos de Caçador — GDD v0.7

Data: 13/11/2026. Documento do protótipo-fonte; o acabamento final depende de playtest no Unity e no Windows.

## Proposta

Um quintal, três caixas de brinquedos, muitas pequenas histórias. O jogador observa dioramas 3D, procura os objetos de uma charada e resolve um desafio ao terminar cada mundo. O “caçador” do título procura descobertas; não há caça de animais, violência, combate ou armas nas fases.

A experiência deve lembrar uma criança espalhando miniaturas para brincar de faroeste, astronauta e super-herói. O foco é reconhecer silhuetas, comparar pistas e explorar com calma. Público de referência: crianças alfabetizadas e pessoas que gostam de puzzles leves. A adequação de leitura, dificuldade e duração é uma hipótese para testar, não uma medição já realizada.

Duração desejada: aproximadamente 10–15 minutos na primeira partida. Controles: mouse, com Esc como atalho opcional. Plataforma de entrega: Windows x64. A implementação web foi retirada do escopo a pedido do usuário.

## Pilares

1. **A brincadeira transforma o lugar.** As salas são caixas abertas, os cenários são miniaturas e os nomes fazem parte da fantasia infantil.
2. **Observar recompensa.** O clique só coleta o alvo realmente visível no primeiro contato do raio; o inventário conta descobertas sem duplicá-las.
3. **Errar faz parte.** Um erro nunca apaga a caça anterior. O jogador pode rever a sequência, trocar uma combinação, consultar pistas ou reiniciar.
4. **Escopo pequeno, caminho completo.** Três mundos curtos, cada um com começo, exploração, puzzle e recompensa.

## Ciclo de jogo

Tela de início → Vamos brincar! → escolher um brinquedo no quarto → ler a missão → explorar três telas → encontrar seis objetos → consultar o caderno → resolver o puzzle → ganhar um selo. Com três selos, aparece a conclusão geral. Os mundos são independentes e podem ser jogados em qualquer ordem.

Não existe cronômetro, pontuação por velocidade ou limite de tentativas. Isso favorece exploração e reduz a necessidade de balancear uma economia de dicas. O progresso é local ao usuário Windows e não acompanha automaticamente o jogador em outro computador. A chave permanece OlhosDeCacador.v1; os dados de uma antiga execução no navegador não são transferidos automaticamente para o executável.

## Mundos e desafios

### Faroeste de Quintal

A turma prepara um piquenique e precisa abrir o baú. O faroeste aparece nas barracas, cactos, caixas, ferramentas e cenário de quintal. O “Salão do Suco” substitui um saloon adulto. A caça inclui balde, garrafinha, martelo, peixe, picareta e garrafa grande.

| Tela | Composição | Pista principal |
| --- | --- | --- |
| Estação do Quintal | Pequena construção, cerca, cactos e bagagens | Primeiro número: 2 |
| Salão do Suco | Bancada, recipientes e objetos fora do lugar | Segundo número: 4 |
| Mina dos Brilhos | Barraca, pedras e baú | Terceiro número: 3 |

Puzzle: três garrafinhas clicáveis controlam números de 0 a 9; o baú recebe a senha **243**. A entrada está no baú da Mina e o desafio tem um enquadramento 3D próprio. As pistas relevantes são registradas ao encontrar alvos específicos. Erro mostra uma mensagem e mantém os números para ajuste. Recompensa: “Xerife da Brincadeira”.

### Missão Caixa Lunar

Uma caixa vira base espacial. A central perdeu o sinal e precisa de seis objetos da missão: dois astronautas, antena, visitante alienígena, meteoro e nave pequena. As telas são Base de Papelão, Jardim de Meteoros e Garagem das Naves.

Puzzle: quatro brinquedos 3D identificados como **Lua, Sol, Estrela, Planeta**, acessados pelo rádio da Garagem. O botão “Assistir / repetir” mostra **Estrela → Lua → Planeta → Sol**. Durante a apresentação, a entrada dos sinais fica bloqueada. Uma resposta errada reinicia somente a tentativa da sequência. Sinais são escritos por extenso e acompanhados de áudio opcional. Recompensa: “Explorador Lunar”.

### Liga dos Brinquedos

A Capitã Faísca e o Robô Violeta ajudam a preparar a cidade para uma festa. A temática de super-herói aparece como uma missão de cuidado, com bonecos e prédios de miniatura. As telas são Praça dos Blocos, Ponte do Faz de Conta e Quartel das Boas Ideias.

Puzzle de associação em dois cliques, acessado pela maquete do Quartel: selecionar uma ferramenta 3D e depois a miniatura de um destino. Ao acertar, a ferramenta é levada ao lugar; com movimento reduzido, muda de posição imediatamente. O **balde** atende a praça com flores secas; o **martelo** atende a ponte; a **antena** atende o quartel. Associações corretas ficam marcadas e não podem ser usadas duas vezes. Uma tentativa incorreta mantém os acertos. Recompensa: “Herói do Quintal”.

## Apresentação visual

Dioramas abertos vistos por uma câmera de perspectiva estável. Sem FPS, câmera livre ou personagem controlável. O movimento curto entre telas pode ser desativado. Volume, escala relativa, sobreposição de objetos e sombras devem dar a sensação 3D.

Paletas: ocre, turquesa e terracota no faroeste; roxo, azul e amarelo na missão espacial; verde-água, dourado e coral na cidade. Bordas e blocos coloridos reforçam a caixa de brinquedos. Os assets vêm de um mesmo autor para favorecer coerência. As cores e texturas originais são ligadas explicitamente a materiais por modelo; UVs fora do intervalo 0–1 usam Repeat. As salas usam primitivas. Placas, molduras e estrelas da interface são arquivos originais do UI Pack Adventure (CC0), sem ilustrações ou texturas novas.

A análise do vídeo enviado de I Spy Fantasy foi aplicada na v0.7: área 3D ampliada a 704 de 900 px, 271 instâncias nas telas de busca, cenário em camadas, quarto com brinquedos de entrada, placas de navegação, reações e desafios em 3D. A implementação usa somente os assets prontos já incluídos; materiais e texturas originais foram preservados. Consulte REFERENCIA_VIDEO.md para o mapeamento das ideias e limites da validação.

A Capitã Faísca usa o robô original character-g (detalhes vermelhos); o Robô Violeta usa character-h; o ajudante amarelinho usa character-d. As charadas e dicas correspondem às cores dos modelos originais.

A colocação inicial prioriza alvos com silhueta legível e tamanho confortável. Não escondemos um alvo inteiro atrás de outro objeto. Após o playtest, ajustar posições e distratores no catálogo para aumentar a busca sem transformar o desafio em clicar num pixel. Não mudar a escala de forma que uma ferramenta fique irreconhecível.

Som: sete clipes prontos de UI Audio para navegação, descoberta e sinais. Nove campos opcionais recebem a voz humana do autor, com volume independente e opção de leitura automática. Não há gravações de narração nem trilha musical incluídas. O jogo continua utilizável só com texto. Consulte NARRACAO.md.

## Interface e ajuda

Uma abertura separada apresenta o título grande, violeta, coral, turquesa e amarelo, estrelas originais do pack e três miniaturas com os assets do jogo. O botão Vamos brincar! (também Enter/Espaço) leva à escolha dos mundos, preservando o progresso. Opções, ajuda, créditos e Sair do jogo estão disponíveis já na abertura. A pausa também oferece saída, com confirmação. O movimento suave dos brinquedos, estrelas e botão é inteiramente desativado pela preferência de movimento reduzido.

O quarto apresenta baú, nave e heroína como entradas clicáveis. Na exploração, o cabeçalho oferece Quarto, Caderno e Pausa. Duas placas nas laterais levam aos outros cantinhos; cada uma também tem rótulo clicável. A faixa inferior de 140 px contém a charada em duas colunas, dois botões de dica e a Voz da aventura. Ler juntos permanece disponível mesmo quando há gravação; Ouvir charada depende do áudio da sala. O desafio é acessado por um objeto do cenário depois dos seis alvos.

Opções é uma tela própria, acessível do início e da pausa: volume de efeitos, volume do narrador, áudio geral, leitura automática e movimento reduzido. Preferências são persistidas. Abrir pausa, opções, caderno ou puzzle interrompe a leitura; a gravação anterior nunca se sobrepõe à sala seguinte. O Desafio continua habilitado somente após os seis alvos.

Cada alvo tem ajuda progressiva: indicar a tela; explicar a posição; destacar o objeto por uma pequena mudança de escala. Em movimento reduzido, a última ajuda usa aumento estático. A descrição continua disponível em texto. Evitar depender apenas da cor.

A versão não é totalmente acessível por teclado ou leitor de tela: a caça depende de apontar para objetos 3D. Essa limitação deve ser informada se o público da entrega exigir outros meios de entrada.

## Estados e recuperação

| Estado | Entrada | Saída |
| --- | --- | --- |
| Tela de início | Inicialização ou retorno pelo menu | Vamos brincar!, ajuda, opções ou créditos |
| Quarto de mundos | Vamos brincar! ou botão Quarto | Clique em baú/nave/heroína; Esc ou Tela inicial retorna à abertura |
| Apresentação | Primeira entrada sem descobertas no mapa | Começar exploração |
| Exploração | Cenário ativo | Pausa, outro lugar, caderno, puzzle ou menu |
| Pausa | Esc ou botão | Continuar, ajuda, opções, reinício ou menu |
| Opções | Início, menu ou pausa | Voltar à origem, mantendo preferências |
| Leitura | Ler juntos, com ou sem gravação | Voltar à busca |
| Puzzle | Seis alvos coletados | Vitória, nova tentativa ou voltar à busca |
| Vitória | Puzzle resolvido | Menu ou visita ao mapa |
| Erro de conteúdo | Modelo ou catálogo indisponível | Mensagem e tentativa de recarregar |

Salvamento contém IDs dos alvos e mapas concluídos. A carga elimina entradas desconhecidas, duplicadas e conclusões impossíveis. Uma cópia de recuperação é mantida. Se ambas forem ilegíveis, uma nova partida fica disponível com aviso. Falhas ao salvar não impedem continuar a sessão. Puzzles parcialmente respondidos não são persistidos; as descobertas e os selos são.

## Arquitetura

| Arquivo | Responsabilidade |
| --- | --- |
| `Runtime/Domain/GameData.cs` | Catálogo, progresso e regras puras dos puzzles |
| `Runtime/HunterGame.cs` | Fluxo, salvamento, áudio e coordenação dos puzzles |
| `Runtime/HunterGame.World.cs` | Câmeras por tela, rótulos projetados, raycast e ações dos objetos |
| `Runtime/WorldHotspot.cs` | Passagens, entradas e peças; reações e deslocamento de ferramentas |
| `Runtime/RoomView.cs` | Instanciar assets prontos, dimensionar a sala e configurar colisores |
| `Runtime/HuntTarget.cs` | Identidade e feedback dos alvos |
| `Runtime/HunterUI.cs` | Interface com sprites prontos e texto |
| `Runtime/ToyShelf.cs` | Miniaturas 3D decorativas e animação suave da abertura |
| `Runtime/NarrationBank.cs` | Associação entre charadas e gravações humanas |
| `Editor/HunterImporter.cs` | Importação dos OBJ/MTL, texturas e áudio |
| `Editor/HunterBuild.cs` | Preparação, conferência de conteúdo e builds |
| `Editor/HunterRenderPipeline.cs` | Configurar e verificar URP em Graphics e em todos os níveis de Quality |
| `Settings/HunterURP.asset` e `Settings/HunterRenderer.asset` | URP 17.6 em Forward, com sombras e MSAA definidos no pipeline |
| `Resources/Hunter/catalog.json` | Conteúdo dos três mundos e suas posições |

Durante a busca, apenas uma sala está ativa. A abertura instancia três miniaturas decorativas. O quarto é uma composição interativa própria. Durante um desafio, a sala de busca fica inativa e o cenário do puzzle é descartado ao sair. Os colisores e componentes de seleção dessas prévias ficam desativados. Na exploração, todos os objetos de cenário participam da oclusão do raycast. A física serve para apontar e bloquear seleção através de superfícies; não há dinâmica de personagem, Rigidbody ou NavMesh. A UI é consultada antes do raycast para separar cliques de menu e de mundo.

## Escopo posterior

As adaptações do vídeo foram implementadas conforme REFERENCIA_VIDEO.md. Falta validá-las no editor e no executável Windows e medir desempenho. As gravações humanas devem ser adicionadas pelo narrador. Tradução, animação de personagens, arrastar objetos, fases procedurais e câmera livre ficam fora deste escopo.

## Registro de decisões

- 13/11/2026: adotados três mundos independentes com três telas cada.
- 13/11/2026: descartado combate; os heróis resolvem necessidades da cidade.
- 13/11/2026: nenhum asset artístico novo; objetos e sons Kenney CC0, salas primitivas.
- 13/11/2026: incluído destino Windows após a leitura da rubrica.
- 13/11/2026: entrega atual limitada a fonte, conteúdo e validações externas; build e playtest aguardam Unity.

- 13/11/2026: versão-alvo alterada para Unity 6000.6.4f1, a pedido do usuário; uGUI 2.6.0 e Test Framework 1.8.0. Compilação e playtest continuam pendentes.

- Revisão v0.2: referências explícitas de materiais; Repeat para UVs negativas; caixa mais colorida; menu com miniaturas; opções em tela própria; nove charadas e espaços para narração humana.

- Revisão v0.3: tela de início separada, título chamativo, animação discreta de assets existentes, atalho Enter/Espaço, retorno à abertura e movimento reduzido aplicado a todas as animações da abertura.

- Revisão v0.4: migração para URP 17.6.0 mantendo Unity 6000.6.4f1; 167 materiais URP/Lit preservam as cores/texturas originais; câmeras e materiais das salas adaptados; Graphics e Quality configurados. Sem arte nova ou mudança de regras. Compilação e playtest continuam pendentes.

- Revisão v0.5: destino exclusivo Windows x64; build Mono, janela redimensionável, saída confirmada na abertura/pausa e documentação atualizada. As adaptações visuais do vídeo continuam pendentes.
- Revisão v0.6: Input System 1.20.1 como único backend de entrada; mouse/teclado e interface migrados. Importação explícita de texturas 2D; reparo de materiais e remaps separado dos callbacks de importação. Nenhuma alteração de arte, charadas, catálogo ou gravações.

- Revisão v0.7: metadados suportados para 83 modelos e sete áudios; viewport ampliado, 271 instâncias nas buscas, quarto com entradas 3D, 18 passagens, três desafios em cenas próprias e reações usando os modelos existentes. Nenhuma arte nova; IDs, charadas, vozes e soluções preservados.
