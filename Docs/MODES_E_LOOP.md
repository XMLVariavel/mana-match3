# Modos jogáveis e loop competitivo

## Primeira expansão

O jogo será organizado em cinco modos jogáveis, com o mesmo tabuleiro e regras de troca, mas objetivos e métricas diferentes:

| Modo | Estado | Regra principal | Métrica salva |
|---|---|---|---|
| Campanha | Existente | Fases com movimentos, objetivos, obstáculos e estrelas | Estrelas por fase |
| Estudo Infinito | Existente, será ampliado | Pontuação sem limite fixo, dificuldade crescente e versículos | Recorde, nível e sequência |
| Desafio Diário | Nova implementação | Seed diário, 30 movimentos e configuração igual para todos | Score do dia e sequência |
| Contra o Relógio | Nova implementação | Sessão de 90 segundos, combos mantêm multiplicador | Score e melhor multiplicador |
| Guardião da Palavra | Nova implementação | Ondas de obstáculos e proteção de objetivos | Ondas concluídas |

## Regras de competitividade

A pontuação global será separada por modo e temporada. O ranking geral não deve misturar uma sessão de 90 segundos com uma campanha longa. Cada publicação conterá `uid`, `displayName`, `avatarId`, `modo`, `temporadaId`, `score`, `melhorCombo`, `sequencia` e `atualizadoEm`.

O Desafio Diário usará uma seed derivada da data UTC e de um identificador de desafio definido pelo servidor. O cliente pode exibir o resultado e salvar localmente, mas a publicação competitiva deverá ser validada no backend. Enquanto essa validação não existir, o modo deve ser tratado como beta e o ranking deve exibir o rótulo de classificação provisória.

## Estado do Estudo Infinito

O modo será organizado em marcos de 500 pontos, com aumento de variedade, chance controlada de obstáculos, card de versículo, recompensa de XP e resumo ao fim da sessão. A evolução seguinte adicionará multiplicador de combo, objetivos opcionais e temporadas, sem tornar a dificuldade impossível por acúmulo de obstáculos.

## Navegação

A Jornada terá cards grandes para Campanha e Estudo Infinito, uma faixa de modos especiais para Desafio Diário, Contra o Relógio e Guardião da Palavra, e uma barra inferior com Início, Jornada, Desafios, Loja e Perfil/Opções. O Placar ficará dentro de Desafios e também acessível a partir do resumo de cada sessão.
