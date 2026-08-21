# Correção de sobreposição dos cards superiores

## Problema identificado

A captura mostrava os cards `PONTOS`, `MOVIMENTOS` e `PROGRESSO` ancorados por porcentagem vertical dentro de um cabeçalho que também continha logo, subtítulo e badge. Em diferentes alturas de tela, os cards subiam sobre o logo e o painel de fundo, causando a sobreposição observada.

## Solução aplicada

Os três cards agora usam uma faixa vertical fixa dentro do cabeçalho, ancorada no topo por pixels de referência, com margens horizontais proporcionais. Cada card possui uma única faixa vertical entre os offsets `-312` e `-166`, mantendo altura de 146 unidades e separação horizontal entre os três blocos.

O modo e a faixa de objetivos também passaram a usar anchors superiores fixos. A faixa de objetivos ocupa os offsets `-416` a `-332`, sempre abaixo dos cards. O logo e o subtítulo ficam na região superior do cabeçalho, enquanto o botão de voltar e o versículo permanecem presos aos cantos. Dessa forma, os quatro grupos não dependem mais de porcentagens verticais que variam com o tamanho do celular.

O tabuleiro começa abaixo da área do cabeçalho e os colliders continuam independentes da UI. A resolução de referência permanece `1080 × 1440`, com `CanvasScaler` configurado para escalar proporcionalmente.

## Validação

A `SampleScene.unity` foi remontada e salva às 09:55:14 de 21 de agosto de 2026, com 1204452 bytes. O arquivo contém `BarraSuperior`, `CartaoScore`, `CartaoMovimentos`, `CartaoProgresso` e `Objetivos`. O log recente do Unity não apresentou `SAFE MODE`, `error CS`, `Compilation failed` ou `Could not register callback class null`.

## Teste recomendado

No Unity, abra o Game View e teste a cena em portrait usando pelo menos três proporções: 1080 × 1440, 1080 × 1920 e 720 × 1280. Confirme que o logo não é coberto pelos cards, que os três cards permanecem em uma linha, que a faixa de objetivos fica abaixo deles e que o tabuleiro começa apenas depois do cabeçalho. Em aparelhos com notch, confirme que o botão de voltar e o badge de versículo continuam dentro da área segura.
