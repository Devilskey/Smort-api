using WebPush;


namespace Smort_api.Handlers
{
    public class VapidKeyGenerator
    {
        public string publicVipidKeys = "";
        public string privateVipidKeys = "";

        public VapidKeyGenerator GenerateVapidKeys()
        {
            var vapidKeys = VapidHelper.GenerateVapidKeys();
            publicVipidKeys = vapidKeys.PublicKey;
            privateVipidKeys = vapidKeys.PrivateKey;

            return this;
        }
    }
}
