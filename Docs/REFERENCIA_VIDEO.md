# I Spy Fantasy: referência e adaptação v0.8

Referência: vídeo enviado pelo usuário (`videoplayback.mp4`), revisto por quadros da abertura/quarto, entrada do castelo, oficina de busca e sequências de interação. O vídeo é referência de estudo; não faz parte dos assets do projeto. Nenhuma malha, textura, imagem, música ou fala de I Spy foi incorporada.

A revisão anterior deixou as buscas fáceis e as telas com aparência de caixas pouco variadas. Esta revisão muda a escala da procura, o fluxo e o ambiente.

| Observação na referência / pedido | Implementação atual |
| --- | --- |
| Quarto cotidiano abre mundos imaginários | Abertura e seleção usam um quarto com cama, mesa, estante, tapete, almofadas e carrinhos; baú, nave e heroína ficam afastados |
| A abertura precisa ser clara | Título e três botões: Iniciar, Opções, Como jogar |
| Muitos objetos competem pela atenção | 67–70 instâncias por busca, com distratores, escalas menores e distribuição diferente dos alvos em cada tela |
| Lista maior e observação real | Seis alvos por busca, 54 no total; os alvos não revelam nome nem aumentam ao passar o mouse; ajuda explícita em três etapas |
| Brinquedos chamativos têm utilidade futura | Cada caminho apresenta duas peças/mecanismos desde a primeira visita; a solução exige voltar com itens encontrados e completar a busca anterior |
| Navegação conecta as brincadeiras | Três cenas de caminho, nove cenas de busca e três desafios finais; sessões persistentes entre carregamentos |
| Objetos devem parecer brinquedos | Packs prontos de mobiliário, carrinhos e comidinhas; materiais originais, iluminação quente e normais suavizadas por ângulo |
| Mais cor e personalidade | Nove paletas, botões coloridos, fonte Lilita One e detalhes de festa feitos com primitivas |
| Charada acompanha a busca | Faixa de seis versos em duas colunas, seis dicas e espaço de leitura/narração humana |

## O que continua diferente

A composição é original e usa modelos estilizados disponíveis nos packs, sem reconstruir a estética fotográfica de I Spy ou modelar objetos novos. O cálculo de normais suaviza a iluminação, mas não adiciona polígonos às silhuetas. Não há narração produzida por IA nem animação modelada de abrir a tampa do baú.

As seis passagens são associações de uma peça com um brinquedo; os desafios finais continuam sendo combinação, memória e associação. A dificuldade foi aumentada pela quantidade, distribuição e semelhança entre objetos; o equilíbrio real ainda depende de playtest.

## Conferência

Os enquadramentos e a visibilidade são conferidos externamente com as malhas originais e projeção geométrica; prévias em Blender não são capturas de uma execução Unity. As cenas `.unity` separadas precisam ser populadas pela primeira preparação no editor. Veja [CENAS.md](CENAS.md) e [VALIDACAO.md](VALIDACAO.md).
