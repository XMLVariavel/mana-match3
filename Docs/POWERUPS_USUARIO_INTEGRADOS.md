# Power-ups fornecidos pelo usuário — integração final

Os três ícones enviados pelo usuário foram incorporados à biblioteca BoardV2 do Maná:

| Asset final | Uso no jogo |
|---|---|
| `power_hammer.png` | Botão `BotaoMartelo` |
| `power_shuffle.png` | Botão `BotaoEmbaralhar` |
| `power_plus5.png` | Botão `BotaoMaisMovimentos` |

O fundo branco das imagens foi removido, as bordas foram convertidas para transparência e os medalhões foram recortados para uso como sprites quadrados. O sistema mantém os medalhões de madeira/verde, os símbolos dourados e o pequeno badge circular de usos restantes.

A integração utiliza `ManaArte.Carregar()` pelos nomes `power_hammer`, `power_shuffle` e `power_plus5`. `ManaUI.BotaoPoder()` cria a arte do usuário dentro do medalhão e usa o símbolo textual somente como fallback caso o PNG não seja localizado.

A `SampleScene.unity` foi remontada e salva às 09:37:03 de 21 de agosto de 2026, com 1200281 bytes. O Unity abriu fora do SAFE MODE e a validação recente não encontrou `error CS`, `Compilation failed` ou `Could not register callback class null`.

Para conferir, abra o Game View em portrait, pressione Play e observe a barra inferior. Os três medalhões devem aparecer na ordem: Martelo, Embaralhar e +5 Movimentos. O toque continua ligado aos mesmos métodos do `GameHUDController`, portanto a substituição visual não altera a lógica dos poderes.
