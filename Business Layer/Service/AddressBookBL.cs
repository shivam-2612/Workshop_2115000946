using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Business_Layer.Interface;
using Repository_Layer.Entity;
using Repository_Layer.Service;
using Repository_Layer.Interface;
using Model_Layer.Model;
using Model_Layer.DTO;
using Repository_Layer.Hashing;
using System.Security.Cryptography;
using System.Net.Mail;
using System.Net;


namespace Business_Layer.Service
{
    public class AddressBookBL:IAddressBookBL
    {
        private readonly IAddressBookRL _addressBookRL;
        private readonly JwtService jwtService;

        public AddressBookBL(IAddressBookRL addressBookRL, JwtService jwtService)
        {
            _addressBookRL = addressBookRL;
            this.jwtService = jwtService;
        }

        public string Register(UserDTO userDto)
        {
            if (_addressBookRL.GetUserByEmail(userDto.Email) != null)
                return "User already exists";

            var user = new UserModel
            {
                Name = userDto.Name,
                Email = userDto.Email,
                PasswordHash = PasswordHashing.HashPassword(userDto.Password)
            };

            _addressBookRL.Register(user);
            return "User registered successfully";
        }

        public string? Login(UserDTO userDto)
        {
            var userEntity = _addressBookRL.GetUserEntityByEmail(userDto.Email);
            if (userEntity == null || !PasswordHashing.VerifyPassword(userDto.Password, userEntity.PasswordHash))
                return null; // Invalid login

            return jwtService.GenerateToken(userEntity);
        }

        public string ForgotPassword(string email)
        {
            var user = _addressBookRL.GetUserByEmail(email);
            if (user == null)
                return "User not found";

            // Generate a secure reset token
            user.ResetToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
            user.ResetTokenExpires = DateTime.UtcNow.AddHours(1); // Token valid for 1 hour

            _addressBookRL.UpdateUser(user);

            SendResetEmail(user.Email, user.ResetToken);

            return "Password reset email sent successfully";
        }

        public string ResetPassword(string token, string newPassword)
        {
            var user = _addressBookRL.GetUserByToken(token); // Find user by token

            if (user == null || user.ResetTokenExpires < DateTime.UtcNow)
                return "Invalid or expired token";

            // Update the user's password
            user.PasswordHash = PasswordHashing.HashPassword(newPassword);
            user.ResetToken = null; // Clear the token after successful reset
            user.ResetTokenExpires = null;

            _addressBookRL.UpdateUser(user);
            return "Password reset successfully";
        }

        public void SendResetEmail(string email, string resetToken)
        {
            string resetLink = $"https://yourdomain.com/reset-password?token={resetToken}";
            string subject = "Password Reset Request";
            string body = $"Click the link below to reset your password:\n{resetLink}";

            using (SmtpClient client = new SmtpClient("smtp.gmail.com"))
            {
                client.Port = 587;
                client.Credentials = new NetworkCredential("goyal2003shivam@gmail.com", "vrpf geab xhsa tozx");
                client.EnableSsl = true;

                MailMessage mail = new MailMessage();
                mail.From = new MailAddress("goyal2003shivam@gmail.com");
                mail.To.Add(email);
                mail.Subject = subject;
                mail.Body = body;

                client.Send(mail);
            }
        }


        public List<AddressBookEntity> GetAllContacts()
        {
            return _addressBookRL.GetAllContacts();
        }

        public AddressBookEntity GetContactById(int id)
        {
            return _addressBookRL.GetContactById(id);
        }

        public AddressBookEntity AddContact(AddressBookEntity contact)
        {
            if (string.IsNullOrEmpty(contact.Name) || string.IsNullOrEmpty(contact.Address))
            {
                return null; // Validation failed
            }
            return _addressBookRL.AddContact(contact);
        }

        public AddressBookEntity UpdateContact(int id, AddressBookEntity contact)
        {
            return _addressBookRL.UpdateContact(id, contact);
        }

        public bool DeleteContact(int id)
        {
            return _addressBookRL.DeleteContact(id);
        }

       
    }
}
