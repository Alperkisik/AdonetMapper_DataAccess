namespace AdonetMapper_DataAccess.Utility
{
    public class Result
    {
        internal Result(bool isSuccess, string error)
        {
            if (!isSuccess && string.IsNullOrEmpty(error)) throw new Exception("string errorMessage cannot be null or empty on Failure!");
            IsSuccess = isSuccess;
            Error = error;
        }

        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;
        public string Error { get; }

        public static Result Success() => new Result(true, "");
        public static Result Failure(string error) => new Result(false, error);
    }
}
