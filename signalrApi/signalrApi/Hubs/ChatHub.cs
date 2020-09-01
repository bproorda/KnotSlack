﻿using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace signalrApi.Hubs
{
    public class ChatHub : Hub
    {
        private List<string> Users { get; set; }

        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }

        public async Task AddUser(string user)
        {
            Users.Add(user);

            if (Users == null)
            {
                Users = new List<string>();
            }

            await Clients.All.SendAsync("ShowUsers", Users.ToArray());
        }

        public async Task RemoveUser(string user)
        {
            Users.Remove(Users.Find(x => x == user));

            await Clients.All.SendAsync("ShowUsers", Users.ToArray());
        }
    }
}
