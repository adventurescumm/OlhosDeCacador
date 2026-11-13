# Correção de importação e Input System — v0.6

Este guia registra a migração da v0.6. Para instalar a versão atual com mais objetos, caminhos e cenas separadas, siga [ATUALIZACAO_V09.md](ATUALIZACAO_V09.md). A correção posterior dos metadados está registrada em [CORRECAO_IMPORTADORES.md](CORRECAO_IMPORTADORES.md).

Unity **6000.6.4f1**, URP **17.6.0**, Input System **1.20.1**, destino **Windows x64**.

## O problema relatado

O Console mostrou `Error assigning CUBE texture to 2D texture property '_BaseMap'` e `_MainTex`, além de um ciclo de importação apontando para `character-e.obj`.

O código anterior definia o tipo Default das texturas, sem definir explicitamente sua forma 2D. Um importador previamente configurado como Cubemap podia manter a forma incompatível. Também alterava remaps no callback de pré-importação do modelo e agendava reparos com reimportação após a recarga dos scripts. Essas operações podiam reiniciar a importação. A captura não traz toda a pilha do erro, e o projeto não foi executado neste ambiente; a correção remove essas condições identificadas no código, mas a ausência dos erros precisa ser confirmada no Unity.

## O que mudou

- Os 17 PNGs fornecidos têm `textureShape: 1` (2D) nos metadados. O importador também define explicitamente `TextureImporterShape.Texture2D`, corrigindo configurações antigas. Os GUIDs permanecem iguais.
- Os modelos continuam com texturas Repeat; os sprites da interface usam Clamp. As imagens originais não foram alteradas.
- O reparo carrega as texturas como `Texture2D` e rejeita uma referência incompatível antes de atribuí-la aos materiais. `_BaseMap` e `_MainTex` recebem a mesma textura original; `_BaseColor` e `_Color` recebem a mesma cor do MTL.
- O callback de materiais devolve o material existente. Não altera remaps, não salva assets e não solicita reimportação.
- O reparo explícito executa texturas, materiais e depois remaps; compara os valores antes de alterá-los. As reimportações ocorrem somente para importadores modificados. O agendamento automático após recarga foi removido tanto do reparo quanto da configuração URP.
- Mouse, teclado e interface usam Input System 1.20.1. `activeInputHandler` é 1, correspondente a **Input System Package (New)**. O EventSystem criado pelo jogo usa `InputSystemUIInputModule`, com ações padrão do pacote; os botões registram o frame da ação para bloquear clique no cenário após fechar um painel.

`character-e.obj` não faz parte do catálogo atual, que usa os robôs d, g e h. Se ele existe na sua cópia, pode ser um arquivo acrescentado ou mantido de outra revisão. A correção não o apaga. Um modelo extra sem material fornecido é deixado como está pelo reparo de remaps.

## Abrir uma cópia nova

1. Feche o Unity para interromper o ciclo da versão anterior.
2. Extraia o ZIP atualizado em uma pasta nova e abra `OlhosDeCacador` no Hub com Unity 6000.6.4f1.
3. Aguarde a resolução dos pacotes. Se aparecer a solicitação de ativar o novo backend de entrada, confirme e permita que o editor reinicie.
4. Aguarde a importação e a compilação terminarem. Fora do Play, execute **Olhos de Cacador > 1. Preparar projeto** e **2. Verificar conteudo**. Preparar já inclui o reparo de materiais.
5. Limpe o Console, abra `Assets/Hunter/Scenes/Main.unity` e teste em Play. Veja se aparecem mensagens novas, em vez de considerar erros antigos já registrados.

Preserve a cópia anterior se ela contém gravações ou cenários alterados. O pacote novo mantém os nove campos de áudio vazios; ele não inclui gravações suas que estejam apenas no seu computador.

## Atualizar sua cópia com alterações próprias

Com o Unity fechado e uma cópia de segurança do projeto, compare e substitua estes cinco arquivos pelos do ZIP. Mantenha seus `.meta`:

| Pasta | Arquivos |
| --- | --- |
| `Assets/Hunter/Editor/` | `HunterImporter.cs`, `HunterRenderPipeline.cs`, `HunterBuild.cs` |
| `Assets/Hunter/Runtime/` | `HunterGame.cs`, `HunterUI.cs` |

1. Em `Packages/manifest.json`, acrescente ou atualize `"com.unity.inputsystem": "1.20.1"` dentro de `dependencies`, preservando seus outros pacotes e a sintaxe JSON.
2. Abra o projeto e espere o pacote ser instalado. Se a Unity solicitar habilitar o backend, confirme e reinicie.
3. Confira **Edit > Project Settings > Player > Other Settings > Configuration > Active Input Handling: Input System Package (New)**. Ao mudar a opção, reinicie o editor. O comando Preparar projeto também grava esse valor, mas não substitui o reinício da sessão.
4. Fora do Play e após a compilação/importação, execute **1. Preparar projeto**, que configura URP e repara texturas, materiais e vínculos. Execute **2. Verificar conteudo** após o reinício, se ele ainda era necessário.
5. Execute **5. Restaurar materiais originais** uma segunda vez. Numa importação estável, ele deve registrar zero correções de texturas, materiais e vínculos. Limpe o Console e repita o Play.

Não é necessário substituir os modelos, imagens, catálogos ou banco de narração da sua cópia para aplicar esse reparo. Os scripts corrigem os importadores e materiais existentes. Se você personalizou cores de materiais gerados, o comando de restauração volta às cores declaradas no `material-bindings.json`; compare suas alterações antes de executá-lo. Não use o Render Pipeline Converter sobre os materiais já convertidos, nem repita Reimport All para tentar encerrar o ciclo.

Se os erros continuarem depois desses passos, a primeira mensagem completa e sua pilha no Console permitem identificar o callback restante. O arquivo apontado como “changed” nem sempre contém o código que iniciou a reimportação.

## Conferência de controles

Teste em Play e no executável Windows: clique nos botões, hover e coleta dos objetos, Enter/Enter numérico/Espaço para iniciar, Esc para pausar/voltar, retorno de opções e puzzles. Um clique que fecha um painel não deve coletar um objeto atrás dele. O código trata ausência de mouse ou teclado sem acessar um dispositivo nulo.

Não acrescente outro EventSystem ou StandaloneInputModule à cena. A migração não exige PlayerInput nem um arquivo de ações personalizado. As ações padrão do módulo cuidam da UI; mouse/teclado são consultados diretamente para as interações 3D e os atalhos. Remapeamento de teclas e jogabilidade completa por controle não foram implementados.

## Validação e referências

O pacote passou por validação de conteúdo, referências, hashes dos assets, configuração de entrada e análise de sintaxe C#. Esses testes não executam importação, compilação ou Play no Unity. O roteiro de [VALIDACAO.md](VALIDACAO.md) mantém esses testes reais como pendentes.

- [Input System para Unity 6000.6](https://docs.unity.com/en-us/engine/6000.6/manual/packages-list/packages-all/pack-safe/com-unity-inputsystem).
- [InputSystemUIInputModule 1.20](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.20/api/UnityEngine.InputSystem.UI.InputSystemUIInputModule.html).
- [Ciclo de atualização do Asset Database](https://docs.unity.com/en-us/engine/6000.6/manual/assets-and-media/asset-database/refreshing).
- [OnAssignMaterialModel](https://docs.unity.com/en-us/engine/6000.6/script-reference/unityeditor/assetpostprocessor/onassignmaterialmodel).
- [TextureImporter.textureShape](https://docs.unity.com/en-us/engine/6000.6/script-reference/unityeditor/textureimporter/textureshape).
