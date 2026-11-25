# ---------- Build stage: has the full .NET SDK ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy only project files first, so 'restore' is cached until dependencies change
COPY src/Churn.Core/Churn.Core.csproj src/Churn.Core/
COPY src/Churn.Data/Churn.Data.csproj src/Churn.Data/
COPY src/Churn.Api/Churn.Api.csproj src/Churn.Api/
RUN dotnet restore src/Churn.Api/Churn.Api.csproj

# Then copy the source code and publish
COPY src/ src/
RUN dotnet publish src/Churn.Api/Churn.Api.csproj -c Release -o /app --no-restore

# ---------- Runtime stage: small image with only the ASP.NET runtime ----------
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .

EXPOSE 8080
ENTRYPOINT ["dotnet", "Churn.Api.dll"]