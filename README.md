# Catálogo Web — ASP.NET MVC Core (.NET 10)

![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet&logoColor=white)
![C#](https://img.shields.io/badge/C%23-13-239120?logo=csharp&logoColor=white)
![EF Core](https://img.shields.io/badge/EF%20Core-10.0-512BD4)
![SQL Server](https://img.shields.io/badge/SQL%20Server-CC2927?logo=microsoftsqlserver&logoColor=white)
![Tests](https://img.shields.io/badge/tests-404%20passing-success)
![License](https://img.shields.io/badge/license-MIT-blue)

Aplicación web de catálogo de artículos con área pública y panel de administración,
construida en ASP.NET MVC Core sobre .NET 10.

> **Este proyecto es la migración de una aplicación previa en ASP.NET WebForms.**
> El repositorio original sigue publicado como punto de partida:
> [catalogo-web-webforms](https://github.com/patricioSostaric/catalogo-web-webforms)

---

## 🔗 Demo

<!-- TODO: reemplazar por la URL real cuando esté desplegada -->
**Demo en producción:** _(pendiente de despliegue)_

**Usuarios de prueba**

| Rol | Email | Contraseña |
| --- | --- | --- |
| Administrador | `admin@catalogo.com` | `Admin@1234` |
| Usuario | `usuario@catalogo.com` | `Usuario@1234` |

> Se crean automáticamente al primer arranque mediante `DbSeeder`.
> Si un administrador cambia la contraseña, el seeder **no** la repone — reponerla
> reabriría un acceso hardcodeado.

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

CatalogoWeb.tests/        404 tests unitarios
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

**404 tests unitarios, la totalidad en verde.**

```bash
dotnet test
# Correctas! - Con error: 0, Superado: 404, Omitido: 0, Total: 404
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
