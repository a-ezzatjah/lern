using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Schema;
using DTO;
using Entities;
using ServiceContract.Interfaces;

namespace Service
{
    public class BranchService : IBranchService
    {

        private readonly ShopDbContext _shopDbContext;
        public BranchService(ShopDbContext shopDbContext)
        {

            _shopDbContext = shopDbContext;
        }



        public List<DtoBranch> GetAll()
        {
        

           return _shopDbContext.branches.Select(x => new DtoBranch { Id = x.Id, Name = x.Name }).ToList();
        }

        //test

        //public List<DtoBranch> GetFilter(string SearchText)
        //{
        //    var query = _shopDbContext.branches.Select(x => new DtoBranch { Id = x.Id, Name = x.Name }).AsQueryable();

        //    if (!string.IsNullOrWhiteSpace(SearchText))
        //    {
        //        if (int.TryParse(SearchText, out var searchid))
        //        {
        //            query = query.Where(x => x.Id == searchid);
        //        }
        //        else
        //        {
        //            query = query.Where(x => x.Name.Contains(SearchText));
        //        }
        //    }

        //    return query.ToList();
        //}



        //public DtoResponse<DtoBranch>AddBranch(DtoBranch model)
        //{

        //    if (model==null)
        //    {
        //        return DtoResponse<DtoBranch>.Fail("شعبه موجود نمیباشد");
        //    }

        //    if (string.IsNullOrWhiteSpace(model.Name))
        //    {
        //        return DtoResponse<DtoBranch>.Fail("نام شعبه الزامی است");
        //    }

        //    var duplicate = _dtoBranches.Any(x => x.Name == model.Name);
        //    if (duplicate)
        //    {
        //        return DtoResponse<DtoBranch>.Fail("شعبه تکراری مباشد");
        //    }

        //    int newid;
        //    if (_dtoBranches.Any())
        //    {

        //         newid = _dtoBranches.Max(x => x.Id) + 1;
        //    }
        //    else
        //    {

        //        newid = 1;
        //    }


        //    model.Id = newid;

        //    _dtoBranches.Add(model);


        //    return DtoResponse<DtoBranch>.Success(model);




        //}



        //public DtoResponse<DtoBranch> Update(DtoBranch model)
        //{
        //    if (model == null)
        //    {
        //        return DtoResponse<DtoBranch>.Fail("شعبه مجود نمیباشد");
        //    }

        //    if (model.Id <= 0)
        //    {
        //        return DtoResponse<DtoBranch>.Fail("شناسه شعبه نامعتبر است");
        //    }

        //    var GetId = _dtoBranches.FirstOrDefault(x => x.Id == model.Id);
        //    if (GetId == null)
        //    {
        //        return DtoResponse<DtoBranch>.Fail("محصول مجود نمیباشد");
        //    }


        //    var DuplicateName = _dtoBranches.Any(x => x.Name == model.Name && x.Id != model.Id);
        //    if (DuplicateName)
        //    {
        //        return DtoResponse<DtoBranch>.Fail("نام یا شماره شعبه تکراری میباشد");
        //    }

        //    GetId.Name = model.Name;
        //    GetId.Id = model.Id;

        //    return DtoResponse<DtoBranch>.Success(GetId);
        //}



        //public DtoResponse<DtoBranch>Delete(int branchid)
        //{

        //    var branch = _dtoBranches.FirstOrDefault(x => x.Id == branchid);
        //    if (branch == null)
        //    {
        //        return DtoResponse<DtoBranch>.Fail("شعبه موجود نمیباشد");
        //    }

        //    _dtoBranches.Remove(branch);


        //    return DtoResponse<DtoBranch>.Success();

        //}


        //public bool Exists(int branchid)
        //{
        //    return _dtoBranches.Any(x => x.Id == branchid);
        //}


    }
}
