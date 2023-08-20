namespace kafkaconsumer.Crypt
{
    public interface ICryptoSettings
    {
        public string ProjId { get; set; }
        public string LocationId { get; set; }
        public string KeyRingId { get; set; }
        public string KeyId { get; set; }
        public string KeyVersionId { get; set; }
    }
}