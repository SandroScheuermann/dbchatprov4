using DBChatPro.Models;

namespace DBChatPro
{
    public class DatabaseManagerService(MySqlDatabaseService mySqlDb, SqlServerDatabaseService msSqlDb, PostgresDatabaseService postgresDb, FirebirdServerDatabaseService firebirdDb) : IDatabaseService
    {
        public async Task<List<List<string>>> GetDataTable(AIConnection conn, string sqlQuery)
        {
            switch (conn.DatabaseType)
            {
                case "MSSQL":
                    return await msSqlDb.GetDataTable(conn, sqlQuery);
                case "MYSQL":
                    return await mySqlDb.GetDataTable(conn, sqlQuery);
				case "POSTGRESQL":
					return await postgresDb.GetDataTable(conn, sqlQuery);
				case "FIREBIRD":
					return await firebirdDb.GetDataTable(conn, sqlQuery);
			}

            return null;
        }

        public async Task<DatabaseSchema> GenerateSchema(AIConnection conn)
        {
            switch (conn.DatabaseType)
            {
                case "MSSQL":
                    return await msSqlDb.GenerateSchema(conn);
                case "MYSQL":
                    return await mySqlDb.GenerateSchema(conn);
				case "POSTGRESQL":
					return await postgresDb.GenerateSchema(conn); 
				case "FIREBIRD":
					return await firebirdDb.GenerateSchema(conn);
			}

            return new() { SchemaStructured = new List<TableSchema>(), SchemaRaw = new List<string>() };
        }
    }
}
