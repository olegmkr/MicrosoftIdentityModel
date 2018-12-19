using Microsoft.IdentityModel.Tokens;

namespace MicrosoftIdentityModel
{
    class MySecurityKey : SecurityKey
    {
        public MySecurityKey(string key)
        {
            KeyId = key;
        }

        public override int KeySize { get; }
    }
}
