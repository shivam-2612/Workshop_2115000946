using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model_Layer.DTO;
using Repository_Layer.Entity;

namespace Business_Layer.Interface
{
    public interface IAddressBookBL
    {
        public string Register(UserDTO userDto);
        public string? Login(UserDTO userDto);
        public string ForgotPassword(string email);
        public string ResetPassword(string token, string newPassword);
        public void SendResetEmail(string email, string resetToken);
        public List<AddressBookEntity> GetAllContacts();
        public AddressBookEntity GetContactById(int id);
        public AddressBookEntity AddContact(AddressBookEntity contact);
        public AddressBookEntity UpdateContact(int id, AddressBookEntity contact);
        public bool DeleteContact(int id);

    }
}
