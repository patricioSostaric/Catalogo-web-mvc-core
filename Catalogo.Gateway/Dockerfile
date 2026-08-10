# El gateway no depende de la biblioteca compartida: solo enruta, no toca datos.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Catalogo.Gateway/Catalogo.Gateway.csproj Catalogo.Gateway/
RUN dotnet restore Catalogo.Gateway/Catalogo.Gateway.csproj

COPY Catalogo.Gateway/ Catalogo.Gateway/
RUN dotnet publish Catalogo.Gateway/Catalogo.Gateway.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Catalogo.Gateway.dll"]
