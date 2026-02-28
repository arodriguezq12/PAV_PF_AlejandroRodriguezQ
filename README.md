Proyecto FarmaDual - instrucciones rápidas

Requisitos:
- Visual Studio con .NET Framework 4.8.1
- SQL Server (LocalDB o instancia accesible)

Pasos para ejecutar:
1. Restaurar paquetes NuGet (si aplica).
2. Crear la base de datos ejecutando el script SQL proporcionado (archivo SQL entregado junto al proyecto).
3. Actualizar la cadena de conexión en `Web.config` (section `connectionStrings`) para apuntar a su servidor/BD.
4. Abrir la solución en Visual Studio y ejecutar (F5).

Crear el primer administrador:
- Si no existe ningún administrador en la tabla `UsuarioAuth`, abra `https://{host}/Account/RegisterAdmin` y cree la cuenta. El primer admin se puede crear desde esa URL y quedará autenticado automáticamente.
- Después de iniciar sesión como admin, puede usar el panel de administración desde el menú ("Panel admin") para promover otros usuarios a administrador.

Promover usuarios a admin:
- Ingrese a "Panel admin" -> Lista de usuarios -> botón "Promover a admin" para cada usuario.

Notas:
- Las acciones críticas (crear/editar/desactivar Módulos y Géneros) están restringidas al rol `Admin`.
- La creación/edición/desactivación de medicamentos también requiere rol `Admin`.
- Si necesita ayuda adicional para desplegar el proyecto o ajustar la cadena de conexión, indíquelo y le doy pasos más detallados.

Script de BD:
- El script de creación de la base de datos ya está listo (según su comentario). Asegúrese de ejecutarlo antes de iniciar la app.

