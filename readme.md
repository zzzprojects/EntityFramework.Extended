#Entity Framework Extended Library

A library the extends the functionality of Entity Framework.

##Download

The Entity Framework Extended library is available on nuget.org via package name `EntityFramework.Extended`.

To install EntityFramework.Extended, run the following command in the Package Manager Console

    PM> Install-Package EntityFramework.Extended
    
More information about NuGet package avaliable at
https://nuget.org/packages/EntityFramework.Extended

##Features


- [Batch Update and Delete](https://github.com/loresoft/EntityFramework.Extended/wiki/Batch-Update-and-Delete)
- [Future Queries](https://github.com/loresoft/EntityFramework.Extended/wiki/Future-Queries)
- [Query Result Cache](https://github.com/loresoft/EntityFramework.Extended/wiki/Query-Result-Cache)
- [Audit Log](https://github.com/loresoft/EntityFramework.Extended/wiki/Audit-Log)
 
### Batch Update and Delete

A current limitations of the Entity Framework is that in order to update or delete an entity you have to first retrieve it into memory. Now in most scenarios this is just fine. There are however some senerios where performance would suffer. Also, for single deletes, the object must be retrieved before it can be deleted requiring two calls to the database. Batch update and delete eliminates the need to retrieve and load an entity before modifying it.

**Deleting**
    
    //delete all users where FirstName matches
    context.Users.Delete(u => u.FirstName == "firstname");

**Update**
    
    //update all tasks with status of 1 to status of 2
    context.Tasks.Update(
        t => t.StatusId == 1, 
        t2 => new Task {StatusId = 2});
    
    //example of using an IQueryable as the filter for the update
    var users = context.Users.Where(u => u.FirstName == "firstname");
    context.Users.Update(users, u => new User {FirstName = "newfirstname"});

### Future Queries

Build up a list of queries for the data that you need and the first time any of the results are accessed, all the data will retrieved in one round trip to the database server. Reducing the number of trips to the database is a great. Using this feature is as simple as appending `.Future()` to the end of your queries. To use the Future Queries, make sure to import the `EntityFramework.Extensions` namespace. 

Future queries are created with the following extension methods...

- Future()
- FutureFirstOrDefault()
- FutureCount()

Sample

    // build up queries
    var q1 = db.Users
        .Where(t => t.EmailAddress == "one@test.com")
        .Future();
    
    var q2 = db.Tasks
        .Where(t => t.Summary == "Test")
        .Future();
    
    // this triggers the loading of all the future queries
    var users = q1.ToList();


In the example above, there are 2 queries built up, as soon as one of the queries is enumerated, it triggers the batch load of both queries.

     
    // base query
    var q = db.Tasks.Where(t => t.Priority == 2);
    // get total count
    var q1 = q.FutureCount();
    // get page
    var q2 = q.Skip(pageIndex).Take(pageSize).Future();
    
    // triggers execute as a batch
    int total = q1.Value;
    var tasks = q2.ToList();
    

In this example, we have a common senerio where you want to page a list of tasks. In order for the GUI to setup the paging control, you need a total count. With Future, we can batch together the queries to get all the data in one database call.

Future queries work by creating the appropriate IFutureQuery object that keeps the IQuerable. The IFutureQuery object is then stored in IFutureContext.FutureQueries list. Then, when one of the IFutureQuery objects is enumerated, it calls back to IFutureContext.ExecuteFutureQueries() via the LoadAction delegate. ExecuteFutureQueries builds a batch query from all the stored IFutureQuery objects. Finally, all the IFutureQuery objects are updated with the results from the query.

### Query Result Cache

To cache query results, use the `FromCache` extension method located in the `EntityFramework.Extensions` namespace. Below is a sample caching query results. Simply construct the LINQ query as you normally would, then append the `FromCache` extension.
     
    //query is cached using the default settings
    var tasks = db.Tasks
        .Where(t => t.CompleteDate == null)
        .FromCache();
 
    //query result is now cached 300 seconds
    var tasks = db.Tasks
        .Where(t => t.AssignedId == myUserId && t.CompleteDate == null)
        .FromCache(CachePolicy.WithDurationExpiration(TimeSpan.FromSeconds(300)));
        
The Query Result Cache also supports tagging the cache so you can expire common cache entries by calling `Expire` on a cache tag.

    // cache assigned tasks
    var tasks = db.Tasks
        .Where(t => t.AssignedId == myUserId && t.CompleteDate == null)
        .FromCache(tags: new[] { "Task", "Assigned-Task-" + myUserId  });

    // some update happened to Task, expire Task tag
    CacheManager.Current.Expire("Task");
    
The `CacheManager` has support for providers.  The default provider uses `MemoryCache` to store the cache entries.  To create a custom provider, implement `ICacheProvider`. The custom provider will then need to be registered in the `Locator` container.

    // Replace cache provider with Memcached provider
    Locator.Current.Register<ICacheProvider>(() => new MemcachedProvider());

### Audit Log

The Audit Log feature will capture the changes to entities anytime they are submitted to the database. The Audit Log captures only the entities that are changed and only the properties on those entities that were changed. The before and after values are recorded.  `AuditLogger.LastAudit` is where this information is held and there is a `ToXml()` method that makes it easy to turn the AuditLog into xml for easy storage. 

The AuditLog can be customized via attributes on the entities or via a Fluent Configuration API.

Fluent Configuration
    
    // config audit when your application is starting up...
    var auditConfiguration = AuditConfiguration.Default;
    
    auditConfiguration.IncludeRelationships = true;
    auditConfiguration.LoadRelationships = true;
    auditConfiguration.DefaultAuditable = true;
    
    // customize the audit for Task entity
    auditConfiguration.IsAuditable<Task>()
        .NotAudited(t => t.TaskExtended)
        .FormatWith(t => t.Status, v => FormatStatus(v));
    
    // set the display member when status is a foreign key
    auditConfiguration.IsAuditable<Status>()
        .DisplayMember(t => t.Name);

Create an Audit Log

    var db = new TrackerContext();
    var audit = db.BeginAudit();

    // make some updates ...

    db.SaveChanges();
    var log = audit.LastLog;

## License

Copyright (c) 2012, LoreSoft
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

- Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
- Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
- Neither the name of LoreSoft nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.