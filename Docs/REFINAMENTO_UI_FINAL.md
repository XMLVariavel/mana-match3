# Refinamento final da interface do Maná

## Resultado

A interface do projeto Unity foi remontada para uma direção visual bíblica premium, com azul-marinho, dourado, teal, pergaminho, madeira escura e molduras ornamentais. A `SampleScene` foi salva novamente em 21 de agosto de 2026, com resolução de referência `1080 x 1440` para mobile portrait.

## Alterações principais

Foram adicionados os assets reutilizáveis em `Assets/Art/UI/Ornamentos/`: `header_ornament.png`, `button_primary.png`, `button_secondary.png`, `card_panel.png` e `bottom_navigation.png`. Também foram adicionados cinco ícones em `Assets/Art/UI/Ornamentos/Modos/`: campanha, estudo infinito, desafio diário, contra o relógio e guardião da palavra.

`ManaArte.cs` agora registra e importa os novos assets como sprites. `ManaUI.cs` passou a usar os fundos ornamentais nos botões, cabeçalhos, painéis, navegação e cards. O novo componente `CardModo` cria cards com ícone, título, descrição e linha de destaque.

A Jornada agora exibe cinco cards funcionais: Campanha, Estudo Infinito, Desafio Diário, Contra o Relógio e Guardião da Palavra. A Campanha inicia a primeira fase disponível; os outros quatro cards chamam os métodos de modo já existentes. A navegação inferior foi reorganizada dentro do próprio `Rodape` com Início, Jornada, Desafios, Loja e Perfil.

`BotaoVoltar` foi corrigido para ficar ancorado entre o topo e a base do próprio cabeçalho, com margens internas seguras. Os painéis de fim de partida, versículo, perfil e configurações receberam o painel ornamental.

`MapaDeFasesController.cs` recebeu o campo `telaMapa`, os destinos da navegação inferior e o método `EntrarNaPrimeiraFase`, eliminando o erro de compilação que apareceu durante a validação.

## Validação

A cena salva contém os cinco cards, a navegação `BotaoDesafios`, o cabeçalho da Jornada e a resolução `1080 x 1440`. Após a correção de `telaMapa`, o Unity foi reiniciado e o log recente não apresentou `error CS`, `Compilation failed`, `Could not register callback class null` ou exceção equivalente.

O teste visual/interativo completo em Play ainda deve ser executado manualmente no Editor, especialmente para conferir a escala dos cards no Game View 1080 x 1440, a abertura da Splash de cinco segundos, a navegação entre as dez telas, o toque nas peças e o fluxo offline do Firebase.

## Procedimento recomendado

Abra o projeto em `F:\Nova pasta\Mana`, confirme que `Assets/Scenes/SampleScene.unity` está aberta, selecione o Game View em `1080 x 1440` portrait e pressione Play. Verifique primeiro Splash, Jornada, Perfil, Placar, Loja e Opções. Depois entre nos cinco modos e teste a interação do tabuleiro com toque/clique nas peças.
