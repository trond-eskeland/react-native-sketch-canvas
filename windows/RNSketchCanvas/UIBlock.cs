using ReactNative.UIManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RNSketchCanvas
{
    class UIBlock : IUIBlock
    {
        private readonly Action<NativeViewHierarchyManager> _action;

        public UIBlock(Action<NativeViewHierarchyManager> action)
        {
            _action = action;
        }

        public void Execute(NativeViewHierarchyManager nativeViewHierarchyManager)
        {
            _action(nativeViewHierarchyManager);
        }
    }
}
