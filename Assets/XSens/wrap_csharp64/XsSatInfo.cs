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

public class XsSatInfo : global::System.IDisposable {
  private global::System.Runtime.InteropServices.HandleRef swigCPtr;
  protected bool swigCMemOwn;

  internal XsSatInfo(global::System.IntPtr cPtr, bool cMemoryOwn) {
    swigCMemOwn = cMemoryOwn;
    swigCPtr = new global::System.Runtime.InteropServices.HandleRef(this, cPtr);
  }

  internal static global::System.Runtime.InteropServices.HandleRef getCPtr(XsSatInfo obj) {
    return (obj == null) ? new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero) : obj.swigCPtr;
  }

  ~XsSatInfo() {
    Dispose();
  }

  public virtual void Dispose() {
    lock(this) {
      if (swigCPtr.Handle != global::System.IntPtr.Zero) {
        if (swigCMemOwn) {
          swigCMemOwn = false;
          xsensdeviceapiPINVOKE.delete_XsSatInfo(swigCPtr);
        }
        swigCPtr = new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero);
      }
      global::System.GC.SuppressFinalize(this);
    }
  }

  public byte m_gnssId {
    set {
      xsensdeviceapiPINVOKE.XsSatInfo_m_gnssId_set(swigCPtr, value);
    } 
    get {
      byte ret = xsensdeviceapiPINVOKE.XsSatInfo_m_gnssId_get(swigCPtr);
      return ret;
    } 
  }

  public byte m_svId {
    set {
      xsensdeviceapiPINVOKE.XsSatInfo_m_svId_set(swigCPtr, value);
    } 
    get {
      byte ret = xsensdeviceapiPINVOKE.XsSatInfo_m_svId_get(swigCPtr);
      return ret;
    } 
  }

  public byte m_cno {
    set {
      xsensdeviceapiPINVOKE.XsSatInfo_m_cno_set(swigCPtr, value);
    } 
    get {
      byte ret = xsensdeviceapiPINVOKE.XsSatInfo_m_cno_get(swigCPtr);
      return ret;
    } 
  }

  public byte m_flags {
    set {
      xsensdeviceapiPINVOKE.XsSatInfo_m_flags_set(swigCPtr, value);
    } 
    get {
      byte ret = xsensdeviceapiPINVOKE.XsSatInfo_m_flags_get(swigCPtr);
      return ret;
    } 
  }

  public XsSatInfo() : this(xsensdeviceapiPINVOKE.new_XsSatInfo(), true) {
  }

}

}
