public TokenPassport CreateTokenPassport()
        {
            String nonce = ComputeNonce();
            long timestamp = ComputeTimestamp();
            string account = ConfigurationManager.AppSettings["login.acct"];
            string consumerKey = ConfigurationManager.AppSettings["login.tbaConsumerKey"];
            string consumerSecret = ConfigurationManager.AppSettings["login.tbaConsumerSecret"];
            string tokenId = ConfigurationManager.AppSettings["login.tbaTokenId"];
            string tokenSecret = ConfigurationManager.AppSettings["login.tbaTokenSecret"];
            TokenPassportSignature signature = ComputeSignature(account, consumerKey, consumerSecret, tokenId, tokenSecret, nonce, timestamp);

            TokenPassport tokenPassport = new TokenPassport();
            tokenPassport.account = account;
            tokenPassport.consumerKey = consumerKey;
            tokenPassport.token = tokenId;
            tokenPassport.nonce = nonce;
            tokenPassport.timestamp = timestamp;
            tokenPassport.signature = signature;

            return tokenPassport;

        }

        private string ComputeNonce()
        {
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            byte[] data = new byte[20];
            rng.GetBytes(data);
            int value = Math.Abs(BitConverter.ToInt32(data, 0));
            return value.ToString();
        }

        private long ComputeTimestamp()
        {
            return ((long)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds);
        }

        private TokenPassportSignature ComputeSignature(string compId, string consumerKey, string consumerSecret,
                                        string tokenId, string tokenSecret, string nonce, long timestamp)
        {
            string baseString = compId + "&" + consumerKey + "&" + tokenId + "&" + nonce + "&" + timestamp;
            string key = consumerSecret + "&" + tokenSecret;
            string signature = "";
            var encoding = new System.Text.ASCIIEncoding();
            byte[] keyBytes = encoding.GetBytes(key);
            byte[] baseStringBytes = encoding.GetBytes(baseString);
            using (var hmacSha256 = new HMACSHA256(keyBytes))
            {
                byte[] hashBaseString = hmacSha256.ComputeHash(baseStringBytes);
                signature = Convert.ToBase64String(hashBaseString);
            }
            TokenPassportSignature sign = new TokenPassportSignature();
            sign.algorithm = "HMAC_SHA256";
            sign.Value = signature;
            return sign;
        }

//make a request
NetSuiteService nss = new NetSuiteService();
nss.Url = APILink;
nss.tokenPassport = CreateTokenPassport();
 SearchResult result = nss.search(....);
