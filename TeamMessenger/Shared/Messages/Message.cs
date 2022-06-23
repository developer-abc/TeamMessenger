using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace TeamMessenger.Shared.Messages
{
    public class Message
    {
        /// <summary>
        /// ID соединения
        /// </summary>
        public string ConnectionId { get; set; }
        /// <summary>
        /// Имя пользователя
        /// </summary>
        public string Username { get; set; }
        /// <summary>
        /// Публичный ключ пользователя
        /// </summary>
        public byte[] PublicKey { get; set; }
        /// <summary>
        /// Идентификатор группы
        /// </summary>
        public byte[] GroupId { get; set; }
        /// <summary>
        /// Заширофванное сообщение
        /// </summary>
        public byte[]? EncryptedMessage { get; set; }
        /// <summary>
        /// Подпись сообщения
        /// </summary>
        public byte[]? Signature { get; set; }
        /// <summary>
        /// Подпись предыдущего сообщения
        /// </summary>
        public byte[] LastSignature { get; set; }

        /// <summary>
        /// Конструктор для десериализации
        /// </summary>
        public Message() { }
        public Message(string connectionId, string username, byte[] publicKey, byte[] groupId, byte[] lastSignature)
        {
            this.ConnectionId = connectionId;
            this.Username = username;
            this.PublicKey = publicKey;
            this.GroupId = groupId;
            this.LastSignature = lastSignature;
        }

        /// <summary>
        /// Шифрование сообщения ключом группы и подпись личным ключом
        /// </summary>
        /// <param name="message">Сообщение</param>
        /// <param name="personalSecretKey">Личный ключ</param>
        /// <param name="groupSecretKey">Ключ группы</param>
        public void EncryptAndSign(string message, byte[] personalSecretKey, byte[] groupSecretKey)
        {
            byte[] lastSignatureHash;
            using (SHA256 sha = SHA256.Create())
                lastSignatureHash = sha.ComputeHash(LastSignature);
            // Уменьшение до размера IV для Rijndael
            lastSignatureHash = (new Span<byte>(lastSignatureHash, 0, 16)).ToArray();
            byte[] messageBytes = System.Text.UnicodeEncoding.Unicode.GetBytes(message);
            EncryptedMessage = Cryptography.Symmetric.Encrypt(messageBytes, groupSecretKey, lastSignatureHash);
            Signature = Cryptography.Signature.SignData(LastSignature
                                                                    .Concat(System.Text.UnicodeEncoding.Unicode.GetBytes(ConnectionId))
                                                                    .Concat(EncryptedMessage).ToArray(),
                                                                    personalSecretKey);
        }

        /// <summary>
        /// Проверка подписи сообщения
        /// </summary>
        public bool Verify()
        {
            if (EncryptedMessage == null || Signature == null)
                return false;
            return Cryptography.Signature.VerifyData(LastSignature
                                                        .Concat(System.Text.UnicodeEncoding.Unicode.GetBytes(ConnectionId))
                                                        .Concat(EncryptedMessage).ToArray(),
                                                        Signature,
                                                        PublicKey);
        }

        /// <summary>
        /// Упаковка сообщения для отправки
        /// </summary>
        /// <returns>null при неудаче</returns>
        public PackedMessage? Pack()
        {
            if (EncryptedMessage == null
                || Signature == null)
                return null;
            return new PackedMessage(this.EncryptedMessage, this.Signature);
        }

        /// <summary>
        /// Расшифровка сообщения и получение
        /// </summary>
        /// <returns>null при неудаче</returns>
        public string? DecryptMessage(byte[] groupSecretKey)
        {
            if (EncryptedMessage == null)
                return null;
            byte[] lastSignatureHash;
            using (SHA256 sha = SHA256.Create())
                lastSignatureHash = sha.ComputeHash(LastSignature);
            // Уменьшение до размера IV для Rijndael
            lastSignatureHash = (new Span<byte>(lastSignatureHash, 0, 16)).ToArray();
            try
            {
                return System.Text.UnicodeEncoding.Unicode.GetString(Cryptography.Symmetric.Decrypt(EncryptedMessage, groupSecretKey, lastSignatureHash));
            } catch(Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }
    }
}
