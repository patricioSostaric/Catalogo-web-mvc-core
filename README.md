# Catálogo Web — ASP.NET MVC Core (.NET 10)

![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet&logoColor=white)
![C#](https://img.shields.io/badge/C%23-13-239120?logo=csharp&logoColor=white)
![EF Core](https://img.shields.io/badge/EF%20Core-10.0-512BD4)
![SQL Server](https://img.shields.io/badge/SQL%20Server-CC2927?logo=microsoftsqlserver&logoColor=white)
![Docker](https://img.shields.io/badge/Docker-ready-2496ED?logo=docker&logoColor=white)
![Tests](https://img.shields.io/badge/tests-409%20passing-success)
![License](https://img.shields.io/badge/license-MIT-blue)

Aplicación web de catálogo de artículos con área pública y panel de administración,
construida en ASP.NET MVC Core sobre .NET 10.

> **Este proyecto es la migración de una aplicación previa en ASP.NET WebForms.**
> El repositorio original sigue publicado como punto de partida:
> [catalogo-web-webforms](https://github.com/patricioSostaric/catalogo-web-webforms)

---

## 🔗 Demo

**Demo en producción:** **https://catalogo-web-wekfqt.azurewebsites.net**

> Corre en el tier gratuito de Azure, que no mantiene la aplicación despierta. Si estuvo
> inactiva, **el primer acceso puede tardar entre 20 y 30 segundos**; a partir de ahí
> responde con normalidad.

**Usuario de prueba**

| Rol | Email | Contraseña |
| --- | --- | --- |
| Usuario | `usuario@catalogo.com` | `Usuario@1234` |

Con esa cuenta se puede navegar el catálogo, marcar favoritos y editar el perfil.

**¿Querés ver el panel de administración?** El ABM de artículos, marcas y categorías, el
control de stock y el registro de auditoría quedan detrás del rol `Admin`. Escribime y te
doy acceso — así la demo se mantiene en pie para el resto de las visitas:

- **LinkedIn:** [patricio-sostaric](https://www.linkedin.com/in/patricio-sostaric-187701248/)
- **Mail:** patriciosostaric923@gmail.com

---

## 📸 Capturas

<!-- TODO: agregar las imágenes a la carpeta screenShots/ y descomentar -->

<!--
| Home — catálogo público | Detalle de artículo |
| --- | --- |
| ![Home](screenShots/home.png) | ![Detalle](screenShots/detalle.png) |

| Administración de artículos | Filtro avanzado |
| --- | --- |
| ![Admin](screenShots/admin-articulos.png) | ![Filtro](screenShots/filtro-avanzado.png) |

| Favoritos | Login |
| --- | --- |
| ![Favoritos](screenShots/favoritos.png) | ![Login](screenShots/login.png) |
-->

---

## ✨ Funcionalidades

### Público (sin autenticación)
- Catálogo de artículos con paginado
- Búsqueda por nombre
- Filtro avanzado por código, nombre, precio, stock, marca y categoría, con criterios
  (*contiene*, *comienza con*, *termina con*, *mayor a*, *menor a*, *igual a*)
- Detalle de artículo — solo expone artículos activos y únicamente los campos públicos

### Usuario autenticado
- Registro con mail de bienvenida
- Login con bloqueo tras intentos fallidos
- Restablecimiento de contraseña por mail
- Favoritos: marcar y desmarcar artículos, con listado propio paginado

### Administrador
- ABM completo de artículos, marcas y categorías
- Baja lógica de artículos mediante la propiedad `Activo` (no se borran filas)
- Control de stock — los artículos sin stock no se publican
- Registro de auditoría de operaciones, con listado paginado

---

## 🧱 Arquitectura

```
Navegador
   │
   ▼
Middleware pipeline ── Localization (es-AR) → HTTPS → Security headers
   │                   → Routing → RateLimiter → Authentication → Authorization
   ▼
Controller ──── recibe, valida, decide. Sin lógica de negocio.
   ▼
Service ─────── reglas de negocio, filtros, armado de ViewModels
   ▼
Repository ──── acceso a datos. La única capa que conoce EF Core.
   ▼
CatalogoContext (DbContext)
   ▼
SQL Server
```

Cada capa se comunica con la de abajo **a través de una interfaz**, resuelta por el
contenedor de inyección de dependencias. Las vistas nunca reciben entidades de EF Core,
siempre ViewModels.

📖 **[ARQUITECTURA.md](ARQUITECTURA.md)** documenta el diseño en detalle: el flujo de una
request, cada capa, los módulos transversales y las decisiones de diseño con su
justificación.

### Estructura del proyecto

```
catalogo-web-mvc/
├── Controllers/          Home, Articulo, Marcas, Categorias, Favoritos, AuditLog, Account
├── Interfaces/           Contratos por módulo: Articulos, Marcas, Categorias, Audit, Email
├── Services/             Lógica de negocio + Email, Audit, Identity
├── Repository/           Acceso a datos por entidad
├── Data/                 CatalogoContext y DbSeeder
├── Models/
│   ├── ViewModels/       Contratos con las vistas
│   └── Settings/         Configuración tipada (SmtpSettings)
├── Migrations/           9 migraciones versionadas de EF Core
├── Views/                Razor, con partials reutilizables
└── Program.cs            Registro de DI y pipeline de middleware

CatalogoWeb.tests/        409 tests unitarios
```

---

## 🛠️ Stack técnico

**Framework y lenguaje**
- .NET 10 · C# · ASP.NET MVC Core · Razor

**Datos**
- Entity Framework Core 10 (`Microsoft.EntityFrameworkCore.SqlServer`)
- SQL Server · `Microsoft.Data.SqlClient`
- Migraciones versionadas con EF Core Tools

**Autenticación y autorización**
- ASP.NET Core Identity (`Microsoft.AspNetCore.Identity.EntityFrameworkCore`)
- Roles `Admin` y `Usuario`, con autorización por atributos

**Testing**
- xUnit 2.9 · Moq 4.20 · `EntityFrameworkCore.InMemory` · coverlet

**Otros**
- X.PagedList 10.5 para el paginado
- Bootstrap · HTML5 · CSS3 · JavaScript

---

## 🧪 Testing

**409 tests unitarios, la totalidad en verde.**

```bash
dotnet test
# Correctas! - Con error: 0, Superado: 409, Omitido: 0, Total: 409
```

Cobertura por capa:

| Área | Archivos de test |
| --- | --- |
| Controllers | Account, Articulo, AuditLog, Categorias, Favoritos, Home, Marcas, AuthorizationAttribute |
| Services | Articulo, Categoria, Marca, EmailTemplates, SmtpEmailSender, SpanishIdentityErrorDescriber |
| Repository | Articulo, Categoria, Marca |
| Data | DbSeeder |

Los repositorios se prueban contra `EntityFrameworkCore.InMemory`; los servicios y
controladores, con dobles construidos con **Moq**. El envío de mails es testeable porque
`SmtpClient` está desacoplado tras una interfaz propia (`ISmtpClient`).

---

## 🔒 Seguridad

Se aplicó una auditoría sobre **OWASP Top 10** y se corrigieron los hallazgos:

| Medida | Qué mitiga |
| --- | --- |
| Cookies `HttpOnly`, `Secure`, `SameSite=Strict` | Robo de sesión vía XSS y ataques CSRF |
| Rate limiting (10 req/min en autenticación) | Fuerza bruta sobre el login |
| Lockout tras 5 intentos fallidos | Fuerza bruta sobre credenciales |
| Política de contraseñas (8+, mayúscula, minúscula, dígito, símbolo) | Credenciales débiles |
| `Content-Security-Policy` | Inyección de scripts y recursos externos |
| `X-Frame-Options: DENY` | Clickjacking |
| `X-Content-Type-Options: nosniff` | MIME sniffing |
| `Referrer-Policy` | Fuga de información en la cabecera Referer |
| HSTS + redirección a HTTPS | Degradación a HTTP |
| ViewModels en lugar de entidades | Over-posting / mass assignment |
| User Secrets y configuración externa | Credenciales versionadas en el repositorio |
| EF Core con consultas parametrizadas | Inyección SQL |

---

## 🚀 Ejecución local

### Requisitos
- [.NET SDK 10.0](https://dotnet.microsoft.com/download)
- SQL Server (LocalDB, Express o instancia completa)

> ¿Preferís no instalar nada? Ver [Ejecución con Docker](#-ejecución-con-docker):
> un solo comando, sin .NET ni SQL Server en la máquina.

### Pasos

**1 · Clonar**
```bash
git clone https://github.com/patricioSostaric/Catalogo-web-mvc-core.git
cd Catalogo-web-mvc-core/catalogo-web-mvc
```

**2 · Configurar la cadena de conexión y el SMTP**

Las credenciales no se versionan. En desarrollo se cargan con User Secrets:

```bash
dotnet user-secrets init

dotnet user-secrets set "ConnectionStrings:CatalogoDB" \
  "Server=(localdb)\\MSSQLLocalDB;Database=CatalogoDB;Trusted_Connection=True;TrustServerCertificate=True"

dotnet user-secrets set "Smtp:Host"      "smtp.gmail.com"
dotnet user-secrets set "Smtp:Port"      "587"
dotnet user-secrets set "Smtp:EnableSsl" "true"
dotnet user-secrets set "Smtp:User"      "tu-cuenta@gmail.com"
dotnet user-secrets set "Smtp:Password"  "tu-app-password"
dotnet user-secrets set "Smtp:FromName"  "Catálogo Web"
```

> Con Gmail hay que usar una **contraseña de aplicación**, no la de la cuenta.
> La app arranca igual sin configurar SMTP; solo fallará el envío de mails.

**3 · Aplicar las migraciones**
```bash
dotnet ef database update
```

**4 · Ejecutar**
```bash
dotnet run
```

Al primer arranque, `DbSeeder` crea los roles y los usuarios de prueba de la tabla de arriba.

**5 · Correr los tests** (desde la raíz del repositorio)
```bash
dotnet test
```

---

## 🐳 Ejecución con Docker

La alternativa al paso a paso anterior: **no requiere tener instalados .NET ni SQL Server**,
solo [Docker Desktop](https://www.docker.com/products/docker-desktop).

```bash
docker compose up --build
```

Eso levanta dos contenedores: la aplicación y una instancia de SQL Server 2022. Al arrancar
se aplican las migraciones y se siembran los usuarios de prueba de la tabla del comienzo.
La app queda en **http://localhost:8080**.

La primera ejecución descarga alrededor de 2 GB de imágenes base; las siguientes son inmediatas.

| Comando | Efecto |
| --- | --- |
| `docker compose up --build` | Levanta el entorno completo |
| `docker compose down` | Detiene los contenedores y **conserva** la base y los avatares |
| `docker compose down -v` | Detiene y **borra** los volúmenes: entorno desde cero |
| `docker compose logs -f web` | Sigue los logs de la aplicación |

> ⚠️ **`docker-compose.yml` es exclusivamente para desarrollo local.** Fija
> `ASPNETCORE_ENVIRONMENT=Development` y trae la contraseña de `sa` escrita en el archivo.
> Para desplegar hay que sumarle `docker-compose.prod.yml` (ver abajo).

**Sobre las credenciales del compose:** la contraseña del usuario `sa` está escrita en
`docker-compose.yml` de forma deliberada. Es un entorno de desarrollo local descartable,
sin exposición a red pública, y tenerla a la vista es lo que permite levantar el proyecto
con un solo comando.

**Diferencia con la ejecución local:** en `Development` la cookie de sesión usa
`SameAsRequest` en lugar de `Always`, porque el contenedor sirve HTTP plano. En producción
se mantiene `Always`, exigiendo HTTPS.

### Despliegue

`docker-compose.prod.yml` se superpone al base y cambia lo que no puede quedar como en
desarrollo:

```bash
cp .env.example .env    # completar con valores reales
docker compose -f docker-compose.yml -f docker-compose.prod.yml up -d
```

| Qué cambia | Por qué |
| --- | --- |
| `ASPNETCORE_ENVIRONMENT=Production` | Reactiva la cookie `Secure` y HSTS |
| Credenciales por `.env` | Ningún secreto versionado; si falta una variable, el arranque falla |
| El puerto 1433 deja de publicarse | SQL Server queda accesible solo desde la red interna |
| `restart: unless-stopped` | La app se recupera sola ante un reinicio |

**La contraseña del administrador es configurable.** `DbSeeder` la toma de
`Seed:AdminPassword` y, si no está definida, cae a un valor por defecto que vive en el
código fuente. Eso alcanza para desarrollo, pero en un entorno publicado ese valor no
protege nada: cualquiera puede leerlo en este repositorio. Al desplegar hay que definirla:

```bash
Seed__AdminPassword=<una contraseña propia>
```

El usuario común mantiene su contraseña conocida a propósito — es la cuenta que esta misma
página ofrece para probar la demo.

**Sobre el proxy inverso:** las plataformas de hosting terminan el TLS por su cuenta y le
pasan a la aplicación un request HTTP plano. Sin intervención, Kestrel concluye que la
conexión no es segura, la cookie con `Secure` nunca se emite y el login falla sin ningún
error visible. Por eso el pipeline arranca con `UseForwardedHeaders`, que lee
`X-Forwarded-Proto` y `X-Forwarded-For` para reconstruir el esquema y la IP originales.

Las listas `KnownNetworks` y `KnownProxies` se vacían porque la IP del proxy no se conoce
de antemano en un contenedor. Eso implica confiar en esas cabeceras: **la aplicación debe
quedar accesible únicamente a través del proxy**. Si además se expusiera de forma directa,
cualquiera podría falsear el origen del request.

---

## 🌿 Flujo de trabajo

El proyecto se desarrolló con ramas por feature y pull requests: **20 PRs integrados**,
con prefijos `feat/`, `fix/`, `refactor/`, `style/` y `test/`.

Cada cambio se aisló en su rama, se revisó en un PR y recién ahí se integró a `main`.

---

## 📈 Evolución del proyecto

Este catálogo fue implementado tres veces, migrando de escritorio a web moderna:

| Versión | Stack | Repositorio |
| --- | --- | --- |
| **v3** | ASP.NET MVC Core (.NET 10), EF Core, Identity, xUnit | *este repositorio* |
| **v2** | ASP.NET WebForms, ADO.NET, .NET Framework | [catalogo-web-webforms](https://github.com/patricioSostaric/catalogo-web-webforms) |
| **v1** | Windows Forms, .NET Framework | [catalogo](https://github.com/patricioSostaric/catalogo) |

---

## 📄 Licencia

MIT

## 👤 Autor

**Juan Patricio Sostaric**
[GitHub](https://github.com/patricioSostaric) · patriciosostaric923@gmail.com
