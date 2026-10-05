1. Alcance y Supuestos
Dentro de alcance (Prioridad actual)
* Módulo de Autenticación Local: Acceso controlado al sistema mediante usuario y contraseña validados contra la base de datos local.
* Módulo de Gestión de Propietarios: Registro, actualización, búsqueda y visualización de la información de contacto de los clientes.
* Módulo de Gestión de Pacientes (Mascotas): Registro de mascotas con vinculación a propietarios (existentes o creados en el flujo inmediato), cálculo dinámico de edad y registro estandarizado de peso en kilogramos.
* Módulo de Historia Clínica y Procedimientos Médicos: Registro cronológico de atenciones médicas (anamnesis, examen físico, diagnóstico, tratamiento) permitiendo la selección explícita del médico veterinario tratante (Dr. Fabio o Dr. William). Consulta consolidada del historial del animal para eliminar la dependencia de llamadas telefónicas entre turnos.
* Módulo de Carnet de Vacunación Digital: Registro de planes de vacunación y desparasitación con fechas de aplicación y refuerzo. Generación y descarga local del carnet en formato PDF para archivo o entrega digital.
* Módulo de Notificaciones y Recordatorios Gratuitos: Generación automatizada de enlaces directos a WhatsApp Web / API pública (wa.me) con plantillas de texto preconfiguradas para avisos de vacunación y controles clínicos sin costos de mensajería empresarial. Propuesta funcional complementaria de plantilla para enlace local de correo (mailto:).
Fuera de alcance / Baja prioridad
* Control y Administración de Inventarios: Monitoreo de stock, fechas de vencimiento de medicamentos, alimentos y accesorios en estantería (postergado explícitamente por el cliente para etapas futuras).
* Facturación Electrónica y Contabilidad: Módulo de caja diaria, generación de facturas tributarias y asientos contables.
* Gestión de Peluquería y Liquidación: Control de turnos, registro de servicios estéticos y liquidación porcentual de honorarios del peluquero.
* Pasarelas de Pago e Integraciones API de Pago: Uso de Meta Business Platform, SMS masivos pagos o pasarelas transaccionales.
* Sincronización Multi-dispositivo / Nube: El aplicativo no contemplará esquemas de sincronización remota ni arquitectura distribuida en esta entrega.
Supuestos funcionales
* S-01 (Identificación del Propietario): Para prevenir duplicidades de clientes y respaldar la custodia legal de la historia clínica, se incluye un número de documento de identidad como identificador funcional del propietario.
* S-02 (Cálculo de Edad con Fecha Estimada): Dado que muchos clientes rescatan animales o desconocen su fecha exacta de nacimiento, el sistema aceptará una "fecha de nacimiento aproximada" para que el cálculo automático de la edad siempre se mantenga operativo.
* S-03 (Catálogo de Veterinarios Predefinido): Ante la restricción de operar con un único usuario de sesión activa en la máquina, el sistema mantendrá una lista fija o configurable de médicos veterinarios tratantes ("Dr. Fabio", "Dr. William") disponible en una lista desplegable en cada evento clínico para asegurar la trazabilidad profesional requerida por la Ley 576 de 2000.
* S-04 (Alternativa Gratuita de Notificación por Correo - URI mailto:): Como canal gratuito alternativo y complementario a WhatsApp, el sistema permitirá disparar enlaces bajo el protocolo estándar mailto:. Esto abrirá el cliente de correo predeterminado del sistema operativo con el destinatario, asunto y cuerpo del mensaje prellenados, sin costo por transacción de envío.
* S-05 (Inmutabilidad del Historial Clínico): Conforme a las normas éticas veterinarias, los registros clínicos guardados no podrán ser eliminados ni sobreescritos arbitrariamente. Cualquier ajuste posterior se considerará una nota de evolución o adenda complementaria.
* S-06 (Conectividad Opcional): El software funcionará de manera autónoma sin internet para todas las operaciones clínicas y de guardado local. La conectividad a internet solo será requerida al invocar el navegador web para cargar el enlace de WhatsApp.
2. Reglas de Negocio (RN)
* RN-01 - Autenticación Obligatoria y Dinámica: Todo acceso a las funciones del software requiere autenticación previa mediante credenciales (nombre de usuario y contraseña). La verificación debe realizarse obligatoriamente contra los registros almacenados en la base de datos local, prohibiéndose accesos anónimos o credenciales estáticas codificadas en la aplicación.
* RN-02 - Trazabilidad Obligatoria del Veterinario Tratante: En cada registro de procedimiento clínico, consulta o inmunización, es obligatorio seleccionar el nombre del médico veterinario responsable de la atención. El sistema debe impedir el guardado de cualquier evento clínico si este campo no ha sido asignado.
* RN-03 - Cardinalidad Propietario-Paciente: Un propietario puede tener asociadas una o muchas mascotas ($1:N$). Toda mascota debe pertenecer estrictamente a un único propietario dentro del sistema.
* RN-04 - Validación de Contacto Celular (Formato Colombia): El número de teléfono de contacto del propietario debe ser un número celular válido para Colombia, compuesto exactamente por 10 dígitos numéricos (iniciando por el dígito 3). Para efectos de enlaces de mensajería, el sistema antepondrá automáticamente el prefijo de país +57.
* RN-05 - Dinamismo en la Edad del Paciente: La edad de una mascota no debe almacenarse como un dato numérico estático. Debe calcularse dinámicamente en tiempo de visualización a partir de la diferencia entre la fecha actual del sistema y la fecha de nacimiento (exacta o aproximada), expresándose en años y meses (o semanas/días para neonatos).
* RN-06 - Estandarización de Peso en Kilogramos (Kg): Todo peso corporal registrado en la ficha del paciente o en sus consultas debe ingresarse y validarse estrictamente en la unidad de medida de Kilogramos (Kg). Debe ser un valor numérico decimal mayor a cero y con un límite superior de control lógico (máximo 150.00 Kg).
* RN-07 - Cronología e Integridad de la Historia Clínica: La historia clínica debe presentar los procedimientos médicos ordenados cronológicamente desde el más reciente hasta el más antiguo. Los registros clínicos confirmados no admiten eliminación destructiva de la base de datos, garantizando la reserva y trazabilidad legal exigida por el Código de Ética Profesional Veterinario (Ley 576 de 2000).
* RN-08 - Estructura Mínima del Carnet de Vacunación: El carnet digital debe consolidar exclusivamente eventos de inmunización y desparasitación que cuenten con: nombre del producto/biológico, fecha de aplicación, fecha de próximo refuerzo y veterinario responsable.
* RN-09 - Política de Notificación Gratuita (Cero Costo Operativo): Las notificaciones a clientes no emplearán servicios de intermediación de pago (APIs comerciales de SMS o WhatsApp Business API pagas). Se ejecutarán mediante la construcción de hipervínculos universales (protocolo [https://wa.me/](https://wa.me/) o mailto:) que transfieran los datos parametrizados directamente a las aplicaciones instaladas en la estación de trabajo.
* RN-10 - Principio de Entrada Ágil de Datos (Anti-Fricción): Las interfaces de registro clínico deben priorizar la agilidad operativa: los campos de fecha deben precargarse con la fecha del sistema por defecto, se debe permitir el alta rápida de un propietario en la misma ventana de registro de la mascota, y no se exigirán campos irrelevantes para la práctica básica ambulatoria.
3. Requisitos Funcionales (RF) - Formato IEEE 830
Campo
	Detalle
	ID
	RF-01
	Nombre
	Autenticación y Control de Acceso Local
	Descripción
	El sistema DEBE autenticar la identidad del usuario mediante nombre de usuario y contraseña antes de permitir el acceso a las funciones operativas, validando las credenciales contra la base de datos local.
	Prioridad
	Alta
	Actores
	Médico Veterinario (Usuario único de sesión)
	Precondiciones
	La base de datos local debe estar inicializada con al menos una cuenta de usuario autorizada.
	Entradas y Atributos
	- Nombre de usuario (Obligatorio, texto)


- Contraseña (Obligatorio, texto secreto)
	Proceso / Comportamiento
	1. El usuario inicia la aplicación y el sistema presenta la pantalla de autenticación.


2. El usuario ingresa su nombre de usuario y contraseña.


3. El sistema busca el usuario en la base de datos local y verifica la coincidencia del valor de acceso.


4. Si las credenciales coinciden, el sistema inicia la sesión y redirige al panel principal.


5. Si no coinciden, el sistema muestra un mensaje de credenciales no válidas y no permite el ingreso.
	Salidas / Resultados
	Acceso concedido al panel principal de la aplicación o mensaje de error en pantalla.
	Postcondiciones
	Sesión de usuario activa en la aplicación local.
	Reglas de Negocio Asociadas
	RN-01
	

Campo
	Detalle
	ID
	RF-02
	Nombre
	Gestión de Propietarios
	Descripción
	El sistema DEBE permitir registrar, consultar y actualizar la información de los propietarios de mascotas, garantizando la captura de sus datos de contacto completos.
	Prioridad
	Alta
	Actores
	Médico Veterinario
	Precondiciones
	Sesión de usuario iniciada en el sistema.
	Entradas y Atributos
	- Número de documento de identificación (Obligatorio, texto/numérico)


- Tipo de documento (Opcional, selección)


- Nombres y apellidos completos (Obligatorio, texto)


- Número de teléfono móvil / WhatsApp (Obligatorio, numérico, 10 dígitos)


- Dirección de residencia (Opcional, texto)


- Correo electrónico (Opcional, texto con formato de correo)
	Proceso / Comportamiento
	1. El usuario selecciona la opción de registrar o modificar propietario.


2. El usuario diligencia los atributos requeridos.


3. El sistema valida que el teléfono cumpla con el estándar nacional colombiano de 10 dígitos y que el documento no se encuentre duplicado para registros nuevos.


4. El usuario confirma la operación y el sistema almacena los datos en la base de datos local.
	Salidas / Resultados
	Mensaje de confirmación en pantalla y registro de propietario persistido.
	Postcondiciones
	El propietario queda disponible para consulta y para asociación de nuevas mascotas.
	Reglas de Negocio Asociadas
	RN-03, RN-04, RN-10
	

Campo
	Detalle
	ID
	RF-03
	Nombre
	Búsqueda y Consulta de Propietarios
	Descripción
	El sistema DEBE permitir buscar y visualizar la información detallada de los propietarios y el listado de sus mascotas asociadas a partir de criterios de búsqueda ágiles.
	Prioridad
	Alta
	Actores
	Médico Veterinario
	Precondiciones
	Sesión iniciada; existencia de propietarios registrados.
	Entradas y Atributos
	- Criterio de búsqueda: Número de documento, nombre completo o número telefónico (Obligatorio, texto).
	Proceso / Comportamiento
	1. El usuario introduce el término de búsqueda.


2. El sistema filtra en la base de datos local las coincidencias exactas o parciales.


3. El sistema despliega la lista de coincidencias con datos básicos.


4. El usuario selecciona un propietario y el sistema presenta la ficha integral con su información de contacto y la lista de mascotas registradas a su nombre.
	Salidas / Resultados
	Visualización en pantalla de los datos del cliente y sus pacientes vinculados.
	Postcondiciones
	Ninguna alteración de datos; sistema listo para navegar hacia la mascota seleccionada.
	Reglas de Negocio Asociadas
	RN-03, RN-10
	

Campo
	Detalle
	ID
	RF-04
	Nombre
	Registro de Paciente (Mascota) con Asociación de Propietario
	Descripción
	El sistema DEBE registrar un nuevo paciente canino o felino asociándolo obligatoriamente a un propietario, permitiendo seleccionar uno existente o abrir la creación de uno nuevo dentro del mismo flujo operativo.
	Prioridad
	Alta
	Actores
	Médico Veterinario
	Precondiciones
	Sesión de usuario activa.
	Entradas y Atributos
	- Propietario asociado (Obligatorio, selección de existente o creación en pantalla)


- Nombre de la mascota (Obligatorio, texto)


- Especie (Obligatorio, selección: Canino, Felino, Otro)


- Raza (Obligatorio, texto)


- Sexo (Obligatorio, selección: Macho, Hembra)


- Fecha de nacimiento o aproximada (Obligatorio, fecha)


- Peso actual (Obligatorio, numérico decimal en Kg)


- Color / Señas particulares (Opcional, texto)


- Estado reproductivo (Opcional, selección: Castrado/Esterilizado, Entero)
	Proceso / Comportamiento
	1. El usuario abre el formulario de registro de mascota.


2. Si el propietario ya existe, lo busca y selecciona; si no existe, activa el panel de creación rápida e ingresa los datos del propietario (RF-02).


3. El usuario completa los datos biológicos del animal.


4. El sistema valida que el peso sea un valor positivo en Kg y que la fecha de nacimiento no sea posterior a la fecha actual.


5. El sistema calcula en pantalla la edad actual resultante.


6. El usuario confirma el registro y el sistema almacena el paciente vinculado al propietario.
	Salidas / Resultados
	Mensaje de éxito en pantalla y registro de paciente creado en la base de datos.
	Postcondiciones
	Paciente habilitado para apertura de historia clínica y procedimientos médicos.
	Reglas de Negocio Asociadas
	RN-03, RN-05, RN-06, RN-10
	

Campo
	Detalle
	ID
	RF-05
	Nombre
	Consulta y Actualización de Ficha de Paciente
	Descripción
	El sistema DEBE permitir consultar y actualizar los datos descriptivos de un paciente, calculando y mostrando siempre su edad cronológica en tiempo de consulta.
	Prioridad
	Media
	Actores
	Médico Veterinario
	Precondiciones
	Paciente previamente registrado en el sistema.
	Entradas y Atributos
	- Identificador o nombre del paciente a consultar (Obligatorio)


- Atributos editables: Nombre, raza, sexo, color, estado reproductivo, fecha de nacimiento, peso actual en Kg (Obligatorios/Opcionales según RF-04).
	Proceso / Comportamiento
	1. El usuario busca el paciente por su nombre o mediante la ficha del propietario.


2. El sistema recupera los datos y ejecuta el cálculo de edad comparando la fecha de nacimiento con la fecha actual.


3. El sistema muestra la ficha con la edad calculada (en años, meses o días).


4. Si el usuario actualiza algún dato permitido y confirma, el sistema valida las reglas de peso y fechas y actualiza el registro local.
	Salidas / Resultados
	Ficha del paciente visualizada y datos actualizados en la base de datos.
	Postcondiciones
	Datos descriptivos del paciente actualizados localmente.
	Reglas de Negocio Asociadas
	RN-05, RN-06
	

Campo
	Detalle
	ID
	RF-06
	Nombre
	Registro de Procedimiento Clínico y Consulta Médica
	Descripción
	El sistema DEBE registrar cada consulta o atención médica ejecutada sobre un paciente, almacenando de forma obligatoria el profesional veterinario específico que realizó la atención.
	Prioridad
	Alta
	Actores
	Médico Veterinario
	Precondiciones
	Paciente seleccionado; sesión activa.
	Entradas y Atributos
	- Paciente asociado (Obligatorio, referencia)


- Fecha y hora de atención (Obligatorio, fecha/hora, por defecto fecha/hora actual)


- Médico veterinario tratante (Obligatorio, selección: Dr. Fabio, Dr. William)


- Peso en la consulta (Obligatorio, numérico decimal en Kg)


- Motivo de consulta / Anamnesis (Obligatorio, texto)


- Hallazgos de examen clínico / Constantes vitales (Opcional, texto)


- Diagnóstico o presunción diagnóstica (Obligatorio, texto)


- Tratamiento, medicamentos y dosis administradas/recetadas (Obligatorio, texto)


- Indicaciones para el propietario (Opcional, texto)


- Fecha de control sugerida (Opcional, fecha)
	Proceso / Comportamiento
	1. El usuario accede al historial del paciente y selecciona "Nuevo Procedimiento".


2. El sistema precarga la fecha y hora del sistema.


3. El usuario selecciona obligatoriamente cuál de los dos veterinarios realizó la atención (Dr. Fabio o Dr. William).


4. El usuario ingresa el peso registrado y completa el motivo, examen, diagnóstico y tratamiento prescrito.


5. El sistema valida que los campos obligatorios estén completos y que el peso sea un decimal válido en Kg.


6. Al confirmar, el sistema almacena la entrada clínica asociada al paciente y actualiza el peso actual en la ficha de la mascota.
	Salidas / Resultados
	Registro de atención médica incorporado al expediente clínico del paciente.
	Postcondiciones
	Entrada clínica inmutable persistida en la base de datos local.
	Reglas de Negocio Asociadas
	RN-02, RN-06, RN-07, RN-10
	

Campo
	Detalle
	ID
	RF-07
	Nombre
	Visualización Consolidada de Historia Clínica
	Descripción
	El sistema DEBE presentar la historia clínica integral de un paciente, listando todos los procedimientos médicos en orden cronológico descendente, evidenciando de forma explícita el veterinario que atendió cada evento.
	Prioridad
	Alta
	Actores
	Médico Veterinario
	Precondiciones
	Paciente seleccionado en el sistema.
	Entradas y Atributos
	- Identificador de paciente (Obligatorio).


- Filtro por rango de fechas o profesional tratante (Opcional).
	Proceso / Comportamiento
	1. El usuario solicita visualizar el historial médico del paciente.


2. El sistema recupera todas las atenciones médicas y procedimientos registrados para dicho paciente.


3. El sistema ordena las atenciones de forma estrictamente cronológica inversa (la más reciente al inicio).


4. El sistema despliega cada registro detallando fecha, nombre del médico veterinario tratante, peso, anamnesis, diagnóstico y tratamiento aplicado.


5. El profesional en turno puede examinar todo el contexto previo sin requerir llamadas telefónicas.
	Salidas / Resultados
	Despliegue en pantalla del expediente clínico consolidado y trazable.
	Postcondiciones
	Ninguna modificación de datos (consulta de lectura).
	Reglas de Negocio Asociadas
	RN-02, RN-07
	

Campo
	Detalle
	ID
	RF-08
	Nombre
	Registro de Inmunización y Desparasitación
	Descripción
	El sistema DEBE registrar la aplicación de vacunas y antiparasitarios a un paciente, capturando las fechas de administración y la fecha programada para el próximo refuerzo.
	Prioridad
	Alta
	Actores
	Médico Veterinario
	Precondiciones
	Paciente registrado en el sistema.
	Entradas y Atributos
	- Paciente asociado (Obligatorio, referencia)


- Tipo de biológico (Obligatorio, selección: Vacuna, Desparasitación)


- Nombre del producto / vacuna aplicada (Obligatorio, texto)


- Lote / Fabricante (Opcional, texto)


- Fecha de aplicación (Obligatorio, fecha, por defecto fecha actual)


- Fecha de próxima dosis / refuerzo (Obligatorio, fecha)


- Médico veterinario responsable (Obligatorio, selección: Dr. Fabio, Dr. William)


- Observaciones (Opcional, texto)
	Proceso / Comportamiento
	1. El usuario abre el módulo de vacunación/desparasitación del paciente.


2. El sistema inicializa la fecha de aplicación con el día actual.


3. El usuario selecciona el veterinario que aplica la dosis y digita el nombre de la vacuna o antiparasitario.


4. El usuario define la fecha en la que vence el refuerzo o toca la próxima dosis.


5. El sistema valida que la fecha de próximo refuerzo sea posterior a la fecha de aplicación.


6. El usuario guarda el registro y el sistema lo añade al expediente de inmunización del paciente.
	Salidas / Resultados
	Registro de inmunización persistido en la base de datos local.
	Postcondiciones
	Nuevo registro habilitado para inclusión en el carnet digital y en el generador de recordatorios.
	Reglas de Negocio Asociadas
	RN-02, RN-08, RN-10
	

Campo
	Detalle
	ID
	RF-09
	Nombre
	Generación y Descarga de Carnet de Vacunación Digital en PDF
	Descripción
	El sistema DEBE compilar y exportar el historial de vacunas y desparasitaciones del paciente en un archivo digital estructurado en formato PDF, descargable localmente en el equipo para su envío o impresión.
	Prioridad
	Alta
	Actores
	Médico Veterinario
	Precondiciones
	Paciente seleccionado con al menos un registro de vacuna o desparasitación.
	Entradas y Atributos
	- Identificador de paciente (Obligatorio)


- Ruta de destino local para guardado de archivo (Obligatorio, selección de carpeta del sistema de archivos)
	Proceso / Comportamiento
	1. El usuario hace clic en la acción "Generar Carnet Digital PDF".


2. El sistema extrae los datos del propietario, de la mascota (nombre, especie, raza, edad calculada, peso) y la tabla de vacunas y desparasitaciones registradas.


3. El sistema ensambla el documento incorporando nombre de la clínica veterinaria, identificación del paciente y tabla con columnas: Fecha de Aplicación, Biológico/Producto, Lote, Próximo Refuerzo y Médico Responsable.


4. El sistema solicita la ubicación de guardado en el computador y genera el archivo binario PDF.


5. El sistema notifica al usuario que el archivo ha sido creado satisfactoriamente y ofrece abrir el archivo o la carpeta contenedora.
	Salidas / Resultados
	Documento en formato PDF almacenado en el almacenamiento local del equipo.
	Postcondiciones
	Archivo digital disponible para ser compartido al cliente o archivado.
	Reglas de Negocio Asociadas
	RN-05, RN-06, RN-08
	

Campo
	Detalle
	ID
	RF-10
	Nombre
	Generación de Enlace Directo de Recordatorio por WhatsApp (wa.me)
	Descripción
	El sistema DEBE construir un enlace URI dinámico utilizando el esquema público de WhatsApp con el número del propietario y un texto predefinido del recordatorio, abriendo el navegador o cliente local de forma 100% gratuita.
	Prioridad
	Alta
	Actores
	Médico Veterinario
	Precondiciones
	El paciente debe tener una vacuna, desparasitación o control con fecha de próxima cita; el propietario debe tener teléfono válido registrado.
	Entradas y Atributos
	- Paciente y propietario seleccionados (Obligatorio)


- Tipo de evento a recordar (Obligatorio, selección: Vacunación, Desparasitación, Control Médico)


- Fecha programada del evento (Obligatorio, fecha extraída del registro)


- Nombre del producto o motivo (Obligatorio, texto extraído)


- Teléfono del propietario (Obligatorio, formateado automáticamente con prefijo 57)
	Proceso / Comportamiento
	1. El usuario ingresa a la vista de recordatorios pendientes o a la ficha del paciente y selecciona "Notificar por WhatsApp".


2. El sistema redacta automáticamente una plantilla de texto personalizada que incluye: nombre del dueño, nombre de la mascota, tipo de procedimiento pendiente y fecha estimada de la cita.


3. El sistema codifica el texto en formato compatible con URI.


4. El sistema construye el enlace: [https://wa.me/57](https://wa.me/57)[TELEFONO]?text=[MENSAJE_CODIFICADO].


5. El sistema ejecuta la apertura del enlace en el navegador o aplicación de mensajería del equipo sin costo operativo alguno.
	Salidas / Resultados
	Apertura del cliente de WhatsApp con el chat del cliente y el mensaje prellenado listo para enviar.
	Postcondiciones
	La aplicación externa asume la comunicación; el sistema no incurre en cobros de API.
	Reglas de Negocio Asociadas
	RN-04, RN-09, RN-10
	

Campo
	Detalle
	ID
	RF-11
	Nombre
	Generación Alternativa Gratuita de Recordatorio por Correo (mailto:)
	Descripción
	El sistema DEBE permitir la construcción de un enlace con protocolo estándar mailto: para abrir el cliente de correo predeterminado del sistema operativo con los datos del recordatorio prellenados, sin costo por transacción.
	Prioridad
	Media
	Actores
	Médico Veterinario
	Precondiciones
	El propietario debe contar con una dirección de correo electrónico válida registrada en el sistema.
	Entradas y Atributos
	- Correo electrónico del propietario (Obligatorio, formato de correo válido)


- Nombre de la mascota y del propietario (Obligatorio, texto)


- Tipo de cita / vacuna y fecha del evento (Obligatorio, texto/fecha)
	Proceso / Comportamiento
	1. El usuario selecciona la opción "Notificar por Correo Electrónico".


2. El sistema valida la presencia del correo del propietario.


3. El sistema ensambla un asunto descriptivo (ej. "Recordatorio Veterinario para [Mascota]") y un cuerpo de mensaje formal.


4. El sistema codifica los parámetros bajo el estándar de URI mailto:[CORREO]?subject=[ASUNTO]&body=[CUERPO].


5. El sistema invoca el gestor de correo predeterminado del equipo para que el veterinario envíe el mensaje con un solo clic.
	Salidas / Resultados
	Ventana del cliente de correo local abierta con destinatario, asunto y cuerpo listos para el envío.
	Postcondiciones
	Comunicación gestionada por el cliente de correo nativo a costo cero.
	Reglas de Negocio Asociadas
	RN-09, RN-10
	4. Requisitos No Funcionales (RNF) - Formato IEEE 830
Categoría: Seguridad
ID
	Nombre
	Descripción
	Criterio de Aceptación / Métrica
	RNF-01
	Protección de Credenciales
	El sistema DEBE almacenar las contraseñas de acceso protegidas mediante funciones de resumen criptográfico irreversible (hashing con sal), prohibiendo contraseñas en texto plano.
	Validación dinámica contra la base de datos local donde ninguna credencial sea legible en archivos de configuración o tablas de datos.
	RNF-02
	Privacidad y Reserva Legal de Datos
	El sistema DEBE restringir el acceso a la historia clínica exclusivamente a través de la interfaz de la aplicación, dando cumplimiento al Art. 61 de la Ley 576 de 2000 sobre la reserva legal del documento médico.
	No debe existir acceso público a la lectura de registros médicos; toda exportación documental debe ser generada bajo demanda del profesional.
	Categoría: Usabilidad
ID
	Nombre
	Descripción
	Criterio de Aceptación / Métrica
	RNF-03
	Reducción de Fricción Operativa (Anti-VeteSoft)
	La interfaz de usuario DEBE optimizarse para que el registro de una consulta clínica estándar no tome más de 90 segundos ni requiera más de 2 formularios de interacción.
	Medición en pruebas de usabilidad donde el veterinario complete el flujo de atención en menos de 1.5 minutos sin asistencia técnica.
	RNF-04
	Legibilidad y Claridad Visual
	La tipografía, contraste y distribución visual DEBEN ser legibles a una distancia operativa de al menos 1 metro de la pantalla del computador.
	Relación de contraste mínima de 4.5:1 en elementos de texto y tamaño de fuente principal de al menos 14 puntos para visualización en consultorio.
	Categoría: Rendimiento
ID
	Nombre
	Descripción
	Criterio de Aceptación / Métrica
	RNF-05
	Tiempo de Respuesta en Operación Local
	El sistema DEBE consultar y desplegar la historia clínica completa de un paciente en un tiempo inferior a 1 segundo al ejecutarse en la máquina local.
	Tiempo de carga y renderizado de expediente clínico menor a 1000 ms para pacientes con hasta 100 registros históricos.
	RNF-06
	Eficiencia en Generación de Documentos PDF
	La generación y descarga del carnet de vacunación en formato PDF no DEBE tomar más de 3 segundos desde la confirmación del usuario.
	Archivo PDF generado y escrito en disco en $\le 3$ segundos sin congelar la interfaz gráfica de usuario.
	Categoría: Disponibilidad
ID
	Nombre
	Descripción
	Criterio de Aceptación / Métrica
	RNF-07
	Operación Autónoma Fuera de Línea (Offline)
	El 100% de las funciones de gestión de historias clínicas, pacientes, vacunas y exportación de carnets DEBEN operar sin requerir conexión a internet activa.
	El sistema opera normalmente con la interfaz de red desconectada; únicamente la resolución de enlaces de mensajería requerirá conexión a internet externa.
	Categoría: Restricciones del Entorno
ID
	Nombre
	Descripción
	Criterio de Aceptación / Métrica
	RNF-08
	Arquitectura de Estación Monopuesto
	El sistema DEBE estar concebido para funcionar en un único computador físico, manteniendo el almacenamiento de datos en la misma máquina sin depender de servidores dedicados.
	El software y su base de datos deben ejecutarse completamente en el entorno del sistema operativo local de la clínica.
	RNF-09
	Cero Costo de Infraestructura de Mensajería
	El software NO DEBE consumir servicios de mensajería que impliquen costos recurrentes, tarifas por mensaje o contratación de planes corporativos de API.
	Utilización estricta de esquemas URI universales del sistema operativo (wa.me y mailto:) sin pasarelas de intermediación de pago.
	5. Casos de Uso (CU)






                 +---------------------------------------------+
                |            SOFTWARE VETERINARIO             |
                |                                             |
                |   +-------------------------------------+   |
                |   | CU-01: Iniciar Sesión en el Sistema |   |
                |   +-------------------------------------+   |
                |                      ^                      |
                |                      | <<include>>          |
                |   +-------------------------------------+   |
                |   | CU-02: Registrar Mascota y Dueño    |   |
                |   +-------------------------------------+   |
+--------------+ |                                             |
|              | |   +-------------------------------------+   |
|  Veterinario |---->| CU-03: Registrar Atención Médica    |   |
| (Fabio /     | |   +-------------------------------------+   |
|  William)    | |                                             |
|              | |   +-------------------------------------+   |
|              |---->| CU-04: Consultar Historia Clínica   |   |
+--------------+ |   +-------------------------------------+   |
                |                                             |
                |   +-------------------------------------+   |
                |   | CU-05: Gestionar Carnet Digital PDF |   |
                |   +-------------------------------------+   |
                |                                             |
                |   +-------------------------------------+   |
                |   | CU-06: Emitir Recordatorio Gratuito |   |
                |   +-------------------------------------+   |
                +---------------------------------------------+

CU-01: Iniciar Sesión en el Sistema
* Actores: Médico Veterinario.
* Precondiciones: El software se encuentra abierto en la pantalla de bienvenida.
* Flujo Principal:
   1. El veterinario ingresa su nombre de usuario y contraseña.
   2. El sistema valida las credenciales contra la base de datos local.
   3. El sistema confirma la autenticidad e inicializa la sesión operativa.
   4. El sistema presenta la pantalla de inicio con acceso a los módulos clínicos.
* Flujos Alternativos y Excepciones:
   * Excepción 1a (Credenciales Incorrectas): Si los datos no coinciden con los registros de la base de datos, el sistema muestra el mensaje "Usuario o contraseña incorrectos", limpia el campo de contraseña y mantiene al usuario en la pantalla de ingreso.
* Postcondiciones: La sesión queda activa y lista para operar en la estación local.
CU-02: Registrar Mascota y Propietario (Nuevo o Existente)
* Actores: Médico Veterinario.
* Precondiciones: Sesión de usuario activa.
* Flujo Principal:
   1. El veterinario selecciona la opción "Registrar Paciente".
   2. El veterinario busca al propietario por su número de cédula o nombre.
   3. El sistema encuentra al propietario y el veterinario lo selecciona como tutor legal del animal.
   4. El veterinario digita el nombre del animal, especie, raza, sexo, peso actual en Kg y fecha de nacimiento (o aproximada).
   5. El sistema calcula y muestra inmediatamente la edad cronológica de la mascota.
   6. El veterinario confirma el registro.
   7. El sistema almacena la mascota vinculada al propietario seleccionado.
* Flujos Alternativos y Excepciones:
   * Flujo Alternativo 2a (Propietario no existe / Cliente nuevo):
      1. En el paso 3, el sistema indica que no existen coincidencias para el criterio de búsqueda.
      2. El veterinario activa el botón "Crear Propietario en este flujo".
      3. El sistema despliega los campos: Cédula, Nombres y Apellidos, Teléfono Celular (10 dígitos), Dirección y Correo electrónico.
      4. El veterinario ingresa los datos y confirma.
      5. El sistema valida el formato del teléfono celular (10 dígitos colombianos), guarda el propietario y lo asocia automáticamente como responsable del nuevo paciente, continuando en el paso 4 del flujo principal.
   * Excepción 2b (Teléfono no cumple formato colombiano): El sistema advierte "El número debe contener 10 dígitos numéricos válidos" y detiene el proceso hasta su corrección.
* Postcondiciones: El paciente queda registrado en el sistema con su propietario enlazado y listo para recibir atenciones.
CU-03: Registrar Atención Médica con Asignación de Profesional
* Actores: Médico Veterinario (Dr. Fabio o Dr. William).
* Precondiciones: Sesión activa; el paciente existe en la base de datos.
* Flujo Principal:
   1. El veterinario localiza la ficha del paciente.
   2. El veterinario selecciona la acción "Nuevo Procedimiento Clínico".
   3. El sistema precarga la fecha y hora actual del equipo.
   4. El profesional selecciona obligatoriamente su nombre en el campo "Veterinario Tratante" (Dr. Fabio o Dr. William).
   5. El profesional ingresa el peso actual evaluado en Kg, el motivo de consulta, diagnóstico y tratamiento/fármacos formulados.
   6. El veterinario presiona "Guardar Procedimiento".
   7. El sistema valida la completitud de los campos obligatorios y persiste el registro de atención en la historia clínica.
* Flujos Alternativos y Excepciones:
   * Excepción 3a (Omisión del Veterinario Tratante): Si el usuario intenta guardar sin seleccionar el nombre del profesional que atendió al paciente, el sistema detiene el guardado y resalta en rojo: "Debe seleccionar el médico veterinario responsable de la atención".
   * Excepción 3b (Peso fuera de rango): Si el peso ingresado es negativo, cero o superior a 150 Kg, el sistema solicita corregir el valor numérico en kilogramos.
* Postcondiciones: La historia clínica del paciente cuenta con una nueva entrada inmutable y trazable al médico actuante.
CU-04: Consultar Historia Clínica Unificada
* Actores: Médico Veterinario (en turno matutino o vespertino).
* Precondiciones: El paciente cuenta con antecedentes clínicos en el sistema.
* Flujo Principal:
   1. El veterinario en turno ingresa el nombre de la mascota o el teléfono/documento del propietario en el buscador rápido.
   2. El sistema despliega las coincidencias y el veterinario selecciona al paciente.
   3. El sistema carga de inmediato la ficha general (nombre, especie, raza, peso y edad dinámica) y la lista de procedimientos médicos.
   4. El sistema presenta las notas médicas ordenadas cronológicamente de la más reciente a la más antigua, mostrando con claridad la fecha, el médico que atendió (Fabio o William), el diagnóstico y la fórmula administrada.
   5. El veterinario revisa el tratamiento prescrito en el turno anterior sin necesidad de contactar por teléfono a su socio.
* Flujos Alternativos y Excepciones:
   * Flujo Alternativo 4a (Paciente sin atenciones previas): El sistema muestra la ficha biológica del paciente indicando "No registra atenciones clínicas previas" y ofrece el botón de primer procedimiento.
* Postcondiciones: El veterinario dispone de todo el contexto clínico para continuar con el manejo médico del animal.
CU-05: Registrar Vacuna y Descargar Carnet Digital en PDF
* Actores: Médico Veterinario.
* Precondiciones: Mascota registrada en la base de datos.
* Flujo Principal:
   1. El veterinario accede a la pestaña "Vacunación y Desparasitación" del paciente.
   2. El veterinario selecciona "Registrar Dosis".
   3. El veterinario elige el tipo (Vacuna / Antiparasitario), escribe el nombre del producto, confirma la fecha actual de aplicación, digita la fecha tentativa del próximo refuerzo y selecciona su nombre como responsable.
   4. El sistema almacena la dosis en la base de datos local.
   5. El veterinario hace clic en "Descargar Carnet Digital en PDF".
   6. El sistema compila todas las dosis históricas del animal en una plantilla visual con membrete de la clínica, genera el archivo PDF y abre la ventana para guardarlo en el disco local del equipo.
* Flujos Alternativos y Excepciones:
   * Excepción 5a (Fecha de refuerzo incongruente): Si la fecha del próximo refuerzo es anterior o igual a la fecha de aplicación, el sistema muestra la advertencia: "La fecha del próximo refuerzo debe ser posterior a la fecha de aplicación".
* Postcondiciones: Registro de inmunización persistido y archivo PDF generado localmente para custodia o entrega al cliente.
CU-06: Emitir Recordatorio Gratuito al Propietario
* Actores: Médico Veterinario.
* Precondiciones: Paciente con fecha futura de control, vacuna o desparasitación; propietario con celular registrado.
* Flujo Principal:
   1. El veterinario consulta las fechas de refuerzo próximas o accede al registro de vacunación del paciente.
   2. El veterinario selecciona la opción "Enviar Recordatorio WhatsApp (wa.me)".
   3. El sistema recupera el celular del dueño, le asigna el prefijo nacional 57, y genera un mensaje con la plantilla:"Hola [Nombre Propietario], la Clínica Veterinaria le recuerda que su mascota [Nombre Mascota] tiene programada su [Vacuna/Control] el día [Fecha]. ¡Los esperamos!"
   4. El sistema codifica la URL y llama al navegador predeterminado: [https://wa.me/57](https://wa.me/57)[TELEFONO]?text=[MENSAJE].
   5. El navegador abre WhatsApp Web o la aplicación de escritorio en el chat del cliente con el texto preparado para enviar.
* Flujos Alternativos y Excepciones:
   * Flujo Alternativo 6a (Canal Alternativo Gratuito por Correo Electrónico):
      1. En el paso 2, el veterinario elige la opción "Enviar Recordatorio por Correo".
      2. El sistema valida que el propietario tenga correo registrado.
      3. El sistema invoca el esquema URI mailto:[CORREO]?subject=[ASUNTO]&body=[MENSAJE].
      4. Se abre el cliente de correo del sistema operativo listo para despacho.
   * Excepción 6b (Propietario sin número telefónico válido): Si el propietario carece de número celular o este tiene menos de 10 dígitos, el sistema notifica "El número de contacto no es válido para enlace de WhatsApp" e impide abrir el navegador.
* Postcondiciones: La notificación se canaliza por las aplicaciones del usuario sin ningún cobro o cargo de servicios de mensajería empresarial.
6. Matriz de Trazabilidad
Caso de Uso (CU)
	Requisito Funcional (RF)
	Reglas de Negocio Asociadas (RN)
	CU-01: Iniciar Sesión en el Sistema
	RF-01
	RN-01
	CU-02: Registrar Mascota y Propietario
	RF-02, RF-03, RF-04, RF-05
	RN-03, RN-04, RN-05, RN-06, RN-10
	CU-03: Registrar Atención Médica
	RF-06
	RN-02, RN-06, RN-07, RN-10
	CU-04: Consultar Historia Clínica
	RF-07
	RN-02, RN-07
	CU-05: Registrar Vacuna y Descargar Carnet
	RF-08, RF-09
	RN-02, RN-05, RN-06, RN-08, RN-10
	CU-06: Emitir Recordatorio Gratuito
	RF-10, RF-11
	RN-04, RN-09, RN-10