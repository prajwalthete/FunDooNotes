namespace ModelLayer.Models
{
    public class ResponseModel<T>
    {
        public int StatusCode { get; set; }
        public bool IsSuccess { get; set; } = true;
        public string Message { get; set; } = string.Empty;
        public string? Token { get; set; }
        public T? Data { get; set; }

    }
}