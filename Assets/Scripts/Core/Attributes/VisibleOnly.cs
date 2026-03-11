using UnityEngine;

namespace Cardevil.Core.Attributes
{
    /// <summary>
    /// Inspector에서 수정을 제한하는 Attribute.
    /// </summary>
    /// <seealso cref="VisibleOnlyDrawer"/>
    public class VisibleOnly : PropertyAttribute
    {
        public EditableIn EditableIn;
        public bool IgnoreParentEditable = false;
        /// <summary>
        /// Inspector에서 수정을 제한하는 Attribute.
        /// </summary>
        /// <param name="editableIn">어느 모드에서 수정 가능한지. None인 경우 수정 불가.</param>
        /// <param name="ignoreParentEditable">상위 오브젝트에서 이미 수정 불가인경우, 무시할 것인지.</param>
        public VisibleOnly(EditableIn editableIn = EditableIn.None, bool ignoreParentEditable = false)
        {
            this.EditableIn = editableIn;
            this.IgnoreParentEditable = ignoreParentEditable;
        }
    }
    public enum EditableIn
    {
        None,
        EditMode,
        PlayMode
    }
}
