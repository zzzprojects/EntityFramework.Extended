#Entity Framework Extended Library

A library the extends the functionality of Entity Framework.

##Features


- Batch Update and Delete
- Future Queries
- Audit Log
 
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

### Audit Log

TODO ...

Fluent Configuration
    
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
