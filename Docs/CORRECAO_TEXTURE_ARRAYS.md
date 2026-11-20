# Correção dos materiais — 20/11/2026

Diagnóstico baseado no `Editor.log` e no `Bem-Te-Vi.7z` anexados.

## Causa confirmada

19 materiais de `holiday-kit` (14), `train-kit` (3) e `brick-kit` (2)
tinham sua textura de cor 2D atribuída a todos os campos de textura.
Isso incluía `unity_Lightmaps`, `unity_LightmapsInd` e `unity_ShadowMasks`,
que o shader URP/Lit declara como `2DArray`. O defeito também estava na
entrega anterior; não foi causado pela aplicação do patch pelo usuário.

O log contém 66 mensagens de dimensão incompatível: 57 durante
`BemTeViMaterialRepair.Repair` e 9 durante a renderização. A primeira
consulta `Material.HasProperty` inicializa/valida os dados do material e
expõe os campos inválidos, embora o código consultasse `_MainTex`.

## Alterações

- Corrigidos 19 arquivos `.mat`: 57 vínculos indevidos nos campos de
  iluminação e 171 nos campos de normal, relevo, oclusão, emissão,
  detalhes e brilho. `_BaseMap` e `_MainTex` mantêm a textura original.
- Removidas 95 ativações de recursos de shader associadas aos mapas
  indevidos. Cores, brilho numérico, texturas, modelos e GUIDs preservados.
- `Restaurar materiais originais` agora limpa os campos serializados
  antes de consultar as propriedades do material. O comando restaura
  os materiais do manifesto ao contrato original de cor/textura difusa.
  Personalizações de mapas adicionais nesses materiais serão removidas
  ao executar esse comando, como parte da restauração.
- A verificação no editor e a validação Python passam a detectar esse
  erro. Adicionado um teste de integração para os materiais importados.

## Aplicação recomendada

1. Feche o Unity.
2. Extraia `Bem-Te-Vi_correcao_texturas.zip` na raiz do seu projeto,
   onde estão `Assets`, `Packages` e `ProjectSettings`. Mescle as pastas
   e substitua somente os arquivos presentes no ZIP.
3. Reabra o projeto e aguarde a importação. Os 19 materiais já estão
   corrigidos nos arquivos; a reimportação aplica a correção.
4. Limpe o Console, abra `Assets/BemTeVi/Scenes/Main.unity` e entre em Play.
   Visite os três mundos para verificar se surgem mensagens novas.

O ZIP de correção e o patch contêm a mesma atualização; escolha um deles.
O patch foi gerado contra o projeto anexado, não contra uma cópia antiga
do repositório. Para usar o patch na raiz desse projeto:

```sh
git apply --check Bem-Te-Vi_correcao_texturas.patch
git apply Bem-Te-Vi_correcao_texturas.patch
```

O arquivo `Bem-Te-Vi_projeto_corrigido.zip` é uma alternativa com o projeto
completo baseado no anexo. Abra-o como projeto separado. Não contém caches
do editor, logs, configurações pessoais do editor ou histórico Git.

## Verificação realizada

- A nova validação rejeita os 19 materiais do anexo e aprova todos os
  293 vínculos de materiais da cópia corrigida.
- Análise sintática dos 25 arquivos C# passou. Isso não substitui uma
  compilação no Unity.
- GUIDs e vínculos de cor preservados; cenas e arte original mantidas
  byte a byte em relação ao anexo.
- A validação geral `Tools/validate_content.py` ainda para na auditoria
  de hashes de modelos do anexo: 202 arquivos já divergiam do manifesto
  antes desta correção, dos quais 52 apenas por finais de linha. A causa
  dos outros hashes não foi investigada nesta correção de materiais.
  Nenhum desses modelos foi alterado aqui.
- Unity não está instalado neste ambiente: compilação, teste NUnit e
  Play Mode precisam ser executados no editor do usuário.

Validação específica independente do Unity:

```sh
python Tools/validate_material_textures.py
```

Referência do shader oficial:
https://github.com/Unity-Technologies/Graphics/blob/master/Packages/com.unity.render-pipelines.universal/Shaders/Lit.shader
