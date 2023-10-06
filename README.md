# DADTKV

No geral, n�o h� toler�ncia a faltas nem suspeitas de outros processos.

LeaseManager est� implementado duma forma b�sica.

TransactionManager e Cliente por come�ar.

## Nota importante
O comando T � ignorado e a primeira inst�ncia do Paxos inicia logo que poss�vel (ap�s D milissegundos de todos os LeaseManagers estarem prontos)

## LeaseManager
### Paxos
- [x] gRPC
- [x] Em s�rie
- [x] Em paralelo (sem optimiza��es)
- [x] Chegar a consenso sobre value (leases)
- [ ] Notificar Learners
- [ ] Tolerar falhas
	> Tolera desde que n�o seja do proposer ativo.
- [	 ] Iniciar uma nova �poca no mesmo slot
### Leases
- [x] Receber pedidos de leases
	> Atualmente est� simulado no Program.cs do projeto LeaseManager porque o TransactionManager n�o est� implementado
- [ ] Quando h� diferen�a de leases entre LMs, concatenar as leases
	- [x] Verifica��o de hashes (enviar hash do Proposer no Prepare)
	- [ ] Concatenar as leases

## Manager
- [x] gRPC
- [x] Iniciar os processos
- [ ] Configurar processos iniciados
	- [x] LeaseManager
	- [ ] TransactionManager
	- [ ] Client
- [x] Visualiza��o de quem est� ativo ou n�o
- [ ] "Crashar" um processo
	- [x] LeaseManager
	- [ ] TransactionManager
	- [ ] Client


## TransactionManager
N�o h� nada de interessante para mostrar.

## Clients
N�o est� nada feito.