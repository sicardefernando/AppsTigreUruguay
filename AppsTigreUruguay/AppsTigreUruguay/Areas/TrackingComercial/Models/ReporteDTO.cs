namespace AppsTigreUruguay.Areas.TrackingComercial.Models
{
    public class ReporteDTO
    {
        public string Producto { get; set; }
        public decimal VentasAnioAnterior { get; set; }
        public decimal VentasAnioActual { get; set; }
        public decimal Porcentaje { get; set; }
        public string Color { get; set; }
    }
}
