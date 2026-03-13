FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY BalanzaService/BalanzaService.csproj BalanzaService/
RUN dotnet restore BalanzaService/BalanzaService.csproj -r linux-x64

COPY BalanzaService/ BalanzaService/
RUN dotnet publish BalanzaService/BalanzaService.csproj -c Release -r linux-x64 --self-contained -o /app/publish

FROM mcr.microsoft.com/dotnet/runtime-deps:8.0
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:80
ENV DOTNET_RUNNING_IN_CONTAINER=true

EXPOSE 80

ENTRYPOINT ["./BalanzaService"]
