# Direção visual e assets do Maná

## Direção visual

A nova direção visual combina **ilustração bíblica de acabamento alto**, madeira escura, pergaminho, azul-noturno e dourado queimado. A tela deve transmitir uma jornada de estudo e descoberta, sem aparência de protótipo: ícones com silhueta inequívoca, bordas douradas discretas, espaços regulares entre as peças e hierarquia clara entre tabuleiro, objetivos e ações.

O alvo visual principal é uma tela móvel em retrato, com tabuleiro 8x8 centralizado dentro de uma moldura de madeira e pergaminho. O fundo deve ser azul-marinho profundo com textura sutil; os elementos importantes usam dourado para destaque, verde-oliva e azul-petróleo para variação. O conteúdo do tabuleiro deve ocupar uma área compacta, com cada peça visualmente menor que a célula e com margem suficiente para que nenhuma peça toque ou cubra a vizinha.

## Referência visual

A referência gerada para esta etapa está em `Docs/reference_visual_mana.png`. Ela define a composição da HUD, a escala relativa do tabuleiro, a paleta e os três poderes avulsos do rodapé.

## Assets planejados

| Categoria | Conteúdo | Uso |
|---|---|---|
| Peças | Pão, peixe, uva, espiga, azeite e pomba | Seis tipos de peça do tabuleiro |
| Especiais | Espada em linha, espada em coluna, tocha, arca e estrela | Resultado de combinações e poderes do tabuleiro |
| Obstáculos | Pedra, corrente, gelo e caixa selada | Progressão da Campanha |
| UI | Moldura do tabuleiro, botões de poder, chips de objetivo e cartão de versículo | Leitura e hierarquia visual |
| Telas | Fundo de jornada, painel de campanha, estudo infinito, perfil, loja, ranking e configurações | Navegação principal e secundária |

## Regras de integração

A correção imediata de escala será aplicada no componente `Tile`, pois os sprites atuais foram importados com 256 pixels por unidade enquanto o pão possui uma área útil aproximada de 887x971 pixels. Isso faz a peça renderizar com quase 3,5 unidades de largura e quase 3,8 de altura em uma célula de 1 unidade. A peça será normalizada por célula em runtime, preservando o collider de toque em 1x1.

Os novos PNGs podem substituir os arquivos em `Assets/Art/Placeholder/` mantendo os nomes atuais. A ordem dos sprites continua alinhada aos enums `TileType` e `SpecialType` para não quebrar os dados de fase existentes.
