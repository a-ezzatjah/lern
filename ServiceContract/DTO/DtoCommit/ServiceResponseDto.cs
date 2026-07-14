using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ServiceContract.DTO.DtoCommit
{
    public class ServiceResponseDto<T>
    {
        public bool Succeeded { get; set; }

        public string? Errormessage { get; set; }

        public List<string>? Errors { get; set; }

        public T? Data { get; set; }


        public static ServiceResponseDto<T> Success(T data) => new ServiceResponseDto<T> { Data = data, Succeeded = true };
        public static ServiceResponseDto<T> Success() => new ServiceResponseDto<T> { Succeeded = true };
        public static ServiceResponseDto<T> Fail(string error) => new ServiceResponseDto<T> { Errormessage = error, Succeeded = false };
        public static ServiceResponseDto<T> Fail(List<string> error) => new ServiceResponseDto<T> { Errors = error, Succeeded = false };

    }

}
