using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using OpenPGP;
using Org.BouncyCastle.Bcpg;
using Org.BouncyCastle.Bcpg.OpenPgp;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities.IO;


namespace OpenPGP
{
    public partial class UserControl1 : UserControl
    {
        public UserControl1()
        {
            InitializeComponent();

        }

        private void Label1_Click(object sender, EventArgs e)
        {

        }

        private void UserControl1_Load(object sender, EventArgs e)
        {

        }

        private void Button1_Click(object sender, EventArgs e)
        {
            string signature = textBox1.Text;
            textBox2.Text = @"C:\Users\MARK\Desktop\PGP\PGPKEY\priv.asc";
            textBox3.Text = @"C:\Users\MARK\Desktop\PGP\PGPKEY\pub.asc";

            //RSA密鑰產生器
            IAsymmetricCipherKeyPairGenerator kpg = GeneratorUtilities.GetKeyPairGenerator("RSA");
            //Key 構造使用參數        
            kpg.Init(new RsaKeyGenerationParameters(
                BigInteger.ValueOf(0x10001), new SecureRandom(),
                1024, // key 的長度
                25));
            AsymmetricCipherKeyPair kp = kpg.GenerateKeyPair();
            char[] password = signature.ToCharArray(); //私鑰的密碼
            Stream out1, out2;
            out1 = File.Create(textBox2.Text); //私鑰放置位置          
            out2 = File.Create(textBox3.Text); //公鑰放置位置
            ExportKeyPair(out1, out2, kp.Public,kp.Private, "MarkWu", password, true);
        }



        private void Button2_Click(object sender, EventArgs args)
        {
            ////Public 1 檔案加密
            //string outputFileName = @"C:/Users/MARK/Desktop/PGP/PGPKEY/Xabc.txt";
            //string inputFileName = @"C:/Users/MARK/Desktop/PGP/PGPKEY/abc.txt";
            //string encKeyFileName = @"C:/Users/MARK/Desktop/PGP/PGPKEY/pub.asc";
            //bool armor = true;
            //bool withIntegrityCheck = false;

            //Public 1 檔案加密
            string outputFileName = @"C:/Users/MARK/Desktop/PGP/PGPKEY/Xabc.txt";
            string inputFileName = textBox7.Text;
            string encKeyFileName = textBox8.Text;
            bool armor = true;
            bool withIntegrityCheck = false;


            try
            {
                EncryptFile(outputFileName, inputFileName,
                    encKeyFileName, armor, withIntegrityCheck);
                Console.WriteLine("加密成功");

            }
            catch (Exception e)
            {
                Console.WriteLine("加密失敗" + e.Message);
            }

            ////private1 檔案解密
            //string decryptEncryptFileName = @"D:/BC/b.txt";
            //string keyFileName = @"D:/BC/priv1.asc";
            //char[] passwd = "123456".ToCharArray();
            //string defaultFileName = @"D:/BC/c.txt";
            //try
            //{
            //    DecryptFile(decryptEncryptFileName, keyFileName,
            //        passwd, defaultFileName);
            //    Console.WriteLine("解密成功");
            //}
            //catch (Exception e)
            //{
            //    Console.WriteLine("解密失敗" + e.Message);
            //}

            textBox9.Text= outputFileName ;

            Console.Read();
        }


        private void Button3_Click(object sender, EventArgs args)
        {

            //private1 檔案解密
            string decryptEncryptFileName = textBox6.Text;
            string keyFileName = textBox5.Text;
            char[] passwd = "123456".ToCharArray();
            string defaultFileName = @"C:/Users/MARK/Desktop/PGP/PGPKEY/一卡通(加密+簽章-認證-解密).txt";

            try
            {
                DecryptFile(decryptEncryptFileName, keyFileName,
                    passwd, defaultFileName);
                Console.WriteLine("解密成功");
            }
            catch (Exception e)
            {
                Console.WriteLine("解密失敗" + e.Message);
            }

            textBox4.Text = defaultFileName;

            Console.Read();


        }


        /*...................................................................生產金鑰開始*/


        private static void ExportKeyPair(
            Stream secretOut,
            Stream publicOut,
            AsymmetricKeyParameter publicKey,
            AsymmetricKeyParameter privateKey,
            string identity,
            char[] passPhrase,
            bool armor)
        {
            if (armor)
            {
                secretOut = new ArmoredOutputStream(secretOut);
            }

            PgpSecretKey secretKey = new PgpSecretKey(
                PgpSignature.DefaultCertification,
                PublicKeyAlgorithmTag.RsaGeneral,
                publicKey,
                privateKey,
                DateTime.UtcNow,
                identity,
                SymmetricKeyAlgorithmTag.Cast5,
                passPhrase,
                null,
                null,
                new SecureRandom()
            );
            secretKey.Encode(secretOut);
            if (armor)
            {
                secretOut.Close();
                publicOut = new ArmoredOutputStream(publicOut);
            }

            PgpPublicKey key = secretKey.PublicKey;
            key.Encode(publicOut);
            if (armor)
            {
                publicOut.Close();
            }
        }



        /*...................................................................生產金鑰結束*/


        /*.......................................................................加密開始*/


        //外部呼叫的 method
        public static void EncryptFile(
            string outputFileName,//加密後輸出檔案名稱位置
            string inputFileName, //欲加密檔案名稱位置
            string encKeyFileName,//提供加密的 public key 檔名及位置
            bool armor,           //不明???，範例預設為true
            bool withIntegrityCheck//不明???，範例預設為false
        )
        {
            PgpPublicKey encKey = PgpExampleUtilities.ReadPublicKey(encKeyFileName);

            using (Stream output = File.Create(outputFileName))
            {
                EncryptFile(output, inputFileName, encKey, armor, withIntegrityCheck);
            }
        }

        //內部的實作參照官方範例
        private static void EncryptFile(
            Stream outputStream,
            string fileName,
            PgpPublicKey encKey,
            bool armor,
            bool withIntegrityCheck)
        {
            if (armor)
            {
                outputStream = new ArmoredOutputStream(outputStream);
            }

            try
            {
                byte[] bytes = PgpExampleUtilities.CompressFile(fileName, CompressionAlgorithmTag.Zip);

                PgpEncryptedDataGenerator encGen = new PgpEncryptedDataGenerator(
                    SymmetricKeyAlgorithmTag.Cast5, withIntegrityCheck, new SecureRandom());
                encGen.AddMethod(encKey);

                Stream cOut = encGen.Open(outputStream, bytes.Length);

                cOut.Write(bytes, 0, bytes.Length);
                cOut.Close();

                if (armor)
                {
                    outputStream.Close();
                }
            }
            catch (PgpException e)
            {
                Console.Error.WriteLine(e);

                Exception underlyingException = e.InnerException;
                if (underlyingException != null)
                {
                    Console.Error.WriteLine(underlyingException.Message);
                    Console.Error.WriteLine(underlyingException.StackTrace);
                }
            }
        }

        /*.......................................................................加密結束*/


        /*.......................................................................解密開始*/

        //外部呼叫解密 mehtod
        public static void DecryptFile(
            string inputFileName,  //欲解密之檔案名稱及位置
            string keyFileName,    //解密 Private key 位置
            char[] passwd,         //Private key password
            string defaultFileName //解密後檔案名稱及位置
        )
        {
            using (Stream input = File.OpenRead(inputFileName), //取得解密檔案之串流
                   keyIn = File.OpenRead(keyFileName)) //取得公鑰之串流
            {
                DecryptFile(input, keyIn, passwd, defaultFileName);
            }
        }

        //內部解密實作參照官方範例
        private static void DecryptFile(
            Stream inputStream,
            Stream keyIn,
            char[] passwd,
            string defaultFileName)
        {
            inputStream = PgpUtilities.GetDecoderStream(inputStream); //遞迴

            try
            {
                PgpObjectFactory pgpF = new PgpObjectFactory(inputStream);
                PgpEncryptedDataList enc;

                PgpObject o = pgpF.NextPgpObject();
                //
                // the first object might be a PGP marker packet.
                //
                if (o is PgpEncryptedDataList)
                {
                    enc = (PgpEncryptedDataList)o;
                }
                else
                {
                    enc = (PgpEncryptedDataList)pgpF.NextPgpObject();
                }

                //
                // find the secret key
                //
                PgpPrivateKey sKey = null;
                PgpPublicKeyEncryptedData pbe = null;
                PgpSecretKeyRingBundle pgpSec = new PgpSecretKeyRingBundle(
                    PgpUtilities.GetDecoderStream(keyIn));

                foreach (PgpPublicKeyEncryptedData pked in enc.GetEncryptedDataObjects())
                {
                    sKey = PgpExampleUtilities.FindSecretKey(pgpSec, pked.KeyId, passwd);

                    if (sKey != null)
                    {
                        pbe = pked;
                        break;
                    }
                }

                if (sKey == null)
                {
                    throw new ArgumentException("secret key for message not found.");
                }

                Stream clear = pbe.GetDataStream(sKey);

                PgpObjectFactory plainFact = new PgpObjectFactory(clear);

                PgpObject message = plainFact.NextPgpObject();

                if (message is PgpCompressedData)
                {
                    PgpCompressedData cData = (PgpCompressedData)message;
                    PgpObjectFactory pgpFact = new PgpObjectFactory(cData.GetDataStream());

                    message = pgpFact.NextPgpObject();
                }

                if (message is PgpLiteralData)
                {
                    PgpLiteralData ld = (PgpLiteralData)message;

                    string outFileName = ld.FileName;
                    //if (outFileName.Length == 0)
                    //{
                    outFileName = defaultFileName;
                    //}

                    Stream fOut = File.Create(outFileName);
                    Stream unc = ld.GetInputStream();
                    Streams.PipeAll(unc, fOut);
                    fOut.Close();
                }
                else if (message is PgpOnePassSignatureList)
                {
                    throw new PgpException("encrypted message contains a signed message - not literal data.");
                }
                else
                {
                    throw new PgpException("message is not a simple encrypted file - type unknown.");
                }

                if (pbe.IsIntegrityProtected())
                {
                    if (!pbe.Verify())
                    {
                        Console.Error.WriteLine("message failed integrity check");
                    }
                    else
                    {
                        Console.Error.WriteLine("message integrity check passed");
                    }
                }
                else
                {
                    Console.Error.WriteLine("no message integrity check");
                }
            }
            catch (PgpException e)
            {
                Console.Error.WriteLine(e);

                Exception underlyingException = e.InnerException;
                if (underlyingException != null)
                {
                    Console.Error.WriteLine(underlyingException.Message);
                    Console.Error.WriteLine(underlyingException.StackTrace);
                }
            }
        }



        /*.......................................................................解密結束*/


        internal class PgpExampleUtilities
        {
            internal static byte[] CompressFile(string fileName, CompressionAlgorithmTag algorithm)
            {
                MemoryStream bOut = new MemoryStream();
                PgpCompressedDataGenerator comData = new PgpCompressedDataGenerator(algorithm);
                PgpUtilities.WriteFileToLiteralData(comData.Open(bOut), PgpLiteralData.Binary,
                    new FileInfo(fileName));
                comData.Close();
                return bOut.ToArray();
            }

            /**
             * Search a secret key ring collection for a secret key corresponding to keyID if it
             * exists.
             * 
             * @param pgpSec a secret key ring collection.
             * @param keyID keyID we want.
             * @param pass passphrase to decrypt secret key with.
             * @return
             * @throws PGPException
             * @throws NoSuchProviderException
             */
            internal static PgpPrivateKey FindSecretKey(PgpSecretKeyRingBundle pgpSec, long keyID, char[] pass)
            {
                PgpSecretKey pgpSecKey = pgpSec.GetSecretKey(keyID);

                if (pgpSecKey == null)
                {
                    return null;
                }

                return pgpSecKey.ExtractPrivateKey(pass);
            }

            internal static PgpPublicKey ReadPublicKey(string fileName)
            {
                using (Stream keyIn = File.OpenRead(fileName))
                {
                    return ReadPublicKey(keyIn);
                }
            }

            /**
             * A simple routine that opens a key ring file and loads the first available key
             * suitable for encryption.
             * 
             * @param input
             * @return
             * @throws IOException
             * @throws PGPException
             */
            internal static PgpPublicKey ReadPublicKey(Stream input)
            {
                PgpPublicKeyRingBundle pgpPub = new PgpPublicKeyRingBundle(
                    PgpUtilities.GetDecoderStream(input));

                //
                // we just loop through the collection till we find a key suitable for encryption, in the real
                // world you would probably want to be a bit smarter about this.
                //

                foreach (PgpPublicKeyRing keyRing in pgpPub.GetKeyRings())
                {
                    foreach (PgpPublicKey key in keyRing.GetPublicKeys())
                    {
                        if (key.IsEncryptionKey)
                        {
                            return key;
                        }
                    }
                }

                throw new ArgumentException("Can't find encryption key in key ring.");
            }

            internal static PgpSecretKey ReadSecretKey(string fileName)
            {
                using (Stream keyIn = File.OpenRead(fileName))
                {
                    return ReadSecretKey(keyIn);
                }
            }

            /**
             * A simple routine that opens a key ring file and loads the first available key
             * suitable for signature generation.
             * 
             * @param input stream to read the secret key ring collection from.
             * @return a secret key.
             * @throws IOException on a problem with using the input stream.
             * @throws PGPException if there is an issue parsing the input stream.
             */
            internal static PgpSecretKey ReadSecretKey(Stream input)
            {
                PgpSecretKeyRingBundle pgpSec = new PgpSecretKeyRingBundle(
                    PgpUtilities.GetDecoderStream(input));

                //
                // we just loop through the collection till we find a key suitable for encryption, in the real
                // world you would probably want to be a bit smarter about this.
                //

                foreach (PgpSecretKeyRing keyRing in pgpSec.GetKeyRings())
                {
                    foreach (PgpSecretKey key in keyRing.GetSecretKeys())
                    {
                        if (key.IsSigningKey)
                        {
                            return key;
                        }
                    }
                }

                throw new ArgumentException("Can't find signing key in key ring.");
            }
        }


        /*.......................................................................數位簽章開始*/


        private static void SignFile(
            string fileName,        //欲作簽章的檔案名稱及位置
            Stream keyIn,       // Private key 的 File Stream
            Stream outputStream,    //簽章後的檔案 File Stream
            char[] pass,        // private Key 的 password
            bool armor,         //用途不明?? 範例預設true
            bool compress       //用途不明?? 範例預設true
)
        {
            if (armor)
            {
                outputStream = new ArmoredOutputStream(outputStream);
            }
            PgpSecretKey pgpSec = PgpExampleUtilities.ReadSecretKey(keyIn);
            PgpPrivateKey pgpPrivKey = pgpSec.ExtractPrivateKey(pass);
            PgpSignatureGenerator sGen = new PgpSignatureGenerator(pgpSec.PublicKey.Algorithm, HashAlgorithmTag.Sha256);
            sGen.InitSign(PgpSignature.BinaryDocument, pgpPrivKey);
            foreach (string userId in pgpSec.PublicKey.GetUserIds())
            {
                PgpSignatureSubpacketGenerator spGen = new PgpSignatureSubpacketGenerator();
                spGen.SetSignerUserId(false, userId);
                sGen.SetHashedSubpackets(spGen.Generate());
                // Just the first one!
                break;
            }
            Stream cOut = outputStream;
            PgpCompressedDataGenerator cGen = null;
            if (compress)
            {
                cGen = new PgpCompressedDataGenerator(CompressionAlgorithmTag.ZLib);
                cOut = cGen.Open(cOut);
            }
            BcpgOutputStream bOut = new BcpgOutputStream(cOut);
            sGen.GenerateOnePassVersion(false).Encode(bOut);
            FileInfo file = new FileInfo(fileName);
            PgpLiteralDataGenerator lGen = new PgpLiteralDataGenerator();
            Stream lOut = lGen.Open(bOut, PgpLiteralData.Binary, file);
            FileStream fIn = file.OpenRead();
            int ch = 0;
            while ((ch = fIn.ReadByte()) >= 0)
            {
                lOut.WriteByte((byte)ch);
                sGen.Update((byte)ch);
            }
            fIn.Close();
            lGen.Close();
            sGen.Generate().Encode(bOut);
            if (cGen != null)
            {
                cGen.Close();
            }
            if (armor)
            {
                outputStream.Close();
            }
        }

        /*.......................................................................數位簽章結束*/

        /*.......................................................................檔案數認證開始*/

        private static void VerifyFile(
            Stream inputStream,     //準備做數位認證檔案的 File Stream 
            Stream keyIn,       // Public Key 的 File Stream
            string outputFileName   // 將數位簽章清除後產生未簽章之原始黨
        )
        {
            inputStream = PgpUtilities.GetDecoderStream(inputStream);
            PgpObjectFactory pgpFact = new PgpObjectFactory(inputStream);
            PgpCompressedData c1 = (PgpCompressedData)pgpFact.NextPgpObject();
            pgpFact = new PgpObjectFactory(c1.GetDataStream());
            PgpOnePassSignatureList p1 = (PgpOnePassSignatureList)pgpFact.NextPgpObject();
            PgpOnePassSignature ops = p1[0];
            PgpLiteralData p2 = (PgpLiteralData)pgpFact.NextPgpObject();
            Stream dIn = p2.GetInputStream();
            PgpPublicKeyRingBundle pgpRing = new PgpPublicKeyRingBundle(PgpUtilities.GetDecoderStream(keyIn));
            PgpPublicKey key = pgpRing.GetPublicKey(ops.KeyId);
            //add
            Stream fileOutput = File.Create(outputFileName);
            ops.InitVerify(key);
            int ch;
            while ((ch = dIn.ReadByte()) >= 0)
            {
                ops.Update((byte)ch);
                fileOutput.WriteByte((byte)ch);
            }
            fileOutput.Close();

            PgpSignatureList p3 = (PgpSignatureList)pgpFact.NextPgpObject();
            PgpSignature firstSig = p3[0];
            if (ops.Verify(firstSig))
            {
                Console.Out.WriteLine("signature verified.");
            }
            else
            {
                Console.Out.WriteLine("signature verification failed.");
            }
        }

        /*.......................................................................檔案數認證結束*/


        private void Button4_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
            this.textBox7.Text = openFileDialog1.FileName;
        }

        private void Button5_Click(object sender, EventArgs e)
        {
            openFileDialog2.ShowDialog();
            this.textBox8.Text = openFileDialog2.FileName;
        }

        private void Button7_Click(object sender, EventArgs e)
        {
            openFileDialog3.ShowDialog();
            this.textBox6.Text = openFileDialog3.FileName;
        }

        private void Button6_Click(object sender, EventArgs e)
        {
            openFileDialog4.ShowDialog();
            this.textBox5.Text = openFileDialog4.FileName;
        }

        private void Button8_Click(object sender, EventArgs args)
        {
            ////private2 對檔案做數位簽章
            //string fileName = @"D:/BC/a.txt";
            //Stream signkeyIn = File.OpenRead(@"D:/BC/priv.asc");
            //Stream signOutputStream = File.Create(@"D:/BC/b.txt");
            //char[] signPass = "123456".ToCharArray();
            //bool signArmor = true;
            //bool compress = true;

            //private2 對檔案做數位簽章
            string fileName = textBox9.Text;
            Stream signkeyIn = File.OpenRead(@"C:\Users\MARK\Desktop\PGP\PGPKEY\priv.asc");
            Stream signOutputStream = File.Create(@"C:\Users\MARK\Desktop\PGP\PGPKEY\XXabc.txt");
            char[] signPass = "123456".ToCharArray();
            bool signArmor = true;
            bool compress = true;

            try
            {
                SignFile(fileName, signkeyIn, signOutputStream, signPass, signArmor, compress);
                Console.WriteLine("簽章成功");
            }
            catch (Exception e)
            {
                Console.WriteLine("簽章失敗" + e.Message);
            }
            finally
            {
                textBox10.Text= @"C:\Users\MARK\Desktop\PGP\PGPKEY\XXabc.txt";
                signkeyIn.Close();
                signOutputStream.Close();
            }


        }

        private void Button9_Click(object sender, EventArgs args)
        {
            ////public2 對檔案做數位認證
            //Stream inputStream = File.OpenRead(@"D:/BC/b.txt");
            //Stream keyIn = File.OpenRead(@"D:/BC/pub.asc");
            //string outputFileName = @"D/BC/c.xml";

            //public2 對檔案做數位認證
            Stream inputStream = File.OpenRead(@"C:\Users\MARK\Desktop\PGP\PGPKEY\一卡通(加密+簽章).txt");
            Stream keyIn = File.OpenRead(@"C:\Users\MARK\Desktop\PGP\PGPKEY\pub.asc");
            string outputFileName = @"C:\Users\MARK\Desktop\PGP\PGPKEY\一卡通(加密+簽章+認證).txt";

            try
            {
                VerifyFile(inputStream, keyIn, outputFileName);
                Console.WriteLine("認證OK");
            }
            catch (Exception e)
            {
                Console.WriteLine("認證失敗" + e.Message);
            }
            Console.Read();
        }
    }
}

