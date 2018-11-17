using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using EntityFramework.Extensions;
using Xunit;
using Tracker.SqlServer.Entities;

namespace Tracker.SqlServer.Test
{
    public class ExtensionTest
    {
        [Fact]
        public void BeginTransactionObjectContext()
        {
            using (var db = new TrackerEntities())
            using (var tx = db.Database.BeginTransaction())
            {
                string emailDomain = "@test.com";

                int count = db.Users
                    .Where(u => u.EmailAddress.EndsWith(emailDomain))
                    .Update(u => new User { IsApproved = false, LastActivityDate = DateTime.Now });

                count = db.Users
                    .Where(u => u.EmailAddress.EndsWith(emailDomain))
                    .Delete();

                tx.Commit();
            }
        }

        [Fact]
        public void NoTransactionObjectContext()
        {
            using (var db = new TrackerEntities())
            {
                string emailDomain = "@test.com";

                int count = db.Users
                    .Where(u => u.EmailAddress.EndsWith(emailDomain))
                    .Update(u => new User { IsApproved = false, LastActivityDate = DateTime.Now });

                count = db.Users
                    .Where(u => u.EmailAddress.EndsWith(emailDomain))
                    .Delete();

            }
        }

        [Fact]
        public void TransactionScopeObjectContext()
        {
            using (var tx = new TransactionScope())
            using (var db = new TrackerEntities())
            {
                string emailDomain = "@test.com";

                int count = db.Users
                    .Where(u => u.EmailAddress.EndsWith(emailDomain))
                    .Update(u => new User { IsApproved = false, LastActivityDate = DateTime.Now });

                count = db.Users
                    .Where(u => u.EmailAddress.EndsWith(emailDomain))
                    .Delete();

                tx.Complete();
            }
        }


        private void _Insert(TrackerEntities db, bool isAsync=false)
        {
            db.Database.Log = s => System.Diagnostics.Trace.WriteLine(s);
            db.ProductSummaries.Delete();
            var query = from product in db.Products
                        join item2 in (
                             from item in db.Items
                             group item by item.ProductId into grItem
                             select new
                             {
                                 ProductId = grItem.Key,
                                 AvgPrice = grItem.Average(x => x.ListPrice + x.UnitCost)
                             }
                         ) on product.ProductId equals item2.ProductId into items
                        from item3 in items.DefaultIfEmpty()
                        select new ProductSummary2
                        {
                            ProductId = product.ProductId,
                            Name = product.Name,
                            AvgPrice = item3.AvgPrice ?? 0
                        };
            if (isAsync) db.ProductSummaries.InsertAsync(query).Wait();
            else db.ProductSummaries.Insert(query);
            var source = query.ToArray();
            var result = db.ProductSummaries.ToArray();
            for (int i = 0; i < source.Length; i++)
            {
                source[i].AvgPrice = Math.Round(source[i].AvgPrice, 2, MidpointRounding.AwayFromZero); //In database, only two digits after decimal point
                source[i].Verified = true; //Verified was not set in query. In database, its default value is true (1)
            }
            Assert.True(result.OrderBy(i => i.ProductId).SequenceEqual(source.OrderBy(i => i.ProductId), new ProductSummaryComparer()));

            db.Item_2.Delete();
            var query2 = db.Items.Where(item => item.ListPrice / item.UnitCost >= 5);
            if (isAsync) db.Item_2.InsertAsync(query2).Wait();
            else db.Item_2.Insert(query2);
            var source2 = query2.ToArray().OrderBy(i => i.ItemId);
            var result2 = db.Item_2.ToArray().Select(i => ItemComparer.GetItem(i)).OrderBy(i => i.ItemId);
            Assert.True(result2.SequenceEqual(source2, new ItemComparer()));


            db.Item_2.Delete();
            var query3 = from item in db.Items where item.ProductId == "K9-RT-02" select item;
            if (isAsync) db.Item_2.InsertAsync(query3).Wait();
            else db.Item_2.Insert(query3);
            var source3 = query3.ToArray().OrderBy(item => item.ItemId);
            var result3 = db.Item_2.ToArray().Select(i => ItemComparer.GetItem(i)).OrderBy(item => item.ItemId);
            Assert.True(result3.SequenceEqual(source3, new ItemComparer()));
        }

        [Fact]
        public void InsertNoTransaction()
        {
            using (var db = new TrackerEntities())
            {
                _Insert(db);
            }
        }

        [Fact]
        public void InsertInTransaction()
        {
            using (var db = new TrackerEntities())
            using (var tx = db.Database.BeginTransaction())
            {
                _Insert(db);
                tx.Commit();
            }
        }

        [Fact]
        public void InsertInTransactionScope()
        {
            using (var tx = new TransactionScope())
            using (var db = new TrackerEntities())
            {
                _Insert(db);
                tx.Complete();
            }
        }

        [Fact]
        public void InsertAsync()
        {
            using (var db = new TrackerEntities())
            {
                _Insert(db, true);
            }
        }

        private void _InsertBulk(TrackerEntities db, bool isAsync = false)
        {
            db.Database.Log = s => System.Diagnostics.Trace.WriteLine(s);
            db.ProductSummaries.Delete();
            db.Database.ExecuteSqlCommand("TRUNCATE TABLE ATable");

            //Create 1000 random records
            Random r = new Random();
            int n = 1000;
            var entities = new List<ProductSummary>();
            int m = (int)Math.Pow(10, 8) - 1;
            for (int i = 1; i <= n; i++)
            {
                entities.Add(new ProductSummary
                {
                    ProductId = i.ToString(),
                    Name = Guid.NewGuid().ToString().Replace("-", "").PadRight(80).Substring(0, 80),
                    AvgPrice = r.Next(-m, m),
                    Verified = r.Next() % 2 == 0,
                    Date = DateTime.Now.AddDays(r.Next(-100, 100)),
                });
            }

            var start = DateTime.Now;

            if (isAsync) db.ProductSummaries.InsertAsync(entities).Wait();
            else db.ProductSummaries.Insert(entities);

            /**
             * Compare the execution time with these two lines of commented code below
            // * **/
            //db.ProductSummaries.AddRange(entities);
            //db.SaveChanges();

            System.Diagnostics.Trace.WriteLine("***** Executing bulk insert (" + n + " items) takes "
                + DateTime.Now.Subtract(start).ToString(@"hh\:mm\:ss\.ffffff"));

            Assert.True(db.ProductSummaries.Count() == n);

            /** Dealing with Identity Column **/
            var entities2 = new List<ATable>();
            for (int i = 1; i <= n; i++)
            {
                entities2.Add(new ATable
                {
                    A = r.NextDouble(),
                    B = Guid.NewGuid().ToString().Replace("-", "").PadRight(50).Substring(0, 50),
                    //C is an Identity column
                    D = DateTime.Now.AddDays(r.Next(-100, 100)),
                    E = r.Next() % 2 == 0,
                });
            }

            if (isAsync) db.ATables.InsertAsync(entities2).Wait();
            else db.ATables.Insert(entities2);
            Assert.True(db.ATables.Count() == n);
        }

        [Fact]
        public void InsertBulkNoTransaction()
        {
            using (var db = new TrackerEntities())
            {
                _InsertBulk(db);
            }
        }

        [Fact]
        public void InsertBulkInTransaction()
        {
            using (var db = new TrackerEntities())
            using (var tx = db.Database.BeginTransaction())
            {
                _InsertBulk(db);
                tx.Commit();
            }
        }

        [Fact]
        public void InsertBulkInTransactionScope()
        {
            using (var tx = new TransactionScope())
            using (var db = new TrackerEntities())
            {
                _InsertBulk(db);
                tx.Complete();
            }
        }

        [Fact]
        public void InsertBulkAsync()
        {
            using (var db = new TrackerEntities())
            {
                _InsertBulk(db, true);
            }
        }
    }

    class ProductSummary2 : ProductSummary { }

    class ProductSummaryComparer : IEqualityComparer<ProductSummary>, System.Collections.IComparer
    {
        public int Compare(object x, object y)
        {
            if (x == null && y == null) return 0;
            var x2 = x as ProductSummary;
            var y2 = y as ProductSummary;
            if (x2 != null && y2 != null)
            {
                if (x2.ProductId == y2.ProductId
                 && x2.Name == y2.Name
                 && x2.AvgPrice == y2.AvgPrice
                 && x2.Verified == y2.Verified)
                    return 0;
            }
            return -1;
        }

        public bool Equals(ProductSummary x, ProductSummary y)
        {
            return Compare(x, y) == 0;
        }

        public int GetHashCode(ProductSummary obj)
        {
            if (obj == null) return 0;
            return obj.ProductId.GetHashCode();
        }
    }

    class ItemComparer : IEqualityComparer<Item>, System.Collections.IComparer
    {
        public int Compare(object x, object y)
        {
            if (x == null && y == null) return 0;
            if (x == null || y == null) return -1;
            Item item = GetItem(x), item2 = GetItem(y);
            if (item.ItemId == item2.ItemId
                && item.ProductId == item2.ProductId
                && item.ListPrice == item2.ListPrice
                && item.UnitCost == item2.UnitCost
                && item.Supplier == item2.Supplier
                && item.Status == item2.Status
                && item.Attr1 == item2.Attr1
                && item.Attr2 == item2.Attr2
                && item.Attr3 == item2.Attr3
                && item.Attr4 == item2.Attr4
                && item.Attr5 == item2.Attr5) return 0;
            return -1;
        }

        public bool Equals(Item x, Item y)
        {
            return Compare(x, y) == 0;
        }

        public int GetHashCode(Item obj)
        {
           return obj.ItemId.GetHashCode();
        }

        public static Item GetItem(object obj)
        {
            if (obj is Item) return obj as Item;
            if (obj is Item_2)
            {
                var item2 = obj as Item_2;
                return new Item
                {
                    ItemId = item2.ItemId,
                    ProductId = item2.ProductId,
                    ListPrice = item2.ListPrice,
                    UnitCost = item2.UnitCost,
                    Supplier = item2.Supplier,
                    Status = item2.Status,
                    Attr1 = item2.Attr1,
                    Attr2 = item2.Attr2,
                    Attr3 = item2.Attr3,
                    Attr4 = item2.Attr4,
                    Attr5 = item2.Attr5
                };
            }
            return null;
        }
    }
}
