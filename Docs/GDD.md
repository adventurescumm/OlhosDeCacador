# Bem-Te-Vi — GDD 0.12

Point-and-click 3D de observação para Windows, ambientado num quarto infantil que contém três mundos de faz de conta. Os cenários usam objetos domésticos, comidinhas, bonecos, blocos e trens como arquitetura e habitantes em miniatura. A referência de composição é I Spy Fantasy; seus assets não fazem parte do projeto.

## Fluxo

Título → cadernos de jogadores → quarto → maquete → circulação livre entre três buscas e um mecanismo final. A entrada tem uma saída para o quarto. Cada mundo forma um circuito com retornos e sem depender da conclusão da sala anterior. Portas, pontes e trilhos com marcas no chão identificam os caminhos; não há menu de níveis sobre as cenas.

São três mundos e três rodadas em cada um. Cada busca apresenta seis versos e seis alvos ativos entre 18 objetos identificados. Todos os objetos estão presentes desde o início. As rodadas mudam os alvos e os versos; os encontrados continuam visíveis. Existem 162 descobertas únicas e nove selos.

## Dificuldade e puzzles

| Mundo | Espaço e mecanismo | Progressão |
| --- | --- | --- |
| Faroeste | Estação, salão e mina em baú; combinação de três garrafinhas num baú de madeira | Números diretos → pequenas operações → relações numéricas escritas em palavras |
| Espaço | Base, jardim lunar e oficina; sequência distribuída por objetos da sala | Quatro, cinco e seis sinais |
| Super-heróis | Praça, ponte e quartel; associação de ferramentas nas miniaturas de um comunicador | Relações diretas → mudança de função dos lugares → descrições de ações |

As rodadas finais usam descrições menos literais dos brinquedos. Dificuldade real, legibilidade e duração ainda exigem playtest. Mais objetos e textos indiretos não comprovam equilíbrio por si só.

Seis mecanismos opcionais utilizam peças descobertas sem consumi-las e animam o cenário. O mecanismo final de cada rodada requer as 18 descobertas daquele mundo para conceder o selo. É possível entrar, tentar e sair antes disso.

## Resposta e apresentação

A descoberta produz uma reação curta e boba — salto, giro, cambalhota, órbita ou deformação — com retorno exato à pose de repouso. A opção de movimento reduzido suprime essas reações. A câmera não se desloca para denunciar alvos.

Não há dicas progressivas, destaque de alvo, coleta assistida ou pop-up de descoberta. Os versos mudam de cor e o narrador confirma o item. Gravações próprias têm prioridade; no Windows, a voz instalada serve como fallback. As charadas e informações descobertas dos mecanismos continuam disponíveis para leitura.

As bases têm camadas de madeira e tinta, painéis de cartão e bordas visíveis no enquadramento. As três miniaturas do quarto ficam agrupadas. A faixa inferior contém apenas a charada; Menu reúne os controles.

## Persistência e limites

Cada jogador tem um caderno automático com rodada, sala, descobertas, mecanismos e estado estável dos puzzles. Há gravação atômica e backup. Tentativas da sequência musical recomeçam ao entrar; não se perde a aventura. Veja SALVAMENTO.md.

O projeto contém fontes e assets; não inclui um executável compilado. Conteúdo e domínio foram verificados fora do Unity. Voz, renderização, testes NUnit e build no Unity permanecem pendentes; consulte VALIDACAO.md.
