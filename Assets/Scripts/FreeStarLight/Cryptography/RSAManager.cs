using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using JavaScience;

namespace Extra
{
    public class RSAManager
    {
        public RSACryptoServiceProvider Provider { get; private set; }
        public RSAParameters PublicKey { get; private set; }
        public RSAParameters? PrivateKey { get; private set; }

        public RSAManager(int keySize = 2048)
        {
            Provider = new RSACryptoServiceProvider(keySize);

            PublicKey = Provider.ExportParameters(false);
            PrivateKey = Provider.ExportParameters(true);
        }

        public RSAManager(RSACryptoServiceProvider provider)
        {
            Provider = provider;

            PublicKey = Provider.ExportParameters(false);
            
            if (!Provider.PublicOnly)
            {
                PrivateKey = Provider.ExportParameters(true);
            }
            else
            {
                PrivateKey = null;
            }
        }

        public static RSAManager FromPrivateKey(RSAParameters privateKey)
        {
            var csp = new RSACryptoServiceProvider();
            csp.ImportParameters(privateKey);

            return new RSAManager(csp);
        }

        public static RSAManager FromBase64XmlPrivateKey(string privateKey)
        {
            return FromPrivateKey(PrivateKeyFromBase64XmlString(privateKey));
        }

        public static RSAManager FromPublicKey(RSAParameters publicKey)
        {
            var csp = new RSACryptoServiceProvider();
            csp.ImportParameters(publicKey);

            return new RSAManager(csp);
        }

        public static RSAManager FromBase64XmlPublicKey(string publicKey)
        {
            return FromPublicKey(PublicKeyFromBase64XmlString(publicKey));
        }

        public static RSAManager FromPEM(string pemString)
        {
            return new RSAManager(opensslkey.DecodePEMKey(pemString));
        }

        public string ToPrivatePEM()
        {
            return opensslkey.ExportPrivateKey(Provider);
        }

        public string ToPublicPEM()
        {
            return opensslkey.ExportPublicKey(Provider);
        }

        public string PublicKeyToString()
        {
            //we need some buffer
            var sw = new System.IO.StringWriter();
            //we need a serializer
            var xs = new System.Xml.Serialization.XmlSerializer(typeof(RSAParameters));
            //serialize the key into the stream
            xs.Serialize(sw, PublicKey);

            string result = Convert.ToBase64String(Encoding.UTF8.GetBytes(sw.ToString()));
            return result;
        }

        public string PrivateKeyToString()
        {
            if (PrivateKey.HasValue)
            {
                //we need some buffer
                var sw = new System.IO.StringWriter();
                //we need a serializer
                var xs = new System.Xml.Serialization.XmlSerializer(typeof(RSAParameters));
                //serialize the key into the stream
                xs.Serialize(sw, PrivateKey.Value);

                string result = Convert.ToBase64String(Encoding.UTF8.GetBytes(sw.ToString()));
                return result; //sw.ToString();
            }
            else
            {
                return String.Empty;
            }
        }

        public static RSAParameters PublicKeyFromBase64XmlString(string publicKey)
        {
            string xml = Encoding.UTF8.GetString(Convert.FromBase64String(publicKey));
            //get a stream from the string
            var sr = new System.IO.StringReader(xml);
            //we need a deserializer
            var xs = new System.Xml.Serialization.XmlSerializer(typeof(RSAParameters));
            //get the object back from the stream

            return (RSAParameters)xs.Deserialize(sr);
        }

        public static RSAParameters PrivateKeyFromBase64XmlString(string privateKey)
        {
            string xml = Encoding.UTF8.GetString(Convert.FromBase64String(privateKey));
            //get a stream from the string
            var sr = new System.IO.StringReader(xml);
            //we need a deserializer
            var xs = new System.Xml.Serialization.XmlSerializer(typeof(RSAParameters));
            //get the object back from the stream

            return (RSAParameters)xs.Deserialize(sr);
        }

        public string PrivateEncrypt(string text)
        {
            if (PrivateKey.HasValue)
            {
                var csp = new RSACryptoServiceProvider();
                csp.ImportParameters(PrivateKey.Value);

                var bytesPlainTextData = System.Text.Encoding.UTF8.GetBytes(text);
                var bytesCypherText = csp.Encrypt(bytesPlainTextData, false);
                var cypherText = Convert.ToBase64String(bytesCypherText);

                return cypherText;
            }
            else
            {
                return String.Empty;
            }
        }

        public string PublicEncrypt(string text)
        {
            var csp = new RSACryptoServiceProvider();
            csp.ImportParameters(PublicKey);

            var bytesPlainTextData = System.Text.Encoding.UTF8.GetBytes(text);
            var bytesCypherText = csp.Encrypt(bytesPlainTextData, false);
            var cypherText = Convert.ToBase64String(bytesCypherText);

            return cypherText;
        }

        public string PrivateDecrypt(string text)
        {
            if (PrivateKey.HasValue)
            {
                var csp = new RSACryptoServiceProvider();
                csp.ImportParameters(PrivateKey.Value);

                var bytesCypherText = Convert.FromBase64String(text);
                var bytesPlainTextData = csp.Decrypt(bytesCypherText, false);
                var plainTextData = Encoding.UTF8.GetString(bytesPlainTextData);

                return plainTextData;
            }
            else
            {
                return String.Empty;
            }
        }

        public string PublicDecrypt(string text)
        {
            var csp = new RSACryptoServiceProvider();
            csp.ImportParameters(PublicKey);

            var bytesCypherText = Convert.FromBase64String(text);
            var bytesPlainTextData = csp.Decrypt(bytesCypherText, false);
            var plainTextData = Encoding.UTF8.GetString(bytesPlainTextData);

            return plainTextData;
        }

        public string Sign(string message)
        {
            byte[] signature = Provider.SignData(
                Encoding.UTF8.GetBytes(message), 
                CryptoConfig.MapNameToOID("SHA512"));

            return Convert.ToBase64String(signature);
        }

        public bool VerifySignature(string signature, string message)
        {
            using (var sha512 = SHA512.Create())
            {
                return Provider.VerifyHash(
                    sha512.ComputeHash(Encoding.UTF8.GetBytes(message)),
                    CryptoConfig.MapNameToOID("SHA512"),
                    Convert.FromBase64String(signature)
                );
            }
        }
    }
}
