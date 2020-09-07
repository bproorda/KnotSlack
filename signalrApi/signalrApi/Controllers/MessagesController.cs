﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using signalrApi.Models;
using signalrApi.Models.DTO;
using signalrApi.Repositories.MessageRepos;
using signalrApi.services;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace signalrApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessagesController : ControllerBase
    {
        private IMessageRepository messageRepository;
        private readonly IUserManager userManager;

        public MessagesController(IMessageRepository messageRepository, IUserManager userManager)
        {
            this.messageRepository = messageRepository;
            this.userManager = userManager;
        }
        
        [HttpPost("msgsender")]
        public async Task<IEnumerable<Message>> GetMessagesSender(string sender)
        {
            var messages = await messageRepository.GetMessagesBySender(sender);
            return messages;
        }


        [HttpPost("msgrec")]
        public async Task<IEnumerable<Message>> GetMessagesRecipient(string recipient)
        {
            var messages = await messageRepository.GetMessagesByRecipient(recipient);
            return messages;
        }

        [HttpPost("new")]
        public async Task<bool> CreateNewMessage(messageCreateDTO msg)
        {
            var user = await userManager.FindByNameAsync(msg.Sender);

            var message = new Message
            {
                Sender = msg.Sender,
                Recipient = msg.Recipient,
                Contents = msg.Contents,
                UserId = user.Id,
                Date = DateTime.Now,
            };

            await messageRepository.CreateNewMessage(message);
            return false;
        }

        // DELETE api/<MessageController>/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<bool>> Delete(int id)
        {
           var message =  await messageRepository.DeleteMessage(id);

            if(message == null)
            {
                return NotFound();
            }

            return true;
        }
    }
}
