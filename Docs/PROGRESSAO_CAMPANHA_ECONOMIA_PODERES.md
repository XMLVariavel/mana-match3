# Progressão da Campanha e economia de poderes

## Progressão da Campanha

A Campanha agora possui um fluxo explícito de avanço. Ao concluir uma fase, o resultado registra as estrelas no `PlayerProgress`, a próxima fase é liberada no mapa e o painel de vitória mostra o botão `Próxima fase` quando existe um nível seguinte. O botão inicia a próxima fase diretamente, respeitando o consumo de vida. Se a fase concluída for a última da lista, o botão não é exibido e permanece disponível o retorno ao mapa.

A recompensa da vitória é exibida no resumo. O jogador recebe `10 moedas por estrela` e, na Campanha, recebe também até `20 moedas` por eficiência, calculadas em função dos movimentos restantes. Assim, três estrelas e muitos movimentos preservados rendem uma recompensa maior sem permitir valores exagerados.

## Economia dos poderes

Os poderes avulsos Martelo, Embaralhar e +5 passaram de cobrança direta a cada uso para um sistema de estoque. Uma conta nova recebe `3 unidades de cada poder` como kit inicial. Usar um poder consome uma unidade do estoque, não moedas. Quando o estoque termina, a HUD informa que o jogador deve comprar um pacote na Loja.

Cada pacote compra `3 unidades`. Os preços atuais são Martelo: `50 moedas`, Embaralhar: `75 moedas` e +5 Movimentos: `100 moedas`. O saldo de moedas é persistido no `PlayerProgress` e o estoque também é sincronizado com Firebase.

| Ação | Efeito |
|---|---|
| Vencer uma fase | Recebe moedas por estrelas e, na Campanha, bônus por movimentos restantes. |
| Abrir a Loja | Visualiza saldo, estoque, preço e quantidade do pacote. |
| Comprar pacote | Gasta moedas e adiciona 3 unidades ao estoque. |
| Usar na partida | Gasta uma unidade do poder e atualiza o badge imediatamente. |
| Sem estoque | O jogo explica que é necessário comprar um pacote. |
| Contra o Relógio/Estudo Infinito | +5 permanece bloqueado porque esses modos não usam limite de movimentos. |

A Loja agora mostra a instrução: ganhe moedas vencendo fases, compre pacotes na Loja e use os poderes na tela de jogo. A HUD mostra a quantidade real disponível nos badges dos três poderes, substituindo o número fixo que aparecia anteriormente.

## Validação

A cena foi remontada em 21 de agosto de 2026 às 14:05:18, com 1.222.212 bytes. O log registrou a remontagem e o salvamento batch concluídos com sucesso, sem erros C# ou falha de compilação. A validação estrutural confirmou `BotaoProximaFase`, detalhes de recompensa, contadores de estoque, instruções da Loja, catálogo `configsAvulsos` ligado e referência do mapa na HUD.

## Teste manual recomendado

Conclua a Fase 1 com pelo menos uma estrela e confirme o resumo de moedas e o botão `Próxima fase`. Toque nesse botão e confirme a abertura da Fase 2. Abra a Loja e confirme os preços e o estoque inicial de três unidades. Use o Martelo em uma peça e verifique que o badge diminui para dois. Compre um pacote quando houver saldo e confirme que o estoque aumenta em três. Termine uma fase com três estrelas e compare a recompensa com uma vitória de uma estrela. No Contra o Relógio e no Estudo Infinito, confirme que o +5 permanece oculto ou bloqueado.
