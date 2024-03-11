namespace ModelLayer.Models
{
    public class ResponseModel<T>
    {
        public int StatusCode { get; set; }
        public bool IsSuccess { get; set; } = true;
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }

    }
}