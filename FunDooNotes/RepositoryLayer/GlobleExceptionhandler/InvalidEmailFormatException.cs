namespace RepositoryLayer.GlobleExeptionhandler
{
    public class InvalidEmailFormatException : Exception
    {
        public InvalidEmailFormatException(string message) : base(message) { }
    }
}
