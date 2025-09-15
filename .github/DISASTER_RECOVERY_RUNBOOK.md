Runbook de Recuperação de Desastres: Banco de Dados Azure SQL
1. Visão Geral
Propósito: Este documento detalha o processo passo a passo para restaurar o banco de dados sqldb-apexfood-dev a partir dos backups automáticos mantidos pelo Azure.

Tecnologia: Restauração Pontual (Point-in-Time Restore - PITR) do Azure SQL.

Frequência do Teste: Este procedimento deve ser testado a cada 3 meses para garantir sua validade e a familiaridade da equipe com o processo.

2. Pré-requisitos
Acesso de leitura/escrita ao Portal do Azure.

Permissões de Contributor (Colaborador) ou superiores no grupo de recursos rg-apexfood-dev.

Nome do Servidor SQL de origem: sql-apexfood-dev.

Nome do Banco de Dados de origem: sqldb-apexfood-dev.

3. Procedimento de Restauração (O Teste)
Objetivo: Restaurar o banco de dados para um novo banco de dados de teste, sem afetar o original.

Passo 3.1: Navegar até o Banco de Dados

Faça login no Portal do Azure.

Navegue até o grupo de recursos rg-apexfood-dev.

Clique no recurso Banco de Dados SQL com o nome sqldb-apexfood-dev.

Passo 3.2: Iniciar o Processo de Restauração

Na página de "Visão Geral" (Overview) do banco de dados, localize e clique no botão "Restaurar" (Restore) na barra de ferramentas superior.

Passo 3.3: Configurar o Ponto de Restauração

Na tela de restauração, a opção "Ponto no tempo" (Point-in-time) estará selecionada.

O Azure selecionará automaticamente o carimbo de data/hora mais recente possível. Para este teste, isso é suficiente. Em um cenário real, você selecionaria a data e a hora para um momento imediatamente anterior ao incidente de perda de dados.

Passo 3.4: Configurar o Destino da Restauração

Na seção "Detalhes do banco de dados", você definirá o nome do novo banco de dados.

IMPORTANTE: Nunca restaure sobrepondo o banco de dados original, a menos que seja uma emergência real e controlada. Para um teste, sempre restauramos para um novo destino.

Nome do banco de dados: Use um nome descritivo, como sqldb-apexfood-dev-restore-test-YYYYMMDD (substitua com a data atual, ex: 20250914).

Garanta que o mesmo servidor (sql-apexfood-dev) esteja selecionado.

Clique em "Revisar + criar" e, em seguida, em "Criar".

O processo de restauração será iniciado. Você pode acompanhar o progresso clicando no ícone de sino (Notificações) no canto superior direito do portal.

4. Procedimento de Verificação Pós-Restauração
Um restore só é considerado bem-sucedido após a verificação.

Passo 4.1: Confirmar a Criação do Recurso

Após a conclusão, volte para o grupo de recursos rg-apexfood-dev.

Confirme que um novo Banco de Dados SQL com o nome sqldb-apexfood-dev-restore-test-... agora existe.

Passo 4.2: Conectar e Verificar o Esquema

Clique no novo banco de dados restaurado.

No menu à esquerda, clique em "Editor de consultas".

Autentique-se (você pode usar sua identidade do Azure AD, que já configuramos como administradora).

Execute as seguintes consultas para validar a integridade do esquema:

SQL

-- Verifica se todas as tabelas foram restauradas
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE';
GO

-- Verifica se o histórico de migrações foi restaurado
SELECT MigrationId FROM __EFMigrationsHistory;
GO
Ambas as consultas devem retornar os resultados esperados, provando que o esquema está intacto.

5. Procedimento de Limpeza (Cleanup)
Após a verificação bem-sucedida, devemos remover os recursos criados para o teste.

Navegue até a página do banco de dados restaurado (sqldb-apexfood-dev-restore-test-...).

Clique no botão "Excluir" (Delete) e confirme a exclusão.