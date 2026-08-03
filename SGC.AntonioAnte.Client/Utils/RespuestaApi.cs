namespace SGC.AntonioAnte.Client.Utils
{
    public class RespuestaApi<T>
    {
        public T? Data { get; set; }
        public string Mensaje { get; set; } = string.Empty;
    }
}
