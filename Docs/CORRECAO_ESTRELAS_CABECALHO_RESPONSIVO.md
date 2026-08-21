# Correção das estrelas e do cabeçalho responsivo

## Diagnóstico

O quadrado amarelo não era um marcador de layout da Unity. Ele era o quadrado de fallback do TextMeshPro: o texto `★` estava sendo enviado para a fonte `LiberationSans SDF`, que não possui esse caractere. O console confirmava o diagnóstico com o aviso de Unicode `U+2605` ausente nos objetos `Estrela1`, `Estrela2`, `Estrela3` e `EstrelaProgresso`.

O logo também podia ficar parcialmente escondido em aparelhos com recorte superior porque a tela de jogo não aplicava o componente de área segura ao seu cabeçalho, embora as outras telas já tivessem essa proteção.

## Correções aplicadas

Foi criado `Assets/Scripts/UI/StarGraphic.cs`, um componente gráfico que desenha uma estrela de cinco pontas diretamente na malha da UI. Assim, as estrelas não dependem de glyphs da fonte e não produzem quadrados amarelos. Foi criado também `StarRatingView.cs`, responsável por controlar a quantidade de estrelas preenchidas e por atualizar a avaliação no painel de vitória.

O card **Pontos** agora contém três estrelas vetoriais douradas abaixo do número. As estrelas permanecem visíveis durante o runtime e podem ser atualizadas sem alterar o texto do TextMeshPro. O card **Progresso** deixou de usar uma estrela textual ao lado da barra, porque o requisito desse card é a barra preenchível com o valor atual e a meta.

A tela de vitória também passou a usar estrelas vetoriais, mantendo o resultado de uma, duas ou três estrelas sem gerar avisos de fonte.

A tela de jogo recebeu `SafeAreaHeader`. A altura da área superior foi ampliada para 480 unidades e os blocos foram separados verticalmente: logo e subtítulo na parte superior; cards entre `-340` e `-182`; modo entre `-374` e `-346`; objetivos entre `-470` e `-380`. A barra de progresso foi ampliada horizontalmente porque não precisa mais reservar espaço para o quadrado/estrela textual.

## Validação realizada

A cena foi remontada em modo batch e salva em 21 de agosto de 2026 às 10:23:36, com 1.206.457 bytes. O log registrou `Cena remontada automaticamente`, `Montagem batch concluída e SampleScene salva` e `Exiting batchmode successfully now`.

A validação estática da cena confirmou a presença de `StarGraphic`, `StarRatingView`, `SafeAreaHeader` e do cabeçalho com 480 unidades. A cena não contém mais os glyphs `★` ou `☆`. Não foram encontrados erros `error CS`, `Compilation failed` ou falha de execução do método batch.

Os avisos `No script asset for Efeito...SO` encontrados no log são referências de dados especiais já existentes no projeto, não estão relacionados ao quadrado amarelo nem às estrelas do HUD e não impediram a remontagem da cena.

## Teste no Unity

Abra o Game View em portrait e valide 1080 × 1440, 1080 × 1920 e 720 × 1280. Em um aparelho ou simulador com notch, confirme que o logo `MANÁ` fica abaixo do recorte, que as três estrelas douradas aparecem abaixo do número de pontos, que o card de progresso mostra apenas a barra e `atual / meta`, e que a faixa de objetivos fica abaixo do modo sem tocar nos cards.
