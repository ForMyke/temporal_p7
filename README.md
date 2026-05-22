# Servicio Web REST - Gestión de Usuarios

Servicio web REST en Java (JAX-RS / Jersey) desplegado en Apache Tomcat, con base de datos MySQL y un cliente web en HTML/JavaScript vanilla.

---

## Requisitos

### Software necesario

| Herramienta | Versión recomendada |
|-------------|---------------------|
| Java JDK    | 8 o superior        |
| Apache Tomcat | 9.x               |
| MySQL       | 5.7 o superior      |

### Librerías JAR (deben estar en `Servicio/WEB-INF/lib/`)

| JAR | Para qué sirve |
|-----|----------------|
| `jersey-server-*.jar` | Núcleo de JAX-RS (Jersey) |
| `jersey-container-servlet-*.jar` | Integración Jersey con servlets |
| `jersey-common-*.jar` | Utilidades comunes de Jersey |
| `jersey-media-json-jackson-*.jar` | Serialización JSON con Jackson |
| `jackson-databind-*.jar` | Mapeo JSON ↔ objetos Java |
| `jackson-core-*.jar` | Núcleo de Jackson |
| `jackson-annotations-*.jar` | Anotaciones de Jackson |
| `javax.ws.rs-api-*.jar` | API estándar JAX-RS |
| `hk2-*.jar` | Inyección de dependencias de Jersey |
| `mysql-connector-j-*.jar` | Driver JDBC para MySQL |

---

## Configuración de la base de datos

1. Crear la base de datos en MySQL:

```sql
CREATE DATABASE servicio_web;
USE servicio_web;

CREATE TABLE usuarios (
  id_usuario INT AUTO_INCREMENT PRIMARY KEY,
  email VARCHAR(255) NOT NULL,
  password VARCHAR(64) NOT NULL,
  nombre VARCHAR(100) NOT NULL,
  apellido_paterno VARCHAR(100) NOT NULL,
  apellido_materno VARCHAR(100),
  fecha_nacimiento DATETIME,
  telefono BIGINT,
  genero CHAR(1),
  token VARCHAR(20)
);

CREATE TABLE fotos_usuarios (
  id_foto INT AUTO_INCREMENT PRIMARY KEY,
  foto MEDIUMBLOB,
  id_usuario INT,
  FOREIGN KEY (id_usuario) REFERENCES usuarios(id_usuario)
);
```

2. Editar `Servicio/META-INF/context.xml` con tus credenciales:

```xml
<Resource
  ...
  username="tu_usuario"
  password="tu_contraseña"
  url="jdbc:mysql://localhost/servicio_web?serverTimezone=UTC"/>
```

---

## Compilar y desplegar

Todos los comandos se ejecutan desde dentro del directorio `Servicio/`.

### macOS / Linux

```bash
# 1. Editar compila.sh y definir la ruta de Tomcat
export CATALINA_HOME=/ruta/absoluta/a/tomcat

# 2. Compilar
javac -cp "WEB-INF/lib/*:." servicio/Servicio.java

# 3. Copiar clases compiladas
rm WEB-INF/classes/servicio/*
cp servicio/*.class WEB-INF/classes/servicio/.

# 4. Empaquetar y desplegar
jar cvf Servicio.war WEB-INF META-INF
rm -rf $CATALINA_HOME/webapps/Servicio.war $CATALINA_HOME/webapps/Servicio
cp Servicio.war $CATALINA_HOME/webapps/.
```

O simplemente edita y ejecuta `compila.sh`.

### Windows

Edita `compila.bat`, descomenta y ajusta `CATALINA_HOME`, luego ejecútalo.

### Verificar que funciona

Inicia Tomcat y abre en el navegador:

```
http://localhost:8080/Servicio/rest/ws/login?email=test@test.com&password=...
```

El frontend de prueba (`prueba.html`) y `WSClient.js` deben servirse desde la raíz del servidor web.

---

## Arquitectura

```
p7/
├── Servicio/
│   ├── servicio/
│   │   ├── Servicio.java      ← AQUÍ van todos los endpoints
│   │   ├── Usuario.java       ← Bean / modelo de datos
│   │   └── Respuesta.java     ← Wrapper para mensajes de respuesta
│   ├── META-INF/
│   │   └── context.xml        ← Credenciales y URL del datasource MySQL
│   ├── WEB-INF/
│   │   ├── web.xml            ← Configuración del servlet Jersey
│   │   ├── lib/               ← JARs de dependencias
│   │   └── classes/servicio/  ← Clases compiladas (.class)
│   ├── compila.sh
│   └── compila.bat
├── prueba.html                ← Frontend SPA de prueba
├── WSClient.js                ← Cliente HTTP fetch reutilizable
└── usuario_sin_foto.png
```

---

## Endpoints disponibles

Base URL: `http://localhost:8080/Servicio/rest/ws`

| Método | Ruta | Autenticación | Descripción |
|--------|------|---------------|-------------|
| GET | `/login` | No | Inicia sesión; devuelve `id_usuario` y `token` |
| POST | `/alta_usuario` | No | Registra un nuevo usuario |
| GET | `/consulta_usuario` | Sí | Obtiene el perfil del usuario |
| PUT | `/modifica_usuario` | Sí | Modifica el perfil del usuario |
| DELETE | `/borra_usuario` | Sí | Elimina el usuario |

Los endpoints autenticados reciben `id_usuario` y `token` como query params. Las contraseñas viajan como hash SHA-256 calculado en el cliente.

---

## Cómo agregar un nuevo endpoint

Todo el código vive en **`Servicio/servicio/Servicio.java`**. Agrega un método con las anotaciones JAX-RS:

```java
@GET                                      // o @POST, @PUT, @DELETE
@Path("nombre_operacion")                 // URL: /rest/ws/nombre_operacion
@Produces(MediaType.APPLICATION_JSON)
public Response miNuevoEndpoint(
  @QueryParam("id_usuario") int id_usuario,
  @QueryParam("token") String token
) throws Exception
{
  try
  {
    Connection conexion = pool.getConnection();

    // Si requiere autenticación:
    if (!verifica_acceso(conexion, id_usuario, token))
      return Response.status(400).entity(j.writeValueAsString(new Respuesta("Acceso denegado"))).build();

    try
    {
      // Tu lógica aquí
      return Response.ok(j.writeValueAsString(/* resultado */)).build();
    }
    finally
    {
      conexion.close();
    }
  }
  catch (Exception e)
  {
    return Response.status(400).entity(j.writeValueAsString(new Respuesta(e.getMessage()))).build();
  }
}
```

### Patrones a seguir

- **Body JSON** (POST/PUT): agrega `@Consumes(MediaType.APPLICATION_JSON)` y un parámetro del tipo bean. Crea un nuevo bean en `servicio/` si necesitas un modelo diferente al de `Usuario`.
- **Transacciones**: usa `conexion.setAutoCommit(false)` + `commit()` / `rollback()` cuando hay múltiples operaciones que deben ser atómicas.
- **Cierre de recursos**: siempre cierra `ResultSet`, `PreparedStatement` y `Connection` en bloques `finally`.
- **Error**: `Response.status(400).entity(j.writeValueAsString(new Respuesta("mensaje"))).build()`
- **Éxito**: `Response.ok(j.writeValueAsString(objeto)).build()`

Después de modificar, recompila y despliega con `compila.sh` / `compila.bat`.
