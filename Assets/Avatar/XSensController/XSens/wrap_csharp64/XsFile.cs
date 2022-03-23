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

public class XsFile : global::System.IDisposable {
  private global::System.Runtime.InteropServices.HandleRef swigCPtr;
  protected bool swigCMemOwn;

  internal XsFile(global::System.IntPtr cPtr, bool cMemoryOwn) {
    swigCMemOwn = cMemoryOwn;
    swigCPtr = new global::System.Runtime.InteropServices.HandleRef(this, cPtr);
  }

  internal static global::System.Runtime.InteropServices.HandleRef getCPtr(XsFile obj) {
    return (obj == null) ? new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero) : obj.swigCPtr;
  }

  ~XsFile() {
    Dispose();
  }

  public virtual void Dispose() {
    lock(this) {
      if (swigCPtr.Handle != global::System.IntPtr.Zero) {
        if (swigCMemOwn) {
          swigCMemOwn = false;
          xsensdeviceapiPINVOKE.delete_XsFile(swigCPtr);
        }
        swigCPtr = new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero);
      }
      global::System.GC.SuppressFinalize(this);
    }
  }

  public XsFile() : this(xsensdeviceapiPINVOKE.new_XsFile(), true) {
  }

  public XsResultValue create(XsString filename, bool writeOnly) {
    XsResultValue ret = (XsResultValue)xsensdeviceapiPINVOKE.XsFile_create(swigCPtr, XsString.getCPtr(filename), writeOnly);
    if (xsensdeviceapiPINVOKE.SWIGPendingException.Pending) throw xsensdeviceapiPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public XsResultValue createText(XsString filename, bool writeOnly) {
    XsResultValue ret = (XsResultValue)xsensdeviceapiPINVOKE.XsFile_createText(swigCPtr, XsString.getCPtr(filename), writeOnly);
    if (xsensdeviceapiPINVOKE.SWIGPendingException.Pending) throw xsensdeviceapiPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public XsResultValue open(XsString fileName, bool readOnly) {
    XsResultValue ret = (XsResultValue)xsensdeviceapiPINVOKE.XsFile_open(swigCPtr, XsString.getCPtr(fileName), readOnly);
    if (xsensdeviceapiPINVOKE.SWIGPendingException.Pending) throw xsensdeviceapiPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public XsResultValue openText(XsString fileName, bool readOnly) {
    XsResultValue ret = (XsResultValue)xsensdeviceapiPINVOKE.XsFile_openText(swigCPtr, XsString.getCPtr(fileName), readOnly);
    if (xsensdeviceapiPINVOKE.SWIGPendingException.Pending) throw xsensdeviceapiPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public XsResultValue reopen(XsString fileName, XsString mode) {
    XsResultValue ret = (XsResultValue)xsensdeviceapiPINVOKE.XsFile_reopen(swigCPtr, XsString.getCPtr(fileName), XsString.getCPtr(mode));
    if (xsensdeviceapiPINVOKE.SWIGPendingException.Pending) throw xsensdeviceapiPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public bool isOpen() {
    bool ret = xsensdeviceapiPINVOKE.XsFile_isOpen(swigCPtr);
    return ret;
  }

  public static bool exists(XsString fileName) {
    bool ret = xsensdeviceapiPINVOKE.XsFile_exists(XsString.getCPtr(fileName));
    if (xsensdeviceapiPINVOKE.SWIGPendingException.Pending) throw xsensdeviceapiPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public XsResultValue close() {
    XsResultValue ret = (XsResultValue)xsensdeviceapiPINVOKE.XsFile_close(swigCPtr);
    return ret;
  }

  public XsResultValue flush() {
    XsResultValue ret = (XsResultValue)xsensdeviceapiPINVOKE.XsFile_flush(swigCPtr);
    return ret;
  }

  public XsResultValue truncate(uint fileSize) {
    XsResultValue ret = (XsResultValue)xsensdeviceapiPINVOKE.XsFile_truncate(swigCPtr, fileSize);
    return ret;
  }

  public XsResultValue resize(uint fileSize) {
    XsResultValue ret = (XsResultValue)xsensdeviceapiPINVOKE.XsFile_resize(swigCPtr, fileSize);
    return ret;
  }

  public static XsResultValue erase(XsString filename) {
    XsResultValue ret = (XsResultValue)xsensdeviceapiPINVOKE.XsFile_erase(XsString.getCPtr(filename));
    if (xsensdeviceapiPINVOKE.SWIGPendingException.Pending) throw xsensdeviceapiPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public uint read(SWIGTYPE_p_void destination, uint size, uint count) {
    uint ret = xsensdeviceapiPINVOKE.XsFile_read(swigCPtr, SWIGTYPE_p_void.getCPtr(destination), size, count);
    return ret;
  }

  public uint write(SWIGTYPE_p_void source, uint size, uint count) {
    uint ret = xsensdeviceapiPINVOKE.XsFile_write(swigCPtr, SWIGTYPE_p_void.getCPtr(source), size, count);
    return ret;
  }

  public int getc() {
    int ret = xsensdeviceapiPINVOKE.XsFile_getc(swigCPtr);
    return ret;
  }

  public XsResultValue putc(int character) {
    XsResultValue ret = (XsResultValue)xsensdeviceapiPINVOKE.XsFile_putc(swigCPtr, character);
    return ret;
  }

  public string gets(string destination, int maxCount) {
    string ret = xsensdeviceapiPINVOKE.XsFile_gets(swigCPtr, destination, maxCount);
    return ret;
  }

  public XsResultValue puts(string source) {
    XsResultValue ret = (XsResultValue)xsensdeviceapiPINVOKE.XsFile_puts(swigCPtr, source);
    return ret;
  }

  public XsResultValue seek(SWIGTYPE_p___int64 offset) {
    XsResultValue ret = (XsResultValue)xsensdeviceapiPINVOKE.XsFile_seek(swigCPtr, SWIGTYPE_p___int64.getCPtr(offset));
    if (xsensdeviceapiPINVOKE.SWIGPendingException.Pending) throw xsensdeviceapiPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public XsResultValue seek_r(SWIGTYPE_p___int64 offset) {
    XsResultValue ret = (XsResultValue)xsensdeviceapiPINVOKE.XsFile_seek_r(swigCPtr, SWIGTYPE_p___int64.getCPtr(offset));
    if (xsensdeviceapiPINVOKE.SWIGPendingException.Pending) throw xsensdeviceapiPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public SWIGTYPE_p___int64 tell() {
    SWIGTYPE_p___int64 ret = new SWIGTYPE_p___int64(xsensdeviceapiPINVOKE.XsFile_tell(swigCPtr), true);
    return ret;
  }

  public bool eof() {
    bool ret = xsensdeviceapiPINVOKE.XsFile_eof(swigCPtr);
    return ret;
  }

  public XsResultValue error() {
    XsResultValue ret = (XsResultValue)xsensdeviceapiPINVOKE.XsFile_error(swigCPtr);
    return ret;
  }

  public static XsResultValue fullPath(XsString filename, XsString fullPath) {
    XsResultValue ret = (XsResultValue)xsensdeviceapiPINVOKE.XsFile_fullPath(XsString.getCPtr(filename), XsString.getCPtr(fullPath));
    if (xsensdeviceapiPINVOKE.SWIGPendingException.Pending) throw xsensdeviceapiPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public XsResultValue getline(XsString line) {
    XsResultValue ret = (XsResultValue)xsensdeviceapiPINVOKE.XsFile_getline__SWIG_0(swigCPtr, XsString.getCPtr(line));
    if (xsensdeviceapiPINVOKE.SWIGPendingException.Pending) throw xsensdeviceapiPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public XsResultValue getline(SWIGTYPE_p_std__string line) {
    XsResultValue ret = (XsResultValue)xsensdeviceapiPINVOKE.XsFile_getline__SWIG_1(swigCPtr, SWIGTYPE_p_std__string.getCPtr(line));
    if (xsensdeviceapiPINVOKE.SWIGPendingException.Pending) throw xsensdeviceapiPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public SWIGTYPE_p_FILE handle() {
    global::System.IntPtr cPtr = xsensdeviceapiPINVOKE.XsFile_handle(swigCPtr);
    SWIGTYPE_p_FILE ret = (cPtr == global::System.IntPtr.Zero) ? null : new SWIGTYPE_p_FILE(cPtr, false);
    return ret;
  }

}

}