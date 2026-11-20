# Revisão 0.14 — recomposição a partir do vídeo local

> Registro histórico: as divergências de hashes mencionadas abaixo foram resolvidas na [revisão 0.15](ATUALIZACAO_V15.md), sem trocar os hashes esperados.

A referência desta revisão é `videoplayback.mp4`, fornecido pelo usuário. O arquivo tem **2h44min18s**, imagem de 640 × 360 e cerca de 356 MB. Foram analisados 109 quadros distribuídos pelo vídeo, além de quadros específicos da abertura, quarto, salão de passagem e buscas em close. Foi uma análise visual por amostragem, sem escuta integral das falas.

A revisão anterior, com seis bancadas repetidas e ilhas ligadas por fitas, foi substituída por ambientes contínuos. O ponto central observado no vídeo foi a passagem de uma sala para outra por portas, escadas e objetos, alternando buscas amplas e closes.

## Espaços

| Mundo | Transição | Buscas | Desafio |
| --- | --- | --- | --- |
| Faroeste | Salão dos Caminhos, com escada, porta e baú | Estação sobre livro, salão de bonecas com mesa/prateleiras, mina dentro de baú aberto | Combinação de garrafinhas no baú em close |
| Espaço | Laboratório das Passagens, com escotilha e hangares | Base com bancada, jardim de brinquedos gigantes, garagem em caixa de sapatos | Memória na sala inteira, com rádio e móbile de objetos |
| Heróis | Rua dos Brinquedos, com construções e ponte | Praça sobre livro, rio de fita com barco e ponte, armário de brinquedos | Associação sobre um único comunicador de brinquedo |

As buscas ocupam áreas de 18,8 × 13 unidades, com câmeras próprias. Os alvos foram redistribuídos em mesas, pisos e prateleiras. Extras pequenos têm espaçamento e distribuição determinística; cada mundo mantém 12, 15 ou 18 por sala, respectivamente. As três rodadas mantêm seus alvos e charadas, totalizando 162 descobertas.

## Caminhos e resposta

Cada sala de transição conecta as três buscas, o desafio e o quarto. Cada busca também leva às outras duas e retorna diretamente aos caminhos. As placas são objetos físicos clicáveis junto às entradas; as etiquetas flutuantes de destinos foram removidas. O botão **Mapa** oferece um retorno adicional. Nenhuma passagem depende de peça, alvo ou sala concluída.

As trocas têm um fade curto, suprimido com movimento reduzido. As três entradas do quarto formam um grupo compacto em alturas diferentes, com a nave sobre a mesa e os outros brinquedos no chão. Essa disposição segue a relação espacial do quarto do vídeo, sem afirmar igualdade em pixels.

Os itens encontrados permanecem no lugar e não contam novamente. Seus versos mudam de cor, sem marcador textual adicional, dica ou mensagem de descoberta. A voz confirma o nome do item; gravações atribuídas ao NarrationBank continuam tendo prioridade. Alguns brinquedos grandes balançam ou flutuam e reagem com giros, cambalhotas ou deformações.

## Modelos

Foram importados **nove modelos CC0**, escolhidos de [Pirate Kit](https://kenney.nl/assets/pirate-kit) e [Castle Kit](https://kenney.nl/assets/castle-kit), publicados por Kenney também no OpenGameArt. Barco, navio, canhão de brinquedo, bandeiras, torre, telhado, porta e escada somam **253.665 bytes** com MTL, dois atlas e licenças. Os pacotes completos e o vídeo ficam fora dos arquivos distribuídos do projeto.

`Tools/import_reference_models.py` reproduz a seleção a partir dos ZIP em `Temp/VideoReferenceV14`. `Tools/prepare_materials.py --pack <pacote>` prepara somente esses materiais. `Tools/recompose_v14.py` recompõe o catálogo sem mudar IDs de descobertas ou salas; depois é preciso reconstruir as cenas no Unity.

## Conferência

As prévias em `Docs/Previews/v14` são renderizações Unity sem a interface de execução. O menu **Bem-Te-Vi > 8. Conferir enquadramentos e gerar previas** verifica por câmera e raycast os 162 alvos e as 54 passagens, incluindo as três entradas do quarto. Colisores da arquitetura permitem detectar oclusões; as placas também são verificadas.

As regras, saves e progressão passam por 657 verificações C#. Os testes EditMode cobrem permanência dos objetos e compatibilidade dos assets. Os resultados finais estão em [VALIDACAO.md](VALIDACAO.md). Voz, dificuldade percebida e uma partida completa ainda precisam de avaliação manual. Não foi produzido executável Windows.

A auditoria Python mantém 202 divergências de hash em OBJ/MTL existentes antes desta tarefa. Esses arquivos foram preservados. Os novos arquivos são conferidos contra os ZIP; hashes antigos não foram substituídos para ocultar divergências.
