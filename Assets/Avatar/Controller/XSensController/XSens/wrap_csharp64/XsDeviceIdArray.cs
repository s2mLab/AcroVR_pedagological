//------------------------------------------------------------------------------
// <auto-generated />
//
// This file was automatically generated by SWIG (http://www.swig.org).
// Version 3.0.5
//
// Do not make changes to this file unless you know what you are doing--modify
// the SWIG interface file instead.
//------------------------------------------------------------------------------

namespace XDA {

public class XsDeviceIdArray : XsDeviceIdArrayImpl {
  private global::System.Runtime.InteropServices.HandleRef swigCPtr;

  internal XsDeviceIdArray(global::System.IntPtr cPtr, bool cMemoryOwn) : base(xsensdeviceapiPINVOKE.XsDeviceIdArray_SWIGUpcast(cPtr), cMemoryOwn) {
    swigCPtr = new global::System.Runtime.InteropServices.HandleRef(this, cPtr);
  }

  internal static global::System.Runtime.InteropServices.HandleRef getCPtr(XsDeviceIdArray obj) {
    return (obj == null) ? new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero) : obj.swigCPtr;
  }

  ~XsDeviceIdArray() {
    Dispose();
  }

  public override void Dispose() {
    lock(this) {
      if (swigCPtr.Handle != global::System.IntPtr.Zero) {
        if (swigCMemOwn) {
          swigCMemOwn = false;
          xsensdeviceapiPINVOKE.delete_XsDeviceIdArray(swigCPtr);
        }
        swigCPtr = new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero);
      }
      global::System.GC.SuppressFinalize(this);
      base.Dispose();
    }
  }

  public XsDeviceIdArray(uint sz, XsDeviceId src) : this(xsensdeviceapiPINVOKE.new_XsDeviceIdArray__SWIG_0(sz, XsDeviceId.getCPtr(src)), true) {
  }

  public XsDeviceIdArray(uint sz) : this(xsensdeviceapiPINVOKE.new_XsDeviceIdArray__SWIG_1(sz), true) {
  }

  public XsDeviceIdArray() : this(xsensdeviceapiPINVOKE.new_XsDeviceIdArray__SWIG_2(), true) {
  }

  public XsDeviceIdArray(XsDeviceIdArray other) : this(xsensdeviceapiPINVOKE.new_XsDeviceIdArray__SWIG_3(XsDeviceIdArray.getCPtr(other)), true) {
    if (xsensdeviceapiPINVOKE.SWIGPendingException.Pending) throw xsensdeviceapiPINVOKE.SWIGPendingException.Retrieve();
  }

  public XsDeviceIdArray(XsDeviceId arg0, uint sz, XsDataFlags flags) : this(xsensdeviceapiPINVOKE.new_XsDeviceIdArray__SWIG_4(XsDeviceId.getCPtr(arg0), sz, (int)flags), true) {
  }

  public XsDeviceIdArray(XsDeviceId arg0, uint sz) : this(xsensdeviceapiPINVOKE.new_XsDeviceIdArray__SWIG_5(XsDeviceId.getCPtr(arg0), sz), true) {
  }

}

}