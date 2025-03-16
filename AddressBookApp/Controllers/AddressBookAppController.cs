using AutoMapper;
using Business_Layer.Interface;
using Business_Layer.Service;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Model_Layer.DTO;
using Model_Layer.Model;
using Repository_Layer.Entity;

namespace AddressBookApp.Controllers
{
    [ApiController]
    [Route("api/addressbook")]
    public class AddressBookAppController : ControllerBase
    {
        private readonly IAddressBookBL _addressBookBL;
        private readonly IMapper _mapper;
        private readonly IValidator<AddressBookDTO> _validator;
        private readonly RabbitMQService rabbitMQService;

        public AddressBookAppController(IAddressBookBL addressBookBL, IMapper mapper, IValidator<AddressBookDTO> validator, RabbitMQService rabbitMQService)
        {
            _addressBookBL = addressBookBL;
            _mapper = mapper;
            _validator = validator;
            this.rabbitMQService = rabbitMQService;
        }

        // GET: Fetch all contacts
        /// <summary>
        /// This api is used to fetch all the contacts in the address book
        /// </summary>
        /// <returns>Details of all contacts in the database<returns>
        [HttpGet]
        public ActionResult<List<AddressBookEntity>> GetAllContacts()
        {
            return Ok(_addressBookBL.GetAllContacts());
        }

        // GET: Get contact by ID
        /// <summary>
        /// This api is used to return the contact details based on what id we provide
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Contact details</returns>
        [HttpGet("{id}")]
        public ActionResult<AddressBookEntity> GetContactById(int id)
        {
            var contact = _addressBookBL.GetContactById(id);
            if (contact == null)
            {
                return NotFound("Contact not found.");
            }
            return Ok(contact);
        }

        // POST: Add a new contact
        /// <summary>
        /// This api is used to add a new contact in address book
        /// </summary>
        /// <param name="contactDTO"></param>
        /// <returns>Contact details of added user</returns>
        [HttpPost]
        public ActionResult<AddressBookDTO> AddContact([FromBody] AddressBookDTO contactDTO)
        {
            var validationResult = _validator.Validate(contactDTO);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }
            
            var contactEntity = _mapper.Map<AddressBookEntity>(contactDTO);
            var createdContact = _addressBookBL.AddContact(contactEntity);
            var createdContactDTO = _mapper.Map<AddressBookDTO>(createdContact);
            // ? Publish event to RabbitMQ
            rabbitMQService.PublishMessage("contact.added", createdContact);

            return CreatedAtAction(nameof(GetContactById), new { id = createdContactDTO.Id }, createdContactDTO);
        }

        // PUT: Update contact
        /// <summary>
        /// This api updates the existing user based on what id we provide
        /// </summary>
        /// <param name="id"></param>
        /// <param name="contact"></param>
        /// <returns>Updated contact details</returns>
        [HttpPut("{id}")]
        public ActionResult<AddressBookEntity> UpdateContact(int id, [FromBody] AddressBookEntity contact)
        {
            var updatedContact = _addressBookBL.UpdateContact(id, contact);
            if (updatedContact == null)
            {
                return NotFound("Contact not found.");
            }
            return Ok(updatedContact);
        }

        // DELETE: Delete contact
        /// <summary>
        /// This api deletes the contact whose id is provided
        /// </summary>
        /// <param name="id"></param>
        /// <returns>True or false</returns>
        [HttpDelete("{id}")]
        public ActionResult DeleteContact(int id)
        {
            bool isDeleted = _addressBookBL.DeleteContact(id);
            if (!isDeleted)
            {
                return NotFound("Contact not found.");
            }
            return NoContent();
        }
    }
}
