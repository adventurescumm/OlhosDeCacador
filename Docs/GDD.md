# Olhos de Caçador — GDD v0.8

Jogo 3D point-and-click para Windows x64, em Unity 6000.6.4f1, URP 17.6.0 e Input System 1.20.1. A criança transforma o quarto em faroeste, espaço e cidade de super-heróis. A referência de gênero é I Spy; a arte é composta com assets prontos, sem copiar a propriedade visual da referência.

## Experiência e fluxo

Abertura com título, Iniciar, Opções e Como jogar → quarto de seleção → caminho do mundo → busca de seis objetos → retorno e uso de uma peça → próxima busca → segundo uso de peça → terceira busca → desafio final → selo.

Há 18 cenas: Main, abertura, quarto e cinco por mundo. O controlador de sessão persiste; cada cena de conteúdo guarda sua câmera e hierarquia depois da primeira preparação no editor. Opções, pausa, caderno e leitura ampliada são painéis sobre a cena atual. A estrutura de edição está em CENAS.md.

| Mundo | Primeira passagem | Segunda passagem | Final |
| --- | --- | --- | --- |
| Faroeste de Quintal | Roda da estação na diligência, após seis descobertas | Martelo do salão na ponte da mina, após seis descobertas | Baú: 2, 4, 3 |
| Missão Caixa Lunar | Antena da base no observatório | Rádio do jardim no carrinho lunar | Sequência: Estrela, Lua, Planeta, Sol |
| Liga dos Brinquedos | Balde da praça no jardim com sede | Martelo da ponte no reparo da passagem | Balde → Praça; Martelo → Ponte; Antena → Quartel |

Cada passagem exige os seis objetos da busca de origem e a peça correta. Os mecanismos aparecem desde a primeira visita, mas não podem ser resolvidos antes desses requisitos. A peça fica representada no cenário depois do uso. O final exige os 18 objetos do mundo.

## Busca e dificuldade

São nove buscas, seis alvos em cada uma, 54 ao todo. As 614 instâncias nas buscas incluem decoração e passagens; cada tela tem de 67 a 70. Os alvos são menores que as grandes peças do ambiente e variam de posição entre as salas. Existem formas e cores semelhantes na decoração, mas não outra instância indistinguível do mesmo modelo-alvo na mesma busca.

O cursor não revela o nome de um alvo nem o amplia automaticamente. Clicar num objeto errado dá retorno, sem punição. Uma dica informa a sala; a próxima indica a região; a terceira destaca o objeto. IDs encontrados contam uma vez, inclusive durante a animação. O raycast respeita a primeira malha atingida.

O aumento de dificuldade é uma hipótese de design a testar, não uma medição. Verificar tempo até a primeira descoberta, tempo por tela, erros e uso de dicas em pessoas que não conheçam a composição.

## Direção visual

Quarto com mobiliário e brinquedos; mundos como brincadeiras montadas nesse espaço. Cores variam por sala, com tecidos, tapetes, comidinhas, carrinhos e miniaturas. Lilita One destaca títulos e botões; textos longos mantêm uma fonte legível. A luz é quente, com sombras suaves e uma luz local adicional. Superfícies curvas usam sombreamento por ângulo, sem alterar a geometria original.

Apenas cubos e esferas são criados para arquitetura, prateleiras, frisos e luzinhas. OBJ, MTL, imagens, efeitos e fonte vêm de downloads registrados no manifesto. As formas continuam estilizadas; não se promete fotorrealismo.

## Voz, acessibilidade e persistência

Nove roteiros humanos opcionais, agora com seis versos. Texto sempre disponível, leitura ampliada, volumes independentes, silêncio e movimento reduzido. Os desafios de memória têm nomes e sinais visuais, permitindo jogar sem áudio. A narração para ao mudar de cena.

A chave de save permanece OlhosDeCacador.v1. Descobertas antigas conhecidas são preservadas; conclusões incompatíveis com os novos 18 objetos são filtradas. A nova lista opened guarda passagens. Reinício parcial afeta só o mundo escolhido; há cópia de recuperação para save danificado.

## Limites da entrega

Sem executável Windows, teste de desempenho ou execução do Unity neste ambiente. A preparação das cenas, os controles em Play e a apresentação URP precisam ser confirmados no editor. Documentação: VALIDACAO.md, NARRACAO.md, CENAS.md e ATUALIZACAO_V08.md.
