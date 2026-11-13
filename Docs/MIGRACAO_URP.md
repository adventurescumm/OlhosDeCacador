# Migração para URP — introduzida na v0.4, mantida na v0.7 Windows

O aviso **“The Built-In Renderer Pipeline is deprecated”** se refere ao pipeline usado na versão anterior. Esta revisão usa **URP 17.6.0**, mantendo **Unity 6000.6.4f1**.

## Abrir a revisão

1. Extraia o ZIP em uma pasta nova e abra `OlhosDeCacador` pelo Unity Hub com o editor 6000.6.4f1.
2. Espere o Package Manager resolver o pacote **Universal Render Pipeline 17.6.0** e a importação terminar.
3. Execute **Olhos de Cacador > 1. Preparar projeto** e depois **2. Verificar conteudo**.
4. Abra `Assets/Hunter/Scenes/Main.unity` e pressione Play.
5. Limpe o Console e reabra o projeto para conferir se o aviso reaparece. Uma mensagem antiga no Console não indica que a configuração atual ainda usa Built-in.

Caso já tenha gravado sua narração ou editado cenários na versão anterior, mantenha essa pasta. Traga para a nova cópia os áudios, o `NarrationBank.asset` preenchido e seus respectivos `.meta`; traga também seu `catalog.json` se o alterou. Mudanças próprias nos scripts devem ser comparadas antes de substituir arquivos. A chave de salvamento do jogo continua `OlhosDeCacador.v1`.

## Campos para conferir no editor

| Local | Valor esperado |
| --- | --- |
| Package Manager → Universal Render Pipeline | 17.6.0 |
| Project Settings → Graphics → Default Render Pipeline | `HunterURP` |
| Project Settings → Quality → Render Pipeline Asset | `HunterURP` em todos os níveis existentes |
| `Assets/Hunter/Settings/HunterURP.asset` → Renderer List | `HunterRenderer`, índice padrão 0 |
| `Assets/Hunter/Settings/HunterRenderer.asset` → Rendering Path | Forward |
| Materiais em `Assets/Resources/Hunter/Materials` → Shader | Universal Render Pipeline/Lit |

O nível de qualidade atual chama-se **Windows**, desde a revisão v0.5. Os assets fornecidos já estão atribuídos em Graphics e Quality. O comando **6. Configurar URP** refaz as referências se tiverem sido removidas, e também é chamado por Preparar projeto/build. Desde a v0.6, o projeto não agenda esse comando após importação ou recarga de scripts: as gravações são feitas por comandos explícitos, com o editor fora de Play e da importação. A configuração mantém os ajustes de qualidade dos assets já existentes; os valores iniciais são MSAA 2×, HDR desligado, sombras de 1024 px e distância de 35 unidades.

Os **Global Settings**, perfis e recursos internos são inicializados pelo próprio URP no editor. Não é necessário montar suas referências manualmente. A revisão usa Render Graph e não adiciona Renderer Features ou passes personalizados. Não procure habilitar o antigo Compatibility Mode para resolver este aviso.

## O que mudou no código e nos materiais

- O manifest instala URP 17.6.0 e habilita os módulos internos de partículas e conversão de imagens usados por ferramentas gráficas do editor; os arquivos de Graphics e Quality apontam para o pipeline incluído. Isso não adiciona efeitos de partículas ao jogo.
- Os 167 materiais mantêm a cor original `Kd` em `_BaseColor` e a textura original em `_BaseMap`.
- Os materiais incluem os metadados de versão usados pelo importador URP 17.6, para que a primeira importação não os confunda com materiais de versões antigas do LWRP.
- As salas primitivas usam URP/Lit e `_Smoothness`; não criam mais materiais Standard.
- As duas câmeras usam dados URP explícitos: uma limpa o fundo e a outra renderiza o diorama em seu viewport. Pós-processamento e cópias de depth/opaque texture ficam desligados.
- O verificador do editor confere pipeline, renderer, níveis de qualidade, shaders, cores e texturas antes do build.

Nenhum OBJ, MTL, PNG ou OGG original foi modificado. A tela de início, as charadas, os puzzles e os nove espaços de gravação permanecem no projeto.

## Correção CS0200 e Safe Mode

As propriedades `supportsMainLightShadows` e `supportsAdditionalLightShadows` do URP 17.6 não oferecem um setter público. A versão anterior de `HunterRenderPipeline.cs` tentou atribuir valores a elas, causando os dois erros CS0200 relatados pelo usuário. O problema estava no script fornecido.

O script corrigido configura `m_MainLightShadowsSupported`, `m_AdditionalLightShadowsSupported` e `m_AnyShadowsSupported` pelo `SerializedObject` já usado na criação do asset. Os valores continuam: sombras da luz principal ligadas, sombras de luzes adicionais desligadas. As outras onze atribuições a propriedades de URP nesse script foram conferidas na API pública 17.6 e têm setter público.

Os passos abaixo aplicam **somente a correção histórica dos dois CS0200**. Para instalar também o reparo de texturas/importação e o Input System da v0.6, siga [CORRECAO_IMPORTACAO_INPUT.md](CORRECAO_IMPORTACAO_INPUT.md).

Para aplicar apenas a correção CS0200 na sua cópia em uso:

1. Extraia o pacote atualizado em uma pasta temporária.
2. Copie **somente** `Assets/Hunter/Editor/HunterRenderPipeline.cs` para o mesmo caminho no seu projeto, substituindo o arquivo anterior. Mantenha o `.meta` existente.
3. Volte ao Unity e aguarde a recompilação. Se ela não iniciar, feche e reabra o projeto.
4. Quando não restarem erros de compilação, a Unity sai automaticamente do Safe Mode e continua a importação.
5. Depois da importação, execute **Olhos de Cacador > 1. Preparar projeto** e **2. Verificar conteudo**.

Não é necessário substituir seus cenários, catálogos ou gravações para corrigir esses dois erros. Se aparecer outra mensagem, ela precisa ser diagnosticada separadamente; esta entrega não contém uma compilação Unity executada neste ambiente.

Referências: [API URP 17.6](https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@17.6/api/UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset.html) e [Safe Mode na Unity 6.6](https://docs.unity.com/en-us/engine/6000.6/manual/programming-environment/script-compilation/safe-mode).

## Se o aviso ou materiais rosas persistirem

Confirme primeiro a pasta aberta no Hub e a versão do editor. Depois veja se o Package Manager terminou sem erro. Sem o pacote URP, os tipos de configuração e o shader Lit não podem ser carregados, e os menus do projeto podem nem compilar.

Com o pacote resolvido, execute Preparar projeto, Restaurar materiais originais e Verificar conteudo. Não é necessário passar o Render Pipeline Converter sobre este pacote: os materiais fornecidos já estão convertidos. Para diagnosticar um problema restante, copie a **primeira mensagem completa de erro** do Console, incluindo arquivo e linha quando houver.

## Limite da validação

Os arquivos, GUIDs, cores, texturas e sintaxe C# foram conferidos externamente. O ambiente desta entrega não possui Unity; não foram executados importação real, compilação, Play ou build Windows. Use o roteiro de [VALIDACAO.md](VALIDACAO.md) antes de publicar.

## Referências técnicas

- [URP na Unity 6.6](https://docs.unity.com/en-us/engine/6000.6/manual/packages-list/packages-all/pack-core/com-unity-render-pipelines-universal).
- [Instalação e atribuição do pipeline](https://docs.unity.com/en-us/engine/6000.6/manual/render-pipelines/universal-render-pipeline/introduction/installing-and-configuring-urp/upgrading-from-birp/install-urpinto-aproject).
- [API UniversalRenderPipelineAsset 17.6](https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@17.6/api/UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset.html).
- [Código oficial URP, linha 6000.6](https://github.com/Unity-Technologies/Graphics/tree/6000.6/staging/Packages/com.unity.render-pipelines.universal): GUIDs e propriedades conferidos em `Runtime/Data/UniversalRenderPipelineAsset.cs`, `Runtime/UniversalRendererData.cs`, `Shaders/Lit.shader`, `Editor/AssetVersion.cs` e `Editor/AssetPostProcessors/MaterialPostprocessor.cs`.
