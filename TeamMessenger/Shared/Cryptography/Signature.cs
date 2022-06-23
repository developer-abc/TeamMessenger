using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamMessenger.Shared.Cryptography
{
    public static class Signature
    {
        public static byte[] SignData(byte[] Data, byte[] PrivateKey)
        {
            ISigner signer = SignerUtilities.GetSigner("SHA256withECDSA");
            signer.Init(true, PrivateKeyFactory.CreateKey(PrivateKey));
            signer.BlockUpdate(Data, 0, Data.Length);
            return signer.GenerateSignature();
        }

        public static bool VerifyData(byte[] Data, byte[] Signature, byte[] PublicKey)
        {
            ISigner signer = SignerUtilities.GetSigner("SHA256withECDSA");
            signer.Init(false, PublicKeyFactory.CreateKey(PublicKey));
            signer.BlockUpdate(Data, 0, Data.Length);
            return signer.VerifySignature(Signature);
        }
    }
}
