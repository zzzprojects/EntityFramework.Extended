using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using EntityFramework.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ComponentModel.DataAnnotations.Schema;
using EntityFramework.Audit;
using System.Data;
using System.Data.Common;
using System.Data.Entity.Core;

//using Utile.Money;

namespace EntityFramework.Test.CodeFirst
{
    [ComplexType]
    public class Money
    {
        public Money()
        {
            CurrencyCode = "USD";
            Amount = 0;
        }
        public Money(decimal amount,string currencyCode)
        {
            CurrencyCode = currencyCode;
            Amount = amount;
        }
        public string CurrencyCode { get; set; }
        public decimal Amount { get; set; }

    }
    public class Transaction
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public Money Money { get; set; }
        public string Detail { get; set; }
    }

    public class EFExtendedEntities : DbContext
    {
        public EFExtendedEntities() : base("EFExtendedCodeFirstTest")
        {

        }
        public DbSet<Transaction> Transactions { get; set; }
    }

    public class TestsInitializer : DropCreateDatabaseAlways<EFExtendedEntities>
    {
        protected override void Seed(EFExtendedEntities ctx)
        {
            //add a transaction
            var trx = new Transaction
            {
                Money = new Money( 123456789012.3456m,"USD"),
                Detail = "First Transaction"
            };
            ctx.Transactions.Add(trx);
            ctx.SaveChanges();
            base.Seed(ctx);
        }
    }

    [TestClass]
    public class EFExtendedCodeFirstTest
    {
        private EFExtendedEntities _ctx;

        [TestInitialize]
        public void Init()
        {
            var auditConfiguration = AuditConfiguration.Default;

            auditConfiguration.IncludeRelationships = true;
            auditConfiguration.LoadRelationships = true;
            auditConfiguration.DefaultAuditable = true;

            Database.SetInitializer(new TestsInitializer());
            _ctx = new EFExtendedEntities();
        }
        

        [TestMethod]
        public void EFExtendedCodeFirst_toXml()
        {
            // Arrange
            var trx = new Transaction
            {
                Money = new Money(123456789012.3456m, "USD"),// complex type
                Detail = "Another Transaction"
            };
            var audit = _ctx.BeginAudit();
            _ctx.Transactions.Add(trx);
            _ctx.SaveChanges();
            var log = audit.LastLog;
            //Act
            var xml = log.ToXml();//Exception thrown here

            // Assert
            Assert.IsTrue(!string.IsNullOrEmpty(xml), "xml is not null or blank");
        }

        [TestMethod]
        public void EFExtendedCodeFirst_Edit_Entities_not_empty_after_Complex_type_edit()
        {
            // Arrange
            var trx = new Transaction
            {
                Money = new Money(123456789012.3456m, "USD"),
                Detail = "Another Transaction"
            };
            _ctx.Transactions.Add(trx);
            _ctx.SaveChanges();

            //Act
            var audit = _ctx.BeginAudit();
            trx.Money.Amount = 10;
            var t = _ctx.Set<Transaction>().FirstOrDefault(x => x.Id == trx.Id);
            _ctx.Entry(t).CurrentValues.SetValues(trx);
            _ctx.Entry(t).State = EntityState.Modified;
            _ctx.SaveChanges();
            var log = audit.LastLog;
            
            // Assert
            Assert.AreEqual(1, log.Entities.Count, "Change to Money was reconised by aduit");

        }

        [TestMethod]
        public void EFExtendedCodeFirst_Edit_Properties_contains_ComplexType_class()
        {
            // Arrange
            var trx = new Transaction
            {
                Money = new Money(123456789012.3456m, "USD"),
                Detail = "Another Transaction"
            };
            _ctx.Transactions.Add(trx);
            _ctx.SaveChanges();

            //Act
            var audit = _ctx.BeginAudit();
            //trx.Money.Amount = 10;
            trx.Detail = "updated detail";
            var t = _ctx.Set<Transaction>().FirstOrDefault(x => x.Id == trx.Id);
            _ctx.Entry(t).CurrentValues.SetValues(trx);
            _ctx.Entry(t).State = EntityState.Modified;
            _ctx.SaveChanges();
            var log = audit.LastLog;

            // Assert

            var obj = log.Entities[0].Properties;
            Assert.AreEqual(3, obj.Count,"count equals number of properties in Transaction");
            

        }

        [TestMethod]
        public void EFExtendedCodeFirst_Edit_Complex_type_property_Original_and_Current_not_the_same()
        {
            // Arrange
            var trx = new Transaction
            {
                Money = new Money(123456789012.3456m, "USD"),
                Detail = "Another Transaction"
            };
            _ctx.Transactions.Add(trx);
            _ctx.SaveChanges();

            //Act
            var audit = _ctx.BeginAudit();
            trx.Money.Amount = 10;
            trx.Detail = "updated detail";
            var t = _ctx.Set<Transaction>().FirstOrDefault(x => x.Id == trx.Id);
            _ctx.Entry(t).CurrentValues.SetValues(trx);
            _ctx.Entry(t).State = EntityState.Modified;
            _ctx.SaveChanges();
            var log = audit.LastLog;

            // Assert
            Assert.AreNotEqual(log.Entities[0].Properties[1].Original, log.Entities[0].Properties[1].Current,"Current and origional of string property Transaction.Detail are not the same.");
            //cast complex type to DbDataRecord to access its property values.
            var currentMoney = (DbDataRecord)log.Entities[0].Properties[2].Current;
            var origionalMoney = (DbDataRecord)log.Entities[0].Properties[2].Original;
            Assert.AreNotEqual(currentMoney.GetDecimal(1), origionalMoney.GetDecimal(1), "Current and origional of complex type property Transaction.Money.Amount are not the same.");

        }

    }
}
