# Refinamento V2 do tabuleiro do Maná

## Diagnóstico da captura

A captura mostrava que a composição geral já estava próxima, mas ainda havia quatro problemas visuais importantes: o banner de objetivos aparecia sem itens, o cabeçalho estava compacto, o progresso era estático e os power-ups pareciam retângulos escuros sem medalhão. A nova versão trata esses pontos sem alterar as regras de match-3.

## Alterações aplicadas

A faixa de objetivos agora recebe metas padrão de demonstração para a abertura da Campanha — pão 20, pomba 14 e azeite 18 — quando o `LevelData` ainda não possui objetivos. Fases reais continuam substituindo esses valores por meio de `Configurar()`. A `GameHUDView` também reemite o estado depois de assinar os eventos, evitando que os objetivos desapareçam por causa da ordem de `OnEnable` entre Controller e View.

Os itens de objetivo agora possuem `LayoutElement`, largura preferencial de 148 unidades, fundo de pergaminho claro, ícone grande e contador horizontal. Isso corrige a faixa vazia e preserva a expansão correta dentro do `HorizontalLayoutGroup`.

O cabeçalho recebeu logo `MANÁ` maior com outline, subtítulo ampliado, badge de versículo `SALMOS 23:1`, três estrelas no cartão de pontos e cartão de progresso com barra preenchida dinamicamente. O texto de progresso apresenta o valor atual e a meta no formato `atual / 20.000`.

A barra de XP passou a usar `Image.Type.Filled` e está ligada ao score por meio de `GameHUDView`. Os três power-ups receberam medalhões circulares maiores, badge numérico de usos restantes, descrições ao lado e fundos ornamentais.

Cada uma das 64 células agora possui um slot azul-marinho individual em `SlotsDasCelulas`, atrás da peça e à frente da moldura. O fundo bíblico foi ampliado por largura e altura para reduzir áreas pretas no portrait. A entrada, colliders e tamanho lógico do tabuleiro continuam sob responsabilidade do `BoardManager`.

## Validação

A `SampleScene.unity` foi remontada e salva em 21 de agosto de 2026 às 07:57:06, com 1193850 bytes. O arquivo contém `VersiculoBadge`, `Estrelas`, `SlotsDasCelulas`, `MetaProgresso`, `BotaoMartelo`, `BotaoEmbaralhar`, `BotaoMaisMovimentos`, `FundoBiblico` e `MolduraTabuleiro`. A resolução de referência continua em `1080 x 1440`.

O log recente do Unity não apresentou `error CS`, `Compilation failed` ou `Could not register callback class null`. Os scripts temporários de montagem foram removidos após o salvamento.

## Teste final recomendado

Abra o Game View no formato portrait de `1080 x 1440` e pressione Play. Entre na Campanha e confirme a presença dos três objetivos, das estrelas, do badge de versículo, da barra de progresso, dos slots individuais e dos medalhões de poder. Em seguida, toque ou arraste duas peças vizinhas e confirme que a seleção continua respondendo; por fim, teste os cinco modos e os três poderes.
