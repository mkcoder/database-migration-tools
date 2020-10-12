# database-migration-tools
A set of tools to allow organizations to orchestrate database schema and data migration with clear insight into what is deployed in which system. 


 My thoughts on a database orchestration. 

Database orchestration is a very complicated and very involved process, matching code to sql. 




My thoughts are that schema changes are DDL where data migration consists of DDL, DCL, TCL. What do i mean by this, if you have a scheme change that is non-breaking and backwards compatible with your system you can deploy this code anytime without downtime. Whereas the other statements can have a strain on your system due the resources they may consume or just not being able to insert data while the application is in use. One good approach to this may be having two separate write nodes that will eventually sync up to your read nodes. Have a time frame in which data migration is applied to your system. 


The job of this tool is to make it possible for you to migrate your schema and data, by applying patches to the initial creates. 

Initial 	This is only ever run once and will not run again. This is use to create your initial database scheme, how your table should look like. Or day 0 schema create. 
Pre	These are before any apply is run, could be drop columns or setup system admin rights or take the database down for some time 
Apply	These are small single isolated forward only backward compatible changes any new files will be applied granted they are presented in the json schema for this 
Post	These files run after all the patches have been applied to the database. 

The above are only schema changes, should not contain any insert, update, delete statements as these will be resolved using the database data migration tool. Which will have transactions rollback, resilience and also ability to do scheduled data migration. 
