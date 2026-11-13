# Validação e roteiro de testes

Data: 13/11/2026. Revisão do projeto: **v0.7 — importadores e adaptação do vídeo**. Versão-alvo atual: **Unity 6000.6.4f1**, revisão `12bfff696524`.

## Verificações realizadas nesta entrega

| Verificação | Resultado | Limite da evidência |
| --- | --- | --- |
| Catálogo e referências locais | 3 mapas, 9 telas, 18 alvos, 271 instâncias nas buscas, 22 no quarto e 33 nos desafios, 83 modelos únicos | Confere os arquivos e dados; não executa Unity |
| Preservação dos assets | 190 arquivos de terceiros conferidos por SHA-256 contra os downloads | Inclui OBJ, MTL, texturas, áudio e sprites de interface; não mede a aparência após importação |
| Materiais e texturas | 167 materiais URP/Lit; cores e texturas conferidas contra os MTL, incluindo aliases `_MainTex` e `_Color`; 17 metadados PNG explicitamente 2D; três robôs com UVs negativas e Repeat configurado | Precisa conferir a importação real no editor |
| Ciclo de importação | Callbacks não chamam reparo, AddRemap, SaveAndReimport ou SaveAssets; agendamento de reparo/configuração após recarga removido | Análise estática do código; não reproduz nem encerra uma sessão Unity real |
| Entrada | Input System 1.20.1, activeInputHandler 1, InputSystemUIInputModule e leituras Mouse/Keyboard; nenhuma chamada ao Input legado no runtime | Configuração e código conferidos; interação real ainda pendente |
| Pipeline | URP 17.6.0, referências Graphics → HunterURP → HunterRenderer e Quality → HunterURP conferidas; Forward, MSAA 2×, HDR desligado | Confere arquivos serializados; ativação do URP e criação dos Global Settings requerem Unity |
| Narração | Nove IDs e nove roteiros correspondem às charadas; campos de áudio vazios | Reprodução e volumes requerem áudios humanos e Unity |
| Enquadramento da abertura | Projeção externa confere três miniaturas dentro da área reservada, incluindo oscilações de até 2° | Não verifica renderização ou desempenho no Unity |
| Importadores | 83 blocos ModelImporter (schema 19300) e sete AudioImporter (schema 6); versões acima dos mínimos 25/6; GUIDs preservados | A ausência dos avisos precisa ser confirmada após importar no Unity |
| Rotas e desafios | Três entradas no quarto, 18 passagens e índices únicos das peças dos três desafios | Integridade de dados; cliques e temporização dependem de Play |
| Cena inicial | GUID do controlador corresponde ao script incluído | Cena ainda não foi aberta no Unity |
| Sintaxe C# | Treze arquivos analisados com parser C#, sem erro de sintaxe | Não é compilação; não verifica ligação às APIs do Unity |
| Atribuições URP em HunterRenderPipeline | Onze atribuições restantes conferidas contra setters públicos da documentação URP 17.6; cinco campos serializados conferidos no código oficial | Verificação da API usada por esse script, sem execução do compilador Unity |
| Composição e visibilidade | Treze composições revistas em Blender; projeção a 720 × 352 verifica os 18 alvos, placas, entradas e peças, com o cenário como oclusor | Prévias e teste matemático externos; não validam o renderer e raycast Unity nem o desempenho |
| Pacotes | Input System 1.20.1, URP 17.6.0, uGUI 2.6.0 e Test Framework 1.8.0; GUIDs do shader Lit e dos tipos de configuração conferidos no código oficial Unity Graphics da linha 6000.6 | A resolução pelo Package Manager precisa ocorrer no computador da equipe |

A v0.5 retira o comando de build web, suas configurações e o servidor HTTP auxiliar; inclui saída com confirmação na abertura/pausa e preparação exclusiva para Windows x64. A revisão visual baseada no vídeo foi implementada na v0.7, conforme REFERENCIA_VIDEO.md; seu playtest continua pendente.

A v0.6 trata os erros relatados de CUBE em `_BaseMap`/`_MainTex` e o ciclo de importação, além de migrar os controles para Input System. O reparo exige texturas 2D, restaura os materiais originais e altera remaps fora dos callbacks. As instruções estão em CORRECAO_IMPORTACAO_INPUT.md. A ausência dos erros após importar esta revisão ainda precisa ser confirmada no editor do usuário.

**Correção após relato do usuário:** a tentativa de abrir o pacote anterior no Unity produziu dois erros CS0200 em `HunterRenderPipeline.cs`, nas atribuições a `supportsMainLightShadows` e `supportsAdditionalLightShadows`. Ambas foram substituídas pela configuração dos campos serializados do asset. A conferência contra a API pública detecta exatamente essas duas atribuições inválidas no arquivo anterior e nenhuma no arquivo corrigido. A recompilação real após a correção e a saída do Safe Mode ainda precisam ser confirmadas no editor do usuário.

Não há **compilação bem-sucedida confirmada** nesta entrega; o ambiente de edição dos arquivos não possui Unity. O Test Runner **não foi executado**, e não há medições de FPS, memória, tempo de carregamento ou tamanho de build. Foram usadas projeções e prévias externas para conferir composição e enquadramento; elas não são execução do jogo. Nenhum print externo deve ser apresentado como captura de uma versão Unity validada. As novas composições foram conferidas com importação OBJ nativa no Blender; o shader e a iluminação externos não comprovam o resultado URP.

Para repetir a conferência de catálogo e assets, com Python 3:

```bash
python Tools/validate_content.py
python Tools/repair_importer_metadata.py --check
```

O resultado termina informando explicitamente que compilação e playtest não foram executados por esse script.

## Testes de domínio fornecidos

Abra `Window > General > Test Runner`, selecione **EditMode** e execute os testes de `OlhosDeCacador.Tests`. Os oito testes cobrem:

1. Clique repetido e ID desconhecido não aumentam a contagem.
2. Um mapa não termina antes de todos os seus objetos serem encontrados.
3. Reiniciar um mapa preserva os outros.
4. Entradas repetidas, desconhecidas e conclusões impossíveis são filtradas ao carregar.
5. Schema inválido pode ser detectado pela camada de recuperação.
6. Serializar e carregar preserva os IDs coletados.
7. Errar a sequência reinicia a tentativa e preserva as descobertas.
8. Senha e associação exigem as respostas corretas completas.

Esses testes não substituem testes de interface, colisores, importação, salvamento físico ou executável Windows.

## Roteiro de playtest obrigatório

Preencha o resultado com data, versão do editor, versão do Windows e hardware. Não marque uma linha como aprovada apenas por leitura de código.

| Caso | Ação | Resultado esperado | Resultado |
| --- | --- | --- | --- |
| Versões de importadores | Abrir o pacote novo ou executar o reparo com Unity fechado; importar e limpar Console | Nenhum novo aviso de AudioImporter 1/6 ou ModelImporter 1/25 | Pendente |
| Quarto | Clicar no baú, nave e heroína, pelas malhas e pelos rótulos | Abre o mundo certo; mostra progresso e selo corretos | Pendente |
| Inicialização | Abrir Main.unity e Play | Tela de início colorida, título completo, três miniaturas e botão Vamos brincar!; nenhum erro vermelho | Pendente |
| URP | Aguardar importação; executar Preparar projeto e Verificar conteudo; limpar Console e reabrir o projeto | URP 17.6.0 resolvido; HunterURP atribuído em Graphics e Quality; aviso Built-in não reaparece | Pendente |
| Reparo de importação | Atualizar a cópia afetada, aguardar importação e executar Preparar projeto; limpar Console | Texturas 2D; nenhum novo erro CUBE ou ciclo de reimportação | Pendente |
| Reparo repetido | Após o primeiro reparo, executar Restaurar materiais originais novamente, sem editar arquivos | Zero correções de texturas, materiais e vínculos; importação permanece estável | Pendente |
| Recuperação de Cubemap | Numa cópia de teste, configurar uma textura de modelo como Cube; depois executar o reparo | Callback/reparo restaura 2D; material usa o Texture2D original; nenhuma reimportação infinita | Pendente |
| Novo backend | Conferir Input System 1.20.1 e Active Input Handling New; reiniciar quando solicitado | Verificar conteudo passa; nenhum uso do backend antigo; um EventSystem com InputSystemUIInputModule em Play | Pendente |
| Câmeras URP | Alternar abertura, menu e busca; redimensionar a janela | Fundo preenche a janela; diorama aparece no espaço correto, sem rastros ou apagar a interface | Pendente |
| Começar | Usar clique, Enter, Enter numérico e Espaço, em aberturas separadas | Abre a seleção de mundos uma vez; preserva descobertas | Pendente |
| Seleção na abertura | Selecionar Como brincar/Opções e pressionar Enter | Aciona o botão selecionado, sem abrir a seleção de mundos pelo atalho global | Pendente |
| Retorno à abertura | Usar Tela inicial e Esc na seleção; repetir cinco vezes | Retorna à abertura, sem acumular miniaturas e sem perder progresso | Pendente |
| Abertura e movimento | Alternar Movimento reduzido pelas opções da abertura; reiniciar | Preferência persistida; miniaturas, estrelas e botão ficam estáticos quando habilitada | Pendente |
| Diálogos da abertura | Abrir e fechar Como brincar, Opções e Créditos | Retorna à abertura; o mesmo fluxo no menu retorna ao menu | Pendente |
| Conteúdo | Executar `2. Verificar conteudo` | Modelos e materiais importados, 18 IDs únicos | Pendente |
| Materiais | Inspecionar as nove telas | Sem material rosa/cinza por falta de vínculo; robôs com detalhes vermelho, violeta e amarelo; texturas legíveis; enquadramento sem cortar os alvos | Pendente |
| Alvos | Clicar nos dois alvos de cada tela, inclusive repetidamente durante a reação | Cada alvo conta uma vez, reage conforme seu tipo e desaparece; animação não bloqueia save | Pendente |
| Oclusão | Clicar num obstáculo à frente de um alvo | O clique não atravessa o obstáculo | Pendente |
| Charada e voz | Conferir os nove textos, Ler juntos, Ouvir/Parar e as duas dicas em 1280 × 720 | Nenhum verso ou controle cortado; voz da sala correta; busca ocupa a área central | Pendente |
| Curiosidades | Clicar em decorações reativas e coletar nave, robô, garrafa e ferramenta | Reações diferentes; foco retorna ao plano geral; progresso só muda em alvos | Pendente |
| UI | Clicar repetidamente em menus sobre o mundo | Nenhum objeto coletado pelo mesmo clique | Pendente |
| Fechar painel sobre alvo | Fechar caderno/diálogo/pausa com um alvo atrás do botão | Volta à busca sem coletar o alvo no mesmo frame | Pendente |
| Dispositivos ausentes | Iniciar sem mouse ou sem teclado em ambiente de teste | Sem NullReferenceException; controles disponíveis continuam funcionando | Pendente |
| Navegação | Usar as 18 placas e seus rótulos; alternar telas pelo menos 20 vezes | Destinos corretos, só uma sala de busca ativa; alvos encontrados não reaparecem | Pendente |
| Ajuda | Pedir três dicas do mesmo alvo | Tela → posição → destaque visual, sem retirar progresso | Pendente |
| Baú | Entrar pelo baú da Mina; clicar nas garrafinhas; tentar senha errada e depois 243 | Nova tentativa disponível; selo concedido apenas na resposta correta | Pendente |
| Sinais | Entrar pelo rádio da Garagem; rever, clicar durante a demonstração, errar, rever e acertar nas peças 3D | Sem entrada durante demonstração; sequência pode reiniciar | Pendente |
| Saída do sinal | Sair durante a demonstração | Coroutine interrompida, HUD e busca continuam utilizáveis | Pendente |
| Ferramentas | Entrar pela maquete do Quartel; tentar associação errada e depois as três corretas usando malhas/rótulos | Ferramenta correta vai ao destino; acertos preservados, sem reutilização | Pendente |
| Reinício parcial | Concluir dois mapas e reiniciar apenas um | Outro mapa e seu selo continuam guardados | Pendente |
| Reinício geral | Cancelar e depois confirmar Novo jogo | Cancelar preserva tudo; confirmar zera só este jogo | Pendente |
| Vitória geral | Resolver os três mapas na mesma sessão | Mensagem final; retorno ao menu e aos mundos | Pendente |
| Persistência | Encontrar objetos, fechar e reabrir | Mesmos IDs e selos; preferências mantidas | Pendente |
| Save danificado | Em ambiente de teste, corromper somente a chave principal do jogo | Cópia de recuperação carregada; aviso; partida utilizável | Pendente |
| Armazenamento indisponível | Simular falha de gravação em ambiente de teste Windows | Se houver falha reportada, continuar a sessão e mostrar aviso | Pendente |
| Opções | Abrir pelo menu e pela pausa; ajustar e voltar, inclusive por Esc | Tela própria; volumes e preferência de movimento persistem; retorno à origem | Pendente |
| Leitura sem áudio | Usar Ler juntos nas nove telas | Texto completo e ampliado; voltar à busca preserva progresso | Pendente |
| Narração humana | Atribuir WAV/OGG a um campo e ouvir/parar/repetir | Áudio da sala correta; texto acompanha; volume de voz independente dos efeitos | Pendente |
| Interrupção da voz | Trocar sala, pausar, abrir puzzle ou mudar de janela durante leitura | Fala anterior interrompida; nenhuma sobreposição | Pendente |
| Leitura automática | Ativar, visitar telas com e sem gravação; silenciar | Reproduz só onde há áudio e volume disponível; texto sempre legível | Pendente |
| Som | Iniciar pelo menu, silenciar e reativar | Sem autoplay obrigatório; puzzles continuam possíveis sem áudio | Pendente |
| Movimento reduzido | Habilitar, coletar alvo, clicar em decoração e acertar uma ferramenta | Sem foco de câmera, salto/rotação ou percurso animado; texto e posicionamento imediato continuam claros | Pendente |
| Foco da janela | Usar Alt+Tab durante o sinal | Volta possível; botão repetir disponível | Pendente |
| Resolução | Testar 1440×900, 1280×720 e fullscreen | UI e câmera alinhadas; alvos clicáveis | Pendente |
| Windows | Executar distribuição fora da pasta do projeto | Abre sem o editor e permite completar os três mapas | Pendente |
| Saída | Usar Sair do jogo na abertura e na pausa; cancelar e confirmar | Cancelar retorna à tela anterior; confirmar fecha o executável; reabrir preserva o progresso salvo | Pendente |
| Saída no editor | Confirmar Sair do jogo em Play Mode | Encerra o Play Mode sem fechar o editor | Pendente |
| Janela Windows | Redimensionar e alternar tela cheia | UI e cliques permanecem alinhados, sem cortar controles | Pendente |

Para testar corrupção, use apenas as chaves `OlhosDeCacador.v1` e `OlhosDeCacador.v1.backup` em uma instalação de teste. Não use `PlayerPrefs.DeleteAll()` para limpar dados de outros projetos. Os dados de aplicação são locais; use um usuário Windows de teste, sem apagar os dados da conta principal.

## Metas de desempenho a medir

Meta de trabalho: 60 FPS no computador de referência; se isso não for possível, manter estabilidade e evitar quedas durante cliques. Escolha e registre o hardware real antes de avaliar a meta. No Profiler, anote frame time, draw calls, triângulos, memória e picos ao trocar de sala. Não há valores medidos nesta entrega.

Se houver problemas, priorize: reduzir sombras; diminuir a qualidade; reduzir distratores; revisar materiais; verificar coleta de memória após muitas trocas. Não retire colisores de oclusão sem testar se os cliques passam a atravessar o cenário.

## Problemas comuns

- **AudioImporter/ModelImporter abaixo do mínimo:** os metadados da v0.7 incluem os blocos necessários. Para reparar uma cópia antiga preservando GUIDs, feche Unity e siga CORRECAO_IMPORTADORES.md. Não apague os `.meta`.
- **Quarto ou passagens ausentes após atualizar:** copie o catálogo v0.7 junto dos scripts; os campos `hub`, `challenge`, câmeras e ações fazem parte desta implementação. Preserve sua versão anterior se tinha alterações próprias.

- **Aviso Built-in depreciado:** confirmar que abriu a revisão v0.7; aguardar URP 17.6.0 no Package Manager, executar `1. Preparar projeto` e conferir Graphics/Quality conforme MIGRACAO_URP.md. Depois limpe o Console para distinguir mensagens antigas de novas.
- **Assets rosas ou sem cores:** aguardar a resolução do URP e a compilação dos scripts, executar `1. Preparar projeto` e `2. Verificar conteudo`. O verificador exige URP/Lit, cores/texturas originais, 2D e Repeat. Não usar Clamp nos robôs: os modelos originais usam UVs negativas.
- **CUBE em BaseMap/MainTex ou ciclo de importação:** fechar Unity, aplicar os cinco scripts e a configuração de entrada conforme CORRECAO_IMPORTACAO_INPUT.md. Aguardar importação e executar o reparo explícito. Não repetir Reimport All dentro do ciclo.
- **Botão Ouvir não aparece:** os nove campos começam vazios. Atribua sua gravação ao NarrationBank, conforme NARRACAO.md. Enquanto isso, Ler juntos é a alternativa de texto.
- **Cena parece vazia antes do Play:** esperado; `HunterGame` monta a sala durante a partida.
- **Input indisponível:** confirmar Input System 1.20.1 e Active Input Handling: Input System Package (New); reiniciar o editor depois de mudar o backend. Remover EventSystems acrescentados à cena por engano; o controlador já cria um.
- **Texto pequeno na janela:** ampliar a janela ou usar fullscreen. Mobile ainda não faz parte do aceite.
- **Erro ao compilar uma plataforma:** conferir o módulo correspondente no Unity Hub e ler a primeira mensagem de erro no Console.
- **Shader ou textura diferentes entre editor e build:** verificar o primeiro build antes de produzir capturas e ajustar o importador se necessário.
