# Implementação das melhorias dos modos de jogo

## Escopo entregue

Foram implementadas melhorias de arquitetura, competitividade e comunicação visual nos cinco modos do Maná. A sessão agora carrega uma definição própria de desafio, com `ChallengeId`, seed, movimentos, duração, metas, objetivos, briefing e flags de regra. Isso evita que a Campanha, o Diário, o Infinito, o Relógio e o Guardião sejam tratados como a mesma partida.

## Arquivos novos

| Arquivo | Responsabilidade |
|---|---|
| `Assets/Scripts/ModeChallengeDefinition.cs` | Define as regras runtime de cada desafio, incluindo variantes diárias e semanais. |
| `Assets/Scripts/CompetitiveScoreRules.cs` | Calcula multiplicador de combo, bônus de velocidade, bônus de sequência e bônus de tempo final. |
| `Assets/Tests/EditMode/ModeChallengeDefinitionTests.cs` | Testa determinismo diário, duração do Relógio e regras de bônus. |

## Gameplay implementado

O `GameManager` agora usa uma única duração configurável para o Contra o Relógio e decrementa o tempo usando `Time.unscaledDeltaTime`, evitando discrepâncias causadas por escala de tempo. O timer é ativado somente nesse modo e é desligado antes de encerrar a partida quando chega a zero.

O `BoardManager` registra a troca válida para calcular combo e velocidade. O `BoardPhysics` aplica no Contra o Relógio a pontuação base de 10 pontos por peça normal e 30 por peça especial, multiplicada por combo e velocidade. O multiplicador de combo cresce até 3x; jogadas em até 1 segundo recebem fator 1,50, e jogadas em até 2 segundos recebem fator 1,20. Há bônus de sequência em combos 5, 10 e 15, respectivamente +50, +125 e +250 pontos.

O consumo de movimentos continua restrito à Campanha, ao Desafio Diário e ao Guardião da Palavra. Estudo Infinito e Contra o Relógio não descontam jogadas.

## Variações de desafios

O Desafio Diário utiliza uma seed derivada da data UTC e permanece igual para todos os jogadores no mesmo dia. Além de manter a reprodutibilidade, ele alterna entre três conjuntos de objetivos. O Guardião da Palavra alterna semanalmente entre objetivos de Pão/Peixe e Pomba/Azeite. A Campanha continua usando as configurações autorais de `LevelData`. O Estudo Infinito mantém progressão procedural por pontuação, variedade de peças, XP, versículos e obstáculos.

## UI/UX implementada

A HUD passou a exibir um briefing específico da sessão, altera o rótulo central entre `MOVIMENTOS` e `COMBO`, altera o card direito entre `PROGRESSO`, `XP`, `TEMPO` e `OBJETIVOS`, oculta o pergaminho quando o modo não usa metas e oculta o `+5` quando não existe limite de movimentos. A tela de resultado agora mostra estado, briefing, score, melhor combo e XP.

No Contra o Relógio, o card de tempo mostra a duração configurada em `mm:ss`, segundos restantes e barra proporcional. No Estudo Infinito, a interface prioriza combo, XP e briefing de progressão. No Diário e no Guardião, objetivos e movimentos continuam visíveis.

## Ranking

O registro do Firebase passou a aceitar `challengeId`, armazenando o resultado com modo, temporada e desafio. Isso permite separar rankings diários e variantes semanais sem sobrescrever automaticamente todos os desafios do mesmo modo na mesma temporada. Chamadas antigas continuam compatíveis por meio dos overloads existentes.

## Validação realizada

A cena foi remontada por `BibleMatch3.EditorTools.MontadorDeUI.MontarTudoAutomaticoBatch` e salva em 21 de agosto de 2026 às 12:58:28, com 1.200.250 bytes. O log final registrou a remontagem, o salvamento e `Exiting batchmode successfully now`, sem erros C# ou falha de compilação.

A validação estrutural confirmou a presença de `Briefing`, `Tempo`, `Detalhes` no resultado, referências do `GameManager` nos componentes de gameplay e ausência de `EstrelaMeta`. O Test Runner foi iniciado, mas o Unity encerrou com código 127 sem produzir XML no ambiente atual; portanto, a entrega não declara os testes automatizados como aprovados. A remontagem batch e a compilação do projeto terminaram com sucesso.

## Roteiro de teste manual

No Game View portrait, iniciar cada modo pelo mapa. No Estudo Infinito, confirmar `COMBO`, `XP`, briefing, pergaminho oculto, progressão de peças e ausência do `+5`. No Contra o Relógio, confirmar início em 90 segundos, contagem regressiva, ausência de consumo de movimentos, bônus de combo, derrota ao chegar a zero e `+5` oculto. No Diário, iniciar duas vezes no mesmo dia e verificar objetivos e configuração iguais. No Guardião, verificar a variante semanal de objetivos e a barra atual/total. Na Campanha, confirmar que `LevelData`, obstáculos, objetivos e estrelas continuam sendo respeitados.
