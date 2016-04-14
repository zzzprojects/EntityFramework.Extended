using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using EntityFramework.Extensions;
using Xunit;
using Tracker.MySql.Entities;
using EntityFramework;
using EntityFramework.Batch;

namespace Tracker.MySql.Test
{
    public class ExtensionTest
    {
        [Fact]
        public void BeginTransactionObjectContext()
        {
            Locator.Current.Register<IBatchRunner>(() => new MySqlBatchRunner());
            try
            {
                using (var db = new TrackerEntities())
                using (var tx = db.Database.BeginTransaction())
                {
                    string emailDomain = "@test.com";

                    int count = db.users
                        .Where(u => u.EmailAddress.EndsWith(emailDomain))
                        .Update(u => new user { IsApproved = false, LastActivityDate = DateTime.Now });

                    count = db.users
                        .Where(u => u.EmailAddress.EndsWith(emailDomain))
                        .Delete();

                    tx.Commit();
                }
            }
            finally
            {
                Locator.Current.Register<IBatchRunner>(() => new SqlServerBatchRunner());
            }
        }

        [Fact]
        public void NoTransactionObjectContext()
        {
            Locator.Current.Register<IBatchRunner>(() => new MySqlBatchRunner());
            try
            {
                using (var db = new TrackerEntities())
                {
                    string emailDomain = "@test.com";

                    int count = db.users
                        .Where(u => u.EmailAddress.EndsWith(emailDomain))
                        .Update(u => new user { IsApproved = false, LastActivityDate = DateTime.Now });

                    count = db.users
                        .Where(u => u.EmailAddress.EndsWith(emailDomain))
                        .Delete();

                }
            }
            finally
            {
                Locator.Current.Register<IBatchRunner>(() => new SqlServerBatchRunner());
            }
        }

        /*** MySQL seems not to support TransactionScope ***/
        //[Fact]
        //public void TransactionScopeObjectContext()
        //{
        //    using (var tx = new TransactionScope())
        //    using (var db = new TrackerEntities())
        //    {
        //        string emailDomain = "@test.com";

        //        int count = db.users
        //            .Where(u => u.EmailAddress.EndsWith(emailDomain))
        //            .Update(u => new user { IsApproved = false, LastActivityDate = DateTime.Now });

        //        count = db.users
        //            .Where(u => u.EmailAddress.EndsWith(emailDomain))
        //            .Delete();

        //        tx.Complete();
        //    }
        //}


        private void _Insert(TrackerEntities db, bool isAsync = false)
        {
            Locator.Current.Register<IBatchRunner>(() => new MySqlBatchRunner());
            try
            {
                db.productsummaries.Delete();
                var query = from product in db.products
                            join item2 in (
                                 from item in db.items
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
                if (isAsync) db.productsummaries.InsertAsync(query).Wait();
                else db.productsummaries.Insert(query);
                var source = query.ToArray();
                var result = db.productsummaries.ToArray();
                for (int i = 0; i < source.Length; i++)
                {
                    source[i].AvgPrice = Math.Round(source[i].AvgPrice, 2, MidpointRounding.AwayFromZero); //In database, only two digits after decimal point
                    source[i].Verified = true; //Verified was not set in query. In database, its default value is true (1)
                }
                Assert.True(result.OrderBy(i => i.ProductId).SequenceEqual(source.OrderBy(i => i.ProductId), new ProductSummaryComparer()));

                db.item_2.Delete();
                var query2 = db.items.Where(item => item.ListPrice / item.UnitCost >= 5);
                if (isAsync) db.item_2.InsertAsync(query2).Wait();
                else db.item_2.Insert(query2);
                var source2 = query2.ToArray().OrderBy(i => i.ItemId);
                var result2 = db.item_2.ToArray().Select(i => ItemComparer.GetItem(i)).OrderBy(i => i.ItemId);
                Assert.True(result2.SequenceEqual(source2, new ItemComparer()));


                db.item_2.Delete();
                //var query3 = from item in db.items where item.ProductId == "K9-RT-02" select item; //Using MySQL provider, ObjectQuery.Parameters is not filled if its parameter is constant
                string productId = "K9-RT-02";
                var query3 = from item in db.items where item.ProductId == productId select item;
                if (isAsync) db.item_2.InsertAsync(query3).Wait();
                else db.item_2.Insert(query3);
                var source3 = query3.ToArray().OrderBy(item => item.ItemId);
                var result3 = db.item_2.ToArray().Select(i => ItemComparer.GetItem(i)).OrderBy(item => item.ItemId);
                Assert.True(result3.SequenceEqual(source3, new ItemComparer()));
            }
            finally
            {
                Locator.Current.Register<IBatchRunner>(() => new SqlServerBatchRunner());
            }
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

        /*** MySQL seems not to support TransactionScope ***/
        //[Fact]
        //public void InsertInTransactionScope()
        //{
        //    using (var tx = new TransactionScope())
        //    using (var db = new TrackerEntities())
        //    {
        //        _Insert(db);
        //        tx.Complete();
        //    }
        //}

        [Fact]
        public void InsertAsync()
        {
            using (var db = new TrackerEntities())
            {
                _Insert(db, true);
            }
        }
    }

    class ProductSummary2 : productsummary { }

    class ProductSummaryComparer : IEqualityComparer<productsummary>, System.Collections.IComparer
    {
        public int Compare(object x, object y)
        {
            if (x == null && y == null) return 0;
            var x2 = x as productsummary;
            var y2 = y as productsummary;
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

        public bool Equals(productsummary x, productsummary y)
        {
            return Compare(x, y) == 0;
        }

        public int GetHashCode(productsummary obj)
        {
            if (obj == null) return 0;
            return obj.ProductId.GetHashCode();
        }
    }

    class ItemComparer : IEqualityComparer<item>, System.Collections.IComparer
    {
        public int Compare(object x, object y)
        {
            if (x == null && y == null) return 0;
            if (x == null || y == null) return -1;
            item item = GetItem(x), item2 = GetItem(y);
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

        public bool Equals(item x, item y)
        {
            return Compare(x, y) == 0;
        }

        public int GetHashCode(item obj)
        {
            return obj.ItemId.GetHashCode();
        }

        public static item GetItem(object obj)
        {
            if (obj is item) return obj as item;
            if (obj is item_2)
            {
                var item2 = obj as item_2;
                return new item
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
