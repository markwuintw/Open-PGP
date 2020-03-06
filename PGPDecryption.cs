using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Org.BouncyCastle.Bcpg;
using Org.BouncyCastle.Bcpg.OpenPgp;
using Org.BouncyCastle.Utilities.IO;

namespace OpenPGP
{
    public partial class PGPDecryption : UserControl
    {
        public PGPDecryption()
        {
            InitializeComponent();
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.ShowDialog();
            textBox2.Text = folderBrowserDialog1.SelectedPath;
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
            this.textBox3.Text = openFileDialog1.FileName;
        }

        private void Button3_Click(object sender, EventArgs e)
        {
            openFileDialog2.ShowDialog();
            this.textBox4.Text = openFileDialog2.FileName;
        }

        private void Button4_Click(object sender, EventArgs args)
        {
            ////public2 對檔案做數位認證
            //Stream inputStream = File.OpenRead(@"D:/BC/b.txt");
            //Stream keyIn = File.OpenRead(@"D:/BC/pub.asc");
            //string outputFileName = @"D/BC/c.xml";


            //public2 對檔案做數位認證
            Stream inputStream = File.OpenRead(textBox3.Text); //預計數位認證原始檔案的資料流
            Stream keyIn = File.OpenRead(textBox4.Text); //簽名方(對方)公鑰的資料流
            string outputFileName = textBox2.Text + @"\" + textBox1.Text + ".gpg"; //預計匯出(數位認證後檔案)的完整路徑

            try
            {
                VerifyFile(inputStream, keyIn, outputFileName);
                Console.WriteLine("認證OK");

                textBox5.Text = outputFileName;
            }
            catch (Exception e)
            {
                Console.WriteLine("認證失敗" + e.Message);
            }
            Console.Read();
        }

        /*.......................................................................檔案數認證開始*/

        /*首先傳送端將訊息經過雜湊演算法計算後得到一個雜湊值，再利用它的私有鑰匙向雜湊值加密成為一個數位簽章（DS），接著，再將數位簽章附加在訊息後面一併傳送出去；
         接收端收到訊息之後，以同樣的雜湊演算計算出雜湊值（H’），並利用傳送端的公開鑰匙將 DS 解密，得到另一端的雜湊值（H）。
         接著，比較兩個雜湊值，如果相同的話，則可以確定該訊息的『完整性』（雜湊值相同），此外也可以確定其『不可否認性』（私有鑰匙與公開鑰匙配對）。*/

        /*
         簽章後的hash值 -> 公鑰(對方)解密 -> hash值
         解密後的文章 -> hash -> 解密後文章的hash值
         */


        private static void VerifyFile(
            Stream inputStream,     //預計數位認證原始檔案的資料流
            Stream keyIn,       //簽名方(對方)公鑰的資料流
            string outputFileName   //預計匯出(數位認證後檔案)的完整路徑
        )
        {
            inputStream = PgpUtilities.GetDecoderStream(inputStream); //串流陣列化 byte Array
            PgpObjectFactory pgpFact = new PgpObjectFactory(inputStream);

            PgpCompressedData c1 = (PgpCompressedData)pgpFact.NextPgpObject();
            pgpFact = new PgpObjectFactory(c1.GetDataStream()); //解壓縮
            PgpOnePassSignatureList p1 = (PgpOnePassSignatureList)pgpFact.NextPgpObject();
            PgpOnePassSignature ops = p1[0];
            PgpLiteralData p2 = (PgpLiteralData)pgpFact.NextPgpObject();
            Stream dIn = p2.GetInputStream();  

            PgpPublicKeyRingBundle pgpRing = new PgpPublicKeyRingBundle(PgpUtilities.GetDecoderStream(keyIn));
            PgpPublicKey key = pgpRing.GetPublicKey(ops.KeyId);  //取出公鑰

            Stream fileOutput = File.Create(outputFileName); //預計匯出檔案建立
            ops.InitVerify(key); //驗證公鑰是否符合
            int ch;
            while ((ch = dIn.ReadByte()) >= 0)
            {
                ops.Update((byte)ch); //進行解密
                fileOutput.WriteByte((byte)ch); //寫入預計匯出檔案
            }
            fileOutput.Close();
            fileOutput.Dispose();

            PgpSignatureList p3 = (PgpSignatureList)pgpFact.NextPgpObject();
            PgpSignature firstSig = p3[0];

            if (ops.Verify(firstSig)) //Hash值比較
            {
                Console.Out.WriteLine("signature verified.");
            }
            else
            {
                Console.Out.WriteLine("signature verification failed.");
            }
        }

        /*.......................................................................檔案數認證結束*/

        private void Button5_Click(object sender, EventArgs e)
        {
            folderBrowserDialog2.ShowDialog();
            textBox7.Text = folderBrowserDialog2.SelectedPath;
        }

        private void Button6_Click(object sender, EventArgs e)
        {
            openFileDialog3.ShowDialog();
            this.textBox8.Text = openFileDialog3.FileName;
        }

        private void Button7_Click(object sender, EventArgs e)
        {
            openFileDialog4.ShowDialog();
            this.textBox9.Text = openFileDialog4.FileName;
        }

        private void Button8_Click(object sender, EventArgs args)
        {
            ////private1 檔案解密
            //string decryptEncryptFileName = @"D:/BC/b.txt";
            //string keyFileName = @"D:/BC/priv1.asc";
            //char[] passwd = "123456".ToCharArray();
            //string defaultFileName = @"D:/BC/c.txt";



            //private1 檔案解密
            string decryptEncryptFileName = textBox8.Text; //預計解密原始檔案的完整路徑
            string keyFileName = textBox9.Text; //接收方(自己)私鑰完整路徑
            char[] passwd = textBox10.Text.ToCharArray(); //私鑰密碼
            string defaultFileName = textBox7.Text + @"/" + textBox6.Text + ".gpg"; //預計匯出(解密後檔案)的完整路徑

            try
            {
                DecryptFile(decryptEncryptFileName, keyFileName,
                    passwd, defaultFileName);
                Console.WriteLine("解密成功");

                textBox11.Text = defaultFileName;
            }
            catch (Exception e)
            {
                Console.WriteLine("解密失敗" + e.Message);
            }

            Console.Read();
        }




        /*.......................................................................解密開始*/

        /*
         加密後的Session Key -> 私鑰(自己)解密 -> Session Key(對稱式)
         加密後的文章 - - - - - - - - - - - - - > Session Key(對稱式) -> 解密後的文章
         */

        //外部呼叫解密 mehtod
        public static void DecryptFile(
            string inputFileName,  //欲解密之檔案名稱及位置
            string keyFileName,    //解密 Private key 位置
            char[] passwd,         //Private key password
            string defaultFileName //解密後檔案名稱及位置
        )
        {
            using (Stream input = File.OpenRead(inputFileName), //取得預計解密檔案之串流
                   keyIn = File.OpenRead(keyFileName)) //取得私鑰之串流
            {
                DecryptFile(input, keyIn, passwd, defaultFileName);
            }
        }

        /*
         加密後的Session Key -> 私鑰(自己)解密 -> Session Key(對稱式)
         加密後的文章 - - - - - - - - - - - - - > Session Key(對稱式) -> 解密後的文章
 */


        private static void DecryptFile(
            Stream inputStream, //欲解密之檔案之串流
            Stream keyIn, //私鑰之串流
            char[] passwd, //私鑰密碼
            string defaultFileName //解密後檔案名稱及位置
            )
        {
            inputStream = PgpUtilities.GetDecoderStream(inputStream); //資料流陣列化 byte Array

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
                // find the Secret Key
                //
                PgpPrivateKey sKey = null;
                PgpPublicKeyEncryptedData pbe = null;
                PgpSecretKeyRingBundle pgpSec = new PgpSecretKeyRingBundle(
                    PgpUtilities.GetDecoderStream(keyIn)); //new 密鑰捆包 物件

                foreach (PgpPublicKeyEncryptedData pked in enc.GetEncryptedDataObjects())
                {
                    sKey = PgpExampleUtilities.FindSecretKey(pgpSec, pked.KeyId, passwd); //(密鑰捆包,identify,私鑰密碼)，抓對稱加密的密鑰，密鑰

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

                Stream clear = pbe.GetDataStream(sKey);  //解碼

                PgpObjectFactory plainFact = new PgpObjectFactory(clear); // BcpgInputStream

                PgpObject message = plainFact.NextPgpObject();  //取出

                if (message is PgpCompressedData) 
                {
                    PgpCompressedData cData = (PgpCompressedData)message; 
                    PgpObjectFactory pgpFact = new PgpObjectFactory(cData.GetDataStream()); 

                    message = pgpFact.NextPgpObject(); //解壓縮
                }

                if (message is PgpLiteralData)
                {
                    PgpLiteralData ld = (PgpLiteralData)message; //內容

                    string outFileName = ld.FileName;
                    //if (outFileName.Length == 0)
                    //{
                    outFileName = defaultFileName;
                    //}

                    Stream fOut = File.Create(outFileName);
                    Stream unc = ld.GetInputStream();
                    Streams.PipeAll(unc, fOut); //Pipe一種傳輸方式，存入匯出檔案
                    fOut.Close();
                    fOut.Dispose();
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

        private void PGPDecryption_Load(object sender, EventArgs e)
        {

        }
    }
}
