# Cadernos de jogadores

A abertura pede um nome e mostra os cadernos existentes. Selecionar um nome recupera sua última sala e rodada. Perfis usam IDs próprios: duas pessoas nunca compartilham o mesmo progresso. Os nomes devem ter 1–24 caracteres e não se repetem, ignorando maiúsculas e espaços das pontas.

Cada caderno registra descobertas de todas as rodadas, nove selos possíveis, mecanismos acionados, última sala, números da combinação e ferramentas já associadas. A sequência de memória reinicia a tentativa ao reabrir o desafio; o progresso da aventura permanece. A posição de cada brinquedo vem da composição da cena: encontrar um objeto não o remove nem salva deslocamentos de animação.

O arquivo `Saves/players.json` fica dentro de `Application.persistentDataPath`. Os dados não são enviados a servidores. As preferências de áudio e movimento são globais e ficam em PlayerPrefs. A tela de jogadores mostra descobertas e selos por nome.

## Integridade

O repositório de saves serializa um envelope JSON com versão e checksum SHA-256. Escreve e descarrega `players.json.tmp`, lê e valida a cópia e só então substitui o arquivo principal. A cópia anterior válida é `players.json.bak`. Um temporário interrompido é ignorado na leitura. Não é um sistema antitrapaça; o checksum identifica truncamento e alterações acidentais.

Ao falhar a leitura do principal, o backup válido é usado e a interface informa a recuperação. Um principal corrupto é preservado com sufixo `.corrupt-<instante>` antes da próxima escrita. Se ambas as cópias falharem, é possível começar um caderno novo sem apresentar o progresso anterior como recuperado. Falhas de gravação aparecem como aviso e o progresso fica disponível naquela sessão.

## Migração e troca de nome

Na ausência de cadernos novos, são lidas as chaves antigas de PlayerPrefs, incluindo backup. No Windows, a migração também tenta as localizações antigas do produto no registro do usuário, em modo somente leitura. Cria o perfil “Minha aventura”. Uma conclusão antiga de mundo corresponde à primeira rodada completa, desde que seus 18 objetos estejam presentes. IDs desconhecidos, duplicatas e conclusões impossíveis são descartados.

Os identificadores antigos aparecem somente no código de migração. Não são nomes exibidos no jogo. O acesso ao registro é opcional: falhas não impedem abrir a aventura. Se a versão anterior já apagou suas chaves na inicialização, esses dados não podem ser reconstruídos.

## Teste manual necessário

Crie Ana e encontre um objeto; crie Bruno e confira que começa vazio. Volte a Ana, atravesse uma passagem, altere dois números do baú e feche o executável. Reabra: Ana deve retomar a sala, o verso marcado e os números. O objeto continua visível e não conta de novo. Complete uma rodada e confira a seguinte. Reinicie um mundo de Ana: Bruno e os outros mundos devem permanecer intactos.

Com o jogo fechado e uma cópia dos dados de teste, danifique somente o principal; a próxima abertura deve recuperar o backup. Confirme também o aviso de falha de gravação num diretório sem permissão, usando um perfil de teste.
