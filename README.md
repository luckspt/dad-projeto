# DADTKV

No geral, não há tolerância a faltas nem suspeitas de outros processos.

LeaseManager está implementado duma forma básica.

TransactionManager e Cliente por começar.

## Nota importante
O comando T é ignorado e a primeira instância do Paxos inicia logo que possível (após D milissegundos de todos os LeaseManagers estarem prontos)

## LeaseManager
### Paxos
- [x] gRPC
- [x] Em série
- [x] Em paralelo (sem optimizações)
- [x] Chegar a consenso sobre value (leases)
- [ ] Notificar Learners
- [ ] Tolerar falhas
	> Tolera desde que não seja do proposer ativo.
- [	 ] Iniciar uma nova época no mesmo slot
### Leases
- [x] Receber pedidos de leases
	> Atualmente está simulado no Program.cs do projeto LeaseManager porque o TransactionManager não está implementado
- [ ] Quando há diferença de leases entre LMs, concatenar as leases
	- [x] Verificação de hashes (enviar hash do Proposer no Prepare)
	- [ ] Concatenar as leases

## Manager
- [x] gRPC
- [x] Iniciar os processos
- [ ] Configurar processos iniciados
	- [x] LeaseManager
	- [ ] TransactionManager
	- [ ] Client
- [x] Visualização de quem está ativo ou não
- [ ] "Crashar" um processo
	- [x] LeaseManager
	- [ ] TransactionManager
	- [ ] Client


## TransactionManager
Não há nada de interessante para mostrar.

## Clients
Não está nada feito.