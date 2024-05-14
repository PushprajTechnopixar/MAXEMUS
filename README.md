# Code Scaffolding of Entity Framework context from db
in Terminal
```
dotnet tool install --global dotnet-ef
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
```
To generate Models and DbContext
```

dotnet ef dbcontext scaffold "Server=DESKTOP-G14R8VA;Database=MaxemusAPI;User Id=sa;Password=;Integrated Security=true;TrustServerCertificate=True;MultipleActiveResultSets=true" Microsoft.EntityFrameworkCore.SqlServer --output-dir .\Models --namespace MaxemusAPI.Models --context-namespace MaxemusAPI.Models --context-dir .\Models --context ApplicationDbContext --force --no-build --no-pluralize --no-onconfiguring


server dbcontext

dotnet ef dbcontext scaffold "Server=MaxemusAPI.ctqf1e2kdq7l.us-east-2.rds.amazonaws.com,1433;Database=MaxemusAPI;User Id=admin;Password=%YgmB4P+UT&XEck6;MultipleActiveResultSets=true" Microsoft.EntityFrameworkCore.SqlServer --output-dir .\Models --namespace MaxemusAPI.Models --context-namespace MaxemusAPI.Models --context-dir .\Models --context ApplicationDbContext --force --no-build --no-pluralize --no-onconfiguring



dotnet ef dbcontext scaffold "Server=maxemus.c9sio040mxfk.ap-south-1.rds.amazonaws.com,1433;Database=Maxemus;User Id=admin;Password=Absolve_123;MultipleActiveResultSets=true" Microsoft.EntityFrameworkCore.SqlServer --output-dir .\Models --namespace MaxemusAPI.Models --context-namespace MaxemusAPI.Models --context-dir .\Models --context ApplicationDbContext --force --no-build --no-pluralize --no-onconfiguring

dotnet ef dbcontext scaffold "Server=DESKTOP-G14R8VA;Database=Dermastation;User Id=sa;Password=;Integrated Security=true;TrustServerCertificate=True;MultipleActiveResultSets=true" Microsoft.EntityFrameworkCore.SqlServer --output-dir .\Models --namespace MaxemusAPI.Models --context-namespace MaxemusAPI.Models --context-dir .\Models --context ApplicationDbContext --force --no-build --no-pluralize --no-onconfiguring


```
pushprajsuperadmin@gmail.com
Admin@123

pushprajadminuser@gmail.com
Jif@1qt01L

pushprajvendor@gmail.com
Jif@1Zmb5L

"emailOrPhone": "superadmin@MaxemusAPI.com",
"password": "Admin@123"



    "DefaultSQLConnection": "Data Source=DESKTOP-G14R8VA;Initial Catalog=MaxemusAPI;Integrated Security=True;TrustServerCertificate=True",

    "DefaultSQLConnection": "Server=maminadb.ch2ibj99tl2o.us-east-2.rds.amazonaws.com,1433;Initial Catalog=MaxemusAPI;Persist Security Info=False;User ID=admin;Password=kJpwdEfe#YL&LV9r;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=True;Connection Timeout=30;"
