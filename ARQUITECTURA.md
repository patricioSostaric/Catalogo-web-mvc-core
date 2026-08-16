# Arquitectura — Catálogo Web MVC

> Documento de referencia para entender y **explicar** el proyecto.
> Organizado en niveles: se lee de arriba hacia abajo, del pitch al detalle.

---

## Nivel 0 — El pitch (30 segundos, sin jerga)

> Es un catálogo de artículos con dos caras: una pública, donde cualquiera navega
> productos, busca y ve el detalle; y una privada de administración, donde un admin
> hace el ABM de artículos, marcas y categorías. Tiene usuarios con roles, favoritos
> por usuario, y un log de auditoría de las operaciones. Está hecho en ASP.NET Core
> MVC con Entity Framework Core y SQL Server.

---

## Nivel 1 — El flujo de una request

Esta es **la** cosa que hay que poder dibujar en un pizarrón. Todo lo demás cuelga de acá:

```
Navegador
   │
   ▼
[ Middleware pipeline ]  ← Program.cs
   │  Localization (es-AR) → HTTPS → Security headers →
   │  Routing → RateLimiter → Authentication → Authorization
   ▼
[ Controller ]           ← recibe, valida, decide. NO tiene lógica de negocio.
   │                        ArticuloController, HomeController, AccountController...
   ▼
[ Service ]              ← la lógica: filtros, reglas, armado de ViewModels
   │                        ArticuloService, CategoriaService, MarcaService
   ▼
[ Repository ]           ← el acceso a datos. Lo único que conoce EF Core.
   │                        ArticuloRepository...
   ▼
[ CatalogoContext ]      ← DbContext de EF Core
   │
   ▼
SQL Server
```

En sentido inverso vuelve un **ViewModel** (no la entidad) hacia una **View** Razor.

**La frase que resume la arquitectura:** cada capa habla solo con la de abajo, y
siempre a través de una interfaz. Por eso existe la carpeta `Interfaces/` separada,
organizada por módulo (`Articulos/`, `Categorias/`, `Marcas/`, `Audit/`, `Email/`).

---

## Nivel 2 — Las cajas, una por una

### Program.cs — el centro de todo

Es el archivo más importante para explicar. Acá se arma la aplicación entera.

**Registro de dependencias (DI).** Cada interfaz se ata a su implementación:

```csharp
builder.Services.AddScoped<IArticuloRepository, ArticuloRepository>();
builder.Services.AddScoped<IArticuloService, ArticuloService>();
```

`AddScoped` = una instancia por request HTTP. Es lo correcto acá porque el `DbContext`
también es scoped, y todos comparten la misma unidad de trabajo dentro de una request.

**Los registros están agrupados en extensiones sobre `IServiceCollection`**, una por
área, en la carpeta `Extensions/`:

```csharp
builder.Services.AddClavesCompartidas(builder.Configuration, builder.Environment);
builder.Services.AddPersistencia(builder.Configuration);
builder.Services.AddIdentidad(builder.Environment);
builder.Services.AddProteccionDeAbuso();
builder.Services.AddCabecerasDeProxy();
builder.Services.AddCorreo(builder.Configuration);
builder.Services.AddServiciosDelCatalogo();
```

El `Program` dice **qué** se configura; cada extensión, **cómo**. Antes eran más de
trescientas líneas donde convivían Identity, el rate limiter, las cabeceras del proxy y
el registro de nueve módulos sin separación visible entre una cosa y la otra.

Esto no cambia la arquitectura —las capas y los servicios son los mismos—, cambia la
legibilidad del arranque. Con una excepción que sí es de diseño: `AddClavesCompartidas`
vive en `Catalogo.Datos` y la llaman **las dos** aplicaciones, porque su configuración
tiene que coincidir exactamente entre el MVC y la API. Estaba copiada en los dos
`Program.cs`, y dos copias de algo que debe coincidir son dos oportunidades de que dejen
de hacerlo.

**El pipeline de middleware.** El orden importa y no es decorativo:
`UseAuthentication` va **antes** de `UseAuthorization` porque primero hay que saber
*quién sos* y después *qué podés hacer*. Y `UseRouting` va antes que ambos porque
hasta que no se resuelve la ruta no se sabe qué atributos `[Authorize]` aplican.

### Data — Modelos y persistencia

| Entidad | Rol |
|---|---|
| `Articulo` | El core. FK a Marca y Categoría, más `Activo` y `Stock` |
| `Marca` / `Categoria` | Catálogos de apoyo |
| `ApplicationUser` | Extiende `IdentityUser` con campos propios |
| `ArticuloFavorito` | Tabla puente usuario ↔ artículo |
| `AuditLog` | Registro de operaciones |

Las **14 migraciones** son la historia versionada de la base:
`InitialCreate` → `UpdateArticuloColumns` → seeds → `AddIdentity` → `AddFavoritos`
→ `AddAuditLog` → `AddArticuloActivoStock` → `SeedMarcasCategoriasArticulos`
→ `AddPerfilUsuario` → `ImagenesLocalesArticulos` → `CarritoYPedidos` → `EstadoDePedido`.

### Repository — solo datos, cero reglas

`ArticuloRepository.GetAll()`:

```csharp
return _context.Set<Articulo>()
    .Include(a => a.Marca)
    .Include(a => a.Categoria)
    .AsNoTracking();
```

Tres decisiones, las tres defendibles:

- **Devuelve `IQueryable`, no `List`** — la consulta todavía no se ejecutó. Permite que
  el Service le siga encadenando filtros y que SQL Server resuelva todo en una sola
  query, en vez de traer 10.000 filas a memoria y filtrar en C#.
- **`Include`** — eager loading, para evitar el problema N+1 al mostrar la marca de
  cada artículo en la grilla.
- **`AsNoTracking()`** — como es solo lectura, EF no necesita rastrear cambios.
  Menos memoria, más rápido.

### Service — acá vive el negocio

`ArticuloService.BuscarAsync()` es el método más rico. Toma el `IQueryable` del repo
y le va apilando condiciones: filtro por activos, búsqueda simple, o filtro avanzado
(campo + criterio + valor, con `switch` anidados). Recién en la última línea,
`ToPagedList()` **materializa** la consulta y trae solo la página pedida.

`ObtenerDetallePublicoAsync()` es el ejemplo perfecto de por qué existe esta capa:

```csharp
if (articulo == null || !articulo.Activo) return null;
```

Regla de negocio — un artículo inactivo no existe para el público — y además mapea a
`ArticuloDetalleViewModel`, exponiendo solo los campos públicos. La entidad completa
(con stock, IDs internos) nunca llega a la vista.

### ViewModels — el contrato con la vista

Nunca se manda una entidad de EF a una View. Cada pantalla tiene su ViewModel con
exactamente lo que necesita: `ArticuloDetalleViewModel`, `LoginViewModel`,
`RegisterViewModel`, `ResetPasswordViewModel`, `FavoritoViewModel`,
`TablaEntidadViewModel`, `DescripcionCrudViewModels`.

Dos razones, y la segunda es de seguridad: **evita over-posting**. Si el modelo de
binding fuera la entidad, alguien podría mandar en el POST un campo que no está en el
formulario y modificarlo.

### Views — Razor con parciales

Refactor fuerte acá: `_Paginado`, la card de artículo, el partial compartido
Login/Register, los formularios de Marca/Categoría unificados.

**El principio:** si el mismo HTML aparece dos veces, es un partial.

---

## Nivel 3 — Los módulos transversales

### Identity + Roles

`AddDefaultIdentity<ApplicationUser>()` con política de contraseña (8 caracteres,
mayúscula, minúscula, dígito, símbolo) y lockout a los 5 intentos por 5 minutos.
`DbSeeder` crea los roles y los usuarios iniciales al arrancar.
`SpanishIdentityErrorDescriber` traduce los mensajes de error de Identity.

### Auditoría

`IAuditService` → `AuditService`. Se inyecta donde hace falta y usa
`IHttpContextAccessor` para saber qué usuario ejecutó la acción.

### SMTP

Tres piezas:

- **`SmtpSettings`** — configuración tipada, cargada con `Configure<SmtpSettings>`
  desde `appsettings` / User Secrets.
- **`ISmtpClient` → `SmtpClientAdapter`** — el detalle clave. El cliente SMTP de .NET
  queda envuelto en una interfaz propia, y eso permite mockearlo en los tests. Sin ese
  adapter no se podría testear el envío sin mandar mails de verdad.
- **`IEmailSender` → `SmtpEmailSender`** + `EmailTemplates` — el armado de los mails
  de bienvenida y de restablecimiento de contraseña.

---

## Nivel 4 — Seguridad (auditoría OWASP Top 10)

Se hizo una auditoría OWASP Top 10 y se corrigieron los hallazgos
(commit `9308884`):

- **Cookies**: `HttpOnly` (JS no las lee → mitiga XSS), `Secure` (solo HTTPS),
  `SameSite=Strict` (mitiga CSRF).
- **Rate limiting**: 10 requests por minuto en el endpoint de auth → frena fuerza bruta.
- **Security headers**: CSP restrictiva, `X-Frame-Options: DENY` (anti-clickjacking),
  `X-Content-Type-Options: nosniff`, `Referrer-Policy`.
- **User Secrets** en desarrollo → las credenciales SMTP y la connection string nunca
  van al repo.
- **HSTS + redirección HTTPS** en producción.

---

## Las 8 decisiones defendibles

Cada una tiene un *por qué*. Estas son las que un entrevistador va a picotear.

1. **Repository separado de Service** — el Service no sabe que existe EF Core.
   Si mañana se cambia a Dapper, se toca una sola capa.
2. **`IQueryable` en vez de `List`** — filtrado y paginado en la base, no en memoria.
3. **`AsNoTracking()` en lecturas** — no se paga el costo del change tracker cuando no
   se va a escribir.
4. **Baja lógica con `Activo`** — no se borran filas. Se preserva integridad
   referencial y el historial, y se puede revertir.
5. **ViewModels en vez de entidades** — contrato explícito con la vista, y protección
   contra over-posting.
6. **`ISmtpClient` propio** — testeabilidad. Es la razón entera de que exista el adapter.
7. **Cultura `es-AR` global** — resolvió un bug real de binding: los precios con coma
   decimal no parseaban (commit `bbd6b3f`).
8. **Partials para todo lo repetido** — un cambio de diseño se hace en un solo archivo.

---

## Cómo estudiar este documento

No releerlo. **Explicarlo en voz alta**, con el documento tapado, en este orden:

1. Pitch
2. Flujo de una request
3. Una capa a elección
4. Una decisión defendida

Donde uno se traba, ahí está el hueco real.
