namespace kafkaconsumer.Crypt
{
    public interface IDecryptAsymmetric
    {
        public ICryptoSettings GetConfigSettings();
        public string DecryptAsymmetricString(byte[] ciphertext = null);
    }
}
