using DBChatPro.Models;
using FirebirdSql.Data.FirebirdClient;

namespace DBChatPro
{
	public class FirebirdServerDatabaseService : IDatabaseService
	{
		public async Task<List<List<string>>> GetDataTable(AIConnection conn, string sqlQuery)
		{
			var rows = new List<List<string>>();
			using (FbConnection connection = new FbConnection(conn.ConnectionString))
			{
				using var command = new FbCommand(sqlQuery, connection);

				await connection.OpenAsync();
				using var reader = await command.ExecuteReaderAsync();

				bool headersAdded = false;
				if (reader.HasRows)
				{
					while (await reader.ReadAsync())
					{
						var cols = new List<string>();
						var headerCols = new List<string>();

						if (!headersAdded)
						{
							for (int i = 0; i < reader.FieldCount; i++)
							{
								headerCols.Add(reader.GetName(i));
							}
							headersAdded = true;
							rows.Add(headerCols);
						}

						for (int i = 0; i < reader.FieldCount; i++)
						{
							try
							{
								cols.Add(reader.GetValue(i)?.ToString() ?? "NULL");
							}
							catch
							{
								cols.Add("DataTypeConversionError");
							}
						}

						rows.Add(cols);
					}
				}
			}

			return rows;
		}

		public async Task<DatabaseSchema> GenerateSchema(AIConnection conn)
		{
			var dbSchema = new DatabaseSchema()
			{
				SchemaRaw = new List<string>(),
				SchemaStructured = new List<TableSchema>()
			};

			List<KeyValuePair<string, string>> rows = new();

			using (FbConnection connection = new FbConnection(conn.ConnectionString))
			{
				await connection.OpenAsync();

				string sql = @"
                    SELECT TRIM(r.RDB$RELATION_NAME) AS TableName,
                           TRIM(f.RDB$FIELD_NAME) AS ColumnName
                    FROM RDB$RELATION_FIELDS f
                    JOIN RDB$RELATIONS r ON r.RDB$RELATION_NAME = f.RDB$RELATION_NAME
                    WHERE r.RDB$SYSTEM_FLAG = 0
                    ORDER BY r.RDB$RELATION_NAME, f.RDB$FIELD_POSITION";

				using var command = new FbCommand(sql, connection);
				using var reader = await command.ExecuteReaderAsync();

				while (await reader.ReadAsync())
				{
					string tableName = reader.GetString(0);
					string columnName = reader.GetString(1);

					rows.Add(new KeyValuePair<string, string>(tableName, columnName));
				}
			}

			var groups = rows.GroupBy(x => x.Key);
			foreach (var group in groups)
			{
				dbSchema.SchemaStructured.Add(new TableSchema()
				{
					TableName = group.Key,
					Columns = group.Select(x => x.Value).ToList()
				});
			}

			var textLines = new List<string>();
			foreach (var table in dbSchema.SchemaStructured)
			{
				var schemaLine = $"- {table.TableName} (";

				foreach (var column in table.Columns)
				{
					schemaLine += column + ", ";
				}

				schemaLine = schemaLine.TrimEnd(',', ' ') + " )";
				textLines.Add(schemaLine);
				Console.WriteLine(schemaLine);
			}

			dbSchema.SchemaRaw = textLines;

			return dbSchema;
		}
	}
}
