using CerberusFramework.Managers.Pool;
using UnityEngine;

namespace CFGameClient.Core.Events
{
    public readonly struct AttachParticleEvent
    {
        public readonly CFPoolKeys PoolKey;
        public readonly GameObject ParticleGameObject;

        public AttachParticleEvent(CFPoolKeys poolKey, GameObject particleObject)
        {
            PoolKey = poolKey;
            ParticleGameObject = particleObject;
        }
    }
}