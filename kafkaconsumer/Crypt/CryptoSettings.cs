namespace kafkaconsumer.Crypt
{
    public class CryptoSettings : ICryptoSettings
    {
        public string ProjId { get; set; }
        public string LocationId { get; set; }
        public string KeyRingId { get; set; }
        public string KeyId { get; set; }
        public string KeyVersionId { get; set; }

        public CryptoSettings() { }  
    }
}
