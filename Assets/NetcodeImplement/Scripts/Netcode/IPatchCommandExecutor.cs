using System.Collections;
using System.Collections.Generic;
using UnityEngine; 

namespace Wayne.Network.NetcodeImplement {
    public interface IPatchCommandExecutor {
        bool Verify(Patch patch);

        void ExeCommand(Patch patch);
    }
}