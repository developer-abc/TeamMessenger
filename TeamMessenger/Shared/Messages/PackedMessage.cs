using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamMessenger.Shared.Messages
{
    /// <summary>
    /// Сообщение, подготовленное для отправки
    /// </summary>
    public class PackedMessage
    {
        /// <summary>
        /// Зашифрованное сообщение
        /// </summary>
        public byte[] EncryptedMessage { get; set; }
        /// <summary>
        /// Подпись сообщения
        /// </summary>
        public byte[] Signature { get; set; }

        /// <summary>
        /// Конструктор для десериализации
        /// </summary>
        public PackedMessage() { }
        internal PackedMessage(byte[] encryptedMessage, byte[] signature)
        {
            this.EncryptedMessage = encryptedMessage;
            this.Signature = signature;
        }
    }
}
