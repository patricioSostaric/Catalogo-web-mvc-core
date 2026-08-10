# Etapa del front: compila React a archivos estaticos. Node no viaja a la imagen
# final, igual que el SDK de .NET: solo se usa para producir el resultado.
FROM node:24-alpine AS front
WORKDIR /front

# Mismo criterio que con el csproj: se copia primero el manifiesto para que Docker
# cachee la instalacion de dependencias mientras no cambien.
COPY catalogo-front/package.json catalogo-front/package-lock.json ./
RUN npm ci

COPY catalogo-front/ ./
# vite.config.js publica en ../catalogo-web-mvc/wwwroot/app, que aca queda en
# /catalogo-web-mvc/wwwroot/app.
RUN npm run build

# Etapa de compilación: usa el SDK completo, que no viaja a la imagen final.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Se copia primero el csproj solo para que Docker cachee el restore mientras
# no cambien las dependencias. Sin esto, cada cambio de código rehace el restore.
COPY Catalogo.Datos/Catalogo.Datos.csproj Catalogo.Datos/
COPY catalogo-web-mvc/catalogo-web-mvc.csproj catalogo-web-mvc/
RUN dotnet restore catalogo-web-mvc/catalogo-web-mvc.csproj

COPY Catalogo.Datos/ Catalogo.Datos/
COPY catalogo-web-mvc/ catalogo-web-mvc/

# El front compilado entra a wwwroot antes de publicar, asi viaja dentro de la
# imagen como un archivo estatico mas. Si faltara este paso la aplicacion
# arrancaria igual y /app responderia 404.
COPY --from=front /catalogo-web-mvc/wwwroot/app catalogo-web-mvc/wwwroot/app

RUN dotnet publish catalogo-web-mvc/catalogo-web-mvc.csproj -c Release -o /app/publish

# Etapa final: solo el runtime de ASP.NET, mucho más liviana.
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# TimeZoneInfo pide "Argentina Standard Time" (ID de Windows). En Linux .NET lo
# traduce vía ICU, pero necesita la base de zonas horarias instalada.
RUN apt-get update \
    && apt-get install -y --no-install-recommends tzdata \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

# Kestrel escucha en 8080; el puerto se publica desde docker-compose.
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "catalogo-web-mvc.dll"]
