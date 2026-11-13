# Importadores antigos e atualização visual — v0.7

O aviso de AudioImporter versão 1 abaixo do mínimo 6, e ModelImporter versão 1 abaixo do mínimo 25, surgiu porque os `.meta` fornecidos tinham apenas cabeçalho e GUID, sem o bloco de configurações do importador. A falha estava nos metadados do pacote. Não era necessário mudar a versão do Unity.

A revisão preenche os 83 importadores de modelos e sete importadores de áudio. Os GUIDs permanecem iguais, assim como os arquivos OBJ, MTL, PNG e OGG originais. ModelImporter usa schema 19300 e AudioImporter usa schema 6, dentro dos mínimos relatados. São schemas usados em um projeto oficial Unity; o editor pode regravá-los em formato mais recente ao importar. Os modelos são estáticos, com normais originais e sem animação/câmera/luz importada. Os efeitos usam Vorbis e mono.

`fileFormatVersion`, `serializedVersion` e `AssetPostprocessor.GetVersion()` têm funções diferentes. Alterar somente GetVersion não corrige a ausência do bloco ModelImporter/AudioImporter no `.meta`.

## Abrir a revisão completa

1. Feche Unity e preserve sua cópia atual, especialmente gravações e mudanças próprias.
2. Extraia o ZIP v0.7 em uma pasta nova e abra `OlhosDeCacador` com **Unity 6000.6.4f1**.
3. Aguarde **URP 17.6.0** e **Input System 1.20.1**. Se o editor solicitar ativar o backend e reiniciar, permita o reinício. Active Input Handling deve ser **Input System Package (New)**.
4. Fora do Play e após importação/compilação, execute **Olhos de Cacador > 1. Preparar projeto** e **2. Verificar conteudo**.
5. Limpe o Console, abra `Assets/Hunter/Scenes/Main.unity` e dê Play. Escolha um brinquedo no quarto e teste placas e desafios.

Para levar suas gravações à nova cópia, copie os áudios e seus `.meta`, além do `NarrationBank.asset` preenchido e seu `.meta`. Os nove IDs e textos continuam iguais. Não substitua o catálogo novo pelo antigo: passagens, câmeras, quarto e desafios 3D dependem dos novos campos.

## Corrigir somente os avisos na cópia existente

`Tools/repair_importer_metadata.py` funciona com Python 3, sem bibliotecas extras. Copie-o para Tools da sua cópia e execute a partir da pasta do projeto, **com Unity fechado**:

```bash
python Tools/repair_importer_metadata.py
python Tools/repair_importer_metadata.py --check
```

O script considera somente modelos e efeitos de `Docs/asset-manifest.json`; preserva GUIDs e não escreve na arte. Metadados com versão suportada são deixados intactos. Antes de substituir um bloco incompleto/antigo, salva a cópia em `ImporterMetadataBackup`, fora de Assets. Na segunda execução estável, deve informar zero reparos. `--check` apenas verifica e falha se restar importador sem versão suportada.

Reabra Unity, espere a importação terminar e execute Preparar projeto / Verificar conteudo. Esse reparo sozinho não instala a revisão visual.

## Atualizar a revisão visual numa cópia personalizada

Com backup e Unity fechado, compare suas alterações antes de substituir:

- `Assets/Hunter/Runtime/`: mudanças de fluxo, composição e animação, novos `WorldHotspot.cs` e `HunterGame.World.cs` e atualização de `Domain/GameData.cs`. Copie também os `.meta` dos dois arquivos novos; mantenha os GUIDs existentes.
- `Assets/Hunter/Editor/HunterBuild.cs`: confere quarto, passagens, peças e versões dos importadores, além das verificações anteriores.
- `Assets/Resources/Hunter/catalog.json`: novas composições, câmeras, quarto, passagens e desafios. Preserve uma cópia caso tenha criado cenários próprios; combine essas alterações com o novo formato.
- `Tools/`, `README.md` e `Docs/`: validadores e instruções atualizados.
- Execute o reparo de metadados acima antes de reabrir. Preparar projeto aplica a versão de produto 0.7.0.

Materiais e configuração de Input System/URP permanecem os da v0.6. Não é necessário apagar `.meta`, recriar GUIDs, apagar o banco de narração ou executar Reimport All repetidamente. Se sua cópia é anterior à v0.6, use o pacote completo para incluir também as correções de textura 2D, reimportação e Input System.

## Conferência feita e limite

Os 90 blocos passaram na verificação de versão. Os 190 arquivos originais conferem com seus hashes; GUIDs anteriores, IDs de progresso e roteiros foram comparados com a v0.6. A sintaxe dos 13 arquivos C# foi analisada. Isso não equivale a compilar/importar no Unity: este ambiente não tem o editor instalado. A ausência dos avisos deve ser confirmada no Console do seu editor.

Referências: [Unity Boat Attack, ModelImporter](https://github.com/Unity-Technologies/BoatAttack/blob/master/Assets/Objects/UI/meshes/checkpoint.fbx.meta) e [AudioImporter](https://github.com/Unity-Technologies/BoatAttack/blob/master/Assets/Audio/Music/MainTrack.wav.meta). As configurações foram adaptadas para OBJ estáticos e efeitos do jogo; nenhum asset de Boat Attack foi adicionado.
