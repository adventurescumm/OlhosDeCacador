# Correção do verificador de modelos — v0.9

O erro `Modelo ausente: Assets/Resources/BemTeVi/Models/polyhaven-duck/rubber_duck_toy_1k.obj` era causado por `BemTeViBuild.ValidateStages`: o método acrescentava `.obj` ao nome de todos os modelos. O pato está corretamente incluído como `rubber_duck_toy_1k.fbx`.

A validação agora carrega o modelo pelo mesmo caminho Resources sem extensão usado pelo jogo e obtém o caminho real via `AssetDatabase.GetAssetPath`. A verificação do ModelImporter usa o `.meta` correspondente ao modelo carregado. Um modelo realmente ausente continua bloqueando a validação.

O comando **3. Compilar Windows** também foi corrigido: ele passa todas as cenas habilitadas em EditorBuildSettings ao build. Antes, a lista explícita continha somente Main, apesar das 18 cenas estarem cadastradas.

## Aplicar à sua cópia v0.9

1. Extraia o ZIP atualizado em uma pasta temporária.
2. Copie somente `Assets/BemTeVi/Editor/BemTeViBuild.cs` para o mesmo caminho no seu projeto. Preserve o `.meta` existente.
3. Volte ao Unity e aguarde a recompilação e qualquer importação em andamento.
4. Execute **Bem-Te-Vi > 2. Verificar conteudo**. Depois, se desejar gerar o executável, use **3. Compilar Windows**.

Não renomeie o FBX para OBJ. Esta correção não exige substituir cenas, catálogo, materiais ou gravações, nem recriar as cenas já preparadas.

## Verificação

O caso anterior foi reproduzido fora do Unity executando os métodos reais do verificador com um adaptador de assets baseado nos arquivos do projeto. Com a correção, o catálogo completo passa, e uma referência inexistente continua sendo rejeitada. Esse teste não executa o importador real da Unity.

Dois testes EditMode em `Assets/BemTeVi/Tests/Editor/BuildContentTests.cs` cobrem o FBX original e a rejeição de um modelo ausente. Eles estão incluídos para execução em **Window > General > Test Runner > EditMode**; não foram executados neste ambiente, que não tem Unity instalado.

Referências: [Resources.Load](https://docs.unity.com/en-us/engine/6000.6/script-reference/unityengine/resources/load), [AssetDatabase.GetAssetPath](https://docs.unity.com/en-us/engine/6000.6/script-reference/unityeditor/assetdatabase/getassetpath) e [BuildPlayerOptions.scenes](https://docs.unity.com/en-us/engine/6000.6/script-reference/unityeditor/buildplayeroptions/scenes).
