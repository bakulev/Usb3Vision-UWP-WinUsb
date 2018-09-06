# Task

Read image data from the usb camera via USB3Vision protocol from UWP app.
Implement control of various camera features (like zoom and gain) defined in vendor-supplied property file.

# Approach

We install generic winusb.sys driver for the camera.
Then the cam can be accessed via .Net UWP Windows.Devices.Usb namespace 

# USB3Vision

USB3Vision is a protocol for USB cameras that resides on top of USB protocol. 
According to [USB3Vision spec](https://github.com/zhoutotong/translateUSB3Vision_V1.0/blob/master/USB3_Vision_v1.0_Jan%2018-2013.pdf)
, every USB3Vision device supports 3 logical interfaces: 
- Control (Interface ID = 0) - to control device
- Stream (Interface ID = 1) - to get data
- Event (Interface ID = 2) - to get device specific events

Data stream is separated into a frames. Each frame starts with leader block (the size of block is set by the device), then goes actual image data block (which can be split to several actual transfers).
The frame ends with trailer block the size of block is set by the device).
(See section 5.2 of the spec).

# Details

## Windows.Devices.Usb

MS official sample available at:
https://github.com/Microsoft/Windows-universal-samples/tree/master/Samples/CustomUsbDeviceAccess

## How-to: Install WinUsb for Basler Cam

Install basler-winusb driver by:
1. Right-clicking on the device and choose "Update Driver",
2. "Search for driver on the computer" 
3. "Choose Driver from the list...", 
4. Have Disk...
5. Browse to basler-winusb.inf (basler-winusb.cat should be in the same dir)


## How to implement USB3Vision

Basler camera VID: 0x2676 PID: 0xBA02

1. Find and connect to Basler device a) Registering new Watcher, b) specifying VID-PID in app manifest, see https://github.com/Microsoft/Windows-universal-samples/tree/master/Samples/CustomUsbDeviceAccess
2. If device is present, create UsbDevice object for it https://docs.microsoft.com/en-us/uwp/api/windows.devices.usb.usbdevice
3. Enumerate logical interfaces for the device  https://docs.microsoft.com/en-us/uwp/api/windows.devices.usb.usbinterface , use UsbInterfaceDescriptor.InterfaceNumber to identify them as either control, event or stream
4. Get UsbBulkInPipe interface for stream interface and both UsbBulkInPipe and UsbBulkOutPipe for control interface , see https://docs.microsoft.com/en-us/uwp/api/windows.devices.usb.usbbulkinpipe These pipes are main channels of communication with the device
5. Get trailer and leader block sizes of the data stream (see "Read trailer and leader block size" below)
6. Setup streaming configuration (see "Setup stream configuration")
7. Read stream data (see "Reading Stream Data")

## USB3Vision command format

Every USB3Vision command has a following header structure. (__le32 means 32 bit int, __le16 means 16 bit int).
Arbitrary command-dependent payload goes after the header. It's size is specified by `length` element of header.

```
struct command_header {
	__le32 prefix;  // must be 0x43563355
	__le16 flags;   // specify U3V_REQUEST_ACK 0x4000 if response is needed
	__le16 cmd;    // command id 
	__le16 length; // size of payload
	__le16 request_id; // unique id of the request (used to identify the response)
};

```

If U3V_REQUEST_ACK is specified in `flags` then device responds with the following data: ack header followed by payload.
Response format.

```
struct ack_header {
	__le32 prefix; // must be 0x43563355
	__le16 status; // 0 if success, or some device error status
	__le16 cmd; // must be command cmd + 1, or 0x0805 that means operation is in progress and some waiting is required
	__le16 length; // length of payload
	__le16 ack_id; // must match request_id
};

```

## How to read control configuration

Device configuration is stored in device registers and can be accessed via control inteface pipe.

To start reading config data, send Read command (cmd=0x0800) to the device via control Out pipe. Then read the answer via control interface In transfer endpoint.

Read command has the following payload format:

```
struct read_mem_cmd_payload {
	__le64 address; // register address to read data from
	__le16 reserved; // must be 0
	__le16 byte_count; // how much data to read
};
```
Response payload has register data.

## How to write control configuration

Device configuration registers can be changed.

To start writing config data, send Write command (cmd=0x0802) to the device via control Out pipe. Then read the answer via control In pipe.

```
struct write_mem_cmd_payload {
	__le64 address; // register to write data to
	__u8 data[0]; // variable size array of actual data to write
};
```

## Read trailer and leader block size

The process of getting actual data is multistep
1. Get SBRM register. Issue read command to read 64 bit int from 0x001D8 address.
2. Read SIRM register. Read 64 bit int from SBRM+0x00020 address.
3. Get leader size. Read 32 bit int from SIRM+0x10
4. Get trailer size. Read 32 bit int from SIRM+0x14

## Setup Stream Configuration

Device SIRM related registers can be configured so to setup streaming details.

1. Set max transfer size for the cam. Set SIRM+0x1C with 32 bit int to 64K
2. Set number of transfers of data per frame. Set SIRM+0x20 with 32 bit int to application image buffer size divided by 64K
3. Set max leader and trailing buffer. Set SIRM+0x18 and SIRM+0x2C with 32 bit int to 64K

## Read stream data

To read stream data issue a number of 64K transfers via stream interface In endpoint: 1 for leader block, N sequential transfer of image data (see "Setup Stream Configuration"), 1 for trailing block.

Note: To learn actual image properties decode leader data, refer to "5.5.5.1.1 Image Leader" section of USB3Vision spec

## Controlling various camera specific features

There are various camera-related features like zoom, and various modes (like continous, single or multi-shot). 
Mappings between camera-related features and registers are stored in XML file in device-specific location.

There are preparsed XML files available for Basler cam (see props.txt)
Some of Nodes of XML listed has 'Prop:Address' that identify register address where parameters for the feature is located and can be altered. (while "Prop:Length" describes the data size in bytes)
Some of meaningful features like e.g. "Gain" point to some no-name ones like "N37" with "Prop:pValue" and "Prop:pParameter" properties.

## How-to: produce prop file

1. Install Basler Pylon software 
2. Connect Camera
3. Run cmd and cd to directory w/ ParametrizeCamera_GenericParameterAccess.exe
4. type `ParametrizeCamera_GenericParameterAccess.exe > props.txt`
