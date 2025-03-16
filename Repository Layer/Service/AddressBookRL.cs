using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Repository_Layer.Context;
using Repository_Layer.Entity;
using Repository_Layer.Interface;
using Model_Layer.Model;
using Model_Layer.DTO;
using Microsoft.EntityFrameworkCore;
using Repository_Layer.Hashing;

namespace Repository_Layer.Service
{
    public class AddressBookRL: IAddressBookRL
    {
        private readonly AddressBookDbContext addressBookDbContext;
        private readonly RedisCacheService redisCache;

        public AddressBookRL(AddressBookDbContext addressBookDbContext, RedisCacheService redisCache)
        {
            this.addressBookDbContext=  addressBookDbContext;
            this.redisCache= redisCache;
        }

        public void Register(UserModel userModel)
        {
            // Map UserModel to UserEntity before saving
            var userEntity = new UserEntity
            {
                Name = userModel.Name,
                Email = userModel.Email,
                PasswordHash = userModel.PasswordHash
            };

            addressBookDbContext.Users.Add(userEntity);
            addressBookDbContext.SaveChanges();
        }
        public UserModel? GetUserByEmail(string email)
        {
            var userEntity = addressBookDbContext.Users.FirstOrDefault(u => u.Email == email);
            if (userEntity == null) return null;

            return new UserModel
            {
                Id = userEntity.Id,
                Name = userEntity.Name,
                Email = userEntity.Email,
                PasswordHash = userEntity.PasswordHash
            };
        }

        public UserEntity? GetUserEntityByEmail(string email)
        {
            return addressBookDbContext.Users.FirstOrDefault(u => u.Email == email);
        }

        public void UpdateUser(UserModel userModel)
        {
            var userEntity = addressBookDbContext.Users.FirstOrDefault(u => u.Id == userModel.Id);
            if (userEntity == null) return;

            userEntity.PasswordHash = userModel.PasswordHash;
            userEntity.ResetToken = userModel.ResetToken;
            userEntity.ResetTokenExpires = userModel.ResetTokenExpires;

            addressBookDbContext.Users.Update(userEntity);
            addressBookDbContext.SaveChanges();
        }

        public UserModel? GetUserByToken(string token)
        {
            var userEntity = addressBookDbContext.Users.FirstOrDefault(u => u.ResetToken == token);
            if (userEntity == null) return null;

            return new UserModel
            {
                Id = userEntity.Id,
                Name = userEntity.Name,
                Email = userEntity.Email,
                PasswordHash = userEntity.PasswordHash,
                ResetToken = userEntity.ResetToken,
                ResetTokenExpires = userEntity.ResetTokenExpires
            };
        }

        // Get all contacts
        public List<AddressBookEntity> GetAllContacts()
        {
            string cacheKey = "addressBook:allContacts";

            // ✅ Check if data is in Redis cache
            var cachedContacts = redisCache.Get<List<AddressBookEntity>>(cacheKey);
            if (cachedContacts != null)
                return cachedContacts;

            // ✅ Fetch from database and store in Redis
            var contacts = addressBookDbContext.AddressBook.ToList();
            redisCache.Set(cacheKey, contacts);

            return contacts;
        }

        // Get contact by ID
        public AddressBookEntity? GetContactById(int id)
        {
            string cacheKey = $"addressBook:contact:{id}";

            // ✅ Check if contact is cached
            var cachedContact = redisCache.Get<AddressBookEntity>(cacheKey);
            if (cachedContact != null)
                return cachedContact;

            // ✅ Fetch from database and store in Redis
            var contact = addressBookDbContext.AddressBook.FirstOrDefault(c => c.Id == id);
            if (contact != null)
            {
                redisCache.Set(cacheKey, contact);
            }

            return contact;
        }


        // Add a new contact
        public AddressBookEntity AddContact(AddressBookEntity contact)
        {
            addressBookDbContext.AddressBook.Add(contact);
            addressBookDbContext.SaveChanges();

            // ❌ Remove outdated cache after adding new contact
            redisCache.Remove("addressBook:allContacts");

            return contact;
        }

        // Update a contact
        public AddressBookEntity UpdateContact(int id, AddressBookEntity updatedContact)
        {
            var existingContact = addressBookDbContext.AddressBook.FirstOrDefault(c => c.Id == id);
            if (existingContact != null)
            {

                existingContact.Name = updatedContact.Name;
                existingContact.Email = updatedContact.Email;
                existingContact.Address = updatedContact.Address;

                addressBookDbContext.SaveChanges();
            }
            return updatedContact;
        }


        // Delete a contact
        public bool DeleteContact(int id)
        {
            var contact = addressBookDbContext.AddressBook.FirstOrDefault(c => c.Id == id);
            if (contact != null)
            {
                addressBookDbContext.AddressBook.Remove(contact);
                addressBookDbContext.SaveChanges();
                return true;
            }
            return false;
        }


    }
}
