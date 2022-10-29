using FSMHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fro_mod
{
    class WalkingState : BaseFSMState
    {
        void OnRespawn()
        {
            MonoBehaviourSingleton<PlayerController>.Instance.ResetAllAnimations();
            MonoBehaviourSingleton<PlayerController>.Instance.AnimGrindTransition(false);
            MonoBehaviourSingleton<PlayerController>.Instance.AnimOllieTransition(false);
            MonoBehaviourSingleton<PlayerController>.Instance.AnimSetupTransition(false);
            base.DoTransition(typeof(PlayerState_Riding), null);
        }
    }
}
