# Atualização 0.12 — Bem-Te-Vi

Esta revisão consolida a expansão anterior e os pedidos de 19/11/2026. A base remota conferida é `9f0779a`, cuja inicialização apagava as chaves de progresso. Esse comportamento foi substituído pelo sistema de cadernos solicitado.

## Instalação

Abra o projeto em uma pasta nova, preservando gravações e composições manuais. Em Unity 6000.6.4f1, abra Main e execute Preparar projeto e Verificar conteudo. A versão de conteúdo passa a 12: cenas preparadas com versões anteriores são recompostas a partir do catálogo. Depois disso, novas edições nas cenas são preservadas até outra mudança de versão ou o comando explícito Recriar cenas.

As pastas, namespaces, menus, produto e nome do executável usam Bem-Te-Vi/BemTeVi. GUIDs e IDs antigos permanecem. Somente identificadores de migração mantêm o nome legado para recuperar saves.

## Alterações

- 19 modelos leves adicionais, 27 charadas e 162 alvos em três rodadas por mundo.
- Rotas livres entre buscas, sinais físicos de passagem e puzzles em salas, baú e comunicador.
- Animações de brinquedos sem remoção, redução de escala permanente ou deslocamento persistente.
- Remoção integral do sistema de dicas e das mensagens de descoberta/erro de seleção.
- Confirmações faladas por item, com fila e interrupção em mudança de contexto.
- Bases com camadas e acabamento, enquadramentos ajustados e entradas do quarto agrupadas: distância horizontal dos mundos extremos reduzida de 10,2 para 6,2 unidades (aproximadamente 39%). A comparação é de composição; não se promete uma medida exata do vídeo.
- Cadernos de jogadores, autosave, retomada de sala/puzzles, integridade, backup e migração.

## GitHub

A entrega contém a revisão local em uma branch baseada no estado remoto conferido. A autenticação GitHub não estava disponível no ambiente; publicação, pull request e renomeação remota não foram executadas. O nome desejado para o repositório é `Bem-Te-Vi`. Depois de publicar a branch, revisar e integrar, o proprietário pode alterar Repository Settings → General → Repository name; atualize então a URL do remote local. O código entregue já usa o nome novo.

## Limites

O ambiente não possui Unity nem Windows. Os testes de domínio e de gravação foram executados, mas a compilação Unity e a voz de Windows precisam de validação no computador de destino. O fallback de voz depende de PowerShell/System.Speech e de uma voz instalada; gravações atribuídas ao NarrationBank dispensam esse fallback.
