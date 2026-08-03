using System;

namespace SGC.AntonioAnte.Application.Common
{
    // Clase base para respuestas sin valor de retorno (ej. Comandos)
    public class Result
    {
        public bool IsSuccess { get; init; }
        public string Error { get; init; }
        public bool IsFailure => !IsSuccess;

        protected Result(bool isSuccess, string error)
        {
            if (isSuccess && error != string.Empty)
                throw new InvalidOperationException("Un resultado exitoso no puede contener un mensaje de error.");
            if (!isSuccess && error == string.Empty)
                throw new InvalidOperationException("Un resultado fallido debe contener un mensaje de error.");

            IsSuccess = isSuccess;
            Error = error;
        }

        public static Result Success() => new(true, string.Empty);
        public static Result Failure(string error) => new(false, error);
    }

    // Clase genérica para respuestas con valor de retorno (ej. Queries)
    public class Result<TValue> : Result
    {
        private readonly TValue? _value;

        public TValue Value => IsSuccess
            ? _value!
            : throw new InvalidOperationException("No se puede acceder al valor de un resultado fallido.");

        protected internal Result(TValue? value, bool isSuccess, string error)
            : base(isSuccess, error)
        {
            _value = value;
        }

        public static Result<TValue> Success(TValue value, string mensaje = "") => new(value, true, string.Empty);
        public static new Result<TValue> Failure(string error) => new(default, false, error);
    }
}