using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model_Layer.Model;
using Repository_Layer.Entity;

namespace Repository_Layer.Interface
{
    public interface IAddressBookRL
    {
        public void Register(UserModel userModel);
        public UserEntity? GetUserEntityByEmail(string email);
        public UserModel? GetUserByEmail(string email);
        public void UpdateUser(UserModel userModel);
        public UserModel? GetUserByToken(string token);
        public List<AddressBookEntity> GetAllContacts();
        public AddressBookEntity GetContactById(int id);
        public AddressBookEntity AddContact(AddressBookEntity contact);
        public AddressBookEntity UpdateContact(int id, AddressBookEntity updatedContact);
        public bool DeleteContact(int id);

    }
}
