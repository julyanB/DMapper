namespace DMapper.Exceptions
{
    public class DMapperBaseException : Exception
    {
        public DMapperBaseException()
            : base("An error occurred in the DMapper library.")
        {
        }
        public DMapperBaseException(string message)
            : base(message)
        {
        }
        public DMapperBaseException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
        public DMapperBaseException(string? paramName, string message)
            : base($"Error in parameter '{paramName}': {message}")
        {
            ParamName = paramName;
        }
        public string? ParamName { get; }
    }
}
