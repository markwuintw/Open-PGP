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
using Org.BouncyCastle.Security;

namespace OpenPGP
{
    public partial class PGPEncryption : UserControl
    {
        public PGPEncryption()
        {
            InitializeComponent();
        }

        private void PGPEncryption_Load(object sender, EventArgs e)
        {

        }

        private void Button1_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.ShowDialog();
            textBox2.Text = folderBrowserDialog1.SelectedPath;
        }

        private void Button2_Click_1(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
            this.textBox3.Text = openFileDialog1.FileName;
        }

        private void Button3_Click(object sender, EventArgs e)
        {
            openFileDialog2.ShowDialog();
            this.textBox4.Text = openFileDialog2.FileName;
        }

        private void Button4_Click_1(object sender, EventArgs args)
        {
            ////Public 1 檔案加密
            //string outputFileName = @"C:/Users/MARK/Desktop/PGP/PGPKEY/Xabc.txt";
            //string inputFileName = @"C:/Users/MARK/Desktop/PGP/PGPKEY/abc.txt";
            //string encKeyFileName = @"C:/Users/MARK/Desktop/PGP/PGPKEY/pub.asc";
            //bool armor = true;
            //bool withIntegrityCheck = false;

            //Public 1 檔案加密
            string outputFileName = textBox2.Text + @"\" + textBox1.Text + ".gpg"; //預計匯出(加密後檔案)的完整路徑
            string inputFileName = textBox3.Text; //預計加密原始檔案的完整路徑
            string encKeyFileName = textBox4.Text; //接收方公鑰的完整路徑
            bool armor = true;
            bool withIntegrityCheck = false;


            try
            {
                EncryptFile(outputFileName, inputFileName,
                    encKeyFileName, armor, withIntegrityCheck);
                Console.WriteLine("加密成功");

                textBox5.Text = outputFileName;

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


            Console.Read();
        }

        /*.......................................................................加密開始*/


        /*文章 -> Session Key(對稱式) - > 文章加密 - - - - - -> 加密後的文章
               -> Session Key(對稱式) - > 公鑰(對方)加密 - - -> 加密後的Session Key
        */


        //外部呼叫的 method
        public static void EncryptFile(
            string outputFileName,//加密後輸出檔案名稱位置
            string inputFileName, //欲加密檔案名稱位置
            string encKeyFileName,//提供加密的 public key(接收方的) 檔名及位置
            bool armor,           //盔甲??，範例預設為true
            bool withIntegrityCheck//完整性檢查，範例預設為false
        )
        {
            PgpPublicKey encKey = PgpExampleUtilities.ReadPublicKey(encKeyFileName); //find encryption key

            using (Stream output = File.Create(outputFileName))
            {
                EncryptFile(output, inputFileName, encKey, armor, withIntegrityCheck);
            }
        }

        /*文章 -> Session Key(對稱式) - > 文章加密 - - - - - -> 加密後的文章
               -> Session Key(對稱式) - > 公鑰(對方)加密 - - -> 加密後的Session Key
       */

        //內部的實作參照官方範例
        private static void EncryptFile(
            Stream outputStream, //加密後輸出檔案之資料流
            string fileName, //欲加密檔案名稱位置
            PgpPublicKey encKey, //接收方的公鑰(對方)
            bool armor,
            bool withIntegrityCheck /*完整性檢查*/)
        {
            if (armor)
            {
                outputStream = new ArmoredOutputStream(outputStream); //位置、headers、雜湊表
            }

            try
            {
                byte[] bytes = PgpExampleUtilities.CompressFile(fileName, CompressionAlgorithmTag.Zip); //資料壓縮一個檔案

                PgpEncryptedDataGenerator encGen = new PgpEncryptedDataGenerator(
                    SymmetricKeyAlgorithmTag.Cast5, withIntegrityCheck, new SecureRandom()); //隨機產生Session Key(對稱-Cast5)

                encGen.AddMethod(encKey); 

                Stream cOut = encGen.Open(outputStream, bytes.Length); //建立 todo 注意，RSA非對稱加密，公鑰加密私鑰解密，預設為SHA1，但可選擇 SHA256雜湊 

                cOut.Write(bytes, 0, bytes.Length); //加密及寫入
                cOut.Close();

                if (armor)
                {
                    outputStream.Close();
                    outputStream.Dispose();
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
            //private2 對檔案做數位簽章
            string fileName = textBox8.Text; //預計數位簽章原始檔案的完整路徑
            Stream signkeyIn = File.OpenRead(textBox9.Text); //發送方(自己)私鑰之資料流
            Stream signOutputStream = File.Create(textBox7.Text + @"\" + textBox6.Text + ".gpg"); //預計匯出(簽章後檔案)之資料流
            char[] signPass = textBox10.Text.ToCharArray(); //私鑰密碼
            bool signArmor = true;
            bool compress = true;



            //////private2 對檔案做數位簽章
            ////string fileName = @"D:/BC/a.txt";
            ////Stream signkeyIn = File.OpenRead(@"D:/BC/priv.asc");
            ////Stream signOutputStream = File.Create(@"D:/BC/b.txt");
            ////char[] signPass = "123456".ToCharArray();
            ////bool signArmor = true;
            ////bool compress = true;


            try
            {
                SignFile(fileName, signkeyIn, signOutputStream, signPass, signArmor, compress);
                Console.WriteLine("簽章成功");
                textBox11.Text = textBox7.Text + @"\" + textBox6.Text + ".gpg";

            }
            catch (Exception e)
            {
                Console.WriteLine("簽章失敗" + e.Message);
            }
            finally
            {
                signkeyIn.Close();
                signkeyIn.Dispose();
                signOutputStream.Close();
                signOutputStream.Dispose();
            }

        }

        /*
         文章 -> hash -> 私鑰(自己)簽章 -> 簽章後的hash值
         文章 - - - - - - - - - - - - - -> 文章
         */


        /*.......................................................................數位簽章開始*/


        private static void SignFile(
            string fileName,        //預計數位簽章原始檔案的完整路徑
            Stream keyIn,       // Private key 的 File Stream (自己) 
            Stream outputStream,    //預計匯出(數位簽章後) File Stream
            char[] pass,        // private Key 的 password
            bool armor,         //盔甲??? 範例預設true
            bool compress       //解壓縮 範例預設true
)
        {
            if (armor)
            {
                outputStream = new ArmoredOutputStream(outputStream);  //匯出位置、headers、雜湊表
            }
            PgpSecretKey pgpSec = PgpExampleUtilities.ReadSecretKey(keyIn); //PgpSecretKey包含私鑰及公鑰整個物件
            PgpPrivateKey pgpPrivKey = pgpSec.ExtractPrivateKey(pass); //需輸入私鑰密碼才能取出私鑰

            /*
             SHA是由美國國家安全局制定，主要應用於數字簽名標準裡面的數字簽名算法( DSA : Digital Signature Algorithm )，
             SHA家族中以SHA1和SHA256最為廣泛使用。SHA1的雜湊值長度為160bit、SHA256則為256bit，長度越長碰撞的機會就越低也越安全，
             但同時計算的時間複雜度也隨著增高。       
             */

            PgpSignatureGenerator sGen = new PgpSignatureGenerator(pgpSec.PublicKey.Algorithm, HashAlgorithmTag.Sha256); //PublicKey.Algorithm即原始公鑰
            sGen.InitSign(PgpSignature.BinaryDocument, pgpPrivKey); //若沒私鑰重新生產一個

            foreach (string userId in pgpSec.PublicKey.GetUserIds()) //ExportKeyPair 的 identity (MarkWu)
            {
                PgpSignatureSubpacketGenerator spGen = new PgpSignatureSubpacketGenerator();
                spGen.SetSignerUserId(false, userId); //數位簽章的使用者
                sGen.SetHashedSubpackets(spGen.Generate()); //將 SignatureSubpacket 陣列化再回傳
                // Just the first one!
                break;
            }
            Stream cOut = outputStream;
            PgpCompressedDataGenerator cGen = null;
            if (compress) //解壓縮
            {
                cGen = new PgpCompressedDataGenerator(CompressionAlgorithmTag.ZLib);
                cOut = cGen.Open(cOut); 
            }
            BcpgOutputStream bOut = new BcpgOutputStream(cOut);
            sGen.GenerateOnePassVersion(false).Encode(bOut);  //hash 加密

            FileInfo file = new FileInfo(fileName);
            PgpLiteralDataGenerator lGen = new PgpLiteralDataGenerator();
            Stream lOut = lGen.Open(bOut, PgpLiteralData.Binary, file);
            FileStream fIn = file.OpenRead();
            int ch = 0;
            while ((ch = fIn.ReadByte()) >= 0) //從資料流讀取一個位元組
            {
                lOut.WriteByte((byte)ch); //寫入預計匯出檔案
                sGen.Update((byte)ch); //進行加密?
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
    }
}
