using System;
using System.Data.Entity;
using System.Linq;
using EntityFramework.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EntityFramework.Test
{
    [TestClass]
    public class InsertSqlGenerationTests
    {
        private IQueryable<Person> linqQuery;

        [TestInitialize]
        public void TestFixtureSetUp()
        {
            Database.SetInitializer<TestContext>(null);

            linqQuery = new Repository<Person>().GetAll()
                                                .Select(person => person);
        }

        [TestMethod]
        public void SelectIntoTempTableFromLinq()
        {
            string insertSql = linqQuery.SelectInsertSql("#tmp");
            Console.WriteLine(insertSql);
            StringAssert.Contains(insertSql, " INTO #tmp FROM ");
        }

        [TestMethod]
        public void InsertSelectFromLinq()
        {
            string insertSql = linqQuery.InsertIntoSql("TableNameToInsert", "Id, FirstName, LastName");
            Console.WriteLine(insertSql);
            StringAssert.Contains(insertSql, "INSERT INTO TableNameToInsert (Id, FirstName, LastName)\r\nSELECT");

            Console.WriteLine();
            string insertSqlSimple = linqQuery.InsertIntoSql("TableNameToInsert");
            Console.WriteLine(insertSqlSimple);
            StringAssert.Contains(insertSqlSimple, "INSERT INTO TableNameToInsert \r\nSELECT");
        }
    }

    public class Person
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }

    public class Repository<TAggregateRoot>
        where TAggregateRoot : class 
    {
        private DbContext _dbContext = new TestContext();

        public IQueryable<TAggregateRoot> GetAll()
        {
            return _dbContext.Set<TAggregateRoot>();
        }
    }

    public class TestContext
        : DbContext
    {
        public DbSet<Person> Persons { get; set; }
    }

}
