# Refinamento visual da interface

## Problemas encontrados

A composição anterior deixava o fundo celestial muito luminoso atrás dos textos, usava botões pretos sem hierarquia suficiente, apresentava a navegação inferior comprimida e mostrava a tela Opções como uma sequência de labels e controles sem agrupamento visual.

## Correções aplicadas

Foi adicionada uma camada de contraste azul-marinho translúcida entre o fundo ilustrado e os elementos da interface. O fundo celestial continua presente como ambientação, mas não compete mais diretamente com os títulos e descrições.

A fábrica de UI recebeu ícones vetoriais dourados para casa, jornada, desafios, loja, perfil, opções, música, efeitos, vibração, conta, privacidade, exportação e exclusão. Esses ícones são construídos sem depender de caracteres Unicode ou de novas fontes e funcionam em resoluções menores.

Os botões da navegação inferior agora exibem ícone e rótulo em hierarquia vertical. A navegação continua responsiva e preserva Início, Jornada, Desafios, Loja, Perfil e Opções.

A tela Opções foi remontada em cartões ornamentados, com ícone, título, descrição, controles de volume, toggles, ações de conta, gerenciamento de anúncios e ações de privacidade. Os cartões possuem altura suficiente para acomodar todos os controles sem sobreposição e ficam dentro de uma área rolável.

## Validação

A cena foi remontada e salva em 21 de agosto de 2026 às 16:31:27, com 2.017.673 bytes. O log batch registrou `Cena remontada automaticamente`, `Montagem batch concluída e SampleScene salva` e `Exiting batchmode successfully now`, sem `error CS`, `Compilation failed` ou exceção do montador.

A validação estrutural confirmou `Contraste`, `CartaoMusica`, `CartaoEfeitos`, `CartaoVibracao`, `CartaoConta`, `CartaoExportar`, `CartaoExcluir`, `BotaoOpcoes`, elementos `Icone` e `Fundo` na cena final.

## Teste recomendado

No Unity, testar Game View portrait em 1080x1440 e 720x1280. Abrir Início, Desafios e Opções; verificar que o fundo permanece escurecido atrás dos textos, que os ícones aparecem, que todos os cartões podem ser rolados e que os sliders/toggles continuam interativos. Confirmar também o estado pressionado dos seis botões da navegação inferior.
