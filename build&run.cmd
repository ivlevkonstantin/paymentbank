dotnet build
start cmd.exe @cmd /k "dotnet run --project ./src/PaymentBank.AccountService/PaymentBank.AccountService.csproj"
start cmd.exe @cmd /k "dotnet run --project ./src/PaymentBank.TransactionService/PaymentBank.TransactionService.csproj"

timeout /T 10

start http://localhost:5000/
start http://localhost:5001/


@pause