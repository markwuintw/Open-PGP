# Open-PGP

使用的套件為 bouncycastle

http://www.bouncycastle.org/csharp/

參考網站及文章

http://fredjama.blogspot.com/2013/08/c-pgp-bouncy-castleopenpgp-library.htm

碰到問題：

1. 第一次寫 win forms ，和過去寫網頁經驗最大的不同，在於網頁關閉時記憶體就自動釋放了，Win Forms 需自行下指令進行。 
   
2. 選擇(上傳/指定)檔案時，也有所不同，常會需要使用 Stream 類別來介接及相關應用。
 
3. 首次接觸密碼學，了解數位簽章、加密、解密、數位認證的概念及方式：

數位簽章

         文章 -> hash -> 私鑰(自己)簽章 -> 簽章後的hash值
         文章 - - - - - - - - - - - - - -> 文章

加密

         文章  -> Session Key(對稱式) - > 文章加密 - - - - - -> 加密後的文章
               -> Session Key(對稱式) - > 公鑰(對方)加密 - - -> 加密後的Session Key

解密

         加密後的Session Key -> 私鑰(自己)解密 -> Session Key(對稱式)
         加密後的文章 - - - - - - - - - - - - - > Session Key(對稱式) -> 解密後的文章

數位認證

         簽章後的hash值 -> 公鑰(對方)解密 -> hash值
         解密後的文章 -> hash -> 解密後文章的hash值
         

參考資料：

https://www.asiapeak.com/PGPTheory.php (PGP 加密原理)

https://zh.wikipedia.org/wiki/PGP (WIKI-PGP)

https://zh.wikipedia.org/wiki/%E6%95%B8%E4%BD%8D%E7%B0%BD%E7%AB%A0 (WIKI-數位簽章)
