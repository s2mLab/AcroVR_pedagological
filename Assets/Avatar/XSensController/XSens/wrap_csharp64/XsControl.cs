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

public class XsControl : global::System.IDisposable {
  private global::System.Runtime.InteropServices.HandleRef swigCPtr;
  protected bool swigCMemOwn;

  internal XsControl(global::System.IntPtr cPtr, bool cMemoryOwn) {
    swigCMemOwn = cMemoryOwn;
    swigCPtr = new global::System.Runtime.InteropServices.HandleRef(this, cPtr);
  }

  internal static global::System.Runtime.InteropServices.HandleRef getCPtr(XsControl obj) {
    return (obj == null) ? new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero) : obj.swigCPtr;
  }

  ~XsControl() {
    Dispose();
  }

  public virtual void Dispose() {
    lock(this) {
      if (swigCPtr.Handle != global::System.IntPtr.Zero) {
        if (swigCMemOwn) {
          swigCMemOwn = false;
          xsensdeviceapiPINVOKE.delete_XsControl(swigCPtr);
        }
        swigCPtr = new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero);
      }
      global::System.GC.SuppressFinalize(this);
    }
  }

  public static SWIGTYPE_p_XsControl construct() {
    global::System.IntPtr cPtr = xsensdeviceapiPINVOKE.XsControl_construct();
    SWIGTYPE_p_XsControl ret = (cPtr == global::System.IntPtr.Zero) ? null : new SWIGTYPE_p_XsControl(cPtr, false);
    return ret;
  }

  public void destruct() {
    xsensdeviceapiPINVOKE.XsControl_destruct(swigCPtr);
  }

  public void flushInputBuffers() {
    xsensdeviceapiPINVOKE.XsControl_flushInputBuffers(swigCPtr);
  }

  public static SWIGTYPE_p_XsVersion version() {
    SWIGTYPE_p_XsVersion ret = new SWIGTYPE_p_XsVersion(xsensdeviceapiPINVOKE.XsControl_version(), true);
    return ret;
  }

  public static XsString libraryPath() {
    XsString ret = new XsString(xsensdeviceapiPINVOKE.XsControl_libraryPath(), true);
    return ret;
  }

  public static XsString resultText(XsResultValue resultCode) {
    XsString ret = new XsString(xsensdeviceapiPINVOKE.XsControl_resultText((int)resultCode), true);
    return ret;
  }

  public static void setLogPath(XsString path) {
    xsensdeviceapiPINVOKE.XsControl_setLogPath(XsString.getCPtr(path));
    if (xsensdeviceapiPINVOKE.SWIGPendingException.Pending) throw xsensdeviceapiPINVOKE.SWIGPendingException.Retrieve();
  }

  public void close() {
    xsensdeviceapiPINVOKE.XsControl_close(swigCPtr);
  }

  public bool openPort(XsString portname, XsBaudRate baudrate, uint timeout, bool detectRs485) {
    bool ret = xsensdeviceapiPINVOKE.XsControl_openPort__SWIG_0(swigCPtr, XsString.getCPtr(portname), (int)baudrate, timeout, detectRs485);
    if (xsensdeviceapiPINVOKE.SWIGPendingException.Pending) throw xsensdeviceapiPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public bool openPort(XsString portname, XsBaudRate baudrate, uint timeout) {
    bool ret = xsensdeviceapiPINVOKE.XsControl_openPort__SWIG_1(swigCPtr, XsString.getCPtr(portname), (int)baudrate, timeout);
    if (xsensdeviceapiPINVOKE.SWIGPendingException.Pending) throw xsensdeviceapiPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public bool openPort(XsString portname, XsBaudRate baudrate) {
    bool ret = xsensdeviceapiPINVOKE.XsControl_openPort__SWIG_2(swigCPtr, XsString.getCPtr(portname), (int)baudrate);
    if (xsensdeviceapiPINVOKE.SWIGPendingException.Pending) throw xsensdeviceapiPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public bool openPort(XsPortInfo portinfo, uint timeout, bool detectRs485) {
    bool ret = xsensdeviceapiPINVOKE.XsControl_openPort__SWIG_3(swigCPtr, XsPortInfo.getCPtr(portinfo), timeout, detectRs485);
    if (xsensdeviceapiPINVOKE.SWIGPendingException.Pending) throw xsensdeviceapiPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public bool openPort(XsPortInfo portinfo, uint timeout) {
    bool ret = xsensdeviceapiPINVOKE.XsControl_openPort__SWIG_4(swigCPtr, XsPortInfo.getCPtr(portinfo), timeout);
    if (xsensdeviceapiPINVOKE.SWIGPendingException.Pending) throw xsensdeviceapiPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public bool openPort(XsPortInfo portinfo) {
    bool ret = xsensdeviceapiPINVOKE.XsControl_openPort__SWIG_5(swigCPtr, XsPortInfo.getCPtr(portinfo));
    if (xsensdeviceapiPINVOKE.SWIGPendingException.Pending) throw xsensdeviceapiPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public bool openPortWithCredentials(XsPortInfo portinfo, XsString id, XsString key, uint timeout) {
    bool ret = xsensdeviceapiPINVOKE.XsControl_openPortWithCredentials__SWIG_0(swigCPtr, XsPortInfo.getCPtr(portinfo), XsString.getCPtr(id), XsString.getCPtr(key), timeout);
    if (xsensdeviceapiPINVOKE.SWIGPendingException.Pending) throw xsensdeviceapiPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public bool openPortWithCredentials(XsPortInfo portinfo, XsString id, XsString key) {
    bool ret = xsensdeviceapiPINVOKE.XsControl_openPortWithCredentials__SWIG_1(swigCPtr, XsPortInfo.getCPtr(portinfo), XsString.getCPtr(id), XsString.getCPtr(key));
    if (xsensdeviceapiPINVOKE.SWIGPendingException.Pending) throw xsensdeviceapiPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public bool openImarPort_internal(XsString portname, XsBaudRate baudrate, int imarType, uint timeout) {
    bool ret = xsensdeviceapiPINVOKE.XsControl_openImarPort_internal__SWIG_0(swigCPtr, XsString.getCPtr(portname), (int)baudrate, imarType, timeout);
    if (xsensdeviceapiPINVOKE.SWIGPendingException.Pending) throw xsensdeviceapiPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public bool openImarPort_internal(XsString portname, XsBaudRate baudrate, int imarType) {
    bool ret = xsensdeviceapiPINVOKE.XsControl_openImarPort_internal__SWIG_1(swigCPtr, XsString.getCPtr(portname), (int)baudrate, imarType);
    if (xsensdeviceapiPINVOKE.SWIGPendingException.Pending) throw xsensdeviceapiPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public bool openPort(int portNr, XsBaudRate baudrate, uint timeout, bool detectRs485) {
    bool ret = xsensdeviceapiPINVOKE.XsControl_openPort__SWIG_6(swigCPtr, portNr, (int)baudrate, timeout, detectRs485);
    return ret;
  }

  public bool openPort(int portNr, XsBaudRate baudrate, uint timeout) {
    bool ret = xsensdeviceapiPINVOKE.XsControl_openPort__SWIG_7(swigCPtr, portNr, (int)baudrate, timeout);
    return ret;
  }

  public bool openPort(int portNr, XsBaudRate baudrate) {
    bool ret = xsensdeviceapiPINVOKE.XsControl_openPort__SWIG_8(swigCPtr, portNr, (int)baudrate);
    return ret;
  }

  public bool openImarPort_internal(int portNr, XsBaudRate baudrate, int imarType, uint timeout) {
    bool ret = xsensdeviceapiPINVOKE.XsControl_openImarPort_internal__SWIG_2(swigCPtr, portNr, (int)baudrate, imarType, timeout);
    return ret;
  }

  public bool openImarPort_internal(int portNr, XsBaudRate baudrate, int imarType) {
    bool ret = xsensdeviceapiPINVOKE.XsControl_openImarPort_internal__SWIG_3(swigCPtr, portNr, (int)baudrate, imarType);
    return ret;
  }

  public void closePort(XsString portname) {
    xsensdeviceapiPINVOKE.XsControl_closePort__SWIG_0(swigCPtr, XsString.getCPtr(portname));
    if (xsensdeviceapiPINVOKE.SWIGPendingException.Pending) throw xsensdeviceapiPINVOKE.SWIGPendingException.Retrieve();
  }

  public void closePort(XsDeviceId deviceId) {
    xsensdeviceapiPINVOKE.XsControl_closePort__SWIG_1(swigCPtr, XsDeviceId.getCPtr(deviceId));
    if (xsensdeviceapiPINVOKE.SWIGPendingException.Pending) throw xsensdeviceapiPINVOKE.SWIGPendingException.Retrieve();
  }

  public void closePort(XsPortInfo portinfo) {
    xsensdeviceapiPINVOKE.XsControl_closePort__SWIG_2(swigCPtr, XsPortInfo.getCPtr(portinfo));
    if (xsensdeviceapiPINVOKE.SWIGPendingException.Pending) throw xsensdeviceapiPINVOKE.SWIGPendingException.Retrieve();
  }

  public void closePort(int portNr) {
    xsensdeviceapiPINVOKE.XsControl_closePort__SWIG_3(swigCPtr, portNr);
  }

  public void closePort(SWIGTYPE_p_XsDevice device) {
    xsensdeviceapiPINVOKE.XsControl_closePort__SWIG_4(swigCPtr, SWIGTYPE_p_XsDevice.getCPtr(device));
  }

  public bool openLogFile(XsString filename) {
    bool ret = xsensdeviceapiPINVOKE.XsControl_openLogFile(swigCPtr, XsString.getCPtr(filename));
    if (xsensdeviceapiPINVOKE.SWIGPendingException.Pending) throw xsensdeviceapiPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public XsResultValue lastResult() {
    XsResultValue ret = (XsResultValue)xsensdeviceapiPINVOKE.XsControl_lastResult(swigCPtr);
    return ret;
  }

  public XsString lastResultText() {
    XsString ret = new XsString(xsensdeviceapiPINVOKE.XsControl_lastResultText(swigCPtr), true);
    return ret;
  }

  public int deviceCount() {
    int ret = xsensdeviceapiPINVOKE.XsControl_deviceCount(swigCPtr);
    return ret;
  }

  public int mainDeviceCount() {
    int ret = xsensdeviceapiPINVOKE.XsControl_mainDeviceCount(swigCPtr);
    return ret;
  }

  public XsDeviceIdArray mainDeviceIds() {
    XsDeviceIdArray ret = new XsDeviceIdArray(xsensdeviceapiPINVOKE.XsControl_mainDeviceIds(swigCPtr), true);
    return ret;
  }

  public int mtCount() {
    int ret = xsensdeviceapiPINVOKE.XsControl_mtCount(swigCPtr);
    return ret;
  }

  public XsDeviceIdArray mtDeviceIds() {
    XsDeviceIdArray ret = new XsDeviceIdArray(xsensdeviceapiPINVOKE.XsControl_mtDeviceIds(swigCPtr), true);
    return ret;
  }

  public XsDeviceIdArray deviceIds() {
    XsDeviceIdArray ret = new XsDeviceIdArray(xsensdeviceapiPINVOKE.XsControl_deviceIds(swigCPtr), true);
    return ret;
  }

  public SWIGTYPE_p_XsDevice getDeviceFromLocationId(ushort locationId) {
    global::System.IntPtr cPtr = xsensdeviceapiPINVOKE.XsControl_getDeviceFromLocationId(swigCPtr, locationId);
    SWIGTYPE_p_XsDevice ret = (cPtr == global::System.IntPtr.Zero) ? null : new SWIGTYPE_p_XsDevice(cPtr, false);
    return ret;
  }

  public XsDeviceId getDeviceIdFromLocationId(ushort locationId) {
    XsDeviceId ret = new XsDeviceId(xsensdeviceapiPINVOKE.XsControl_getDeviceIdFromLocationId(swigCPtr, locationId), true);
    return ret;
  }

  public XsDeviceId dockDeviceId(XsDeviceId deviceId) {
    XsDeviceId ret = new XsDeviceId(xsensdeviceapiPINVOKE.XsControl_dockDeviceId(swigCPtr, XsDeviceId.getCPtr(deviceId)), true);
    if (xsensdeviceapiPINVOKE.SWIGPendingException.Pending) throw xsensdeviceapiPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public bool isDeviceWireless(XsDeviceId deviceId) {
    bool ret = xsensdeviceapiPINVOKE.XsControl_isDeviceWireless(swigCPtr, XsDeviceId.getCPtr(deviceId));
    if (xsensdeviceapiPINVOKE.SWIGPendingException.Pending) throw xsensdeviceapiPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public bool isDeviceDocked(XsDeviceId deviceId) {
    bool ret = xsensdeviceapiPINVOKE.XsControl_isDeviceDocked(swigCPtr, XsDeviceId.getCPtr(deviceId));
    if (xsensdeviceapiPINVOKE.SWIGPendingException.Pending) throw xsensdeviceapiPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public bool loadFilterProfiles(XsString filename) {
    bool ret = xsensdeviceapiPINVOKE.XsControl_loadFilterProfiles(swigCPtr, XsString.getCPtr(filename));
    if (xsensdeviceapiPINVOKE.SWIGPendingException.Pending) throw xsensdeviceapiPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public void setOptions(XsOption enable, XsOption disable) {
    xsensdeviceapiPINVOKE.XsControl_setOptions(swigCPtr, (int)enable, (int)disable);
  }

  public void setOptionsForce(XsOption enabled) {
    xsensdeviceapiPINVOKE.XsControl_setOptionsForce(swigCPtr, (int)enabled);
  }

  public bool setLatLonAlt(XsVector lla) {
    bool ret = xsensdeviceapiPINVOKE.XsControl_setLatLonAlt(swigCPtr, XsVector.getCPtr(lla));
    if (xsensdeviceapiPINVOKE.SWIGPendingException.Pending) throw xsensdeviceapiPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public bool setInitialPositionLLA(XsVector lla) {
    bool ret = xsensdeviceapiPINVOKE.XsControl_setInitialPositionLLA(swigCPtr, XsVector.getCPtr(lla));
    if (xsensdeviceapiPINVOKE.SWIGPendingException.Pending) throw xsensdeviceapiPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public SWIGTYPE_p_XsDevice device(XsDeviceId deviceId) {
    global::System.IntPtr cPtr = xsensdeviceapiPINVOKE.XsControl_device(swigCPtr, XsDeviceId.getCPtr(deviceId));
    SWIGTYPE_p_XsDevice ret = (cPtr == global::System.IntPtr.Zero) ? null : new SWIGTYPE_p_XsDevice(cPtr, false);
    if (xsensdeviceapiPINVOKE.SWIGPendingException.Pending) throw xsensdeviceapiPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public XsDevicePtrArray mainDevices() {
    XsDevicePtrArray ret = new XsDevicePtrArray(xsensdeviceapiPINVOKE.XsControl_mainDevices(swigCPtr), true);
    return ret;
  }

  public SWIGTYPE_p_XsDevice broadcast() {
    global::System.IntPtr cPtr = xsensdeviceapiPINVOKE.XsControl_broadcast(swigCPtr);
    SWIGTYPE_p_XsDevice ret = (cPtr == global::System.IntPtr.Zero) ? null : new SWIGTYPE_p_XsDevice(cPtr, false);
    return ret;
  }

  public XsResultValue testSynchronization() {
    XsResultValue ret = (XsResultValue)xsensdeviceapiPINVOKE.XsControl_testSynchronization(swigCPtr);
    return ret;
  }

  public void clearCallbackHandlers(bool chain) {
    xsensdeviceapiPINVOKE.XsControl_clearCallbackHandlers__SWIG_0(swigCPtr, chain);
  }

  public void clearCallbackHandlers() {
    xsensdeviceapiPINVOKE.XsControl_clearCallbackHandlers__SWIG_1(swigCPtr);
  }

  public void addCallbackHandler(XsCallbackPlainC cb, bool chain) {
    xsensdeviceapiPINVOKE.XsControl_addCallbackHandler__SWIG_0(swigCPtr, XsCallbackPlainC.getCPtr(cb), chain);
  }

  public void addCallbackHandler(XsCallbackPlainC cb) {
    xsensdeviceapiPINVOKE.XsControl_addCallbackHandler__SWIG_1(swigCPtr, XsCallbackPlainC.getCPtr(cb));
  }

  public void removeCallbackHandler(XsCallbackPlainC cb, bool chain) {
    xsensdeviceapiPINVOKE.XsControl_removeCallbackHandler__SWIG_0(swigCPtr, XsCallbackPlainC.getCPtr(cb), chain);
  }

  public void removeCallbackHandler(XsCallbackPlainC cb) {
    xsensdeviceapiPINVOKE.XsControl_removeCallbackHandler__SWIG_1(swigCPtr, XsCallbackPlainC.getCPtr(cb));
  }

  public XsControl() : this(xsensdeviceapiPINVOKE.new_XsControl(), true) {
  }

}

}
