using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Repository_Layer.Entity;

namespace Repository_Layer.Context
{
    public class AddressBookDbContext : DbContext
    {
        

        public AddressBookDbContext(DbContextOptions<AddressBookDbContext> options) : base(options) { }

        public virtual DbSet<AddressBookEntity> AddressBook { get; set; }
        public virtual DbSet<UserEntity>Users { get; set; }
    }
}
