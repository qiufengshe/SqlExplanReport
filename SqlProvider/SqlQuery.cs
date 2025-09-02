using System.Data.Common;

namespace SqlExplanReport;

public class SqlQuery
{
    /// <summary>
    /// 将执行sql所在的command命令对象
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    public static string GetQueryPlan(DbCommand command)
    {
        var provider = GetDatabaseProvider(command);

        if (provider == null)
        {
            return "Unsupported database provider {command.GetType().FullName}";
        }

        try
        {
            //获取sql的执行计划的内容
            var rawPlan = provider.ExtractPlan();
            return provider.Encode(rawPlan);
        }
        catch (Exception ex)
        {
            return $"Failed to extract execution plan. {ex.Message}";
        }
    }


    private static DatabaseProvider GetDatabaseProvider(DbCommand command)
    {
        return command.GetType().FullName switch
        {
            "Microsoft.Data.SqlClient.SqlCommand" => new SqlServerDatabaseProvider(command),
            "Npgsql.NpgsqlCommand" => new PostgresDatabaseProvider(command),
            "Oracle.ManagedDataAccess.Client.OracleCommand" => new OracleDatabaseProvider(command),
            "Microsoft.Data.Sqlite.SqliteCommand" => new SQLiteDatabaseProvider(command),
            _ => throw new Exception("不支持的数据库")
        };
    }
}