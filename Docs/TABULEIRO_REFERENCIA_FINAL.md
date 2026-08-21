# Refinamento do tabuleiro do Maná

## Objetivo

O tabuleiro foi ajustado para seguir a referência bíblica premium fornecida: cabeçalho ornamentado, identidade `MANÁ`, cartões de pontos e movimentos, cartão de progresso, faixa de objetivos em pergaminho, moldura maior, peças mais preenchidas e barra inferior de poderes.

## Alterações aplicadas

A montagem de `MontadorDeUI.cs` agora cria uma HUD superior de aproximadamente 292 unidades, com a marca `MANÁ`, o subtítulo `BUSQUE O CÉU, VIVA A PALAVRA`, botão de retorno interno, cartões ornamentados para pontos, movimentos e progresso, barra de XP visual, modo atual e faixa de objetivos.

A barra inferior passou para aproximadamente 230 unidades de altura e utiliza três componentes `BotaoPoder`: `MARTELO`, `EMBARALHAR` e `+5 MOV.`. Cada poder possui círculo colorido de destaque, símbolo, descrição e custo, mantendo os callbacks existentes do `GameHUDController`.

O prefab `ItemObjetivo` passou a utilizar cápsula visual de pergaminho, ícone maior e contador alinhado horizontalmente. O tabuleiro lógico foi ampliado para células de `1.08` unidade, deslocado para ocupar a área central entre a HUD e os poderes, e a moldura foi ampliada para aproximadamente `9.5` unidades.

A câmera adaptativa foi recalibrada para a largura da nova moldura. Foi adicionado o fundo bíblico da Jornada atrás do tabuleiro, sem interferir nos colliders ou na entrada. As peças passaram de `82%` para `90%` de ocupação visual da célula; o collider continua sendo compensado para cobrir a célula lógica inteira.

## Validação técnica

A `SampleScene.unity` foi remontada e salva em 21 de agosto de 2026 às 02:31:59, com 984263 bytes. O arquivo persistido contém `CartaoScore`, `CartaoMovimentos`, `CartaoProgresso`, `BarraXP`, `Objetivos`, `BotaoMartelo`, `BotaoEmbaralhar`, `BotaoMaisMovimentos`, `FundoBiblico`, `MolduraTabuleiro` e a resolução de referência `1080 x 1440`.

A compilação recente não apresentou erros `CS`, `Compilation failed` ou `Could not register callback class null`. A cena permaneceu aberta no Unity Editor após a remontagem.

## Teste recomendado

No Unity Editor, abra o Game View em portrait com referência `1080 x 1440` e pressione Play. Entre na Campanha e confira a composição vertical. Depois teste o toque ou arraste nas peças, os três poderes, a atualização de objetivos, a vitória/derrota e os cinco modos de jogo. A validação visual final deve ser feita no dispositivo Android ou no Simulator, pois a aparência pode variar conforme a resolução e o Safe Area.
