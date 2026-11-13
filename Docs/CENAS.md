# Cenas e edição no Unity — v0.9

O projeto usa **18 cenas `.unity`**. `Main` inicia a sessão; as outras 17 representam a abertura, o quarto e os cinco espaços de cada mundo. As trocas usam `SceneManager.LoadSceneAsync` em modo Single. O controlador, as câmeras, a interface, o áudio e o progresso ficam em um único objeto persistente.

## Organização

Todos os caminhos abaixo são relativos a `Assets/Hunter/Scenes/`.

| Pasta | Cenas | Função |
| --- | --- | --- |
| Raiz | `Main.unity` | Inicialização da sessão e entrada do build Windows |
| `Menus/` | `inicio.unity`, `quarto.unity` | Título com Iniciar/Opções/Como jogar; escolha dos mundos no quarto |
| `faroeste/` | `faroeste_caminho.unity`, `faroeste_estacao.unity`, `faroeste_salao.unity`, `faroeste_mina.unity`, `faroeste_desafio.unity` | Entrada da vila, três maquetes de busca e baú final |
| `espaco/` | `espaco_caminho.unity`, `espaco_base.unity`, `espaco_jardim.unity`, `espaco_garagem.unity`, `espaco_desafio.unity` | Pátio de pouso, três maquetes de busca e sequência final |
| `herois/` | `herois_caminho.unity`, `herois_praca.unity`, `herois_ponte.unity`, `herois_quartel.unity`, `herois_desafio.unity` | Entrada da cidade, três maquetes de busca e associação final |

Opções, Como jogar, pausa, caderno e leitura ampliada são painéis sobre a cena de origem. Fechá-los retorna ao contexto anterior. A vitória aparece na própria cena do desafio concluído.

## Mapa interconectado

Cada mundo forma um circuito: entrada ↔ primeira busca ↔ segunda busca ↔ terceira busca ↔ entrada. A terceira busca também se liga ao desafio final. Os mecanismos das primeiras duas buscas liberam as etapas seguintes; a ligação entre entrada e terceira busca funciona como atalho depois do progresso. O quarto só é acessível pela passagem na entrada. Não há botões flutuantes de destinos nem seleção de salas na interface.

`PropData.action = "exit"` define uma passagem física; `destination` contém o ID da cena vizinha. A ação `gate` usa a peça encontrada e movimenta os objetos ligados ao mesmo campo `gate`; `showAfter` revela a peça montada. O catálogo precisa ter destinos recíprocos. O verificador confere esse circuito.

## Primeira preparação

1. Abra o projeto com Unity **6000.6.4f1**, aguarde os pacotes e a compilação e abra `Main.unity`.
2. Execute **Olhos de Cacador > 1. Preparar projeto**. Além do URP e dos materiais, o comando preenche as cenas ainda não preparadas com os modelos prontos do catálogo, grava seus objetos e registra as 18 cenas no build.
3. Execute **2. Verificar conteudo**. Abra uma das cenas das pastas acima para editar sua composição.

**O ZIP inclui as 18 cenas com identificadores e enquadramentos, mas a montagem e gravação das hierarquias completas precisam ocorrer no seu Unity.** Este ambiente não possui o editor para executar essa etapa. Enquanto uma cena ainda não foi preparada, o runtime monta seu conteúdo a partir do catálogo ao dar Play.

Depois da primeira preparação, cada cena de conteúdo possui:

- **00 — Configuração:** componente `HunterStage`, com ID, posição da câmera, ponto observado, campo de visão e referência ao cenário.
- **01 — Cenário editável / Arquitetura e iluminação:** bases de maquete, patamares, rios e caminhos primitivos; o quarto mantém paredes e luzinhas.
- **Brinquedos e decoração:** modelos originais que compõem o ambiente.
- **Objetos da charada:** seis objetos identificados nas cenas de busca.
- **Passagens e mecanismos:** entradas, destinos e peças interativas.

As configurações de materiais criadas para a arquitetura primitiva ficam em `Assets/Hunter/Settings/RoomMaterials`. Os modelos e texturas de terceiros continuam em Resources; nenhuma geometria artística é modelada pelo preparador.

## Como editar

Mova, gire ou redimensione os objetos na cena preparada e salve a cena. O jogo usa essas posições e a câmera de `HunterStage` ao carregá-la. O componente `HunterSceneProp` mantém a identidade e a ação de cada brinquedo; preserve IDs de alvos e destinos de passagens.

O campo `geometry` descreve as primitivas arquitetônicas de cada maquete; modelos artísticos continuam vindos dos downloads. `openedOffset` e `openedRotation` indicam como um mecanismo se move ao abrir, relativamente à posição editada.

Os textos, regras, peças necessárias e charadas continuam em `Assets/Resources/Hunter/catalog.json`. Mudar o campo `asset` de um objeto já montado não substitui sua malha automaticamente: atualize o catálogo e recrie a cena, ou substitua conscientemente o modelo na hierarquia, mantendo os dados e colisores correspondentes.

**Preparar projeto preserva as cenas marcadas como preparadas.** Para aplicar novamente as posições do catálogo, use **7. Recriar cenas a partir do catalogo**. Esse comando avisa que sobrescreve as alterações manuais das 17 cenas de conteúdo; guarde suas alterações antes de confirmar. `Main`, gravações humanas e saves não são recriados por esse comando.

É possível iniciar o Play numa cena de busca para inspecioná-la diretamente; isso não simula a progressão normal de entrada. Um desafio aberto diretamente só inicia se o save já tiver os objetos exigidos; caso contrário, o jogo vai ao caminho do mundo. Para testar a aventura completa, comece por `Main`.

## Responsabilidades no código

| Arquivo | Responsabilidade |
| --- | --- |
| `HunterGame.Scenes.cs` | Sessão persistente, carregamento assíncrono e entrada por uma cena específica |
| `HunterStage.cs` | Identificação, câmera e raiz editável de uma cena |
| `HunterSceneProp.cs` | Dados de interação junto ao objeto da hierarquia |
| `HunterSceneBuilder.cs` | Preparação explícita das cenas no editor e registro no build |
| `HunterGame.Navigation.cs` | Caminhos intermediários, peças encontradas e passagens |
| `HunterGame.World.cs` | Câmera, rótulos e cliques no cenário |
| `RoomView.cs` | Vincula os objetos da cena ao progresso; monta a alternativa por catálogo quando necessário |
| `Domain/GameData.cs` | Regras de coleta, passagens, conclusão e recuperação do save |

O fluxo de carregamento e a gravação das cenas ainda precisam ser testados no editor. Consulte [VALIDACAO.md](VALIDACAO.md).
