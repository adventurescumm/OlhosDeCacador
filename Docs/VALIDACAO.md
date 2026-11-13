# Validação e roteiro de testes — v0.8

Data: **13/11/2026**. Destino: **Unity 6000.6.4f1** (`12bfff696524`), Windows x64, URP 17.6.0 e Input System 1.20.1 (New).

## Evidência disponível

| Verificação | Resultado | Limite |
| --- | --- | --- |
| Catálogo | 3 mundos, 9 buscas, 54 IDs de alvo, 6 versos por busca e 614 instâncias nas buscas; 115 modelos distintos usados | Integridade dos dados, sem execução Unity |
| Progressão | 3 caminhos, 6 passagens com peças e 3 desafios finais; requisitos e destinos consistentes | Cliques, transições e ritmo precisam de playtest |
| Cenas | 18 arquivos `.unity`, nomes únicos, GUIDs e lista de build correspondentes; 17 marcadores de conteúdo | As hierarquias completas serão montadas por Preparar projeto no Unity |
| Domínio C# | **97 verificações passaram** com o arquivo real `Domain/GameData.cs` compilado pelo Roslyn/.NET 8 | Compilação apenas da lógica independente, sem APIs Unity |
| Sintaxe C# | 18 arquivos analisados por parser C#, sem erro de sintaxe | Não verifica ligação às APIs do editor e dos pacotes |
| Arquivos de terceiros | 309 arquivos conferidos por SHA-256 contra as fontes baixadas; 141 modelos OBJ | Os arquivos artísticos originais não foram alterados |
| Materiais | 246 vínculos URP/Lit e 19 importadores PNG explicitamente 2D; cores/texturas correspondem aos MTL | Aparência e importação reais ainda pendentes |
| Importadores | 141 ModelImporter com schema 19300 e 7 AudioImporter com schema 6; nenhum bloco abaixo do mínimo | A ausência dos avisos precisa ser confirmada no Console |
| Narração | Nove IDs e roteiros correspondentes no catálogo, banco e NARRACAO.md; seis versos por leitura | Os campos de áudio continuam vazios |
| Entrada e renderer | New Input System, UI Input Module, URP/Forward, referências Graphics/Quality e luzes adicionais por pixel | Resolução dos pacotes e execução dependem do Unity |
| Composição | 16 composições distintas revistas externamente; a abertura e o quarto compartilham a composição | Prévias de Blender não são capturas Unity |
| Visibilidade | Projeção com oclusão por triângulos dos modelos e áreas de interface verifica os 54 alvos e entradas/mecanismos | Não substitui MeshCollider, raycast, UI ou renderização real |

**Não há compilação completa Unity, montagem de cenas, Play, build Windows ou medição de desempenho executados neste ambiente.** O editor não está instalado. O teste de domínio e o parser não comprovam que os scripts ligados ao Unity compilem. Os resultados externos de composição não devem ser apresentados como capturas de uma versão validada do jogo.

As correções anteriores de propriedades URP somente leitura, texturas CUBE em slots 2D, reimportação e metadados foram preservadas. Os materiais e GUIDs antigos também foram mantidos. A revisão acrescenta normais calculadas por ângulo, modelos prontos e fontes, sem editar a geometria artística original.

Para repetir as verificações estáticas incluídas:

```bash
python Tools/validate_content.py
python Tools/repair_importer_metadata.py --check
```

## Testes de domínio

O ensaio externo com 97 verificações cobre coleta repetida e IDs desconhecidos, seis passagens, peças erradas/ausentes, busca de origem incompleta, uso correto, persistência, migração de saves antigos, reinício parcial e os três desafios. Ele usa o catálogo real e percorre a aventura completa na camada de regras.

O projeto também inclui **11 testes NUnit** em `OlhosDeCacador.Tests`. Execute-os em **Window > General > Test Runner > EditMode**. O Test Runner do Unity não foi executado nesta entrega. Esses testes não cobrem interface, colisores, importação, armazenamento físico ou executável Windows.

## Primeira verificação no Unity

1. Abra uma cópia nova do pacote no editor exato; preserve a versão anterior e suas gravações.
2. Aguarde os pacotes e a importação. Confira o primeiro erro completo do Console, caso apareça.
3. Abra `Assets/Hunter/Scenes/Main.unity`; execute **1. Preparar projeto** e **2. Verificar conteudo**.
4. Abra uma cena de busca: a hierarquia deve conter arquitetura, decoração, seis objetos da charada e passagens. As 18 cenas devem constar na lista do build.
5. Execute o Test Runner. Depois inicie a aventura por Main e complete o roteiro abaixo.

## Roteiro de playtest

Todos os casos abaixo estão **pendentes**. Registre data, versão Windows e hardware. Leitura de código não aprova uma linha.

| Caso | Ação | Resultado esperado |
| --- | --- | --- |
| Importação | Abrir, preparar, verificar e limpar Console | Sem novo CS0200, CUBE/2D, ciclo de importação ou importador abaixo do mínimo |
| Reparo repetido | Executar Restaurar materiais originais novamente | Zero correções quando os vínculos já estiverem certos; importação estável |
| Pipeline e entrada | Conferir Graphics, Quality, pacotes e Active Input Handling | URP ativo e New Input System; controles funcionam sem backend legado |
| Montagem das cenas | Executar Preparar projeto duas vezes | Primeira execução preenche as 17 hierarquias; segunda preserva cenas já preparadas |
| Edição individual | Mover um distrator e a câmera de HunterStage, salvar, preparar e jogar | Posições editadas preservadas e usadas pelo jogo |
| Recriação explícita | Numa cópia de teste, cancelar e depois confirmar Recriar cenas | Cancelar preserva; confirmar restaura o catálogo nas 17 cenas; Main não muda |
| Cenas abertas | Preparar com uma cena de conteúdo ainda não montada aberta | Preparação segura e restauração do conjunto de cenas abertas |
| Abertura | Jogar a partir de Main | Quarto ao fundo, título, Iniciar, Opções e Como jogar; sem seletores de mundos na abertura |
| Atalhos | Começar por clique, Enter e Espaço; testar também outros botões selecionados | Uma transição por ação; o atalho não substitui Opções/Como jogar |
| Quarto | Clicar no baú, nave e heroína, também pelos rótulos | Mundos corretos; entradas afastadas e progresso 0–18 por mundo |
| Busca | Visitar todas as nove telas e encontrar seis objetos em cada uma | Uma contagem por ID, sem nome/destaque automático ao passar o mouse; coleta com reação |
| Dificuldade | Pedir a uma pessoa sem roteiro que complete a primeira busca | Charada compreensível e desafio de observação; ajustar posição/tamanho se necessário |
| Enquadramento | Inspecionar 54 alvos, entradas e peças, em 1440×900 e 1280×720 | Malhas clicáveis, texto legível e nenhum alvo oculto pela interface |
| Oclusão | Clicar no obstáculo à frente de um alvo | Clique não atravessa a superfície |
| Dicas | Pedir as três etapas de ajuda para cada um dos seis versos | Contexto, localização e destaque final, sem perder progresso |
| Caminho | Voltar de cada busca e escolher entradas | Cenário intermediário visível; rotas seguintes inicialmente fechadas |
| Uso de peças | Tentar uma peça ausente, origem incompleta, peça errada e peça correta | Só a correta, com busca de origem completa, abre a passagem; objeto colocado fica visível |
| Persistência de passagem | Abrir uma passagem, trocar mundo e reiniciar o jogo | Passagem aberta e peça colocada preservadas |
| Troca de cena | Alternar menus, buscas, caminhos e desafios ao menos 30 vezes | Uma cena de conteúdo, uma sessão, um EventSystem e um AudioListener; sem duplicar luzes ou UI |
| Pausa/opções | Pausar em busca e caminho; ajustar movimento/áudio; voltar por botão e Esc | Retorno à mesma cena e progresso; sem carregamento concorrente |
| UI sobre mundo | Fechar caderno, leitura ou pausa com alvo atrás do botão | O mesmo clique não coleta o alvo |
| Narração | Atribuir uma gravação de seis versos; ouvir, parar, repetir e trocar cena | Áudio correspondente; interrupção sem sobreposição; volumes independentes |
| Leitura sem áudio | Usar Ler juntos nas nove buscas | Seis versos completos; retornar preserva a busca |
| Leitura automática | Ativar e visitar telas com e sem clipe | Reproduz somente clipes presentes; texto sempre disponível |
| Baú final | Depois dos 18 alvos, tentar senha incorreta e 243 | Nova tentativa; selo apenas com resposta correta |
| Sinais finais | Rever, clicar durante demonstração, errar, repetir e acertar | Entrada bloqueada na demonstração; sequência reinicia corretamente |
| Associação final | Tentar ferramenta errada e depois as três corretas | Acertos preservados, sem reutilização indevida |
| Saída do desafio | Sair durante sinais ou após uma tentativa | Coroutine interrompida; retorno ao caminho ou busca de origem |
| Movimento reduzido | Habilitar e coletar/interagir/trocar telas | Sem pulos, rotações e foco animado; ações mantêm resultado claro |
| Foco/dispositivos | Alt+Tab durante sinais; testar ausência de mouse ou teclado | Retorno utilizável e nenhuma referência nula de dispositivo |
| Save anterior | Carregar save v0.7 | IDs antigos preservados; conclusões sem os 18 alvos novos são filtradas; passagens ainda por resolver |
| Reinício | Reiniciar um mundo, depois cancelar e confirmar Novo jogo | Reinício parcial preserva outros mundos; confirmação geral limpa só este jogo |
| Save danificado | Em instalação de teste, corromper somente a chave principal | Recuperação do backup e mensagem; partida utilizável |
| Vitória | Completar os três mundos em sequência | Três selos, mensagem final e retorno funcional |
| Build Windows | Compilar e executar toda a pasta de distribuição fora do editor | Aventura completa, salvamento e saída funcionam sem Unity aberto |
| Saída | Cancelar e confirmar Sair, no quarto e na pausa | Cancelar retorna; confirmar encerra o executável ou apenas o Play Mode no editor |

Para corrupção controlada, use apenas `OlhosDeCacador.v1` e `OlhosDeCacador.v1.backup` numa instalação de teste. Não use `PlayerPrefs.DeleteAll()`.

## Desempenho e diagnóstico

Meta de trabalho: **60 FPS**, ainda sem medição. Registre hardware, frame time, draw calls, triângulos, memória e picos em trocas de cena. Verifique a memória após muitas trocas. Ajuste sombras, materiais e densidade com evidência do Profiler; preserve a oclusão dos cliques.

- **Cena sem objetos no editor:** execute Preparar projeto; consulte [CENAS.md](CENAS.md). As cenas do ZIP ainda precisam da primeira montagem no Unity.
- **Mudança no catálogo não apareceu:** a preparação preserva cenas montadas. Edite a cena ou use a recriação explícita, guardando alterações próprias.
- **Material rosa/cinza:** aguarde URP, execute Preparar projeto e Verificar conteudo. Modelos usam 2D/Repeat; não use Clamp nos robôs.
- **Conteúdo antigo ou referências ausentes:** instale a revisão completa conforme [ATUALIZACAO_V08.md](ATUALIZACAO_V08.md); scripts, cenas, catálogo e assets formam a mesma versão.
- **Narração antiga:** regrave os nove roteiros de [NARRACAO.md](NARRACAO.md); os IDs iguais não tornam o áudio de dois alvos compatível com seis.
- **Outro erro:** registre a primeira mensagem completa do Console, com arquivo e linha, e a ação que a produziu.
