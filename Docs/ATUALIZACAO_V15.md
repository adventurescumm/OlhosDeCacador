# Revisão 0.15 — integridade dos assets e buscas camufladas

## As 202 divergências foram resolvidas

A causa era a conversão de terminações de linha durante a cópia dos modelos. Foram restaurados 150 OBJ com cabeçalhos LF e dados de malha CRLF, e 52 OBJ/MTL de móveis originalmente em LF. O ZIP de Blocky Characters obtido do autor confirmou tanto o hash do pacote registrado quanto os bytes mistos dos modelos.

Cada restauração só foi aceita depois de produzir **exatamente o SHA-256 que já estava no manifesto**. A comparação do texto normalizado garante que não mudou nenhuma coordenada, face, UV ou valor de material. Nenhum hash esperado, GUID ou arquivo de licença foi substituído. Os **445 arquivos** agora passam na auditoria.

O recibo por arquivo está em [ASSET_INTEGRITY_V15.json](ASSET_INTEGRITY_V15.json). `python Tools/repair_asset_bytes.py --apply` reproduz a recuperação; uma segunda execução estável faz zero reparos. As regras `*.obj -text` e `*.mtl -text` em `.gitattributes` preservam os bytes nos próximos checkouts.

## Charadas e composição

Foram revistos quadros do vídeo local na oficina, gaveta de objetos, composição de peixes e armário espacial. A referência esconde por mistura de formas, cores, orientações e sobreposição, em vez de deixar cada item isolado num espaço próprio.

As nove buscas agora distribuem os alvos em nichos irregulares: entre livros, sob telhados, junto a caixas, em mesas, prateleiras, recipientes e elementos da paisagem. Os extras acompanham esses grupos, com variação de escala e orientação. Objetos maiores escondem parte das silhuetas; alguns alvos planos se misturam a faces de livros ou ao cenário. Foram mantidos os 162 IDs, as falas de confirmação e as pistas dos desafios finais.

As **27 charadas** foram reescritas em seis versos de observação. Por exemplo: “Um anel coberto de pequenos confeitos” e “Um presente que esqueceu de ter quinas”. Não há indicação de onde clicar nem sistema de dicas. Os roteiros correspondentes em NarrationBank foram sincronizados sem alterar referências a clipes.

`Tools/recompose_v15.py` registra as composições. O comando Unity `BemTeViLayoutReview.BatchFit` ajusta os objetos que cobrem os alvos e reconstrói as cenas. O menu de conferência gera as novas imagens em `Docs/Previews/v15`.

## Visibilidade e testes

A verificação compara a silhueta real de cada alvo com a primeira superfície atingida pelos raios. Vazios de malhas, como o centro de uma roda, não contam como partes escondidas. A exposição é uma estimativa por amostragem, não uma contagem exata de pixels.

- 162 alvos e 54 passagens passaram na conferência de enquadramento e clique.
- 133 alvos apresentam exposição inferior a 94% na amostragem. Nenhum ficou abaixo do limite de reconhecimento adotado, 28%.
- Exposição média por rodada: aproximadamente 81%, 70% e 65%, respectivamente.
- Foram corrigidos colisores de esferas e cilindros achatados. Antes, uma escotilha plana podia ter um colisor inflado à frente dela, bloqueando um alvo que parecia visível. Dois testes de regressão cobrem esse caso.
- **19 testes EditMode** e **657 verificações C# de domínio/persistência** passaram.
- O validador Python passou, incluindo os 445 hashes, charadas, scripts de narração, materiais, metadados e cenas.

Os registros locais ficam em `Logs/SearchVisibility-v15.json`, `Logs/LayoutReview-v15.log` e `Logs/EditMode-v15.xml`. As prévias são renderizações Unity sem a interface de execução. Dificuldade percebida e conforto de uma partida completa ainda exigem avaliação humana.
