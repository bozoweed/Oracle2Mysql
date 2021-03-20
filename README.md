# Oracle2Mysql
Software to convert oracle sql dump to mysql compatible sql
This softawre is made to help you to migrate from oracle to myslq easly and quickly

# How it's work
- it's going to read a sql file made from oracle 
- looking for all "create table" and put them in first sql file (it's for avoid the possibility to made request who need an other table and so get error on injection )
- looking for all register regex from oracle to replace them in good mysql one 
- generate some sql with X line ( define in config.js)
- all file was create and converted by mulithread process to made it faster 
- store all sql in Folder AllSql ( generate and clean up at every convertion )  

# Required
- MariaDb 10x and more
- NetCore3.1 or more
- Navicate ( Recommanded than phpmyadmin for insert sql )

# How to do
- first made dump of your oracle database
- put the dump in folder of convertor ( the folder of Oracle2Mysql.exe file ) 
- run Oracle2Mysql.exe as admin 
- enter the name of you dump to convert ( for exemple my_databas_dump.sql)
- then what for end and open AllSql folder 
- to finish insert sql file by name order ( sql0.sql => sql1.sql => .....)
