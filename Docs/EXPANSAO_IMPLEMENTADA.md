# Expansão implementada do Maná

## Modos disponíveis

O jogo deixou de ter apenas a Campanha e o Estudo Infinito como entradas de gameplay. A primeira expansão jogável agora possui cinco modos: Campanha, Estudo Infinito, Desafio Diário, Contra o Relógio e Guardião da Palavra.

A Campanha continua orientada por fases e objetivos. O Estudo Infinito usa pontuação, XP, escalada de dificuldade, versículos por marcos, limites de obstáculos e combo. O Desafio Diário usa uma semente determinística derivada do dia UTC, permitindo que os jogadores enfrentem uma configuração comparável. O Contra o Relógio começa com 90 segundos e exibe cronômetro. O Guardião da Palavra combina objetivos temáticos de peças com progressão de pontuação.

## Estudo Infinito

O modo agora tem combo com janela curta, melhor combo da sessão, aumento progressivo de dificuldade, desbloqueio de versículos, geração de obstáculos limitada e recorde persistente no perfil. O HUD mostra o modo atual, o combo e o cronômetro quando aplicável. A pontuação final é publicada por modo e temporada quando existe consentimento e conexão.

## Referência visual

A montagem central foi alinhada à linguagem das referências: azul-marinho profundo, dourado queimado, painéis escuros, cabeçalhos ornamentais, fundo ilustrado, botões compactos e navegação inferior. A Jornada ganhou uma ação principal de Estudo Infinito e três cards rápidos para os modos especiais. O Perfil recebeu preview, grade e persistência de cinco avatares bíblicos: Davi, Ester, Daniel, Rute e Moisés.

## Abertura

Foi incluído um vídeo vertical de cinco segundos em `Assets/Video/intro_mana_jesus_te_ama.mp4`. O montador cria uma `RawImage`, reproduz o vídeo em `RenderTexture` e sobrepõe a frase exata `Jesus Te Ama`. O fluxo aguarda os cinco segundos antes de abrir consentimento ou carregamento, sem deixar o login do Firebase interromper a abertura.

## Placar global

O Placar consulta até 50 entradas globais e permite filtrar por Geral, Estudo Infinito, Desafio Diário, Contra o Relógio e Guardião da Palavra. Cada entrada pode carregar nome, avatar, modo, temporada, pontuação e melhor combo. A publicação usa UID e temporada para evitar identificar jogadores apenas por nome.

Antes de uma publicação competitiva, ainda é necessário mover a validação do score para backend/Cloud Function. O cliente sanitiza os dados e bloqueia publicação sem consentimento, mas não deve ser a autoridade final sobre pontuação em um lançamento público. Os requisitos estão em `Docs/FIREBASE_RANKING.md`.
