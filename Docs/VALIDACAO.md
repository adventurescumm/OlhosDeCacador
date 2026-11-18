# Validação — v0.10

Conferência de 14/11/2026. Unity **6000.6.4f1**, Windows x64, URP 17.6.0 e Input System 1.20.1 (New).

## Resultados externos

| Verificação | Resultado | Limite |
| --- | --- | --- |
| Catálogo | 3 mundos, 9 buscas, 54 IDs, 6 versos por busca, 629 instâncias nas buscas | Integridade de dados |
| Mapa | Passagens físicas recíprocas, circuito por mundo, seis mecanismos, atalhos bloqueados e final após progresso | Não mede legibilidade das passagens em Play |
| Lógica C# | **175 verificações passaram**, compilando o arquivo real Domain/GameData.cs com Roslyn/.NET 8 | Não compila APIs Unity |
| Sintaxe | 20 arquivos C# sem erro de sintaxe no parser | Não verifica referências às APIs dos pacotes |
| Assets | 370 arquivos originais conferidos por SHA-256; 157 OBJ e um FBX; 141 modelos distintos usados | Não executa importação Unity |
| Materiais | 274 vínculos URP, 24 importadores de imagens explicitamente 2D | Aparência final precisa de Unity |
| Metadados | 158 ModelImporters completos, schema 19300; 28 AudioImporters schema 6 | Avisos reais precisam ser conferidos no Console |
| Preservação | IDs e GUIDs existentes mantidos; originais artísticos antigos sem alteração | Não importa cenas personalizadas do usuário |
| Cenas | 18 arquivos e referências no build, com câmeras atualizadas | Primeira preparação deve montar hierarquias no editor |
| Narração | Nove IDs e textos correspondem ao catálogo e banco; roteiros v0.9 preservados | Nenhuma gravação humana incluída |
| Novas fontes | Os 54 versos cabem nas duas colunas; acentos portugueses e marcador disponíveis | Layout Unity deve ser conferido |
| Áudio pronto | 28 OGG decodificados sem erro; 21 adições, arquivos originais preservados | Mixagem perceptiva, transições e volume precisam de audição no Unity |
| Composição | 16 composições distintas conferidas externamente com os modelos originais | Prévias Blender não são capturas do jogo Unity |
| Visibilidade | Os 54 alvos e interações têm área visível na projeção externa, com geometria como oclusor; mínimo de 35 pixels na resolução reduzida do teste | Não comprova raycast Unity, conforto ou dificuldade |
| Interface de navegação | Sem WorldButton de destino, botão Caminho, seletor de peças ou atalho de escolha de mundo na pausa | Leitura e conforto ainda precisam de playtest |

**Não foram executados compilação completa Unity, importação real, preparação das cenas, Play, build Windows ou Profiler.** Não há executável pronto nem resultados de FPS. Os testes de domínio e o parser não substituem a compilação no editor.

## Repetir as conferências incluídas

```bash
python Tools/validate_content.py
python Tools/repair_importer_metadata.py --check
```

O projeto inclui **15 testes NUnit**: 13 em `Assets/Hunter/Tests/Editor/ProgressTests.cs`, cobrindo coleta, saves, mecanismos, reinício, memória, combinação, associação e passagens físicas, e dois em `BuildContentTests.cs`, cobrindo a validação dos modelos OBJ/FBX e a rejeição de uma referência ausente. Execute **Window > General > Test Runner > EditMode**. Esses testes não foram executados no Test Runner deste ambiente. O reparo do erro de extensão e sua checagem externa estão documentados em [CORRECAO_MODELO_FBX.md](CORRECAO_MODELO_FBX.md).

## Roteiro no Unity — todos pendentes

| Caso | Ação | Resultado esperado |
| --- | --- | --- |
| Importação | Abrir cópia nova, aguardar pacotes, preparar e verificar | Sem erros CS0200, CUBE/2D, ciclo de importação ou versões antigas de importadores |
| Bonecos | Observar as 26 instâncias e clicar nelas | Rostos visíveis, sem esconder alvos ou introduzir metas adicionais |
| Som | Coletar, atravessar e resolver mecanismos em cada mundo; abrir Menu durante ambiente | Efeitos coerentes, ambiente discreto, sem repetição sobreposta; voz humana em primeiro plano |
| Pato FBX | Inspecionar no quarto e na estação | Textura original com olhos/bico, normais corretas, sem material rosa; escala e colisores válidos |
| Reparo repetido | Restaurar materiais novamente sem mudanças | Nenhuma correção desnecessária nem novo ciclo de importação |
| Cenas | Preparar duas vezes e abrir uma busca | Primeira montagem cria grupos editáveis; segunda preserva alterações manuais |
| Objetos condicionais | Resolver mecanismo numa cena preparada | Peça aparece sem componente duplicado ou referência nula; não aparece antes da solução |
| Abertura | Main → Iniciar, Opções e Como jogar | Título e quarto; retorno correto dos painéis |
| Entradas | Clicar nas três maquetes do quarto | Mundo correspondente; nenhuma lista de destinos sobre o cenário |
| Travessia | Percorrer entrada → busca 1 → busca 2 → busca 3 → entrada e voltar | Somente conexões físicas; passagens seguintes bloqueadas até seus requisitos |
| Atalho | Tentar da entrada à busca 3 antes/depois do segundo mecanismo | Bloqueio inicialmente; atalho utilizável depois |
| Mecanismos | Clicar sem peça, com charada incompleta e depois completa | Mensagens adequadas; ação só após requisitos; peça montada e movimento visível |
| Movimento reduzido | Resolver com a opção ligada/desligada | Mesmo resultado, imediatamente quando reduzido; sem acumular deslocamentos ao abrir opções |
| Busca | Coletar seis alvos em cada uma das nove maquetes | Sem duplicação, objetos visíveis/clicáveis, descoberta e save corretos |
| Mouse neutro | Passar por alvo e decoração comum | Mesmo texto e ausência de destaque exclusivo do alvo |
| Enquadramento | Visitar todas as buscas e passagens; usar roda e botão direito | Plano próximo fixo; roda e botão direito não deslocam a câmera; alvos e passagens acessíveis |
| Oclusão | Clicar em superfície que esteja à frente de um alvo | O clique não atravessa a superfície |
| Dificuldade | Jogador novo completa a primeira busca | Registrar tempo, erros e dicas; ajustar composição se for trivial ou frustrante |
| Dicas/charada | Testar Menu → Uma ajudinha, leitura ampliada e faixa em 1280×720 | Versos completos, ordem correta, ajuda gradual sem perder progresso |
| Narração | Atribuir clipe da estação atualizado; ouvir/parar/trocar cena | Texto e voz correspondem; fala para ao mudar de cena ou pausar |
| Pausa e opções | Abrir em busca e entrada, ajustar e voltar por Esc | Retorna à cena atual sem duplicar a sessão |
| UI sobre alvo | Fechar painel com objeto atrás do botão | O mesmo clique não coleta o alvo |
| Desafios | Resolver senha, sequência e associação, incluindo erros e saídas | Repetição possível, selo correto e saída física retorna à terceira busca |
| Carregamento | Trocar de cena 30 vezes | Uma sessão, um EventSystem, um AudioListener, sem salas acumuladas |
| Save v0.8 | Carregar descobertas e passagens antigas | IDs, selos válidos e mecanismos abertos preservados |
| Reinício | Reiniciar um mundo e cancelar/confirmar novo jogo | Outros mundos preservados no parcial; cancelamento não apaga dados |
| Recuperação | Corromper somente a chave principal em instalação de teste | Backup recuperado ou nova partida utilizável com aviso |
| Windows | Compilar e executar fora do editor | Jogo completo, saída, save, janela e áudio funcionam |

Chaves de save: `OlhosDeCacador.v1` e `OlhosDeCacador.v1.backup`. Para testes de corrupção, use cópia isolada; não use PlayerPrefs.DeleteAll().

## Desempenho e limites visuais

Meta de trabalho: 60 FPS, ainda sem medição. Registre hardware, frame time, draw calls, triângulos, memória e picos de carregamento. Confira também a memória após muitas trocas. O enquadramento próximo aumenta a legibilidade, mas ainda precisa de playtest em tamanhos de janela diferentes.

A maioria dos modelos continua estilizada. O novo pato acrescenta geometria e textura mais detalhadas; a entrega não afirma que todos os assets tenham deixado de ser poligonais. Todas as malhas são trianguladas pelo renderer; o objetivo visual é brinquedos reconhecíveis e maquetes coerentes.

Para atualizar cenas e gravações sem restaurar o fluxo antigo, siga [ATUALIZACAO_V09.md](ATUALIZACAO_V09.md). Para hierarquias vazias antes da primeira preparação, siga [CENAS.md](CENAS.md).
