# Atualizar para v0.9 — maquetes e passagens

Unity **6000.6.4f1**, Windows x64, URP 17.6.0 e Input System 1.20.1 (New) permanecem iguais.

1. Preserve sua cópia anterior, inclusive gravações e cenas editadas.
2. Extraia esta revisão em uma pasta nova e abra-a no editor indicado.
3. Aguarde a importação; abra Main e execute **Olhos de Cacador > 1. Preparar projeto**, depois **2. Verificar conteudo**.
4. A primeira preparação monta as hierarquias das 17 cenas de conteúdo. Inicie por Main para testar a progressão normal.

As 18 cenas e seus GUIDs foram preservados. As composições mudaram: não copie cenas já montadas da v0.8 sobre as novas, pois elas contêm os antigos seletores. Caso combine alterações manualmente, use o catálogo e os dados novos de passagens. Preparar projeto preserva cenas já montadas; **7. Recriar cenas a partir do catalogo** substitui suas alterações de composição após confirmação explícita.

## O que muda

- Bases de maquete, patamares, rios, ruas e construções, com arranjos diferentes nas nove buscas.
- Circuitos com passagens físicas, retornos e atalhos. Foram removidos os botões de destinos, o botão Caminho e o seletor de peças da antiga navegação.
- Os seis mecanismos agora estão nas buscas. Complete a charada e clique no mecanismo: a peça correta encontrada é usada, aparece montada e o brinquedo se move.
- Roda do mouse amplia a região apontada; botão direito restaura a visão geral. A aproximação não depende de detectar um alvo.
- O texto do mouse é igual sobre alvos e decoração comum. Apenas passagens e mecanismos mostram sua função.
- Modelo original Rubber Duck Toy, de Plat251/Poly Haven, e seis modelos adicionais dos packs Kenney já usados. Os arquivos artísticos não foram alterados.

## Save e voz

Os 54 IDs e a chave OlhosDeCacador.v1 continuam iguais. Descobertas, selos e mecanismos abertos da v0.8 são mantidos. Reinício parcial ainda limpa somente o mundo escolhido. Para experimentar todos os mecanismos novos, use Recomeçar este mapa em uma cópia de teste.

Somente o último verso de `faroeste_estacao` mudou: agora pede um pato de borracha. O ID técnico `west_bear` é conservado para compatibilidade de save, mas seu nome visível e modelo correspondem ao pato. Regrave a charada da estação; as outras oito gravações da v0.8 podem ser reaproveitadas. Veja NARRACAO.md.

## Assets e importação

São 147 OBJ e um FBX original, com 264 vínculos de materiais URP. O FBX do pato usa sua textura difusa original e normais importadas; o importador e o reparo agora também tratam FBX/JPG/EXR. Os 148 ModelImporters possuem blocos completos; os sete AudioImporters antigos continuam corrigidos. Nenhum reparo de remaps é executado dentro dos callbacks de importação.

Não houve geração de modelos artísticos, texturas, sprites ou voz. As bases, caminhos e volumes arquitetônicos usam apenas cubos, esferas e cilindros. Não há build Windows incluído: compilação, importação real e Play ainda precisam ser executados no Unity.
