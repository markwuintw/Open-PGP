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
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;

namespace OpenPGP
{
    public partial class RSAKeys : UserControl
    {
        public RSAKeys()
        {
            InitializeComponent();
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.ShowDialog();
            textBox3.Text= folderBrowserDialog1.SelectedPath;
        }
        private void Button1_Click_1(object sender, EventArgs e)
        {
            //string signature = textBox1.Text;
            //textBox4.Text = @"C:\Users\MARK\Desktop\PGP\PGPKEY\priv.asc";
            //textBox5.Text = @"C:\Users\MARK\Desktop\PGP\PGPKEY\pub.asc";

            ////RSA密鑰產生器
            //IAsymmetricCipherKeyPairGenerator kpg = GeneratorUtilities.GetKeyPairGenerator("RSA");
            ////Key 構造使用參數        
            //kpg.Init(new RsaKeyGenerationParameters(
            //    BigInteger.ValueOf(0x10001), new SecureRandom(),
            //    1024, // key 的長度
            //    25));
            //AsymmetricCipherKeyPair kp = kpg.GenerateKeyPair();
            //char[] password = signature.ToCharArray(); //私鑰的密碼
            //Stream out1, out2;
            //out1 = File.Create(textBox4.Text); //私鑰放置位置          
            //out2 = File.Create(textBox5.Text); //公鑰放置位置
            //ExportKeyPair(out1, out2, kp.Public, kp.Private, "MarkWu", password, true);

            string signature = textBox1.Text;
            string keyWord = textBox2.Text;
            string storagePath = textBox3.Text;
            string priv_Path = storagePath + @"\" + keyWord + "_priv.gpg";
            string pub_Path = storagePath + @"\" + keyWord + "_pub.gpg";


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
            out1 = File.Create(priv_Path); //私鑰放置位置          
            out2 = File.Create(pub_Path); //公鑰放置位置
            ExportKeyPair(out1, out2, kp.Public, kp.Private, "MarkWu", password, true); //匯出

            out1.Close(); //連線中斷
            out2.Close();
            out1.Dispose(); //解構值
            out2.Dispose();

            textBox4.Text= priv_Path;
            textBox5.Text = pub_Path;
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

            // PgpSecretKey物件包含私鑰及公鑰，使用私鑰需要配合私鑰密碼

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
            secretKey.Encode(secretOut); //寫入檔案
            if (armor)
            {
                secretOut.Close();
                publicOut = new ArmoredOutputStream(publicOut);
            }

            PgpPublicKey key = secretKey.PublicKey;
            key.Encode(publicOut); //寫入檔案
            if (armor)
            {
                publicOut.Close();
            }
        }

        /*...................................................................生產金鑰結束*/

    }
}
