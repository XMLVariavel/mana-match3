# Avaliação do estado atual e plano de melhoria

## Diagnóstico

O projeto é um jogo Unity 2D de match-3 com base funcional relevante: há separação entre `BoardManager`, `MatchDetector`, `BoardPhysics`, `GameManager`, `GameHUDController` e views de UI, além de testes EditMode e PlayMode. Também existe um montador de cena por código de Editor, o que é uma boa decisão para manter referências da Unity válidas.

O problema visual mais grave da tela enviada é determinístico e está na relação entre **escala da arte e unidade da grade**. O tabuleiro usa `cellSize = 1`, mas o sprite de pão é importado com `spritePixelsPerUnit = 256` e uma área útil de aproximadamente 887x971 pixels. Portanto, o sprite ocupa aproximadamente 3,46x3,79 unidades, enquanto cada célula tem apenas 1 unidade. O resultado é exatamente a sobreposição de peças vista na captura: a lógica pode estar criando uma grade correta, mas a arte ultrapassa a célula.

A interface também está montada com muitos blocos escuros/dourados iguais e textos pequenos, sem uma diferenciação forte entre **Campanha** e **Estudo Infinito**. O rodapé usa rótulos simples para poderes, mas não comunica bem ícone, quantidade, custo e estado. A tela de jogo precisa de um cabeçalho mais compacto, objetivos em chips e um painel de tabuleiro que separe visualmente a área jogável do fundo.

## O que será corrigido

| Área | Situação observada | Correção planejada |
|---|---|---|
| Escala das peças | Sprite maior que a célula, causando sobreposição | Normalização visual em `Tile` por célula, mantendo collider 1x1 |
| Tabuleiro | Falta de moldura, respiro e leitura de célula | Painel de fundo e espaçamento visual consistente |
| HUD | Score, movimentos e objetivos pouco hierarquizados | Cabeçalho compacto, chips de objetivo e progressão de score |
| Poderes | Botões com texto puro e pouca informação de estado | Ícone, rótulo, contador, estados ativo/desabilitado e feedback de seleção |
| Modos | Campanha e Infinito têm entrada funcional, mas pouca distinção visual | Campanha orientada a fases/estrelas; Infinito orientado a pontuação, XP e versículos |
| Telas | Placeholder visual e repetição de painéis | Direção de pergaminho, madeira, azul-noturno e dourado, com arte temática |
| Feedback | Pouco destaque para match, cascata, vitória e versículo | Animações/feedback visuais sem alterar as regras de jogo |

## Critérios de aceite

A tela de jogo deve apresentar uma grade legível em aparelhos móveis, sem nenhuma peça visual ultrapassar significativamente a célula. Os seis tipos de peça precisam ser distinguíveis por cor e silhueta. A Campanha deve exibir fase, objetivos e estrelas; o Estudo Infinito deve exibir score, XP, escalonamento, versículo e ausência de limite fixo de movimentos. Os botões do rodapé devem continuar acionando os poderes atuais e informar visualmente quando estiverem indisponíveis.

A alteração será feita primeiro em código e prefabs gerados, evitando editar YAML de cena à mão sempre que houver um caminho de Editor existente. Depois, o projeto deve ser aberto no Unity, a cena deve ser remontada pelo menu `Tools > Maná > Montar cena completa`, e o fluxo deve ser verificado em Play Mode.
