# Olhos de Caçador

Revisão **v0.8**: um quarto de criança, buscas com mais objetos e navegação entre cenas. Jogo 3D point-and-click para **Windows x64**, **Unity 6000.6.4f1**, **URP 17.6.0** e **Input System 1.20.1 (New)**.

São **54 alvos**, seis em cada uma das nove buscas, com **614 instâncias** de brinquedos, decoração e passagens nessas telas. O quarto leva a três mundos; cada um tem um cenário intermediário, três buscas, duas passagens que exigem peças e um desafio final. A abertura mostra o nome do jogo e os botões **Iniciar**, **Opções** e **Como jogar**.

**18 cenas separadas:** uma de inicialização, duas de menu e cinco por mundo. Na primeira execução de **Preparar projeto**, o Unity monta e salva as hierarquias editáveis usando os modelos incluídos. Veja [CENAS.md](Docs/CENAS.md). As cenas do pacote têm identificadores e enquadramentos; essa montagem ainda precisa ser executada no editor.

**Estado da entrega:** código, conteúdo, fontes e assets incluídos; sem executável compilado. Os testes do domínio C# passaram fora do Unity, mas o editor não está instalado neste ambiente: importação, montagem das cenas, Play, desempenho e build Windows continuam pendentes. [VALIDACAO.md](Docs/VALIDACAO.md) distingue esses resultados.

Os importadores de **141 modelos e sete efeitos de áudio** têm blocos completos. As correções anteriores de textura 2D, GUIDs, reimportação e Input System foram preservadas. A v0.8 calcula o sombreamento das superfícies por ângulo, sem alterar os arquivos de malha, e acrescenta assets prontos de mobiliário, carrinhos, comidinhas e uma fonte arredondada. [ATUALIZACAO_V08.md](Docs/ATUALIZACAO_V08.md) explica a migração.

## Abrir e jogar

1. Extraia a pasta `OlhosDeCacador` para uma pasta local nova. Preserve sua cópia anterior, especialmente se já tiver acrescentado gravações ou alterado os cenários.
2. No Unity Hub, use **Add / Add project from disk** e selecione essa pasta, que contém `Assets`, `Packages` e `ProjectSettings`.
3. Use **Unity 6000.6.4f1 (Unity 6.6)**. O projeto agora usa **Universal Render Pipeline (URP) 17.6.0**, com Universal Renderer em Forward.
4. Espere a importação dos modelos e a resolução dos pacotes, incluindo **Input System 1.20.1**. A primeira abertura pode levar alguns minutos. Se a Unity solicitar habilitar o novo backend de entrada e reiniciar, aceite; aguarde a nova importação antes de continuar.
5. Execute **Olhos de Cacador > 1. Preparar projeto** e **2. Verificar conteudo**.
6. Abra `Assets/Hunter/Scenes/Main.unity` e pressione **Play**. A abertura aparece primeiro; clique em **Iniciar** para entrar no quarto e escolher um mundo.

`Main` contém o controlador de inicialização. Depois da preparação, as outras cenas têm os próprios cenários e objetos na hierarquia. O jogo preserva a sessão enquanto troca a cena ativa. Os nove campos de voz são opcionais; os roteiros foram ampliados para seis versos e precisam de gravações correspondentes.

O código usa **Input System 1.20.1**, com `Project Settings > Player > Other Settings > Active Input Handling: Input System Package (New)`. Mouse e teclado são lidos por `Mouse.current` e `Keyboard.current`; a interface usa `InputSystemUIInputModule` com as ações padrão do pacote. O controlador cria o EventSystem ao iniciar. Não é necessário acrescentar PlayerInput, outro EventSystem ou outra câmera à cena principal. O novo backend exige reiniciar o editor quando habilitado numa cópia anterior.

## O que existe no código

| Mundo | Telas | Puzzle final |
| --- | --- | --- |
| Faroeste de Quintal | Estação do Quintal; Salão do Suco; Mina dos Brilhos | Encontrar pistas para a senha de um baú |
| Missão Caixa Lunar | Base de Papelão; Jardim de Meteoros; Garagem das Naves | Observar e repetir quatro sinais |
| Liga dos Brinquedos | Praça dos Blocos; Ponte do Faz de Conta; Quartel das Boas Ideias | Levar cada ferramenta ao lugar certo |

- Seis alvos por tela; 18 por mundo. Cada busca tem de 67 a 70 instâncias, com tamanhos variados, objetos parecidos e posições diferentes entre as salas.
- Os alvos não revelam nome nem aumentam de tamanho só ao passar o mouse. Dica 1 a 6 oferece ajuda gradual; o destaque fica para a dica final.
- Objetos grandes aparecem nos caminhos antes de poderem ser usados. Peças coletadas, após completar a busca de origem, abrem as próximas passagens.
- O quarto tem cama, mesa, estante, almofadas, tapete e carrinhos. O baú fica à esquerda, a nave sobre a mesa ao fundo e a heroína à direita.
- Cada sala tem sua própria paleta; a interface usa botões de cores variadas e a fonte Lilita One nos destaques.
- Opções próprias para efeitos, narrador, leitura automática, silêncio e movimento reduzido.
- Três desafios finais 3D: combinação, memória e associação, além das seis interações de passagem.
- Progresso e caminhos abertos são salvos; reiniciar um mundo preserva os outros.
- Modelos, texturas, áudio e fonte vêm de arquivos prontos. Apenas arquitetura e luzinhas primitivas são construídas em código.

## Controles

| Ação | Controle |
| --- | --- |
| Começar | Iniciar, Enter ou Espaço na tela de início |
| Voltar à abertura | Tela inicial ou Esc na seleção de mundos |
| Encontrar objeto | Botão esquerdo do mouse sobre o objeto 3D |
| Escolher mundo | Baú, nave ou heroína no quarto, ou seu rótulo |
| Mudar de tela | Voltar ao Caminho e escolher outra entrada; as passagens posteriores exigem peças |
| Pedir ajuda | Um dos seis botões `Dica 1` a `Dica 6` |
| Ler / ouvir a charada | `Ler juntos` e `Ouvir charada`, na faixa “Voz da aventura” |
| Ajustar áudio e movimento | `Opções` no início ou na pausa |
| Ler pistas | `Caderno`, durante a busca |
| Abrir passagem | Selecione uma peça disponível na faixa inferior do Caminho e clique no brinquedo correspondente |
| Resolver puzzle | Após os 18 alvos, clique no mecanismo final do caminho ou em Desafio final |
| Pausar / voltar | `Esc` ou botões da interface |
| Fechar o jogo | `Sair` no quarto ou `Sair do jogo` na pausa; confirmar a saída |

As cores ajudam na orientação, mas os sinais dos puzzles também têm nomes. Os sons são complementares. O destino atual é Windows x64 com mouse, em janela redimensionável. Suporte a celular, leitor de tela e controle não foi validado.

## Tela de início

A abertura usa o quarto como fundo e apresenta somente título, Iniciar, Opções e Como jogar. Iniciar leva à cena de seleção dentro do mesmo quarto, com as três entradas afastadas em profundidade e na horizontal. Créditos, Novo jogo e Sair ficam no quarto; a pausa também permite sair.

Opções e Como jogar retornam à origem. Movimento reduzido desliga o pequeno movimento das estrelas e do botão inicial, os focos de descoberta e as animações de brinquedos. O cenário não gira como uma vitrine de miniaturas.

## Mostrar o jogo em movimento

Grave de 60 a 120 segundos da janela Game em Play, ou do executável Windows, preferencialmente em MP4, 720p ou 1080p e 30 fps. Use o gravador de tela que já tiver; mantenha o cursor visível. O roteiro útil para revisão é:

1. Tela de início e quarto: clique num brinquedo para escolher o mundo.
2. Uma busca, o retorno ao Caminho e o uso de uma peça para abrir a próxima passagem.
3. Uma descoberta com animação, o uso de uma dica e alguns brinquedos que não pertencem à lista atual.
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

Os 246 materiais `.mat` incluídos usam **Universal Render Pipeline/Lit**. São configurações de importação derivadas dos MTL originais: conservam as cores `Kd` em `_BaseColor` e os mapas de cor em `_BaseMap`, com os aliases `_Color` e `_MainTex` sincronizados. Cada modelo tem referências explícitas para evitar colisões entre materiais com o mesmo nome, como `colormap`. Texturas de modelos usam **2D e Repeat**, pois os robôs originais têm UVs negativas; limitar essas coordenadas à borda apaga suas cores. Os mapas de até 1024 px são preservados. As imagens de interface usam Sprite 2D e Clamp.

Durante a importação, o callback de associação apenas devolve o material persistente correspondente. Ele não altera remaps nem solicita novas importações. Espere a importação e a compilação terminarem, saia do Play e execute **Olhos de Cacador > 1. Preparar projeto**; esse comando inclui o reparo. **5. Restaurar materiais originais** executa somente esse reparo, na ordem texturas → materiais → vínculos, alterando apenas o que estiver incorreto. Depois execute **2. Verificar conteudo**. Não é necessário repetir Reimport All. Confira os passos e os limites da validação em [CORRECAO_IMPORTACAO_INPUT.md](Docs/CORRECAO_IMPORTACAO_INPUT.md).

## Migração do renderer

Esta revisão substitui o Built-in depreciado por URP. `Assets/Hunter/Settings/HunterURP.asset` está atribuído em Graphics e Quality; `HunterRenderer.asset` usa Forward. Os materiais das salas criados em código também usam URP/Lit. A configuração inicial usa MSAA 2×, sombras de 1024 px, distância de sombras de 35 unidades e HDR desligado.

**1. Preparar projeto** e os comandos de build configuram essas referências. **6. Configurar URP** permite repetir só essa configuração. O URP cria seus Global Settings e resolve os recursos internos do pacote ao ser ativado. Não habilite Compatibility Mode: esta revisão usa o Render Graph da linha 6.6.

O guia [MIGRACAO_URP.md](Docs/MIGRACAO_URP.md) explica como conferir os campos do editor e preservar suas mudanças locais. A migração de renderer não exige alterar IDs; nesta revisão de conteúdo, os nove roteiros foram ampliados, conforme NARRACAO.md.

## Compilar para Windows

Desde a v0.5, **Windows x64 é o único destino de entrega**. O comando web, sua configuração específica e o servidor HTTP auxiliar foram removidos.

1. Abra o projeto com Unity **6000.6.4f1** e confirme que o suporte para compilar para Windows está instalado. Se o editor indicar módulo ausente, acrescente-o no Unity Hub.
2. Em **File > Build Profiles**, selecione Windows e ative essa plataforma se outra estiver ativa.
3. Execute **Olhos de Cacador > 1. Preparar projeto**, **2. Verificar conteudo** e **3. Compilar Windows**.
4. A saída esperada fica em `Builds/Windows/OlhosDeCacador.exe`. Execute o jogo fora do editor e complete o roteiro de testes.
5. Distribua **toda a pasta `Builds/Windows`**, incluindo a pasta de dados e as bibliotecas produzidas pela Unity. Compacte a pasta inteira para publicar como download Windows no itch.io.

O build usa **Mono**, abre inicialmente em uma janela de 1440 × 900 e permite redimensionamento e alternância de tela cheia pelo sistema. O jogo mantém mouse e Esc como controles. O botão Sair do jogo solicita confirmação; no editor, ele encerra apenas o Play Mode. O progresso e as preferências usam o armazenamento local do usuário Windows, com a mesma chave `OlhosDeCacador.v1`. Saves de uma antiga execução no navegador não são transferidos automaticamente para o executável.

## Alterar os cenários

As posições podem ser editadas nas cenas preparadas. O catálogo de referência está em `Assets/Resources/Hunter/catalog.json`. Cada objeto define:

- `asset`: caminho de um modelo pronto, sem extensão, relativo a `Hunter/Models`;
- `x`, `y`, `z`: posição da base no mundo;
- `size`: dimensão máxima desejada; a proporção original é preservada;
- `rotation`: rotação horizontal;
- `action`, `destination`, `value`: ação de um objeto clicável, destino da passagem ou índice da peça do puzzle;
- `reaction`: reação de descoberta (`hop`, `tip`, `spin`, `launch`);
- `id`, `label`, `hint`, `clue`: preenchidos nos alvos da caça.

Cada tela define `camera`, `lookAt`, `fieldOfView`, `layout` e sua paleta. `hub` descreve o quarto; cada mundo tem `junction`, dois `gates` e um `challenge`. As cenas preparadas guardam seus próprios enquadramentos em HunterStage. Veja CENAS.md antes de recriar cenas já editadas.

Mantenha IDs únicos e permanentes. Ao adicionar um alvo, atualize a interface, os validadores e os testes: esta versão assume seis por busca e 18 por mapa. As notas do baú devem continuar consistentes com `solution`. Uma mudança de idioma ou posição não exige mudar o ID. Se mudar a estrutura de progresso, versione o save e faça uma migração explícita.

## Assets e autoria

O pacote contém **141 modelos originais** e mantém seus OBJ, MTL e texturas sem mudanças de bytes. Sete efeitos prontos e imagens de interface existentes também permanecem intactos. A fonte Lilita One foi obtida do repositório oficial Google Fonts, sob SIL OFL 1.1. Nenhuma imagem, textura, modelo artístico ou voz foi gerada.

Os materiais traduzem MTLs para URP; a iluminação e o cálculo de normais suavizam a aparência das formas existentes. Isso não aumenta a geometria dos modelos nem promete uma reprodução fotográfica de I Spy. As animações movem os objetos prontos. Cubos e esferas formam arquitetura, detalhes e luzinhas permitidos pela restrição do projeto.

| Pack | Uso | Fonte / licença |
| --- | --- | --- |
| Survival Kit | Barracas, ferramentas, baús, recipientes e móveis | [Kenney](https://kenney.nl/assets/survival-kit) · CC0 |
| Nature Kit | Cactos, pedras, plantas e ponte | [Kenney](https://kenney.nl/assets/nature-kit) · CC0 |
| Space Kit | Naves, antenas, astronautas e equipamentos | [Kenney](https://kenney.nl/assets/space-kit) · CC0 |
| City Kit (Commercial) | Cidade de miniaturas | [Kenney](https://kenney.nl/assets/city-kit-commercial) · CC0 |
| Blocky Characters | Robôs e bonecos de brinquedo | [Kenney](https://kenney.nl/assets/blocky-characters) · CC0 |
| Furniture Kit | Cama, mesa, estante, tapetes e almofadas | [Kenney](https://kenney.nl/assets/furniture-kit) · CC0 |
| Toy Car Kit | Carrinhos, rodas e pistas | [Kenney](https://kenney.nl/assets/toy-car-kit) · CC0 |
| Food Kit | Comidinhas de faz de conta e distratores arredondados | [Kenney](https://kenney.nl/assets/food-kit) · CC0 |
| Lilita One | Fonte de títulos e botões | [Google Fonts](https://github.com/google/fonts/tree/main/ofl/lilitaone) · SIL OFL 1.1 |
| UI Audio | Cliques, acertos e sinais | [Kenney](https://kenney.nl/assets/ui-audio) · CC0 |
| UI Pack Adventure | Placas, molduras, painéis e estrelas | [Kenney](https://kenney.nl/assets/ui-pack-adventure) · CC0 |

Licenças originais: `Assets/ThirdPartyLicenses`. Lista exata dos arquivos e hashes: `Docs/asset-manifest.json`. Os textos longos usam a `LegacyRuntime.ttf` incluída no Unity; títulos e botões usam Lilita One. Não redistribuímos fontes do sistema operacional.

Conceito solicitado por Gabriel. Código e documentação preparados com assistência de IA para este projeto. Preencha os nomes e contribuições da equipe antes da entrega acadêmica; revise e compreenda o código que será apresentado. Nenhum código de colegas ou de um jogo I Spy foi importado. A autoria/licença dos assets permanece a de seus criadores.

## Documentação e referências

- [Cenas e edição](Docs/CENAS.md): estrutura, preparação e troca de cenas.
- [Atualização v0.8](Docs/ATUALIZACAO_V08.md): mudanças, saves e gravações anteriores.
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
