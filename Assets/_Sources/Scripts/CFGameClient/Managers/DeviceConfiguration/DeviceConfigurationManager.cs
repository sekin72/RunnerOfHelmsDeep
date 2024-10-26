using System.Threading;
using CerberusFramework.UI.Components;
using Cysharp.Threading.Tasks;

namespace CFGameClient.Managers.DeviceConfiguration
{
    public class DeviceConfigurationManager : CerberusFramework.Managers.DeviceConfiguration.DeviceConfigurationManager
    {
        public override UniTask DeviceConfiguration(CancellationToken disposeToken)
        {
            if (IsPC)
            {
                SafeArea.MinimumPadding = new EdgeInsets(0, 0, 0, 0);
                SafeArea.MaximumPadding = new EdgeInsets(2147483647, 2147483647, 2147483647, 2147483647);
            }

            return UniTask.CompletedTask;
        }
    }
}
