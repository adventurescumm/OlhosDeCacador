# Cenas e edição no Unity — v0.12

O projeto usa **18 cenas `.unity`**. `Main` inicia a sessão; as outras 17 representam a abertura, o quarto e os cinco espaços de cada mundo. As trocas usam `SceneManager.LoadSceneAsync` em modo Single. O controlador, as câmeras, a interface, o áudio e o progresso ficam em um único objeto persistente.

## Organização

Todos os caminhos abaixo são relativos a `Assets/BemTeVi/Scenes/`.

| Pasta | Cenas | Função |
| --- | --- | --- |
| Raiz | `Main.unity` | Inicialização da sessão e entrada do build Windows |
| `Menus/` | `inicio.unity`, `quarto.unity` | Título com Iniciar/Opções/Como jogar; escolha dos mundos no quarto |
| `faroeste/` | `faroeste_caminho.unity`, `faroeste_estacao.unity`, `faroeste_salao.unity`, `faroeste_mina.unity`, `faroeste_desafio.unity` | Entrada da vila, três maquetes de busca e baú final |
| `espaco/` | `espaco_caminho.unity`, `espaco_base.unity`, `espaco_jardim.unity`, `espaco_garagem.unity`, `espaco_desafio.unity` | Pátio de pouso, três maquetes de busca e sequência final |
| `herois/` | `herois_caminho.unity`, `herois_praca.unity`, `herois_ponte.unity`, `herois_quartel.unity`, `herois_desafio.unity` | Entrada da cidade, três maquetes de busca e associação final |

Opções, Como jogar, pausa, caderno e leitura ampliada são painéis sobre a cena de origem. Fechá-los retorna ao contexto anterior. A vitória aparece na própria cena do desafio concluído.

## Mapa interconectado

Cada mundo forma um circuito: entrada ↔ primeira busca ↔ segunda busca ↔ terceira busca ↔ entrada. A terceira busca também se liga ao desafio final. Todas as passagens do circuito ficam disponíveis desde o começo. Os mecanismos mudam os brinquedos e montam peças sem bloquear a exploração. O quarto só é acessível pela passagem na entrada. Não há botões flutuantes de destinos nem seleção de salas na interface.

`PropData.action = "exit"` define uma passagem física; `destination` contém o ID da cena vizinha. A ação `gate` usa a peça encontrada e movimenta os objetos ligados ao mesmo campo `gate`; `showAfter` revela a peça montada. O catálogo precisa ter destinos recíprocos. O verificador confere esse circuito.

## Primeira preparação

1. Abra o projeto com Unity **6000.6.4f1**, aguarde os pacotes e a compilação e abra `Main.unity`.
2. Execute **Bem-Te-Vi > 1. Preparar projeto**. Além do URP e dos materiais, o comando preenche as cenas ainda não preparadas com os modelos prontos do catálogo, grava seus objetos e registra as 18 cenas no build.
3. Execute **2. Verificar conteudo**. Abra uma das cenas das pastas acima para editar sua composição.

**O ZIP inclui as 18 cenas com identificadores e enquadramentos, mas a montagem e gravação das hierarquias completas precisam ocorrer no seu Unity.** Este ambiente não possui o editor para executar essa etapa. Enquanto uma cena ainda não foi preparada, o runtime monta seu conteúdo a partir do catálogo ao dar Play.

Depois da primeira preparação, cada cena de conteúdo possui:

- **00 — Configuração:** componente `BemTeViStage`, com ID, posição da câmera, ponto observado, campo de visão e referência ao cenário.
- **01 — Cenário editável / Arquitetura e iluminação:** bases de maquete, patamares, rios e caminhos primitivos; o quarto mantém paredes e luzinhas.
- **Brinquedos e decoração:** modelos originais que compõem o ambiente.
- **Objetos da charada:** 18 objetos identificados por sala; seis ativos em cada rodada.
- **Passagens e mecanismos:** entradas, destinos e peças interativas.

As configurações de materiais criadas para a arquitetura primitiva ficam em `Assets/BemTeVi/Settings/RoomMaterials`. Os modelos e texturas de terceiros continuam em Resources; nenhuma geometria artística é modelada pelo preparador.

## Como editar

Mova, gire ou redimensione os objetos na cena preparada e salve a cena. O jogo usa essas posições e a câmera de `BemTeViStage` ao carregá-la. O componente `BemTeViSceneProp` mantém a identidade e a ação de cada brinquedo; preserve IDs de alvos e destinos de passagens.

O campo `geometry` descreve as primitivas arquitetônicas de cada maquete; modelos artísticos continuam vindos dos downloads. `openedOffset` e `openedRotation` indicam como um mecanismo se move ao abrir, relativamente à posição editada.

Os textos, regras, peças necessárias e charadas continuam em `Assets/Resources/BemTeVi/catalog.json`. Mudar o campo `asset` de um objeto já montado não substitui sua malha automaticamente: atualize o catálogo e recrie a cena, ou substitua conscientemente o modelo na hierarquia, mantendo os dados e colisores correspondentes.

**Preparar projeto preserva cenas da versão de conteúdo atual (12); recompõe versões antigas a partir do catálogo.** Para aplicar novamente as posições do catálogo, use **7. Recriar cenas a partir do catalogo**. Esse comando avisa que sobrescreve as alterações manuais das 17 cenas de conteúdo; guarde suas alterações antes de confirmar. `Main`, gravações humanas e saves não são recriados por esse comando.

É possível iniciar o Play numa cena de busca para inspecioná-la diretamente; isso não simula a progressão normal de entrada. Sem jogador selecionado, o jogo abre o título; com um caderno existente, permite iniciar a cena para inspeção. Para testar a aventura completa, comece por `Main`.

## Responsabilidades no código

| Arquivo | Responsabilidade |
| --- | --- |
| `BemTeViGame.Scenes.cs` | Sessão persistente, carregamento assíncrono e entrada por uma cena específica |
| `BemTeViStage.cs` | Identificação, câmera e raiz editável de uma cena |
| `BemTeViSceneProp.cs` | Dados de interação junto ao objeto da hierarquia |
| `BemTeViSceneBuilder.cs` | Preparação explícita das cenas no editor e registro no build |
| `BemTeViGame.Navigation.cs` | Caminhos intermediários, peças encontradas e passagens |
| `BemTeViGame.World.cs` | Câmera, rótulos e cliques no cenário |
| `RoomView.cs` | Vincula os objetos da cena ao progresso; monta a alternativa por catálogo quando necessário |
| `Domain/GameData.cs` | Regras de coleta, passagens, conclusão e recuperação do save |

O fluxo de carregamento e a gravação das cenas ainda precisam ser testados no editor. Consulte [VALIDACAO.md](VALIDACAO.md).

## Câmeras e som na v0.12

As câmeras das buscas são próximas e fixas. O viewport de busca/desafio ocupa os primeiros 760 pixels de uma referência 1440 × 900; os últimos 140 são a faixa textual. Entradas e quarto usam a área completa. Camera Position, Look At e Field of View do BemTeViStage continuam editáveis. Não há zoom da roda nem reposicionamento automático ao encontrar alvos.

`PropData.sound` referencia um clipe original em BemTeVi/Audio, sem extensão. `soundscapes.json` define os sons de passagens, mecanismos e ambiente por ID de cena. Novos bonecos são props de inspeção sem ID de alvo.
