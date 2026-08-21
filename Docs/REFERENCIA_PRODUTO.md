# Direção de produto e referência visual do Maná

## Leitura da referência

As imagens fornecidas definem um produto mobile portrait com identidade bíblica premium. A linguagem visual combina azul-marinho profundo, dourado, teal, pergaminho, madeira, bronze, brilho suave, ícones grandes e cartões com molduras ornamentais. A navegação principal aparece no rodapé como uma barra persistente de quatro ou cinco destinos, enquanto títulos e ações usam faixas ornamentais e hierarquia visual clara.

A tela de partida é o núcleo do produto: cabeçalho com marca Maná e referência bíblica, cards de pontos/movimentos/progresso, objetivos em pergaminho, tabuleiro grande dentro de uma moldura e poderes com ícone, contador e descrição curta. Loja, Jornada, Perfil e Configurações devem usar o mesmo sistema de cor, espaçamento, bordas e tipografia, evitando telas que pareçam pertencer a jogos diferentes.

## Quantidade atual de modos

O projeto atual possui **dois modos reais** no enum `GameMode`: `Campanha` e `EstudoInfinito`. A navegação possui várias telas, mas Loja, Perfil, Ranking e Configurações não são modos de partida. Portanto, a resposta objetiva é: atualmente existem dois modos jogáveis, não cinco.

## Proposta de modos jogáveis

| Modo | Objetivo | Diferencial | Competitividade |
|---|---|---|---|
| Campanha | Concluir fases com objetivos e estrelas | Jornada bíblica progressiva | Estrelas, fases perfeitas e colecionáveis |
| Estudo Infinito | Fazer a maior pontuação possível | Dificuldade crescente, versículos e obstáculos | Ranking semanal e recorde pessoal |
| Desafio Diário | Resolver uma fase com a mesma configuração para todos | Seed diário, regras iguais e tentativa limitada | Placar diário e sequência de dias |
| Contra o Relógio | Pontuar antes que o cronômetro termine | Combos e decisões rápidas | Melhor tempo, score e multiplicador |
| Guardião da Palavra | Proteger objetivos/versículos de obstáculos | Tabuleiro com ondas e defesa de áreas | Ondas concluídas e ranking por temporada |
| Jornada de Versículos | Completar capítulos temáticos | Cada sequência libera uma reflexão | Coleção, progresso e conquistas |
| Duelo de Pontuação | Competição assíncrona contra o recorde de outro usuário | Mesmo seed e replay de pontuação | Ranking por liga, sem PvP em tempo real |

A recomendação é lançar primeiro os quatro modos de menor risco: Campanha, Estudo Infinito, Desafio Diário e Contra o Relógio. Guardião da Palavra, Jornada de Versículos e Duelo podem entrar em uma segunda temporada, depois que o ranking e a telemetria estiverem estáveis.

## Inovações propostas

O produto pode se diferenciar com um sistema de **versículo do dia**, no qual o jogador recebe uma reflexão após uma meta de pontuação; **semanas temáticas** por livro ou personagem bíblico; **conquistas** por comportamento de jogo, como vencer sem usar poderes; **relíquias** que alteram regras de uma tentativa; e **desafios assíncronos** baseados em uma mesma configuração de tabuleiro. A inovação deve ampliar o significado bíblico sem transformar o conteúdo religioso em uma vantagem paga.

## Evolução do Estudo Infinito

O modo atual já tem variedade crescente, obstáculos e cards de versículo, mas deve ganhar uma estrutura de sessão clara: multiplicador de combo, marcos de pontuação, objetivos opcionais, recompensas por sequência, seleção de dificuldade e resumo final. O ranking deve separar recorde geral, melhor pontuação da temporada e melhor sequência, evitando que apenas uma pontuação antiga domine toda a experiência.

## Ranking global e Firebase

O projeto já possui uma coleção `leaderboard` no Firebase e consulta o Top N por `highScore`. Isso significa que a base técnica existe, mas a tela atual ainda não apresenta posição global, modo, temporada, avatar ou destaque robusto do próprio jogador. A próxima implementação deve usar um documento por usuário, com UID como chave, nome sanitizado, avatar escolhido, modo, score, temporada e timestamp. A tela deve oferecer abas por **Geral**, **Estudo Infinito**, **Desafio Diário** e **Temporada**, além de estado offline e posição do próprio usuário.

O ranking só deve aceitar pontuação de usuários com consentimento adequado e conta identificável pelo Firebase. Usuários que recusarem nuvem podem jogar normalmente, mas não devem aparecer no ranking global. Regras do Firestore precisam ser ajustadas para impedir que o cliente publique livremente scores arbitrários; em produção, a validação de recordes deve ser feita por Cloud Functions ou mecanismo server-authoritative.

## Perfil e avatares

O perfil atual permite apenas nome, nível, XP, score e versículos. A extensão planejada inclui avatar bíblico, moldura, título, coleção de personagens e seleção persistente. Os avatares devem ser apresentados como identidade visual, não como vantagem de gameplay. A primeira coleção pode conter personagens como **Davi, Ester, Daniel, Rute, José, Moisés, Maria, Pedro e Paulo**, com desbloqueio por campanha, conquistas ou eventos; compras podem ser cosméticas e opcionais.

## Abertura

A abertura deve apresentar uma sequência curta de cinco segundos com fundo bíblico em movimento sutil, logotipo Maná e a frase **“Jesus Te Ama”**, com áudio opcional, botão de pular após o primeiro segundo e respeito às configurações de som. O texto deve ser legível, acolhedor e não bloquear o acesso de quem estiver offline.

## Melhorias adicionais recomendadas

As prioridades de produto são melhorar onboarding, permitir continuar de onde o usuário parou, apresentar recompensas de forma clara, criar missões semanais, usar feedback visual e sonoro consistente, manter textos legíveis em telas pequenas, reduzir cliques desnecessários, mostrar estado offline, evitar anúncios intrusivos, registrar erros sem expor dados e separar claramente moeda de progresso, cosméticos e compras reais.
