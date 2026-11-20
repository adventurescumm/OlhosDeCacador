# Atualizar para v0.10 — olhar de perto

Unity **6000.6.4f1**, Windows x64, URP 17.6.0 e Input System 1.20.1 permanecem iguais.

## Instalar

1. Preserve sua pasta anterior, principalmente cenas editadas e gravações.
2. Extraia o ZIP em uma pasta nova e abra `BemTeVi` no Unity Hub.
3. Aguarde pacotes/importação e execute **Bem-Te-Vi → 1. Preparar projeto**, depois **2. Verificar conteudo**.
4. Abra Main e dê Play. Confira as buscas e as passagens antes de compilar Windows.

Esta revisão modifica scripts, fontes, sons, catálogo e câmeras das cenas. Copiar apenas BemTeViBuild.cs aplica a correção histórica do FBX, mas não instala a v0.10.

As 18 cenas e GUIDs foram mantidos. O pacote contém suas configurações iniciais; Preparar projeto monta as hierarquias no Unity. Não copie cenas já montadas da versão anterior sobre as novas: elas conservariam as câmeras antigas e não teriam os novos bonecos. Preparar projeto continua preservando cenas já montadas. Para atualizar uma cópia existente sem edições próprias, o comando **7. Recriar cenas a partir do catalogo** reconstrói as composições após sua confirmação. Se houver edições próprias, compare o catálogo e os enquadramentos antes de usar esse comando.

## Mudanças

- Câmeras próximas e fixas. Roda do mouse e botão direito não alteram o enquadramento; não há deslocamento automático após uma descoberta.
- Nas buscas, o cenário ocupa 760 dos 900 pixels da referência de interface. A faixa inferior guarda somente os seis versos, com marca e cor nas descobertas.
- Um botão Menu reúne leitura, voz, dicas por verso, caderno, opções e pausa. Entradas dos mundos usam o cenário inteiro.
- Fonte Itim no texto e Boogaloo nos títulos/botões, ambas prontas e sob SIL OFL 1.1.
- Dez modelos Mini Characters prontos, distribuídos em 26 bonecos/bonecas decorativos. Eles reagem ao clique, mas não acrescentam alvos obrigatórios.
- 21 gravações prontas adicionais, distribuídas por objetos e cenários. Nenhum áudio foi gerado ou editado.
- No desafio espacial, clique no rádio da frente para iniciar ou repetir a sequência; a faixa inferior fica apenas com instruções.

## Saves e narração

Os 54 IDs, as seis passagens e a chave de save continuam iguais. Os nove roteiros e o banco de narração não mudaram em relação à v0.9. Reaproveite as gravações; veja [NARRACAO.md](NARRACAO.md) para os novos caminhos dos controles.

A correção do carregamento OBJ/FBX e a inclusão das 18 cenas no comando de build foram preservadas. Não renomeie o pato FBX para OBJ.

## Limites da entrega

Foram executadas checagens de dados, hashes, importadores, sintaxe, lógica, áudio e visibilidade externa. As composições foram inspecionadas com os modelos originais em Blender; essas prévias não são capturas do jogo. O Unity não está instalado neste ambiente. Importação real, compilação completa, mixagem percebida, Play, desempenho e executável Windows ainda precisam ser testados no editor. Roteiro: [VALIDACAO.md](VALIDACAO.md).
