// See https://aka.ms/new-console-template for more information
using EntitySync;
using SqlSugar;

var db = new SqlSugarClient(new ConnectionConfig()
{
    ConnectionString = "server=localhost;Database=DBTest;Uid=root;Pwd=123456@q;",
    DbType = DbType.MySql,
    IsAutoCloseConnection = true,
    InitKeyType = InitKeyType.Attribute
});
DBHelper.SyncEntity(db);

