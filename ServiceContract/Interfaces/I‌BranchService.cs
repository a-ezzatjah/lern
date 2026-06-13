using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DTO;
using Entities;

namespace ServiceContract.Interfaces
{
    public interface I‌BranchService
    {

        List<DtoBranch> GetAll();

        // List<DtoBranch> GetFilter(string SearchText);

        // DtoResponse<DtoBranch> AddBranch(DtoBranch model);

        // DtoResponse<DtoBranch> Update(DtoBranch model);


        //DtoResponse<DtoBranch> Delete(int branchid);

        // bool Exists(int branchid);


        //public Product? GetEntityById(int product);
    }
}
