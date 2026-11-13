# Referência em vídeo — adaptação v0.7

Estado em 13/11/2026: **ideias implementadas no código e no catálogo; importação, compilação e playtest Unity ainda pendentes**.

A referência é `videoplayback.mp4`, enviado pelo usuário, com aproximadamente 17 minutos de um mundo de I Spy Fantasy no modo fácil. Foram revistas as sequências do quarto e da entrada no castelo (00:35–00:47), do cenário do mago e sua charada (01:36–01:56), além das sequências de descobertas e desafios. O vídeo e suas capturas não são assets do projeto. Não copiamos personagens, cenários, imagens, sons ou código de I Spy.

| Ideia observada | Implementação nesta revisão | Conferência realizada |
| --- | --- | --- |
| Cenário dominante; charada em faixa inferior | Área 3D de 1440 × 704 numa referência de 1440 × 900, cerca de 78% da altura. Cabeçalho de 56 px e faixa inferior de 140 px, com quatro versos em duas colunas | Coordenadas e larguras de texto conferidas; testar no Game View e com janela redimensionada |
| Sobreposições, objetos próximos e diferenças de escala | Nove composições com 271 instâncias, contra 118 antes; 28–32 por tela. Recipientes, caixas empilhadas, bancada/prateleira, pedras, prédios e móbiles espaciais | Projeção com oclusão confere área visível dos 18 alvos. Prévias externas em Blender ajudaram a corrigir enquadramentos e interseções; não são screenshots Unity |
| Brincadeira montada fisicamente | Mesmos modelos Kenney de madeira, ferramentas, móveis, naves e bonecos, com materiais/texturas originais. Prateleiras, caixas, faixas de piso e suportes dos móbiles usam somente primitivas autorizadas | 190 arquivos de arte/áudio de terceiros idênticos por SHA-256 |
| Entradas fazem parte do quarto | Quarto 3D com baú, nave e heroína como entradas. Clique na malha ou no rótulo, que informa contagem e selo | Três destinos conferidos contra os IDs dos mapas; meshes visíveis na projeção |
| Navegação integrada ao ambiente | Duas placas 3D em cada tela levam aos outros cantinhos. Abas inferiores removidas. Rótulos clicáveis oferecem área adicional de clique | As 18 passagens formam rotas válidas e não dependem de coletar alvos |
| Enquadramentos próximos e pequenas reações | Câmeras por tela; foco breve em descobertas e brinquedos curiosos; alvos saltam, giram, inclinam ou decolam antes de desaparecer. Decorações respondem ao clique | Código e sintaxe conferidos. Movimento reduzido elimina deslocamentos/rotações e mantém texto e indicação estática |
| Desafios pertencem ao mundo | Clique no baú da Mina, rádio da Garagem ou maquete do Quartel após os seis alvos. Cada um abre um enquadramento 3D próprio; peças e rótulos compartilham a mesma ação | Índices, peças, soluções e acesso conferidos; interação e temporização exigem Play |
| Recompensa preserva o cenário | Vitória mantém o cenário 3D visível, mostra o selo e reage com os brinquedos presentes; nave dá um salto de decolagem e figuras da cidade comemoram | Fluxo implementado; percepção e duração precisam de playtest |

## Os três desafios

- **Faroeste:** clique em três garrafinhas para ajustar os números e depois no baú. A solução continua 243. A comemoração usa o baú existente; não foi criada uma tampa articulada ou um modelo novo de baú aberto.
- **Espaço:** os brinquedos Lua, Sol, Estrela e Planeta recebem sinais com nome escrito, indicação de escala e som opcional. Durante a demonstração, cliques nas peças não contam. A sequência continua Estrela → Lua → Planeta → Sol.
- **Heróis:** selecione a antena, balde ou martelo e depois a miniatura da praça, ponte ou quartel. Uma ferramenta correta é levada ao destino; com movimento reduzido, muda de posição imediatamente. Acertos não são reutilizados e erros não apagam o progresso.

## Narração, progresso e limites

Os 18 IDs, nove charadas, roteiros de narração, soluções e chave de save foram preservados. `Ler juntos` permanece disponível; `Ouvir charada` depende da gravação humana de cada sala. Nenhuma voz foi criada. A abertura colorida, opções separadas, pausas e saída Windows foram mantidas.

Esta é uma adaptação das ideias à estética dos assets prontos do projeto. Não pretende reproduzir o acabamento fotográfico de I Spy. Não há animações esqueléticas novas, personagens gerados, texturas pintadas, cursor ilustrado novo ou trilha gerada.

As composições foram revisadas externamente e o código foi analisado, mas este ambiente não possui Unity. A confirmação de cores após importação, colisores, animações, resolução, desempenho e funcionamento dos puzzles depende de [VALIDACAO.md](VALIDACAO.md).
