# Atualizar para a v0.8

A v0.8 amplia as buscas para **seis alvos por tela**, acrescenta caminhos com uso de peças e separa a aventura em **18 cenas**. Ela mantém Unity 6000.6.4f1, Windows x64, URP 17.6.0 e Input System 1.20.1 (New).

## Instalação recomendada

1. Feche o editor e guarde sua cópia anterior, incluindo cenas próprias, gravações e metadados.
2. Extraia o ZIP em uma pasta nova e adicione essa pasta ao Unity Hub.
3. Abra com a versão indicada; espere a resolução dos pacotes e a compilação.
4. Abra `Assets/BemTeVi/Scenes/Main.unity` e execute **1. Preparar projeto**. Na primeira vez, o comando também popula e salva as 17 cenas de conteúdo, organizando suas hierarquias. As configurações de materiais geradas nessa etapa são apenas para as primitivas das salas.
5. Execute **2. Verificar conteudo**, limpe o Console e teste pelo Main. Para editar as cenas, siga [CENAS.md](CENAS.md).

O ambiente que preparou o ZIP não possui Unity. A montagem das hierarquias, importação e compilação do projeto completo ainda precisam ser executadas aí. As cenas e os caminhos do build já estão incluídos no pacote; o runtime tem uma montagem por catálogo enquanto a preparação não tiver sido feita.

## Progresso anterior

A chave local permanece `BemTeVi.v1`. Os 18 IDs antigos continuam válidos e suas descobertas são mantidas. Foram acrescentados 36 alvos; um mundo que antes tinha seis objetos agora exige 18. Por isso, selos antigos sem todos os novos objetos são retirados da lista de conclusões ao carregar; os objetos encontrados não são apagados.

O save ganha uma lista `opened` para as seis passagens. Saves antigos começam com essa lista vazia. Uma passagem exige a peça correta e todos os objetos da busca de origem. Reiniciar um mundo limpa somente seus objetos, selo e passagens.

## Gravações e alterações próprias

Os nove IDs de sala permanecem iguais, mas as charadas agora têm seis versos. As leituras antigas de quatro versos precisam ser refeitas. Preserve os arquivos para referência; não substitua o banco atualizado por um banco antigo sem adaptar os textos. [NARRACAO.md](NARRACAO.md) contém os nove roteiros atuais.

Para uma cópia com código/cenas personalizados, compare toda a pasta Runtime, os scripts Editor, o catálogo, NarrationBank, ProjectSettings e os novos modelos, materiais, texturas, fonte e licenças. Copiar apenas BemTeViGame.cs é insuficiente: o controlador está dividido em arquivos parciais e depende de BemTeViStage, BemTeViSceneProp e BemTeViSceneBuilder. Os `.meta` acompanham os arquivos; não regenere GUIDs.

Após preparar as cenas, alterações de posição feitas nelas são preservadas por Preparar projeto. O comando **7. Recriar cenas a partir do catalogo** é explícito e substitui as composições editadas; mantenha backup antes de usá-lo.

## Arte e importação

Os arquivos artísticos são prontos: Kenney Furniture Kit, Toy Car Kit, Food Kit e os packs anteriores; Lilita One vem de Google Fonts. OBJ, MTL, PNG, OGG e TTF têm hashes de origem no manifesto. Nenhuma arte ou voz foi sintetizada. O sombreamento por ângulo de 75° e o filtro bilinear dos novos packs são configurações de importação, sem modelar novas malhas ou pintar texturas.

Os 141 ModelImporters e sete AudioImporters têm blocos completos; os 19 PNGs têm importação explicitamente 2D. A revisão preserva a ausência de reparos automáticos em callbacks. Se aparecerem erros, registre a primeira mensagem nova após a importação; não apague os `.meta` nem use Reimport All repetidamente. O reparador do pacote considera somente os arquivos do manifesto.
