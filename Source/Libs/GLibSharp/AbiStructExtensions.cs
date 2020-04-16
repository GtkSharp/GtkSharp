using System;
using System.Runtime.InteropServices;

namespace GLib
{

    public static class AbiStructExtension
    {
        public static N BaseOverride<N>(this AbiStruct class_abi, GType gtype, string fieldname) where N : Delegate
        {
            N unmanaged = null;
            unsafe {
                IntPtr raw_ptr = IntPtr.Zero;
                while (raw_ptr == IntPtr.Zero && GType.IsManaged(gtype)) {
                    gtype = gtype.GetThresholdType();
                    var abi_ptr = (IntPtr*) (((long) gtype.GetClassPtr()) + (long) class_abi.GetFieldOffset(fieldname));
                    raw_ptr = *abi_ptr;
                }

                if (raw_ptr != IntPtr.Zero) {
                    unmanaged = Marshal.GetDelegateForFunctionPointer<N>(raw_ptr);
                }
            }

            return unmanaged;
        }
    }
}