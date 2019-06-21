namespace HideezSafe.Modules.ActionHandler
{
    public class Account
    {
        public Account(string deviceId, ushort key, string name, string login, bool hasOtpSecret, string[] apps, string[] urls)
        {
            DeviceId = deviceId;
            Key = key;
            Name = name;

            Apps = apps;
            Urls = urls;

            Login = login;
            HasOtpSecret = hasOtpSecret;
        }

        public string DeviceId { get; }
        public ushort Key { get; }
        public string Name { get; }

        public string[] Apps { get; }
        public string[] Urls { get; }

        public string Login { get; }
        public bool HasOtpSecret { get; }

        public override bool Equals(object obj)
        {
            if (obj is Account accountObj)
            {
                return Name == accountObj.Name
                        && Apps == accountObj.Apps
                        && Urls == accountObj.Urls
                        && Login == accountObj.Login;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return $"{Name}{string.Join("", Apps)}{string.Join("", Urls)}{Login}".GetHashCode(); 
        }
    }
}
