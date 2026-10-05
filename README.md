# VetClinic Pro - Sistema de Gestión Clínica Veterinaria

Sistema de escritorio para la administración de historias clínicas, pacientes, propietarios y planes de inmunización en clínicas veterinarias, diseñado bajo principios de inmutabilidad legal (Ley 576 de 2000, Colombia), rendimiento local y cero costo operativo en mensajería.

---

## 📋 Tabla de Contenidos

- [Características Principales](#-características-principales)
- [Arquitectura y Stack Tecnológico](#-arquitectura-y-stack-tecnológico)
- [Estructura del Proyecto](#-estructura-del-proyecto)
- [Requisitos Previos](#-requisitos-previos)
- [Instalación y Puesta en Marcha](#-instalación-y-puesta-en-marcha)
- [Reglas de Negocio Destacadas](#-reglas-de-negocio-destacadas)
- [Documentación del Repositorio](#-documentación-del-repositorio)

---

## ✨ Características Principales

- **Control de Acceso Local Seguro**: Autenticación criptográfica local con algoritmo PBKDF2 (HMAC-SHA256, sal de 128 bits, 100.000 iteraciones).
- **Gestión Integral de Propietarios y Pacientes**:
  - Validación estricta de teléfono celular para Colombia (`+57`, 10 dígitos iniciando en 3).
  - Cálculo dinámico de edad en tiempo de visualización (días, meses, años).
  - Estandarización de peso corporal en kilogramos (Kg).
- **Historia Clínica Inmutable**:
  - Registro de consultas, anamnesis, examen físico, diagnóstico y tratamiento.
  - Trazabilidad nominal obligatoria del veterinario tratante (Dr. Fabio / Dr. William).
  - Inmutabilidad estricta: los registros clínicos no admiten borrado ni edición destructiva.
- **Carnet de Vacunación y Desparasitación Digital**:
  - Control de biológicos y fechas de refuerzo.
  - Exportación nativa a PDF de alto rendimiento mediante `QuestPDF`.
- **Recordatorios Gratuitos (Zero-Cost Messaging)**:
  - Generación de enlaces universales de WhatsApp Web / API (`wa.me`) con plantillas predefinidas.
  - Soporte para enlaces directos de correo mediante protocolo estándar `mailto:`.

---

## 🛠️ Arquitectura y Stack Tecnológico

El proyecto está diseñado bajo el patrón **MVVM Estricto** desacoplado en tres capas con Inyección de Dependencias nativa (`Microsoft.Extensions.DependencyInjection`):

- **Plataforma:** .NET 8.0 LTS (C# 12)
- **Capa de Presentación:** WPF (`VetClinic.Presentation`)
- **Capa de Dominio:** .NET Standard / .NET 8 Class Library (`VetClinic.Domain`)
- **Capa de Infraestructura y Acceso a Datos:** Entity Framework Core 8.0 (`VetClinic.Infrastructure`)
- **Motor de Base de Datos:** SQLite local con modo WAL (`Write-Ahead Logging`) y Foreign Keys activas
- **Generación de Reportes / PDF:** QuestPDF (Community License)

---

## 📁 Estructura del Proyecto

```text
VetClinicSolution.sln
│
├── VetClinic.Presentation/         # Capa de Presentación (WPF .NET 8.0-windows)
│   ├── Assets/                     # Recursos gráficos y vectores XAML
│   ├── Converters/                 # Value Converters (fechas, kilogramos, visibilidad)
│   ├── Services/                   # Implementaciones de UI (DialogService, NavigationService)
│   ├── ViewModels/                 # Modelos de vista (MVVM, comandos reactivos)
│   ├── Views/                      # Vistas y ventanas XAML
│   └── Styles/                     # Estilos y diccionarios de recursos
│
├── VetClinic.Domain/               # Capa de Dominio (Entidades y Lógica de Negocio)
│   ├── Entities/                   # Usuario, Veterinario, Propietario, Paciente, etc.
│   ├── Enums/                      # Especies, sexos, tipos de biológicos
│   ├── Interfaces/                 # Contratos de repositorios y servicios de dominio
│   └── ValueObjects/               # Objetos de valor (NumeroCelular, PesoCorporal)
│
└── VetClinic.Infrastructure/       # Capa de Infraestructura y Datos
    ├── Data/                       # DbContext, migraciones y DbInitializer
    ├── Repositories/               # Implementación de repositorios y Unit of Work
    ├── Security/                   # Hash criptográfico PBKDF2
    └── Services/                   # QuestPdfExportService, ExternalLauncherService
```

---

## ⚙️ Requisitos Previos

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (versión 8.0.x o superior)
- Sistema Operativo: Windows 10/11 (recomendado para desarrollo/ejecución WPF) o entorno compatible con soporte para desktop .NET
- IDE recomendada: Visual Studio 2022 (con carga de trabajo *.NET Desktop Development*) o JetBrains Rider / VS Code con extensiones C# Dev Kit

---

## 🚀 Instalación y Puesta en Marcha

1. **Clonar el repositorio:**
   ```bash
   git clone https://github.com/AnyGomez13/GeminiSoftware.git
   cd GeminiSoftware
   ```

2. **Restaurar dependencias:**
   ```bash
   dotnet restore VetClinicSolution.sln
   ```

3. **Compilar la solución:**
   ```bash
   dotnet build VetClinicSolution.sln -c Release
   ```

4. **Ejecutar la aplicación:**
   ```bash
   dotnet run --project VetClinic.Presentation
   ```

> **Nota:** En el primer inicio, `DbInitializer` crea automáticamente la base de datos SQLite local (`vetclinic.db`), aplica las restricciones de inmutabilidad y siembra el usuario administrativo inicial junto con el catálogo de veterinarios.

---

## ⚖️ Reglas de Negocio Destacadas

| Código | Regla | Descripción |
| :--- | :--- | :--- |
| **RN-01** | Autenticación Obligatoria | Validación estricta contra base de datos local mediante hash PBKDF2. |
| **RN-02** | Trazabilidad del Médico | Obligatoriedad de seleccionar al veterinario tratante en cada evento clínico. |
| **RN-04** | Formato Celular Colombia | Validación exacta de 10 dígitos numéricos iniciando en `3`. |
| **RN-05** | Edad Dinámica | La edad no se guarda estática; se calcula al momento de visualización. |
| **RN-06** | Peso Estandarizado | Registro numérico estricto en kilogramos (Kg), decimal mayor a cero. |
| **RN-07** | Inmutabilidad de la Historia | Prohibición absoluta de borrado físico o modificación arbitraria de actos clínicos (Ley 576 de 2000). |
| **RN-09** | Enlaces Sin Costo | Notificaciones vía esquemas directos `https://wa.me/` y `mailto:` sin intermediarios de pago. |

---

## 📚 Documentación del Repositorio

- [requisitos.md](file:///home/jorshua/Imágenes/GeminiSoftware/requisitos.md): Especificación de Requisitos de Software bajo estándar IEEE 830, reglas de negocio detalladas y casos de uso.
- [diseño.md](file:///home/jorshua/Imágenes/GeminiSoftware/diseño.md): Especificación de arquitectura técnica, esquemas de base de datos SQLite, diagramas de clases, DDL e implementación de servicios.