# Auditoria dos modos de jogo e da HUD

## Diagnóstico geral

Os cinco modos compartilham a mesma cena e a mesma `GameHUDView`. Antes da correção, a HUD permanecia visualmente parecida em todos os desafios: o card central podia continuar com a semântica de movimentos mesmo quando o modo não usava jogadas, o pergaminho de objetivos aparecia vazio em modos sem metas, o card de progresso não distinguia score, XP, objetivos e tempo, e o poder `+5` continuava disponível onde não produzia efeito. No Contra o Relógio, a duração estava duplicada entre a regra e o texto visual, o que permitia que o cronômetro real e a interface ficassem diferentes.

## Matriz final de regras e interface

| Modo | Regra principal | Card central | Card de progresso | Objetivos | `+5` movimentos |
|---|---|---|---|---|---|
| **Campanha** | Fase carregada de `LevelData`, com movimentos, obstáculos e estrelas configuráveis. | `MOVIMENTOS` com o contador restante. | `PROGRESSO` com score atual/meta da fase. | Pergaminho visível com os objetivos da fase; fases sem metas recebem fallback bíblico. | Ativo. |
| **Estudo Infinito** | Pontuação contínua, variedade de peças e obstáculos escalonados; sem encerramento por movimentos. | `COMBO`, sem contador de jogadas. | `XP` e pontuação acumulada. | Pergaminho oculto. | Oculto e bloqueado. |
| **Desafio Diário** | Semente determinística do dia, 30 movimentos e missão renovada diariamente. | `MOVIMENTOS`. | `PROGRESSO` com score/meta. | Pergaminho visível com a missão. | Ativo. |
| **Contra o Relógio** | Duração configurável, inicialmente 90 segundos; o tempo encerra a sessão. | `COMBO`, sem contador de jogadas. | `TEMPO`, barra proporcional ao tempo restante, relógio `mm:ss` e segundos restantes. | Pergaminho oculto. | Oculto e bloqueado. |
| **Guardião da Palavra** | 35 movimentos e objetivos de Pão e Peixe; a fase vence ao completar todos. | `MOVIMENTOS`. | `OBJETIVOS`, barra agregada e quantidade atual/total. | Pergaminho visível com os objetivos da Palavra. | Ativo. |

## Correções de gameplay

O `GameManager` passou a expor explicitamente se o modo usa limite de movimentos. O `BoardManager` somente chama `ScoreAndObjectiveManager.UseMove()` em Campanha, Desafio Diário e Guardião da Palavra. Portanto, uma troca válida no Estudo Infinito ou no Contra o Relógio não reduz mais um contador artificial de jogadas.

A duração do Contra o Relógio agora é um único campo configurável, `duracaoContraRelogio`, com valor inicial de 90 segundos. O gameplay e a HUD usam essa mesma fonte. O título do modo passa a exibir a duração configurada, e a barra de progresso usa `tempo restante / duração total`. O fim do tempo continua acionando a derrota por `EncerrarPorTempo()`.

O `+5` foi protegido em duas camadas. A HUD o oculta em Estudo Infinito e Contra o Relógio, e o controlador recusa chamadas externas nesses modos. O `ScoreAndObjectiveManager` também ignora adição de movimentos quando o contador está em `int.MaxValue`, evitando overflow ou alteração sem significado.

## Correções de interface

A HUD agora altera rótulos, visibilidade e mensagens conforme o modo. O card central passa de `MOVIMENTOS` para `COMBO` nos modos contínuo e temporizado. O pergaminho de objetivos é ocultado em Estudo Infinito e Contra o Relógio. A estrela isolada que aparecia no card de Progresso foi removida; ela não representava uma métrica válida. A progressão agora é score/meta na Campanha e no Diário, XP no Estudo Infinito, tempo restante no Contra o Relógio e objetivos atual/total no Guardião.

As mensagens inferiores também ficaram específicas: o Relógio informa que é necessário fazer combos antes do tempo acabar, o Infinito orienta o ganho de XP, o Diário informa a missão do dia, o Guardião orienta o cumprimento dos objetivos e a Campanha mantém a orientação de jogada especial.

## Validação

A cena foi remontada por `MontarTudoAutomaticoBatch` e salva em 21 de agosto de 2026 às 12:23:38, com 1.186.675 bytes. O log final registrou `Cena remontada automaticamente`, `Montagem batch concluída e SampleScene salva` e `Exiting batchmode successfully now`, sem `error CS`, `Compilation failed` ou exceção do montador.

A validação estrutural confirmou que a cena contém o campo `Tempo`, não contém mais `EstrelaMeta`, possui o painel de objetivos, o botão `+5` e a referência do `GameManager` no `BoardManager`. O Test Runner do Unity iniciou a importação e encerrou em modo batch, mas o ambiente não produziu o XML de resultados; por isso, não é correto declarar os testes automatizados como aprovados. A compilação e a remontagem batch, porém, foram concluídas sem erros do projeto.

## Roteiro de teste visual

No Game View portrait, iniciar cada modo pelo mapa e verificar a matriz acima. No Contra o Relógio, confirmar que o título exibe a duração configurada, o card central exibe `COMBO`, o card direito exibe `TEMPO`, o relógio começa em `01:30`, a barra diminui e a tela de derrota aparece ao chegar a zero. No Estudo Infinito, confirmar `COMBO`, `XP`, pergaminho oculto e ausência do `+5`. No Diário e no Guardião, confirmar que movimentos, metas, barra e `+5` permanecem visíveis e funcionais. Na Campanha, confirmar que os obstáculos e metas do `LevelData` continuam sendo carregados.
