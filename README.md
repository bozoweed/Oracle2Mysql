# Oracle2Mysql
Software to convert oracle sql dump to mysql compatible sql
This softawre is made to help you to migrate from oracle to myslq easly and quickly

# How it's work
- it's going to read a sql file made from oracle 
- looking for all "create table" and put them in first sql file (it's for avoid the possibility to made request who need an other table and so get errors on injections )
- looking for all register regex from oracle to replace them in the good mysql
- generate some sql with X line ( define in config.js)
- all file was created and converted by multithread process to make it faster 
- store all sql in Folder AllSql ( generate and clean up at every convertion )  

# Required
- MariaDb 10x and more
- NetCore3.1 or more
- Navicate ( More recommanded than phpmyadmin for insert sql )

# How to do
- first make dump of your oracle database
- put the dump in folder of convertor ( the folder of Oracle2Mysql.exe file ) 
- run Oracle2Mysql.exe as admin 
- enter the name of you dump to convert ( for exemple my_databas_dump.sql)
- then wait for an ending and open AllSql folder 
- to finish insert sql file by this name order ( sql0.sql => sql1.sql => .....)
