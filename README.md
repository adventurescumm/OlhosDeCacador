# Olhos de Caçador — v0.9

Jogo 3D point-and-click para **Windows x64**, **Unity 6000.6.4f1**, **URP 17.6.0** e **Input System 1.20.1 (New)**.

Esta revisão transforma os mundos em **maquetes interconectadas**. Portas, pontes, trilhos e túneis no próprio cenário levam a cenas vizinhas. Os seletores de destinos e o menu central de navegação foram removidos. Há **54 objetos para encontrar**, seis por busca, e **seis mecanismos** que usam descobertas para abrir passagens.

**Estado:** código e assets incluídos, sem executável compilado. Passaram 175 verificações da lógica C# fora do Unity. O editor não está instalado neste ambiente: importação, montagem das cenas, Play, desempenho e build Windows precisam ser executados aí. Veja [VALIDACAO.md](Docs/VALIDACAO.md).

## Abrir

1. Extraia o ZIP em uma pasta nova e preserve seu projeto anterior, especialmente gravações e cenas próprias.
2. Adicione a pasta `OlhosDeCacador` no Unity Hub e abra com **6000.6.4f1**.
3. Aguarde os pacotes e a importação. Se o editor solicitar reinício para o novo backend de entrada, reinicie.
4. Abra `Assets/Hunter/Scenes/Main.unity` e execute **Olhos de Cacador > 1. Preparar projeto**, depois **2. Verificar conteudo**.
5. Dê Play por Main. A abertura oferece **Iniciar**, **Opções** e **Como jogar** sobre o quarto infantil.

**As 18 cenas já estão incluídas e registradas no build, com identificadores e câmeras.** A primeira preparação monta e salva as 17 hierarquias de conteúdo; depois disso, você pode editar cada composição no editor. Preparar projeto preserva cenas montadas. [CENAS.md](Docs/CENAS.md) explica os grupos e o comando explícito para recriar cenas a partir do catálogo.

## Como jogar

Clique numa das três maquetes do quarto: vila do faroeste à esquerda, nave sobre a mesa e cidade à direita. Dentro do mundo, procure portas, trilhos, pontes e túneis; o texto inferior identifica as passagens ao apontar. Não há lista de níveis nem botões flutuantes de destinos.

Cada mundo forma um circuito: entrada ↔ busca 1 ↔ busca 2 ↔ busca 3 ↔ entrada. O atalho da entrada para a terceira busca fica bloqueado até o progresso necessário. A terceira busca leva ao desafio final. Para voltar ao quarto, siga a passagem na entrada do mundo.

| Ação | Controle |
| --- | --- |
| Encontrar objeto / atravessar passagem | Clique esquerdo no cenário |
| Aproximar a região apontada | Roda do mouse |
| Restaurar visão geral | Botão direito |
| Pedir uma dica | Dica 1 a 6, conforme o verso |
| Ler / ouvir | Ler juntos e Ouvir charada |
| Usar peça encontrada | Complete os seis objetos e clique no mecanismo da maquete |
| Pausar / fechar painel | Esc ou Pausa |
| Rever pistas | Caderno |
| Sair do jogo | Sair no quarto ou na pausa; confirmar |

Alvos e decoração comum mostram o mesmo texto ao passar o mouse. A aproximação também funciona sobre qualquer região. As dicas são explícitas e progressivas. Clicar errado não apaga progresso.

Os mecanismos aparecem antes de poderem ser usados. Ao completar a charada e clicar no brinquedo correspondente, a peça correta é aplicada automaticamente: aparece montada, o brinquedo se move e a próxima passagem abre. O resultado continua após salvar e retornar.

| Mundo | Maquetes de busca | Desafio final |
| --- | --- | --- |
| Faroeste de Quintal | Estação do Quintal, Salão do Suco, Mina dos Brilhos | Senha do baú |
| Missão Caixa Lunar | Base de Papelão, Jardim de Meteoros, Garagem das Naves | Sequência de sinais |
| Liga dos Brinquedos | Praça dos Blocos, Ponte do Faz de Conta, Quartel das Boas Ideias | Associação de ferramentas |

## O que mudou visualmente

As nove buscas têm bases espessas sobre uma mesa, patamares, ruas, rios e construções em miniatura. Os objetos estão agrupados em superfícies e cantinhos próprios, com enquadramentos diferentes. São **611 instâncias** nas buscas, incluindo peças que só aparecem depois de resolver mecanismos. A interface mantém cores variadas e Lilita One nos títulos.

Foram usados novos modelos prontos de mobiliário e um pato de borracha de Poly Haven, com formas arredondadas e textura original. Muitos assets restantes continuam estilizados; suavizar seu sombreamento não aumenta sua geometria. Nenhuma arte ou voz foi gerada.

## Sua narração

O banco contém nove campos vazios, um por busca. Grave os textos de [NARRACAO.md](Docs/NARRACAO.md), importe seus WAV/OGG e atribua os clipes em `Assets/Resources/Hunter/Narration/NarrationBank.asset`. A voz é opcional; Ler juntos sempre funciona.

**Da v0.8 para a v0.9, somente a charada da estação mudou:** o último verso agora pede um pato de borracha. As outras oito mantêm o texto. Grave novamente a estação antes de reutilizar os clipes. Volumes de voz e efeitos são independentes; leitura automática começa desligada. Trocar de cena ou abrir pausa interrompe a voz.

## Configuração e correções preservadas

O projeto usa exclusivamente Input System Package (New), com `activeInputHandler: 1`, Mouse/Keyboard e InputSystemUIInputModule. O controlador cria um EventSystem e um AudioListener persistentes; não acrescente duplicatas às cenas.

Graphics e Quality apontam para HunterURP; HunterRenderer usa Forward, MSAA 2×, HDR desligado e Render Graph. Materiais URP/Lit conservam as cores e texturas originais. As correções de propriedades URP somente leitura, texturas CUBE/2D, reimportação e versões de importadores foram preservadas. O reparo agora também inclui FBX, JPG e EXR.

Os **148 ModelImporters** têm schema 19300 e blocos completos; os **sete AudioImporters** têm schema 6. Não apague os `.meta`. Espere a importação terminar antes de usar os comandos de reparo; não repita Reimport All dentro de um ciclo de importação.

## Build Windows

1. Confirme o suporte Windows no Unity Hub e ative Windows em **File > Build Profiles**.
2. Execute Preparar projeto, Verificar conteudo e **3. Compilar Windows**.
3. A saída esperada é `Builds/Windows/OlhosDeCacador.exe`, usando Mono, janela inicial 1440×900 e redimensionamento.
4. Execute fora do editor e distribua **toda a pasta Builds/Windows**, incluindo dados e bibliotecas.

O save local usa `OlhosDeCacador.v1`. Os 54 IDs e mecanismos da v0.8 foram preservados. Para uma atualização segura, veja [ATUALIZACAO_V09.md](Docs/ATUALIZACAO_V09.md).

## Assets e autoria

São **147 modelos OBJ e um FBX**, 264 vínculos de materiais e 326 arquivos originais verificados por SHA-256. Geometria, UVs, texturas, efeitos e fonte de terceiros permanecem com os bytes de origem. As formas arquitetônicas usam apenas cubos, esferas e cilindros. Animações movimentam os modelos prontos.

| Fonte | Conteúdo e licença |
| --- | --- |
| [Kenney](https://kenney.nl/assets) | Survival Kit, Nature Kit, Space Kit, City Kit Commercial, Blocky Characters, Furniture Kit, Toy Car Kit, Food Kit, UI Audio e UI Pack Adventure — CC0 |
| [Rubber Duck Toy — Poly Haven](https://polyhaven.com/a/rubber_duck_toy) | Plat251 — CC0. Powered by Poly Haven |
| [Lilita One — Google Fonts](https://github.com/google/fonts/tree/main/ofl/lilitaone) | Juan Montoreano — SIL OFL 1.1 |

Licenças: `Assets/ThirdPartyLicenses`. Arquivos, hashes e URLs: `Docs/asset-manifest.json`. Conceito solicitado por Gabriel; código e documentação preparados com assistência de IA. Complete os nomes e contribuições da equipe antes da entrega acadêmica.

## Documentação

- [Cenas e edição](Docs/CENAS.md)
- [Atualização v0.9](Docs/ATUALIZACAO_V09.md)
- [Narração](Docs/NARRACAO.md)
- [GDD](Docs/GDD.md)
- [Validação e roteiro de testes](Docs/VALIDACAO.md)
- [Adaptação do vídeo](Docs/REFERENCIA_VIDEO.md)
- [Rubricas e entrega](Docs/RUBRICAS_E_ENTREGA.md)
- [Migração URP](Docs/MIGRACAO_URP.md)
- [Histórico dos importadores](Docs/CORRECAO_IMPORTADORES.md)
- [Histórico da entrada e texturas](Docs/CORRECAO_IMPORTACAO_INPUT.md)

Para mostrar o jogo em movimento, grave 60–120 segundos em MP4, com cursor visível: abertura, entrada numa maquete, descoberta, uso de mecanismo e travessia de ida/volta. Identifique se é Play ou executável Windows. Prévias externas não substituem essa gravação nem medição pelo Profiler.
