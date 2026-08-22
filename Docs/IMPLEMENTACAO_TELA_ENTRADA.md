# Implementação da tela de entrada

## Fluxo inicial

Depois dos cinco segundos do vídeo de abertura, o jogo agora apresenta a tela `Entrada`. A decisão não passa mais por uma tela de consentimento isolada antes de o jogador conhecer as opções. A própria tela de entrada apresenta os dois caminhos e registra o aceite quando o usuário continua.

O botão `JOGAR COMO CONVIDADO` registra o consentimento, garante a tentativa de login anônimo idempotente e abre a tela `Inicio`. O progresso local continua disponível mesmo quando os serviços online não estão disponíveis.

O botão `ENTRAR COM GMAIL` registra o consentimento e chama o fluxo existente de Google Sign-In. Quando o plugin e o Web Client ID estiverem configurados, o token é encaminhado ao `LoginController` e a conta anônima é vinculada ao Gmail. Sem o plugin, a tela mostra uma mensagem clara informando que o recurso não está disponível na build, sem quebrar o jogo.

## Arte utilizada

As artes fornecidas foram copiadas para nomes canônicos em `Assets/Art/UI/Entrada/`:

| Arquivo | Uso |
|---|---|
| `fundo_tela_entrada.jpg` | Fundo vertical com cruz, caminho e paisagem bíblica. |
| `painel_bem_vindo.png` | Painel de madeira com a mensagem de boas-vindas. |
| `logo_jogo.png` | Logo transparente Bible Match Maná. |

As imagens JPEG de referência que continham o padrão xadrez foram mantidas apenas como referência visual; não foram usadas como fundo de runtime para evitar que o xadrez apareça no jogo.

## Interface

A tela possui fundo em tela cheia, vinheta escura, moldura ornamental, logo no topo, painel de boas-vindas, explicação curta e dois botões reais empilhados. Os botões possuem áreas de toque separadas das artes e ícones vetoriais de conta e início.

No rodapé, o texto está dividido para tornar os links funcionais:

> Ao continuar, você concorda com nossos Termos e Política de Privacidade

`Termos` e `Política de Privacidade` são botões sublinhados e abrem o navegador do aparelho. As URLs padrão são `https://palavivagames.com/termos` e `https://palavivagames.com/privacidade`, mas ficam expostas no `EntradaController` para substituição quando as páginas oficiais forem publicadas.

A versão exibida é `Versão 1.0.0 | 2026 PalaVivaGames`.

## Validação

A cena foi remontada em modo batch em 22 de agosto de 2026 às 12:59:58, com 1.813.774 bytes. O log registrou três texturas reimportadas, montagem batch concluída e saída normal do Unity.

A cena final contém `Entrada`, `FundoEntrada`, `PainelBoasVindas`, `LogoJogo`, `BotaoEntrarGmail`, `BotaoConvidado`, `LinkTermos` e `LinkPrivacidade`. A compilação terminou sem `error CS`, `Compilation failed` ou exceção do montador.

## Teste no Unity

Verificar no Game View portrait 1080x1440 e 720x1280. Confirmar que o vídeo segue para Entrada, que os botões não se sobrepõem, que Convidado abre Início, que Gmail mostra o estado correto quando o plugin não está configurado, e que os dois links abrem URLs no navegador. Depois de publicar as páginas, substituir as duas URLs no componente `EntradaController` e remontar a cena.
