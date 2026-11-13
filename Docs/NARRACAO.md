# Sua voz em Olhos de Caçador

Guia atualizado para a **v0.7**, em 13/11/2026. Projeto para **Windows**, com **Unity 6000.6.4f1**, URP e Input System Package (New).

O jogo tem **nove charadas, uma por tela de busca**, com dois objetos para encontrar em cada tela. Os espaços estão preparados para **suas gravações humanas**; o pacote continua sem clipes de narração. Você também pode ler ao vivo enquanto outra pessoa procura, usando a faixa inferior ou o botão **Ler juntos**.

## O que mudou com a revisão visual

A abertura leva ao **Quarto das Brincadeiras**, onde os próprios brinquedos dão acesso aos mundos. Nas buscas, o cenário ocupa a área central, os quatro versos aparecem em duas colunas na faixa inferior e o painel **Voz da aventura** fica à direita. As placas do cenário e seus rótulos permitem trocar de cantinho.

| Mundo | Entrada no quarto | Acesso ao desafio após encontrar os seis objetos |
| --- | --- | --- |
| Faroeste de Quintal | Baú | Baú do piquenique, na Mina dos Brilhos |
| Missão Caixa Lunar | Nave | Rádio da nave, na Garagem das Naves |
| Liga dos Brinquedos | Heroína | Missão da maquete, no Quartel das Boas Ideias |

O quarto e os três desafios 3D somam quatro composições às nove buscas, mas **não acrescentam gravações ao banco**. Os nove IDs e roteiros abaixo foram preservados; gravações compatíveis da versão anterior podem ser reutilizadas.

Atualmente, a abertura, o quarto, as instruções dos desafios, as dicas e as mensagens de descoberta não têm campos próprios de voz nem reprodução automática pelo banco. Você pode lê-las ao vivo. Adicionar arquivos para essas telas, por si só, não cria novas falas no jogo. Os efeitos dos brinquedos e os sinais do puzzle espacial usam os áudios prontos já incluídos.

## Integrar as gravações

1. Grave um arquivo por charada. WAV ou OGG são adequados. Comece diretamente na leitura, com uma pequena pausa entre versos; evite música que atrapalhe a compreensão.
2. Fora do Play Mode, importe os áudios para uma pasta em `Assets` e aguarde o Unity terminar a importação. Não é necessário regravar os efeitos prontos.
3. Na janela Project, selecione `Assets/Resources/Hunter/Narration/NarrationBank.asset`.
4. Expanda `Readings` no Inspector. Cada item já traz `Room Id` e `Recording Script`.
5. Arraste a gravação correspondente para o campo `Clip`. Mantenha os IDs existentes.
6. Execute **Olhos de Cacador > 2. Verificar conteudo**. O verificador confere os nove roteiros e informa quantas salas têm uma gravação disponível; não avalia o conteúdo falado do áudio.
7. Abra `Assets/Hunter/Scenes/Main.unity`, dê Play, passe pela abertura e escolha um brinquedo no quarto. Na sala correspondente, clique em **Ouvir charada**.

**Alternativa sem atribuição manual:** coloque o arquivo em `Assets/Resources/Hunter/Narration/`, com o nome exato do ID listado abaixo, por exemplo `faroeste_estacao.wav`. O banco procura esse recurso automaticamente quando o campo Clip está vazio. Não coloque dois arquivos de extensões diferentes com o mesmo nome nesse caminho. Uma atribuição explícita em Clip tem prioridade sobre o nome automático.

O campo `Recording Script` serve como roteiro no Inspector. O texto mostrado no jogo vem de `Assets/Resources/Hunter/catalog.json`. Se mudar uma charada, atualize os dois textos, este guia e a gravação correspondente. Preserve os **quatro versos separados por quebras de linha**, usados pelas duas colunas da interface. `2. Verificar conteudo` detecta divergências entre o banco e o catálogo. Editar somente este Markdown não altera o jogo; não altere um ID para corrigir o nome de uma sala.

## Durante a brincadeira

| Controle ou situação | Comportamento atual |
| --- | --- |
| **Ler juntos** | Está sempre disponível, com ou sem gravação. Interrompe a voz gravada e abre os quatro versos em letras maiores. Use **Voltar e procurar** ou Esc para retornar à busca. |
| **Ouvir charada** | Disponível quando a sala tem um clipe. Reproduz a gravação do começo. |
| **Parar leitura** | Substitui Ouvir charada enquanto a voz toca. Interrompe a reprodução; o próximo clique em Ouvir reinicia do começo, sem retomar o ponto anterior. |
| **Sem gravação** | Aparece desabilitado quando não há clipe para a sala. Ler juntos e o texto na faixa inferior continuam disponíveis. |
| Trocar de cantinho pelas placas | Interrompe a fala anterior e passa a usar a charada da sala de destino. |
| Encontrar um objeto | Mostra a reação e a descoberta, sem interromper ou reiniciar a charada em andamento. Os efeitos podem tocar junto da voz. |
| Abrir pausa, caderno, leitura ampliada ou desafio; voltar ao quarto | Interrompe a fala. O desafio usa suas próprias instruções de texto e efeitos. |
| Perder o foco da janela, por exemplo com Alt+Tab | Interrompe a fala. Voltar à janela não a reinicia automaticamente; use Ouvir charada. |

As gravações de voz não se sobrepõem. A narração é opcional e os desafios continuam funcionando sem ela.

### Volume e leitura automática

Abra **Opções** na tela inicial ou no quarto. Durante a busca, use **Pausa > Opções de áudio e movimento**. A tela permite ajustar **Sons dos brinquedos** e **Voz do narrador** separadamente; desligar o áudio geral silencia os dois. Se os efeitos atrapalharem a leitura, reduza apenas o volume deles.

**Leitura automática** começa desligada numa instalação sem preferências salvas. Quando ativada, lê ao entrar em uma sala e ao retornar à busca, desde que haja uma gravação, o áudio geral esteja ligado e o volume de voz seja maior que zero. Isso inclui voltar do caderno, da pausa, de Ler juntos e do desafio: a leitura começa novamente do início, mesmo se os objetos já foram encontrados. Na primeira visita a um mundo, a apresentação inicial é fechada antes de continuar a leitura na busca.

As preferências de volume, silêncio e leitura automática são guardadas. **Movimento reduzido** altera as animações e o foco da câmera, sem mudar o texto ou a reprodução da voz.

### Ler ao vivo

Para narrar enquanto outra pessoa procura, leia a faixa inferior da própria busca. **Ler juntos** é útil para uma leitura ampliada antes de voltar a procurar; seu painel ocupa a tela e precisa ser fechado para retomar a interação com os objetos. Desative Leitura automática se não quiser que a gravação recomece ao fechar esse painel. Você pode deixar somente Voz do narrador em zero para manter os efeitos durante a leitura ao vivo.

## Preservar suas gravações ao atualizar

1. Feche o Unity e guarde uma cópia do projeto que contém suas gravações.
2. Extraia a revisão atual em outra pasta. Copie os arquivos de áudio e seus `.meta`, mantendo os caminhos relativos dentro de `Assets`.
3. Se você preencheu campos `Clip` ou personalizou os roteiros, copie também seu `NarrationBank.asset` e seu `.meta` para o mesmo caminho. Confira os nove IDs e textos antes de substituir o banco novo. Se usava apenas a convenção automática de nomes e não alterou o banco, basta preservar os áudios e seus `.meta`.
4. Mantenha o **catálogo da v0.7**, que contém quarto, passagens, câmeras e desafios. Se personalizou versos no catálogo antigo, combine apenas essas alterações de texto com o novo arquivo e atualize os roteiros correspondentes. Não substitua o catálogo inteiro pela versão antiga.
5. Abra com Unity 6000.6.4f1, aguarde a importação e execute **1. Preparar projeto** e **2. Verificar conteudo**. Teste as nove salas antes de gerar o executável Windows.

Os GUIDs dos `.meta` preservam os vínculos dos campos Clip. Não substitua seu banco preenchido pelo banco vazio do ZIP nem apague metadados para tentar corrigir a narração.

A correção de AudioImporter da v0.7 abrange os **sete efeitos originais**, não nove narrações: estas ainda serão fornecidas por você. O reparador `Tools/repair_importer_metadata.py` atua apenas nos assets do manifesto e **não repara gravações pessoais**. Importe novos áudios normalmente pelo Unity; para os avisos de importação do pacote e a atualização completa, consulte [CORRECAO_IMPORTADORES.md](CORRECAO_IMPORTADORES.md).

## Roteiro completo

Leia os quatro versos de cada bloco; o ID é apenas o identificador do arquivo e não precisa ser falado.

### Estação do Quintal

ID: `faroeste_estacao`

> Na estação, a aventura já vai começar.  
> Procuro um balde para água levar.  
> Uma garrafinha resolveu se esconder.  
> Olhos de caçador, quem consegue ver?  

### Salão do Suco

ID: `faroeste_salao`

> No salão do suco, há uma festa no ar.  
> Um martelo de brinquedo veio ajudar.  
> E um peixe sem rio, que coisa engraçada!  
> Encontre os dois para seguir a jornada.  

### Mina dos Brilhos

ID: `faroeste_mina`

> Na mina dos brilhos, vamos procurar:  
> Uma picareta pronta para trabalhar.  
> Uma garrafa grandona espera no chão.  
> Dois novos tesouros para a coleção!  

### Base de Papelão

ID: `espaco_base`

> Nossa base de papelão já pode decolar.  
> Um astronauta de bolso quer viajar.  
> Uma antena curiosa escuta a imensidão.  
> Ache os dois para começar a missão!  

### Jardim de Meteoros

ID: `espaco_jardim`

> No jardim de meteoros, quem veio passear?  
> Um visitante simpático de outro lugar.  
> Um meteoro cheio de marcas caiu por aqui.  
> Procure bem os dois: será que estão ali?  

### Garagem das Naves

ID: `espaco_garagem`

> Na garagem das naves, já é hora de partir.  
> Uma nave pequenininha quer se divertir.  
> O segundo tripulante espera a contagem.  
> Encontre os dois e prepare a viagem!  

### Praça dos Blocos

ID: `herois_praca`

> Na praça dos blocos, que grande animação!  
> A Capitã Faísca está de prontidão.  
> Um balde leva água para a flor crescer.  
> Ache os dois: ajudar é um superpoder!  

### Ponte do Faz de Conta

ID: `herois_ponte`

> Na ponte de brinquedo, a turma vai passar.  
> Um martelo bem pequeno vem para consertar.  
> O Robô Violeta quer dar uma mão.  
> Encontre os dois e complete a missão!  

### Quartel das Boas Ideias

ID: `herois_quartel`

> No quartel das ideias, já toca o chamado.  
> Um comunicador espera ser encontrado.  
> Um ajudante amarelinho quer participar.  
> Ache os dois: a cidade vamos ajudar!  

## Conferir antes de publicar

1. Visite os três mundos pelo quarto e as nove salas pelas placas. Confirme que cada gravação corresponde ao nome e aos quatro versos da sala atual.
2. Teste Ouvir, Parar e Ouvir novamente: a repetição deve começar do início. Confira também Ler juntos com e sem clipe associado.
3. Durante a fala, encontre um objeto, troque de sala, abra Caderno/Pausa e alterne para outra janela. Compare as interrupções com a tabela acima.
4. Ative Leitura automática e teste a entrada na sala e o retorno de Ler juntos e do desafio. Repita com voz em zero e com áudio geral desligado.
5. Confira o texto e os botões em 1280 × 720, o equilíbrio de voz/efeitos e a preferência de movimento reduzido.
6. Depois de importar e vincular os clipes, gere um **novo build Windows** e repita os testes no executável, fora do editor. Um executável gerado antes da inclusão das gravações não passa a contê-las automaticamente.

O verificador de conteúdo não escuta os arquivos nem comprova a reprodução. Este guia foi conferido com o código e os nove roteiros da v0.7; a reprodução em Play e no Windows ainda precisa ser validada no Unity. Veja também o roteiro completo em [VALIDACAO.md](VALIDACAO.md).
