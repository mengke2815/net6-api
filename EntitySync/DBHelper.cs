using NET6.Domain.Entities;
using SqlSugar;

namespace EntitySync
{
    /// <summary>
    /// DBHelper
    /// </summary>
    public class DBHelper
    {
        /// <summary>
        /// 同步数据库结构
        /// </summary>
        public static void SyncEntity(SqlSugarClient db)
        {
            #region Aop
            //db.Aop.OnLogExecuting = (sql, pars) =>
            //{
            //    Console.WriteLine(sql + "" + Db.Utilities.SerializeObject
            //        (pars.ToDictionary(it => it.ParameterName, it => it.Value)));
            //    Console.WriteLine();
            //}; 
            #endregion

            #region 对话框
            Console.WriteLine("是否确定同步数据库表结构？(同名数据表将会被备份，生产环境慎用，回车确认)");
            var str = Console.ReadKey();
            if (str.Key == ConsoleKey.Enter)
            {
                Console.WriteLine("同步中，请稍后...");
            }
            else
            {
                Console.WriteLine("\r\n输入错误，已退出...");
                return;
            }
            #endregion

            //同步数据表结构
            db.DbMaintenance.CreateDatabase();
            db.CodeFirst.SetStringDefaultLength(50).BackupTable().InitTables(new Type[]
            {
                typeof(Address)
            });
            Console.WriteLine("数据库结构同步完成!");
        }
    }
}
