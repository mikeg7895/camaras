# Configuración de la Base de Datos

## Configuración de la Conexión a MySQL

### 1. Archivos de Configuración

La aplicación utiliza los siguientes archivos de configuración:

- **`appsettings.json`**: Configuración de producción
- **`appsettings.Development.json`**: Configuración de desarrollo

### 2. Cadena de Conexión

Modifica la cadena de conexión en `appsettings.json` o `appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=ServidorCamDB;User=root;Password=yourpassword;"
  }
}
```

**Parámetros:**
- **Server**: Dirección del servidor MySQL (ej: `localhost`, `127.0.0.1`)
- **Port**: Puerto de MySQL (por defecto: `3306`)
- **Database**: Nombre de la base de datos
- **User**: Usuario de MySQL
- **Password**: Contraseña del usuario

### 3. Crear la Base de Datos

Puedes crear la base de datos manualmente o usar migraciones de Entity Framework Core.

#### Opción A: Manualmente con MySQL

```sql
CREATE DATABASE ServidorCamDB CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
```

#### Opción B: Usando Migraciones de EF Core

Desde el directorio `Server.Host`, ejecuta:

```bash
# Crear la migración inicial
dotnet ef migrations add InitialCreate --project ../Server.Infrastructure --startup-project .

# Aplicar la migración (crear las tablas)
dotnet ef database update --project ../Server.Infrastructure --startup-project .
```

### 4. Ejecutar la Aplicación

```bash
cd Server.Host
dotnet run
```

### 5. Variables de Entorno

Alternativamente, puedes usar variables de entorno para la cadena de conexión:

```bash
export ConnectionStrings__DefaultConnection="Server=localhost;Port=3306;Database=ServidorCamDB;User=root;Password=yourpassword;"
dotnet run
```

### 6. Estructura de la Base de Datos

La aplicación creará las siguientes tablas:

- **Users**: Usuarios del sistema
- **Cameras**: Cámaras registradas
- **Videos**: Videos grabados por las cámaras
- **ProcessedFrames**: Frames procesados de los videos

### Notas Importantes

- Asegúrate de que MySQL esté instalado y corriendo
- Verifica que el usuario tenga permisos para crear y modificar bases de datos
- No subas archivos `appsettings.json` con credenciales reales a repositorios públicos
- Considera usar `dotnet user-secrets` para desarrollo local
