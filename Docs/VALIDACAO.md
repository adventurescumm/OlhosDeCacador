# Validação 0.15 — 20/11/2026

## Executado neste ambiente

| Verificação | Resultado | Limite |
| --- | --- | --- |
| Domínio e persistência C# reais | 657 verificações passaram com .NET 8 | Não simula cliques na interface |
| Testes Unity EditMode | 19 passaram, zero falhas | Inclui permanência do objeto e colisores após descoberta/reentrada |
| Compilação Unity | Unity 6000.6.4f1 compilou e importou o projeto | Sem build Windows |
| Cenas e conteúdo Unity | 18 cenas reconstruídas, 27 charadas, 162 IDs, materiais e importadores válidos | Validação do editor executada |
| Enquadramento e camuflagem | 162 alvos e 54 passagens clicáveis; 133 silhuetas parcialmente encobertas | Amostragem de raios sobre a malha, com limite de reconhecimento; não é partida manual |
| Composição | 16 prévias renderizadas no próprio Unity, incluindo quarto, mapas, buscas e desafios | Sem interface de execução; Docs/Previews/v15 |
| Estrutura Python | Todos os testes de conteúdo e hashes passaram | Navegação, cenas, charadas, materiais e bytes originais |
| Modelos complementares | 22 arquivos novos conferidos com os ZIP Kenney, 253.665 bytes | Nove modelos de Pirate Kit e Castle Kit |
| Integridade dos assets | 445 hashes corretos, zero divergências; 202 arquivos reparados | Terminações de linha originais restauradas; hashes esperados preservados |

Os testes estão em `Tools/DomainChecks` e `Assets/BemTeVi/Tests/Editor`. As evidências desta execução ficam em `Logs/EditMode-v15.xml`, `Logs/EditMode-v15.log` e `Logs/LayoutReview-v15.log` (logs locais ignorados pelo Git). Para reproduzir a conferência de câmera, use **Bem-Te-Vi > 8. Conferir enquadramentos e gerar previas**.

## Pendências no Unity/Windows

1. Fazer uma partida manual pelo Main no Unity 6000.6.4f1; conferir interface, placas e transições.
2. Repetir os testes EditMode quando houver alterações de regras, cenas ou materiais; os 19 testes desta revisão passaram.
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
