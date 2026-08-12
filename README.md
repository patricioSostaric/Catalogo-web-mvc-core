# Store Sostaric — ASP.NET MVC Core (.NET 10)

![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet&logoColor=white)
![C#](https://img.shields.io/badge/C%23-14-239120?logo=csharp&logoColor=white)
![EF Core](https://img.shields.io/badge/EF%20Core-10.0-512BD4)
![SQL Server](https://img.shields.io/badge/SQL%20Server-CC2927?logo=microsoftsqlserver&logoColor=white)
![React](https://img.shields.io/badge/React-19-61DAFB?logo=react&logoColor=black)
![Docker](https://img.shields.io/badge/Docker-ready-2496ED?logo=docker&logoColor=white)
![Tests](https://img.shields.io/badge/tests-541%20passing-success)
![License](https://img.shields.io/badge/license-MIT-blue)

Tienda de artículos electrónicos con catálogo público, carrito, pedidos y panel de
administración, construida en ASP.NET MVC Core sobre .NET 10.

> **Este proyecto es la migración de una aplicación previa en ASP.NET WebForms.**
> El repositorio original sigue publicado como punto de partida:
> [catalogo-web-webforms](https://github.com/patricioSostaric/catalogo-web-webforms)

> 🔄 **El catálogo público se está migrando a Web API + React** aplicando el patrón
> *strangler fig*: lo nuevo crece al lado de lo viejo, sin apagar nada.
> Ver [Migración en curso](#-migración-en-curso--web-api--react).

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

**¿Querés ver el panel de administración?** El ABM de artículos, marcas y categorías y el
control de stock quedan detrás del rol `Admin`. Escribime y te doy acceso — así la demo se
mantiene en pie para el resto de las visitas:

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
- Carrito de compras con cantidades por artículo y confirmación de pedido simulada
- Historial de pedidos, con los precios vigentes al momento de cada compra
- Cancelación del pedido propio mientras no haya sido enviado: el stock vuelve al catálogo

### Administrador
- ABM completo de artículos, marcas y categorías
- Gestión de pedidos: despachar y marcar como entregados, con filtro por estado
- Baja lógica de artículos mediante la propiedad `Activo` (no se borran filas)
- Control de stock — los artículos sin stock no se publican

### Superadministrador
- Todo lo del administrador, más el registro de auditoría de operaciones
- Listado de usuarios con sus roles y estado de bloqueo
- Desbloqueo de cuentas bloqueadas por intentos fallidos

La auditoría queda separada a propósito: guarda correos de quienes usan la aplicación.
Son datos de terceros, y el rol `Admin` está pensado para poder compartirse con quien
quiera recorrer el panel de administración.

**Las direcciones IP se anonimizan antes de guardarse.** Se conserva la red de origen y
se descarta el último octeto: `203.0.113.45` se almacena como `203.0.113.0`. Eso alcanza
para distinguir veinte intentos fallidos de un mismo origen de veinte personas distintas,
que es para lo que sirve el dato en un registro de seguridad, sin almacenar información
que identifique a una persona. Es el principio de minimización de datos del GDPR. La IP
completa no llega ni a la base ni a los registros de la aplicación.

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
Catalogo.Datos/           Biblioteca compartida: no depende de ninguna aplicación
├── Interfaces/           Contratos por módulo: Articulos, Marcas, Categorias, Carrito, Pedidos, Favoritos, Usuarios, Avatar, Audit, Email
├── Services/             Lógica de negocio + Email, Audit, Identity
├── Repository/           Acceso a datos por entidad
├── Data/                 CatalogoContext y DbSeeder
├── Models/
│   ├── ViewModels/       Contratos con las vistas
│   └── Settings/         Configuración tipada (SmtpSettings)
└── Migrations/           14 migraciones versionadas de EF Core

catalogo-web-mvc/         Aplicación web (Razor + Identity)
├── Controllers/          Home, Articulo, Marcas, Categorias, Favoritos, Carrito, Pedidos, GestionPedidos, AuditLog, Usuarios, Account
├── Views/                Razor, con partials reutilizables
├── wwwroot/app/          El front en React compilado
└── Program.cs            Registro de DI y pipeline de middleware

Catalogo.Endpoints/       Los controladores JSON, en una biblioteca: dos apps los alojan
├── Controllers/          ArticulosApi, FavoritosApi
└── Dtos/                 Contratos con los clientes de la API

Catalogo.Api/             Host de la API: no emite sesiones, solo lee la cookie del MVC

Catalogo.Gateway/         Proxy inverso con YARP
catalogo-front/           Front en React (Vite)
CatalogoWeb.tests/        541 tests unitarios
```

Las capas de negocio y datos viven en una biblioteca aparte para que más de una
aplicación pueda usarlas: el MVC y la API. Una biblioteca no puede referenciar a una
aplicación, así que la dependencia solo va en una dirección.

Los DTO viven junto a los controladores y no en `Catalogo.Datos`: son el contrato público
de la API, no un tipo del dominio. Los ViewModels, en cambio, quedaron en la biblioteca
compartida porque los usan tanto las vistas como los servicios que las alimentan.

`Catalogo.Endpoints` existe porque los controladores necesitan **dos anfitriones**. Al
principio vivían dentro de `Catalogo.Api`, pero referenciar un ejecutable desde el MVC
arrastraba su `Program` y su `appsettings.json`, que chocaba con el del MVC al publicar
(`NETSDK1152`). Uno habría pisado al otro sin que estuviera definido cuál — cadena de
conexión incluida. Separar los controladores a una biblioteca lo resuelve de raíz y deja a
`Catalogo.Api` como lo que es: un host.

Los comandos de EF Core necesitan indicar los dos proyectos, porque el contexto ya no
vive donde está la configuración:

```bash
dotnet ef migrations add NombreDeLaMigracion --project Catalogo.Datos --startup-project catalogo-web-mvc
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
- Roles `SuperAdmin`, `Admin` y `Usuario`, con autorización por atributos

**Testing**
- xUnit 2.9 · Moq 4.20 · `EntityFrameworkCore.InMemory` · coverlet

**Front desacoplado** (migración en curso)
- React 19 · Vite · JavaScript
- Proxy de desarrollo hacia el MVC, en el rol que después cumplirá YARP

**Otros**
- X.PagedList 10.5 para el paginado
- Bootstrap · HTML5 · CSS3 · JavaScript

---

## 🧪 Testing

**541 tests unitarios, la totalidad en verde.**

```bash
dotnet test
# Correctas! - Con error: 0, Superado: 541, Omitido: 0, Total: 541
```

Cobertura por capa:

| Área | Archivos de test |
| --- | --- |
| Controllers | Account, Articulo, ArticulosApi, AuditLog, Categorias, Favoritos, FavoritosApi, GestionPedidos, Home, Marcas, Pedidos, Usuarios, AuthorizationAttribute (incluye qué rol protege cada controlador) |
| Services | Articulo, Carrito, Categoria, Marca, Pedido, UsuarioAdmin, EmailTemplates, SmtpEmailSender, SpanishIdentityErrorDescriber |
| Repository | Articulo, Categoria, Marca |
| Data | DbSeeder |
| Models | Articulo, EstadoPedido, UrlImagenSeguraAttribute |

Los repositorios se prueban contra `EntityFrameworkCore.InMemory`; los servicios y
controladores, con dobles construidos con **Moq**. El envío de mails es testeable porque
`SmtpClient` está desacoplado tras una interfaz propia (`ISmtpClient`).

---

## 🔒 Seguridad

Se aplicó una auditoría sobre **OWASP Top 10** y se corrigieron los hallazgos:

| Medida | Qué mitiga |
| --- | --- |
| Auditoría restringida al rol `SuperAdmin` | Exposición de correos de terceros a una cuenta compartida |
| Anonimización de IP antes de persistir | Almacenamiento de datos personales sin necesidad |
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

**El superadministrador también.** Es la única cuenta que accede a la auditoría, así que
conviene un correo real: es la vía de recuperación si se pierde el acceso.

```bash
Seed__SuperAdminEmail=<un correo propio>
Seed__SuperAdminPassword=<una contraseña propia>
```

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

Las tres primeras fueron reescrituras: cada versión empezó de cero mirando a la anterior.
La cuarta no.

---

## 🔄 Migración en curso — Web API + React

El catálogo público se está migrando a una arquitectura de API con front desacoplado,
aplicando el patrón **strangler fig**: lo nuevo crece al lado de lo viejo y se le van
mudando responsabilidades de a una, sin apagar nada ni frenar el desarrollo. Lo opuesto
a la reescritura completa, que obliga a mantener dos sistemas en paralelo hasta un
encendido único.

### Estado

| Etapa | Estado |
| --- | --- |
| 1 · Endpoint `/api/articulos` dentro del proyecto MVC | ✅ hecho |
| 2 · Front en React (Vite) consumiendo la API | ✅ hecho |
| 3 · Extraer la API a su propio proyecto, con **YARP** por delante | ✅ hecho (en local) |
| 4 · Sesión compartida y primer módulo autenticado migrado (favoritos) | ✅ hecho (en local) |

Las etapas 3 y 4 corren con los proyectos por separado en la máquina de desarrollo. En
Azure todo va en **un solo contenedor**: el plan gratuito admite una única instancia y
tres aplicaciones necesitarían tres. La decisión fue priorizar entender el patrón por sobre
pagar por mostrarlo.

Eso es posible porque los controladores JSON viven en `Catalogo.Endpoints`, una biblioteca
que **dos aplicaciones distintas pueden alojar**: `Catalogo.Api` cuando corre como proceso
propio detrás del gateway, y el MVC —vía `AddApplicationPart`— cuando todo va junto. Los
controladores no cambian una línea entre un caso y el otro, que es justamente la prueba de
que la API no depende del MVC.

Es una concesión al presupuesto, no al diseño: la separación real es la del `compose`, y
volver a ella es quitar una línea del `Program.cs` y una referencia del `.csproj`. La
contra a tener presente es que producción y desarrollo dejan de compartir topología, y esa
diferencia es de las que esconden errores.

Uno apareció al probarlo. Cuando el MVC aloja los endpoints, deja de correr el `Program`
de `Catalogo.Api`, que era donde vivía la regla de responder `401` en lugar de redirigir al
login. Sin ella, un `fetch` sin sesión recibía el HTML del formulario de login con estado
`200` e intentaba leerlo como JSON. Se resolvió en `ConfigureApplicationCookie`: la misma
falta de sesión devuelve `401` bajo `/api` y sigue redirigiendo al login en las vistas
Razor, según quién pregunte.

```
Navegador → Catalogo.Gateway (YARP)
                 ├── /api/*        → Catalogo.Api
                 └── {**catch-all} → catalogo-web-mvc (Razor + React en /app)
```

**El proxy se puso antes de mover nada.** Primero reenviaba todo al MVC, sin que cambiara
nada visible; recién después se creó `Catalogo.Api` y se cambió el destino de `/api`. Ese
orden es lo que vuelve reversible cada mudanza: si la API nueva falla, se borra su ruta y
todo vuelve al MVC sin recompilar ni desplegar. Extrayendo la API primero habría que tocar
el front, el proxy y el back en un mismo movimiento.

Mover una funcionalidad de una aplicación a otra es cambiar el `ClusterId` de su ruta.

Ese paso destapó dos acoplamientos que nadie veía mientras todo vivía en un proyecto:
`IArticuloService` devolvía `SelectList` —un tipo de la capa de vistas— y `ArticuloService`
consultaba el `DbContext` salteándose su propio repositorio. Los dos se arreglaron antes de
extraer nada.

Las vistas Razor siguen atendiendo el catálogo y el resto de la aplicación. El front en
React es una segunda forma de acceder a los mismos datos, no un reemplazo — todavía. La
excepción es favoritos, que ya se puede ver y quitar desde React; marcar sigue siendo cosa
de las vistas Razor. Que un módulo esté a medio camino sin que nada se rompa es
exactamente lo que el patrón permite.

### Por qué el endpoint arrancó dentro del MVC

Crear el proyecto separado desde el principio obligaba a resolver la referencia a las
entidades, la cadena de conexión duplicada, CORS, dos imágenes de Docker y dos servicios
en Azure **antes de escribir la primera línea de API**.

Adentro del MVC, el endpoint se apoya en el `IArticuloService` que ya existía: no duplica
lógica de negocio ni de filtrado, y el filtrado por texto es el mismo que usa el buscador
de la vista Razor. La mudanza a un proyecto propio se hace después, con el contrato ya
consumido y sabiendo exactamente qué hay que mover.

### El contrato

`GET /api/articulos?buscar=galaxy&page=1&pageSize=6`

```json
{
  "pagina": 1,
  "tamanioPagina": 6,
  "totalArticulos": 15,
  "totalPaginas": 3,
  "articulos": [
    {
      "id": 1,
      "nombre": "Galaxy S10",
      "marca": "Samsung",
      "categoria": "Celulares",
      "precio": 699999,
      "imagenUrl": "/imagen/articulos/s01.jpg",
      "disponible": true
    }
  ]
}
```

Devuelve **DTOs y no entidades**: el contrato público queda desacoplado del modelo de
datos. Devolver la entidad habría expuesto los identificadores internos, el código, el
estado de alta y el stock exacto, y cualquier propiedad agregada más adelante aparecería
sola en la respuesta pública. El stock se reduce a un booleano — el catálogo solo necesita
saber si hay o no hay; la cantidad exacta es información del negocio.

El paginado va en un envoltorio y no en un array pelado: quien consume necesita saber en
qué parte del total está para poder dibujar su paginador. `pageSize` se acota con
`Math.Clamp` a un máximo de 50, para que una sola petición no pueda pedir el catálogo
entero.

`GET /api/articulos/{id}` devuelve el detalle de un artículo, con su descripción y sin el
código —dato de administración que sirve para reponer stock y no le aporta nada a quien
consume el catálogo.

Responde **404 tanto si el artículo no existe como si está dado de baja**, sin
distinguirlos. Diferenciar los dos casos revelaría que ese artículo existe en el catálogo
interno aunque no se publique.

### Un solo login para dos aplicaciones

El catálogo es público, pero para mover cualquier módulo con datos propios de cada usuario
—carrito, pedidos, favoritos— la API primero necesita saber quién está del otro lado.

Las dos aplicaciones comparten el juego de claves de Data Protection y el nombre de
aplicación. Eso es lo que le permite a la API descifrar la cookie que emite el MVC: la
cookie no dice quién es el usuario, va cifrada. Sin esa coincidencia, el usuario tendría
que iniciar sesión dos veces para usar una sola aplicación.

La API **solo lee** la cookie. No emite sesiones ni tiene pantalla de login: de eso sigue
ocupándose el MVC, que es el único que conoce las credenciales. Ante una petición sin
sesión responde `401` en lugar de redirigir al login, porque para un cliente que espera
JSON una redirección es peor que un error claro.

Las claves quedan fuera del repositorio y en un volumen compartido: con ellas se podrían
falsificar sesiones de cualquier usuario.

### Favoritos, el primer módulo autenticado

| Método | Ruta | Respuesta |
| --- | --- | --- |
| `GET` | `/api/favoritos` | los del usuario de la cookie |
| `DELETE` | `/api/favoritos/{articuloId}` | `204`, exista o no el favorito |

El `DELETE` es idempotente a propósito: quitar algo que ya no está es el estado que el
cliente pidió, no un error. Tocar dos veces el botón no tiene por qué fallar.

**El id del usuario no viaja en la ruta**, sale de la cookie. Si viajara, cualquiera con
una sesión válida podría leerle o vaciarle los favoritos a otro cambiando un número.

No lleva antiforgery, a diferencia de los formularios del MVC: la cookie se emite con
`SameSite=Strict` y un `DELETE` no se puede disparar desde un formulario ajeno. Queda
anotado en el código que esto hay que revisarlo si el front pasa a vivir en otro dominio.

Al ir a exponerlo apareció que favoritos era el único módulo sin servicio ni repositorio:
el controlador consultaba el `DbContext` directo y `HomeController` repetía dos de esas
consultas por su cuenta. Se extrajo `IFavoritoService` antes de tocar la API, para no
dejar la misma consulta escrita en dos aplicaciones. El servicio devuelve entidades y no
ViewModels ni DTOs: de él dependen las dos, y cada una arma la forma que necesita.

### El proxy

En desarrollo, el servidor de Vite reenvía `/api` y `/imagen` al MVC, de modo que el
navegador ve un solo origen y CORS no interviene. Es el mismo papel que va a cumplir YARP
en la etapa 3, con la diferencia de que este solo existe en desarrollo: al desplegar hay
que resolverlo de nuevo.

### El front

```
catalogo-front/
├── vite.config.js        Proxy hacia el MVC
└── src/
    ├── App.jsx           Layout y tabla de rutas
    ├── index.css
    ├── paginas/
    │   ├── Catalogo.jsx  Listado, paginado y búsqueda
    │   ├── Detalle.jsx   Ficha de un artículo
    │   ├── Favoritos.jsx Pantalla autenticada: lista y quita
    │   └── Privacidad.jsx
    └── components/
        ├── Layout.jsx    Encabezado + {children} + pie
        ├── Encabezado.jsx
        ├── PiePagina.jsx
        ├── TarjetaArticulo.jsx
        └── TarjetaFavorito.jsx
```

Catálogo con tarjetas, paginado y búsqueda por texto con *debounce* de 300 ms: cada tecla
cancela el pedido anterior, así escribir una palabra genera una sola consulta en lugar de
una por letra. La pantalla distingue los estados de carga, error y sin resultados: una
consulta que falló también devuelve una lista vacía, y decir «no se encontraron artículos»
sería engañoso.

El ruteo es del lado del cliente: al navegar no hay viaje al servidor, se reemplaza solo
el contenido y el encabezado y el pie no se redibujan. `Layout` recibe cada página por
`children`, que cumple el mismo papel que `@RenderBody()` en `_Layout.cshtml`.

Se comparte el estilo con las vistas Razor a propósito. Durante la migración las dos
mitades conviven, y quien entra no debería notar de cuál viene cada pantalla.

Vive en el mismo repositorio que el MVC porque el front y la API cambian de a pares: si se
toca el contrato, quien lo consume tiene que enterarse en el mismo commit.

```bash
cd catalogo-front
npm install
npm run dev
```

Requiere el stack levantado con `docker compose up`: el proxy de Vite apunta al gateway
en `http://localhost:8080`, no al MVC. Desde que la API se mudó a su propio proyecto, el
MVC dejó de atender `/api`, y el gateway es el único que sabe qué ruta va a cuál
aplicación.

---

## 📄 Licencia

MIT

## 👤 Autor

**Juan Patricio Sostaric**
[GitHub](https://github.com/patricioSostaric) · patriciosostaric923@gmail.com
