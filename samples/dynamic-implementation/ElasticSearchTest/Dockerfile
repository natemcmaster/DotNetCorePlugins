FROM microsoft/dotnet:2.1-runtime AS base
WORKDIR /app

FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /src
COPY ElasticSearchTest/ElasticSearchTest.csproj ElasticSearchTest/
RUN dotnet restore ElasticSearchTest/ElasticSearchTest.csproj
COPY . .
WORKDIR /src/ElasticSearchTest
RUN dotnet build ElasticSearchTest.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish ElasticSearchTest.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "ElasticSearchTest.dll"]
