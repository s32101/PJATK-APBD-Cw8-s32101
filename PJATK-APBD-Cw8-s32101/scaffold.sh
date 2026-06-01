#!/bin/bash

read -p "Wprowadź hasło do bazy danych: " PASS
export PASS

dotnet ef dbcontext scaffold "Server=localhost,1433;Database=Hospital;User Id=sa;Password=$(echo $PASS);TrustServerCertificate=True" Microsoft.EntityFrameworkCore.SqlServer  --no-onconfiguring --output-dir Models
