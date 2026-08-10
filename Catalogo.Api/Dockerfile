# El contexto de construccion es la raiz del repositorio, porque este proyecto
# depende de la biblioteca compartida y hay que copiar las dos.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Primero los csproj: mientras no cambien las dependencias, Docker reusa el restore.
COPY Catalogo.Datos/Catalogo.Datos.csproj Catalogo.Datos/
COPY Catalogo.Api/Catalogo.Api.csproj Catalogo.Api/
RUN dotnet restore Catalogo.Api/Catalogo.Api.csproj

COPY Catalogo.Datos/ Catalogo.Datos/
COPY Catalogo.Api/ Catalogo.Api/
RUN dotnet publish Catalogo.Api/Catalogo.Api.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Las fechas de la auditoria se calculan sobre la zona horaria de Argentina, que en
# Linux necesita la base de zonas instalada.
RUN apt-get update \
    && apt-get install -y --no-install-recommends tzdata \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Catalogo.Api.dll"]
