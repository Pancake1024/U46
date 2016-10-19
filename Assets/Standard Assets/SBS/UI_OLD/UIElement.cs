using UnityEngine;
using SBS.Math;

namespace SBS.UI
{
    public interface UIElement
    {
        OverlaysBatch Batch { get; }
        SBSMatrix4x4 Transform { get; set; }
        Rect ScreenRect { get; set; }
        Color Color { get; set; }
        //bool Clipped { get; }
        bool Visibility { get; set; }
        //int ScissorRectIndex { get; set; }
        //string ScissorRectName { get; set; }
        int Depth { get; set; }

        void SetScissorRect(string name, Rect scissorRect);
        void SetScissorRectWithTransform(string name, Rect scissorRect, SBSMatrix4x4 newTransform);
        void RemoveScissorRect();
        string GetScissorRectName();

        void Destroy();
    }
}
