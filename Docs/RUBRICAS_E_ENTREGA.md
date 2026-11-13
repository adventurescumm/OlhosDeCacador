# Rubricas e preparação da entrega

Fonte: [documento fornecido da disciplina](https://docs.google.com/document/d/1jBL3xrub3hth5j5P7CbFNZcJv1uFdbAc/edit), consultado em 13/11/2026. Prazo indicado: **25/11/2026**. A entrega pede uma versão Windows, repositório do código, GDD atualizado e publicação na [jam do itch.io](https://itch.io/jam/projeto-jogo-3d-insper-20262), com a confirmação nos canais da disciplina. A versão web deixou de fazer parte do escopo, conforme a orientação posterior do usuário. A publicação prevista é um download Windows.

A matriz abaixo descreve decisões deste protótipo e evidências que a equipe deve produzir. Não atribui uma nota e não substitui avaliação do professor.

| Frente da rubrica | Decisão implementada no código | Evidência para demonstrar | Estado |
| --- | --- | --- | --- |
| Controles, câmera e sensação 3D | Câmera de perspectiva fixa; transição curta; seleção por raio; UI separada da área de jogo | Gravar navegação e seleção em nove telas; testar alvos pequenos e oclusão | Teste no Unity pendente |
| Desempenho, fluidez e colisões | Uma cena de conteúdo ativa; quarto com três entradas; modelos leves; malhas como colisores estáticos; sem física de locomoção | Profiler, cliques em superfícies, troca repetida de telas e sessão completa no executável Windows | Medição pendente |
| Modelos, materiais e texturas | 141 modelos prontos de oito packs 3D; dois packs adicionais de efeitos e interface; fonte Lilita One; texturas originais; escala normalizada e 246 materiais ligados explicitamente ao conteúdo MTL | Inspecionar as nove telas coloridas, os robôs vermelho/violeta/amarelo, sombras, escala, molduras e faixa de charada | Integridade dos arquivos verificada; aparência Unity pendente |
| Gerenciamento de estados e erros | Progresso por ID; proteção contra duplicação; tentativas ilimitadas; reinício parcial; recuperação de save | Executar testes de domínio e roteiro de falhas; concluir todos os mapas sem reabrir o jogo | 97 verificações do domínio C# passaram fora do Unity; Test Runner pendente |

Como o jogo é point-and-click, travamento de personagem em quinas não se aplica. A evidência equivalente de colisões é respeitar o primeiro obstáculo entre câmera e alvo. Como não há vidas, “falha” significa uma tentativa incorreta de puzzle, com retorno claro para nova tentativa. Se o professor esperar locomoção ou uma tela de derrota literal, alinhar essa interpretação antes de expandir o projeto.

## Organização até a entrega

| Período sugerido | Resultado esperado |
| --- | --- |
| 13–15/11 | Importar, resolver eventuais incompatibilidades, executar testes e completar o jogo no editor |
| 16–18/11 | Melhorar composição de cada mapa; testar pistas com duas ou três pessoas |
| 19–21/11 | Medir desempenho no Windows; corrigir casos de reinício, foco e áudio |
| 22–24/11 | Preparar build Windows final, README, GDD, créditos, capturas reais e página itch.io |
| 25/11 | Conferir arquivos publicados e concluir o procedimento de entrega da disciplina |

## Conferência antes de enviar

- Fonte versionada, incluindo `.meta`, `Packages` e `ProjectSettings`; sem `Library`, `Temp` ou caches.
- Build Windows completo, executado em um computador de teste.
- Nomes e contribuições da equipe preenchidos; referências e assistência de código declaradas.
- Créditos dos packs e suas licenças mantidos no repositório.
- GDD atualizado para refletir mudanças reais.
- Instruções de como jogar na página, incluindo pistas e eventual informação importante para avaliação.
- Capturas e vídeo feitos da versão que será entregue. Prévia de composição não substitui captura do jogo.
- Todos os 54 objetos e três puzzles verificados numa partida contínua.
- Gravações humanas adicionadas, se desejadas; testar volumes, repetição, troca de sala e alternativa de texto.
- Tela de opções e seleção de mundos no quarto testados em janela e fullscreen.

Nenhum repositório foi criado nem uma publicação foi enviada por esta entrega de arquivos.
