using VetClinic.Domain.Common;
using VetClinic.Domain.Enums;

namespace VetClinic.Domain.Entities;

public class Paciente : BaseEntity
{
    public int PropietarioId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public Especie Especie { get; set; } = Especie.Canino;
    public string Raza { get; set; } = string.Empty;
    public Sexo Sexo { get; set; } = Sexo.Macho;
    public DateTime FechaNacimiento { get; set; } = DateTime.Today;
    public bool EsFechaEstimada { get; set; } = false;
    public double PesoActualKg { get; set; }
    public string? ColorSenas { get; set; }
    public EstadoReproductivo EstadoReproductivo { get; set; } = EstadoReproductivo.Entero;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
    public bool IsDeleted { get; set; } = false;

    public Propietario? Propietario { get; set; }
    public ICollection<AtencionClinica> AtencionesClinicas { get; set; } = new List<AtencionClinica>();
    public ICollection<Inmunizacion> Inmunizaciones { get; set; } = new List<Inmunizacion>();

    public string EdadFormateada => CalcularEdadFormateada();

    public string CalcularEdadFormateada(DateTime? fechaReferencia = null)
    {
        var hasta = (fechaReferencia ?? DateTime.Today).Date;
        var desde = FechaNacimiento.Date;

        if (hasta < desde)
        {
            return "0 días";
        }

        var totalDias = (hasta - desde).Days;
        if (totalDias < 30)
        {
            return totalDias == 1 ? "1 día" : $"{totalDias} días";
        }

        int anios = hasta.Year - desde.Year;
        int meses = hasta.Month - desde.Month;
        int dias = hasta.Day - desde.Day;

        if (dias < 0)
        {
            meses--;
            var mesAnterior = hasta.AddMonths(-1);
            dias += DateTime.DaysInMonth(mesAnterior.Year, mesAnterior.Month);
        }

        if (meses < 0)
        {
            anios--;
            meses += 12;
        }

        if (anios == 0)
        {
            string parteMeses = meses == 1 ? "1 mes" : $"{meses} meses";
            string parteDias = dias == 1 ? "1 día" : $"{dias} días";
            return dias > 0 ? $"{parteMeses}, {parteDias}" : parteMeses;
        }
        else
        {
            string parteAnios = anios == 1 ? "1 año" : $"{anios} años";
            string parteMeses = meses == 1 ? "1 mes" : $"{meses} meses";
            return meses > 0 ? $"{parteAnios}, {parteMeses}" : parteAnios;
        }
    }
}
