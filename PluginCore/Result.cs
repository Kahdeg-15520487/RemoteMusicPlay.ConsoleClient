using System;
using System.Linq;
using System.Net;

namespace PluginCore.Core
{
    public struct Result
    {
        public ResultCode Code { get; private set; }
        public string Message { get; private set; }

        public Result(ResultCode code, string message)
        {
            Code = code;
            Message = message;
        }

        public static Result None = new Result(ResultCode.None, "");
        public static Result Success(string message = "")
        {
            return new Result(ResultCode.Success, message);
        }

        public static Result InvalidArgument = new Result(ResultCode.InvalidArgument, "");
        public static Result Error(string message = "")
        {
            return new Result(ResultCode.Error, message);
        }

        public static Result Error(HttpStatusCode statusCode)
        {
            return new Result(ResultCode.Error, statusCode.ToString());
        }

        public static Result Error(Exception ex)
        {
            switch (ex)
            {
                case AggregateException aex:
                    return Error(string.Join(Environment.NewLine, aex.InnerExceptions.Select(e => $"{e.GetType().Name} : {e.Message}")));
                default:
                    return Error($"{ex.GetType().Name} : {ex.Message}");
            }
        }

        public enum ResultCode
        {
            None,
            Success,
            InvalidArgument,
            Error
        }

        public override string ToString()
        {
            return $"{Code} :\n{(string.IsNullOrEmpty(Message) ? "empty" : Message)}";
        }
    }
}
