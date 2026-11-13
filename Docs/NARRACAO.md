# Sua voz em Olhos de Caçador — v0.8

Atualizado em 13/11/2026 para Unity 6000.6.4f1 e Windows. Há **nove charadas, cada uma com seis versos e seis objetos**. Os nove IDs de sala continuam iguais, mas os textos foram ampliados: gravações da v0.7, com apenas dois alvos, precisam ser refeitas. O pacote não contém voz gerada nem gravações de narrador.

## Onde a narração aparece

A abertura mostra Iniciar, Opções e Como jogar sobre o quarto. Iniciar leva à seleção: baú à esquerda, nave sobre a mesa e heroína à direita. Cada mundo começa numa cena de caminho, que conecta suas três buscas e o desafio final. Consulte [CENAS.md](CENAS.md) para os nomes das cenas.

A voz gravada é usada **somente nas nove buscas**. No caminho, no quarto, nas passagens e nos desafios, as instruções são texto e podem ser lidas ao vivo. Essas cenas não acrescentam campos ao banco de narração.

Durante uma busca, os versos ficam na faixa inferior, em duas colunas de três linhas. Leia a primeira coluna de cima para baixo e depois a segunda. Os botões **Dica 1 a Dica 6** correspondem aos alvos na ordem dos versos. O painel **Voz da aventura**, à direita, mantém Ler juntos e o controle de áudio.

## Gravar e integrar

1. Grave um arquivo por charada, lendo os seis versos do bloco correspondente. Não leia o ID. Faça pausas curtas entre versos e deixe os nomes dos objetos claros.
2. Fora do Play, importe seus WAV ou OGG em `Assets` e aguarde o Unity. Não é necessário regravar os sete efeitos prontos.
3. Selecione `Assets/Resources/Hunter/Narration/NarrationBank.asset` na janela Project.
4. Expanda `Readings`. Cada item contém `Room Id`, `Recording Script` e `Clip`. Mantenha os nove IDs.
5. Arraste a gravação nova para o `Clip` correspondente.
6. Execute **Olhos de Cacador > 2. Verificar conteudo**. O comando compara os roteiros com o catálogo e conta as gravações disponíveis; ele não escuta o conteúdo dos clipes.
7. Dê Play por `Assets/Hunter/Scenes/Main.unity`, entre no mundo e visite a busca. Teste **Ouvir charada**.

**Alternativa por nome:** com Clip vazio, o banco procura `Assets/Resources/Hunter/Narration/<ID>.wav` ou `.ogg`, por exemplo `faroeste_estacao.wav`. Use apenas um arquivo com esse nome-base. Um Clip atribuído explicitamente tem prioridade. Importar um arquivo com o nome do caminho ou do desafio não cria uma fala nova nessas cenas.

O texto de jogo vem de `Assets/Resources/Hunter/catalog.json`; o Inspector mostra `Recording Script`. Se personalizar a charada, mantenha os dois textos e este guia sincronizados, com **seis linhas**, e atualize o áudio. Editar este Markdown sozinho não altera a fala nem o texto no jogo.

## Controles e interrupções

| Situação | Comportamento |
| --- | --- |
| Ler juntos | Sempre disponível. Interrompe o áudio e abre os seis versos maiores; Voltar e procurar ou Esc retorna à busca. |
| Ouvir charada | Toca o clipe da sala desde o começo. |
| Parar leitura | Interrompe; ao ouvir novamente, começa do início, sem retomar o ponto anterior. |
| Sem gravação | Botão desabilitado quando não existe clipe; Ler juntos continua disponível. |
| Encontrar um objeto | A voz continua; o efeito de descoberta pode tocar junto. |
| Voltar ao Caminho, mudar de cena, abrir pausa/caderno/leitura ou desafio | A fala é interrompida. Não continua tocando na cena seguinte. |
| Alt+Tab ou perda de foco | Interrompe a voz; voltar à janela não a reinicia sozinho. |

As gravações não se sobrepõem. A voz é opcional e não é necessária para resolver as passagens ou os desafios.

### Opções e leitura ao vivo

Abra Opções na abertura/quarto, ou **Pausa > Opções de áudio e movimento** durante a aventura. **Sons dos brinquedos** e **Voz do narrador** têm volumes independentes; o silêncio geral afeta ambos. As preferências são guardadas.

**Leitura automática** começa desligada numa instalação sem preferências anteriores. Quando ativada, toca ao entrar numa busca e ao retornar à busca após pausa, caderno ou Ler juntos, se houver clipe e volume. Não toca no caminho nem no quarto. Voltar de um desafio agora leva ao caminho; a próxima busca visitada terá sua própria leitura. Objetos já encontrados não impedem a repetição da charada.

Para narrar ao vivo enquanto alguém procura, use os versos na faixa inferior. Ler juntos ocupa a tela e precisa ser fechado para retomar os cliques no cenário. Desative Leitura automática ou deixe somente Voz do narrador em zero para evitar fala gravada junto da sua. Movimento reduzido não altera os versos nem o áudio.

## Migrar gravações anteriores

Guarde os áudios antigos e seus `.meta` numa cópia do projeto. Os GUIDs preservam referências, mas **não tornam uma gravação antiga compatível com os seis alvos novos**. Regrave os nove textos abaixo e atribua os novos clipes ao banco da v0.8. Se mantiver o mesmo arquivo para preservar um vínculo, substitua também seu conteúdo de áudio pela leitura nova.

Não substitua o banco e o catálogo atuais por cópias antigas: isso restauraria roteiros de quatro versos e removeria dados dos caminhos. Para personalizações, combine os novos textos e campos conscientemente. Veja [ATUALIZACAO_V08.md](ATUALIZACAO_V08.md).

Os sete AudioImporters corrigidos pertencem aos efeitos originais. `Tools/repair_importer_metadata.py` atua nos assets do manifesto, não em gravações particulares. Importe sua voz normalmente pelo Unity, conservando os metadados que ele gerar.

## Roteiros completos

### Estação do Quintal

ID: `faroeste_estacao`

> Um balde quer água levar,  
> uma garrafinha veio passear.  
> Uma roda pede outra viagem,  
> uma maçã se esconde na bagagem.  
> Rosquinha com confeitos eu vou procurar,  
> e um biscoito de gente quer me acompanhar.  

### Salão do Suco

ID: `faroeste_salao`

> Um martelo quer ajudar,  
> um peixe sem rio veio brincar.  
> Xícara de chá para a visita,  
> biscoito com chocolate na pista.  
> Uma banana curva eu quero achar,  
> e livros empilhados para imaginar.  

### Mina dos Brilhos

ID: `faroeste_mina`

> Uma picareta quer trabalhar,  
> uma garrafa grandona vou encontrar.  
> Moeda dourada vale um tesouro,  
> cogumelo de mentira não é ouro.  
> Cristais juntinhos brilham no caminho,  
> e uma cenoura espera num cantinho.  

### Base de Papelão

ID: `espaco_base`

> Um astronauta de bolso vai viajar,  
> uma antena curiosa quer escutar.  
> Carrinho de corrida perdeu a direção,  
> um mouse apareceu nesta missão.  
> Pirulito redondo parece um planeta,  
> e um abajur redondo ilumina a descoberta.  

### Jardim de Meteoros

ID: `espaco_jardim`

> Um visitante de outro lugar,  
> um meteoro com marcas veio pousar.  
> Um rádio de brinquedo guarda o sinal,  
> uma laranja parece um sol especial.  
> Berinjela virou nave na imaginação,  
> e cristais juntinhos completam a missão.  

### Garagem das Naves

ID: `espaco_garagem`

> Uma nave pequenina vai partir,  
> um segundo tripulante quer se divertir.  
> Uma rodinha solta precisa voltar,  
> almofada azul é boa para descansar.  
> Duas cerejas juntinhas fazem par,  
> e sorvete no copinho eu quero encontrar.  

### Praça dos Blocos

ID: `herois_praca`

> A Capitã Faísca está de prontidão,  
> um balde leva água para a missão.  
> Caminhão de brinquedo veio trabalhar,  
> melancia de mentira quer participar.  
> Livros empilhados guardam uma aventura,  
> e um bolinho de festa completa a procura.  

### Ponte do Faz de Conta

ID: `herois_ponte`

> Um martelo pequeno pode consertar,  
> o Robô Violeta veio ajudar.  
> Biscoito de gente observa a ponte,  
> uma pera se esconde no horizonte.  
> Um cone de trânsito pede atenção,  
> e caneca com alça espera uma mão.  

### Quartel das Boas Ideias

ID: `herois_quartel`

> Um comunicador quer ser encontrado,  
> um ajudante amarelinho está ao lado.  
> Carro de corrida chegou à reunião,  
> biscoito de gente entrou na missão.  
> Um picolé no palito quer participar,  
> e abajur quadrado ilumina o lugar.  

## Conferir antes de distribuir

Teste os nove clipes em Play e no executável Windows: correspondência com os seis versos, Ouvir/Parar/repetir, interrupção ao trocar de cena, leitura automática, silêncio e equilíbrio com os efeitos. Confira também uma sala sem clipe, Ler juntos e a faixa inferior em 1280 × 720.

Depois de importar as gravações, gere um novo build; um executável anterior não passa a conter os áudios sozinho. Os roteiros deste arquivo foram comparados com o banco e o catálogo. Reprodução e transições de voz ainda precisam ser confirmadas no Unity, conforme [VALIDACAO.md](VALIDACAO.md).
