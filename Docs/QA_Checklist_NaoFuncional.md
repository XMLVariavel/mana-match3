# Checklist de QA Manual — Não-Funcionais (Fase A)

Itens que **não dá pra automatizar** com o Unity Test Framework porque dependem
de hardware real, percepção humana ou comportamento do sistema operacional.
Os testes automatizados (EditMode/PlayMode) cobrem a lógica; isto aqui cobre
a experiência real no aparelho.

## Performance em dispositivo
- [ ] Rodar o Unity Profiler conectado a um Android de entrada (~2 GB RAM) e
      confirmar que uma cascata grande (vários combos em sequência) não causa
      spikes de GC visíveis nem queda de frame rate abaixo do aceitável.
- [ ] Sessão de 20–30 minutos contínuos: checar crescimento de memória ao
      longo do tempo (vazamento por peças/obstáculos não devolvidos ao pool).
- [ ] Tempo de carregamento da cena do tabuleiro em aparelho de entrada.

## Input / responsividade
- [ ] Swipe em diferentes tamanhos e densidades de tela (o `swipeThreshold`
      em unidades de mundo pode precisar de ajuste por DPI).
- [ ] Confirmar que um toque rápido (tap, sem arrasto) não dispara troca
      acidental.
- [ ] Testar com a peça travada por Corrente: o dedo consegue tocar noutra
      peça normalmente, só a travada é que não reage.

## Compatibilidade
- [ ] Testar na versão mínima de Android suportada e na mais recente.
- [ ] Diferentes aspect ratios (celulares alongados, tablets).
- [ ] Orientação: confirmar que o jogo trava em retrato (ou landscape, o que
      for definido) sem esticar/cortar o tabuleiro.

## Interrupção e ciclo de vida do app
- [ ] Receber uma ligação/notificação durante uma animação de queda —
      confirmar que o estado do tabuleiro não corrompe ao voltar.
- [ ] Colocar o app em background no meio de uma cascata e retomar.

## Usabilidade
- [ ] Um jogador novo entende o objetivo da fase sem explicação?
- [ ] A distinção visual entre peça normal, peça especial e peça sob
      obstáculo (Gelo/Caixa/Corrente/Pedra) é clara à distância de um braço?
- [ ] Feedback tátil (haptics) em matches/combos — sentir se está exagerado
      ou insuficiente.

## Acessibilidade
- [ ] Contraste dos ícones das 6 peças (teste com simulador de daltonismo).
- [ ] Tamanho de fonte ajustável nos textos de HUD (quando existirem).

---
Os itens acima ficam de fora dos testes automatizados citados em
`/Tests/EditMode` e `/Tests/PlayMode` de propósito — são qualitativos ou
dependem de hardware que o Test Runner não reproduz.
