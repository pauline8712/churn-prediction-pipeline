FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/Churn.Core/Churn.Core.csproj src/Churn.Core/
COPY src/Churn.Data/Churn.Data.csproj src/Churn.Data/
COPY src/Churn.Training/Churn.Training.csproj src/Churn.Training/
RUN dotnet restore src/Churn.Training/Churn.Training.csproj

COPY src/ src/
RUN dotnet publish src/Churn.Training/Churn.Training.csproj -c Release -o /app --no-restore

# Console app: the plain .NET runtime is enough
FROM mcr.microsoft.com/dotnet/runtime:10.0
WORKDIR /app
COPY --from=build /app .

ENTRYPOINT ["dotnet", "Churn.Training.dll"]