FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/Churn.Core/Churn.Core.csproj src/Churn.Core/
COPY src/Churn.Data/Churn.Data.csproj src/Churn.Data/
COPY src/Churn.Simulator/Churn.Simulator.csproj src/Churn.Simulator/
RUN dotnet restore src/Churn.Simulator/Churn.Simulator.csproj

COPY src/ src/
RUN dotnet publish src/Churn.Simulator/Churn.Simulator.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/runtime:10.0
WORKDIR /app
COPY --from=build /app .

ENTRYPOINT ["dotnet", "Churn.Simulator.dll"]