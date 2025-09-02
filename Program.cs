using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Tinyfox;
using Tinyfox.WebApiEngine;
using Tinyfox.WebApiEngine.Results;

namespace SqlExplanReport;

internal class Program
{
    private static string? SqlConnectionStr;

    private static void Main(string[] args)
    {
        var builder = new ConfigurationBuilder()
             .SetBasePath(Directory.GetCurrentDirectory()) // 设置基础路径为当前工作目录
             .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false);

        // 构建配置根对象
        IConfigurationRoot configuration = builder.Build();

        SqlConnectionStr = configuration["ConnectionStrings:MSSQLLocalDB"];
        if (string.IsNullOrWhiteSpace(SqlConnectionStr))
        {
            Console.WriteLine("未配置数据库");
            return;
        }

        // 设置服务端口（默认8088）
        Fox.Port = 9090;
        //Fox.NoStaticFile = true;
        Fox.UseDynamicHtml = true;

        // 配置路由关系
        // 如果路由条目很多，可以单独放到一个静态方法中集中处理
        var RT = (HttpRoute)Fox.Router;

        RT.GET["/"] = _ => TextResult.FromHtmlRoot("index.html", null);
        RT.GET["/api/values/{value}"] = c =>
        {
            var v = c.Request["value"];
            return new TextResult($"value is '{v}'");
        };
        RT.POST["/query"] = Query;

        // .....
        RT.OnNotFound = _ => new NotFoundResult("找不到你希望获取的资源...");


        // 启动并阻止程序退出
        Fox.Start();
        Fox.WaitExit();
    }

    /// <summary>
    /// 获取sql执行计划
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    private static TextResult Query(HttpContext context)
    {
        var form = context.Request.Form;
        var content = form["content"];

        var map = new Dictionary<string, string?>();
        using (SqlConnection con = new(SqlConnectionStr))
        {
            using (SqlCommand cmd = con.CreateCommand())
            {
                cmd.CommandText = content;
                var paln = SqlQuery.GetQueryPlan(cmd);
                map["plan"] = paln;
            }
        }

        return TextResult.FromHtmlRoot("SqlServer/template.html", map);
    }
}
