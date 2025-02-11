FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

USER app
FROM --platform=linux mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG configuration=Release
WORKDIR /src
COPY ["/src/QbitHelper.ServiceDefaults/QbitHelper.ServiceDefaults.csproj", "./src/QbitHelper.ServiceDefaults/"]
COPY ["/src/QbitHelper.App/QbitHelper.App.csproj", "./src/QbitHelper.App/"]
RUN dotnet restore "src/QbitHelper.App/QbitHelper.App.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "src/QbitHelper.App/QbitHelper.App.csproj" -c $configuration -o /app/build

FROM build AS publish
ARG configuration=Release
RUN dotnet publish "src/QbitHelper.App/QbitHelper.App.csproj" -c $configuration -o /app/publish /p:UseAppHost=false

FROM runtime AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "QbitHelper.App.dll"]
