# Checklist de Conformidade LGPD — Para Revisão Jurídica

> **Isto não é aconselhamento jurídico.** É um checklist de engenharia/produto
> que mapeia o que já está implementado, o que falta implementar, e o que
> precisa necessariamente de um advogado especializado em LGPD antes do
> lançamento — principalmente por causa do público infantil.

## O que já está implementado (`PrivacyManager` + `FirebaseManager`)

- [x] Minimização de dados: o schema (`PlayerProgress`) só guarda o
      necessário para funcionalidade — progresso, XP, ranking. Nenhum dado
      sensível (nome completo, e-mail, documento) é coletado.
- [x] Login anônimo por padrão — o jogo funciona sem qualquer cadastro.
- [x] Registro técnico do consentimento (aceite/recusa + timestamp), local e
      no Firestore.
- [x] Direito de acesso: `ExportarMeusDados()` monta um JSON com tudo que é
      guardado sobre o jogador.
- [x] Direito de eliminação: `ExcluirMinhaContaEDados()` apaga o documento de
      progresso, a entrada no leaderboard e a conta de autenticação.
- [x] Regras do Firestore (`firestore.rules`) impedem qualquer jogador de
      ler/escrever o dado de outro.

## O que falta implementar (não é trabalho jurídico, é código/infra)

- [ ] **Tela de consentimento** em si (texto, botões aceitar/recusar) — a UI
      ainda não foi construída, só o método que registra a decisão.
- [ ] **Tela "Meus Dados"** nas Configurações, consumindo
      `ExportarMeusDados()`/`ExcluirMinhaContaEDados()`.
- [ ] **Purga automática de contas inativas**: hoje não existe nenhum job
      rodando isso — precisaria de uma Cloud Function agendada (o que exige
      o plano Blaze do Firebase, não o Spark atual) ou um processo manual
      periódico enquanto o projeto for pequeno.
- [ ] Botão de "recusar e sair" funcional (o app não pode forçar aceite para
      funcionar, se a recusa significar não poder jogar nada minimamente).

## Itens que exigem decisão/revisão jurídica antes do lançamento

- [ ] **Dado de criança (LGPD art. 14)**: definir se o app permite conta
      vinculada (Google) para menores, e se sim, qual mecanismo de
      consentimento parental será usado. Hoje o app não verifica idade —
      isso é uma decisão de produto que precisa virar requisito técnico
      depois de definida.
- [ ] **Texto da Política de Privacidade e Termos de Uso** — não foram
      redigidos aqui de propósito.
- [ ] **Base legal do tratamento** (consentimento vs. execução de contrato,
      etc.) para cada finalidade de dado coletado.
- [ ] **Google/Firebase como operador de dados**: confirmar se o DPA
      (Data Processing Agreement) do Google Cloud/Firebase cobre o uso
      pretendido e se precisa ser referenciado na Política de Privacidade.
- [ ] **Prazo de retenção** formal (quantos meses de inatividade até a purga)
      — o código está pronto pra executar a purga, mas o número em si é uma
      decisão de política, não técnica.
- [ ] Necessidade (ou não) de Encarregado de Dados (DPO) formalmente
      designado, dependendo do porte esperado do app.

## Antes de publicar na Play Store

- [ ] Preencher a seção "Segurança de dados" do Google Play Console
      refletindo exatamente o que este checklist descreve.
- [ ] Confirmar classificação indicativa/etária do app na Play Store
      condizente com a decisão sobre dado de criança acima.
