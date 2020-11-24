using System;
using System.Data.Common;
using System.Data.SqlClient;
using System.Transactions;
using NUnit.Framework;
using Shuttle.Core.Data;
using Shuttle.Core.Transactions;

namespace Shuttle.ContentStore.Tests.Integration.DataAccess
{
    [TestFixture]
    public class DataAccessFixture
    {
        [SetUp]
        public void DataAccessSetUp()
        {
            DbProviderFactories.RegisterFactory("System.Data.SqlClient", SqlClientFactory.Instance);

            TransactionScopeFactory =
                new DefaultTransactionScopeFactory(true, IsolationLevel.ReadCommitted, TimeSpan.FromSeconds(120));
            DatabaseGateway = new DatabaseGateway();
            DatabaseContextCache = new ThreadStaticDatabaseContextCache();
            DatabaseContextFactory = new DatabaseContextFactory(new ConnectionConfigurationProvider(),  new DbConnectionFactory(), new DbCommandFactory(),
                DatabaseContextCache);
            DatabaseContextFactory.ConfigureWith("DocumentStore");
            QueryMapper = new QueryMapper(DatabaseGateway, new DataRowMapper());
        }

        protected static string DefaultConnectionStringName = "DocumentStore";

        protected ITransactionScopeFactory TransactionScopeFactory { get; private set; }
        protected IDatabaseContextCache DatabaseContextCache { get; private set; }
        protected IDatabaseGateway DatabaseGateway { get; private set; }
        protected IDatabaseContextFactory DatabaseContextFactory { get; private set; }
        protected IQueryMapper QueryMapper { get; private set; }
    }
}