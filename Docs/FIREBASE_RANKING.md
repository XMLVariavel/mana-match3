# Ranking global e Firebase

## Estado implementado

O runtime agora grava entradas na coleção `leaderboard`, usando um documento por usuário, modo e temporada. Cada entrada contém `uid`, `displayName`, `avatarId`, `modo`, `temporadaId`, `highScore`, `melhorCombo` e `atualizadoEm`. O Placar consulta até 50 entradas, permite filtros por Geral, Estudo Infinito, Desafio Diário, Contra o Relógio e Guardião da Palavra, mostra avatar e identifica o próprio jogador por UID.

A publicação ocorre ao concluir uma partida por vitória ou derrota. O modo real e uma entrada `geral` são publicados para permitir comparação ampla. Usuários sem consentimento de nuvem ou em modo offline continuam jogando, mas não aparecem no ranking global.

## Requisito de produção

A implementação atual é adequada para protótipo e teste integrado, mas a pontuação ainda é enviada pelo cliente. Antes de publicar competitivamente, a gravação do score deve ser movida para uma Cloud Function ou para um endpoint server-authoritative que valide a sessão, a seed, o número de movimentos, o tempo e o resultado. O cliente não deve ter permissão de definir livremente `highScore`.

A regra conceitual para a coleção deve permitir leitura pública apenas dos campos competitivos, exigir autenticação para qualquer escrita e aceitar escrita apenas quando o UID do documento corresponder ao usuário autenticado. A validação de score máximo, modo permitido, temporada vigente e tamanho de textos deve ficar no backend. O cliente já aplica sanitização defensiva, mas isso não substitui a regra do servidor.

## Índices e migração

A consulta por `modo`, `temporadaId` e `highScore` pode exigir índice composto no Firestore. Ao executar a primeira consulta, o Firebase informa o link para criação do índice caso ele ainda não exista. Em produção, a coleção deverá ser organizada por temporada e modo ou receber índices previamente versionados.

Os documentos antigos do leaderboard, caso existam, podem ser migrados adicionando `avatarId`, `modo` e `temporadaId`. Entradas sem esses campos continuam sendo tratadas como `geral` durante a leitura, mas não devem ser consideradas válidas para uma temporada competitiva nova.

## Privacidade

O consentimento continua sendo obrigatório para sincronização e ranking. O Perfil deve exibir o avatar e o nome escolhidos, e Configurações deve manter exportação e exclusão. A exclusão de conta precisa remover o documento de progresso e todas as entradas do usuário em cada modo e temporada; essa etapa deve ser concluída no backend antes do lançamento público.
