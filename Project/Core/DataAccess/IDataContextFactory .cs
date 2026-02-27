using LinqToDB.Data;

namespace bbbbb.Project.Infrastructure.DataAccess
{
    public interface IDataContextFactory<TDataContext> where TDataContext : DataConnection
    {
        TDataContext CreateDataContext();
    }
}