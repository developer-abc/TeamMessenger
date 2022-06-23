using System;
using System.Security.Claims;
using System.Threading.Tasks;
using TeamMessenger.Server.Data;
using TeamMessenger.Shared.Messages;
using Microsoft.AspNetCore.SignalR;

namespace TeamMessenger.Server.Hubs
{
    public class ChatHub : Hub
    {
        private readonly MessagesContext _dbContext;
        public ChatHub(MessagesContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task Broadcast(PackedMessage packedMessage)
        {
            if (!(bool)(Context.Items["IsLoggedIn"] ?? false)
                || !Context.Items.ContainsKey("Username")
                || !Context.Items.ContainsKey("PublicKey")
                || !Context.Items.ContainsKey("GroupId")
                || !Context.Items.ContainsKey("LastSignature")
                || Context.Items["Username"] == null
                || Context.Items["PublicKey"] == null
                || Context.Items["GroupId"] == null
                || Context.Items["LastSignature"] == null
                )
            {
                Console.WriteLine($"{Context.ConnectionId} sent message before login");
                Context.Abort();
                return;
            }
            Console.WriteLine($"{Context.ConnectionId} sent message");
            string Username = (string?)Context.Items["Username"] ?? string.Empty;
            byte[] PublicKey = (byte[]?)Context.Items["PublicKey"] ?? new byte[0];
            byte[] GroupId = (byte[]?)Context.Items["GroupId"] ?? new byte[0];
            byte[] LastSignature = (byte[]?)Context.Items["LastSignature"] ?? new byte[0];
            Message message = new Message(Context.ConnectionId, Username, PublicKey, GroupId, LastSignature);
            message.EncryptedMessage = packedMessage.EncryptedMessage;
            message.Signature = packedMessage.Signature;
            message.GroupId = GroupId;
            if (!message.Verify())
            {
                Console.WriteLine($"{Context.ConnectionId} wrong message signature");
                Context.Abort();
                return;
            }
            Context.Items["LastSignature"] = message.Signature;
            await Clients.Group(BitConverter.ToString(GroupId)).SendAsync("Broadcast", message);
            Console.WriteLine($"Verified and broadcasted message from {Context.ConnectionId}");
            await _dbContext.AddAsync(message);
            await _dbContext.SaveChangesAsync();
            Console.WriteLine($"Saved message from {Context.ConnectionId}");
        }

        public async Task Login(string username, byte[] publicKey, byte[] groupId, byte[] signature)
        {
            if (Context.Items.ContainsKey("IsLoggedIn")
                &&
                (bool)(Context.Items["IsLoggedIn"] ?? false))
            {
                Console.WriteLine($"{Context.ConnectionId} repeated login");
                Context.Abort();
                return;
            }
            if (!TeamMessenger.Shared.Cryptography.Signature.VerifyData(System.Text.UnicodeEncoding.Unicode.GetBytes(Context.ConnectionId + username).Concat(groupId).ToArray(), signature, publicKey))
            {
                Console.WriteLine($"{Context.ConnectionId} wrong login signature");
                Context.Abort();
                return;
            }
            Context.Items["IsLoggedIn"] = true;
            Context.Items.Add("Username", username);
            Context.Items.Add("PublicKey", publicKey);
            Context.Items.Add("GroupId", groupId);
            await Groups.AddToGroupAsync(Context.ConnectionId, BitConverter.ToString(groupId));
            Context.Items.Add("LastSignature", signature);
            Console.WriteLine($"{Context.ConnectionId} logged in as {BitConverter.ToString(publicKey)}");
            int i = 0;
            foreach (Message msg in _dbContext.Messages.Where(msg => msg.GroupId == groupId))
            {
                await Clients.Caller.SendAsync("Broadcast", msg);
                i++;
            }
            Console.WriteLine($"{Context.ConnectionId} recovered {i} messages");
        }

        public override async Task OnConnectedAsync()
        {
            Console.WriteLine($"{Context.ConnectionId} connected");
            Context.Items.Add("IsLoggedIn", false);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? e)
        {
            Console.WriteLine($"Disconnected {e?.Message} {Context.ConnectionId}");
            await base.OnDisconnectedAsync(e);
        }
    }
}
