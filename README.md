# DADTKV
<small>Pelo sim pelo não, faça **Rebuild** da Solução.</small>

## Execução normal
Para executar o projeto, `CTRL+F5` (ou doutra forma) para iniciar o projeto `Manager`.

Este projeto pede para selecionar o ficheiro de configuração do sistema e depois trabalha de forma autónoma.

## Debug
Os processos são iniciados com argumentos. Para tal, deve passar esses argumentos ao projeto que deseja fazer *Debug*. Veja como [aqui](https://stackoverflow.com/a/276547).

No `Program.cs` dos projetos `Client`, `TransactionManager`, e `LeaseManager`, estão definidos num género de "javadoc" os argumentos que esperam receber.

> Se necessitar de ajuda para descobrir os argumentos: fazer *Debug* do `Manager` e ver os valores dos argumentos em `Main.cs`, nas linhas `190`, `197`, e `242`.
