using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Model_Layer.DTO;
using Model_Layer.Model;
using Repository_Layer.Entity;

namespace Business_Layer.Service
{
   

    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<AddressBookEntry, AddressBookEntity>().ReverseMap();

            CreateMap<AddressBookEntry, AddressBookDTO>().ReverseMap();
        }
    }

}
