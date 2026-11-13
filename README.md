# Olhos de Caçador

Revisão v0.7: importadores corrigidos e adaptação das ideias do vídeo de **I Spy Fantasy**, mantendo **Input System Package**, Windows, URP e a abertura colorida. Protótipo de jogo 3D point-and-click para **Windows x64**, em Unity: três dioramas de brinquedo, nove telas, 18 objetos escondidos e três puzzles. A proposta é brincar de imaginar mundos dentro de uma caixa: faroeste, espaço e super-heróis.

**Estado desta entrega:** projeto-fonte preparado, com assets reais incluídos. O editor Unity não estava disponível no ambiente de criação; portanto, este pacote **não inclui builds compilados** nem uma declaração de aprovação em playtest. Consulte [VALIDACAO.md](Docs/VALIDACAO.md) antes de publicar.

**Correção dos avisos de importação:** os 83 modelos e sete áudios agora têm blocos `.meta` completos, com schemas suportados e os mesmos GUIDs. Os mínimos relatados eram 25 para ModelImporter e 6 para AudioImporter. O pacote usa um schema ModelImporter 19300 e AudioImporter 6, que a Unity pode atualizar ao importar. O problema estava nos metadados incompletos fornecidos anteriormente. Consulte [CORRECAO_IMPORTADORES.md](Docs/CORRECAO_IMPORTADORES.md) para corrigir uma cópia em uso sem apagar referências.

**Adaptações do vídeo implementadas no código e no catálogo:** cenário com 704 px de altura numa referência de 900 px; charada compacta em duas colunas; quarto com três brinquedos de entrada; navegação por placas 3D; 271 instâncias nas nove telas de busca, com prateleiras, móbiles e agrupamentos; enquadramentos próximos e reações de descoberta; três desafios jogados em peças 3D. Veja a correspondência entre referência e implementação em [REFERENCIA_VIDEO.md](Docs/REFERENCIA_VIDEO.md). A execução no Unity ainda precisa ser validada.

## Abrir e jogar

1. Extraia a pasta `OlhosDeCacador` para uma pasta local nova. Preserve sua cópia anterior, especialmente se já tiver acrescentado gravações ou alterado os cenários.
2. No Unity Hub, use **Add / Add project from disk** e selecione essa pasta, que contém `Assets`, `Packages` e `ProjectSettings`.
3. Use **Unity 6000.6.4f1 (Unity 6.6)**. O projeto agora usa **Universal Render Pipeline (URP) 17.6.0**, com Universal Renderer em Forward.
4. Espere a importação dos modelos e a resolução dos pacotes, incluindo **Input System 1.20.1**. A primeira abertura pode levar alguns minutos. Se a Unity solicitar habilitar o novo backend de entrada e reiniciar, aceite; aguarde a nova importação antes de continuar.
5. Execute **Olhos de Cacador > 1. Preparar projeto** e **2. Verificar conteudo**.
6. Abra `Assets/Hunter/Scenes/Main.unity` e pressione **Play**. A abertura aparece primeiro; clique em **Vamos brincar!** para escolher um mundo.

A cena contém um controlador. Além das nove telas de busca, há um quarto de seleção e três enquadramentos dedicados aos desafios.  A interface e a sala selecionada são montadas ao iniciar a partida; é normal a cena parecer vazia antes do Play. O jogo funciona sem preencher campos do Inspector. Para acrescentar sua voz, use os nove campos opcionais do banco de narração, descritos abaixo.

O código usa **Input System 1.20.1**, com `Project Settings > Player > Other Settings > Active Input Handling: Input System Package (New)`. Mouse e teclado são lidos por `Mouse.current` e `Keyboard.current`; a interface usa `InputSystemUIInputModule` com as ações padrão do pacote. O controlador cria o EventSystem ao iniciar. Não é necessário acrescentar PlayerInput, outro EventSystem ou outra câmera à cena principal. O novo backend exige reiniciar o editor quando habilitado numa cópia anterior.

## O que existe no código

| Mundo | Telas | Puzzle final |
| --- | --- | --- |
| Faroeste de Quintal | Estação do Quintal; Salão do Suco; Mina dos Brilhos | Encontrar pistas para a senha de um baú |
| Missão Caixa Lunar | Base de Papelão; Jardim de Meteoros; Garagem das Naves | Observar e repetir quatro sinais |
| Liga dos Brinquedos | Praça dos Blocos; Ponte do Faz de Conta; Quartel das Boas Ideias | Levar cada ferramenta ao lugar certo |

- Busca por clique em malhas 3D, respeitando os objetos que ficam na frente.
- Dois alvos por tela; seis por mapa. Cada tela tem de 28 a 32 instâncias de objetos, contando alvos, decoração e passagens. Todos os mapas ficam disponíveis desde o começo.
- Nove charadas rimadas, dois botões de dica por tela, três níveis de ajuda e caderno de descobertas. Textos e IDs preservados para manter gravações e saves anteriores.
- Tela de início própria: título grande, fundo violeta, cores vivas, estrelas prontas e três miniaturas com movimento suave.
- Botão Vamos brincar! leva à seleção dos mundos, sem apagar o progresso; Enter ou Espaço também iniciam.
- Quarto de seleção: clique no baú, na nave ou na heroína para entrar no mundo correspondente; os rótulos também são clicáveis.
- Paletas mais vivas nas caixas e materiais explicitamente ligados às cores/texturas originais.
- Tela própria de opções: volumes separados para efeitos e voz, leitura automática, áudio e movimento.
- Espaço fixo “Voz da aventura”, com gravações humanas opcionais e leitura em texto ampliado.
- Início, quarto, exploração, pausa, puzzles, vitória por mapa e vitória geral. Garrafinhas controlam a senha do baú; brinquedos recebem sinais; ferramentas são levadas às miniaturas da cidade.
- Reinício de um mapa ou de toda a partida, com confirmação dentro do jogo.
- Erros de puzzle recuperáveis, sem apagar descobertas nem perder vidas.
- Salvamento local com cópia de recuperação, som opcional e movimento reduzido.
- Comando de build para **Windows x64**, com backend Mono.
- Botão **Sair do jogo** na abertura e na pausa, com confirmação.

## Controles

| Ação | Controle |
| --- | --- |
| Começar | Vamos brincar!, Enter ou Espaço na tela de início |
| Voltar à abertura | Tela inicial ou Esc na seleção de mundos |
| Encontrar objeto | Botão esquerdo do mouse sobre o objeto 3D |
| Escolher mundo | Baú, nave ou heroína no quarto, ou seu rótulo |
| Mudar de tela | Duas placas nas laterais do cenário, ou seus rótulos |
| Pedir ajuda | Um dos dois botões `DICA` ao lado da charada |
| Ler / ouvir a charada | `Ler juntos` e `Ouvir charada`, na faixa “Voz da aventura” |
| Ajustar áudio e movimento | `Opções` no início ou na pausa |
| Ler pistas | `Caderno` |
| Resolver puzzle | Após os seis alvos, clique no baú da Mina, no rádio da Garagem ou na maquete do Quartel |
| Pausar / voltar | `Esc` ou botões da interface |
| Fechar o jogo | `Sair do jogo`, na abertura ou na pausa; confirmar a saída |

As cores ajudam na orientação, mas os sinais dos puzzles também têm nomes. Os sons são complementares. O destino atual é Windows x64 com mouse, em janela redimensionável. Suporte a celular, leitor de tela e controle não foi validado.

## Tela de início

A abertura precede a escolha dos mundos e usa os mesmos assets prontos. Os dioramas balançam suavemente; as estrelas giram alguns graus e o botão principal pulsa discretamente. **Opções > Movimento reduzido** deixa tudo estático, incluindo após reabrir o jogo. Não há flashes, voz automática ou nova partida forçada na abertura.

Como brincar, Opções e Créditos voltam para a tela de onde foram abertos. No quarto de seleção, Tela inicial (ou Esc) retorna à abertura. As descobertas permanecem salvas.

## Mostrar o jogo em movimento

Grave de 60 a 120 segundos da janela Game em Play, ou do executável Windows, preferencialmente em MP4, 720p ou 1080p e 30 fps. Use o gravador de tela que já tiver; mantenha o cursor visível. O roteiro útil para revisão é:

1. Tela de início e quarto: clique num brinquedo para escolher o mundo.
2. Uma troca de cenário pelas placas 3D e um objeto decorativo que reage ao clique.
3. Hover, descoberta com animação e uso de uma dica.
4. A charada e sua gravação, se já estiver adicionada.
5. Pausa, opções e retorno; interações 3D de um desafio, incluindo uma tentativa errada.

Envie o MP4 como arquivo na conversa. Se o anexo de vídeo não estiver disponível, envie um link de download acessível. Para um defeito específico, indique o instante (por exemplo, 00:42) e o que deveria acontecer. Informe se a gravação foi feita no editor ou no executável Windows. Se houver erro no Unity, inclua a mensagem do Console por escrito ou em uma captura separada.

Para mostrar I Spy como referência, envie um trecho do longplay com a navegação, uma charada e um puzzle; indique quais detalhes visuais devem inspirar a revisão.

A análise de uma gravação por quadros permite conferir aparência, sequência de ações e transições; não substitui a medição de FPS pelo Profiler nem garante interpretação completa de áudio. Os arquivos desta entrega ainda não foram executados no editor Unity.

## Sua voz nas charadas

O projeto vem com **nove espaços de gravação vazios**, um por tela. Não foi gerada nem incluída uma voz artificial.

1. Grave os textos de [Docs/NARRACAO.md](Docs/NARRACAO.md), um áudio por charada.
2. Importe os seus WAV ou OGG para o projeto.
3. Selecione `Assets/Resources/Hunter/Narration/NarrationBank.asset` no Project.
4. Abra cada item de `Readings` no Inspector e arraste a gravação para `Clip`. O texto em `Recording Script` identifica a leitura correspondente.
5. Dê Play: o painel mostra **Ouvir charada** quando há áudio. Sem gravação, mostra **Ler juntos**, com a charada ampliada.

O volume de voz é independente dos efeitos. A leitura automática é opcional e começa desligada. Sair da sala, abrir uma pausa/puzzle ou perder o foco interrompe a fala. O guia explica também a alternativa de nomes de arquivo automáticos.

## Cores e materiais

Os 167 materiais `.mat` incluídos usam **Universal Render Pipeline/Lit**. São configurações de importação derivadas dos MTL originais: conservam as cores `Kd` em `_BaseColor` e os mapas de cor em `_BaseMap`, com os aliases `_Color` e `_MainTex` sincronizados. Cada modelo tem referências explícitas para evitar colisões entre materiais com o mesmo nome, como `colormap`. Texturas de modelos usam **2D e Repeat**, pois os robôs originais têm UVs negativas; limitar essas coordenadas à borda apaga suas cores. Os mapas de até 1024 px são preservados. As imagens de interface usam Sprite 2D e Clamp.

Durante a importação, o callback de associação apenas devolve o material persistente correspondente. Ele não altera remaps nem solicita novas importações. Espere a importação e a compilação terminarem, saia do Play e execute **Olhos de Cacador > 1. Preparar projeto**; esse comando inclui o reparo. **5. Restaurar materiais originais** executa somente esse reparo, na ordem texturas → materiais → vínculos, alterando apenas o que estiver incorreto. Depois execute **2. Verificar conteudo**. Não é necessário repetir Reimport All. Confira os passos e os limites da validação em [CORRECAO_IMPORTACAO_INPUT.md](Docs/CORRECAO_IMPORTACAO_INPUT.md).

## Migração do renderer

Esta revisão substitui o Built-in depreciado por URP. `Assets/Hunter/Settings/HunterURP.asset` está atribuído em Graphics e Quality; `HunterRenderer.asset` usa Forward. Os materiais das salas criados em código também usam URP/Lit. A configuração inicial usa MSAA 2×, sombras de 1024 px, distância de sombras de 35 unidades e HDR desligado.

**1. Preparar projeto** e os comandos de build configuram essas referências. **6. Configurar URP** permite repetir só essa configuração. O URP cria seus Global Settings e resolve os recursos internos do pacote ao ser ativado. Não habilite Compatibility Mode: esta revisão usa o Render Graph da linha 6.6.

O guia [MIGRACAO_URP.md](Docs/MIGRACAO_URP.md) explica como conferir os campos do editor e preservar suas mudanças locais. A migração não acrescenta arte, nem altera as charadas ou os espaços de voz.

## Compilar para Windows

Desde a v0.5, **Windows x64 é o único destino de entrega**. O comando web, sua configuração específica e o servidor HTTP auxiliar foram removidos.

1. Abra o projeto com Unity **6000.6.4f1** e confirme que o suporte para compilar para Windows está instalado. Se o editor indicar módulo ausente, acrescente-o no Unity Hub.
2. Em **File > Build Profiles**, selecione Windows e ative essa plataforma se outra estiver ativa.
3. Execute **Olhos de Cacador > 1. Preparar projeto**, **2. Verificar conteudo** e **3. Compilar Windows**.
4. A saída esperada fica em `Builds/Windows/OlhosDeCacador.exe`. Execute o jogo fora do editor e complete o roteiro de testes.
5. Distribua **toda a pasta `Builds/Windows`**, incluindo a pasta de dados e as bibliotecas produzidas pela Unity. Compacte a pasta inteira para publicar como download Windows no itch.io.

O build usa **Mono**, abre inicialmente em uma janela de 1440 × 900 e permite redimensionamento e alternância de tela cheia pelo sistema. O jogo mantém mouse e Esc como controles. O botão Sair do jogo solicita confirmação; no editor, ele encerra apenas o Play Mode. O progresso e as preferências usam o armazenamento local do usuário Windows, com a mesma chave `OlhosDeCacador.v1`. Saves de uma antiga execução no navegador não são transferidos automaticamente para o executável.

## Alterar os cenários

O catálogo está em `Assets/Resources/Hunter/catalog.json`. Cada objeto define:

- `asset`: caminho de um modelo pronto, sem extensão, relativo a `Hunter/Models`;
- `x`, `y`, `z`: posição da base no mundo;
- `size`: dimensão máxima desejada; a proporção original é preservada;
- `rotation`: rotação horizontal;
- `action`, `destination`, `value`: ação de um objeto clicável, destino da passagem ou índice da peça do puzzle;
- `reaction`: reação de descoberta (`hop`, `tip`, `spin`, `launch`);
- `id`, `label`, `hint`, `clue`: preenchidos nos alvos da caça.

Cada tela também define `camera`, `lookAt`, `fieldOfView` e `layout`. `hub` descreve o quarto; cada mundo tem um `challenge` com suas peças 3D. Esses campos são necessários para esta revisão.

Mantenha IDs únicos e permanentes. Ao adicionar um alvo, atualize a interface, os validadores e os testes: esta versão assume seis por mapa. As notas do baú devem continuar consistentes com `solution`. Uma mudança de idioma ou posição não exige mudar o ID. Se mudar a estrutura de progresso, versione o save e faça uma migração explícita.

## Assets e autoria

Os **83 modelos**, suas definições de materiais, texturas, **sete clipes de efeitos** e **12 imagens de interface** foram copiados de downloads oficiais da Kenney. Nenhuma imagem, modelo de objeto, textura ou gravação foi criada por geração de IA. Primitivos cúbicos formam salas, bordas e blocos; a UI combina texto, retângulos e as imagens prontas do UI Pack Adventure. Os materiais traduzem as configurações MTL existentes, sem pintar ou substituir a arte original. Animações de feedback movem ou escalam os objetos já existentes.

| Pack | Uso | Fonte / licença |
| --- | --- | --- |
| Survival Kit | Barracas, ferramentas, baús, recipientes e móveis | [Kenney](https://kenney.nl/assets/survival-kit) · CC0 |
| Nature Kit | Cactos, pedras, plantas e ponte | [Kenney](https://kenney.nl/assets/nature-kit) · CC0 |
| Space Kit | Naves, antenas, astronautas e equipamentos | [Kenney](https://kenney.nl/assets/space-kit) · CC0 |
| City Kit (Commercial) | Cidade de miniaturas | [Kenney](https://kenney.nl/assets/city-kit-commercial) · CC0 |
| Blocky Characters | Robôs e bonecos de brinquedo | [Kenney](https://kenney.nl/assets/blocky-characters) · CC0 |
| UI Audio | Cliques, acertos e sinais | [Kenney](https://kenney.nl/assets/ui-audio) · CC0 |
| UI Pack Adventure | Placas, molduras, painéis e estrelas | [Kenney](https://kenney.nl/assets/ui-pack-adventure) · CC0 |

Licenças originais: `Assets/ThirdPartyLicenses`. Lista exata dos arquivos e hashes: `Docs/asset-manifest.json`. A fonte de texto de interface é a `LegacyRuntime.ttf` incluída no Unity. Não redistribuímos fontes do sistema operacional.

Conceito solicitado por Gabriel. Código e documentação preparados com assistência de IA para este projeto. Preencha os nomes e contribuições da equipe antes da entrega acadêmica; revise e compreenda o código que será apresentado. Nenhum código de colegas ou de um jogo I Spy foi importado. A autoria/licença dos assets permanece a de seus criadores.

## Documentação e referências

- [Guia de narração](Docs/NARRACAO.md): integração da sua voz e roteiro das nove charadas.
- [GDD](Docs/GDD.md): experiência, regras, estética e arquitetura.
- [Rubricas e entrega](Docs/RUBRICAS_E_ENTREGA.md): critérios e evidências a produzir.
- [Importadores e atualização v0.7](Docs/CORRECAO_IMPORTADORES.md): correção dos avisos e instalação preservando gravações.
- [Validação e roteiro de testes](Docs/VALIDACAO.md): resultados locais e verificações pendentes.
- [Migração para URP](Docs/MIGRACAO_URP.md): configuração do renderer e diagnóstico do aviso.
- [Referência em vídeo](Docs/REFERENCIA_VIDEO.md): ideias de I Spy Fantasy implementadas e limites da validação.
- [Rubrica fornecida](https://docs.google.com/document/d/1jBL3xrub3hth5j5P7CbFNZcJv1uFdbAc/edit), consultada em 13/11/2026.
- [Playlist indicada de I Spy](https://www.youtube.com/playlist?list=PL-GmB9xDITWlLewwUaRzDFjO2ddP9fl9Y). A reprodução ficou presa no carregamento neste ambiente. A direção visual foi complementada pela observação de capturas de I Spy Fantasy e I Spy Junior: brinquedos coloridos, compartimentos, contraste de tamanhos e faixa de charada. As capturas são referências de estudo e não fazem parte do jogo; nenhuma arte ou fase de I Spy foi copiada.
- [Unity 6000.6.4f1](https://unity.com/releases/editor/whats-new/6000.6.4f1).
- [URP na Unity 6.6](https://docs.unity.com/en-us/engine/6000.6/manual/packages-list/packages-all/pack-core/com-unity-render-pipelines-universal) e [API URP 17.6](https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@17.6/api/UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset.html).
- [uGUI 2.6](https://docs.unity3d.com/Packages/com.unity.ugui@2.6/manual/index.html) e [Test Framework 1.8](https://docs.unity3d.com/Packages/com.unity.test-framework@1.8/manual/index.html), pacotes da linha do editor.
- [Configurações do Windows Player](https://docs.unity.com/en-us/engine/6000.6/manual/platform-specific/windows/playersettings).
