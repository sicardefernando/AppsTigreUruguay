namespace AppsTigreUruguay.Areas.MapeoSobrestock.Models
{
    public class Pasillo
    {
        public string nombrePasillo { get; set; } = string.Empty;
        public List<List<int>> distribucion { get; set; } = new List<List<int>>();
        public List<object> nomColumnas { get; set; } = new List<object>();
        public int cantNiveles { get; set; }
    }
}
