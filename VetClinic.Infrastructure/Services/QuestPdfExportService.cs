using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using VetClinic.Domain.Common;
using VetClinic.Domain.Entities;
using VetClinic.Domain.Interfaces.Services;
using VetClinic.Infrastructure.Data;

namespace VetClinic.Infrastructure.Services;

public class QuestPdfExportService : IPdfExportService
{
    private readonly VetClinicDbContext _context;

    public QuestPdfExportService(VetClinicDbContext context)
    {
        _context = context;
        // Configurar licencia comunitaria de QuestPDF (STF-03)
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task<Result<string>> GenerarCarnetVacunacionAsync(int pacienteId, string rutaDestino, CancellationToken cancellationToken = default)
    {
        try
        {
            var stopwatch = Stopwatch.StartNew();

            var paciente = await _context.Pacientes
                .AsNoTracking()
                .Include(p => p.Propietario)
                .Include(p => p.Inmunizaciones)
                    .ThenInclude(i => i.Veterinario)
                .FirstOrDefaultAsync(p => p.Id == pacienteId, cancellationToken);

            if (paciente is null)
            {
                return Result.Failure<string>($"No se encontró el paciente con ID {pacienteId}.");
            }

            var directorio = Path.GetDirectoryName(rutaDestino);
            if (!string.IsNullOrEmpty(directorio) && !Directory.Exists(directorio))
            {
                Directory.CreateDirectory(directorio);
            }

            var documento = ConstruirDocumento(paciente);
            documento.GeneratePdf(rutaDestino);
            stopwatch.Stop();

            // Validación de RNF-06: compilación y escritura en disco en <= 3 segundos
            if (stopwatch.ElapsedMilliseconds > 3000)
            {
                Debug.WriteLine($"[ADVERTENCIA] Generación de PDF tomó {stopwatch.ElapsedMilliseconds} ms, superando el objetivo de 3000 ms.");
            }

            return Result.Success(rutaDestino);
        }
        catch (Exception ex)
        {
            return Result.Failure<string>($"Error al generar el carnet digital en PDF: {ex.Message}");
        }
    }

    public async Task<Result<byte[]>> GenerarCarnetVacunacionBytesAsync(int pacienteId, CancellationToken cancellationToken = default)
    {
        try
        {
            var stopwatch = Stopwatch.StartNew();

            var paciente = await _context.Pacientes
                .AsNoTracking()
                .Include(p => p.Propietario)
                .Include(p => p.Inmunizaciones)
                    .ThenInclude(i => i.Veterinario)
                .FirstOrDefaultAsync(p => p.Id == pacienteId, cancellationToken);

            if (paciente is null)
            {
                return Result.Failure<byte[]>($"No se encontró el paciente con ID {pacienteId}.");
            }

            var documento = ConstruirDocumento(paciente);
            var bytes = documento.GeneratePdf();
            stopwatch.Stop();

            if (stopwatch.ElapsedMilliseconds > 3000)
            {
                Debug.WriteLine($"[ADVERTENCIA] Generación de PDF en memoria tomó {stopwatch.ElapsedMilliseconds} ms, superando el objetivo de 3000 ms.");
            }

            return Result.Success(bytes);
        }
        catch (Exception ex)
        {
            return Result.Failure<byte[]>($"Error al generar el carnet digital en PDF: {ex.Message}");
        }
    }

    private static Document ConstruirDocumento(Paciente paciente)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.Letter);
                page.Margin(30);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontFamily("Segoe UI").FontSize(10).FontColor("#212121"));

                // Encabezado institucional
                page.Header().Column(col =>
                {
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Column(headerCol =>
                        {
                            headerCol.Item().Text("CLÍNICA VETERINARIA").FontSize(18).Bold().FontColor("#1B5E20");
                            headerCol.Item().Text("VetClinic Pro - Estación de Cuidado Animal").FontSize(10).FontColor("#616161");
                            headerCol.Item().Text("Dres. Fabio y William | Atención Médica e Inmunización").FontSize(9).FontColor("#616161");
                        });

                        row.ConstantItem(120).AlignRight().Text("CARNET DIGITAL").FontSize(12).Bold().FontColor("#2E7D32");
                    });

                    col.Item().PaddingVertical(8).LineHorizontal(1).LineColor("#2E7D32");
                });

                    // Contenido del documento
                    page.Content().Column(col =>
                    {
                        // Ficha del Paciente y Propietario
                        col.Item().Background("#F8F9FA").Padding(10).Row(row =>
                        {
                            // Columna Paciente
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("DATOS DEL PACIENTE").FontSize(11).Bold().FontColor("#1B5E20");
                                c.Item().Text($"Nombre: {paciente.Nombre}").Bold();
                                c.Item().Text($"Especie / Raza: {paciente.Especie} / {paciente.Raza}");
                                c.Item().Text($"Sexo: {paciente.Sexo}");
                                c.Item().Text($"Edad actual: {paciente.CalcularEdadFormateada()}");
                                c.Item().Text($"Último peso: {paciente.PesoActualKg:F2} Kg");
                            });

                            // Columna Propietario
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("DATOS DEL PROPIETARIO").FontSize(11).Bold().FontColor("#1B5E20");
                                var prop = paciente.Propietario;
                                if (prop != null)
                                {
                                    c.Item().Text($"Tutor: {prop.GetNombreCompleto()}").Bold();
                                    c.Item().Text($"Documento: {prop.TipoDocumento} {prop.NumeroDocumento}");
                                    c.Item().Text($"Teléfono: {prop.Telefono}");
                                    c.Item().Text($"Correo: {prop.Email ?? "No registrado"}");
                                }
                                else
                                {
                                    c.Item().Text("Tutor no asociado.");
                                }
                            });
                        });

                        col.Item().PaddingVertical(10);

                        // Tabla de Inmunizaciones y Desparasitaciones (RN-08)
                        col.Item().Text("HISTORIAL DE VACUNACIÓN Y DESPARASITACIÓN").FontSize(12).Bold().FontColor("#1B5E20");

                        col.Item().PaddingTop(5).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(75);  // Fecha Aplicación
                                columns.ConstantColumn(85);  // Tipo
                                columns.RelativeColumn(2);   // Producto
                                columns.RelativeColumn(1);   // Lote
                                columns.ConstantColumn(75);  // Próximo Refuerzo
                                columns.RelativeColumn(1.5f);// Médico Responsable
                            });

                            // Cabecera de tabla
                            table.Header(header =>
                            {
                                header.Cell().Background("#1B5E20").Padding(4).Text("Aplicación").Bold().FontColor(Colors.White);
                                header.Cell().Background("#1B5E20").Padding(4).Text("Tipo").Bold().FontColor(Colors.White);
                                header.Cell().Background("#1B5E20").Padding(4).Text("Biológico/Fármaco").Bold().FontColor(Colors.White);
                                header.Cell().Background("#1B5E20").Padding(4).Text("Lote").Bold().FontColor(Colors.White);
                                header.Cell().Background("#1B5E20").Padding(4).Text("Refuerzo").Bold().FontColor(Colors.White);
                                header.Cell().Background("#1B5E20").Padding(4).Text("Veterinario").Bold().FontColor(Colors.White);
                            });

                            var listaInmunizaciones = paciente.Inmunizaciones
                                .OrderByDescending(i => i.FechaAplicacion)
                                .ToList();

                            if (listaInmunizaciones.Count == 0)
                            {
                                table.Cell().ColumnSpan(6).Padding(10).AlignCenter().Text("No se registran eventos de inmunización o desparasitación.");
                            }
                            else
                            {
                                foreach (var inm in listaInmunizaciones)
                                {
                                    table.Cell().BorderBottom(0.5f).BorderColor("#E0E0E0").Padding(4).Text(inm.FechaAplicacion.ToString("dd/MM/yyyy"));
                                    table.Cell().BorderBottom(0.5f).BorderColor("#E0E0E0").Padding(4).Text(inm.TipoBiologico.ToString());
                                    table.Cell().BorderBottom(0.5f).BorderColor("#E0E0E0").Padding(4).Text(inm.NombreProducto);
                                    table.Cell().BorderBottom(0.5f).BorderColor("#E0E0E0").Padding(4).Text(inm.LoteFabricante ?? "-");
                                    table.Cell().BorderBottom(0.5f).BorderColor("#E0E0E0").Padding(4).Text(inm.FechaRefuerzo.ToString("dd/MM/yyyy")).Bold().FontColor("#2E7D32");
                                    table.Cell().BorderBottom(0.5f).BorderColor("#E0E0E0").Padding(4).Text(inm.Veterinario?.Nombre ?? "Dr. Fabio");
                                }
                            }
                        });
                    });

                    // Pie de página
                    page.Footer().Column(col =>
                    {
                        col.Item().LineHorizontal(0.5f).LineColor("#E0E0E0");
                        col.Item().PaddingTop(4).Row(row =>
                        {
                            row.RelativeItem().Text($"Documento legal expedido el {DateTime.Now:dd/MM/yyyy HH:mm} - Ley 576 de 2000").FontSize(8).FontColor("#616161");
                            row.ConstantItem(80).AlignRight().Text(text =>
                            {
                                text.Span("Página ");
                                text.CurrentPageNumber();
                                text.Span(" de ");
                                text.TotalPages();
                            });
                        });
                    });
                });
            });
    }
}
