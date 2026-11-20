# Revisão 0.13 — mapas abertos e buscas organizadas

> Registro histórico: as divergências de hashes mencionadas abaixo foram resolvidas na [revisão 0.15](ATUALIZACAO_V15.md), sem trocar os hashes esperados.

Cada mundo agora tem uma sala de transição que funciona como mapa físico. Cinco destinos ficam ligados por caminhos de fita: três buscas, um desafio e a saída para o quarto. As miniaturas e as placas permitem entrar em cada destino. As placas das buscas mostram o progresso daquela rodada; não revelam objetos. Todas as buscas e desafios têm retorno direto pelo botão **Mapa** e por uma passagem do cenário. Uma busca incompleta nunca impede a visita a outra.

Os seis mecanismos que dependiam de peças e as peças encaixadas foram removidos do catálogo. Também foram retiradas as instruções que sugeriam procurar itens para abrir passagens. Os campos antigos de save continuam legíveis; ao carregar, flags de passagens que deixaram de existir são descartadas, preservando descobertas, rodadas e jogadores.

## Composição

As nove salas cresceram de 14 × 10 para **18 × 12,6 unidades**. Cada uma contém seis conjuntos de objetos, com superfícies baixas e uma alameda livre. Os cenários mantêm silhuetas próprias: estação, salão, mina dentro do baú, base, jardim de meteoros, garagem, praça, ponte e quartel. Os objetos grandes ficam atrás das buscas para não cobrir os alvos.

| Mundo | Alvos disponíveis por sala | Extras pequenos por sala |
| --- | ---: | ---: |
| Faroeste de Quintal | 18, seis por rodada | 12 |
| Missão Caixa Lunar | 18, seis por rodada | 15 |
| Liga dos Brinquedos | 18, seis por rodada | 18 |

Os alvos ficam menores nas rodadas posteriores e um pouco menores nos mundos seguintes. Todos permanecem no cenário após descobertos. As 27 charadas, as confirmações do narrador e os 162 IDs foram preservados. As buscas no baú, o desafio de memória na sala inteira e o comunicador mantêm suas escalas próprias.

As três entradas do quarto passaram de posições horizontais −3,1 / 0 / 3,1 para **−2,5 / 0 / 2,5**. As bases foram estreitadas junto com os brinquedos, mantendo separação entre as miniaturas. Não há alegação de equivalência em pixels ao vídeo.

## Modelos

Foram selecionados cinco modelos do [Marble Kit de Kenney](https://kenney.nl/assets/marble-kit), licença CC0: `marble-low`, `marble-center-low`, `helix-left`, `funnel` e `banner`. O [espelho publicado pelo autor](https://opengameart.org/content/marble-kit) forneceu o ZIP; o endereço do site oficial retornou HTML nesta sessão. São **97.194 bytes** de arquivos de origem, incluindo cinco OBJ, cinco MTL, um atlas e a licença. O ZIP completo fica fora do projeto. As bolinhas ocupam posições de decoração existentes; espirais e funis aparecem nos mapas e reagem a cliques.

`Tools/import_marble_selection.py <ZIP>` reproduz a seleção. `Tools/prepare_materials.py --pack marble-kit` prepara somente os materiais desse pacote. `Tools/recompose_v13.py` registra a migração de composição e preserva os IDs; em catálogo já atualizado não altera nada.

## Verificação

- 657 verificações reais C# de domínio e persistência passaram.
- Os 17 testes EditMode passaram no Unity, incluindo o teste que descobre, revisita e confirma a permanência e os colisores do objeto.
- Importação, compilação e validação de conteúdo do Unity 6000.6.4f1 passaram; as 18 cenas foram reconstruídas com conteúdo versão 13.
- A conferência de câmera e raycast encontrou os 162 alvos visíveis e clicáveis. As imagens em `Docs/Previews/v13` foram renderizadas no Unity; não incluem a interface de execução.
- `Tools/validate_content.py` confere conexões de ida e volta, placas, espaçamento dos alvos, cenas, modelos, charadas e materiais. A comparação de cores aceita somente a pequena diferença de representação de floats serializados pelo Unity; a leitura dos arquivos usa UTF-8 explicitamente no Windows.
- A auditoria de hashes ainda retorna falha em 202 arquivos OBJ/MTL **já divergentes no estado inicial de Git**. Os bytes desses arquivos coincidem com `HEAD`; os hashes do manifesto não foram alterados para ocultar a divergência. Os 12 arquivos novos conferem com o ZIP. O validador agora apresenta a contagem e conclui as demais verificações antes de retornar falha.

A tentativa de abrir o vídeo fornecido foi bloqueada pelo serviço de consulta; esta revisão usa os critérios descritos no pedido e a documentação existente. Não foi assistido o longplay completo nesta sessão. Voz no Windows, ritmo, dificuldade percebida e uma partida completa continuam dependendo de avaliação manual. Nenhum executável foi produzido.
