# Olhos de Caçador — GDD v0.9

Point-and-click 3D para Windows x64, Unity 6000.6.4f1, URP 17.6.0 e Input System 1.20.1. Um quarto infantil contém três maquetes: faroeste, espaço e cidade de super-heróis. A referência de gênero é I Spy; modelos e imagens da referência não foram incorporados.

## Exploração

Abertura com título, Iniciar, Opções e Como jogar → quarto → clique numa maquete → exploração por passagens físicas. Não há menu de destinos ou lista de níveis dentro dos mundos. Portas, trilhos, pontes e túneis são objetos clicáveis que conectam cenas vizinhas.

Cada mundo contém uma entrada, três buscas e um desafio final, formando um circuito: entrada ↔ busca 1 ↔ busca 2 ↔ busca 3 ↔ entrada. A busca 3 também se liga ao desafio. As duas passagens de progresso ficam bloqueadas até os mecanismos funcionarem; o retorno entre busca 3 e entrada torna-se um atalho. A passagem ao quarto fica na entrada.

São 18 cenas ao todo: Main, abertura, quarto e cinco por mundo. Câmeras, UI e progresso persistem entre cargas. O editor monta as hierarquias na primeira preparação, preservando edições posteriores; consulte CENAS.md.

## Busca e mecanismos

Seis objetos por busca, 54 no total. Há 611 instâncias nas buscas, incluindo cenário, alvos, passagens e peças que só aparecem após o uso. Os objetos ficam em agrupamentos ligados à arquitetura, em balcões, patamares, margens de rios e entre brinquedos. Escalas e alturas variam. Uma cópia visível indistinguível de um alvo é evitada antes de sua coleta.

| Mundo | Mecanismo na busca 1 | Mecanismo na busca 2 | Final |
| --- | --- | --- | --- |
| Faroeste | Roda encontrada põe a diligência em movimento | Martelo permite baixar a ponte | Senha 243 |
| Espaço | Antena orienta o observatório | Rádio põe o veículo lunar em movimento | Estrela, Lua, Planeta, Sol |
| Super-heróis | Balde faz o jardim reagir | Martelo permite baixar a ponte | Balde → Praça; Martelo → Ponte; Antena → Quartel |

Complete os seis objetos e clique no mecanismo: a peça correspondente é aplicada automaticamente, aparece montada e o brinquedo muda de posição/orientação. O resultado é salvo. Isso evita o seletor de peças da antiga navegação. As associações ainda são simples; os desafios finais mantêm combinação, memória e associação.

Alvos e decoração comum mostram o mesmo texto ao passar o mouse. Não há ampliação automática exclusiva dos alvos. Dicas explícitas oferecem contexto, região e destaque. Roda do mouse aproxima a região apontada; botão direito restaura o plano geral. Essa aproximação funciona sobre qualquer parte do cenário.

A dificuldade precisa ser medida com pessoas que não conheçam as composições: tempo por descoberta, cliques incorretos, uso de dicas e identificação das passagens. Quantidade de objetos e oclusão externa não comprovam equilíbrio de jogo.

## Direção visual e assets

Os mundos têm bases espessas de maquete sobre uma mesa, construções em miniatura, patamares, ruas, rios e caminhos. As composições e os enquadramentos variam entre estação, salão, mina, base, jardim lunar, hangar, praça, ponte e quartel. O quarto mantém mobiliário, luzinhas e três entradas afastadas.

Modelos existentes: oito packs 3D Kenney e Rubber Duck Toy de Plat251/Poly Haven. O pato tem curvas e textura originais mais detalhadas; muitos outros objetos continuam estilizados. A revisão não promete converter todos os modelos para fotorrealismo. Materiais URP preservam cores/texturas de origem; normais suavizadas melhoram os OBJ e o FBX mantém suas normais.

Nenhum modelo artístico, textura, sprite, áudio ou fonte foi sintetizado. Cubos, esferas e cilindros formam somente volumes arquitetônicos e detalhes permitidos. Os 326 arquivos originais são verificados por SHA-256 no manifesto.

## Voz e persistência

Nove campos de narração humana opcionais; seis versos em duas colunas e leitura ampliada. A última linha da estação agora pede um pato; as outras oito charadas não mudaram em relação à v0.8. Volumes independentes, silêncio e movimento reduzido permanecem.

A chave de save e os 54 IDs foram preservados. Encontrados, selos e passagens da v0.8 continuam válidos. O ID histórico west_bear agora corresponde ao pato para evitar quebrar saves. Reinício parcial e recuperação de backup continuam disponíveis.

## Limites de validação

175 verificações do domínio C# passaram fora do Unity. O projeto inclui 13 testes NUnit, ainda sem execução no Test Runner. Importação, preparação das cenas, Play, desempenho e build Windows continuam pendentes no editor. Leia VALIDACAO.md antes da entrega final.
