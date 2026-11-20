# Validação 0.12 — 19/11/2026

## Executado neste ambiente

| Verificação | Resultado | Limite |
| --- | --- | --- |
| Domínio e persistência C# reais | 629 verificações passaram | Executadas com .NET 8, fora do Unity |
| Isolamento e retomada | Perfis independentes, sala, progresso, números do baú, nove rodadas, reset parcial e migração | Não substitui interação de UI |
| Integridade do save | Escrita temporária, backup anterior, checksum, corrupção de principal/backup, recuperação e preservação de original | Testado no filesystem Linux |
| Conteúdo | 3 mundos, 18 cenas, 27 charadas, 162 IDs únicos, 651 instâncias nas buscas | Dados, modelos, referências e scripts |
| Assets originais | 411 arquivos conferidos por SHA-256 | Sem importação Unity |
| Materiais e imagens | 293 vínculos e 29 importadores de imagens 2D | Sem renderização URP |
| Sintaxe | 25 arquivos C# sem erros no parser | Não resolve APIs Unity |
| APIs de plataforma | Domínio, saves e auxiliares Windows compilam contra .NET Standard 2.1 com símbolo Windows | Debug substituído por stub; sem execução do Windows |
| Visibilidade | Todos os alvos e pontos interativos com pelo menos 35 pixels na projeção externa (720 px de largura) | Não substitui Physics.Raycast, MSAA ou Play |
| Composição | Quarto, base espacial, baú e comunicador renderizados e inspecionados externamente | Prévias Blender, não Unity |

O teste executável de domínio está em `Tools/DomainChecks`. Os testes NUnit foram atualizados para as três rodadas e exploração livre; foi adicionado um teste de descoberta e recarga que verifica que o objeto permanece visível e com colisores. **Os testes NUnit não foram executados neste ambiente.**

## Pendências no Unity/Windows

1. Importar em Unity 6000.6.4f1, preparar o projeto e verificar conteúdo. Console sem erros ou materiais rosa.
2. Test Runner/EditMode: executar ProgressTests e BuildContentTests, incluindo preservação de objeto após descoberta e recarga.
3. Encontrar um item: confirmar voz, retorno da animação à pose original, verso marcado, ausência de mensagens, item visível e clique repetido sem contagem extra.
4. Sair da sala e voltar; fechar e reabrir o executável; o mesmo objeto deve continuar visível.
5. Testar dois jogadores, nova partida, troca, reset de um mundo, backup e retomada de números/associações conforme SALVAMENTO.md.
6. Testar as 27 charadas e completar as nove rodadas. Repetir a tentativa de um final antes de reunir os 18 alvos; sair e voltar deve permitir jogar novamente.
7. Ir e voltar por todas as passagens sem concluir as buscas. Verificar setas e colisores com câmera fixa em 1440×900, 1920×1080 e 1024×768.
8. Testar Windows com voz pt-BR, com voz padrão de outro idioma, com clipes gravados e com PowerShell indisponível. Sem clipe e sem serviço de voz, a narração não pode ser produzida; os versos continuam disponíveis.
9. Conferir mute, volume, pausa, perda de foco, troca rápida de cena e fila de várias descobertas. Não deve ficar fala de uma sala anterior.
10. Observar cambalhotas, giros, órbitas e squash com movimento normal e reduzido; conferir todos os mecanismos e pistas de puzzle.
11. Medir desempenho com Profiler e executar build Windows x64. Não há medição de FPS nem executável nesta entrega.

## Alcance

A branch de trabalho foi preparada a partir do commit remoto `9f0779a`. Publicação e renomeação do repositório exigem autenticação GitHub e não foram executadas. A preparação de cenas de versão antiga recompõe o catálogo: guardar edições manuais antes de atualizar.
