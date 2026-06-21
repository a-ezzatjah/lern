using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ServiceContract.DTO.DtoCommit
{
    public class DtoResponse<T>
    {
        public bool Succeeded { get; set; }

        public string? Errormessage { get; set; }

        public List<string>? Errors { get; set; }

        public T? Data { get; set; }


        public static DtoResponse<T> Success(T data) => new DtoResponse<T> { Data = data, Succeeded = true };
        public static DtoResponse<T> Success() => new DtoResponse<T> { Succeeded = true };
        public static DtoResponse<T> Fail(string error) => new DtoResponse<T> { Errormessage = error, Succeeded = false };
        public static DtoResponse<T> Fail(List<string> error) => new DtoResponse<T> { Errors = error, Succeeded = false };

    }

}
